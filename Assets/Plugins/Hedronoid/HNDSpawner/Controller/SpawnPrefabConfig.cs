using System;
using UnityEngine;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// Responsible for setting up the spawned prefab with everything that needs to be configured.
    /// Spawn animation is planned but not in yet.
    /// </summary>
    [System.Serializable]
    public class SpawnPrefabConfig
    {
        [SerializeField]
        private GameObject m_Prefab;

        /// <summary>
        /// Returns the prefab given in the config.
        /// </summary>
        /// <returns></returns>
        public GameObject GetPrefab()
        {
            return m_Prefab;
        }

        /// <summary>
        /// Setup configuration tied to the prefab.
        /// </summary>
        /// <param name="go"></param>
        public void Setup(GameObject go)
        {
        }
    }
}