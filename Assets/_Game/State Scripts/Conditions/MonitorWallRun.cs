using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Conditions/Monitor Wall-Run")]
    public class MonitorWallRun : Condition
    {
        public StateActions onTrueAction;
        public bool WallRunning { get { return _wallRunning; } private set { _wallRunning = value; } }

        private bool _wallRunning;
        private CollisionVariables collisionVariables;

        public override void InitCondition(PlayerStateManager state)
        {
            onTrueAction.Execute_Start(state);
            collisionVariables = state.collisionVariables;
        }

        public override bool CheckCondition(PlayerStateManager state)
        {
            //RaycastHit[] arrayHits = collisionVariables.ArrayHits;
            //bool[] arrayFlags = collisionVariables.ArrayFlags;

            //for (int i = 0; i < arrayFlags.Length; i++)
            //{
            //    if (arrayFlags[i])
            //    {

            //        return false;
            //    }
            //}

            return false;
        }

        //void CheckIfWallRunning(PlayerStateManager state)
        //{
        //    for (int i = 0; i < collision.contactCount; i++)
        //    {
        //        var contact = collision.contacts[i];
        //        var dot = Vector3.Dot(GravityService.Instance.GravityUp, contact.normal);
        //        if (Mathf.Approximately(0f, dot))
        //        {
        //            onTrueAction.Execute(state);
        //            WallRunning = true;
        //        }
        //    }
        //}
    }
}

