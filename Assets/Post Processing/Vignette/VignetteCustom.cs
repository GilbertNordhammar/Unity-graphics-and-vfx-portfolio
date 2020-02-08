using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace HDRPAdditions
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Vignette (Custom)")]
    public sealed class VignetteCustom : PostProcessingComponentBase
    {
        public ColorParameter _color = new ColorParameter(Color.black);
        public Vector2Parameter _center = new Vector2Parameter(new Vector2(0.5f, 0.5f));
        public ClampedFloatParameter _intensity = new ClampedFloatParameter(1, 0, 10);
        public ClampedFloatParameter _smoothness = new ClampedFloatParameter(0, 0, 1);
        public BoolParameter _rounded = new BoolParameter(false);

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        const string _shaderName = "Hidden/Shader/Vignette";

        public VignetteCustom()
        {
            Initialize(_shaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if(!MayRender())
            {
                return;
            }

            _material.SetTexture("_inputTexture", source);
            _material.SetColor("_color", _color.value);
            _material.SetVector("_center", _center.value);
            _material.SetFloat("_intensity", _intensity.value);
            _material.SetFloat("_smoothness", _smoothness.value);
            _material.SetInt("_rounded", _rounded.value ? 1 : 0);


            HDUtils.DrawFullScreen(cmd, _material, destination);
        }
    }
}
