using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sand-like fill for a SpriteRenderer:
/// - Clears alpha of all "inside" pixels (alpha > threshold in source)
/// - Fills columns starting from an apex-x, spreading outward; farther columns fill slower
/// - 'process' controls % of inside pixels to become visible (alpha=1)
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSandFill : MonoBehaviour
{
    [Header("Source")]
    public SpriteRenderer targetSR;                 // Nếu để trống sẽ tự lấy trên cùng GO


    [Header("Fill Style")]
    [Tooltip("Gom pixel theo ô (grid) để hạt trông 'to' hơn. Ô càng lớn nhìn càng thô/hạt.")]
    public bool useGridLumping = true;
    [Min(1)] public int gridCellWidth = 2;
    [Min(1)] public int gridCellHeight = 2;

    [Min(0.05f)] public float duration = 1.5f;

    // --- internals ---
    public Sprite origin;
    Texture2D _workTex;
    Sprite _workSprite;          // sprite tạo từ _workTex
    Color[] _src;              // nguồn (vùng rect)
    Color[] _dst;              // đích/đang làm việc
    int _tw, _th;                // width/height vùng rect
    Rect _rect;
    int _x0, _y0;                // pixel offset trong texture gốc
    bool _prepared;

    List<int>[] _colsToRows;     // per-col sorted row indices (inside-only)
    int _insideCountTotal;

    Coroutine _running;

    public void Reset()
    {
        targetSR = GetComponent<SpriteRenderer>();
        DestroyImmediate(_workSprite);
        //targetSR.sprite = origin;
    }

    void Start()
    {
       // Reset();

        //Prepare();
        //if (autoRunOnStart && Application.isPlaying)
        //{
          
        //    FillDemo();
        //}
    }

    void OnDisable()
    {
        if (_running != null) StopCoroutine(_running);
    }

    /// <summary>Chuẩn bị texture làm việc, gom cột, sắp thứ tự row mỗi cột để fill.</summary>
    public void Prepare()
    {
        if (targetSR == null || targetSR.sprite == null) return;

        var sp = targetSR.sprite;
        _rect = sp.rect;                   // rect trong atlas
        _tw = Mathf.RoundToInt(_rect.width);
        _th = Mathf.RoundToInt(_rect.height);
        _x0 = Mathf.RoundToInt(_rect.x);
        _y0 = Mathf.RoundToInt(_rect.y);

        // Lấy pixel vùng rect
        Color[] srcBlock;
        try
        {
            // Nếu texture readable, dùng GetPixels(x,y,w,h)
            srcBlock = sp.texture.GetPixels(_x0, _y0, _tw, _th);
        }
        catch
        {
            // Fallback: copy toàn texture rồi cắt (ít gặp nếu Read/Write bật)
            var full = sp.texture.GetPixels();
            srcBlock = new Color[_tw * _th];
            for (int yy = 0; yy < _th; yy++)
            {
                int srcRowStart = (yy + _y0) * sp.texture.width + _x0;
                int dstRowStart = yy * _tw;
                for (int xx = 0; xx < _tw; xx++)
                    srcBlock[dstRowStart + xx] = full[srcRowStart + xx];
            }
        }
        _src = srcBlock;
        // Lấy danh sách màu duy nhất
        for (int i = 0; i < _src.Length; i++)
        {
            // Nếu pixel có alpha đủ lớn (tránh pixel trong suốt hoàn toàn)
            if (_src[i].a == 1 && !uniqueColors.Contains(_src[i]))
                uniqueColors.Add(_src[i]);
            
            if(uniqueColors.Count > 20)
                break;
        }


        // Tạo work texture (RGBA32), clamp edges
        _workTex = new Texture2D(_tw, _th, TextureFormat.RGBA32, false, false);
        _workTex.filterMode = FilterMode.Point;
        _workTex.wrapMode = TextureWrapMode.Clamp;

        _dst = new Color[_src.Length];
        System.Array.Copy(_src, _dst, _src.Length);

        // Xác định pixel "inside" theo alpha threshold; đồng thời clear alpha bên trong = 0
        _insideCountTotal = 0;
        for (int i = 0; i < _dst.Length; i++)
        {
            if (_dst[i].a > 0f)
            {
                _insideCountTotal++;
                var c = _dst[i];
                c.a = 0;           // clear alpha
                _dst[i] = c;
            }
        }


        // Tạo sprite làm việc và gán
        _workTex.SetPixels(_dst);
        _workTex.Apply(false, false);
        if (_workSprite != null) DestroyImmediate(_workSprite);

        Vector3 originalPos = targetSR.transform.localPosition;

        //_workSprite = Sprite.Create(_workTex, _rect, sp.pivot, sp.pixelsPerUnit);
        _workSprite = Sprite.Create(_workTex, new Rect(0, 0, _tw, _th), new Vector2(
            (sp.pivot.x - _rect.x) / _rect.width,
            (sp.pivot.y - _rect.y) / _rect.height
        ), sp.pixelsPerUnit);


        // Điều chỉnh position của SpriteRenderer để bù lại sự thay đổi pivot
        //Vector2 pivotOffset = (sp.pivot - new Vector2(_rect.x + _tw * 0.5f, _rect.y + _th * 0.5f)) / sp.pixelsPerUnit;
        //targetSR.transform.localPosition += (Vector3)pivotOffset;

        Debug.Log($"SandFill infor {gameObject.name}:  {sp.pivot.x }/{_rect.x}/{_rect.width} and {sp.pivot.y}/{_rect.y}/{_rect.height}");

        _workSprite.name = sp.name + "_SandFill";
        targetSR.sprite = _workSprite;

        Vector2 rectOffset = new Vector2(_rect.x, _rect.y) / sp.pixelsPerUnit;
        targetSR.transform.localPosition = originalPos - (Vector3)rectOffset;



        _prepared = true;
    }
    public List<Color> uniqueColors = new List<Color>();
    /// <summary>
    /// Fill 1 pixel theo (row=y, col=x) bằng đúng màu gốc từ _src.
    /// onlyInside: chỉ fill nếu pixel ban đầu thuộc vùng "inside" theo alphaInsideThreshold.
    /// </summary>
    public void FillPixel(int row, int col, bool applyNow = true)
    {
        if (!_prepared || _workTex == null) return;
        if (row < 0 || row >= _th || col < 0 || col >= _tw) return;

        int idx = row * _tw + col;

        // Nếu chỉ cho phép fill vùng "inside", kiểm tra alpha của _src
        if (_src[idx].a < 0.8f) return;
        if (_dst[idx].a > 0) return;

        Color c = _src[idx];   // lấy đúng màu gốc (RGBA) của pixel
        _dst[idx] = c;
        _workTex.SetPixel(col, row, c);

        if (applyNow)
            _workTex.Apply(false, false);
    }
    public void FillPixelWithColor(int row, int col, Color custom)
    {
        if (!_prepared || _workTex == null) return;
        if (row < 0 || row >= _th || col < 0 || col >= _tw) return;

        int idx = row * _tw + col;

        // Nếu chỉ cho phép fill vùng "inside", kiểm tra alpha của _src
        if (_src[idx].a < 0.8f) return;
        if (_dst[idx].a > 0) return;

        _workTex.SetPixel(col, row, custom);

    }

    //public static AnimationCurve RandomizeCurve(AnimationCurve baseCurve, float valueNoise = 0.05f, float tangentNoise = 0.2f)
    //{
    //    var newCurve = new AnimationCurve();
    //    foreach (var key in baseCurve.keys)
    //    {
    //        Keyframe newKey = key;

    //        // Thêm nhiễu nhẹ vào value (giữ trong [0,1])
    //        newKey.value = Mathf.Clamp01(key.value + Random.Range(-valueNoise, valueNoise));

    //        // Thêm nhiễu nhẹ vào tangents
    //        newKey.inTangent += Random.Range(-tangentNoise, tangentNoise);
    //        newKey.outTangent += Random.Range(-tangentNoise, tangentNoise);

    //        newCurve.AddKey(newKey);
    //    }

    //    // Có thể smooth lại một chút
    //    for (int i = 0; i < newCurve.length; i++)
    //        AnimationUtility.SetKeyBroken(newCurve, i, true);

    //    return newCurve;
    //}
    public void FillDemo()
    {
        StartCoroutine(FillThredByTime(0.02f));

        //DOVirtual.DelayedCall(2f, () =>
        //{
        //    processInit = 1;
        //    StopAllCoroutines();
        //    StartCoroutine(FillThredByTime(0.01f));
        //});
    }
    public void SetHighest(float apeX)
    {
        this.apexX01 = apeX;
    }
    public void StartFill(float process, System.Action fillDone)
    {
        StopAllCoroutines();
        processInit = process;

        // THÊM DÒNG NÀY: Reset lại texture về trạng thái ban đầu
        ResetTextureBeforeFill();

        StartCoroutine(FillThredByTime(0.013f, fillDone));
    }

    // Method mới: Reset texture và thread data
    private void ResetTextureBeforeFill()
    {
        if (!_prepared || _workTex == null) return;

        // 2. Reset lại lastRowFill cho tất cả columns
        if (thredColumnData != null)
        {
            for (int col = 0; col < thredColumnData.Length; col++)
            {
                if (thredColumnData[col] != null)
                {
                    thredColumnData[col].lastRowFill = thredColumnData[col].row_Colors.Count - 1;
                }
            }
        }
    }

    [Header("Apex & Spread")]
    [Tooltip("Vị trí đỉnh theo tỉ lệ bề ngang sprite rect (0..1). 0 = mép trái, 1 = mép phải.")]
    [Range(0f, 1f)] public float apexX01 = 0.5f;
    [Tooltip("Fill max bao nhiêu % sprite")]
    public float processInit { get; set; } = 0;
    [Header("Edge Offset Settings")]
    public AnimationCurve edgeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // curve điều khiển độ cong

    int maxTransSandRow => Random.Range(10, 20);
    ThredColData[] thredColumnData;
    public class ThredColData
    {
        public int col; // cột mà thread này quản lý

        public List<(int row, Color color)> row_Colors = new List<(int row, Color color)>();
        public int HeighestRow => row_Colors[0].row;

        SpriteSandFill mainFill;

        int process;

        public int Process
        {
            get { return process; }
            set { process = value; }
        }
        public bool MaxRow(int maxRow) => HeighestRow > maxRow;

        public void Init(int col, int maxTransSandRow, SpriteSandFill mainFill)
        {
            this.col = col;
            if(row_Colors.Count <= 0)
            {
                for (int i  = 0; i < maxTransSandRow; i++)
                    row_Colors.Add((0 - i, new Color(0,0,0,0) ));
            }

            lastRowFill = row_Colors.Count - 1;
            this.mainFill = mainFill;
        }

        public void RandomColor(List<Color> uniqueColors)
        {
            for (int i = 0; i < row_Colors.Count; i++)
            {
                row_Colors[i] = (row_Colors[i].row, uniqueColors[UnityEngine.Random.Range(0, uniqueColors.Count)]);
            }
        }

        public void GetColor(ThredColData other , List<Color> uniqueColors)
        {
            int n = Mathf.Min(row_Colors.Count, other.row_Colors.Count);
            for (int i = 0; i < n; i++)
            {
                int rand = Random.Range(0, 100);
                Color color = rand % 5 != 0 ? other.row_Colors[i].color : uniqueColors[UnityEngine.Random.Range(0, uniqueColors.Count)];
                row_Colors[i] = (row_Colors[i].row, color);
            }
              
        }

        public void FillColor()
        {
            // vẽ từ đầu list đến áp chót = màu custom, phần tử cuối = màu gốc sprite
            for (int i = 0; i < row_Colors.Count; i++)
            {
                if (i < (row_Colors.Count - 1) && row_Colors[i].color != new Color(0, 0, 0, 0))
                {

                    mainFill.FillPixelWithColor(row_Colors[i].row, col, row_Colors[i].color);
                }
                else
                {
                    // phần tử cuối cùng: lấy màu gốc sprite
                    mainFill.FillPixel(row_Colors[i].row, col, applyNow: false);
                }
            }
        }

        public void UpRow(List<Color> uniqueColors)
        {
            process++;
            if (process < 0) return;

            for (int i = 0; i < row_Colors.Count; i++)
            {
                row_Colors[i] = (row_Colors[i].row + 1, row_Colors[i].color);
            }
        }
        public int lastRowFill;
        public void UpColor(Color color)
        {
            lastRowFill--;
        }
    }
    void InitThredOffsets(int center)
    {

       // thredCol = new (int col, int thredRow)[_tw];

        int maxRadius = Mathf.Max(center, (_tw - 1) - center);
        if (maxRadius <= 0) maxRadius = 1;

        // Đỉnh (cao nhất)
        thredColumnData[center].Process = 0;

        // Hàm tính độ lệch theo khoảng cách - trả về giá trị có thể âm/dương
        int OffsetForDx(int dx)
        {
            float nd = Mathf.Clamp01((float)dx / maxRadius);
            float eval = 1 -edgeCurve.Evaluate(nd);

            // Chuyển từ [0,1] thành [-1,0] để tạo độ cong lõm
            // hoặc [0,1] để tạo độ cong lồi tùy ý muốn
            float offset = (eval - 1.0f) * maxRadius * 0.5f; // Nhân với hệ số để điều chỉnh độ cong

            return Mathf.RoundToInt(offset);
        }

        // Phải - tạo độ cong
        for (int col = center + 1; col < _tw; col++)
        {
            int dx = col - center;
            thredColumnData[col].Process = OffsetForDx(dx);

        }

        // Trái - tạo độ cong (đối xứng)
        for (int col = center - 1; col >= 0; col--)
        {
            int dx = center - col;
            thredColumnData[col].Process = OffsetForDx(dx);
  
        }
    }
    IEnumerator FillThredByTime(float delay , System.Action fillDone = null)
    {
        if (!_prepared || _workTex == null) yield break;

        int center = Mathf.Clamp((int)(_tw * apexX01), 0, _tw - 1);

        // Khởi tạo mảng thread cho từng cột
        if (thredColumnData == null || thredColumnData.Length != _tw)
        {
            thredColumnData = new ThredColData[_tw];

            for (int col = 0; col < _tw; col++)
            {
                if (thredColumnData[col] == null) thredColumnData[col] = new ThredColData();

                thredColumnData[col].Init(col, maxTransSandRow, this);
            }
        }
        InitThredOffsets(center);

        // Mức fill tối đa (giới hạn y)
        int maxRow = Mathf.Clamp((int)(_th * processInit), 0, _th - 1);

        // Helper: cột còn "đang chạy" nếu phần tử cuối vẫn <= maxRow
        bool ColumnActive(ThredColData tcd) => tcd.HeighestRow <= maxRow;

        while (true)
        {
            bool anyPending = false;

            // 1) Seed màu tại cột đỉnh (center) mỗi nhịp
            thredColumnData[center].RandomColor(uniqueColors);

            // 2) Fill cột center
            {
                var tcd = thredColumnData[center];
                if (!tcd.MaxRow(maxRow))
                {
                    tcd.FillColor();
                }
                
                if (ColumnActive(tcd)) anyPending = true;
            }

            // 3) Đoạn trái: từ 0 -> center-1, cột hiện lấy màu từ cột ngay sau (col+1)
            for (int col = 0; col <= center - 1; col++)
            {
               
                var cur = thredColumnData[col];
                if (cur.MaxRow(maxRow) || cur.Process < 0) continue;
                var next = thredColumnData[col + 2]; // "ngay sau"

                cur.GetColor(next, uniqueColors);

                cur.FillColor();
                if (ColumnActive(cur)) anyPending = true;
            }

            // 4) Đoạn phải: từ center+1 -> end, cột hiện lấy màu từ cột ngay trước (col-1)
            for (int col = _tw -1 ; col >= center + 1; col--)
            {
                var cur = thredColumnData[col];
                if (cur.MaxRow(maxRow) || cur.Process < 0) continue;
                var prev = thredColumnData[col - 2]; // "ngay trước"

                cur.GetColor(prev, uniqueColors);

                cur.FillColor();
                if (ColumnActive(cur)) anyPending = true;
            }

            // 5) Apply toàn bộ buffer 1 lần cho mượt
            ApplyFillBuffer();

            // 6) Nếu không còn cột nào hoạt động -> xong
            if (!anyPending)
            {
                fillDone?.Invoke();
                yield break;
            }

            // 7) Sau khi fill xong frame: đẩy tất cả row lên 1 hàng (UpRow)
            for (int col = 0; col < _tw; col++)
                if (thredColumnData[col].MaxRow(maxRow))
                {
                    if(thredColumnData[col].lastRowFill >= 0)
                    {
                        int row = thredColumnData[col].row_Colors[thredColumnData[col].lastRowFill].row;
                        FillPixel(row, col, applyNow: false);
                        thredColumnData[col].UpColor(_src[col]);
                    }    
                }

                else
                {
                    thredColumnData[col].UpRow(uniqueColors);
                }

            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator FillStepByStep(float delay)
    {
        for (int row = 0; row < _th; row++)
        {
            for (int col = 0; col < _tw; col++)
            {
                if (row < _th * 0.5f)
                {
                    FillPixel(row, col, false);
                }
              
            }
            ApplyFillBuffer();
            yield return new WaitForSeconds(delay);
        }
      
    }
    /// <summary>
    /// Nếu trước đó bạn gọi FillPixel(..., applyNow:false) nhiều lần, 
    /// dùng hàm này để đẩy mọi thay đổi lên GPU 1 lần.
    /// </summary>
    public void ApplyFillBuffer()
    {
        if (!_prepared || _workTex == null) return;
        _workTex.Apply(false, false);
    }
   
}
