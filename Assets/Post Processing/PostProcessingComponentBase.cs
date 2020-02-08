using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace HDRPAdditions
{
    [Serializable]
    public abstract class PostProcessingBaseComponent<TComponent> : CustomPostProcessVolumeComponent, IPostProcessComponent where TComponent : CustomPostProcessVolumeComponent
    {
        public BoolParameter _enabled = new BoolParameter(false, true);

        protected Material _material;
        bool _initialized;
        string _shaderName;
        
        public void Initialize(string shaderName)
        {
            _shaderName = shaderName;
            _initialized = true;
        }

        public bool IsActive() => _material != null && _enabled.value == true;
    
        public override void Setup()
        {
            var shader = Shader.Find(_shaderName);
            if (shader != null)
            {
                _material = new Material(shader);
            }
            else
            {
                Debug.LogError($"Unable to find shader '{_shaderName}'. Post Process Volume Pixelation is unable to load.");
            }

            if (!_initialized)
            {
                Debug.Log("Base class 'PostProcessingBaseComponent' wasn't initialized! " +
                          "In order to do so, call 'Initialize()' in the constructor of the inheriting class");
            }
        }

        protected bool MayRender()
        {
            bool mayRender = true;
            if (!_initialized || _material == null)
            {
                mayRender = false;
            }

            return mayRender;
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }
    }
}
