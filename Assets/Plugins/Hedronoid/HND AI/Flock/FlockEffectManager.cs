using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Hedronoid.Objects;
using Hedronoid;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public class FlockEffectManager : HNDGameObject
    {
        private static FlockEffectManager m_Instance = null;
        // private Dictionary<NPC.NPCType, List<FlockAttract>> m_Attractors = new Dictionary<NPC.NPCType, List<FlockAttract>>();
        // private Dictionary<NPC.NPCType, List<FlockRepel>> m_Repels = new Dictionary<NPC.NPCType, List<FlockRepel>>();
        private Dictionary<NPC.NPCType, List<BaseInteractable>> m_Interact = new Dictionary<NPC.NPCType, List<BaseInteractable>>();

        private Dictionary<Collider, FlockAttract> m_Attractors = new Dictionary<Collider, FlockAttract>();
        private Dictionary<Collider, FlockRepel> m_Repels = new Dictionary<Collider, FlockRepel>();
        private Dictionary<Collider, HerdMemberNavigation> m_Members = new Dictionary<Collider, HerdMemberNavigation>();
        private List<HerdMemberNavigation> m_MembersList = new List<HerdMemberNavigation>();

        const int m_Thoughts = 10;
        private int[] m_CurrentThinking;
        private int m_AssuredBrainCell = 0;

        protected override void Awake()
        {
            base.Awake();
            m_CurrentThinking = new int[m_Thoughts];
            ResetBrainCells();
        }

        private void ResetBrainCells()
        {
            int membersCount = 0;
            membersCount = m_Members.Count > 0 ? m_Members.Count : m_Thoughts;
            for (int i = 0; i < m_Thoughts; i++)
            {
                m_CurrentThinking[i] = i % membersCount;
            }
        } 

        private void Update()
        {
            if (m_MembersList.Count <= 0)
            {
                return;
            }
            for (int index = 0; index < m_Thoughts; index++)
            {
                if (m_MembersList.Count <= m_CurrentThinking[index])
                {
                    ResetBrainCells();
                }
                int tries = Mathf.FloorToInt((float)m_MembersList.Count / (float)m_Thoughts);
                while ((!m_MembersList[m_CurrentThinking[index]] || !m_MembersList[m_CurrentThinking[index]].Active || !m_MembersList[m_CurrentThinking[index]].gameObject.activeInHierarchy) && tries > 0)
                {
                    m_CurrentThinking[index] += m_Thoughts;
                    m_CurrentThinking[index] %= m_MembersList.Count;
                    tries--;
                }
                if (m_MembersList[m_CurrentThinking[index]] && m_MembersList[m_CurrentThinking[index]].Active && m_MembersList[m_CurrentThinking[index]].gameObject.activeSelf)
                {
                    m_MembersList[m_CurrentThinking[index]].CanThink = true;
                    m_CurrentThinking[index] += m_Thoughts;
                    m_CurrentThinking[index] %= m_MembersList.Count;
                }
                if (!m_MembersList[m_CurrentThinking[index]])
                {
                    m_MembersList.RemoveAll(item => item == null);
                    ResetBrainCells();
                }
            }
            if (m_Members.Count <= m_AssuredBrainCell)
                m_AssuredBrainCell = 0;
            m_MembersList[m_AssuredBrainCell].CanThink = true;
            m_AssuredBrainCell++;
            m_AssuredBrainCell %= m_Members.Count;
        }

        public static FlockEffectManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject fem = new GameObject();
                    fem.name = "FlockEffectManager";
                    m_Instance = fem.AddComponent<FlockEffectManager>();
                }
                return m_Instance;
            }

            set {  m_Instance = value; }
        }

        public void RegisterFlockAttract(FlockAttract fa)
        {
            var coll = fa.GetComponent<Collider>();
            if (coll && !m_Attractors.ContainsKey(coll)) m_Attractors.Add(coll, fa);
        }

        public void UnRegisterFlockAttract(FlockAttract fa)
        {
            HNDEvents.Instance.Raise(new FlockAttractUnsubscribeEvent { Attract = fa });
            var coll = fa.GetComponent<Collider>();
            if (coll && m_Attractors.ContainsKey(coll)) m_Attractors.Remove(coll);
        }

        public FlockAttract GetAttract(Collider coll)
        {
            FlockAttract result;
            m_Attractors.TryGetValue( coll, out result );
            return result;
        }


        public void RegisterFlockRepel(FlockRepel fr)
        {
            var coll = fr.GetComponent<Collider>();
            if (coll && !m_Repels.ContainsKey(coll)) m_Repels.Add(coll, fr);
        }

        public void UnRegisterFlockRepel(FlockRepel fr)
        {
            HNDEvents.Instance.Raise(new FlockRepeltUnsubscribeEvent { Repel = fr });
            var coll = fr.GetComponent<Collider>();
            if (coll && m_Repels.ContainsKey(coll)) m_Repels.Remove(coll);
        }

        public FlockRepel GetRepel(Collider coll)
        {
            FlockRepel result;
            m_Repels.TryGetValue( coll, out result );
            return result;
        }

        public void RegisterMember(HerdMemberNavigation mbr)
        {
            var coll = mbr.GetComponent<Collider>();
            if (coll && !m_Members.ContainsKey(coll))
            {
                m_Members.Add(coll, mbr);
                m_MembersList.Add(mbr);
                ResetBrainCells();
            }
        }
        
        public void UnRegisterMember(HerdMemberNavigation fa)
        {
            var coll = fa.GetComponent<Collider>();
            if (coll && m_Members.ContainsKey(coll))
            {
                m_Members.Remove(coll);
                m_MembersList.Remove(fa);
                ResetBrainCells();
            }
        }

        public HerdMemberNavigation GetMember(Collider coll)
        {
            HerdMemberNavigation result;
            m_Members.TryGetValue( coll, out result );
            return result;
        }

        public void RegisterInteract(BaseInteractable i)
        {
            foreach (NPC.NPCType npc in i.InteractNPCTypes)
            {
                if (!m_Interact.ContainsKey(npc))
                    m_Interact.Add(npc, new List<BaseInteractable>());
                m_Interact[npc].Add(i);
            }
        }

        public void UnRegisterInteract(BaseInteractable i)
        {
            foreach (NPC.NPCType npc in i.InteractNPCTypes)
            {
                if (m_Interact.ContainsKey(npc))
                {
                    m_Interact[npc].Remove(i);
                }
            }
        }

        public void RemoveTypeInteract(BaseInteractable i, NPC.NPCType type)
        {
            if(m_Interact.ContainsKey(type))
                m_Interact[type].Remove(i);
        }

        public void AddTypeInteract(BaseInteractable i, NPC.NPCType type)
        {
            if (!m_Interact.ContainsKey(type))
                m_Interact.Add(type, new List<BaseInteractable>());
            m_Interact[type].Add(i);
        }

        public List<BaseInteractable> GetInteract(NPC.NPCType type)
        {
            if (m_Interact.ContainsKey(type))
                return m_Interact[type];
            return new List<BaseInteractable>();
        }
    }
}
