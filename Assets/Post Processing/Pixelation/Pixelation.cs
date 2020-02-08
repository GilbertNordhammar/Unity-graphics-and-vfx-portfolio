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


//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.HighDefinition;
//using System;

//[Serializable, VolumeComponentMenu("Post-processing/Custom/Pixelation")]
//public sealed class Pixelation : CustomPostProcessVolumeComponent, IPostProcessComponent
//{
//    public BoolParameter _enabled = new BoolParameter(true);
//    public Vector2Parameter _resolution = new Vector2Parameter(new Vector2(100, 100));

//    Material m_Material;

//    public bool IsActive() => m_Material != null && _enabled.value == true;

//    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
//    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

//    const string kShaderName = "Hidden/Shader/Pixelation";

//    public override void Setup()
//    {
//        if (Shader.Find(kShaderName) != null)
//            m_Material = new Material(Shader.Find(kShaderName));
//        else
//            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Pixelation is unable to load.");
//    }

//    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
//    {
//        if (m_Material == null)
//            return;

//        m_Material.SetTexture("_inputTexture", source);
//        m_Material.SetVector("_resolution", _resolution.value);
//        HDUtils.DrawFullScreen(cmd, m_Material, destination);
//    }

//    public override void Cleanup()
//    {
//        CoreUtils.Destroy(m_Material);
//    }
//}
