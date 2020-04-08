using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using System;
using System.Collections.Generic;

namespace HDRPAdditions
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Bloom (Custom)")]
    public sealed class BloomCustom : PostProcessingComponentBase
    {
        public MinFloatParameter _threshold = new MinFloatParameter(0.5f, 0f);
        public ClampedFloatParameter _softThreshold = new ClampedFloatParameter(0.5f, 0, 1);
        public MinFloatParameter _intensity = new MinFloatParameter(1f, 0f);
        public ClampedIntParameter _scatter = new ClampedIntParameter(3, 1, 16);
        public ColorParameter _tint = new ColorParameter(Color.white);

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        const string _shaderName = "Hidden/Shader/Bloom";
        enum ShaderPass { Highlight, Blur, Final }
        enum SamplingDirection { Horizontal, Vertical }
        RTHandle _initialRth;

        public BloomCustom()
        {
            Initialize(_shaderName);
        }

        void ApplyParameters()
        {
            _material.SetFloat("_highlightIntensity", _intensity.value);
            _material.SetColor("_tintColor", _tint.value);

            float knee = _threshold.value * _softThreshold.value;
            Vector4 highlightFilter = new Vector4(
                _threshold.value,
                _threshold.value - knee,
                2f * knee,
                0.25f / (knee + 0.00001f)
            );
            _material.SetVector("_highlightFilter", highlightFilter);
        }

        void CreateHighlightFilter(CommandBuffer cmd, RTHandle source, HDCamera camera)
        {
            if (_initialRth != null)
            {
                _initialRth.Release();
            }
            _initialRth = RTHandles.Alloc(camera.actualWidth / 2, camera.actualHeight / 2);

            _material.SetTexture("_frameTexture", source);
            HDUtils.DrawFullScreen(cmd, _material, _initialRth, null, (int)ShaderPass.Highlight);
            cmd.SetGlobalTexture("_blurTexture", _initialRth);
        }

        Vector4[] GetBlurSamplingOffsets(int textureWidth, int textureHeight,
                                         SamplingDirection direction)
        {
            Vector4 increment;
            if (direction == SamplingDirection.Horizontal)
            {
                float texelWidth = 1f / textureWidth;
                increment = new Vector4(texelWidth, 0);
            }
            else
            {
                float texelHeight = 1f / textureHeight;
                increment = new Vector4(0, texelHeight);
            }

            Vector4[] samplingOffsets = new Vector4[11];
            for (int j = -5; j <= 5; j++)
            {
                samplingOffsets[j + 5] = j * increment;
            }

            return samplingOffsets;
        }

        void BlurHighlight(CommandBuffer cmd, HDCamera camera)
        {
            cmd.BeginSample("Downsampling");

            var tempRts = new List<RenderTexture>();
            for (int i = 0; i < _scatter.value; i++)
            {
                int divisor = (int)Math.Pow(2, i + 1);
                int sampleTexWidth = camera.actualWidth / divisor;
                int sampleTexHeight = camera.actualHeight / divisor;

                cmd.SetGlobalVector("_blurTextureSize", new Vector4(sampleTexWidth, sampleTexHeight));

                // horizontal blurring
                cmd.SetGlobalVectorArray("_blurSampleOffsets",
                    GetBlurSamplingOffsets(sampleTexWidth, sampleTexHeight, SamplingDirection.Horizontal));
                var rt1 = RenderTexture.GetTemporary(sampleTexWidth, sampleTexHeight);
                ShaderUtils.RenderToGlobalTexture(cmd, rt1, "_blurTexture", _material, (int)ShaderPass.Blur);

                // vertical blurring
                cmd.SetGlobalVectorArray("_blurSampleOffsets",
                    GetBlurSamplingOffsets(sampleTexWidth, sampleTexHeight, SamplingDirection.Vertical));
                var rt2 = i < _scatter.value - 1 ?
                    RenderTexture.GetTemporary(sampleTexWidth / 2, sampleTexHeight / 2) :
                    RenderTexture.GetTemporary(sampleTexWidth * 2, sampleTexHeight * 2);
                ShaderUtils.RenderToGlobalTexture(cmd, rt2, "_blurTexture", _material, (int)ShaderPass.Blur);

                tempRts.Add(rt1);
                tempRts.Add(rt2);
            }
            cmd.EndSample("Downsampling");

            foreach (var rt in tempRts)
            {
                RenderTexture.ReleaseTemporary(rt);
            }

            cmd.BeginSample("Upsampling");
            tempRts = new List<RenderTexture>();
            for (int i = 0; i < _scatter.value - 1; i++)
            {
                int divisor = (int)Math.Pow(2, _scatter.value - i - 1);
                int sampleTexWidth = camera.actualWidth / divisor;
                int sampleTexHeight = camera.actualHeight / divisor;

                cmd.SetGlobalVector("_blurTextureSize", new Vector4(sampleTexWidth, sampleTexHeight));

                // horizontal blurring
                cmd.SetGlobalVectorArray("_blurSampleOffsets",
                    GetBlurSamplingOffsets(sampleTexWidth, sampleTexHeight, SamplingDirection.Horizontal));
                var rt1 = RenderTexture.GetTemporary(sampleTexWidth, sampleTexHeight);
                ShaderUtils.RenderToGlobalTexture(cmd, rt1, "_blurTexture", _material, (int)ShaderPass.Blur);

                // vertical blurring
                cmd.SetGlobalVectorArray("_blurSampleOffsets",
                    GetBlurSamplingOffsets(sampleTexWidth, sampleTexHeight, SamplingDirection.Vertical));
                var rt2 = RenderTexture.GetTemporary(sampleTexWidth * 2, sampleTexHeight * 2);
                ShaderUtils.RenderToGlobalTexture(cmd, rt2, "_blurTexture", _material, (int)ShaderPass.Blur);

                tempRts.Add(rt1);
                tempRts.Add(rt2);
            }

            foreach (var rt in tempRts)
            {
                RenderTexture.ReleaseTemporary(rt);
            }
            cmd.EndSample("Upsampling");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            cmd.BeginSample("Custom Bloom");

            ApplyParameters();
            CreateHighlightFilter(cmd, source, camera);
            BlurHighlight(cmd, camera);

            HDUtils.DrawFullScreen(cmd, _material, destination, null, (int)ShaderPass.Final);

            cmd.EndSample("Custom Bloom");
        }
    }
}
