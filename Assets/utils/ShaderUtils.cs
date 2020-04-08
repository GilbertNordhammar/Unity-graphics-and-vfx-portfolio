using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace HDRPAdditions
{
    public class ShaderUtils
    {
        public static void RenderToGlobalTexture(CommandBuffer cmd,
                                                 RenderTexture targetRt,
                                                 string targetShaderTexture,
                                                 Material material,
                                                 int shaderPass)
        {
            cmd.Blit(null, targetRt, material, shaderPass);
            cmd.SetGlobalTexture(targetShaderTexture, targetRt);
        }
    }
}