using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCulling : MonoBehaviour
{
    public List<Portal> portals = new List<Portal>();

    void OnPreCull()
    {
        if (portals.Count == 0) return;

        for (int i = 0; i < portals.Count; i++)
        {
            if(portals[i])
                portals[i].PrePortalRender();
        }
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i])
                portals[i].Render();
        }
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i])
                portals[i].PostPortalRender();
        }
    }
}
