﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hedronoid
{
    public class UpdateBool : StateMachineBehaviour
    {
        public string targetBool;
        public bool status;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(targetBool, status);
        }
    }
}