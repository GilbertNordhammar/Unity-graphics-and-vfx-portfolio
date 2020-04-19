using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace HDRPAdditions
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Motion Blur (HDRPAdditions)")]
    public sealed class MotionBlur : PostProcessingComponentBase
    {
        public MinFloatParameter _intensity = new MinFloatParameter(10f, 0f);
        public MinFloatParameter _turningIntensity = new MinFloatParameter(3f, 0f);
        public ClampedFloatParameter _maxVelocity = new ClampedFloatParameter(1, 0, 1);
        public ClampedIntParameter _sampleCount = new ClampedIntParameter(8, 2, 20);
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        const string _shaderName = "Hidden/Shader/MotionBlur";

        Vector3 _prevCameraRotation;
        public MotionBlur()
        {
            Initialize(_shaderName);
        }

        bool IsCameraRotating(Vector3 rotation)
        {
            var difference = 0.1f;

            var xDifference = Mathf.Abs(rotation.x - _prevCameraRotation.x);
            var yDifference = Mathf.Abs(rotation.y - _prevCameraRotation.y);
            var zDifference = Mathf.Abs(rotation.z - _prevCameraRotation.z);

            return xDifference > difference || yDifference > difference || zDifference > difference;
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (!MayRender())
            {
                return;
            }

            _material.SetTexture("_inputTexture", source);
            _material.SetFloat("_intensity", _intensity.value);
            _material.SetFloat("_sampleCount", _sampleCount.value);
            _material.SetFloat("_maxVelocity", _maxVelocity.value);
            _material.SetFloat("_turningIntensity", _turningIntensity.value);

            var cameraRotation = camera.camera.transform.rotation.eulerAngles;
            var isCameraRotating = _prevCameraRotation != null ? IsCameraRotating(cameraRotation) : false;
            _prevCameraRotation = cameraRotation;
            _material.SetInt("_isCameraRotating", isCameraRotating ? 1 : 0);

            HDUtils.DrawFullScreen(cmd, _material, destination);
        }
    }
}