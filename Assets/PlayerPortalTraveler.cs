using Hedronoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortalTraveler : PortalTraveller
{
    Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        cam.enabled = false;
        PlayerStateManager.Instance.transform.position = pos;
        Physics.SyncTransforms();
        cam.enabled = true;
    }
}
