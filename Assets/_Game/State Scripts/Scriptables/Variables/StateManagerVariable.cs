using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    [CreateAssetMenu(menuName ="Variables/State Manager")]
    public class StateManagerVariable : ScriptableObject
    {
        public StateManager value;

        public void Set(StateManager sm)
        {
            value = sm;
        }
    }
}
