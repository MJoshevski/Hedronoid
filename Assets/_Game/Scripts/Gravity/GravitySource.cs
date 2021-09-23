using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid.Core;
using System.Linq;
using Hedronoid.AI;

namespace Hedronoid
{
    [RequireComponent(typeof(Collider))]
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

        [HideInInspector]
        [Range(1,10)]
        public int CurrentPriorityWeight = 1;
        [HideInInspector]
        public bool IsPlayerInGravity = false;

        public List<GravitySource> OverlappingSources { get; private set; } = new List<GravitySource>();

        protected Rigidbody m_Rb;
        protected Collider m_triggerCol;
        public Collider TriggerCol
        {
            get { return m_triggerCol; }
        }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            TryGetComponent(out m_triggerCol);
            if (!m_triggerCol || !m_triggerCol.isTrigger)
            {
                m_triggerCol = null;
                D.GravErrorFormat("Gravity source with name {0} has no trigger attached to it. Please attach one.", name);
            }

            OnValidate();
        }

        protected virtual void OnValidate()
        {
            if (!m_Rb) TryGetComponent(out m_Rb);               

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

            if ((other.gameObject.layer & (1 << HNDAI.Settings.PlayerLayer)) > 0)
            {
                IsPlayerInGravity = true;

                //List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
                //if (activeGravities.Count == 1)
                //{
                //    //CurrentPriorityWeight = 2;
                //    //foreach (GravitySource gs in OverlappingSources)
                //    //    gs.CurrentPriorityWeight = 1;
                //}
                //GravityService.PrioritizeActiveOverlappedGravities(other.transform.position,
                //    GameplaySceneContext.Player.movementVariables.MoveDirection,
                //    OverlappingSources);
            }
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            //if ((other.gameObject.layer & (1 << HNDAI.Settings.PlayerLayer)) > 0)
            //{
            //    GravityService.PrioritizeActiveOverlappedGravities(other.transform.position,
            //        GameplaySceneContext.Player.movementVariables.MoveDirection,
            //        OverlappingSources);
            //}
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            GravitySource grSrc = other.gameObject.GetComponent<GravitySource>();
            if (grSrc && OverlappingSources.Contains(grSrc))
                OverlappingSources.Remove(grSrc);

            if ((other.gameObject.layer & (1 << HNDAI.Settings.PlayerLayer)) > 0)
            {
                IsPlayerInGravity = false;
                ////CurrentPriorityWeight = 1;
                //GravityService.PrioritizeActiveOverlappedGravities(
                //    other.transform.position,
                //    GameplaySceneContext.Player.movementVariables.MoveDirection,
                //    OverlappingSources);
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