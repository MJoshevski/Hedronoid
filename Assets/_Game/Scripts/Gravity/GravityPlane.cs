using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    public class GravityPlane : GravitySource
    {
        [Header("Gravity Variables")]
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField]
        Vector2 boundaryDistance = Vector2.one;

        [SerializeField, Min(0f)]
        float outerDistance = 0f, outerFalloffDistance = 0f;

        float outerFalloffFactor;

        // COLLIDER BOUNDS
        [HideInInspector]
        public BoxCollider boundsCollider;

        [Header("Resized")]
        [SerializeField]
        [Tooltip("Resized box dimensions. Color = Black")]
        private Vector3 resizedBounds;
        [SerializeField]
        [Tooltip("Resized collider center coordinates.")]
        private Vector3 resizedCenter = Vector3.zero;

        private Vector3 originalBounds;
        private Vector3 originalCenter;
        protected override void OnValidate()
        {
            base.OnValidate();
        
            boundaryDistance = Vector2.Max(boundaryDistance, Vector2.zero);
            outerFalloffDistance = Mathf.Max(outerFalloffDistance, outerDistance);
            outerFalloffFactor = 1f / (outerFalloffDistance - outerDistance);

            if (!boundsCollider)
                boundsCollider = GetComponent<BoxCollider>();

            boundsCollider.isTrigger = true;

            if (AutomaticColliderSize)
            {
                boundsCollider.size =
                    2 * new Vector3(boundaryDistance.x, outerFalloffDistance / 2f, boundaryDistance.y);
                boundsCollider.center = new Vector3(0, outerFalloffDistance / 2f, 0);
            }
        }
        protected override void Awake()
        {
            base.Awake();

            originalBounds = boundsCollider.size;
            originalCenter = boundsCollider.center;
        }
        public override Vector3 GetGravity(Vector3 position)
        {
            if (CurrentPriorityWeight < GravityService.GetMaxPriorityWeight() || !IsPlayerInGravity)
                return Vector3.zero;

            position =
               transform.InverseTransformDirection(position - transform.position);

            Vector3 vector = Vector3.zero;
            int outside = 0;

            if (position.x > boundaryDistance.x)
            {
                vector.x = boundaryDistance.x - position.x;
                outside = 1;
            }
            else if (position.x < -boundaryDistance.x)
            {
                vector.x = -boundaryDistance.x - position.x;
                outside = 1;
            }

            if (position.y > 0)
            {
                vector.y = -position.y;
                outside += 1;
            }
            else return Vector3.zero;

            if (position.z > boundaryDistance.y)
            {
                vector.z = boundaryDistance.y - position.z;
                outside += 1;
            }
            else if (position.z < -boundaryDistance.y)
            {
                vector.z = -boundaryDistance.y - position.z;
                outside += 1;
            }

            float distance = outside == 1 ?
                    Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;

            if (distance > outerFalloffDistance)
            {
                return Vector3.zero;
            }

            float g = gravity / distance;
            if (distance > outerDistance)
            {
                g *= 1f - (distance - outerDistance) * outerFalloffFactor;
            }

            return transform.TransformDirection(g * vector);
        }
        public override void ResizeColliderBounds(bool shouldResize)
        {
            base.ResizeColliderBounds(shouldResize);

            if (shouldResize)
            {
                boundsCollider.size = 2 * resizedBounds;
                boundsCollider.center = resizedCenter;
            }
            else
            {
                boundsCollider.size = originalBounds;
                boundsCollider.center = originalCenter;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix =
                Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                Vector3.zero, 
                2f * new Vector3(boundaryDistance.x, 0 ,boundaryDistance.y)
                );

            if (outerDistance > 0f)
            {
                Gizmos.color = Color.yellow;
                DrawGizmosOuterRect(outerDistance);
            }

            if (outerFalloffDistance > outerDistance)
            {
                Gizmos.color = Color.cyan;
                DrawGizmosOuterRect(outerFalloffDistance);
            }

            if (ResizeColliderOnEnter)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(resizedCenter, 2f * resizedBounds);
            }
        }

        void DrawGizmosOuterRect(float distance)
        {
            Vector3 a, b, c, d;
            Vector2 boundaryDistanceHalved = boundaryDistance / 2f;

            a.x = d.x = boundaryDistanceHalved.x;
            b.x = c.x = -boundaryDistanceHalved.x;
            a.z = b.z = boundaryDistanceHalved.y;
            c.z = d.z = -boundaryDistanceHalved.y;
            a.y = b.y = c.y = d.y = distance;

            DrawGizmosRect(a, b, c, d);
        }

        void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
#endif

    }
}