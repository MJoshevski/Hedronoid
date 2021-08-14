using UnityEngine;
using System.Collections.Generic;
using System;
using Hedronoid.Core;
using System.Linq;
using Hedronoid.AI;

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

                List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
                if (activeGravities.Count == 1)
                {
                    CurrentPriorityWeight = 2;
                    foreach (GravitySource gs in OverlappingSources)
                        gs.CurrentPriorityWeight = 1;
                }
                PrioritizeActiveOverlappedGravities(other.transform.position);
            }
        }

        public virtual void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            if ((other.gameObject.layer & (1 << HNDAI.Settings.PlayerLayer)) > 0)
            {
                PrioritizeActiveOverlappedGravities(other.transform.position);
            }
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
                CurrentPriorityWeight = 1;
                PrioritizeActiveOverlappedGravities(other.transform.position);
            }
        }

        private float lastTimestampPrioritization = 0;
        protected void PrioritizeActiveOverlappedGravities(Vector3 position)
        {
            Dictionary<Vector3, List<GravitySource>> filteredSrcs = FilterEmbeddedGravities();
            List<GravitySource> activeGravities = new List<GravitySource>();

            if (filteredSrcs.Count == 1)
                activeGravities = filteredSrcs.First().Value;
            else if (filteredSrcs.ContainsKey(Vector3.zero))
                activeGravities = filteredSrcs[Vector3.zero];
            else if (activeGravities == null || activeGravities.Count == 0)
            {
                Vector3 key = filteredSrcs.First().Key;
                float distance = Vector3.Distance(key, position);
                Vector3 candidate = key;

                foreach (Vector3 srcPos in filteredSrcs.Keys)
                {
                    if (srcPos == key) break;

                    float curr = Vector3.Distance(srcPos, position);

                    if (distance > curr)
                    {
                        distance = curr;
                        candidate = srcPos;
                    }
                }

                if (candidate != Vector3.zero)
                    activeGravities = filteredSrcs[candidate];
            }

            List<Vector3> activeGrvPositions = new List<Vector3>();
            Dictionary<GravitySource, float> srcAngleDictionary = new Dictionary<GravitySource, float>();
            Dictionary<GravitySource, float> srcDistanceDictionary = new Dictionary<GravitySource, float>();
            Vector3 moveDir = Vector3.zero;

            if (GameplaySceneContext.Player)
                moveDir = GameplaySceneContext.Player.movementVariables.MoveDirection;

            if (activeGravities.Count > 1)
            {
                foreach (GravitySource gs in activeGravities)
                {
                    Vector3 playerToGravityDir = (gs.transform.position - position).normalized;
                    float angle = Vector3.Angle(playerToGravityDir, moveDir);
                    float distance = Vector3.Distance(gs.transform.position, position);

                    activeGrvPositions.Add(gs.transform.position);
                    srcAngleDictionary.Add(gs, angle);
                    srcDistanceDictionary.Add(gs, distance);
                    //Debug.LogErrorFormat("SOURCE {0} has angle with player {1}.", gs.name, angle);
                }

                List<GravitySource> srcs = srcAngleDictionary.Keys.ToList();
                List<float> angleVals = srcAngleDictionary.Values.ToList();
                List<float> distanceVals = srcDistanceDictionary.Values.ToList();

                GravitySource l = srcs[0];

                if (activeGrvPositions.Any(o => o != activeGrvPositions[0]))
                {
                    // This prevents the update priority to execute on each frame. 
                    // WHY: Jittering between gravities bug. When we are between overlapping 
                    // gravities the movement direction rotates to accomodate for the new gravity. 
                    // By doing so it made a small angle and overrode the priority system by reverting 
                    // the gravity back to the previous one.
                    // By constraining the execution step (with a sweetspot of 0.2s), we avoid this issue (so far).

                    if (Time.realtimeSinceStartup - lastTimestampPrioritization <= 0.2f) return;
                    else lastTimestampPrioritization = Time.realtimeSinceStartup;
                    //
                    if (moveDir == null || moveDir == Vector3.zero) return;

                    if (angleVals.Any(o => o != angleVals[0]))
                    {
                        GravitySource r;
                        for (int i = 1; i < srcs.Count; ++i)
                        {
                            r = srcs[i];
                            if (srcAngleDictionary[l] > srcAngleDictionary[r])
                                l = r;
                        }
                    }
                    else if (distanceVals.Any(o => o != distanceVals[0]))
                    {
                        GravitySource r;
                        for (int i = 1; i < srcs.Count; ++i)
                        {
                            r = srcs[i];
                            if (srcDistanceDictionary[l] > srcDistanceDictionary[r])
                                l = r;
                        }
                    }
                }
                else
                {
                    l = PrioritizeEmbeddedGravities(position, activeGravities);
                    //Debug.LogError("NAME: " + l.name);
                }

                //Debug.LogErrorFormat("UNO:  SRC {0} has angle with player {1}. And this is src: {2}",
                //    srcs[0].name, srcAngleDictionary[srcs[0]], name);
                //Debug.LogErrorFormat("DUE:  SRC {0} has angle with player {1}. And this is src: {2}",
                //    srcs[1].name, srcAngleDictionary[srcs[1]], name);

                //Debug.LogErrorFormat("MIN>>>>>SRC {0} has angle with player {1}. And this is src: {2}",
                //    l.name, srcAngleDictionary[l], name);

                if (this != l)
                {
                    //Debug.LogErrorFormat("BEFORE!!!! NAME: {0} ,THIS IS NOT MIN. CURR GRAV W: {1}", name, CurrentPriorityWeight);

                    if (CurrentPriorityWeight > 1)
                        CurrentPriorityWeight--;
                    //Debug.LogErrorFormat("NAME: {0} ,THIS IS NOT MIN. CURR GRAV W: {1}", name, CurrentPriorityWeight);
                }
                else 
                {
                    CurrentPriorityWeight = activeGravities.Count + 1;
                    //Debug.LogErrorFormat("NAME: {0} ,THIS IS IT!!!. CURR GRAV W: {1}", l.name, l.CurrentPriorityWeight);
                }
            }
            else if (activeGravities.Count == 1 && activeGravities[0] == this)
            {
                CurrentPriorityWeight = 2;

                foreach (GravitySource gs in OverlappingSources)
                    gs.CurrentPriorityWeight = 1;
            }
        }

        protected GravitySource PrioritizeEmbeddedGravities(Vector3 position, List<GravitySource> sources)
        {
            List<float> colliderSizes = new List<float>();

            foreach (GravitySource src in sources)
            {
                if (src.TriggerCol && src.TriggerCol.isTrigger)
                {
                    //Debug.LogErrorFormat("SRC NAME: {0}, BOUNDS: {1}", src.name, src.TriggerCol.bounds.size.sqrMagnitude);
                    colliderSizes.Add(src.TriggerCol.bounds.size.sqrMagnitude);
                }
                else D.GravError("One of the embedded gravities doesn't have a trigger collider attached.");
            }

            float smallest = colliderSizes[0];

            for (int i = 1; i < colliderSizes.Count; ++i)
                if (smallest > colliderSizes[i])
                {
                    smallest = colliderSizes[i];
                }

            return sources[colliderSizes.FindIndex(o => o == smallest)];
        }

        protected Dictionary<Vector3, List<GravitySource>> FilterEmbeddedGravities()
        {
            List<GravitySource> activeGravities = GravityService.GetActiveGravitySources();
            Dictionary<Vector3, List<GravitySource>> dict = new Dictionary<Vector3, List<GravitySource>>();
            Dictionary<Vector3, List<GravitySource>> sortedDict = new Dictionary<Vector3, List<GravitySource>>();

            for (int i = 0; i < activeGravities.Count; i++)
            {
                List<GravitySource> dummyList = new List<GravitySource>();

                for (int j = 0; j < activeGravities.Count; j++)
                {
                    if (activeGravities[i].transform.position == 
                        activeGravities[j].transform.position)
                        dummyList.Add(activeGravities[j]);
                }

                if (!dummyList.Contains(activeGravities[i]))
                    dummyList.Add(activeGravities[i]);

                if (!dict.ContainsKey(activeGravities[i].transform.position))
                    dict.Add(activeGravities[i].transform.position, dummyList);
            }

            //Sort out between embedded and non-embedded
            List<GravitySource> nonEmd = new List<GravitySource>();
            foreach (var item in dict)
            {
                if (item.Value.Count > 1 && !sortedDict.Contains(item))
                    sortedDict.Add(item.Key, item.Value);
                else nonEmd.Add(item.Value[0]);
            }

            if (nonEmd.Count > 0)
                sortedDict.Add(Vector3.zero, nonEmd);

            return sortedDict;
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