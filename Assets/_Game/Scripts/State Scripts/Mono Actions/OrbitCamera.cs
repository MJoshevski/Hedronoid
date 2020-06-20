using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Camera/Orbit Camera")]
    public class OrbitCamera : Action
    {
        public TransformVariable focus;
        public TransformVariable cameraTransform;
        public TransformVariable pivotTransform;
        public CameraVariable camera;
        public FloatVariable delta, unscaledDelta;

        // How fast the rig will adapt to the newly changed gravity
        [SerializeField, Range(100f, 1000f)]
        private float m_GravityAdaptTurnSpeed = 500f;

        [Range(1f, 20f)]
	    public float distance = 5f;
        [Min(0f)]
        public float focusRadius = 1f;
        [Range(0f, 1f)]
	    public float focusCentering = 0.5f;
        [Min(0f)]
        public float upAlignmentSpeed = 360f;
        [Min(0f), Range(1f, 360f)]
	    public float rotationSpeed = 90f;
        [Min(0f)]
        public float followSpeed = 9;
        [Range(-89f, 89f)]
	    public float minVerticalAngle = -30f, maxVerticalAngle = 60f;
        [Range(-89f, 89f)]
        public float minHorizontalAngle = 45f, maxHorizontalAngle = 75f;
        [Min(0f)]
        public float alignDelay = 5f;
        [Range(0f, 90f)]
	    public float alignSmoothRange = 45f;
        public LayerMask obstructionMask = -1;

        [Header("Manual Offsets")]
        public Vector3 manualPositionOffset = Vector3.zero;
        public Vector3 manualRotationOffset = Vector3.zero;

        private Vector3 focusPoint, previousFocusPoint;
        private Vector2 orbitAngles = new Vector2(45f, 0f);
        private float lastManualRotationTime;

        private Quaternion gravityAlignment = Quaternion.identity;
        private Quaternion orbitRotation;

        private IGravityService gravityService;

        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y =
                    camera.value.nearClipPlane *
                    Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.value.fieldOfView);
                halfExtends.x = halfExtends.y * camera.value.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        public override void Execute_Start()
        {
            focusPoint = focus.value.position;
            gravityService = GravityService.Instance;
            gravityAlignment = Quaternion.identity;
            cameraTransform.value.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
        }

        public override void Execute()
        {
            OnValidate();
            UpdateGravityAlignment();
            UpdateFocusPoint();

            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }

            Quaternion lookRotation = gravityAlignment * orbitRotation;

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            //Camera collisions
            Vector3 rectOffset = lookDirection * camera.value.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focusPoint;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;

            if (Physics.BoxCast(
                castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
                lookRotation, castDistance - camera.value.nearClipPlane, obstructionMask, 
                QueryTriggerInteraction.Ignore))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }
            //

            lookPosition += manualPositionOffset;

            Quaternion manualRotation = Quaternion.Euler(manualRotationOffset);
            lookRotation *= manualRotation;

            cameraTransform.value.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateGravityAlignment()
        {
            Vector3 fromUp = gravityAlignment * Vector3.up;
            Vector3 toUp = GravityService.GetUpAxis(focusPoint);
            float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            float maxAngle = upAlignmentSpeed * Time.deltaTime;

            Quaternion newAlignment =
                Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

            if (angle <= maxAngle)
            {
                gravityAlignment = newAlignment;
            }
            else
            {
                gravityAlignment = Quaternion.SlerpUnclamped(
                    gravityAlignment, newAlignment, maxAngle / angle
                );
            }
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.value.position;

            if (focusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                if (distance > focusRadius)
                {
                    focusPoint = Vector3.Lerp(
                        targetPoint, focusPoint, focusRadius / distance
                    );
                }

                if (distance > 0.01f && focusCentering > 0f)
                {
                    focusPoint = Vector3.Lerp(
                        targetPoint, focusPoint,
                        Mathf.Pow(1f - focusCentering, unscaledDelta.value)
                    );
                }
            }
            else
            {
                focusPoint = targetPoint;
            }
        }

        bool ManualRotation()
        {
            if (Time.timeScale < float.Epsilon)
                return false;

            var playerAction = InputManager.Instance.PlayerActions;

            var x = playerAction.Look.X;
            var y = playerAction.Look.Y;

            var horizontalAngle = x * Time.deltaTime * InputManager.Instance.MouseHorizontalSensitivity;
            //cameraTransform.value.Rotate(new Vector3(0, horizontalAngle, 0), Space.Self);

            float verticalAngle = -y * Time.deltaTime * InputManager.Instance.MouseVerticalSensitivity;
            //pivotTransform.value.Rotate(new Vector3(verticalAngle, 0, 0), Space.Self);

            const float e = 0.001f;
            if (horizontalAngle < -e || horizontalAngle > e || verticalAngle < -e || verticalAngle > e)
            {
                orbitAngles += rotationSpeed * unscaledDelta.value * new Vector2(verticalAngle, horizontalAngle);
                lastManualRotationTime = unscaledDelta.value;
                return true;
            }

            //Vector2 input = new Vector2(
            //    Input.GetAxis("Vertical Camera"),
            //    Input.GetAxis("Horizontal Camera")
            //);
            //const float e = 0.001f;
            //if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            //{
            //    orbitAngles += rotationSpeed * unscaledDelta.value * input;
            //    lastManualRotationTime = unscaledDelta.value;
            //    return true;
            //}

            return false;
        }


        bool AutomaticRotation()
        {
            if (unscaledDelta.value - lastManualRotationTime < alignDelay)
            {
                return false;
            }

            Vector3 alignedDelta =
                Quaternion.Inverse(gravityAlignment) *
                (focusPoint - previousFocusPoint);

            Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed *
                Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

            if (deltaAbs < alignSmoothRange)
            {
                rotationChange *= deltaAbs / alignSmoothRange;
            }
            else if (180f - deltaAbs < alignSmoothRange)
            {
                rotationChange *= (180f - deltaAbs) / alignSmoothRange;
            }

            orbitAngles.y =
                Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

            return true;
        }

        void OnValidate()
        {
            if (maxVerticalAngle < minVerticalAngle)
            {
                maxVerticalAngle = minVerticalAngle;
            }
        }

        void ConstrainAngles()
        {
            orbitAngles.x =
                Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }

        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0f ? 360f - angle : angle;
        }

        //GLOBAL GRAVITY SYSTEM
        public void GravityRotation()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            var playerAction = InputManager.Instance.PlayerActions;

            var x = playerAction.Look.X;
            var y = playerAction.Look.Y;

            var horizontalAngle = x * Time.deltaTime * InputManager.Instance.MouseHorizontalSensitivity;
            cameraTransform.value.Rotate(new Vector3(0, horizontalAngle, 0), Space.Self);

            float verticalAngle = -y * Time.deltaTime * InputManager.Instance.MouseVerticalSensitivity;
            pivotTransform.value.Rotate(new Vector3(verticalAngle, 0, 0), Space.Self);

            //TODO: add clamping
            // var angles = m_Pivot.localRotation.eulerAngles;
            // clampedX = Mathf.Clamp(angles.x, m_TiltMin, m_TiltMax);
            // m_Pivot.localRotation = Quaternion.Euler(angles);

            if (cameraTransform.value.up != gravityService.GravityUp)
            {
                cameraTransform.value.rotation = Quaternion.RotateTowards(
                    cameraTransform.value.rotation,
                    gravityService.GravityRotation,
                    m_GravityAdaptTurnSpeed * Time.deltaTime);
            }
        }
    }
}
