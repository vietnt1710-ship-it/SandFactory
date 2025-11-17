using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.PlayerSettings;

/// <summary>
/// Optimized sand fill with single coroutine managing all columns via time-based state
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSandFillOptimized : MonoBehaviour
{
    [Header("Source")]
    public SpriteRenderer targetSR;                 // Nếu để trống sẽ tự lấy trên cùng GO

    [Header("Fill Style")]
    [Tooltip("Gom pixel theo ô (grid) để hạt trông 'to' hơn. Ô càng lớn nhìn càng thô/hạt.")]
    public bool useGridLumping = true;
    [Min(1)] public int gridCellWidth = 2;
    [Min(1)] public int gridCellHeight = 2;

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


    Coroutine _running;

    public void Reset()
    {
        targetSR = GetComponent<SpriteRenderer>();
        DestroyImmediate(_workSprite);
        targetSR.sprite = origin;
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
        uniqueColors.Clear();
        // Lấy danh sách màu duy nhất
        for (int i = 0; i < _src.Length; i++)
        {
            // Nếu pixel có alpha đủ lớn (tránh pixel trong suốt hoàn toàn)
            if (_src[i].a == 1 && !uniqueColors.Contains(_src[i]))
                uniqueColors.Add(_src[i]);

            if (uniqueColors.Count > 20)
                break;
        }

        // Tạo work texture (RGBA32), clamp edges
        _workTex = new Texture2D(_tw, _th, TextureFormat.RGBA32, false, false);
        _workTex.filterMode = FilterMode.Point;
        _workTex.wrapMode = TextureWrapMode.Clamp;

        _dst = new Color[_src.Length];
        System.Array.Copy(_src, _dst, _src.Length);

        // Xác định pixel "inside" theo alpha threshold; đồng thời clear alpha bên trong = 0
        for (int i = 0; i < _dst.Length; i++)
        {
            //Debug.Log($"inside{_dst[i].a} / {255f} = {_dst[i].a / 255f}");
            if (_dst[i].a > 0)
            {
                // Debug.Log($"inside {_dst[i].a}");
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

        Debug.Log($"SandFill infor {gameObject.name}:  {sp.pivot.x}/{_rect.x}/{_rect.width} and {sp.pivot.y}/{_rect.y}/{_rect.height}");

        _workSprite.name = sp.name + "_SandFill";
        targetSR.sprite = _workSprite;

        Vector2 rectOffset = new Vector2(_rect.x, _rect.y) / sp.pixelsPerUnit;
        targetSR.transform.localPosition = originalPos - (Vector3)rectOffset;



        _prepared = true;
    }
    List<Color> uniqueColors = new List<Color>();

    public void FillDemo()
    {
        FillBuyTween();
    }
    public void StartFill(float process, System.Action actionDone)
    {
        StopAllCoroutines();
        this.processInit = process;
        FillBuyTween(actionDone);
    }
    public void SetHighest(float apeX)
    {
        this.apexX01 = apeX;
    }

    [Header("Apex & Spread")]
    [Range(0f, 1f)] public float apexX01 = 0.5f;
    public float durationFillARow = 0.75f;
    public float delayTime = 0.01f;

    public float processInit = 0;
    [Header("Edge Offset Settings")]

    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0); // curve điều khiển độ cong

    int maxTransSandRow => Random.Range(7, 12);
    ThredColData[] thredColumnData;
    public class ThredColData
    {
        public int col; // cột mà thread này quản lý

        public List<Color> rims = new List<Color>();
        SpriteSandFillOptimized mainFill;

        float totalTime = 0;
        float elapsed;
        int targetRow;
        int lastFilledRow = -1;
        int startRowFill;

        float startDelay = 0;

        public void SetFill(float startDelay, int targetRow, ThredColData thred = null)
        {
            startRowFill = lastFilledRow + 1;
            this.targetRow = targetRow;
            elapsed = 0;
            totalTime = 0;
            this.startDelay = startDelay;
            if(refThred == null)
            {
                this.refThred = thred;
            }
         
        }
        public ThredColData refThred;

        public int maxRims;

        int listIndex = 0;
        public void Fill()
        {
            totalTime += mainFill.delayTime;
            int rimLimit = 0;
            if (totalTime >= startDelay)
            {
                elapsed = totalTime - startDelay;
                float t = Mathf.Clamp01(elapsed / mainFill.durationFillARow);
                float curveValue = mainFill.curve.Evaluate(t);
                int rowProces = Mathf.FloorToInt(curveValue * (targetRow - startRowFill));
                int currentRow = startRowFill + rowProces;//Mathf.FloorToInt(curveValue * targetRow);

                for (int row = lastFilledRow + 1; row <= currentRow; row++)
                {
                    mainFill.FillPixel(row, col, applyNow: false);
                }
                lastFilledRow = currentRow;
            }
            else
            {
                rimLimit = (int) ((startDelay - totalTime) / mainFill.delayTime);
            }
            
            if (refThred == null)
            {
                if (listIndex >= mainFill.rimsColor.Count)
                {
                    listIndex = 0;
                }
                rims = mainFill.rimsColor[listIndex];
                // Debug.Log("Rims == 0" + rims.Count);
                listIndex++;
            }
            else
            {
                rims = refThred.rims;
                if (rims.Count <= 0)
                {
                    rims = mainFill.rimsColor[Random.Range(0, mainFill.rimsColor.Count)];
                }
            }
            //RandomColor(maxRim);
            int sandRim = lastFilledRow + rims.Count;
            if(rimLimit != 0 && rimLimit < 10)
            {
                sandRim = Mathf.Clamp(sandRim, 0, 10 - rimLimit);
                int i = 0;
                for (int row = lastFilledRow + 1; row <= sandRim; row++)
                {
                    mainFill.FillPixelWithColor(row, col, rims[i]);
                    i++;
                }
            }
            else if (rimLimit == 0)
            {
                sandRim = Mathf.Clamp(sandRim, 0, targetRow - 1);
                int i = 0;
                for (int row = lastFilledRow + 1; row <= sandRim; row++)
                {
                    mainFill.FillPixelWithColor(row, col, rims[i]);
                    i++;
                }
            }
           
        }
        public void Rim()
        {
            int rimLimit = 0;
            if (totalTime < startDelay)
            {
                rimLimit = (int)((startDelay - totalTime) / mainFill.delayTime);
            }
            if (refThred == null)
            {
                if (listIndex >= mainFill.rimsColor.Count)
                {
                    listIndex = 0;
                }
                rims = mainFill.rimsColor[listIndex];
                // Debug.Log("Rims == 0" + rims.Count);
                listIndex++;
            }
            else
            {
                rims = refThred.rims;
                if (rims.Count <= 0)
                {
                    rims = mainFill.rimsColor[Random.Range(0, mainFill.rimsColor.Count)];
                }
            }
            //RandomColor(maxRim);
            int sandRim = lastFilledRow + rims.Count;
            if (rimLimit != 0 && rimLimit < 10)
            {
                sandRim = Mathf.Clamp(sandRim, 0, 10 - rimLimit);
                int i = 0;
                for (int row = lastFilledRow + 1; row <= sandRim; row++)
                {
                    mainFill.FillPixelWithColor(row, col, rims[i]);
                    i++;
                }
            }
            else if (rimLimit == 0)
            {
                sandRim = Mathf.Clamp(sandRim, 0, targetRow - 1);
                int i = 0;
                for (int row = lastFilledRow + 1; row <= sandRim; row++)
                {
                    mainFill.FillPixelWithColor(row, col, rims[i]);
                    i++;
                }
            }
        }
        public void Init(int col, SpriteSandFillOptimized mainFill)
        {
            this.col = col;
            this.mainFill = mainFill;
        }
       
    }

    public List<List<Color>> rimsColor = new List<List<Color>>();
    public int rimCount;
    public int center;
    void InitThredOffsets(int center,int maxRow, out int maxCol)
    {
        rimsColor.Clear();
        this.center = center;
        thredColumnData[center].SetFill(0, maxRow);
        
        //thredColumnData[center].rims = rimsColor[0];
        int counter = 0;
        
        //Phải
        for (int col = center + 1; col < _tw; col++)
        {
            counter++;
            int dx = col - center;
            thredColumnData[col].SetFill(dx * delayTime, maxRow, thredColumnData[col - 1]);
            if (rimsColor.Count <= rimCount)
            {
                int counterColor = maxTransSandRow;
                List<Color> temp = new List<Color>();
                for (int row = 0; row < _th - 1; row++)
                {
                    Color c = PixelColor(row, col);
                    if (c.a > 0.5f)
                    {
                        temp.Add(c);
                        if (temp.Count >= counterColor) break;
                    }
                }
                rimsColor.Add(temp);
            }
          

        }
        maxCol = counter;

        counter = 0;

        // Trái - tạo độ cong (đối xứng)
        for (int col = center - 1; col >= 0; col--)
        {
            counter++;
            int dx = center - col;
            thredColumnData[col].SetFill(dx * delayTime, maxRow, thredColumnData[col + 1]);
            if (rimsColor.Count <= rimCount)
            {
                int counterColor = maxTransSandRow;
                List<Color> temp = new List<Color>();
                for (int row = 0; row < _th - 1; row++)
                {
                    Color c = PixelColor(row, col);
                    if (c.a > 0.5f)
                    {
                        temp.Add(c);
                        if (temp.Count >= counterColor) break;
                    }
                }
                rimsColor.Add(temp);
            }
        }
        thredColumnData[center].rims = rimsColor[0];
        if (counter > maxCol) maxCol = counter;
    }
    public void FillBuyTween( System.Action fillDone = null)
    {
        if (!_prepared || _workTex == null) return;
        int center = Mathf.Clamp((int)(_tw * apexX01), 0, _tw - 1);

        // Khởi tạo mảng thread cho từng cột
        if (thredColumnData == null || thredColumnData.Length != _tw)
        {
            thredColumnData = new ThredColData[_tw];

            for (int col = 0; col < _tw; col++)
            {
                if (thredColumnData[col] == null) thredColumnData[col] = new ThredColData();

                thredColumnData[col].Init(col, this);
            }
        }
        int maxRow = Mathf.Clamp((int)(_th * processInit), 0, _th - 1);

        int maxCol = 0;
        InitThredOffsets(center, maxRow, out maxCol);

        float duration = delayTime * maxCol + 3;

        StartCoroutine(StartTweenFill(duration, fillDone));

    }
    public IEnumerator StartTweenFill(float duration, System.Action fillDone)
    {
        float elapsed = 0f;
        int a = 0;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(delayTime / 2);

            a++;
            if (a == 1)
            {
                // Bước 1: gọi Rim() để chuẩn bị hiển thị viền
                thredColumnData[center].Rim();

                for (int col = 0; col < center; col++)
                    thredColumnData[col].Rim();

                for (int col = _tw - 1; col > center; col--)
                    thredColumnData[col].Rim();
            }

            if (a == 2)
            {
                // Bước 2: gọi Fill() để đổ đầy toàn bộ
                thredColumnData[center].Fill();

                for (int col = 0; col < center; col++)
                    thredColumnData[col].Fill();

                for (int col = _tw - 1; col > center; col--)
                    thredColumnData[col].Fill();

                elapsed += delayTime;
                a = 0;
            }

            ApplyFillBuffer();
        }

        fillDone?.Invoke();
    }


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
    public Color PixelColor(int row, int col)
    {
        Color c = Color.black;
        c.a = 0;
         
        if (!_prepared || _workTex == null) return c;
        if (row < 0 || row >= _th || col < 0 || col >= _tw) return c;

        int idx = row * _tw + col;

        return _src[idx];

    }
    public void FillPixelWithColor(int row, int col, Color custom)
    {
        if (!_prepared || _workTex == null) return;
        if (row < 0 || row >= _th || col < 0 || col >= _tw) return;

        int idx = row * _tw + col;

        // Nếu chỉ cho phép fill vùng "inside", kiểm tra alpha của _src
        if (_src[idx].a < 0.8f) return;

        _workTex.SetPixel(col, row, custom);

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