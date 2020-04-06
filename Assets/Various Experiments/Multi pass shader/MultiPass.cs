using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class MultiPass : CustomPass
{
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.

    RTHandle _buffer;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        _buffer = RTHandles.Alloc(Vector2.one, 
                                  TextureXR.slices, 
                                  dimension: TextureXR.dimension, 
                                  colorFormat: GraphicsFormat.R8G8B8A8_SRGB, 
                                  useDynamicScale: true, 
                                  name: "My Buffer");
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume
    }

    protected override void Cleanup()
    {
        _buffer.Release();
    }
}