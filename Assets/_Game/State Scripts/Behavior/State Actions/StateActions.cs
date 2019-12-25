using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public abstract class StateActions : ScriptableObject
    {
        public abstract void Execute_Start(StateManager states);
        public abstract void Execute(StateManager states);
    }
}
