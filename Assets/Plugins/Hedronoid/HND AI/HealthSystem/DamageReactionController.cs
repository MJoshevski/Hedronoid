using Hedronoid;
using Hedronoid.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.Health
{
    public class DamageReactionController : HNDGameObject
    {
        [SerializeField]
        Animator animator;

        [SerializeField]
        [HideInInspector]
        private List<HitReactionAnimation> m_hitReactionAnimations = new List<HitReactionAnimation>();

        private const string REACT_TRIGGER = "ReactTrigger";
        private const string REACTION_TYPE = "ReactionType";

        public void React(EDamageType damageType)
        {
            if (animator)
            {
                foreach (HitReactionAnimation item in m_hitReactionAnimations)
                {
                    if (damageType == item.type)
                    {
                        animator.SetTrigger(REACT_TRIGGER);
                        animator.SetInteger(REACTION_TYPE, item.animationType);
                    }
                }
            }
        }
    }

    [Serializable]
    public struct HitReactionAnimation
    {
        public EDamageType type;
        public int animationType;
    }
}
