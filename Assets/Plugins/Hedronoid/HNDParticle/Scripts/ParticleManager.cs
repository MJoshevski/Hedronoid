using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.Events;
using Hedronoid.ObjectPool;

namespace Hedronoid.Particle
{
    public class PlayParticleSystem : HNDBaseEvent
    {
        public ParticleManager.PlayConfig Config;
        public NapParticleSystem ParticleSystemInstance;
    }

    public class StopParticleSystem : HNDBaseEvent
    {
        public NapParticleSystem ParticleSystemInstance;
        public bool InstantKill = true;
        public float Delay = -1f;
    }

    public class StopAllParticleSystems : HNDBaseEvent
    { }

    [Serializable]
    public class ParticleManager : HNDPoolManager
    {
        public class PlayConfig
        {
            public string Name;
            public Vector3 Position = Vector3.zero;
            public Vector3 NormalVector = Vector3.up;
            public float Duration = -1f;
            public bool KeepRunning = false;
            public Transform FollowTransform = null;
            public bool FollowPositionOnly = false;
        }

        [SerializeField]
        private ParticleManagerData m_ParticleManagerData;

        [SerializeField]
        private int m_InitialCountOfEach = 4;

        protected override void Awake()
        {
            base.Awake();

            HNDEvents.Instance.AddListener<PlayParticleSystem>(OnPlayParticleSystem);
            HNDEvents.Instance.AddListener<StopParticleSystem>(OnStopParticleSystem);
            HNDEvents.Instance.AddListener<StopAllParticleSystems>(OnStopAllParticleSystems);

            D.CoreLog("Particle manager start time: " + Time.realtimeSinceStartup);
            for (int i = 0; i < m_ParticleManagerData.ParticleSystems.Count; i++)
            {
                // Don't put any objects in the pool yet. We only create them once they are needed.
                if (m_ParticleManagerData.ParticleSystems[i] != null && m_ParticleManagerData.ParticleSystems[i].ParticleSystemPrefab != null)
                    CreatePoolFromPrefab(m_ParticleManagerData.ParticleSystems[i].ParticleSystemPrefab.gameObject, m_InitialCountOfEach);
            }
            D.CoreLog("Particle manager end time: " + Time.realtimeSinceStartup);
        }

        private void OnPlayParticleSystem(PlayParticleSystem e)
        {
            NapParticleSystem prefab = GetParticleSystemPrefabFromName(e.Config.Name);
            if (prefab == null)
            {
                Debug.LogError("Did not find particle system with name '" + e.Config.Name + "'!");
                return;
            }

            // If a custom duration is set, use that, otherwise get the duration to whatever the particle system is.
            // Duration of -1 means the particle system will not be auto-returned. Looping particle systems will never be auto-returned.
            float duration = e.Config.Duration;
            if (e.Config.KeepRunning)
            {
                // If 'Keep Running' is enabled, duration is set to -1, to keep the particle system playing.
                duration = -1;
            }
            else
            {
                // If 'Keep Running' is not enabled, and duration is set to negative, we get the duration from the particle system.
                if (duration < 0)
                    duration = prefab.GetDuration();
            }

            GameObject parGO = RentObject(prefab.gameObject, e.Config.Position, Quaternion.identity, null, duration);
            if (parGO != null)
            {
                NapParticleSystem fps = parGO.GetComponent<NapParticleSystem>();
                if (fps != null)
                {
                    fps.Play(e.Config.Position, e.Config.NormalVector, e.Config.FollowTransform, e.Config.FollowPositionOnly);
                    e.ParticleSystemInstance = fps;
                }
            }
            else
            {
                Debug.LogError("Failed to start particle system! Unable to rent '" + (parGO != null ? parGO.name : "NULL") + " from particle system pool.", cachedGameObject);
            }
        }

        private void OnStopParticleSystem(StopParticleSystem e)
        {
            if (e.ParticleSystemInstance == null)
            {
                Debug.LogError("Trying to stop non-existing particle system.");
                return;
            }
            if (e.InstantKill)
            {
                e.ParticleSystemInstance.Stop();
                ReturnObject(e.ParticleSystemInstance.cachedGameObject);
                return;
            }

            if (e.Delay > 0)
            {
                StartCoroutine(ReturnObjectWithDelay(e.ParticleSystemInstance.cachedGameObject, e.ParticleSystemInstance, e.Delay));
            }
            else
            {
                StartCoroutine(ReturnObjectDelayed(e.ParticleSystemInstance.cachedGameObject, e.ParticleSystemInstance));
            }
        }

        private void OnStopAllParticleSystems(StopAllParticleSystems e)
        {
            PullBackAllRentedObjects();
        }

        IEnumerator ReturnObjectWithDelay(GameObject go, NapParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.Stop();
            ReturnObject(go);
        }

        IEnumerator ReturnObjectDelayed(GameObject go, NapParticleSystem ps)
        {
            ParticleSystem[] children = ps.gameObject.GetComponentsInChildren<ParticleSystem>();
            bool[] loops = new bool[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                var main = children[i].main;
                loops[i] = main.loop;
                main.loop = false;
            }
            while (ps.cachedParticleSystem.IsAlive(true))
                yield return null;

            for (int i = 0; i < children.Length; i++)
            {
                var main = children[i].main;
                main.loop = loops[i];
            }
            ps.Stop();
            ReturnObject(go);
        }

        private NapParticleSystem GetParticleSystemPrefabFromName(string name)
        {
            NapParticleSystemData data = m_ParticleManagerData.ParticleSystems.Find(p => p.Name == name);
            if (data != null)
            {
                return data.ParticleSystemPrefab;
            }
            return null;
        }
    }
}