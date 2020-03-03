using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Actions/State Actions/Handle Dash Velocity")]
    public class HandleDashVelocity : StateActions
    {
        Coroutine _forceApplyCoroutine = null;
        [SerializeField]
        int _executions = 0;
        DashVariables dashVariables;
        IGravityService gravityService;

        public override void Execute_Start(PlayerStateManager states)
        {
            dashVariables = states.dashVariables;
            gravityService = GravityService.Instance;
        }

        public override void Execute(PlayerStateManager states)
        {
            Vector3 moveDirection = states.movementVariables.MoveDirection;

            if (moveDirection.sqrMagnitude < .25f)
                moveDirection = states.Transform.forward;

            Vector3 forceDirection = states.Transform.forward;

            forceDirection.Normalize();

            if (states.dashVariables.ContinuousInput
            && _forceApplyCoroutine != null)
            {
                states.StopCoroutine(_forceApplyCoroutine);
                _forceApplyCoroutine = null;
                AfterApplyForce();
            }
            
            if (_executions >= dashVariables.ExecutionsBeforeReset)
                return;

            states.StartCoroutine(
                DoApplyForceOverTime(states, forceDirection, dashVariables.PhysicalForce));

        }

        IEnumerator DoApplyForceOverTime(PlayerStateManager states, Vector3 forceDirection, PhysicalForceSettings forceSettings)
        {
            _executions++;

            _forceApplyCoroutine = states.StartCoroutine(
                states.Rigidbody.ApplyForceContinuously(forceDirection, forceSettings));
            yield return _forceApplyCoroutine;

            AfterApplyForce();
        }

        void AfterApplyForce()
        {
            _executions--;
            _forceApplyCoroutine = null;
        }
    }
}
