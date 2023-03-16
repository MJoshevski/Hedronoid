using Hedronoid.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hedronoid
{
    [RequireComponent(typeof(MeshCollider))]
    public class GravityCylinder : GravitySource
    {
        [Header("Gravity Variables")]
        [SerializeField]
        float gravity = 9.81f;

        [Header("Boundary")]
        [SerializeField, Min(0f)]
        [Tooltip("Boundary cylinder height. Color = Red")]
        float boundaryHeight = 6f;
        [SerializeField, Min(0f)]
        [Tooltip("Boundary cylinder radius. Color = Red")]
        float boundaryRadius = 4f;
        [SerializeField]
        [Tooltip("Show boundary cylinder radius gizmo?")]
        bool showBoundaryRadiusGizmo = true;

        [Header("Inner")]
        [SerializeField, Min(0f)]
        [Tooltip("Inner falloff cylinder radius. Color = Cyan")]
        float innerFalloffRadius = 1f;
        [SerializeField]
        [Tooltip("Show inner falloff cylinder radius gizmo?")]
        bool showInnerFalloffRadiusGizmo = true;
        [SerializeField, Min(0f)]
        [Tooltip("Inner cylinder radius. Color = Yellow")]
        float innerRadius = 5f;
        [SerializeField]
        [Tooltip("Show inner cylinder radius gizmo?")]
        bool showInnerRadiusGizmo = true;

        [Header("Outer")]
        [SerializeField, Min(0f)]
        [Tooltip("Outer falloff cylinder radius. Color = Cyan")]
        float outerFalloffRadius = 15f;
        [SerializeField]
        [Tooltip("Show outer falloff cylinder radius gizmo?")]
        bool showOuterFalloffRadiusGizmo = true;
        [SerializeField, Min(0f)]
        [Tooltip("Outer cylinder radius. Color = Yellow")]
        float outerRadius = 10f;
        [SerializeField]
        [Tooltip("Show outer cylinder radius gizmo?")]
        bool showOuterRadiusGizmo = true;

        float innerFalloffFactor, outerFalloffFactor;

        [Header("Mesh Colliders")]
        [SerializeField, Min(1)]
        [Tooltip("Height of the mesh collider with no player in it.")]
        private int originalHeight = 2;
        [SerializeField, Min(1)]
        [Tooltip("Radius of the mesh collider with no player in it.")]
        private int originalRadius = 1;
        [SerializeField]
        [Tooltip("Amount of Z-positioning correction on the original collider.")]
        private float originalZCorrection = 0;
        [SerializeField, Min(1)]
        [Tooltip("Height of the resized cylinder once the player enters.")]
        private int resizedHeight = 2;
        [SerializeField, Min(1)]
        [Tooltip("Radius of the resized cylinder once the player enters.")]
        private int resizedRadius = 2;
        [SerializeField]
        [Tooltip("Amount of Z-positioning correction on the resized collider.")]
        private float resizedZCorrection = 0;


        // BOUNDS
        public MeshCollider originalCollider;
        public MeshCollider resizedCollider;
        [Tooltip("This acts as a button. Click it to rescan the gravity bounds to generate the mesh collider.")]
        public bool scanForColliders = false;

        private bool hasEntered = false;

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

            if (AutomaticColliderSize)
            {
                originalHeight = (int) boundaryHeight;
                originalRadius = (int) outerFalloffRadius;
            }

            if (scanForColliders)
            {
                Collider[] colliders = GetComponents<Collider>();

                for (int i = 0; i < colliders.Length; i++)
                {
                    Collider toBeDestroyed = colliders[i];


#if UNITY_EDITOR
                    if (toBeDestroyed != null)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            Undo.DestroyObjectImmediate(toBeDestroyed);
                        };
                    }
#endif
                }

                originalCollider = ProceduralPrimitives.GenerateCylinder(transform, originalRadius, 20, originalHeight, originalHeight / 2 + originalZCorrection);
                resizedCollider = ProceduralPrimitives.GenerateCylinder(transform, resizedRadius, 20, resizedHeight, resizedHeight /2 + resizedZCorrection);

                scanForColliders = false;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            ResizeColliderBounds(false);
        }
        public override Vector3 GetGravity(Vector3 position)
        {
            if (CurrentPriorityWeight < GravityService.GetMaxPriorityWeight() || !IsPlayerInGravity)
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
        public override void ResizeColliderBounds(bool shouldResize)
        {
            resizedCollider.enabled = shouldResize;
            originalCollider.enabled = !shouldResize;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (showBoundaryRadiusGizmo)
            {
                // Draw boundary cylinder: RED
                DrawWireCylinder(
                    transform.position,
                    transform.rotation,
                    boundaryHeight,
                    boundaryRadius,
                    Color.red);
            }

            if (showInnerFalloffRadiusGizmo)
            {
                // Draw inner falloff cylinder: CYAN
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   boundaryHeight,
                   innerFalloffRadius,
                   Color.cyan);
            }

            if (showInnerRadiusGizmo)
            {
                // Draw inner cylinder: YELLOW
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   boundaryHeight,
                   innerRadius,
                   Color.yellow);
            }

            if (showOuterFalloffRadiusGizmo)
            {
                // Draw outer falloff cylinder: CYAN
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   boundaryHeight,
                   outerFalloffRadius,
                   Color.cyan);
            }

            if (showOuterRadiusGizmo)
            {
                // Draw outer cylinder: YELLOW
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   boundaryHeight,
                   outerRadius,
                   Color.yellow);
            }

            if (ResizeColliderOnEnter)
            {
                // Draw resized collider cylinder: WHITE
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   resizedHeight,
                   resizedRadius,
                   Color.white);
            }
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