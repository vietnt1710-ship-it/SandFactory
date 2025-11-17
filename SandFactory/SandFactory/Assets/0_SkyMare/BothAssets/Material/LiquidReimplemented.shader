Shader "Custom/LiquidCylinder" {
    Properties {
        _Color ("Liquid Color", Color) = (0.2, 0.5, 1.0, 1.0)
        _ColorShadow ("Shadow Color", Color) = (0.1, 0.2, 0.5, 1.0)
        _MatCap ("MatCap Texture", 2D) = "white" {}
        _Fill ("Fill Amount", Range(0, 1)) = 0.5
        _Waves ("Wave Intensity", Range(0, 1)) = 0.3
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 3.0
        _Rim ("Rim Power", Range(0, 5)) = 2.0
        
        [Header(Gravity Settings)]
        _GravityDir ("Gravity Direction", Vector) = (0, -1, 0, 0)
        _CylinderHeight ("Cylinder Height", Float) = 2.0
    }
    
    SubShader {
        Tags { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }
        LOD 200
        Cull Off // Render both sides
        
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float2 matcapUV : TEXCOORD3;
                float3 localPos : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };
            
            // Properties
            float4 _Color;
            float4 _ColorShadow;
            sampler2D _MatCap;
            float _Fill;
            float _Waves;
            float _WaveSpeed;
            float _WaveFrequency;
            float _Rim;
            float3 _GravityDir;
            float _CylinderHeight;
            
            // Wave function in world space along gravity plane
            float CalculateWave(float3 worldPos, float3 gravityDir) {
                float time = _Time.y * _WaveSpeed;
                
                // Create two perpendicular vectors to gravity for wave plane
                float3 up = abs(gravityDir.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
                float3 right = normalize(cross(up, gravityDir));
                float3 forward = cross(gravityDir, right);
                
                // Project world position onto wave plane
                float x = dot(worldPos, right);
                float z = dot(worldPos, forward);
                
                // Multiple wave layers
                float wave = 0.0;
                wave += sin(x * _WaveFrequency + time) * 0.5;
                wave += sin(z * _WaveFrequency * 1.3 + time * 1.2) * 0.3;
                wave += sin((x + z) * _WaveFrequency * 0.7 + time * 0.8) * 0.2;
                
                return wave * _Waves * 0.1;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                o.localPos = v.vertex.xyz;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Calculate MatCap UV
                float3 viewNormal = mul(UNITY_MATRIX_V, float4(o.worldNormal, 0.0)).xyz;
                o.matcapUV = viewNormal.xy * 0.5 + 0.5;
                
                UNITY_TRANSFER_FOG(o, o.pos);
                
                return o;
            }
            
            float4 frag(v2f i) : SV_Target {
                // Normalize gravity direction
                float3 gravityDir = normalize(_GravityDir);
                
                // Get object center in world space
                float3 objectCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Calculate position along gravity axis
                float3 relativePos = i.worldPos - objectCenter;
                float heightAlongGravity = dot(relativePos, -gravityDir);
                
                // Calculate fill levels
                float totalHeight = _CylinderHeight;
                float fillHeight = (_Fill - 0.5) * totalHeight; // Center at 0
                
                // Add waves
                float waveOffset = CalculateWave(i.worldPos, gravityDir);
                
                // Top surface (clip above liquid)
                float topSurface = fillHeight + waveOffset;
                clip(topSurface - heightAlongGravity);
                
                // Bottom surface (clip below cylinder bottom)
                float bottomSurface = -totalHeight * 0.5;
                clip(heightAlongGravity - bottomSurface);
                
                // MatCap lighting
                float4 matcapColor = tex2D(_MatCap, i.matcapUV);
                
                // Lighting based on surface orientation relative to gravity
                float upDot = dot(i.worldNormal, -gravityDir);
                float topLight = saturate(upDot);
                float4 finalColor = lerp(_ColorShadow, _Color, topLight * 0.5 + 0.5);
                
                // Apply MatCap
                finalColor *= matcapColor;
                
                // Fresnel/Rim effect
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _Rim);
                finalColor.rgb += fresnel * _Color.rgb * 0.5;
                
                // Surface wave highlight (at the top surface)
                float distToSurface = abs(heightAlongGravity - topSurface);
                float surfaceGlow = exp(-distToSurface * 20.0) * 0.3;
                finalColor.rgb += surfaceGlow * _Color.rgb;
                
                // Darken bottom slightly
                float depthFactor = saturate((heightAlongGravity - bottomSurface) / (topSurface - bottomSurface));
                finalColor.rgb *= lerp(0.7, 1.0, depthFactor);
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Diffuse"
}