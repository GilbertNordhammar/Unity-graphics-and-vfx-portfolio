Shader "Hidden/Shader/Vignette"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        
		UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
	half4 _color;
	half2 _center;
	half _intensity;
	half _smoothness;
	int _rounded;

    TEXTURE2D_X(_inputTexture);

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		// Setting aspect ratio
		float2 centerToPos = input.texcoord - _center;
		if (_rounded == 1) {
			if (_ScreenSize.x > _ScreenSize.y) {
				centerToPos.x *= _ScreenSize.x / _ScreenSize.y;
			}
			else {
				centerToPos.y *= _ScreenSize.y / _ScreenSize.x;
			}
		}
		
		// Calculating vignette
		float distFromCenter = sqrt(dot(centerToPos, centerToPos));
		float vignette = (1 - distFromCenter * _intensity);
		vignette = smoothstep(0, _smoothness, vignette);

		uint2 positionSS = input.texcoord * _ScreenSize.xy;
		float3 outColor = LOAD_TEXTURE2D_X(_inputTexture, positionSS).xyz;
		outColor = saturate(outColor * vignette);
		outColor += (1 - vignette) * _color;

		return float4(outColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Vingette"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
