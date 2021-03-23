using UnityEngine;
using System.Collections.Generic;
using System;

namespace Hedronoid
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GravitySource : HNDMonoBehaviour
    {
        [Header("Collisions and overlaps")]
        public LayerMask triggerLayers;
        public int priorityWeight = 0;
        public List<GravitySource> OverlappingSources { get; private set; } = new List<GravitySource>();

        protected override void OnEnable()
        {
            base.OnEnable();

            OverlappingSources.Clear();
            OverlappingSources = GetOverlaps();
            GravityService.Register(this);
        }

        protected virtual void Update()
        {
            if (transform.hasChanged)
            {
                OverlappingSources.Clear();
                OverlappingSources = GetOverlaps();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GravityService.Unregister(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GravityService.Unregister(this);
        }

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!IsInLayerMaskOrTag(other)) return;
        }

        protected virtual List<GravitySource> GetOverlaps()
        {
            throw new NotImplementedException();
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
            return ((triggerLayers.value & (1 << other.gameObject.layer)) > 0);
        }
    }
}