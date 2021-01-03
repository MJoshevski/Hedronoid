using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class FlockAttract : HNDGameObject
    {
        [SerializeField]
        private bool m_Volumetric = false;

        [SerializeField]
        private bool m_EffectActive = true;
        [SerializeField]
        private bool m_StealTarget = false;
        [SerializeField]
        private float m_Intensity = 1f;
        [SerializeField]
        private float m_Priority = 1f;
        [SerializeField]
        private float m_Distance = 25f;
        [SerializeField]
        private float m_AttractDistance = 25f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_IgnoreDistanceFraction = 0f;
        [SerializeField]
        private List<string> m_FlockTypeToAttract = new List<string>();
        [SerializeField]
        private List<NPC.NPCType> m_NPCTypeToAttract = new List<NPC.NPCType>();
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


        public float Distance
        {
            get { return MasterScale * m_Distance; }
            set { m_Distance = value; }
        }

        public List<string> FlockTypeToAttract
        {
            get { return m_FlockTypeToAttract; }
            set { m_FlockTypeToAttract = value; }
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

        public float Priority
        {
            get
            {
                if (UseAnimationCurve)
                {
                    return m_Priority * DistancePriority.Evaluate(m_LastDistanceCheck / m_AttractDistance);
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
                    return MasterScale * m_Intensity * DistanceIntesity.Evaluate(m_LastDistanceCheck / m_AttractDistance);
                }
                return MasterScale * m_Intensity;
            }
            set { m_Intensity = value; }
        }

        public bool StealTarget
        {
            get { return m_StealTarget; }
            set { m_StealTarget = value; }
        }

        public float AttractDistance
        {
            get { return MasterScale * m_AttractDistance; }
            set { m_AttractDistance = value; }
        }

        public float IgnoreDistance
        {
            get { return m_IgnoreDistanceFraction * Distance; }
            set { m_IgnoreDistanceFraction = value; }
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

        public List<NPC.NPCType> NPCTypeToAttract
        {
            get { return m_NPCTypeToAttract; }
            set { m_NPCTypeToAttract = value; }
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

        private HashSet<NPC.NPCType> m_NPCTypeToAttractSet = new HashSet<NPC.NPCType>();

        protected void UpdateColliderRadius()
        {
            if (m_OwnsCollider) ((SphereCollider)m_Collider).radius = Distance;
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

            FlockEffectManager.Instance.RegisterFlockAttract(this);
            if (CoolDown < Mathf.Infinity)
                StartCoroutine(CoolDownCount());

            for (int i = 0; i < m_NPCTypeToAttract.Count; i++)
            {
                m_NPCTypeToAttractSet.Add(m_NPCTypeToAttract[i]);
            }
        }

        public bool IsAttractingNPCType(NPC.NPCType type)
        {
            return m_NPCTypeToAttractSet.Contains(type);
        }

        IEnumerator CoolDownCount()
        {
            yield return new WaitForSeconds(CoolDown);
            FlockEffectManager.Instance.UnRegisterFlockAttract(this);
            if (DestroyGameObject)
                Destroy(gameObject);
            else
                Destroy(this);
        }

        public void DestroyAttract()
        {
            FlockEffectManager.Instance.UnRegisterFlockAttract(this);
            if (DestroyGameObject)
                Destroy(gameObject);
            else
                Destroy(this);
        }

        public void Kill(float time = 0f)
        {
            FlockEffectManager.Instance.UnRegisterFlockAttract(this);
            Destroy(gameObject, time);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Distance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, AttractDistance);
        }
#endif

        public void Attract(NPC.NPCType type)
        {
            if (!NPCTypeToAttract.Contains(type))
            {
                NPCTypeToAttract.Add(type);
            }
        }

        public void Unattract(NPC.NPCType type)
        {
            if (NPCTypeToAttract.Contains(type))
            {
                NPCTypeToAttract.Remove(type);
            }
        }


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

        public Vector3 GetAttractDirection(Vector3 location)
        {
            Vector3 point = GetPoint(location);
            Vector3 direction = Vector3.zero;
            if (Vector3.Distance(location, point) <= AttractDistance)
                direction += GetPoint(location) - location;

            return direction;
        }
    }
}
