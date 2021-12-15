using UnityEngine;

namespace UnityMovementAI
{
    public class PursueUnit : MonoBehaviour
    {
        public MovementAIRigidbody target;
        public GameObject targetGO;

        SteeringBasics steeringBasics;
        Pursue pursue;

        void Start()
        {
            steeringBasics = GetComponent<SteeringBasics>();
            pursue = GetComponent<Pursue>();
        }

        void FixedUpdate()
        {
            Vector3 accel = Vector3.zero;
            if (target != null)
            {
                accel = pursue.GetSteering(target);
            }
            else
            {
                accel = pursue.GetSteering(targetGO);
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}