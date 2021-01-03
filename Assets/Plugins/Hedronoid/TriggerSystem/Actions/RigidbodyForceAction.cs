using UnityEngine;
using System.Collections;

namespace Hedronoid.TriggerSystem
{
    public class RigidbodyForceAction : HNDAction
    {
        public enum eRigidBodyForceActionDirectionType
        {
            StaticDirection,
            OppositeVelocity,
            InHitDirection,
            InDirectionOfObject,
            InVelocityDirection
        }

        // Used for grouping in inspector
        public static string path { get { return "Rigidbody/"; } }

        // NOTE:
        // We're hiding most of the parameters in this script, because it has a custom inspector that draws these things in a more understandable manner.
        [Header("'Rigidbody Force' Specific Settings")]
        [Tooltip("If true, force will be applied to the object that triggered this action.")]
        [SerializeField]
        private bool m_ApplyToCollidingObject;
        [Tooltip("List of rigidbodies to apply force to.")]
        [SerializeField]
        private Rigidbody[] m_TargetRigidbodies;
        [Tooltip("If true, the rigidbody will be completely stopped before applying force.")]
        [SerializeField]
        private bool m_StopBeforeApplying;

        [HideInInspector]
        [SerializeField]
        private eRigidBodyForceActionDirectionType m_ForceDirectionType;
        public eRigidBodyForceActionDirectionType ForceDirectionType { get { return m_ForceDirectionType; } set { m_ForceDirectionType = value; } }

        [HideInInspector]
        [SerializeField]
        private bool m_ProportionalToDistance;
        public bool ProportionalToDistance { get { return m_ProportionalToDistance; } set { m_ProportionalToDistance = value; } }
        [HideInInspector]
        [SerializeField]
        private float m_MaxProportionalDistance;
        public float MaxProportionalDistance { get { return m_MaxProportionalDistance; } set { m_MaxProportionalDistance = value; } }
        [HideInInspector]
        [SerializeField]
        private GameObject m_DirectionObject;
        public GameObject DirectionObject { get { return m_DirectionObject; } set { m_DirectionObject = value; } }
        [HideInInspector]
        [SerializeField]
        private float m_ForceMultiplier;
        public float ForceMultiplier { get { return m_ForceMultiplier; } set { m_ForceMultiplier = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_Force;
        public Vector3 Force { get { return m_Force; } set { m_Force = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_InLocalCoords;
        public bool InLocalCoords { get { return m_InLocalCoords; } set { m_InLocalCoords = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_MinRandomMultipliers;
        public Vector3 MinRandomMultipliers { get { return m_MinRandomMultipliers; } set { m_MinRandomMultipliers = value; } }
        [HideInInspector]
        [SerializeField]
        private Vector3 m_MaxRandomMultipliers;
        public Vector3 MaxRandomMultipliers { get { return m_MaxRandomMultipliers; } set { m_MaxRandomMultipliers = value; } }
        //[HideInInspector]
        [SerializeField]
        private ForceMode m_ForceMode;
        public ForceMode ForceMode { get { return m_ForceMode; } set { m_ForceMode = value; } }

        [HideInInspector]
        [SerializeField]
        private bool m_UseRigidbodyLocalSpace = false; //Warning, if set to true, use with care.
        public bool UseRigidbodyLocalSpace { get { return m_UseRigidbodyLocalSpace; } set { m_UseRigidbodyLocalSpace = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_NormalizeForceVector = false;
        public bool NormalizeForceVector { get { return m_NormalizeForceVector; } set { m_NormalizeForceVector = value; } }

        protected override void PerformAction(GameObject triggeringObject)
        {
            base.PerformAction(triggeringObject);
            //Debug.Log("Performing action for : " + other.gameObject.name + " @ " + (other.gameObject.GetComponentInParent<ServerPlayer>() ? other.gameObject.GetComponentInParent<ServerPlayer>().gameObject.name : other.gameObject.transform.parent.gameObject.name));
                
            if (m_ApplyToCollidingObject)
            {
                Rigidbody rb = triggeringObject.GetComponent<Rigidbody>();
                if (rb != null)
                    ApplyForce(rb);
            }

            for (int i = 0; i < m_TargetRigidbodies.Length; i++)
            {
                if (m_TargetRigidbodies[i] != null)
                    ApplyForce(m_TargetRigidbodies[i]);
            }
        }

        void ApplyForce(Rigidbody rb)
        {
            if (m_StopBeforeApplying)
                rb.velocity = Vector3.zero;

            Vector3 forceVector = Vector3.zero;

            switch (m_ForceDirectionType)
            {
                case eRigidBodyForceActionDirectionType.StaticDirection:
                    forceVector = m_InLocalCoords ? transform.TransformDirection(m_Force) : m_Force;
                    break;
                case eRigidBodyForceActionDirectionType.OppositeVelocity:
                    forceVector = -rb.velocity;
                    break;
                case eRigidBodyForceActionDirectionType.InVelocityDirection:
                    forceVector = rb.velocity;
                    break;
                case eRigidBodyForceActionDirectionType.InHitDirection:
                    forceVector = (rb.position - cachedTransform.position).normalized;
                    break;
                case eRigidBodyForceActionDirectionType.InDirectionOfObject:
                    if (m_DirectionObject != null)
                    {
                        forceVector = (m_DirectionObject.transform.position - rb.position).normalized;
                        if (m_ProportionalToDistance)
                        {
                            forceVector *= Mathf.Max(1f - ((m_DirectionObject.transform.position - rb.position).magnitude / m_MaxProportionalDistance), 0f);
                        }
                    }
                    break;
            }

            if (m_MinRandomMultipliers != Vector3.zero || m_MaxRandomMultipliers != Vector3.zero)
            {
                forceVector = new Vector3(m_Force.x * Random.Range(m_MinRandomMultipliers.x, m_MaxRandomMultipliers.x),
                                            m_Force.y * Random.Range(m_MinRandomMultipliers.y, m_MaxRandomMultipliers.y),
                                            m_Force.z * Random.Range(m_MinRandomMultipliers.z, m_MaxRandomMultipliers.z));
            }
            if (m_NormalizeForceVector)
                forceVector.Normalize();
            forceVector *= m_ForceMultiplier;

            Debug.DrawLine(rb.transform.position, rb.transform.position + forceVector);

            if (!m_UseRigidbodyLocalSpace)
                rb.AddForce(forceVector, m_ForceMode);
            else
                rb.AddRelativeForce(forceVector, m_ForceMode);
        }
    }
}