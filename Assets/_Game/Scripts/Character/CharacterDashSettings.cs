using Core;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDashSettings : ScriptableObject
{
    public string ActionName;
    public int ExecutionsBeforeReset = 0;
    public bool ContinuousInput = false;

    public PhysicalForceSettings PhysicalForce;
}