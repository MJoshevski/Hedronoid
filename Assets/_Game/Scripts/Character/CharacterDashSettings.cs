using UnityEngine;

[CreateAssetMenu]
public class CharacterDashSettings : ScriptableObject
{
    public Vector3 RelativeDirection;
    public string ActionName;
    public float Power = 8;
    public bool ShouldBeGrounded;
}