Shader "Custom/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
            "Queue" = "Geometry-100"  // Đảm bảo vẽ trước
            "RenderType" = "Opaque"
        }
        
        ColorMask 0  // Không ghi màu
        ZWrite Off   // Tắt depth write
        Cull Off     // Tắt culling để đảm bảo hiển thị cả 2 mặt
        
        Stencil {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Không cần sample texture, chỉ cần stencil
                discard;
                return 0;
            }
            ENDCG
        }
    }
}