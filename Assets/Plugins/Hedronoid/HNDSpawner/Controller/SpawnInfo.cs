using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.ObjectPool;
using UnityEngine;

namespace Hedronoid.Spawners {
    /// <summary>
    /// Responsible for spawning. Decides when to spawn and uses SpawnPicker to decide what and where to spawn.
    /// Also contains configuration regarding Transform properties, and maximum spawned objects. 
    /// </summary>
    [System.Serializable]
    public class SpawnInfo {
        [Delayed]
        public string name = "SpawnInfo";

        [Tooltip ("A random spawn picker will be selected when spawning.")]
        [SerializeField]
        private List<SpawnPicker> m_SpawnPickers;

        [Header ("Spawn Configurations")]
        [Tooltip ("If enabled, spawned object rotation and scale will be kept as they are in the prefab")]
        [SerializeField]
        private bool m_CopyRotationFromPrefab;
        [Tooltip ("If enabled, reset velocity and angular velocity of the rigidbody")]
        [SerializeField]
        private bool m_ResetObjectsRigidbody;
        [Tooltip ("If enabled, all objects spawned will be scaled by Objects Scale vector.")]
        [SerializeField]
        private bool m_ScaleObjects;
        [Tooltip ("If Scale Objects is enabled, this is the scale that the spawned objects will be assigned.")]
        [SerializeField]
        private Vector3 m_ObjectsScale = Vector3.one;
        [Tooltip ("The number of items that the spawn managers will be initialized to handle. This only influences some spawn managers, i.e. SpawnInCircle and SpawnInLine.")]
        [SerializeField]
        private int m_SpawnersNumberOfItems;

        [Header ("Spawn limit")]
        [Tooltip ("The maximum number of active objects. If the amount of objects spawned by this controller reaches this number, it will not spawn any more objects until some of the currently active objects are destroyed.\nIf negative, there's no limit.")]
        [SerializeField]
        private int m_MaxActiveObjectsCount = 10;
        [Tooltip ("The maximum number of objects to spawn. When this many objects has been spawned from this controller, it will not spawn any more objects until it resets.\nIf negative, there's no limit.")]
        [SerializeField]
        private int m_MaxTotalObjectCount = 100;

        [Header ("Spawn initial")]
        [SerializeField]
        bool m_SpawnAtRoundStart = true;
        [Tooltip ("If Spawn At Round Start is enabled, this is the amount of objects that will be spawned by each spawn manager whenever a round starts.")]
        [SerializeField]
        private int m_SpawnAtRoundStartCount = 5;

        [Header ("Spawn Intervals")]
        [Tooltip ("Should we spawn at certain intervals?")]
        [SerializeField]
        private bool m_SpawnAtInterval = true;
        [Tooltip ("If Spawn At Interval is enabled, this is the amount of objects that will be spawned by each spawn manager at each interval.")]
        [SerializeField]
        private int m_SpawnAtIntervalCount = 5;
        [Tooltip ("If Spawn At Interval is enabled, the first interval will be this amount of seconds.")]
        [SerializeField]
        private float m_InitialInterval;
        [Tooltip ("If Spawn At Interval is enabled, every interval after the initial one will be at least this amount of seconds.")]
        [SerializeField]
        private float m_MinInterval;
        [Tooltip ("If Spawn At Interval is enabled, every interval after the initial one will be at most this amount of seconds.")]
        [SerializeField]
        private float m_MaxInterval;

        //Getters
        public bool SpawnAtInterval { get { return m_SpawnAtInterval; } }
        public int SpawnAtIntervalCount { get { return m_SpawnAtIntervalCount; } }
        public bool SpawnAtRoundStart { get { return m_SpawnAtRoundStart; } }
        public int SpawnAtRoundStartCount { get { return m_SpawnAtRoundStartCount; } }
        public float InitialInterval { get { return m_InitialInterval; } }
        public float MinInterval { get { return m_MinInterval; } }
        public float MaxInterval { get { return m_MaxInterval; } }

        private List<GameObject> m_ActiveObjects = new List<GameObject> ();
        private int m_TotalObjectsSpawned = 0;
        private float m_TimeSinceLastSpawn = 0f;
        private float m_NextSpawnTime = 0f;
        private bool m_IsSpawning = true;
        private Transform m_ObjectsParent;
        private HNDPoolManager m_PoolManager;

        /// <summary>
        /// Enables spawning, resets variables, and setups the SpawnPickers with SpawnManagers.
        /// Depending on the configuration spawns a given amount of objects on start.
        /// </summary>
        /// <param name="parent">Spawned objects will be children of this Transform</param>
        /// <param name="poolManager">If any PoolManager is provided it will be used to spawn objects.</param>
        public void Init (Transform parent, HNDPoolManager poolManager = null) {
            // Init all spawn config
            for (int i = 0; i < m_SpawnPickers.Count; i++) {
                m_SpawnPickers[i].SetAmountOfEquallyDistributedItems (m_SpawnersNumberOfItems);
            }

            m_IsSpawning = true;
            m_ObjectsParent = parent;
            m_PoolManager = poolManager;
            m_TotalObjectsSpawned = 0;
            m_NextSpawnTime = InitialInterval;

            if (m_SpawnAtRoundStart) {
                SpawnObjects (m_SpawnAtRoundStartCount);
            }
        }

