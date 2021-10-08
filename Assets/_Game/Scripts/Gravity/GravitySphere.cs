using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public class GravitySphere : GravitySource
    {
        [Header("Gravity Variables")]
        [SerializeField]
        float gravity = 9.81f;
        public float Gravity { get { return gravity; } }

        [Header("Outer")]
        [SerializeField, Min(0f)]
        [Tooltip("Radius for the outer sphere. Color = Yellow")]
        float outerRadius = 10f;
        [SerializeField, Min(0f)]
        [Tooltip("Radius for the outer falloff sphere. Color = Cyan")]
        float outerFalloffRadius = 15f;
        public float OuterRadius { get { return outerRadius; } }
        public float OuterFalloffRadius { get { return outerFalloffRadius; } }

        [Header("Inner")]
        [SerializeField, Min(0f)]
        [Tooltip("Radius for the inner falloff sphere. Color = Cyan")]
        float innerFalloffRadius = 1f;
        [SerializeField, Min(0f)]
        [Tooltip("Radius for the inner sphere. Color = Yellow")]
        float innerRadius = 5f;
        public float InnerFalloffRadius { get { return innerFalloffRadius; } }
        public float InnerRadius { get { return innerRadius; } }

        float innerFalloffFactor, outerFalloffFactor;
        public float ResizedRadius { get { return resizedRadius; } }

        [Header("Resize")]
        [SerializeField, Min(0f)]
        [Tooltip("Radius for the resize sphere. Color = Black")]
        float resizedRadius;
        float originalRadius;

        // BOUNDS
        [HideInInspector]
        public SphereCollider boundsCollider;
        protected override void OnValidate()
        {
            base.OnValidate();

            innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
            innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
            outerRadius = Mathf.Max(outerRadius, innerRadius);
            outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

            innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
            outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);

            if (!boundsCollider)
                boundsCollider = GetComponent<SphereCollider>();

            boundsCollider.isTrigger = true;

            if (AutomaticColliderSize)
            {
                boundsCollider.radius = outerFalloffRadius;
                boundsCollider.center = Vector3.zero;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            originalRadius = boundsCollider.radius;
        }
        public override Vector3 GetGravity(Vector3 position)
        {
            if (CurrentPriorityWeight < GravityService.GetMaxPriorityWeight() || !IsPlayerInGravity)
                return Vector3.zero;

            Vector3 vector = transform.position - position;
            float distance = vector.magnitude;
            if (distance > outerFalloffRadius || distance < innerFalloffRadius)
            {
                return Vector3.zero;
            }
            float g = gravity / distance;
            if (distance > outerRadius)
            {
                g *= 1f - (distance - outerRadius) * outerFalloffFactor;
            }
            else if (distance < innerRadius)
            {
                g *= 1f - (innerRadius - distance) * innerFalloffFactor;
            }
            return g * vector;
        }
        protected override void ResizeColliderBounds(bool shouldResize)
        {
            base.ResizeColliderBounds(shouldResize);

            if (shouldResize)
            {
                boundsCollider.radius = resizedRadius;
                boundsCollider.center = Vector3.zero;
            }
            else
            {
                boundsCollider.radius = originalRadius;
                boundsCollider.center = Vector3.zero;
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Vector3 p = transform.position;
            if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(p, innerFalloffRadius);
            }
            Gizmos.color = Color.yellow;
            if (innerRadius > 0f && innerRadius < outerRadius)
            {
                Gizmos.DrawWireSphere(p, innerRadius);
            }
            Gizmos.DrawWireSphere(p, outerRadius);
            if (outerFalloffRadius > outerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(p, outerFalloffRadius);
            }

            if (ResizeColliderOnEnter)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(p, resizedRadius);
            }
        }
#endif

    }
}