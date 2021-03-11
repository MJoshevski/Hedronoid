using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class RenderPipelineSwitching
{
    private const string URP_ASSET_PATH = "Render Pipeline Assets/URP/UniversalRenderPipelineAsset";

    [MenuItem("Hedronoid/Switch Render Pipelines &`")]
    public static void SwitchRenderPipelines()
    {
        UniversalRenderPipelineAsset urpPipeline = Resources.Load(URP_ASSET_PATH) as UniversalRenderPipelineAsset;

        if (GraphicsSettings.currentRenderPipeline == urpPipeline)
            GraphicsSettings.renderPipelineAsset = null;
        else if (GraphicsSettings.currentRenderPipeline != urpPipeline)
            GraphicsSettings.renderPipelineAsset = urpPipeline;
    }
}
