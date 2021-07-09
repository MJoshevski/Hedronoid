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
    public class DetectPlayerSensor : BlockheadSensor
    {
        [SerializeField]
        private LayerMask m_PlayerLayer;

        private Collider[] m_colliderBuffer = new Collider[10];

        public override Transform GetTargetWithinReach(float distance)
        {
            // First check if we have any players in range
            var players = Physics.OverlapSphereNonAlloc(transform.position, distance, m_colliderBuffer, m_PlayerLayer);
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

            return null;
        }

    }
}