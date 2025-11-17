using System;
using UnityEngine;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using UnityEngine.UI;
#endif

[ExecuteAlways]
public class PixelSandFill : MonoBehaviour
{
    [Header("Source Sprite")]
    public Sprite sourceSprite; // nếu để trống sẽ lấy sprite từ SpriteRenderer/Image

   
    public enum AxisMode { Normalized01, Pixel }
    [Header("Apex (đỉnh) & Base (đáy)")]
    [Tooltip("Vị trí đỉnh (theo X) - tâm điểm cát chảy")]
    public AxisMode apexXMode = AxisMode.Normalized01;
    [Range(0f, 1f)] public float apexX01 = 0.5f;
    public int apexXPx = 0;

    [Tooltip("Vị trí đáy (theo Y) - mọi pixel dưới đây luôn được fill")]
    public AxisMode baseYMode = AxisMode.Pixel;
    [Range(0f, 1f)] public float baseY01 = 0f;
    public int baseYPx = 0;

    [Header("Shape Settings")]
    [Min(1)] public int groupWidthPx = 10;
    [Min(0)] public int dropPerGroupPx = 4;

    public enum FalloffMode { Log, Sqrt }
    public FalloffMode falloffMode = FalloffMode.Log;
    [Range(0.01f, 10f)] public float falloffStrength = 3f;

 
    public enum EaseMode { Linear, Smooth, EaseInOutCubic }
    [Header("Animation")]
    public EaseMode easeMode = EaseMode.Smooth;

    [Range(0f, 1f)] public float progress01 = 0f;
    public bool autoAnimate = false;
    public float speed = 0.5f;

    // runtime
    SpriteRenderer _sr;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
    Image _img;
#endif
    Texture2D _runtimeTex;
    Sprite _runtimeSprite;
    Color[] _src;
    Color[] _buf;
    int _w, _h;
    bool _inited;

    void Awake() => TryInit();
    void OnEnable() => TryInit();

    void Update()
    {
        if (!_inited) TryInit();
        if (!_inited) return;

        if (autoAnimate)
            progress01 = Mathf.Clamp01(progress01 + speed * Time.deltaTime);

        ApplyFill();
    }

    void OnValidate()
    {
        if (_inited) ApplyFill();
    }

    void TryInit()
    {
        if (_inited) return;

        _sr = GetComponent<SpriteRenderer>();
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        _img = GetComponent<Image>();
#endif

        var spr = sourceSprite != null ? sourceSprite :
                  (_sr ? _sr.sprite :
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
                  (_img ? _img.sprite :
#endif
                  null
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
                  )
#endif
                  );

        if (spr == null || spr.texture == null) return;

        Rect r = spr.rect;
        _w = Mathf.RoundToInt(r.width);
        _h = Mathf.RoundToInt(r.height);

        _src = spr.texture.GetPixels(
            Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), _w, _h);

        _runtimeTex = new Texture2D(_w, _h, TextureFormat.RGBA32, false)
        {
            filterMode = spr.texture.filterMode,
            wrapMode = TextureWrapMode.Clamp
        };
        _buf = new Color[_w * _h];

        var pivot = new Vector2(spr.pivot.x / r.width, spr.pivot.y / r.height);
        _runtimeSprite = Sprite.Create(_runtimeTex, new Rect(0, 0, _w, _h), pivot, spr.pixelsPerUnit);

        if (_sr) _sr.sprite = _runtimeSprite;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        if (_img) _img.sprite = _runtimeSprite;
#endif

        _inited = true;
        ApplyFill();
    }

    void ApplyFill()
    {
        if (!_inited) return;
        Array.Fill(_buf, new Color(0, 0, 0, 0));

        // --- Lấy toạ độ đỉnh và đáy ---
        float apexX = (apexXMode == AxisMode.Normalized01) ? apexX01 * (_w - 1) : Mathf.Clamp(apexXPx, 0, _w - 1);
        int baseY = (baseYMode == AxisMode.Normalized01) ? Mathf.RoundToInt(baseY01 * (_h - 1))
                                                         : Mathf.Clamp(baseYPx, 0, _h);

        int groupW = Mathf.Max(1, groupWidthPx);
        float leftSpan = apexX;
        float rightSpan = (_w - 1) - apexX;
        float maxSpanPx = Mathf.Max(leftSpan, rightSpan);
        int maxGroups = Mathf.Max(1, Mathf.CeilToInt(maxSpanPx / groupW));

        // Độ cao tối đa
        float baseMax = _h + maxGroups * (float)dropPerGroupPx;

        // tiến độ chung
        float tLocal = Ease01(progress01);

        for (int x = 0; x < _w; x++)
        {
            float dx = Mathf.Abs((x + 0.5f) - apexX);
            float gCont = dx / groupW;
            float gNorm = Mathf.Clamp01(gCont / maxGroups);

            // --- Falloff (thoải dần) ---
            float k = Mathf.Max(0.01f, falloffStrength);
            float f = (falloffMode == FalloffMode.Log)
                      ? Mathf.Log(1f + k * gNorm) / Mathf.Log(1f + k)
                      : Mathf.Pow(gNorm, 0.5f / k);
            float gEff = f * maxGroups;

            // --- Chiều cao fill ---
            int filledByFlow = Mathf.RoundToInt(baseMax * tLocal - (float)dropPerGroupPx * gEff);
            int fillHeight = Mathf.Clamp(Mathf.Max(baseY, filledByFlow), 0, _h);

            if (fillHeight <= 0) continue;

            int idx = x;
            for (int y = 0; y < fillHeight; y++, idx += _w)
                _buf[idx] = _src[idx];
        }

        _runtimeTex.SetPixels(_buf);
        _runtimeTex.Apply(false, false);
    }
    int ApexHeightPx()
    {
        int groupW = Mathf.Max(1, groupWidthPx);

        // Tính maxGroups dựa trên vị trí đỉnh
        float apexX = (apexXMode == AxisMode.Normalized01) ? apexX01 * (_w - 1) : Mathf.Clamp(apexXPx, 0, _w - 1);
        float leftSpan = apexX;
        float rightSpan = (_w - 1) - apexX;
        float maxSpanPx = Mathf.Max(leftSpan, rightSpan);
        int maxGroups = Mathf.Max(1, Mathf.CeilToInt(maxSpanPx / groupW));

        float baseMax = _h + maxGroups * (float)dropPerGroupPx;
        float tLocal = Ease01(progress01);

        // Đỉnh không bị trừ drop (gEff = 0)
        int apex = Mathf.RoundToInt(baseMax * tLocal);
        // Luôn tôn trọng đáy
        return Mathf.Clamp(Mathf.Max(baseYPx, apex), 0, _h);
    }

    float Ease01(float t)
    {
        t = Mathf.Clamp01(t);
        switch (easeMode)
        {
            case EaseMode.Smooth: return t * t * (3f - 2f * t);
            case EaseMode.EaseInOutCubic: return (t < 0.5f) ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            default: return t;
        }
    }
}
