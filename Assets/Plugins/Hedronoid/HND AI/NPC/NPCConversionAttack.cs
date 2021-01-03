using Hedronoid.AI;
using System.Collections;
using UnityEngine;

/// <summary>
/// This should be connected to an object that has a regular collider and will start converting
/// an NPC when it touches it.
/// </summary>
public class NPCConversionAttack : MonoBehaviour
{
    [SerializeField]
    private NPC.NPCType _convertFrom;
    [SerializeField]
    private NPC.NPCType _convertTo;

    private void OnCollisionEnter(Collision collision)
    {
        var npc = collision.collider.GetComponent<NPC>();
        if (npc && npc.NpcType == _convertFrom)
        {
            npc.ConvertNpc(_convertTo);
        }
    }
}