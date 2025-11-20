Shader "IKame/3D/Unlit Perspective" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Perspective ("Perspective FOV", Float) = 1
	}
	
	SubShader{
		Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}
		LOD 200
		Pass
		{
			// Bật alpha blending
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;
			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}
			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;
			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};
		float4 frag(Fragment_Stage_Input input) : SV_TARGET
{
    float4 texColor = _MainTex.Sample(sampler_MainTex, input.uv.xy);
    
    // Lerp giữa màu trắng (1,1,1) và _Color.rgb dựa trên _Color.a
    float3 blendedColor = lerp(float3(1, 1, 1), _Color.rgb, _Color.a);
    
    return float4(texColor.rgb * blendedColor, 1.0);
}
			ENDHLSL
		}
	}
}