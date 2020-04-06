Shader "Hidden/Shader/Bloom"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
	#include "Assets/Resources/Shaders/utils/Blur.cginc"

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

    TEXTURE2D_X(_frameTexture);
    TEXTURE2D(_blurTexture);
    float2 _blurTextureSize;
    float2 _blurSampleOffsets[11];
    float4 _highlightFilter;
    float _highlightIntensity;
    float3 _tintColor;
    
    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float4 Highlight(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 frame = LOAD_TEXTURE2D_X(_frameTexture, positionSS).xyz;

		float brightness = (frame.r * 0.2126) + (frame.g * 0.7152) + (frame.b * 0.722);
        half soft = brightness - _highlightFilter.y;
        soft = clamp(soft, 0, _highlightFilter.z);
        soft = soft * soft * _highlightFilter.w;
        half contribution = max(soft, brightness - _highlightFilter.x);
		contribution /= max(brightness, 0.00001);

        float3 outColor =  frame * contribution * _highlightIntensity * _tintColor;

        return float4(outColor, 1);
    }

    float4 Blur(Varyings input) : SV_TARGET
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        
        float3 outColor = LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[0] ) * _blurTextureSize).xyz * 0.0093;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[1] ) * _blurTextureSize).xyz * 0.028002;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[2] ) * _blurTextureSize).xyz * 0.065984;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[3] ) * _blurTextureSize).xyz * 0.121703;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[4] ) * _blurTextureSize).xyz * 0.175713;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[5] ) * _blurTextureSize).xyz * 0.198596;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[6] ) * _blurTextureSize).xyz * 0.175713;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[7] ) * _blurTextureSize).xyz * 0.121703;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[8] ) * _blurTextureSize).xyz * 0.065984;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[9] ) * _blurTextureSize).xyz * 0.028002;
        outColor += LOAD_TEXTURE2D(_blurTexture, (input.texcoord + _blurSampleOffsets[10] ) * _blurTextureSize).xyz * 0.0093;
        
        return float4(outColor, 1);
    }

    float4 Final(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 frame = LOAD_TEXTURE2D_X(_frameTexture, positionSS).xyz;
        float3 blur = LOAD_TEXTURE2D(_blurTexture, positionSS).xyz;
		float3 bloom = frame + blur;
        return float4(bloom, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Highlight"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment Highlight
                #pragma vertex Vert
            ENDHLSL
        }

        Pass
        {
            Name "Blur"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment Blur
                #pragma vertex Vert
            ENDHLSL
        }

        Pass
        {
            Name "Final"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment Final
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
