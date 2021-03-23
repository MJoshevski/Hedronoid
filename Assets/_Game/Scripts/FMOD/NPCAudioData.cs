using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Hedronoid/Audio/Player", fileName = "New Player Sheet")]
public class PlayerAudioData : ScriptableObject
{
    [Header("Movement")]
    [FMODUnity.EventRef]
    public string footsteps = null;
    [FMODUnity.EventRef]
    public string jump = null;
    [FMODUnity.EventRef]
    public string doubleJump = null;
    [FMODUnity.EventRef]
    public string dash = null;
    [FMODUnity.EventRef]
    public string land = null;

    [Header("Shooting")]
    [FMODUnity.EventRef]
    public string[] bulletPrimary = null;
    [FMODUnity.EventRef]
    public string bulletSecondary = null;

    [Header("Character Sounds")]
    [FMODUnity.EventRef]
    public string recieveHit = null;
}
