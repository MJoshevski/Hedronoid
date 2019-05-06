using UnityEngine;

[CreateAssetMenu(menuName = "Physics/VelocityDamp")]
public class VelocityDampSettings : ScriptableObject
{
    [Tooltip("y is velocity magnitude, x is damping force multiplier")]
    public AnimationCurve SpeedDampingCurve;

}