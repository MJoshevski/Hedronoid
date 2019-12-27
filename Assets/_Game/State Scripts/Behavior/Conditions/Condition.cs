using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public abstract class Condition : ScriptableObject
    {
		public string description;

        public abstract bool CheckCondition(PlayerStateManager state);

    }
}
