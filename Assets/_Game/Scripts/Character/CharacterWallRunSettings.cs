using UnityEngine;

[CreateAssetMenu]
public class CharacterWallRunSettings : ScriptableObject
{
    [Tooltip("To negate gravity must be equal to GravityApplier on object")]
    [Range(0f, 10f)]
    public float GravityNegateMultiplier;

    [Tooltip("How long the wall run applies.")]
    public float Duration;
}