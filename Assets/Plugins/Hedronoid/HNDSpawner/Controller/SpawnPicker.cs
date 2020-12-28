using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// Responsible for picking what prefab to spawn, what SpawnManager to use, and in which patterns the prefabs are picked.
    /// Used by the ObjectSpawnerController to assist in spawning.
    /// </summary>
    [System.Serializable]
    public class SpawnPicker
    {
        [Tooltip("The spawn managers that should be used to determine the spawn position of objects. Spawn managers are chosen randomly.")]
        [SerializeField]
        private List<SpawnManager> m_SpawnManagers = new List<SpawnManager>();
        [Tooltip("The list of objects that can be spawned.")]
        [SerializeField]
        private List<SpawnPrefabConfig> m_SpawnPrefabConfigs = new List<SpawnPrefabConfig>();
        [Tooltip("Selects prefab to spawn on a pattern. The prefab is selected using a number to match the index of an prefab index, in the list m_ObjectsToSpawn. If empty will spawn objects at random.")]
        [SerializeField]
        private List<int> m_ObjectTypeSpawnPattern = new List<int>();
        private int m_CurrentPattern = 0;
        private System.Random m_Random;

        public SpawnPicker(List<SpawnManager> spawnManagers, List<SpawnPrefabConfig> objectsToSpawn)
        {
            m_Random = new System.Random();
            m_SpawnManagers = spawnManagers;
            m_SpawnPrefabConfigs = objectsToSpawn;
            m_CurrentPattern = 0;
        }

        /// <summary>
        /// Initializes all SpawnManagers that implementing ISpawnEquallyDistributed stating they support equal distribution 
        /// and then provides the amount that will be distributed.
        /// </summary>
        /// <param name="numberOfItems"></param>
        public void SetAmountOfEquallyDistributedItems(int numberOfItems)
        {
            for (int i = 0; i < m_SpawnManagers.Count; i++)
            {
                if (m_SpawnManagers[i] is ISpawnEquallyDistributed)
                {
                    ((ISpawnEquallyDistributed)m_SpawnManagers[i]).SetAmountOfEquallyDistributedItems(numberOfItems);
                }
            }
            m_CurrentPattern = 0;
        }

        /// <summary>
        /// Returns random spawn manager
        /// </summary>
        /// <returns></returns>
        public SpawnManager GetRandomSpawnManager()
        {
            return m_SpawnManagers[GetRandom().Next(m_SpawnManagers.Count)];
        }

        /// <summary>
        /// Returns the instance of the Random
        /// </summary>
        /// <returns></returns>
        private System.Random GetRandom()
        {
            if (m_Random == null)
                m_Random = new System.Random();
            return m_Random;
        }

        /// <summary>
        /// Gets an object to spawn using the pattern if any are provided
        /// </summary>
        /// <returns></returns>
        public SpawnPrefabConfig GetSpawnGameObjectConfig()
        {
            if (m_ObjectTypeSpawnPattern.Count == 0)
                return m_SpawnPrefabConfigs[GetRandom().Next(m_SpawnPrefabConfigs.Count)];
            else
            {
                int index = m_ObjectTypeSpawnPattern[m_CurrentPattern];
                if (m_SpawnPrefabConfigs.Count > index)
                {
                    return m_SpawnPrefabConfigs[index];
                }
                else
                {
                    return m_SpawnPrefabConfigs[m_Random.Next(m_SpawnPrefabConfigs.Count)];
                }
            }
        }

        /// <summary>
        /// Moves to next object in the pattern
        /// </summary>
        public void Next()
        {
            if (m_ObjectTypeSpawnPattern.Count == 0) return;
            m_CurrentPattern++;
            m_CurrentPattern %= m_ObjectTypeSpawnPattern.Count;
        }
    }
}