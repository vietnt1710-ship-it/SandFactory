Shader "PolygonArsenal/PolyRimLightTransparent"
{
    Properties 
    {
        _InnerColor ("Inner Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimWidth ("Rim Width", Range(0.2,20.0)) = 3.0
        _RimGlow ("Rim Glow Multiplier", Range(0.0,9.0)) = 1.0
    }
    
    SubShader 
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Back
            Blend One One
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _InnerColor;
                float4 _RimColor;
                float _RimWidth;
                float _RimGlow;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                
                // Calculate rim lighting
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                half3 rimEmission = _RimColor.rgb * _RimGlow * pow(rim, _RimWidth);
                
                // Combine inner color with rim emission
                half3 finalColor = _InnerColor.rgb + rimEmission;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, _InnerColor.a);
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
}