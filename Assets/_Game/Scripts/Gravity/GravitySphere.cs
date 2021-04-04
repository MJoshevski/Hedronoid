using UnityEngine;

namespace Hedronoid
{
    public class GravitySphere : GravitySource
    {
        [Header("Gravity Variables")]
        [SerializeField]
        float gravity = 9.81f;
        public float Gravity { get { return gravity; } }

        [SerializeField, Min(0f)]
        float outerRadius = 10f, outerFalloffRadius = 15f;
        public float OuterRadius { get { return outerRadius; } }
        public float OuterFalloffRadius { get { return outerFalloffRadius; } }

        [SerializeField, Min(0f)]
        float innerFalloffRadius = 1f, innerRadius = 5f;
        public float InnerFalloffRadius { get { return innerFalloffRadius; } }
        public float InnerRadius { get { return innerRadius; } }

        float innerFalloffFactor, outerFalloffFactor;

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

        public override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                IsPlayerInGravity = true;

                CurrentPriorityWeight = 2;
                foreach (GravitySource gs in GravityService.GetActiveGravitySources())
                    if (gs is GravityPlane)
                    {
                    }
                    else if (gs != this)
                        gs.CurrentPriorityWeight = 1;
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);


            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                CurrentPriorityWeight = 1;
            }
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            if (CurrentPriorityWeight < GravityService.GetMaxPriorityWeight())
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
        }
    }
}