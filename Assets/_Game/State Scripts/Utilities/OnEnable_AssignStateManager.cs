﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SA
{
    public class OnEnable_AssignStateManager : MonoBehaviour
    {
        public StateManagerVariable targetVariable;

        private void OnEnable()
        {
            targetVariable.value = GetComponent<StateManager>();
            Destroy(this);
        }
    }
}