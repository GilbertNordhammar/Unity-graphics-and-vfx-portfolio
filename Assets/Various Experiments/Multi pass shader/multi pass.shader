Shader "Experiments/Multi pass" {
	HLSLINCLUDE
	
	#pragma target 4.5
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

	struct VertexInput
	{
		uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 positionCS : SV_POSITION;
		float2 texcoord   : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f Vert(VertexInput input) 
	{
		v2f output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	float4 AddingRed(v2f input) : SV_TARGET
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		return float4(1,1,0,1);
	}

	ENDHLSL
	
	SubShader
	{
		Pass
		{
			Name "Custom Pass 0"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
				#pragma fragment AddingRed
				#pragma vertex Vert
			ENDHLSL
		}
	}
	Fallback Off
}

//SubShader{

	//	// PASS #0 RED
	//	Pass{
	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag
	//		#pragma debug

	//		struct v2f {
	//			float4  pos : SV_POSITION;
	//			float2  uv : TEXCOORD0;
	//		};

	//		struct VertexIn
	//		{
	//			float4 vertex  : POSITION;
	//		};

	//		v2f vert(VertexIn v)
	//		{
	//			v2f o;
	//			o.pos = UnityObjectToClipPos(v.vertex);
	//			return o;
	//		}

	//		half4 frag(v2f i) : COLOR
	//		{
	//			return float4(1,0,0,1);
	//		}
	//		ENDCG
	//	}

	//	// PASS #1 GREEN
	//	Pass{
	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag

	//		struct v2f {
	//			float4  pos : SV_POSITION;
	//			float2  uv : TEXCOORD0;
	//		};

	//		struct VertexIn
	//		{
	//			float4 vertex  : POSITION;
	//		};

	//		v2f vert(VertexIn v)
	//		{
	//			v2f o;
	//			o.pos = UnityObjectToClipPos(v.vertex);
	//			return o;
	//		}

	//		half4 frag(v2f i) : COLOR
	//		{
	//			return float4(0,1,0,1);
	//		}
	//		ENDCG
	//	}

	//	// PASS #2 BLUE
	//	Pass{
	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag

	//		struct v2f {
	//			float4  pos : SV_POSITION;
	//			float2  uv : TEXCOORD0;
	//		};

	//		struct VertexIn
	//		{
	//			float4 vertex  : POSITION;
	//		};

	//		v2f vert(VertexIn v)
	//		{
	//			v2f o;
	//			o.pos = UnityObjectToClipPos(v.vertex);
	//			return o;
	//		}

	//		half4 frag(v2f i) : COLOR
	//		{
	//			return float4(0,0,1,1);
	//		}
	//		ENDCG
	//	}
	//}