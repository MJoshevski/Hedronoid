using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.ObjectPool;

namespace Hedronoid.Weapons
{
    public class GetBulletToFire : HNDBaseEvent
    {
        public BulletPoolManager.BulletConfig Config;
        public GameObject BulletInstance;
    }

    public class DestroyFiredBullet : HNDBaseEvent
    {
        public GameObject BulletInstance;
        public bool InstantKill = true;
        public float Delay = -1f;
    }

    public class DestroyAllFiredBullets : HNDBaseEvent
    { }

    public class BulletPoolManager : HNDPoolManager
    {
        public class BulletConfig
        {
            public GameObject Prefab;
            public Vector3 Position = Vector3.zero;
            public Quaternion Rotation = Quaternion.identity;
            public Transform Parent = null;
            public Vector3 NormalVector = Vector3.up;
            public float Duration = -1f;
        }

        [SerializeField]
        private List<GameObject> bulletPrefabs = new List<GameObject>();

        [SerializeField]
        private int m_InitialCountOfEach = 40;

        protected override void Awake()
        {
            base.Awake();

            HNDEvents.Instance.AddListener<GetBulletToFire>(OnGetBulletToFire);
            HNDEvents.Instance.AddListener<DestroyFiredBullet>(OnDestroyFiredBullet);
            HNDEvents.Instance.AddListener<DestroyAllFiredBullets>(OnDestroyAllFiredBullets);

            D.CoreLog("Bullet pool manager start time: " + Time.realtimeSinceStartup);
            for (int i = 0; i < bulletPrefabs.Count; i++)
            {
                // Don't put any objects in the pool yet. We only create them once they are needed.
                if (bulletPrefabs[i] != null)
                    CreatePoolFromPrefab(bulletPrefabs[i], m_InitialCountOfEach);
            }
            D.CoreLog("Bullet pool manager end time: " + Time.realtimeSinceStartup);
        }

        public GameObject GetBulletToFire(BulletPoolManager.BulletConfig Config)
        {
            if (Config.Prefab == null)
            {
                D.CoreError("Did not find bullet with name '" + Config.Prefab.name + "'!");
                return null;
            }
            GameObject bulletGO =
                RentObject(Config.Prefab, Config.Position, Config.Rotation, Config.Parent, Config.Duration);

            if (bulletGO != null)
                return bulletGO;
            else
                D.CoreError("Failed to start bullet system! Unable to rent '" + (bulletGO != null ? bulletGO.name : "NULL") + " from bullet pool.", cachedGameObject);

            return null;
        }

        private void OnGetBulletToFire(GetBulletToFire e)
        {
            if (e.Config.Prefab == null)
            {
                D.CoreError("Did not find bullet with name '" + e.Config.Prefab.name + "'!");
                return;
            }
            GameObject bulletGO = 
                RentObject(e.Config.Prefab, e.Config.Position, e.Config.Rotation, e.Config.Parent, e.Config.Duration);

            if (bulletGO != null)
                e.BulletInstance = bulletGO;
            else
                D.CoreError("Failed to start bullet system! Unable to rent '" + (bulletGO != null ? bulletGO.name : "NULL") + " from bullet pool.", cachedGameObject);
        }

        private void OnDestroyFiredBullet(DestroyFiredBullet e)
        {
            if (e.BulletInstance == null)
            {
                D.CoreError("Trying to destroy non-existing bullet.");
                return;
            }

            if (e.InstantKill)
            {
                ReturnObject(e.BulletInstance);
                return;
            }

            if (e.Delay > 0)
            {
                StartCoroutine(ReturnObjectWithDelay(e.BulletInstance, e.Delay));
            }
        }

        private void OnDestroyAllFiredBullets(DestroyAllFiredBullets e)
        {
            PullBackAllRentedObjects();
        }

        IEnumerator ReturnObjectWithDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnObject(go);
        }
    }
}
