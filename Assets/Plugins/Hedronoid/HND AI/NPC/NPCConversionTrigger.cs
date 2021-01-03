using Hedronoid.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This should be connected to an object that has a trigger collider and will convert
/// an NPC that stays withing the trigger for the given time.
/// </summary>
public class NPCConversionTrigger : MonoBehaviour
{
    [SerializeField]
    private NPC.NPCType _convertFrom;
    [SerializeField]
    private NPC.NPCType _convertTo;
    [SerializeField]
    [Tooltip("How long should the NPC be in the circle before conversion starts?")]
    private float _timeBeforeConversionStarts = 1;

    private Dictionary<NPC, Coroutine> _conversionRoutines = new Dictionary<NPC, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<NPC>();
        if (npc && npc.NpcType == _convertFrom)
        {
            // Start converting an npc
            var c = StartCoroutine(ConvertNPC(npc));
            _conversionRoutines.Add(npc, c);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponent<NPC>();
        if (npc)
        {
            if (_conversionRoutines.ContainsKey(npc))
            {
                StopCoroutine(_conversionRoutines[npc]);
                _conversionRoutines.Remove(npc);
            }
        }
    }

    IEnumerator ConvertNPC(NPC npc)
    {
        // TODO: We should probably trigger some animation or conversion effect here.
        yield return new WaitForSeconds(_timeBeforeConversionStarts);
        npc.ConvertNpc(_convertTo);
    }
}
