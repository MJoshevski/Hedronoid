using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    public class AnimatorHashes
    {
        public int Vertical = Animator.StringToHash("Vertical");
        public int Horizontal = Animator.StringToHash("Horizontal");
        public int LeftFootForward = Animator.StringToHash("leftFootForward");
        public int JumpForward = Animator.StringToHash("Jump_Forward");
        public int JumpIdle = Animator.StringToHash("Jump_Idle");
        public int IsGrounded = Animator.StringToHash("isGrounded");
    }
}
