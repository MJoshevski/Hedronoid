using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.Events;

namespace Hedronoid.Particle
{
    public class HNDParticleSystem : HNDGameObject
    {
        private Transform m_FollowTransform;
        private bool m_IgnoreFollowRotation;
        private Quaternion m_FollowRotation;
        private Vector3 m_FollowOffset;
        private bool m_FollowPositionOnly;
        private ParticleSystem m_cachedParticleSystem;
        public ParticleSystem cachedParticleSystem
        {
            get
            {
                // Particle systems in HND are structured with an empty game object holder as the parent
                // and one child object, which contains the actual particle system.
                if (m_cachedParticleSystem == null && cachedTransform.childCount > 0)
                    m_cachedParticleSystem = cachedTransform.GetChild(0).GetComponent<ParticleSystem>();
                return m_cachedParticleSystem;
            }
        }

        bool b_HasParticleSystem;

        private Dictionary<ParticleSystem, float> m_ParticleSystemDefaultStartSizes = new Dictionary<ParticleSystem, float>();

        protected override void Awake()
        {
            base.Awake();

            // Some 'particle systems' are not actually particles. Since it's all prefab based, this will not change at runtime, so we cache whether or not an actual particle system exists here.
            b_HasParticleSystem = cachedParticleSystem != null;

            foreach (var particleSys in GetComponentsInChildren<ParticleSystem>())
            {
                m_ParticleSystemDefaultStartSizes.Add(particleSys, particleSys.GetStartSize());
            }
        }

        public void Play()
        {
            if (b_HasParticleSystem)
            {
                cachedParticleSystem.Clear();
                cachedParticleSystem.Play(true);
            }
        }

        public void Play(Vector3 position, Vector3 normal, Transform followTransform, bool ignoreFollowRotation, Vector3 scale)
        {
            m_FollowTransform = followTransform;
            m_FollowOffset = position;
            m_FollowRotation = Quaternion.FromToRotation(Vector3.forward, normal);
            m_IgnoreFollowRotation = ignoreFollowRotation;

            if (m_FollowTransform != null)
            {
                cachedTransform.position = m_FollowTransform.position /*+ (m_FollowRotation * m_FollowOffset)*/;
                if (!m_IgnoreFollowRotation)
                {
                    cachedTransform.rotation = m_FollowTransform.rotation * m_FollowRotation;
                }
            }
            else
            {
                cachedTransform.position = m_FollowOffset;
                cachedTransform.rotation = m_FollowRotation;
            }

            cachedTransform.localScale = scale;

            Play();
        }

        protected void Update()
        {
            if (m_FollowTransform != null)
            {
                cachedTransform.position = m_FollowTransform.position + (m_FollowRotation * m_FollowOffset);
                if (!m_IgnoreFollowRotation)
                {
                    cachedTransform.rotation = m_FollowTransform.rotation * m_FollowRotation;
                }
            }
        }

        public void Stop()
        {
            if (b_HasParticleSystem)
            {
                cachedParticleSystem.Stop();
            }
        }

        public void SetScale(float scale)
        {
            foreach (var particleSys in GetComponentsInChildren<ParticleSystem>())
            {
                float defaultSize = m_ParticleSystemDefaultStartSizes[particleSys];
                particleSys.SetStartSize(scale * defaultSize);
            }

            foreach (var meshRend in GetComponentsInChildren<MeshRenderer>())
            {
                meshRend.transform.localScale = Vector3.one * scale;
            }

            foreach (var lineRend in GetComponentsInChildren<LineRenderer>())
            {
                Debug.LogWarning("Scaling PRT-LineRenderer is not possible!");
                // lineRend.transform.localScale = scale;
            }
        }

        public float GetDuration()
        {
            if (cachedParticleSystem)
            {
                return cachedParticleSystem.main.duration + cachedParticleSystem.main.startLifetime.constant + cachedParticleSystem.main.startDelay.constant;
            }

            return -1;
        }

        public bool IsLooping()
        {
            if (cachedParticleSystem)
            {
                return cachedParticleSystem.IsLooping();
            }

            return true;
        }
    }
}