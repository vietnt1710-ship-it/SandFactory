Shader "Custom/PixelSandFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Sand Fill Settings)]
        _ApexX01("Apex X (Normalized)", Range(0,1)) = 0.5
        _BaseY01("Base Y (Normalized)", Range(0,1)) = 0
        _GroupWidthPx("Group Width (Pixels)", Float) = 10
        _DropPerGroupPx("Drop Per Group (Pixels)", Float) = 4
        _FalloffStrength("Falloff Strength", Range(0.01,10)) = 3
        _Progress01("Progress", Range(0,1)) = 0
        
        [Enum(Log,0,Sqrt,1)] _FalloffMode("Falloff Mode", Int) = 0
        [Enum(Linear,0,Smooth,1,EaseInOutCubic,2)] _EaseMode("Ease Mode", Int) = 1
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            float _ApexX01;
            float _BaseY01;
            float _GroupWidthPx;
            float _DropPerGroupPx;
            float _FalloffStrength;
            float _Progress01;
            int _FalloffMode;
            int _EaseMode;

            float Ease01(float t)
            {
                t = saturate(t);
                if (_EaseMode == 1) // Smooth
                    return t * t * (3.0 - 2.0 * t);
                else if (_EaseMode == 2) // EaseInOutCubic
                    return (t < 0.5) ? 4.0 * t * t * t : 1.0 - pow(-2.0 * t + 2.0, 3.0) * 0.5;
                else // Linear
                    return t;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Get texture dimensions and pixel position
                float2 texSize = _MainTex_TexelSize.zw;
                float2 pixelPos = IN.texcoord * texSize;
                
                // Calculate apex and base positions
                float apexX = _ApexX01 * (texSize.x - 1);
                int baseY = _BaseY01 * (texSize.y - 1);
                
                int groupW = max(1, _GroupWidthPx);
                float leftSpan = apexX;
                float rightSpan = (texSize.x - 1) - apexX;
                float maxSpanPx = max(leftSpan, rightSpan);
                int maxGroups = max(1, ceil(maxSpanPx / groupW));
                
                // Maximum height
                float baseMax = texSize.y + maxGroups * _DropPerGroupPx;
                
                // Local progress with easing
                float tLocal = Ease01(_Progress01);
                
                // Current x position
                float x = pixelPos.x;
                float dx = abs((x + 0.5) - apexX);
                float gCont = dx / groupW;
                float gNorm = saturate(gCont / maxGroups);
                
                // Falloff calculation
                float k = max(0.01, _FalloffStrength);
                float f;
                if (_FalloffMode == 0) // Log
                    f = log(1.0 + k * gNorm) / log(1.0 + k);
                else // Sqrt
                    f = pow(gNorm, 0.5 / k);
                
                float gEff = f * maxGroups;
                
                // Fill height calculation
                int filledByFlow = baseMax * tLocal - _DropPerGroupPx * gEff;
                int fillHeight = clamp(max(baseY, filledByFlow), 0, texSize.y);
                
                // Only show pixels below fill height
                if (pixelPos.y >= fillHeight)
                {
                    color.a = 0;
                }
                
                return color;
            }
            ENDCG
        }
    }
}