using UnityEngine;
using System.Collections.Generic;
using System;

namespace Hedronoid.ObjectPool
{
    /// <summary>
    /// Generic object pool class for all your object pool needs
    /// </summary>
    public class HNDObjectPool : HNDGameObject
    {
        [SerializeField]
        private int m_InitialObjectCount = 1;
        [SerializeField]
        private int m_MaxInstanceAmount = 500;
        private int m_InstanceAmount = 0;

        [SerializeField]
        private GameObject m_ObjectPrefab;

        [SerializeField]
        private bool m_ReturnWhenFull = false;

        private bool m_HasBeenInitialized = false;
        private List<GameObject> m_AllObjects = new List<GameObject>();
        private List<GameObject> m_AvailableObjects = new List<GameObject>();
        private List<GameObject> m_RentedObjects = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        public void Initialize()
        {
            if (m_HasBeenInitialized)
                return;
            if (m_ObjectPrefab == null)
                return;

            m_AllObjects.Clear();
            m_AvailableObjects.Clear();
            m_RentedObjects.Clear();
            for (int i = 0; i < m_InitialObjectCount; i++)
            {
                AddObjectToPool();
            }

            m_HasBeenInitialized = true;
        }

        public void DestroyAllObjects()
        {
            for (int i = 0; i < m_AllObjects.Count; i++)
            {
                Destroy(m_AllObjects[i]);
            }
            m_AllObjects.Clear();
            m_AvailableObjects.Clear();
            m_RentedObjects.Clear();
        }

        public void SetObjectPrefab(GameObject objectPrefab)
        {
            m_ObjectPrefab = objectPrefab;
            DestroyAllObjects();

            m_HasBeenInitialized = false;
            Initialize();
        }

        public GameObject GetObjectPrefab()
        {
            return m_ObjectPrefab;
        }

        public void SetInitialObjectCount(int initialObjectCount)
        {
            m_InitialObjectCount = initialObjectCount;
        }

        /// <summary>
        /// Rent an object from this pool.
        /// </summary>
        /// <returns>Rented object instance.</returns>
        public GameObject RentObject()
        {
            return RentObject(Vector3.zero, Quaternion.identity, null);
        }

        /// <summary>
        /// Rent an object from this pool.
        /// </summary>
        /// <param name="position">The position that the object should be activated at. Position will be changed before activating the object, so OnEnable calls will know about the updated position.</param>
        /// <param name="rotation">The rotation that the object should be activated at. Rotation will be changed before activating the object, so OnEnable calls will know about the updated rotation.</param>
        /// <param name="parent">The parent transform that the object will be a child of. Parent will be changed before activating the object, so OnEnable calls will know about the updated parent.</param>
        /// <returns>Rented object instance.</returns>
        public GameObject RentObject(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (m_AvailableObjects.Count == 0)
            {
                AddObjectToPool();
            }

            if (m_AvailableObjects.Count > 0)
            {
                GameObject go = null;
                while (go == null)
                {
                    if (m_AvailableObjects.Count == 0)
                    {
                        // If there are no more objects, add a new one.
                        AddObjectToPool();

                        // If we failed adding an object or the added object is null, there's nothing more we can do. Return null.
                        if (m_AvailableObjects.Count == 0 || m_AvailableObjects[0] == null)
                        {
                            Debug.LogError("Failed to add new object to pool '" + cachedGameObject.name + "'! This should not happen! Return null.");
                            return null;
                        }
                    }

                    go = m_AvailableObjects[0];
                    m_AvailableObjects.RemoveAt(0);
                }
                m_RentedObjects.Add(go);
                Transform t = go.transform;
                t.position = position;
                t.rotation = rotation;
                if (parent != null)
                {
                    t.SetParent(parent);
                }
                go.SetActive(true);
                return go;
            }

            return null;
        }

        public void ReturnAll()
        {
            while (m_RentedObjects.Count > 0)
            {
                ReturnObject(m_RentedObjects[0]);
            }
        }

        public bool ReturnObject(GameObject obj)
        {
            if (obj == null)
                return true;

            if (m_RentedObjects.Contains(obj))
            {
                m_RentedObjects.Remove(obj);
                m_AvailableObjects.Add(obj);
                obj.transform.SetParent(cachedTransform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.SetActive(false);

                return true;
            }

            Debug.Log(cachedGameObject.name + " does not contain " + obj.name);
            return false;
        }

        private bool AddObjectToPool()
        {
            if (m_InstanceAmount >= m_MaxInstanceAmount)
            {
                // Reuse existing
                if (m_ReturnWhenFull && m_RentedObjects.Count > 0)
                {
                    ReturnObject(m_RentedObjects[0]);
                }

                return false;
            }

            GameObject go = GameObject.Instantiate(m_ObjectPrefab, cachedTransform);
            HNDPoolObject pgo = go.AddComponent<HNDPoolObject>();
            pgo.SetOwningPool(this);
            go.name += "_" + m_InstanceAmount.ToString();
            go.transform.SetParent(cachedTransform);
            go.SetActive(false);
            m_AllObjects.Add(go);
            m_AvailableObjects.Add(go);
            m_InstanceAmount++;

            return true;
        }

        /// <summary>
        /// Will return true if the game object is part of this pool and is currently not rented out.
        /// </summary>
        /// <param name="go">The game object to test against.</param>
        /// <returns></returns>
        public bool IsObjectAvailableInPool(GameObject go)
        {
            return m_AvailableObjects.Contains(go);
        }

        /// <summary>
        /// Will return true if the game object is part of this pool, but currently rented out.
        /// </summary>
        /// <param name="go">The game object to test against.</param>
        /// <returns></returns>
        public bool IsObjectRentedOutFromPool(GameObject go)
        {
            return m_RentedObjects.Contains(go);
        }

        public List<GameObject> RentedObjects { get { return m_RentedObjects; } }
    }
}
