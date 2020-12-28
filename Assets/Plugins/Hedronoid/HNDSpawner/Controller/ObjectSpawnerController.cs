using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.ObjectPool;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// This is responsible for spawning.
    /// </summary>
    public class ObjectSpawnerController : HNDMonoBehaviour
    {
        public class ObjectSpawnedEvent : NNBaseEvent { public GameObject GameObject; }
        public class ObjectRemovedEvent : NNBaseEvent { public GameObject GameObject; }
        public class SpawnObjectEvent : NNBaseEvent { public string SpawnInfoName; public SpawnPrefabConfig PrefabConfig; internal int Amount; }

        [Tooltip("Should this object spawner use a pool?")]
        [SerializeField]
        private bool m_UsePool;
        [Tooltip("Reference to the pool manager to use. If none is set, one will be created at runtime.")]
        [SerializeField]
        private HNDPoolManager m_PoolManager;

        [SerializeField]
        private SpawnInfo m_SpawnInfo;

        /// <summary>
        /// Spawns a PoolManager if none were provided in inspector
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (m_PoolManager == null)
            {
                GameObject go = new GameObject(gameObject.name + "_PoolManager");
                m_PoolManager = go.AddComponent<HNDPoolManager>();
            }
        }

        /// <summary>
        /// Clears spawnermanagers spawns (In the case someone else has used them) and initializes them with settings.
        /// </summary>
        protected override void Start()
        {
            m_SpawnInfo.Init(transform, m_UsePool ? m_PoolManager : null);
            NNEvents.Instance.AddListener<SpawnObjectEvent>(OnSpawnObjectEvent);
        }

        /// <summary>
        /// Updates all active spawns
        /// </summary>
        private void Update()
        {
            m_SpawnInfo.Update(Time.deltaTime);
        }

        /// <summary>
        /// Listens for a SpawnObjectEvent and spawns an object in response
        /// </summary>
        /// <param name="e"></param>
        private void OnSpawnObjectEvent(SpawnObjectEvent e)
        {
            if (e.SpawnInfoName == m_SpawnInfo.name)
            {
                m_SpawnInfo.SpawnObjects(e.Amount, e.PrefabConfig);
            }
        }

        public SpawnInfo GetSpawnInfo()
        {
            return m_SpawnInfo;
        }
    }
}
