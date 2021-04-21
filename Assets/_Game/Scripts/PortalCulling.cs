using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalCulling : MonoBehaviour
{
    public List<Portal> portals = new List<Portal>();

    void Start()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    // URP Variant
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Put the code that you want to execute before the camera renders here
        // If you are using URP or HDRP, Unity calls this method automatically
        // If you are writing a custom SRP, you must call RenderPipeline.BeginCameraRendering

        if (portals.Count == 0) return;

        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i])
                portals[i].PrePortalRender();
        }
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i])
                portals[i].Render(context);
        }
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i])
                portals[i].PostPortalRender();
        }
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // Built-in RP variant
    //void OnPreCull()
    //{
    //    if (portals.Count == 0) return;

    //    for (int i = 0; i < portals.Count; i++)
    //    {
    //        if (portals[i])
    //            portals[i].PrePortalRender();
    //    }
    //    for (int i = 0; i < portals.Count; i++)
    //    {
    //        if (portals[i])
    //            portals[i].Render();
    //    }
    //    for (int i = 0; i < portals.Count; i++)
    //    {
    //        if (portals[i])
    //            portals[i].PostPortalRender();
    //    }
    //}
}
