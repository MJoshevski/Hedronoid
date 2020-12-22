using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCulling : MonoBehaviour
{
    public List<Portal> portals = new List<Portal>();

    void OnPreCull()
    {
        for (int i = 0; i < portals.Count; i++)
        {
            portals[i].PrePortalRender();
        }
        for (int i = 0; i < portals.Count; i++)
        {
            portals[i].Render();
        }
        for (int i = 0; i < portals.Count; i++)
        {
            portals[i].PostPortalRender();
        }
    }
}
