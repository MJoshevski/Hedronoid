using UnityEngine;

[CreateAssetMenu]
public class CharacterMoveSettings : ScriptableObject
{
    public float TurnSpeedMultiplier = 2f;
    public float MoveSpeedMultiplier = 1f;
    public float MoveVeloctiyChangeRate = 1f;
}
