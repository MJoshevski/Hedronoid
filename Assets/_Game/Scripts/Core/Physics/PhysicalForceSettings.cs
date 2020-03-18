using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Physics/Force")]
    public class PhysicalForceSettings : ScriptableObject
    {
        public Vector3 Direction;
        public float Multiplier = 1;
        public ForceMode ForceMode;
        public AnimationCurve PowerOverTime;
    }
}