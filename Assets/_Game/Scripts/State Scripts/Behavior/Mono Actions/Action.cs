using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public abstract class Action : ScriptableObject 
    {
        public abstract void Execute_Start();
        public abstract void Execute();
    }
}
