Shader "Custom/SpriteFilled"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        [KeywordEnum(Horizontal, Vertical, Radial90, Radial180, Radial360)] _FillMode("Fill Mode", Float) = 0
        [KeywordEnum(Left, Right, Top, Bottom)] _FillOrigin("Fill Origin", Float) = 0
        _FillClockwise ("Fill Clockwise", Float) = 1
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
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _FILLMODE_HORIZONTAL _FILLMODE_VERTICAL _FILLMODE_RADIAL90 _FILLMODE_RADIAL180 _FILLMODE_RADIAL360
            #pragma multi_compile_local _FILLORIGIN_LEFT _FILLORIGIN_RIGHT _FILLORIGIN_TOP _FILLORIGIN_BOTTOM
            
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _FillAmount;
            float _FillMode;
            float _FillOrigin;
            float _FillClockwise;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }
            float2 getCenteredUV(float2 uv)
            {
                return (uv - 0.5) * 2.0;
            }
            float getRadialFill(float2 uv, float fillAmount, float segments, float clockwise)
            {
                float2 centered = getCenteredUV(uv);
                float angle = atan2(centered.y, centered.x);
                
                // Normalize angle to 0-1 range starting from top (90 degrees)
                angle = (angle / 3.14159265359 + 1.0) * 0.5;
                angle = frac(angle + 0.25); // Start from top
                
                if (clockwise < 0.5)
                    angle = 1.0 - angle;
                
                float fillAngle = fillAmount * (segments / 360.0);
                return step(angle, fillAngle);
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float fillMask = 1.0;
                float2 uv = i.uv;
                #if defined(_FILLMODE_HORIZONTAL)
                    #if defined(_FILLORIGIN_LEFT)
                        fillMask = step(uv.x, _FillAmount);
                    #elif defined(_FILLORIGIN_RIGHT)
                        fillMask = step(1.0 - uv.x, _FillAmount);
                    #else
                        fillMask = step(uv.x, _FillAmount);
                    #endif
                #elif defined(_FILLMODE_VERTICAL)
                    #if defined(_FILLORIGIN_BOTTOM)
                        fillMask = step(uv.y, _FillAmount);
                    #elif defined(_FILLORIGIN_TOP)
                        fillMask = step(1.0 - uv.y, _FillAmount);
                    #else
                        fillMask = step(uv.y, _FillAmount);
                    #endif
                #elif defined(_FILLMODE_RADIAL90)
                    fillMask = getRadialFill(uv, _FillAmount, 90.0, _FillClockwise);
                #elif defined(_FILLMODE_RADIAL180)
                    fillMask = getRadialFill(uv, _FillAmount, 180.0, _FillClockwise);
                #elif defined(_FILLMODE_RADIAL360)
                    fillMask = getRadialFill(uv, _FillAmount, 360.0, _FillClockwise);
                #endif
                fixed4 col = tex2D(_MainTex, uv) * i.color;
                col.a *= fillMask;
                
                clip(col.a - 0.01);
                
                return col;
            }
            ENDCG
        }
    }
}