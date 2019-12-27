using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName ="Variables/State Manager")]
    public class StateManagerVariable : ScriptableObject
    {
        public PlayerStateManager value;

        public void Set(PlayerStateManager sm)
        {
            value = sm;
        }
    }
}
