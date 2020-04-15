using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace HDRPAdditions
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Motion Blur (HDRPAdditions)")]
    public sealed class MotionBlur : PostProcessingComponentBase
    {
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        const string _shaderName = "Hidden/Shader/MotionBlur";

        public MotionBlur()
        {
            Initialize(_shaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (!MayRender())
            {
                return;
            }
            Debug.Log(camera.camera.cameraToWorldMatrix);

            _material.SetTexture("_InputTexture", source);
            HDUtils.DrawFullScreen(cmd, _material, destination);
        }
    }
}