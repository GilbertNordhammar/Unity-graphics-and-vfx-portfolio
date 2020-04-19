Shader "Hidden/Shader/MotionBlur"
{
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"
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

    TEXTURE2D_X(_inputTexture);
    float _intensity;
    float _sampleCount;
    float _maxVelocity;
    int _isCameraRotating;
    float _turningIntensity;
    
    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float depth = LoadCameraDepth(positionSS);
        float4 worldPos = float4(ComputeWorldSpacePosition(input.texcoord, depth, UNITY_MATRIX_I_VP), 1);
        
        float4 posNDC = float4(input.texcoord.x * 2 - 1, input.texcoord.y * 2 - 1, depth, 1);
        float4 prevPosNDC = mul(UNITY_MATRIX_PREV_VP, worldPos);
        prevPosNDC.y = -prevPosNDC.y; // y-direction becomes opposite of posNDC.y for some reason
        prevPosNDC /= prevPosNDC.w;

        float2 velocity = (prevPosNDC - posNDC)/2.f;
        velocity = abs(velocity) > 0.001 ? velocity : 0;
        velocity = clamp(velocity, -_maxVelocity, _maxVelocity);
        velocity *= InterleavedGradientNoise(positionSS, 0);
        velocity *= _isCameraRotating == 1 ? _turningIntensity : _intensity;
        
        float3 outColor = LOAD_TEXTURE2D_X(_inputTexture, positionSS).xyz;
        
        float2 deltaPos = velocity / _sampleCount;
        for(int i = 0; i < _sampleCount - 1; i++) 
        {
            input.texcoord += deltaPos;
            input.texcoord = clamp(input.texcoord, 0, 0.9999);
            positionSS = input.texcoord * _ScreenSize.xy;
            outColor += LOAD_TEXTURE2D_X(_inputTexture, positionSS).xyz;
        }
        outColor /= _sampleCount;
        
        return float4(outColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "MotionBlur"

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
