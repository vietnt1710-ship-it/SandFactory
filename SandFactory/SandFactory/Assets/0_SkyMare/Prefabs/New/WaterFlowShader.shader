Shader "Universal/WaterFlowShader"
{
    Properties
    {
        _FlowTex ("Flow Texture (Mask)", 2D) = "white" {}
        _Color ("Water Color", Color) = (0, 0.5, 1, 1)
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.5
        _Opacity ("Opacity", Range(0, 1)) = 0.8
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 2.0
        _Fill ("Fill Amount", Range(0, 1)) = 1.0
        _FlowDirection ("Flow Direction", Vector) = (0, -1, 0, 0)
        
        // URP specific
        [HideInInspector] _Surface("__surface", Float) = 1.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 5.0
        [HideInInspector] _DstBlend("__dst", Float) = 10.0
        [HideInInspector] _ZWrite("__zw", Float) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float height : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            TEXTURE2D(_FlowTex);
            SAMPLER(sampler_FlowTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FlowTex_ST;
                float4 _Color;
                float _FlowSpeed;
                float _Opacity;
                float _EmissionStrength;
                float _Fill;
                float4 _FlowDirection;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                output.uv = TRANSFORM_TEX(input.uv, _FlowTex);
                output.height = input.uv.y;
                
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Fill Amount logic:
                // 0.0 -> 0.5: Fill từ đáy lên đỉnh (rót nước)
                // 0.5 -> 1.0: Rút từ đáy lên đỉnh (nước ở đáy biến mất trước)
                
                float fillProgress;
                
                if (_Fill <= 0.5)
                {
                    // Giai đoạn 1: Rót nước (0 -> 0.5)
                    // Chuyển đổi 0->0.5 thành 0->1
                    fillProgress = _Fill * 2.0;
                    // Render từ đáy lên: height >= (1 - fillProgress)
                    clip(fillProgress - (1.0 - input.height));
                }
                else
                {
                    // Giai đoạn 2: Rút nước từ đáy (0.5 -> 1.0)
                    // Chuyển đổi 0.5->1.0 thành 1->0 (đảo ngược)
                    fillProgress = 1.0 - ((_Fill - 0.5) * 2.0);
                    // Render phần trên đỉnh: height <= fillProgress
                    // Tức là phần đáy (height cao) sẽ bị clip trước
                    clip((1.0 - input.height) - (1.0 - fillProgress));
                }
                
                // Tạo hiệu ứng chảy
                float2 flowUV = input.uv;
                flowUV.y += _Time.y * _FlowSpeed;
                
                // Sample texture như mask (chỉ lấy giá trị grayscale)
                half4 texMask = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, flowUV);
                float maskValue = (texMask.r + texMask.g + texMask.b) / 3.0;
                
                // Áp dụng màu từ Color property
                half4 col = _Color;
                col.rgb *= _EmissionStrength;
                
                // Fade ở ranh giới
                float edgeFade;
                if (_Fill <= 0.5)
                {
                    // Fade ở đỉnh khi đang fill lên
                    float distanceFromTop = fillProgress - (1.0 - input.height);
                    edgeFade = smoothstep(0.0, 0.05, distanceFromTop);
                }
                else
                {
                    // Fade ở đáy khi đang rút từ dưới
                    float distanceFromDrainLine = (1.0 - input.height) - (1.0 - fillProgress);
                    edgeFade = smoothstep(0.0, 0.05, distanceFromDrainLine);
                }
                
                // Hiệu ứng sóng
                float wave = sin(input.height * 10.0 + _Time.y * 3.0) * 0.5 + 0.5;
                col.rgb *= (0.8 + wave * 0.2);
                
                // Áp dụng opacity
                col.a = maskValue * _Opacity * edgeFade;
                
                // Apply fog
                col.rgb = MixFog(col.rgb, input.fogFactor);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline" = ""
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float height : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            sampler2D _FlowTex;
            float4 _FlowTex_ST;
            float4 _Color;
            float _FlowSpeed;
            float _Opacity;
            float _EmissionStrength;
            float _Fill;
            float4 _FlowDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _FlowTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.height = v.uv.y;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fill Amount logic:
                // 0.0 -> 0.5: Fill từ đáy lên đỉnh (rót nước)
                // 0.5 -> 1.0: Rút từ đáy lên đỉnh (nước ở đáy biến mất trước)
                
                float fillProgress;
                
                if (_Fill <= 0.5)
                {
                    // Giai đoạn 1: Rót nước (0 -> 0.5)
                    // Chuyển đổi 0->0.5 thành 0->1
                    fillProgress = _Fill * 2.0;
                    // Render từ đáy lên: height >= (1 - fillProgress)
                    clip(fillProgress - (1.0 - i.height));
                }
                else
                {
                    // Giai đoạn 2: Rút nước từ đáy (0.5 -> 1.0)
                    // Chuyển đổi 0.5->1.0 thành 1->0 (đảo ngược)
                    fillProgress = 1.0 - ((_Fill - 0.5) * 2.0);
                    // Render phần trên đỉnh: height <= fillProgress
                    // Tức là phần đáy (height cao) sẽ bị clip trước
                    clip((1.0 - i.height) - (1.0 - fillProgress));
                }
                
                // Tạo hiệu ứng chảy
                float2 flowUV = i.uv;
                flowUV.y += _Time.y * _FlowSpeed;
                
                // Sample texture như mask (chỉ lấy giá trị grayscale)
                fixed4 texMask = tex2D(_FlowTex, flowUV);
                float maskValue = (texMask.r + texMask.g + texMask.b) / 3.0;
                
                // Áp dụng màu từ Color property
                fixed4 col = _Color;
                col.rgb *= _EmissionStrength;
                
                // Fade ở ranh giới
                float edgeFade;
                if (_Fill <= 0.5)
                {
                    // Fade ở đỉnh khi đang fill lên
                    float distanceFromTop = fillProgress - (1.0 - i.height);
                    edgeFade = smoothstep(0.0, 0.05, distanceFromTop);
                }
                else
                {
                    // Fade ở đáy khi đang rút từ dưới
                    float distanceFromDrainLine = (1.0 - i.height) - (1.0 - fillProgress);
                    edgeFade = smoothstep(0.0, 0.05, distanceFromDrainLine);
                }
                
                // Hiệu ứng sóng
                float wave = sin(i.height * 10.0 + _Time.y * 3.0) * 0.5 + 0.5;
                col.rgb *= (0.8 + wave * 0.2);
                
                // Áp dụng opacity
                col.a = maskValue * _Opacity * edgeFade;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}