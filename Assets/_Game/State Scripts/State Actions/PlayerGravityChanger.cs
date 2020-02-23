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


                var origin = states.Transform.localPosition;
                Vector3 originOffseted = origin;

                if (states.gravityService.Direction == GravityDirections.DOWN)
                    originOffseted = new Vector3(origin.x, origin.y + 1f, origin.z);
                else if (states.gravityService.Direction == GravityDirections.UP)
                    originOffseted = new Vector3(origin.x, origin.y - 1f, origin.z);
                else if (states.gravityService.Direction == GravityDirections.LEFT)
                    originOffseted = new Vector3(origin.x + 1f, origin.y, origin.z);
                else if (states.gravityService.Direction == GravityDirections.RIGHT)
                    originOffseted = new Vector3(origin.x - 1f, origin.y, origin.z);
                else if (states.gravityService.Direction == GravityDirections.FRONT)
                    originOffseted = new Vector3(origin.x, origin.y, origin.z - 1f);
                else if (states.gravityService.Direction == GravityDirections.BACK)
                    originOffseted = new Vector3(origin.x, origin.y, origin.z + 1f);

                RaycastHit hit = new RaycastHit();

                Debug.DrawRay(originOffseted, states.Transform.forward * 1, Color.green);
                Debug.DrawRay(originOffseted, states.Transform.forward * -1, Color.green);
                Debug.DrawRay(originOffseted, states.Transform.up * 1, Color.green);
                Debug.DrawRay(originOffseted, states.Transform.up * -1, Color.green);
                Debug.DrawRay(originOffseted, states.Transform.right * 1, Color.green);
                Debug.DrawRay(originOffseted, states.Transform.right * -1, Color.green);



                bool planeDetect = false;

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                        originOffseted,
                        states.Transform.forward,
                        out hit,
                        2,
                        Layers._ignoreLayersController);
                }

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                        originOffseted,
                        states.Transform.forward * -1,
                        out hit,
                        2,
                        Layers._ignoreLayersController);
                }

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                        originOffseted,
                        states.Transform.up,
                        out hit,
                        2,
                        Layers._ignoreLayersController);
                }

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                        originOffseted,
                        states.Transform.up * -1,
                        out hit,
                        2,
                        Layers._ignoreLayersController);
                }

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                         originOffseted,
                         states.Transform.right,
                         out hit,
                         2,
                         Layers._ignoreLayersController);
                }

                if (!planeDetect)
                {
                    planeDetect = Physics.Raycast(
                         originOffseted,
                         states.Transform.right * -1,
                         out hit,
                         2,
                         Layers._ignoreLayersController);
                }

                //planeDetect = Physics.SphereCast(
                //    originOffseted,
                //    0.00000001f,
                //    states.Transform.forward,
                //    out hit);


                if (planeDetect)
                {
                    MostRecentCollision = hit.collider.tag;

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