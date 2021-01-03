using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{

    /// <summary>
    /// The Grunt sensor is set up to look for players first and then Rollandians second.
    /// It uses the physiscs system to find nearby players or NPC's by layer and is onmiprescent.
    /// </summary>
    public class GruntSensor : BlockheadSensor
    {
        // [SerializeField]
        // private LayerMask m_NPCLayer;
        // [SerializeField]
        // private NPC.NPCType m_NPCTypeToHunt;

        private Collider[] m_colliderBuffer = new Collider[10];
        private List<Transform> m_targetsInRange = new List<Transform>(10);

        public override Transform GetTargetWithinReach(float distance)
        {
            m_targetsInRange.Clear();
            // First check if we have any players in range
            var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, HNDAI.Settings.PlayerLayer);
            if (players > 0)
            {
                if (players == 1)
                {
                    return m_colliderBuffer[0].transform;
                }
                if (GetModifiedAggroValue(m_colliderBuffer[0].gameObject) > GetModifiedAggroValue(m_colliderBuffer[1].gameObject))
                {
                    return m_colliderBuffer[0].transform;
                }
                else
                {
                    return m_colliderBuffer[1].transform;
                }
            }
            // else
            // {
            //     // We do not have any players, see if we have NPCs
            //     var npcs = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, m_NPCLayer);
            //     for (int i = 0; i < npcs; i++)
            //     {
            //         var t = m_colliderBuffer[i].transform;
            //         var npc = t.GetComponent<NPC>();
            //         if (npc)
            //         {
            //             if (npc.NpcType == m_NPCTypeToHunt)
            //             {
            //                 m_targetsInRange.Add(t);
            //             }
            //         }
            //         else
            //         {
            //             Debug.LogWarning("We have an object in the NPC layer that is not an NPC: " + t, t);
            //         }
            //     }
            //     return m_targetsInRange.Count > 0 ? m_targetsInRange[UnityEngine.Random.Range(0, m_targetsInRange.Count)] : null;
            // }

            return null;
        }
    }
}