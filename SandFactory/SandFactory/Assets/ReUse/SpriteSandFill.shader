Shader "Custom/SandFillEffect"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        
        [Header(Fill Control)]
        _FillProgress ("Fill Progress", Range(0, 1)) = 0.5
        _ApexX ("Apex X Position", Range(0, 1)) = 0.5
        _AlphaThreshold ("Alpha Inside Threshold", Range(0, 1)) = 0.01
        
        [Header(Fill Style)]
        [Toggle] _FillFromBottom ("Fill From Bottom", Float) = 1
        
        [Header(Edge Curve)]
        _EdgeCurvePower ("Edge Curve Power", Range(0.1, 5)) = 2.0
        _EdgeCurveStrength ("Edge Curve Strength", Range(0, 1)) = 0.5
        
        [Header(Thread Effect)]
        _ThreadHeight ("Thread Transition Height", Range(1, 20)) = 5
        _ColorVariation ("Color Variation", Range(0, 1)) = 0.2
        _ThreadSpeed ("Thread Animation Speed", Range(0, 10)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
            #pragma multi_compile _ _FILLFROMOTTOM_ON
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            float _FillProgress;
            float _ApexX;
            float _AlphaThreshold;
            float _FillFromBottom;
            float _EdgeCurvePower;
            float _EdgeCurveStrength;
            float _ThreadHeight;
            float _ColorVariation;
            float _ThreadSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            // Hash function for pseudo-random numbers
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            // Noise function
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Calculate edge curve offset
            float calculateEdgeOffset(float normalizedDist)
            {
                // Apply curve: peaks at center (0), dips at edges (1)
                float curve = pow(normalizedDist, _EdgeCurvePower);
                return (1.0 - curve) * _EdgeCurveStrength;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // If pixel is too transparent (outside), keep it transparent
                if (col.a < _AlphaThreshold)
                    return col;
                
                // Store original color
                fixed4 originalColor = col;
                
                // Calculate pixel position
                float pixelY = _FillFromBottom > 0.5 ? i.uv.y : (1.0 - i.uv.y);
                float pixelX = i.uv.x;
                
                // Calculate distance from apex
                float distFromApex = abs(pixelX - _ApexX);
                float maxRadius = max(_ApexX, 1.0 - _ApexX);
                float normalizedDist = saturate(distFromApex / maxRadius);
                
                // Apply edge curve to fill progress
                float edgeOffset = calculateEdgeOffset(normalizedDist);
                float adjustedProgress = _FillProgress - edgeOffset;
                
                // Determine if this pixel should be filled
                float fillThreshold = adjustedProgress;
                
                // Add thread transition effect
                float threadZone = _ThreadHeight * _MainTex_TexelSize.y;
                float distFromFillLine = pixelY - fillThreshold;
                
                // If pixel is below fill line (should be filled)
                if (pixelY < fillThreshold)
                {
                    // Check if in thread transition zone
                    if (distFromFillLine > -threadZone && distFromFillLine < 0)
                    {
                        // Thread transition colors
                        float threadProgress = (-distFromFillLine) / threadZone;
                        
                        // Animate thread position
                        float animTime = _Time.y * _ThreadSpeed;
                        float threadNoise = noise(float2(pixelX * 50.0, animTime + pixelY * 20.0));
                        
                        // Color variation based on position and noise
                        float colorShift = threadNoise * _ColorVariation;
                        
                        // Mix between thread color and original
                        float threadMix = smoothstep(0.0, 1.0, threadProgress);
                        
                        // Apply color variation
                        col.rgb = lerp(
                            col.rgb * (1.0 + colorShift * float3(0.9, 1.1, 0.8)),
                            originalColor.rgb,
                            threadMix
                        );
                        col.a = originalColor.a;
                    }
                    else
                    {
                        // Fully filled - use original color
                        col = originalColor;
                    }
                }
                else
                {
                    // Above fill line - make transparent
                    col.a = 0;
                }
                
                return col * i.color;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}