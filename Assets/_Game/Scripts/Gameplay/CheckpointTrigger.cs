using Hedronoid;
using Hedronoid.AI;
using Hedronoid.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : HNDMonoBehaviour
{
    private PlayerSpawner playerSpawner;
    public GameObject checkpointRespawnPosition;
    public GameObject physicalCheckpointObj;
    public List<ParticleList.ParticleSystems> OnCheckpointActivatePFX = new List<ParticleList.ParticleSystems>();

    protected override void Start()
    {
        base.Start();

        var persistents = GameObject.FindGameObjectsWithTag("Persistent");

        // Matej: DELETE ME PLX HACK
        foreach (GameObject go in persistents)
            if (go.GetComponent<PlayerSpawner>() != null)
                playerSpawner = go.GetComponent<PlayerSpawner>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (HNDAI.Settings.PlayerLayer ==
                (HNDAI.Settings.PlayerLayer | (1 << other.gameObject.layer)))
        {
            for (int i = 0; i < playerSpawner.m_SpawnPoints.Count; i++)
            {
                if (playerSpawner.m_SpawnPoints[i].transform.position == 
                    checkpointRespawnPosition.gameObject.transform.position)
                {
                    GameObject tmp = playerSpawner.m_SpawnPoints[i].gameObject;
                    GameObject tmp2 = playerSpawner.m_SpawnPoints[0].gameObject;

                    playerSpawner.m_SpawnPoints[0] = tmp.transform;
                    playerSpawner.m_SpawnPoints[i] = tmp2.transform;

                    for (int j = 0; j < OnCheckpointActivatePFX.Count; j++)
                        ParticleHelper.PlayParticleSystem(OnCheckpointActivatePFX[j], transform.position, transform.up);


                    physicalCheckpointObj.SetActive(false);
                    break;
                }
            }            
        }
    }
}
