using Hedronoid;
using Hedronoid.Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageHandlersManager : HNDGameObject
{
    private DamageHandler[] handlers;
    private IDamagedHandlerManagerExtension[] extensions;

    protected override void Awake()
    {
        base.Awake();
        handlers = GetComponentsInChildren<DamageHandler>(true);
        foreach (var item in handlers)
        {
            item.damagedEvent += DamagedHandler;
        }
        extensions = GetComponents<IDamagedHandlerManagerExtension>();
    }

    private void DamagedHandler(DamageHandler damagedHandler, DamageInfo damagedInfo, HealthBase health, HealthInfo healthInfo)
    {
        foreach (var item in extensions)
        {
            if(item.MeetCondition(damagedHandler, damagedInfo, health, healthInfo))
                item.Handle(damagedHandler, damagedInfo, health, healthInfo);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var item in handlers)
        {
            item.damagedEvent -= DamagedHandler;
        }
    }
}
