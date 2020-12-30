using Hedronoid;
using Hedronoid.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortalTraveler : PortalTraveller, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        cam.enabled = false;
        GameplaySceneContext.Player.transform.position = pos;
        Physics.SyncTransforms();
        cam.enabled = true;
    }
}
