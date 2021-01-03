using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class ChaserMotor : AIBaseMotor
    {
        public enum MovementType
        {
            FORCE = 0,
            TRANSLATE,
            POSITION
        }
        [SerializeField]
        protected MovementType m_MovementType = MovementType.FORCE;
        [SerializeField]
        protected ForceMode m_ForceMode = ForceMode.VelocityChange;
        [SerializeField]
        protected float m_WanderSpeed = 3f;
        [SerializeField]
        protected float m_MaxSpeed = 3f;
        [SerializeField]
        protected float m_MinSpeed = 1f;
        [SerializeField]
        protected float m_Speed = 0.001f;
        [SerializeField]
        protected float m_Aceleration = 3f;
        [SerializeField]
        protected float m_RotationSpeed = 4.0f;

        protected bool m_Wander = false;

        public bool Wander 
        {
            get { return m_Wander; }
            set { m_Wander = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            m_Speed = Random.Range(m_MinSpeed, m_MaxSpeed);
        }

        public override void Move(Vector3 Direction)        
        {
            if (Direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Direction), m_RotationSpeed * Time.deltaTime);
            }

            switch (m_MovementType)
            {
                case MovementType.FORCE:
                    if (cachedRigidbody.velocity.magnitude < (Wander ? m_WanderSpeed : m_Speed))
                        cachedRigidbody.AddForce(transform.forward * Time.deltaTime * (m_ForceMode == ForceMode.VelocityChange ? (Wander ? m_WanderSpeed : m_Speed) : m_Aceleration), m_ForceMode);
                    break;
                case MovementType.TRANSLATE:
                    transform.Translate(0, 0, Time.deltaTime * (Wander ? m_WanderSpeed : m_Speed));
                    break;
                case MovementType.POSITION:
                        transform.position += transform.forward * (Wander ? m_WanderSpeed : m_Speed) * Time.deltaTime;
                    break;
            }
        }
    }
}