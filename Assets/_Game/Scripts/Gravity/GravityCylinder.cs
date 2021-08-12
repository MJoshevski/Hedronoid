using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
namespace Hedronoid
{
    public class GravityCylinder : GravitySource
    {
        [Header("Gravity Variables")]
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField, Min(0f)]
        float boundaryHeight = 6f, boundaryRadius = 4f;

        [SerializeField, Min(0f)]
        float innerFalloffRadius = 1f;

        [SerializeField, Min(0f)]
        float innerRadius = 5f;

        [SerializeField, Min(0f)]
        float outerRadius = 10f;

        [SerializeField, Min(0f)]
        float outerFalloffRadius = 15f;

        float innerFalloffFactor, outerFalloffFactor;

        // BOUNDS
        [HideInInspector]
        public CapsuleCollider boundsCollider;

        protected override void OnValidate()
        {
            base.OnValidate(); 

            boundaryHeight = Mathf.Max(boundaryHeight, 0f);
            boundaryRadius = Mathf.Max(boundaryRadius, 0f);

            innerRadius = Mathf.Min(innerRadius, boundaryRadius);
            innerFalloffRadius = Mathf.Min(innerFalloffRadius, innerRadius);
            
            outerRadius = Mathf.Max(outerRadius, boundaryRadius);
            outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

            innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
            outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);

            if (!boundsCollider)
                boundsCollider = GetComponent<CapsuleCollider>();

            boundsCollider.isTrigger = true;

            if (AutomaticColliderSize)
            {
                boundsCollider.radius = outerFalloffRadius;
                boundsCollider.height = boundaryHeight + (2f * outerFalloffRadius);
                boundsCollider.center = Vector3.zero;
            }
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            if (CurrentPriorityWeight < GravityService.GetMaxPriorityWeight())
                return Vector3.zero;

            Vector3 vector = transform.position - position;
            position =
               transform.InverseTransformDirection(position - transform.position);

            if (transform.up.y + (boundaryHeight / 2f) >= position.y &&
                transform.up.y - (boundaryHeight / 2f) <= position.y)
            {
                vector = new Vector3(transform.up.x, position.y, transform.up.z) -
                    position;
            }

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
            return transform.TransformDirection(g * vector);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Draw boundary cylinder: RED
            DrawWireCylinder(
                transform.position, 
                transform.rotation, 
                boundaryHeight, 
                boundaryRadius, 
                Color.red);

            // Draw inner falloff cylinder: CYAN
            DrawWireCylinder(
               transform.position,
               transform.rotation,
               boundaryHeight,
               innerFalloffRadius,
               Color.cyan);

            // Draw outer falloff cylinder: CYAN
            DrawWireCylinder(
               transform.position,
               transform.rotation,
               boundaryHeight,
               outerFalloffRadius,
               Color.cyan);

            // Draw inner cylinder: YELLOW
            DrawWireCylinder(
               transform.position,
               transform.rotation,
               boundaryHeight,
               innerRadius,
               Color.yellow);

            // Draw outer cylinder: YELLOW
            DrawWireCylinder(
               transform.position,
               transform.rotation,
               boundaryHeight,
               outerRadius,
               Color.yellow);

        }
        public static void DrawWireCylinder(Vector3 _pos, Quaternion _rot, float _height, float _radius, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                // Draw bottom unit circle
                List<Vector3> stack = GenerateUnitCircleVertices((-1 * _height)/2f, _radius);
                for (int i = 0; i < stack.Count - 1; i++)
                    Handles.DrawLine(stack[i], stack[i + 1]);

                // Draw top unit circle
                List<Vector3> stack2 = GenerateUnitCircleVertices(_height / 2f, _radius);
                for (int i = 0; i < stack2.Count - 1; i++)
                    Handles.DrawLine(stack2[i], stack2[i + 1]);

                // Connect lines between unit circle segment points
                for (int i = 0; i < stack.Count - 1; i++)
                    Handles.DrawLine(stack[i], stack2[i]);
            }
        }
#endif

        // generate a unit circle on XY-plane
        public static List<Vector3> GenerateUnitCircleVertices(float height, float radius)
        {
            float sectorCount = 42;
            float sectorStep = 2 * Mathf.PI / sectorCount;
            float sectorAngle;  //radians

            List<Vector3> unitCircleVertices = new List<Vector3>();

            for (int i = 0; i <= sectorCount; i++)
            {
                sectorAngle = i * sectorStep;
                Vector3 point = new Vector3(
                    radius * Mathf.Cos(sectorAngle),
                    height,
                    radius * Mathf.Sin(sectorAngle));

                unitCircleVertices.Add(point);
            }
            return unitCircleVertices;
        }
    }
}