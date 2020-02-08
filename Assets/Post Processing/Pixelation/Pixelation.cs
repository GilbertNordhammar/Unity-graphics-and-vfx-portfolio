using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace HDRPAdditions
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Pixelation")]
    public sealed class Pixelation : PostProcessingComponentBase
    {
        public Vector2Parameter _resolution = new Vector2Parameter(new Vector2(100, 100));

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        const string _shaderName = "Hidden/Shader/Pixelation";

        public Pixelation()
        {
            Initialize(_shaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (!MayRender())
            {
                return;
            }

            _material.SetTexture("_inputTexture", source);
            _material.SetVector("_resolution", _resolution.value);
            HDUtils.DrawFullScreen(cmd, _material, destination);
        }
    }
}