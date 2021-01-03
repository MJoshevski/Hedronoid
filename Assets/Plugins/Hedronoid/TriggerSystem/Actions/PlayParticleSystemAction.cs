using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class PlayParticleSystemAction : HNDAction
    {
        // Used for grouping in inspector
        public static string path { get { return "Basic/"; } }

        [Header("'Play Particle System' Specific Settings")]
        [SerializeField]
        private ParticleSystem m_Target;

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            m_Target.Play();
        }

        protected override void Revert(GameObject triggeringObject)
        {
            base.Revert(triggeringObject);
            m_Target.Stop();
        }
    }
}