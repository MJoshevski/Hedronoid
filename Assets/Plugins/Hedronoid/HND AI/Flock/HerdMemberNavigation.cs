using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using System.Linq;
using Hedronoid.Events;
using Hedronoid.HNDFSM;
using Hedronoid.Objects;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{

    public class HerdMemberNavigation : AIBaseNavigation
    {
        public enum EHerdStates
        {
            Disperse = EStates.Highest + 1,   
        }

        protected bool m_CanThink = false;
        protected bool m_SubscribeInitialToGameObject = true;
        [Header("Herd Settings")]
        [SerializeField]
        protected bool m_Wander = false;
        [SerializeField]
        protected float m_WanderChangeTime = 10f;
        [SerializeField]
        protected NPC.NPCType m_Type = NPC.NPCType.Rollandian;
        //protected HerdManager m_Flock;
        protected Vector3 m_PreviousDirection = Vector3.zero;
        protected float m_PreviousDirectionTimeStamp = 0f;
        protected float m_DirectionTimeStamp = 0f;
        [SerializeField]
        protected bool m_NormalizeFlockDirections = false;

        [Header("Neighbour Settings")]
        [SerializeField]
        protected float m_FlockCoherence = 1f;
        [SerializeField]
        protected float m_NeighbourDistance = 10f;
        [SerializeField]
        protected float m_NeighbourAttractStrength = 2f;
        [SerializeField]
        protected float m_NeighbourAvoidDistance = 1f;
        [SerializeField]
        protected float m_NeighbourAvoidStrength = 2f;

        [Header("Speed settings for navigation")]
        [SerializeField]
        protected float m_Speed = 0f;
        [SerializeField]
        protected float m_LastSpeed = 0f;
        [SerializeField]
        protected float m_DefaultMinSpeed = 10f;
        [SerializeField]
        protected float m_DefaultMaxSpeed = 40f;

        [Header("Fears")]
        [SerializeField]
        protected float m_TotalPanicThreshold = 5.5f;
        [SerializeField]
        protected float m_FearThreshold = 4f;
        [SerializeField]
        protected float m_MaxPanicIntensity = 6f;

        [Header("Loves")]
        [SerializeField]
        protected float m_MaxHappinessIntensity = 6;
        protected float m_TargetPriority = 0f;

        [Header("Debug")]
        [SerializeField]
        protected float m_PanicIntensity = 0f;
        [SerializeField]
        protected List<FlockRepel> m_Fears = new List<FlockRepel>();
        [SerializeField]
        protected float m_HappinessIntensity = 0f;
        [SerializeField]
        protected List<FlockAttract> m_Loves = new List<FlockAttract>();
        [SerializeField]
        protected Vector3 m_LastDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_RepelDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_AvoidDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_AttractDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_GroupCenterDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_FlockCenterDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_AttractRepelDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_TowardsAwayDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_TotalDirection = Vector3.zero;
        [SerializeField]
        protected Vector3 m_CenterDirection = Vector3.zero;
        protected float m_PanicPriority = 0f;
        protected float m_HappinessPriority = 0f;

        [SerializeField]
        protected int m_GroupSize = 0;
        protected NPC.NPCType m_FriendlyType;

        [SerializeField]
        private ParticleSystem m_EnterBuildingParticlePrefab;

        protected bool m_Active = true;
        protected Vector3 m_CachedPosition;

        private int m_OverlapCount;

        private const int MaxColliders = 60;
        private Collider[] m_Colliders = new Collider[MaxColliders];

        [Header("Wall Collision Deflection")]
        [SerializeField]
        [Tooltip("The minimum Y component of a surface normal for the surface to be considered a wall for deflection.")]
        private float m_collDeflectionVerticalThreshold = 0.5f;
        [SerializeField]
        [Tooltip("After collision with a wall, the AI will be repulsed by the collision point while still within this radius from the point.")]
        private float m_collisionDeflectionPersistenceRadius = 4.0f;

        private Vector3 m_CollDeflectionDirSum = Vector3.zero;
        private Vector3 m_CollDeflectionDirection = Vector3.zero;
        private Vector3 m_CollDeflectionPoint = Vector3.zero;
       
        private float m_collisionDeflectionPersistenceRadiusSq = 0.0f;

        private bool m_hasCollisionDeflection = false;
        private int m_NoCollisionDeflectLayerMask;

        private int m_numCollDeflectionContacts = 0;


        private Camera[] m_playerCameras = new Camera[2]{ null, null};
        private Transform[] m_playerTx = new Transform[2]{ null, null};
        [SerializeField]
        private bool m_isVisible = true;

        private float m_LastJump;


        public float Speed {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public Vector3 LastDirection
        {
            get { return m_LastDirection; }
            set { m_LastDirection = value; }
        }

        public float LastSpeed {
            get { return m_LastSpeed; }
            set { m_LastSpeed = value; }
        }

        public float FlockCoherence
        {
            get { return m_FlockCoherence; }
            set { m_FlockCoherence = value; }
        }

        public NPC.NPCType Type {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public bool Active {
            get { return m_Active; }
            set { m_Active = value; }
        }

        public Vector3 CachedPosition {
            get { return m_CachedPosition; }
            set { m_CachedPosition = value; }
        }

        public bool CanThink {
            get {  return m_CanThink; }
            set { m_CanThink = value; }
        }

        protected override void Awake()
        {
            base.Awake();

            CreateState(EHerdStates.Disperse, OnUpdateDisperse, OnEnterDisperse, OnExitDisperse);

            HNDEvents.Instance.AddListener<FlockDisperseEvent>(FlockDisperseEvent);
            HNDEvents.Instance.AddListener<FlockRepeltUnsubscribeEvent>(FlockRepeltUnsubscribeEvent);
            HNDEvents.Instance.AddListener<FlockAttractUnsubscribeEvent>(FlockAttractUnsubscribeEvent);
            HNDEvents.Instance.AddListener<FlockAttractUnsubscribeTypeEvent>(FlockAttractUnsubscribeTypeEvent);
            HNDEvents.Instance.AddListener<FlockRepeltUnsubscribeTypeEvent>(FlockRepeltUnsubscribeTypeEvent);
            
            SetFriendlyType();
            
            m_NoCollisionDeflectLayerMask = LayerMask.GetMask("NPCs", "Enemies", "Players", "Tether");


            m_collisionDeflectionPersistenceRadiusSq = m_collisionDeflectionPersistenceRadius * m_collisionDeflectionPersistenceRadius;
        }

        protected override void Start()
        {
            base.Start();
            FlockEffectManager.Instance.RegisterMember(this);

            ChangeState(EStates.DefaultMovement);

            m_LastJump = Time.time;
        }

        private void SetFriendlyType()
        {
            switch (Type)
            {
                case NPC.NPCType.Firellandian:
                    m_FriendlyType = NPC.NPCType.Rollandian;
                    break;
                case NPC.NPCType.Rollandian:
                    m_FriendlyType = NPC.NPCType.Firellandian;
                    break;
                case NPC.NPCType.Zombie:
                    m_FriendlyType = NPC.NPCType.Firembie;
                    break;
                case NPC.NPCType.Firembie:
                    m_FriendlyType = NPC.NPCType.Zombie;
                    break;
            }
        }
        void FlockRepeltUnsubscribeTypeEvent(FlockRepeltUnsubscribeTypeEvent e)
        {
            if (e.FlockType != m_Type) return;
            if (m_Fears.Contains(e.Repel))
                m_Fears.Remove(e.Repel);
        }

        void FlockAttractUnsubscribeTypeEvent(FlockAttractUnsubscribeTypeEvent e)
        {
            if (e.FlockType != m_Type) return;
            if (m_Loves.Contains(e.Attract))
                m_Loves.Remove(e.Attract);
        }

        void FlockRepeltUnsubscribeEvent(FlockRepeltUnsubscribeEvent e)
        {
            if (m_Fears.Contains(e.Repel))
                m_Fears.Remove(e.Repel);
        }

        void FlockAttractUnsubscribeEvent(FlockAttractUnsubscribeEvent e)
        {
            if (m_Loves.Contains(e.Attract))
                m_Loves.Remove(e.Attract);
        }

        protected void FlockDisperseEvent(FlockDisperseEvent e)
        {
            if (e.FlockType != Type) return;
            
            if (IsInState(EHerdStates.Disperse))
            {
                return;
            }

            if (Vector3.Distance(transform.position, e.Target.position) > e.Range) return;
            m_Target = e.Target;
            Speed = Speed < m_DefaultMinSpeed || Speed > m_DefaultMaxSpeed ? UnityEngine.Random.Range(m_DefaultMinSpeed, m_DefaultMaxSpeed) : Speed;
            Speed *= e.Panic;

            ChangeState(EHerdStates.Disperse);
            
            StartCoroutine(RecoverFromDisperse(e.Time));
        }

        IEnumerator RecoverFromDisperse(float time)
        {
            yield return new WaitForSeconds(time);
            ChangeState(EStates.DefaultMovement);
            yield return null;
        }

        IEnumerator DelayedDisperse()
        {
            yield return null;
        }

        protected virtual void OnEnterDisperse(FSMState fromState)
        {
            StartCoroutine(DelayedDisperse());
        }

        protected virtual void OnExitDisperse(FSMState toState)
        {

        }

        protected virtual void OnUpdateDisperse()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetDirection()
        {
            Vector3 direction = Vector3.zero;
            CachedPosition = transform.position;

            if (IsInState(EStates.GoToTarget))
            {
                direction = Target.position - transform.position;
                direction.Normalize();
            }
            else if (IsInState(EStates.ReturnToDefault))
            {
                direction.Normalize();
            }
            else if (IsInState(EStates.DefaultMovement))
            {
                direction = ApplyRules();
                direction.Normalize();
            }
            else if (IsInState(EStates.FleeFromTarget))
            {
                direction = ApplyRules();
                if (m_GroupSize < 0)
                {
                    direction = transform.position - Target.position;
                }
                direction.Normalize();
            }
            else if (IsInState(EHerdStates.Disperse))
            {
                direction = transform.position - Target.position;
                direction.Normalize();
            }
            m_DirectionTimeStamp = Time.time;
            if (m_Wander && m_GroupSize <= 0 && m_Fears.Count <= 0 && m_Loves.Count <= 0)
            {
                if (m_PreviousDirection.magnitude > 0.01f && m_PreviousDirectionTimeStamp + m_WanderChangeTime > Time.time)
                {
                    direction = m_PreviousDirection;
                    m_DirectionTimeStamp = m_PreviousDirectionTimeStamp;
                }
                else
                    direction = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
                Speed = m_DefaultMinSpeed;
            }

            //Debug.DrawRay(transform.position, direction);

            m_PreviousDirection = direction;
            m_PreviousDirectionTimeStamp = m_DirectionTimeStamp;
            if (m_hasCollisionDeflection)
            {
                //Debug.DrawRay(m_CollDeflectionPoint, m_CollDeflectionDirection);
                direction += m_CollDeflectionDirection;
                direction.Normalize();
            }

            return direction;
        }        

        protected virtual Vector3 ApplyRules()
        {
            Vector3 direction = Vector3.zero;
            Vector3 directionAwayFromDanger = Vector3.zero;
            Vector3 directionTowardsInterest = Vector3.zero;
            Vector3 directionInterestDanger = Vector3.zero;
            Vector3 directionTowardsAwayFriends = Vector3.zero;

            if( !m_isVisible )
              return direction;

            m_OverlapCount = Physics.OverlapSphereNonAlloc(this.transform.position, 25f, m_Colliders, LayerMask.GetMask("NPCs", "Enemies", "Players", "FlockAffectors"), QueryTriggerInteraction.Collide);            

            directionAwayFromDanger = GetDirectionAwayFromDanger();
            directionTowardsInterest = GetDirectionTowardsInterest();

            directionInterestDanger = (directionTowardsInterest * m_HappinessPriority + directionAwayFromDanger * m_PanicPriority).normalized * (directionTowardsInterest.magnitude + directionAwayFromDanger.magnitude) / 2f;
            directionTowardsAwayFriends = GetDirectionTowardsAwayFriends();

            m_RepelDirection = directionAwayFromDanger;
            m_AttractDirection = directionTowardsInterest;
            m_TotalDirection = directionTowardsAwayFriends;
            m_TowardsAwayDirection = directionTowardsAwayFriends;
            m_AttractRepelDirection = directionInterestDanger;

            direction = (directionInterestDanger + directionTowardsAwayFriends * m_FlockCoherence).normalized;
            m_TotalDirection = direction;
            DoInteractions();

            return direction;
        }

        public Vector3 GetDirectionTowardsAwayFriends()
        {
            float speed = UnityEngine.Random.Range(m_DefaultMinSpeed, m_DefaultMaxSpeed);
            speed *= Mathf.Max(m_HappinessIntensity, m_PanicIntensity);
            float dist = 0f;
            float gspeed = 0f;
            Vector3 direction = Vector3.zero;
            Vector3 vcentre = Vector3.zero;
            Vector3 vavoid = Vector3.zero;
            Vector3 avgdirection = Vector3.zero;
            m_GroupSize = 0;

            
            for (int i = 0; i < m_OverlapCount; i++)
            {
                var member = FlockEffectManager.Instance.GetMember(m_Colliders[i]);
                if (member && (member.Type == Type || member.Type == m_FriendlyType) && member != this)
                { 
                    member.CachedPosition = member.transform.position;
                    dist = Vector3.Distance(member.CachedPosition, CachedPosition);
                    
                    if( dist < m_NeighbourDistance )
                    {
                      vcentre += member.CachedPosition;
                      avgdirection += member.LastDirection;
                      gspeed = member.LastSpeed;
                      m_GroupSize++;
                    }
            
                    if ( dist <= m_NeighbourAvoidDistance)
                    {
                        vavoid += CachedPosition - member.CachedPosition;
                    }
                }
            }
            
            vcentre = m_GroupSize > 0f ? vcentre / m_GroupSize : CachedPosition;
            vcentre = vcentre - CachedPosition;
            vcentre = (m_NormalizeFlockDirections ? vcentre.normalized : vcentre) ;
            m_CenterDirection = vcentre;
            vavoid = (m_NormalizeFlockDirections ? vavoid.normalized : vavoid) ;
            m_AvoidDirection = vavoid;
            avgdirection = (m_NormalizeFlockDirections ? avgdirection.normalized : avgdirection);
            m_FlockCenterDirection = avgdirection;

            direction = (vcentre * m_NeighbourAttractStrength + vavoid * m_NeighbourAvoidStrength  + avgdirection).normalized;
            direction *= (vcentre.magnitude + vavoid.magnitude + avgdirection.magnitude) / 3f;
                
            gspeed /= m_GroupSize > 0f ? m_GroupSize : 1f;
            speed = (speed + gspeed) / 2f;

            Speed = speed;

            return direction;
        }
        

        public Vector3 GetDirectionTowardsInterest()
        {
            Vector3 direction = Vector3.zero;
            List<FlockAttract> LovesToRemove = new List<FlockAttract>();
            m_HappinessIntensity = 1f;
            m_HappinessPriority = 0f;

            for (int i = 0; i < m_OverlapCount; i++)
            {
                var attract = FlockEffectManager.Instance.GetAttract(m_Colliders[i]);
                
                if (attract && attract.IsAttractingNPCType(Type))
                {   
                  if (attract.EffectActive && attract.GetDistance(CachedPosition) <= attract.Distance && attract.GetDistance(CachedPosition) > attract.IgnoreDistance)
                  {
                      if (!m_Loves.Contains(attract)) m_Loves.Add(attract);
                  }
                }
            }

            float avgMagnitude = 0f;
            float gSize = 0f;
            float totalPriority = 0f;
            foreach (FlockAttract at in m_Loves)
            {
                if (!at) continue;
                Vector3 atDirection = at.GetAttractDirection(CachedPosition);
                float dist = at.GetDistance(CachedPosition);
                if (at && atDirection.magnitude > 0f && at.EffectActive && at.gameObject.activeSelf && dist > at.IgnoreDistance)
                {
                    avgMagnitude += atDirection.magnitude;
                    direction += atDirection.normalized * at.Priority;
                    totalPriority += at.Priority;
                    gSize++;
                    if (at.Intensity > m_HappinessIntensity) m_HappinessIntensity = at.Intensity;
                    if (at.Priority > m_HappinessPriority) m_HappinessPriority = at.Priority;

#if UNITY_EDITOR
                    //if (UnityEditor.Selection.activeGameObject == this.gameObject) Debug.Log("atDirection.magnitude: " + atDirection.magnitude + ", at.IgnoreDistance: " + at.IgnoreDistance + ", avgMagnitude: " + avgMagnitude);
#endif
                }
                else
                {
                    if (m_Loves.Contains(at)) LovesToRemove.Add(at);
                }
            }
            if (gSize > 0f)
            {
                avgMagnitude /= gSize;
                direction.Normalize();
                direction *= avgMagnitude;
            }
            foreach (FlockAttract fa in LovesToRemove)
                m_Loves.Remove(fa);
            if (m_HappinessIntensity > m_MaxHappinessIntensity) m_HappinessIntensity = m_MaxHappinessIntensity;
            return (m_NormalizeFlockDirections ? direction.normalized : direction);
        }

        public void DoInteractions()
        {
            foreach (BaseInteractable at in FlockEffectManager.Instance.GetInteract(Type))
            {
                if (at && Vector3.Distance(CachedPosition, at.transform.position) <= at.InteractDistance && at.Interactable)
                {
                    at.Interact(gameObject);
                    if (m_LastJump + 0.8f < Time.time)
                    {
                        cachedRigidbody.velocity = Vector3.up;
                        cachedRigidbody.angularVelocity = Vector3.zero;
                        Invoke("PushDown", 0.4f);
                        m_LastJump = Time.time;
                    }
                }
            }
        }

        void PushDown()
        {
            cachedRigidbody.velocity = Vector3.down;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }


        public Vector3 GetDirectionAwayFromDanger()
        {
            Vector3 direction = Vector3.zero;
            m_PanicIntensity = 1f;
            float gSize = 0f;
            float totalPriority = 0f;
            float avgMagnitude = 0f;
      
            for (int i = 0; i < m_OverlapCount; i++)
            {
                var repel = FlockEffectManager.Instance.GetRepel(m_Colliders[i]);
                if (repel != null && repel.IsRepellingNPCType(Type) && repel.EffectActive && repel.GetDistance(CachedPosition) <= repel.Distance )
                {
                    if (!m_Fears.Contains(repel)) m_Fears.Add(repel);
                }
            }
         
            List<FlockRepel> FearsToRemove = new List<FlockRepel>();
            foreach (FlockRepel rp in m_Fears)
            {
                if (!rp) continue;
                Vector3 rpDirection = rp.GetRepelDirection(CachedPosition);
                if (rpDirection.magnitude > 0f && rp.EffectActive && rp.gameObject.activeSelf)
                {
                    avgMagnitude += rpDirection.magnitude;
                    direction += rpDirection.normalized * rp.Priority;
                    totalPriority += rp.Priority;
                    gSize++;
                    if (rp.Intensity > m_PanicIntensity)
                        m_PanicIntensity = rp.Intensity;
                    if (rp.Priority > m_PanicPriority)
                        m_PanicPriority = rp.Priority;
                }
                else if (m_Fears.Contains(rp))
                    FearsToRemove.Add(rp);
            }
            if (gSize > 0f)
            {
                avgMagnitude /= gSize;
                direction.Normalize();
                direction *= avgMagnitude;
            }
            foreach (FlockRepel fr in FearsToRemove)
            {
                m_Fears.Remove(fr);
            }

            m_PanicIntensity = (m_PanicIntensity > m_MaxPanicIntensity ? m_MaxPanicIntensity : m_PanicIntensity);
            return (m_NormalizeFlockDirections ? direction.normalized : direction);
        }
        
        public void DestroyHerdMember()
        {

            FlockEffectManager.Instance.UnRegisterMember(this);
            Destroy(gameObject);
        }

        public void EnterBuilding()
        {
            // SoundRouter.CutsceneAudioFilter("Statue_Suck");

            var particle = Instantiate(m_EnterBuildingParticlePrefab, this.transform.position, Quaternion.identity);
            Destroy(particle, 10f);
            DestroyHerdMember();
        }

        public void MoveTowards(Transform target)
        {
            SetTarget(target);
            ChangeState(EStates.GoToTarget);
        }

        public void MoveTowards(Transform target, float speed)
        {
            MoveTowards(target);
            Speed = speed;
        }

        public void ChangeType(NPC.NPCType type, float coherence)
        {
            Type = type;
            SetFriendlyType();
            m_FlockCoherence = coherence;
            m_Loves.Clear();
            m_Fears.Clear();
        }

        public void ActivateHerd(bool Object = true)
        {
            gameObject.SetActive(Object);
            Active = true;
        }

        public void DeactivateHerd(bool Object = false)
        {
            Active = false;
            gameObject.SetActive(Object);
        }

        T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            System.Reflection.FieldInfo[] fields = type.GetFields(flags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        

        protected override void Update()
        {
            base.Update();
            
            // //Get player cameras
            // if(  m_playerCameras[0] == null || m_playerCameras[1])
            // {
            //   RollPlayingGame.Characters.CharacterBase player = PlayerManager.Instance.GetPlayer(0);
                
            //   if( player )
            //   {
            //     m_playerTx[0]  = player.transform;
            //     m_playerCameras[0] = player.PlayerCamera.Camera;

            //     player = PlayerManager.Instance.GetPlayer(1);
            //     m_playerTx[1]  = player.transform;
            //     m_playerCameras[1] = player.PlayerCamera.Camera;              
            //   }              
            // }
            // else
            // {
            //   bool visible = Math.Min( Vector3.Distance( m_playerTx[0].position,transform.position),Vector3.Distance( m_playerTx[1].position,transform.position) ) < 30.0f || 
            //                  Vector3.Magnitude( cachedRigidbody.velocity ) > 1.0f; 
            //   if( !visible )
            //   {
            //     //If it's not in range, check if a camera is seeing it
            //     Vector3 vp0 = m_playerCameras[0].WorldToViewportPoint(transform.position);
            //     Vector3 vp1 = m_playerCameras[1].WorldToViewportPoint(transform.position);
            //     visible = ( ( vp0.x > 0.0 && vp0.x < 1.0 && vp0.y > 0.0 && vp0.y < 1.0 && vp0.z > 0.0 && vp0.z < 100.0f ) || 
            //                 ( vp1.x > 0.0 && vp1.x < 1.0 && vp1.y > 0.0 && vp1.y < 1.0  && vp1.z > 0.0 && vp1.z < 100.0f  ) );               
            //   }
                    
            //   if( visible && !m_isVisible)
            //   {
            //         //If herd became visible enable physics.
            //         cachedRigidbody.isKinematic = false;
            //         cachedRigidbody.detectCollisions = true;
            //         cachedRigidbody.WakeUp();
            //         ActivateHerd(gameObject.activeSelf);
                  
            //   }
            //   else if(!visible && m_isVisible)
            //   {
            //         //If herd became invisible disable physics
            //         cachedRigidbody.isKinematic = true;
            //         cachedRigidbody.detectCollisions = false;
            //         cachedRigidbody.Sleep();
            //         DeactivateHerd(gameObject.activeSelf);
            //   }

            //   m_isVisible = visible;
            // }
            
            if (m_numCollDeflectionContacts > 0)
            {
                UpdateCollisionDeflectionDirection();
            }
        }

        protected override void FixedUpdate()
        {
            if (m_numCollDeflectionContacts == 0 && m_hasCollisionDeflection)
            {
                if ((cachedTransform.position - m_CollDeflectionPoint).sqrMagnitude > m_collisionDeflectionPersistenceRadiusSq)
                {
                    m_hasCollisionDeflection = false;
                }
            }

            ClearDeflectionCollisionContacts();   
        }

        private void ClearDeflectionCollisionContacts()
        {
            m_numCollDeflectionContacts = 0;
            m_CollDeflectionDirSum = Vector3.zero;
        }

        private void UpdateCollisionDeflectionDirection()
        {
            if (m_numCollDeflectionContacts < 1) return;
            m_hasCollisionDeflection = true;
            m_CollDeflectionDirection = (m_CollDeflectionDirSum / (float)m_numCollDeflectionContacts).normalized;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (((1<<collision.collider.gameObject.layer) & m_NoCollisionDeflectLayerMask) != 0) return;
            if (collision.contacts[0].normal.y < m_collDeflectionVerticalThreshold)
            {
                m_numCollDeflectionContacts++;
                m_CollDeflectionDirSum += collision.contacts[0].normal;
                m_CollDeflectionPoint = cachedTransform.position;
            }
            //Debug.Log("CONTACT #: " + collision.contacts.Length);
        }
    }
}