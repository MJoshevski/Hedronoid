using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid.Core
{
    public class PlayerSpawner : HNDGameObject
    {
        public Transform[] SpawnPoints {  get { return m_SpawnPoints; } }
        [SerializeField]
        private Transform[] m_SpawnPoints;

        public Transform ActiveSpawnPoint { get { return m_ActiveSpawnPoint; } }
        [SerializeField]
        private Transform m_ActiveSpawnPoint;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
        public Transform GetSpawnPoint(int id)
        {
            if (m_SpawnPoints == null || id < 0 || id >= m_SpawnPoints.Length)
            {
                D.CoreError("There's no Spawn Point allocated to field no. " + id);
                return null;
            }

            return m_SpawnPoints[id];
        }

        public void SetActiveCheckpoint(GameObject checkpoint)
        {
            m_ActiveSpawnPoint = checkpoint.transform;
        }
    }
}