using UnityEngine;
using System.Collections;

namespace Hedronoid
{
    public static class RigidbodyExtensions
    {
        public static void ApplyForce(this Rigidbody rigidbody, Vector3 force, ForceMode mode = ForceMode.Force)
        {
#if UNITY_EDITOR
            Debug.DrawRay(rigidbody.transform.position, force * .05f, Color.green);
#endif
            rigidbody.AddForce(force, mode);
        }

        public static IEnumerator ApplyForceContinuously(this Rigidbody rigidbody, Vector3 force, PhysicalForceSettings forceSettings)
        {
            float time = 0f;
            AnimationCurve powerOverTime = forceSettings.PowerOverTime;
            Keyframe lastKeyFrame = powerOverTime[powerOverTime.length - 1];
            while (time <= lastKeyFrame.time)
            {
                float power = powerOverTime.Evaluate(time);

                rigidbody.ApplyForce(force * power * forceSettings.Multiplier, forceSettings.ForceMode);

                yield return new WaitForFixedUpdate();

                time += Time.fixedDeltaTime;
            }
        }
    }
}