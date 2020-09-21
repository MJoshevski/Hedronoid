using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public class AnimatorHashes
    {
        public int Vertical = Animator.StringToHash("Vertical");
        public int Horizontal = Animator.StringToHash("Horizontal");
        public int LeftFootForward = Animator.StringToHash("leftFootForward");
        public int JumpForward = Animator.StringToHash("Jump_Forward");
        public int JumpIdle = Animator.StringToHash("Jump_Idle");
        public int DoubleJump = Animator.StringToHash("Double_Jump");
        public int IsGrounded = Animator.StringToHash("isGrounded");
        public int Flying = Animator.StringToHash("Flying");
        public int Falling = Animator.StringToHash("Falling");
        public int LandFast = Animator.StringToHash("Land_Fast");
        public int LandRun = Animator.StringToHash("Land_Run");
        public int Dash = Animator.StringToHash("Dash");
        public int IsPlayingAnim = Animator.StringToHash("isPlayingAnim");
    }
}
