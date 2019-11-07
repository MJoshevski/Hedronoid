using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    [CreateAssetMenu(menuName = "Actions/Mono Actions/Action Batch")]

    public class ActionBatch : Action
    {
        public Action[] actions;
        public override void Execute()
        {
            foreach (Action a in actions)
                a.Execute();
        }
    }
}
