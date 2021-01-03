using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.AI;
using Hedronoid;

// This is a base class for objects that Zombies and Rollandians can interact with

namespace Hedronoid.Objects
{
    public class BaseInteractable : HNDGameObject
    {
        [SerializeField]
        protected bool m_Interactable = false;
        [SerializeField]
        protected float m_InteractDistance = 3f;
        [SerializeField]
        protected List<string> m_InteractTypes = new List<string>();
        [SerializeField]
        protected List<NPC.NPCType> m_InteractNPCTypes = new List<NPC.NPCType>();

        public float InteractDistance
        {
            get { return m_InteractDistance; }
            set { m_InteractDistance = value; }
        }

        public bool Interactable
        {
            get { return m_Interactable; }
            set
            {
                if (value != m_Interactable)
                {/*
                    if (value)
                        FlockEffectManager.Instance.RegisterInteract(this);
                    else
                        FlockEffectManager.Instance.UnRegisterInteract(this);*/
                    m_Interactable = value;
                }
            }
        }

        public List<NPC.NPCType> InteractNPCTypes
        {
            get { return m_InteractNPCTypes; }
            set { m_InteractNPCTypes = value; }
        }

        public virtual void Interact(GameObject npc, float valueF = 0, bool valueB = false)
        {
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, InteractDistance);
        }
#endif

        protected override void Start()
        {
            base.Start();
            if (m_Interactable)
                FlockEffectManager.Instance.RegisterInteract(this);
        }
    }
}