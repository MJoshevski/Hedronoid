using UnityEngine;

namespace Hedronoid
{
    public class GravitySource : MonoBehaviour
    {
        void OnEnable()
        {
            GravityService.Register(this);
        }

        void OnDisable()
        {
            GravityService.Unregister(this);
        }

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }
    }
}