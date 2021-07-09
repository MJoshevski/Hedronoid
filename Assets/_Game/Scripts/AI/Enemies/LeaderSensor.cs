using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using Hedronoid.Objects;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    /// <summary>
    /// The leader sensor is set up to look for players first otherwise choose a tower that we want to move to.
    /// It uses the physiscs system to find nearby players or NPC's by layer and is onmiprescent.
    /// </summary>
    public class LeaderSensor : BlockheadSensor
    {
        [SerializeField]
        private LayerMask m_TargetLayer;
        [SerializeField]
        private float m_ClosesDistanceToNPCs = 3;
        [SerializeField]
        private float m_MaxDistanceToPlayer = 12;

        private Collider[] colliders = new Collider[40];

        protected override void Start()
        {
            base.Start();
        }

        public override Transform GetTargetWithinReach(float distance)
        {
            int amount = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, m_TargetLayer);
            if (amount > 0)
            {
                // We have a player within range, return him.
                Transform targetTransform;
                Transform farNPC = null;
                Transform player = null;
                float playerAggro = 0;
                float maxDist = 0;
                for (int i = 0; i < amount; i++)
                {
                    var col = colliders[i];
                    targetTransform = col.transform;
                    if (col.attachedRigidbody)
                    {
                        targetTransform = col.attachedRigidbody.transform;
                    }
                    if (targetTransform.CompareTag(HNDAI.Settings.PlayerTag) && Vector3.Distance(targetTransform.position,transform.position) < m_MaxDistanceToPlayer)
                    {
                        var aggro = GetModifiedAggroValue(targetTransform.gameObject);
                        if (aggro > playerAggro)
                        {
                            playerAggro = aggro;
                            player = targetTransform;
                        }
                    }
                    else if (player == null) // If we have found a player we don't care about NPC's anymore
                    {
                        NPC npc = targetTransform.gameObject.GetComponent<NPC>();
                        if (npc && (npc.NpcType == NPC.NPCType.Zombie || npc.NpcType == NPC.NPCType.Firembie))
                        {
                            float dist = Vector3.Distance(transform.position, targetTransform.position);
                            if(dist > maxDist && dist < m_ClosesDistanceToNPCs)
                            {
                                maxDist = dist;
                                farNPC = targetTransform;
                            }
                        }
                    }
                }
                return player ?? farNPC;
            }
            return null;
        }

    }
}