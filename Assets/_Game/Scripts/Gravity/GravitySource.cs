using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid.Core;

namespace Hedronoid
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GravitySource : HNDMonoBehaviour, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }

        [Header("Collisions and overlaps")]
        public LayerMask triggerLayers;

        //[HideInInspector]
        [Range(1,10)]
        public int CurrentPriorityWeight = 1;
        [HideInInspector]
        public bool IsPlayerInGravity = false;

        public List<GravitySource> OverlappingSources { get; private set; } = new List<GravitySource>();

        protected Rigidbody m_Rb;

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            OnValidate();
        }

        protected virtual void OnValidate()
        {
            if (!m_Rb)
                m_Rb = GetComponent<Rigidbody>();

            if (m_Rb && m_Rb.useGravity)
                m_Rb.useGravity = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            GravityService.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OverlappingSources.Clear();
            GravityService.Unregister(this);
        }

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            GravitySource grSrc = other.gameObject.GetComponent<GravitySource>();
            if (grSrc && !OverlappingSources.Contains(grSrc))
                    OverlappingSources.Add(grSrc);

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                IsPlayerInGravity = true;

                List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
                if (activeGravities.Count == 1)
                {
                    CurrentPriorityWeight = 2;
                    foreach (GravitySource gs in OverlappingSources)
                        gs.CurrentPriorityWeight = 1;
                }
            }
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMask(other)) return;
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            GravitySource grSrc = other.gameObject.GetComponent<GravitySource>();
            if (grSrc && OverlappingSources.Contains(grSrc))
                OverlappingSources.Remove(grSrc);

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
                IsPlayerInGravity = false;
        }

        protected void PrioritizeActiveOverlappedGravities(Vector3 position)
        {
            List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
            Dictionary<GravitySource, float> srcAngleDictionary = new Dictionary<GravitySource, float>();
            Vector3 moveDir = GameplaySceneContext.Player.movementVariables.MoveDirection;

            if (activeGravities.Count > 1 && moveDir != null && moveDir != Vector3.zero)
            {
                foreach (GravitySource gs in activeGravities)
                {
                    Vector3 playerToGravityDir = (gs.transform.position - position).normalized;
                    float angle = Mathf.Abs(Vector3.Angle(playerToGravityDir, moveDir));

                    srcAngleDictionary.Add(gs, angle);
                    //Debug.LogErrorFormat("SOURCE {0} has angle with player {1}.", gs.transform.parent.name, angle);
                }

                GravitySource[] srcs = new GravitySource[srcAngleDictionary.Count];
                srcAngleDictionary.Keys.CopyTo(srcs, 0);

                GravitySource l = srcs[0];

                for (int i = 1; i < srcs.Length; ++i)
                {
                    GravitySource r = srcs[i];

                    if (srcAngleDictionary[l] > srcAngleDictionary[r])
                        l = r;
                }

                //Debug.LogErrorFormat("MIN>>>>>SRC {0} has angle with player {1}. And this is src: {2}",
                //    l.transform.parent.name, srcAngleDictionary[l], this.transform.parent.name);

                if (this != l)
                {
                    CurrentPriorityWeight = 1;
                }
                else
                {
                    CurrentPriorityWeight = 2;
                }
            }
        }

        private void EnableDisableSources(List<GravitySource> sources, bool enable)
        {
            if (sources.Count > 0)
                foreach (GravitySource gs in sources)
                    if (gs && ((enable && !gs.enabled) || (!enable && gs.enabled)))
                        gs.enabled = enable;
        }

        private bool IsInLayerMask(Collider other)
        {
            return ((triggerLayers.value & (1 << other.gameObject.layer)) > 0);
        }
    }
}