using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class Outline : CustomPass
{
    public LayerMask _outlineLayer = 0;
    [ColorUsage(false, true)]
    public Color _outlineColor = Color.black;
    public float _threshold = 1;

    // To make sure the shader will ends up in the build, we keep it's reference in the custom pass
    [SerializeField, HideInInspector]
    Shader _outlineShader;

    Material _fullscreenOutline;
    MaterialPropertyBlock _outlineProperties;
    ShaderTagId[] _shaderTags;
    RTHandle _outlineBuffer;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        _outlineShader = Shader.Find("Hidden/Outline");
        _fullscreenOutline = CoreUtils.CreateEngineMaterial(_outlineShader);
        _outlineProperties = new MaterialPropertyBlock();

        // List all the materials that will be replaced in the frame
        _shaderTags = new ShaderTagId[3]
        {
            new ShaderTagId("Forward"),
            new ShaderTagId("ForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
        };

        _outlineBuffer = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
            useDynamicScale: true, name: "Outline Buffer"
        );
    }

    void DrawOutlineMeshes(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        var result = new RendererListDesc(_shaderTags, cullingResult, hdCamera.camera)
        {
            // We need the lighting render configuration to support rendering lit objects
            rendererConfiguration = PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume | PerObjectData.Lightmaps,
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.BackToFront,
            excludeObjectMotionVectors = false,
            layerMask = _outlineLayer,
        };

        CoreUtils.SetRenderTarget(cmd, _outlineBuffer, ClearFlag.Color);
        HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(result));
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
    {
        DrawOutlineMeshes(renderContext, cmd, camera, cullingResult);

        SetCameraRenderTarget(cmd);
        
        _outlineProperties.SetColor("_OutlineColor", _outlineColor);
        _outlineProperties.SetTexture("_OutlineBuffer", _outlineBuffer);
        _outlineProperties.SetFloat("_Threshold", _threshold);
        CoreUtils.DrawFullScreen(cmd, _fullscreenOutline, _outlineProperties);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(_fullscreenOutline);
        _outlineBuffer.Release();
    }
}