using UnityEngine;
using System.Collections.Generic;

namespace Hedronoid
{
    public class GravitySource : MonoBehaviour
    {
        [Header("Collision Layer/Tag")]
        public LayerMask triggerLayers;
        public string triggerTag;

        [Header("On Enter")]
        public List<GravitySource> enableOnEnter = new List<GravitySource>();
        public List<GravitySource> disableOnEnter = new List<GravitySource>();

        [Header("On Stay")]
        public List<GravitySource> enableOnStay = new List<GravitySource>();
        public List<GravitySource> disableOnStay = new List<GravitySource>();

        [Header("On Exit")]
        public List<GravitySource> enableOnExit = new List<GravitySource>();
        public List<GravitySource> disableOnExit = new List<GravitySource>();

        void OnEnable()
        {
            GravityService.Register(this);
        }

        void OnDisable()
        {
            GravityService.Unregister(this);
        }

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;

            EnableDisableSources(enableOnEnter, true);
            EnableDisableSources(disableOnEnter, false);
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;

            EnableDisableSources(enableOnStay, true);
            EnableDisableSources(disableOnStay, false);
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;

            EnableDisableSources(enableOnExit, true);
            EnableDisableSources(disableOnExit, false);
        }

        private void EnableDisableSources(List<GravitySource> sources, bool enable)
        {
            if (sources.Count > 0)
                foreach (GravitySource gs in sources)
                    if (gs && ((enable && !gs.enabled) || (!enable && gs.enabled)))
                        gs.enabled = enable;
        }

        private bool IsInLayerMaskOrTag(Collider other)
        {
            return (other.tag == triggerTag || 
                (triggerLayers.value & (1 << other.gameObject.layer)) > 0);
        }
    }
}