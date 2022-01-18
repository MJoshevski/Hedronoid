using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{

    public class HerdRegion : MonoBehaviour 
    {
        [SerializeField]
        private int m_PlayersIn = 0;
        [SerializeField]
        private bool m_DeactivateObjects = true;
        [SerializeField]
        private List<HerdMemberNavigation> m_HerdsIn = new List<HerdMemberNavigation>();

        void OnTriggerEnter(Collider other)
        {
            HerdMemberNavigation hmn = other.GetComponent<HerdMemberNavigation>();
            if(hmn && !m_HerdsIn.Contains(hmn))
            {
                m_HerdsIn.Add(hmn);
            }

            if (other.gameObject.CompareTag("Player"))
            {
                m_PlayersIn++;
                if(m_PlayersIn == 1)
                {
                    ActivateHerd();
                    Invoke("ActivateHerd", 3f);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            HerdMemberNavigation hmn = other.GetComponent<HerdMemberNavigation>();

            if (hmn && m_HerdsIn.Contains(hmn))
            {
                m_HerdsIn.Remove(hmn);
            }

            if (other.gameObject.CompareTag("Player"))
            {
                m_PlayersIn--;
                if (m_PlayersIn <=0)
                {
                    Invoke("DeactivateHerd", 3f);
                }
            }
        }

        void ActivateHerd()
        {
            //Debug.Log("Activate herd " + m_HerdsIn.Count + " in " + gameObject.name);
            foreach(HerdMemberNavigation hmn in m_HerdsIn)
            {
                if (hmn)
                {
                    hmn.ActivateHerd(true);
                }
            }
        }

        void DeactivateHerd()
        {
           // Debug.Log("Deactivate herd " + m_HerdsIn.Count + " in " + gameObject.name);
            foreach (HerdMemberNavigation hmn in m_HerdsIn)
            {
                if (hmn)
                {
                    hmn.DeactivateHerd(!m_DeactivateObjects);
                }
            }
        }

        void CheckForPlayers()
        {
            if (m_PlayersIn <= 0)
            {
                Invoke("DeactivateHerd", 0.5f);
            }
            else
            {
                Invoke("ActivateHerd", 0.5f);
            }
        }

        private void Start()
        {
            Invoke("CheckForPlayers", 0.5f);
        }
    }
}