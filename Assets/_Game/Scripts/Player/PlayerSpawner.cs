using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Lowscope.Saving;

namespace Hedronoid.Core
{
    public class PlayerSpawner : HNDGameObject, ISaveable
    {
        public Transform[] SpawnPoints {  get { return m_SpawnPoints; } }
        [SerializeField]
        private Transform[] m_SpawnPoints;

        public Transform ActiveSpawnPoint { get { return m_ActiveSpawnPoint; } }
        [SerializeField]
        private Transform m_ActiveSpawnPoint;

        private Transform m_LastActiveSpawnPoint;

        public Transform GetSpawnPoint(int id)
        {
            if (m_SpawnPoints == null || id < 0 || id >= m_SpawnPoints.Length)
            {
                D.CoreError("There's no Spawn Point allocated to field no. " + id);
                return null;
            }

            return m_SpawnPoints[id];
        }

        [System.Serializable]
        public struct SaveData
        {
            public Vector3 spawnPointPos;
        }
        public void SetActiveCheckpoint(GameObject checkpoint)
        {
            m_ActiveSpawnPoint = checkpoint.transform;
        }

        public void OnLoad(string data)
        {
            m_ActiveSpawnPoint = 
                GetSpawnPointTransformByPosition(JsonUtility.FromJson<SaveData>(data).spawnPointPos);
             m_LastActiveSpawnPoint = m_ActiveSpawnPoint;
        }

        public string OnSave()
        {
            return JsonUtility.ToJson(new SaveData() { spawnPointPos = m_ActiveSpawnPoint.position}); ;
        }

        public bool OnSaveCondition()
        {
            if (m_LastActiveSpawnPoint != m_ActiveSpawnPoint)
            {
                m_LastActiveSpawnPoint = m_ActiveSpawnPoint;
                return true;
            }

            return false;
        }

        private Transform GetSpawnPointTransformByPosition(Vector3 pos)
        {
            for (int i = 0; i < m_SpawnPoints.Length; i++)
                 if (m_SpawnPoints[i].position == pos)
                    return m_SpawnPoints[i];

            return null;
        }
    }
}