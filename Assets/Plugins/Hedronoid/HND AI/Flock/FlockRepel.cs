using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class FlockRepel : HNDGameObject
    {
        [SerializeField]
        private bool m_Volumetric = false;
        [SerializeField]
        private bool m_EffectActive = true;
        [SerializeField]
        private float m_Intensity = 1f;
        [SerializeField]
        private float m_Priority = 1f;
        [SerializeField]
        private float m_Distance = 25f;
        [SerializeField]
        private float m_ScareDistance = 25f;
        [SerializeField]
        private List<string> m_FlockTypeToRepel = new List<string>();
        [SerializeField]
        private List<NPC.NPCType> m_NPCTypeToRepel = new List<NPC.NPCType>();
        [SerializeField]
        private float m_CoolDown = Mathf.Infinity;
        [SerializeField]
        private bool m_DestroyGameObject = false;

        [Header("Distance priority")]
        [SerializeField]
        private bool m_UseAnimationCurve = false;
        [SerializeField]
        private AnimationCurve m_DistancePriority;

        [Header("Distance Intensity")]
        [SerializeField]
        private bool m_UseAnimationCurveIntensity = false;
        [SerializeField]
        private AnimationCurve m_DistanceIntesity;

        private LineRenderer m_LineRenderer;
        private Collider m_Collider;

        private bool m_OwnsCollider = false;

        private float m_MasterScale = 1.0f;
        private float m_LastDistanceCheck = 0f;

        public float MasterScale
        {
            get { return m_MasterScale; }
            set
            {
                m_MasterScale = value;
                UpdateColliderRadius();
            }
        }

        protected void UpdateColliderRadius()
        {
            if (m_OwnsCollider) ((SphereCollider)m_Collider).radius = Distance;
        }

        public float Priority
        {
            get
            {
                if (UseAnimationCurve)
                {
                    return m_Priority * DistancePriority.Evaluate(m_LastDistanceCheck / m_ScareDistance);
                }
                return m_Priority;
            }
            set { m_Priority = value; }
        }

        public float Intensity
        {
            get
            {
                if (UseAnimationCurveIntensity)
                {
                    return MasterScale * m_Intensity * DistanceIntesity.Evaluate(m_LastDistanceCheck / m_ScareDistance);
                }
                return MasterScale * m_Intensity;
            }
            set { m_Intensity = value; }
        }

        public float Distance
        {
            get { return MasterScale * m_Distance; }
            set { m_Distance = value; }
        }

        public List<string> FlockTypeToRepel
        {
            get { return m_FlockTypeToRepel; }
            set { m_FlockTypeToRepel = value; }
        }

        public float CoolDown
        {
            get { return m_CoolDown; }
            set { m_CoolDown = value; }
        }

        public bool DestroyGameObject
        {
            get { return m_DestroyGameObject; }
            set { m_DestroyGameObject = value; }
        }

        public float ScareDistance
        {
            get { return MasterScale * m_ScareDistance; }
            set { m_ScareDistance = value; }
        }

        public bool EffectActive
        {
            get { return m_EffectActive; }
            set { m_EffectActive = value; }
        }

        public bool Volumetric
        {
            get { return m_Volumetric; }
            set { m_Volumetric = value; }
        }

        public LineRenderer LineRenderer
        {
            get { return m_LineRenderer; }
            set { m_LineRenderer = value; }
        }

        public List<NPC.NPCType> NPCTypeToRepel
        {
            get { return m_NPCTypeToRepel; }
            set { m_NPCTypeToRepel = value; }
        }

        public AnimationCurve DistancePriority
        {
            get
            {
                return m_DistancePriority;
            }

            set
            {
                m_DistancePriority = value;
            }
        }

        public bool UseAnimationCurve
        {
            get
            {
                return m_UseAnimationCurve;
            }

            set
            {
                m_UseAnimationCurve = value;
            }
        }

        public bool UseAnimationCurveIntensity
        {
            get
            {
                return m_UseAnimationCurveIntensity;
            }

            set
            {
                m_UseAnimationCurveIntensity = value;
            }
        }

        public AnimationCurve DistanceIntesity
        {
            get
            {
                return m_DistanceIntesity;
            }

            set
            {
                m_DistanceIntesity = value;
            }
        }

        private HashSet<NPC.NPCType> m_NPCTypeToRepelSet = new HashSet<NPC.NPCType>();


        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            m_Collider = GetComponent<Collider>();

            if (!m_Collider)
            {
                m_Collider = this.gameObject.AddComponent<SphereCollider>();
                m_Collider.isTrigger = true;
                ((SphereCollider)m_Collider).radius = Distance;
                m_OwnsCollider = true;
            }

            FlockEffectManager.Instance.RegisterFlockRepel(this);
            if (CoolDown < Mathf.Infinity)
                StartCoroutine(CoolDownCount());

            for (int i = 0; i < m_NPCTypeToRepel.Count; i++)
            {
                m_NPCTypeToRepelSet.Add(m_NPCTypeToRepel[i]);
            }
        }

        public bool IsRepellingNPCType(NPC.NPCType type)
        {
            return m_NPCTypeToRepelSet.Contains(type);
        }

        IEnumerator CoolDownCount()
        {
            yield return new WaitForSeconds(CoolDown);
            FlockEffectManager.Instance.UnRegisterFlockRepel(this);
            if (DestroyGameObject)
                Destroy(gameObject);
            else
                Destroy(this);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (LineRenderer)
            {
                Vector3[] positions = new Vector3[LineRenderer.positionCount];
                LineRenderer.GetPositions(positions);
                foreach (Vector3 point in positions)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(point, Distance);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(point, ScareDistance);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, Distance);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, ScareDistance);
            }
        }
