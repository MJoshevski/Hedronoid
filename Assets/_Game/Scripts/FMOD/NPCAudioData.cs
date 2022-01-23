using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hedronoid/Audio/NPC", fileName = "New NPC Sheet")]
public class NPCAudioData : ScriptableObject
{
    [Header("Default")]
    [FMODUnity.EventRef]
    public string idle = null;
    [FMODUnity.EventRef]
    public string idleMovement = null;
    [FMODUnity.EventRef]
    public string recieveHit = null;
    [FMODUnity.EventRef]
    public string blockHit = null;
    [FMODUnity.EventRef]
    public string death = null;

    [Header("Attacking")]
    [FMODUnity.EventRef]
    public string[] primaryAttack = null;
    [FMODUnity.EventRef]
    public string secondaryAttack = null;
}
