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

        public StateManagerVariable playerStates;
        public ActionBatch inputUpdateBatch;

        public override void Execute_Start()
        {
            inputUpdateBatch.Execute_Start();
        }

        public override void Execute()
        {
            inputUpdateBatch.Execute();

            playerStates.value.jumpPressed = jumpBtnDown.value;
            playerStates.value.jumpReleased = jumpBtnUp.value;
        }        
    }

}