        /// <summary>
        /// Destroys all spawned GameObjects and raises ObjectRemovedEvent events.
        /// </summary>
        public void DestroyAllSpawnedObjects () {
            m_TimeSinceLastSpawn = 0f;
            m_NextSpawnTime = 0f;

            for (int i = 0; i < m_ActiveObjects.Count; i++) {
                if (m_ActiveObjects[i] != null) {
                    NNEvents.Instance.Raise (new ObjectSpawnerController.ObjectRemovedEvent () { GameObject = m_ActiveObjects[i] });

                    if (m_PoolManager) {
                        m_PoolManager.ReturnObject (m_ActiveObjects[i]);
                    } else {
                        GameObject.Destroy (m_ActiveObjects[i]);
                    }
                }
            }

            m_ActiveObjects.Clear ();
        }

        /// <summary>
        /// Spawns objects within intervals
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update (float deltaTime) {
            if (m_SpawnAtInterval && m_IsSpawning) {
                m_TimeSinceLastSpawn += deltaTime;
                if (m_TimeSinceLastSpawn > m_NextSpawnTime) {
                    SpawnObjects (m_SpawnAtIntervalCount);

                    m_TimeSinceLastSpawn = 0f;
                    m_NextSpawnTime = Random.Range (m_MinInterval, m_MaxInterval);
                }
            }
        }

        public void SpawnObjects (int spawnCount, SpawnPrefabConfig prefabConfig = null) {
            var config = m_SpawnPickers[UnityEngine.Random.Range (0, m_SpawnPickers.Count)];

            if (SpawnObjects (spawnCount, config.GetRandomSpawnManager (), prefabConfig != null ? prefabConfig : config.GetSpawnGameObjectConfig ())) {
                config.Next ();
            }
        }

        private bool SpawnObjects (int spawnCount, SpawnManager spawnManager, SpawnPrefabConfig prefabConfig) {
            if (prefabConfig == null) {
                return false;
            }

            int spawned = 0;
            if (spawnManager != null) {
                for (int j = 0; j < spawnCount; j++) {
                    if (m_MaxActiveObjectsCount >= 0) {
                        // If we've reached the maximum amount of active objects, we don't allow spawning a new object.
                        if (GetActiveObjectsCount () >= m_MaxActiveObjectsCount)
                            break;
                    }
                    if (m_MaxTotalObjectCount >= 0) {
                        // If we've reached the maximum amount of total objects, we don't allow spawning a new object.
                        if (m_TotalObjectsSpawned >= m_MaxTotalObjectCount)
                            break;
                    }

                    // Get next spawn point from current spawn manager
                    NNTransformValues spawnPoint = spawnManager.GetValidSpawnPoint ();

                    // Instantiate object
                    GameObject go = null;
                    if (m_PoolManager != null) {
                        go = m_PoolManager.RentObject (prefabConfig.GetPrefab (), spawnPoint.Position, spawnPoint.Rotation, m_ObjectsParent);
                    } else {
                        go = GameObject.Instantiate (prefabConfig.GetPrefab (), spawnPoint.Position, spawnPoint.Rotation, m_ObjectsParent);
                    }
                    prefabConfig.Setup (go);

                    if (m_ScaleObjects) {
                        go.transform.localScale = m_ObjectsScale;
                    }

                    if (m_CopyRotationFromPrefab) {
                        go.transform.rotation = prefabConfig.GetPrefab ().transform.rotation;
                    }

                    if (m_ResetObjectsRigidbody) {
                        if (go.GetComponent<Rigidbody> () != null) {
                            go.GetComponent<Rigidbody> ().velocity = Vector3.zero;
                            go.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
                        }
                    }

                    m_ActiveObjects.Add (go);
                    m_TotalObjectsSpawned++;
                    spawned++;

                    NNEvents.Instance.Raise (new ObjectSpawnerController.ObjectSpawnedEvent () { sender = (spawnManager ? spawnManager.gameObject : null), GameObject = go });
                }
            }
            if (spawned == 0)
                return false;
            return true;
        }

        private int GetActiveObjectsCount () {
            for (int i = m_ActiveObjects.Count - 1; i >= 0; i--) {
                if (m_ActiveObjects[i] == null) {
                    m_ActiveObjects.RemoveAt (i);
                } else {
                    HNDPoolObject pgo = m_ActiveObjects[i].GetComponent<HNDPoolObject> ();
                    if (pgo != null && pgo.HasObjectBeenReturnedToPool ()) {
                        m_ActiveObjects.RemoveAt (i);
                    }
                }
            }

            return m_ActiveObjects.Count;
        }
    }
}