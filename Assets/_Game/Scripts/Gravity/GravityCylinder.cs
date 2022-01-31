using Hedronoid.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using Gizmos = Popcron.Gizmos;
namespace Hedronoid
{
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

        [Header("Inner")]
        [SerializeField, Min(0f)]
        [Tooltip("Inner falloff cylinder radius. Color = Cyan")]
        float innerFalloffRadius = 1f;

        [SerializeField, Min(0f)]
        [Tooltip("Inner cylinder radius. Color = Yellow")]
        float innerRadius = 5f;

        [Header("Outer")]
        [SerializeField, Min(0f)]
        [Tooltip("Outer cylinder radius. Color = Yellow")]
        float outerRadius = 10f;

        [SerializeField, Min(0f)]
        [Tooltip("Outer falloff cylinder radius. Color = Cyan")]
        float outerFalloffRadius = 15f;

        float innerFalloffFactor, outerFalloffFactor;
        float originalRadius, originalBoundaryHeight;
        public float ResizedRadius { get { return resizedRadius; } }
        public float ResizedBoundaryHeight { get { return resizedBoundaryHeight; } }

        [Header("Resized")]
        [SerializeField, Min(0f)]
        [Tooltip("Resized cylinder radius. Color = Black")]
        float resizedRadius;
        [SerializeField, Min(0f)]
        [Tooltip("Resized cylinder height. Color = Black")]
        float resizedBoundaryHeight;

        // BOUNDS
        [HideInInspector]
        public CapsuleCollider boundsCollider;

        private bool hasEntered = false, hasExited = false;

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

        protected override void Awake()
        {
            base.Awake();

            originalRadius = boundsCollider.radius;
            originalBoundaryHeight = boundsCollider.height;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }
        public override void OnTriggerStay(Collider other)
        {
            if (!IsInLayerMask(other)) return;

            if ((other.gameObject.layer & (1 << HNDAI.Settings.PlayerLayer)) > 0)
            {
                //Debug.LogErrorFormat("HEIGHT: {0}, BELOW: {1}, PLAYER POS Y: {2}",
                //    transform.position.y + (boundaryHeight / 2f), transform.position.y - (boundaryHeight / 2f),
                //    other.gameObject.transform.position.y);

                if ((transform.position.y + (boundaryHeight / 2f) >= other.gameObject.transform.position.y &&
                transform.position.y - (boundaryHeight / 2f) <= other.gameObject.transform.position.y) &&
                !hasEntered)
                {
                    hasEntered = true;
                    IsPlayerInGravity = true;

                    GravitySource grSrc = other.gameObject.GetComponent<GravitySource>();
                    if (grSrc && !OverlappingSources.Contains(grSrc))
                        OverlappingSources.Add(grSrc);

                    if (ResizeColliderOnEnter)
                    {
                        if (AutomaticColliderSize) AutomaticColliderSize = false;
                        ResizeColliderBounds(true);

                        foreach (GravitySource gs in OverlappingSources)
                            if (gs.ResizeColliderOnEnter)
                                gs.ResizeColliderBounds(false);
                    }

                }
                else if ((transform.position.y + (boundaryHeight / 2f) < other.gameObject.transform.position.y ||
                transform.position.y - (boundaryHeight / 2f) > other.gameObject.transform.position.y) &&
                hasEntered)       
                {
                    hasEntered = false;
                    IsPlayerInGravity = false;

                    GravitySource grSrc = other.gameObject.GetComponent<GravitySource>();
                    if (grSrc && OverlappingSources.Contains(grSrc))
                        OverlappingSources.Remove(grSrc);

                    if (ResizeColliderOnEnter)
                    {
                        if (AutomaticColliderSize) 
                            AutomaticColliderSize = false;

                        ResizeColliderBounds(false);
                    }
                }
            }
        }
        public override void OnTriggerExit(Collider other)
        {
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
            base.ResizeColliderBounds(shouldResize);

            if (shouldResize)
            {
                boundsCollider.radius = resizedRadius;
                boundsCollider.height = resizedBoundaryHeight;
                boundsCollider.center = Vector3.zero;
            }
            else
            {
                boundsCollider.radius = originalRadius;
                boundsCollider.height = originalBoundaryHeight;
                boundsCollider.center = Vector3.zero;
            }
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

            if (ResizeColliderOnEnter)
            {
                // Draw resized collider cylinder: BLACK
                DrawWireCylinder(
                   transform.position,
                   transform.rotation,
                   resizedBoundaryHeight,
                   resizedRadius,
                   Color.black);
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