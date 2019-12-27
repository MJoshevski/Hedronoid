using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Physics/Force")]
    public class PhysicalForceSettings : ScriptableObject
    {
        public Vector3 Direction;
        public ForceMode ForceMode;
        public AnimationCurve PowerOverTime;
    }
}