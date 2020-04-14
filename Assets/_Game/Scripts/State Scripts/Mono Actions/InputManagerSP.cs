using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Mono Actions/Input Manager")]
    public class InputManagerSP : Action
    {
        public FloatVariable horizontal;
        public FloatVariable vertical;
        public BoolVariable jumpBtnDown;
        public BoolVariable jumpBtnUp;
        public BoolVariable dashBtnDown;
        public BoolVariable dashBtnUp;

        public StateManagerVariable playerStates;
        public ActionBatch inputUpdateBatch;

        private PlayerActionSet playerActions;

        public override void Execute_Start()
        {
            inputUpdateBatch.Execute_Start();
            playerActions = playerStates.value.PlayerActions;
        }

        public override void Execute()
        {
            inputUpdateBatch.Execute();

            playerStates.value.jumpPressed = jumpBtnDown.value;
            playerStates.value.jumpReleased = jumpBtnUp.value;

            playerStates.value.dashPressed = dashBtnDown.value;
            playerStates.value.dashReleased = dashBtnUp.value;

            playerStates.value.movementVariables.Horizontal = horizontal.value = playerActions.Move.X;
            playerStates.value.movementVariables.Vertical = vertical.value = playerActions.Move.Y;


            playerStates.value.movementVariables.desiredVelocity =
                new Vector3(horizontal.value, 0f, vertical.value) *
            playerStates.value.movementVariables.MovementSpeed;
        }        
    }

}
