﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public class AnimatorData
    {

        public Transform leftFoot;
        public Transform rightFoot;

        public AnimatorData(Animator anim)
        {
            leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }
}
