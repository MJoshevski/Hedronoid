using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hedronoid
{
    public class OnEnable_AssignStateManager : MonoBehaviour
    {
        public StateManagerVariable targetVariable;

        private void OnEnable()
        {
            targetVariable.value = GetComponent<PlayerStateManager>();
            Destroy(this);
        }
    }
}
