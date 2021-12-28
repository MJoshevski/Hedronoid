using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid.Core
{
    public class PlayerSpawner : HNDGameObject
    {
        [SerializeField]
        public List<Transform> m_SpawnPoints = new List<Transform>();

        [HideInInspector]
        public Transform ActiveSpawnPoint;
        public Transform GetSpawnPoint(int id)
        {
            if (m_SpawnPoints == null || id < 0 || id >= m_SpawnPoints.Count)
            {
                D.CoreError("There's no Spawn Point allocated to field no. " + id);
                return null;
            }

            return m_SpawnPoints[id];
        }
    }
}