#endif

        public float GetDistance(Vector3 location)
        {
            if (m_LineRenderer)
            {
                Vector3[] positions = new Vector3[LineRenderer.positionCount];
                LineRenderer.GetPositions(positions);
                float minDist = Mathf.Infinity;
                foreach (Vector3 point in positions)
                {
                    float dist = Vector3.Distance(point, location);
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }
                return minDist;
            }
            if (Volumetric)
            {
                if (cachedRigidbody)
                    return Vector3.Distance(cachedRigidbody.ClosestPointOnBounds(location), location);
                if (m_Collider)
                    return Vector3.Distance(m_Collider.ClosestPointOnBounds(location), location);
            }
            m_LastDistanceCheck = Vector3.Distance(transform.position, location);
            return m_LastDistanceCheck;
        }
        public Vector3 GetPoint(Vector3 location)
        {
            if (m_LineRenderer)
            {
                Vector3[] positions = new Vector3[LineRenderer.positionCount];
                Vector3 fpoint = transform.position;
                LineRenderer.GetPositions(positions);
                float minDist = Mathf.Infinity;
                foreach (Vector3 point in positions)
                {
                    float dist = Vector3.Distance(point, location);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        fpoint = point;
                    }
                }
                return fpoint;
            }
            if (Volumetric)
            {
                if (cachedRigidbody)
                    return cachedRigidbody.ClosestPointOnBounds(location);
                if (m_Collider)
                    return m_Collider.ClosestPointOnBounds(location);
            }
            return transform.position;
        }

        public Vector3 GetRepelDirection(Vector3 location)
        {
            Vector3 point = GetPoint(location);
            Vector3 direction = Vector3.zero;
            if (Vector3.Distance(location, point) <= ScareDistance)
                direction += location - GetPoint(location);

            return direction;
        }

        public void Repel(NPC.NPCType type)
        {
            if (!NPCTypeToRepel.Contains(type))
            {
                NPCTypeToRepel.Add(type);
            }
        }

        public void UnRepel(NPC.NPCType type)
        {
            if (NPCTypeToRepel.Contains(type))
            {
                NPCTypeToRepel.Remove(type);
            }
        }

        public void Kill(float time = 0f)
        {
            FlockEffectManager.Instance.UnRegisterFlockRepel(this);
            Destroy(gameObject, time);
        }
    }
}
