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
        [Tooltip("Which layers are allowed to collide with this gravity source?")]
        public LayerMask triggerLayers;
        [Tooltip("Should the boundaries control the scale/position of the trigger collider?")]
        [SerializeField]
        protected bool AutomaticColliderSize = true;
        public bool ParentToEmbededSource = false;

        [HideInInspector]
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

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
            }
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

        private float lastTimestampPrioritization = 0;
        protected void PrioritizeActiveOverlappedGravities(Vector3 position)
        {           
            List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
            Dictionary<GravitySource, float> srcAngleDictionary = new Dictionary<GravitySource, float>();

            Vector3 moveDir = Vector3.zero;

            if (GameplaySceneContext.Player)
                moveDir = GameplaySceneContext.Player.movementVariables.MoveDirection;

            if (activeGravities.Count > 1 && moveDir != null && moveDir != Vector3.zero)
            {
                // HACK (Maybe..): This prevents the update priority to execute on each frame. 
                // WHY: Jittering between gravities bug. When we are between overlapping 
                // gravities the movement direction rotates to accomodate for the new gravity. 
                // By doing so it made a small angle and overrode the priority system by reverting 
                // the gravity back to the previous one.
                // By constraining the execution step, we avoid this issue (so far).

                if (Time.realtimeSinceStartup - lastTimestampPrioritization <= 0.2f) return;
                else lastTimestampPrioritization = Time.realtimeSinceStartup;
                //

                foreach (GravitySource gs in activeGravities)
                {
                    Vector3 playerToGravityDir = (gs.transform.position - position).normalized;
                    float angle = Vector3.Angle(playerToGravityDir, moveDir);

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

                if (l is GravityPlane)
                {
                    if (this != l)
                    {
                        CurrentPriorityWeight = 2;
                    }
                    else
                    {
                        CurrentPriorityWeight = 3;
                    }
                }
            }
            else if (activeGravities.Count == 1 && activeGravities[0] == this)
            {
                CurrentPriorityWeight = 2;

                foreach (GravitySource gs in OverlappingSources)
                    gs.CurrentPriorityWeight = 1;
            }
        }

        protected void EnableDisableSources(List<GravitySource> sources, bool enable)
        {
            if (sources.Count > 0)
                foreach (GravitySource gs in sources)
                    if (gs && ((enable && !gs.enabled) || (!enable && gs.enabled)))
                        gs.enabled = enable;
        }

        protected bool IsInLayerMask(Collider other)
        {
            return ((triggerLayers.value & (1 << other.gameObject.layer)) > 0);
        }
    }
}