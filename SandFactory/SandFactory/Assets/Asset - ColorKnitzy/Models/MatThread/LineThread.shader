Shader "Custom/LineThread"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Threshold ("White Threshold", Range(0,1)) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
            float4 _TintColor;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Tính độ sáng (luminance) từ màu gốc
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // Tạo mask: 1 khi trắng, 0 khi tối
                float mask = smoothstep(_Threshold, 1.0, luminance);

                // Chỉ đổi màu phần trắng
                col.rgb = lerp(col.rgb, _TintColor.rgb, mask);

                return col;
            }
            ENDCG
        }
    }
}
