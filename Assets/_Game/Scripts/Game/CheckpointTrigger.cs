using Hedronoid;
using Hedronoid.AI;
using Hedronoid.Core;
using Lowscope.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : HNDMonoBehaviour, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    private PlayerSpawner playerSpawner;
    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);

        playerSpawner = GameplaySceneContext.PlayerSpawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HNDAI.Settings.PlayerLayer ==
                (HNDAI.Settings.PlayerLayer | (1 << other.gameObject.layer)) &&
                playerSpawner.ActiveSpawnPoint != gameObject.transform)
        {
            playerSpawner.SetActiveCheckpoint(gameObject);
            playerSpawner.OnSave();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (HNDAI.Settings.PlayerLayer ==
        (HNDAI.Settings.PlayerLayer | (1 << other.gameObject.layer)) &&
                playerSpawner.ActiveSpawnPoint != gameObject.transform)
        {
            playerSpawner.SetActiveCheckpoint(gameObject);
            playerSpawner.OnSave();
        }
    }
}