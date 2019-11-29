using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    public abstract class StateActions : ScriptableObject
    {
        public abstract void Execute(StateManager states);
    }
}
