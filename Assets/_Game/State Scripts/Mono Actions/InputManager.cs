using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SO;

namespace SA
{
    [CreateAssetMenu(menuName = "Actions/Mono Actions/Input Manager")]
    public class InputManager : Action
    {
        public FloatVariable horizontal;
        public FloatVariable vertical;
        public BoolVariable jump;

        public StateManagerVariable playerStates;
        public ActionBatch inputUpdateBatch;

        public override void Execute()
        {
            inputUpdateBatch.Execute();

            if(playerStates.value != null)
            {
                playerStates.value.movementVariables.Horizontal = horizontal.value;
                playerStates.value.movementVariables.Vertical = vertical.value;

                float moveAmount =
                    Mathf.Clamp01(Mathf.Abs(horizontal.value) + Mathf.Abs(vertical.value));
                playerStates.value.movementVariables.MoveAmount = moveAmount;
                playerStates.value.isJumping = jump.value;

            }
        }
    }

}
