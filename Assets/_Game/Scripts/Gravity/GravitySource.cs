using UnityEngine;
using System.Collections.Generic;

namespace Hedronoid
{
    public class GravitySource : MonoBehaviour
    {
        [Header("Layer trigger actions")]
        public LayerMask triggerLayers;
        public string triggerTag;

        public List<GravitySource> enableOnEnter = new List<GravitySource>();
        public List<GravitySource> disableOnEnter = new List<GravitySource>();

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

            EnableDisableOnCollisionEnter();
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;

            EnableDisableOnCollisionExit();
        }

        public void EnableDisableOnCollisionEnter()
        {
            foreach (GravitySource gs in enableOnEnter)
                gs.enabled = true;

            foreach (GravitySource gs in disableOnEnter)
                gs.enabled = false;
        }

        public void EnableDisableOnCollisionExit()
        {
            foreach (GravitySource gs in enableOnExit)
                gs.enabled = true;

            foreach (GravitySource gs in disableOnExit)
                gs.enabled = false;
        }

        private bool IsInLayerMaskOrTag(Collider other)
        {
            return (other.tag == triggerTag || 
                (triggerLayers.value & (1 << other.gameObject.layer)) > 0);
        }
    }
}