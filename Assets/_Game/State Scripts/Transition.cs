using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    [System.Serializable]
    public class Transition
    {
        public int id;
        public Condition condition;
        public State targetState;
        public bool disable;
    }
}
