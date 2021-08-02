using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Particle
{
    [Serializable]
    public class HNDParticleSystemData
    {
        public string Name;
        public int GUID;
        public HNDParticleSystem ParticleSystemPrefab;
    }

    [CreateAssetMenu(fileName = "ParticleManagerData", menuName = "Hedronoid/ScriptableObjects/ParticleManagerData", order = 1)]
    public class ParticleManagerData : ScriptableObject
    {
        [SerializeField]
        private List<HNDParticleSystemData> m_ParticleSystems = new List<HNDParticleSystemData>();

        public List<HNDParticleSystemData> ParticleSystems
        {
            get
            {
                return m_ParticleSystems;
            }
        }
    }
}