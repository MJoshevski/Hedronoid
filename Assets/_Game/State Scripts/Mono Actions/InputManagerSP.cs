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
        public BoolVariable jump;

        public StateManagerVariable playerStates;
        public ActionBatch inputUpdateBatch;

        public override void Execute_Start()
        {
            inputUpdateBatch.Execute_Start();
        }

        public override void Execute()
        {
            inputUpdateBatch.Execute();
            playerStates.value.isJumping = jump.value;
        }        
    }

}
