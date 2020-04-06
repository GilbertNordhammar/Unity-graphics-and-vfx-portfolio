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
        public FloatParameter _threshold = new FloatParameter(0.5f);
        public FloatParameter _intensity = new FloatParameter(1f);
        public ClampedIntParameter _scatter = new ClampedIntParameter(3, 1, 16);
        public ColorParameter _tint = new ColorParameter(Color.white);

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        const string _shaderName = "Hidden/Shader/Bloom";
        enum ShaderPass { Highlight, Blur, Final }
        RTHandle _initialRth;

        public BloomCustom()
        {
            Initialize(_shaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            cmd.BeginSample("Custom Bloom");

            _material.SetFloat("_highlightThreshold", _threshold.value);
            _material.SetFloat("_highlightIntensity", _intensity.value);
            _material.SetColor("_tintColor", _tint.value);

            if(_initialRth != null) {
                _initialRth.Release();
            }
            _initialRth = RTHandles.Alloc(camera.actualWidth / 2, camera.actualHeight / 2);
            
            var props = new MaterialPropertyBlock();
            props.SetTexture("_frameTexture", source);
            HDUtils.DrawFullScreen(cmd, _material, _initialRth, props, (int) ShaderPass.Highlight);
            cmd.SetGlobalTexture("_blurTexture", _initialRth);

            
            cmd.BeginSample("Downsampling");
            var tempRts = new List<RenderTexture>();
            for(int i = 0; i < _scatter.value; i++) {
                int divisor = (int) Math.Pow(2, i+1);
                int sampleTexWidth = camera.actualWidth / divisor;
                int sampleTexHeight = camera.actualHeight / divisor;

                cmd.SetGlobalVector("_blurTextureSize", new Vector4(sampleTexWidth, sampleTexHeight));
                var blurSampleOffset = new Vector4[11];
                
                // horizontal blurring
                float sampleTexTexelWidth = 1f / sampleTexWidth;
                for(int j = -5; j <= 5; j++) {
                    blurSampleOffset[j+5] = j * new Vector4(sampleTexTexelWidth, 0);
                }
                cmd.SetGlobalVectorArray("_blurSampleOffsets", blurSampleOffset);
                var rt1 = RenderTexture.GetTemporary(sampleTexWidth, sampleTexHeight);
                cmd.Blit(null, rt1, _material, (int) ShaderPass.Blur);
                cmd.SetGlobalTexture("_blurTexture", rt1);

                // vertical blurring asdasddas
                float sampleTexTexelHeight = 1f / sampleTexHeight;
                for(int j = -5; j <= 5; j++) {
                    blurSampleOffset[j+5] = j * new Vector4(0, sampleTexTexelHeight);
                }
                
                cmd.SetGlobalVectorArray("_blurSampleOffsets", blurSampleOffset);
                var rt2 = i < _scatter.value-1  ? 
                    RenderTexture.GetTemporary(sampleTexWidth/2, sampleTexHeight/2) :
                    RenderTexture.GetTemporary(sampleTexWidth * 2, sampleTexHeight * 2);
                cmd.Blit(null, rt2, _material, (int) ShaderPass.Blur);
                cmd.SetGlobalTexture("_blurTexture", rt2);

                tempRts.Add(rt1);
                tempRts.Add(rt2);
            }

            foreach(var rt in tempRts) {
                RenderTexture.ReleaseTemporary(rt);
            }
            cmd.EndSample("Downsampling");

            cmd.BeginSample("Upsampling");
            tempRts = new List<RenderTexture>();
            for(int i = 0; i < _scatter.value - 1; i++) {
                int divisor = (int) Math.Pow(2, _scatter.value-i-1);
                int sampleTexWidth = camera.actualWidth / divisor;
                int sampleTexHeight = camera.actualHeight / divisor;
    
                cmd.SetGlobalVector("_blurTextureSize", new Vector4(sampleTexWidth, sampleTexHeight));
                var blurSampleOffset = new Vector4[11];
                
                // horizontal blurring
                float sampleTexTexelWidth = 1f / sampleTexWidth;
                for(int j = -5; j <= 5; j++) {
                    blurSampleOffset[j+5] = j * new Vector4(sampleTexTexelWidth, 0);
                }
                cmd.SetGlobalVectorArray("_blurSampleOffsets", blurSampleOffset);
                var rt1 = RenderTexture.GetTemporary(sampleTexWidth, sampleTexHeight);
                cmd.Blit(null, rt1, _material, (int) ShaderPass.Blur);
                cmd.SetGlobalTexture("_blurTexture", rt1);

                // vertical blurring
                float sampleTexTexelHeight = 1f / sampleTexHeight;
                for(int j = -5; j <= 5; j++) {
                    blurSampleOffset[j+5] = j * new Vector4(0, sampleTexTexelHeight);
                }
                
                cmd.SetGlobalVectorArray("_blurSampleOffsets", blurSampleOffset);
                var rt2 = RenderTexture.GetTemporary(sampleTexWidth*2, sampleTexHeight*2);
                cmd.Blit(null, rt2, _material, (int) ShaderPass.Blur);
                cmd.SetGlobalTexture("_blurTexture", rt2);

                tempRts.Add(rt1);
                tempRts.Add(rt2);
            }
            foreach(var rt in tempRts) {
                RenderTexture.ReleaseTemporary(rt);
            }

            cmd.EndSample("Upsampling");
            
            HDUtils.DrawFullScreen(cmd, _material, destination, props, (int) ShaderPass.Final);

            cmd.EndSample("Custom Bloom");
        }
    }
}
