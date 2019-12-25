using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Mono Actions/Time Manager")]
    public class TimeManager : Action
    {
        public FloatVariable targetDelta;

        public override void Execute_Start()
        {
        }

        public override void Execute()
        {
            targetDelta.value = Time.deltaTime;
        }
    }
}
