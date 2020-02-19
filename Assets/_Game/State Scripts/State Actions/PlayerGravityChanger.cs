using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/State Actions/Player Gravity Changer")]
    public class PlayerGravityChanger : StateActions
    {

        private string MostRecentCollision;
        private bool CanSwitchAgain = true;
        private GravityVariables gravityVariables;

        public override void Execute_Start(PlayerStateManager states)
        {
            gravityVariables = states.gravityVariables;
        }

        public override void Execute(PlayerStateManager states)
        {
            if (gravityVariables.ChangeOnKeyPress)
            {
                IGravityService gravityService = GravityService.Instance;
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    gravityService.SwitchDirection(GravityDirections.DOWN);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    gravityService.SwitchDirection(GravityDirections.UP);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    gravityService.SwitchDirection(GravityDirections.LEFT);
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    gravityService.SwitchDirection(GravityDirections.RIGHT);
                if (Input.GetKeyDown(KeyCode.Alpha5))
                    gravityService.SwitchDirection(GravityDirections.FRONT);
                if (Input.GetKeyDown(KeyCode.Alpha6))
                    gravityService.SwitchDirection(GravityDirections.BACK);                
            }
            else
            {
                IGravityService gravityService = GravityService.Instance;
                RaycastHit m_Hit;

                bool m_HitDetect = Physics.SphereCast(
                    states.Transform.localPosition,
                    0.00000001f,
                    states.Transform.forward,
                    out m_Hit);

                if (m_HitDetect)
                {
                    MostRecentCollision = m_Hit.collider.tag;

                    //Debug.Log("Hit object with tag: " + MostRecentCollision);

                    if (MostRecentCollision != gravityService.Direction.ToString() && CanSwitchAgain)
                    {
                        if (MostRecentCollision == "DOWN")
                            gravityService.SwitchDirection(GravityDirections.DOWN);
                        else if (MostRecentCollision == "UP")
                            gravityService.SwitchDirection(GravityDirections.UP);
                        else if (MostRecentCollision == "LEFT")
                            gravityService.SwitchDirection(GravityDirections.LEFT);
                        else if (MostRecentCollision == "RIGHT")
                            gravityService.SwitchDirection(GravityDirections.RIGHT);
                        else if (MostRecentCollision == "FRONT")
                            gravityService.SwitchDirection(GravityDirections.FRONT);
                        else if (MostRecentCollision == "BACK")
                            gravityService.SwitchDirection(GravityDirections.BACK);
                        else return;

                        CanSwitchAgain = false;
                        states.StartCoroutine(states.WaitForSeconds(
                            gravityVariables.GravitySwitchCooldown));
                        CanSwitchAgain = true;
                    }
                }
            }
        }
    }
}