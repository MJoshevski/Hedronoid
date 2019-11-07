using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class AnimatorHashes
    {
        public int Vertical = Animator.StringToHash("Vertical");
        public int Horizontal = Animator.StringToHash("Horizontal");
        public int LeftFootForward = Animator.StringToHash("leftFootForward");
        public int JumpForward = Animator.StringToHash("JumpForward");
        public int JumpIdle = Animator.StringToHash("JumpIdle");
        public int IsGrounded = Animator.StringToHash("isGrounded");
    }
}
