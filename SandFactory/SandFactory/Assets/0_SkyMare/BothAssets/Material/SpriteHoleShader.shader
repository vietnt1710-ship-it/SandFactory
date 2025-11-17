Shader "Custom/SpriteHoleShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SpriteMask ("Sprite Mask", 2D) = "white" {}
        _Process ("Process (0-1)", Range(0, 1)) = 0.5
        _MinScale ("Min Scale", Range(0.01, 1)) = 0.1
        _MaxScale ("Max Scale", Range(0.01, 2)) = 1.0
        
        // Unity UI properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
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
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _SpriteMask;
            float4 _MainTex_ST;
            float4 _SpriteMask_ST;
            float _Process;
            float _MinScale;
            float _MaxScale;
            float4 _ClipRect;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex.xy;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu từ texture chính (hỗ trợ tiled)
                fixed4 mainColor = tex2D(_MainTex, i.uv) * i.color;
                
                // Tính toán scale dựa trên process
                float scale = lerp(_MinScale, _MaxScale, _Process);
                
                // Normalize UV về screen space [0,1] cho tiled
                // Lấy phần fractional của UV để xử lý tiling
                float2 normalizedUV = frac(i.uv);
                
                // Tính toán UV cho sprite mask
                // Đưa UV về trung tâm (0,0)
                float2 centeredUV = normalizedUV - 0.5;
                
                // Scale UV
                float2 spriteUV = centeredUV / scale;
                
                // Đưa về khoảng [0,1]
                spriteUV += 0.5;
                
                // Kiểm tra xem pixel có nằm trong vùng sprite không
                if (spriteUV.x >= 0.0 && spriteUV.x <= 1.0 && 
                    spriteUV.y >= 0.0 && spriteUV.y <= 1.0)
                {
                    // Lấy alpha từ sprite mask
                    fixed4 maskColor = tex2D(_SpriteMask, spriteUV);
                    
                    // Nếu alpha > 0, làm trong suốt (tạo lỗ)
                    if (maskColor.a > 0.0)
                    {
                        discard; // Loại bỏ pixel này
                    }
                }
                
                // Apply clipping cho UI
                mainColor.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(mainColor.a - 0.001);
                #endif
                
                return mainColor;
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}