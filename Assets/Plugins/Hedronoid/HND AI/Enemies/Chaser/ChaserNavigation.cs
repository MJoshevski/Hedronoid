using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{

    public class ChaserNavigation : AIBaseNavigation
    {
        [Header("Chaser Settings")]
        [SerializeField]
        protected float m_ReturnAreaSize = 15f;

        protected Vector3 m_PreviousDirection = Vector3.zero;

        public float ReturnAreaSize
        {
            get
            {
                return m_ReturnAreaSize;
            }

            set
            {
                m_ReturnAreaSize = value;
            }
        }

        public override Vector3 GetDirection()
        {
            Vector3 direction = Vector3.zero;

            if (IsInState(EStates.GoToTarget))
            {
                direction = m_Target.position - transform.position;
                direction.Normalize();
            }
            else if (IsInState(EStates.ReturnToDefault))
            {
                direction = m_DefaultTarget.position - transform.position;
                direction.Normalize();
            }
            else if (IsInState(EStates.DefaultMovement))
            {
                if (Random.Range(0f, 1f) > 0.95f)
                {
                    direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                }
                else
                {
                    direction = m_PreviousDirection;
                }
                if ((transform.position.x >= m_DefaultTarget.position.x + ReturnAreaSize / 2f) ||
                    (transform.position.x <= m_DefaultTarget.position.x - ReturnAreaSize / 2f) ||
                    (transform.position.z >= m_DefaultTarget.position.z + ReturnAreaSize / 2f) ||
                    (transform.position.z <= m_DefaultTarget.position.z - ReturnAreaSize / 2f))
                {

                    direction = m_DefaultTarget.position - transform.position;
                }
                direction.Normalize();
            }
            m_PreviousDirection = direction;
            return direction;
        }

        public override void OnGoToTargetUpdate()
        {
            ((ChaserMotor)m_Motor).Wander = false;
            ((ChaserMotor)m_Motor).Move(GetDirection());
        }

        public override void OnFleeFromTargetUpdate()
        {
        }

        public override void OnReturnToDefaultUpdate()
        {
            ((ChaserMotor)m_Motor).Move(GetDirection());
            if (Vector3.Distance(transform.position, m_DefaultTarget.position) < ReturnAreaSize / 2f)
            {
                ChangeState(EStates.DefaultMovement);
            }
        }

        public override void OnDefaultMovementUpdate()
        {
            ((ChaserMotor)m_Motor).Wander = true;
            ((ChaserMotor)m_Motor).Move(GetDirection());
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (m_DefaultTarget)
                Gizmos.DrawWireCube(m_DefaultTarget.position, new Vector3(ReturnAreaSize, ReturnAreaSize, ReturnAreaSize));
        }
#endif
    }
}