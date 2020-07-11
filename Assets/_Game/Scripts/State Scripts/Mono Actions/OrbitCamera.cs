using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Hedronoid
{
    public enum OrbitCameraTypes
    {
        SimpleCenterFollow = 0,
        ManualShoulderSwitch = 1
    }

    [CreateAssetMenu(menuName = "Actions/Camera/Orbit Camera")]
    public class OrbitCamera : Action
    {
        #region PUBLIC VARS
        [Header("References")]
        public TransformVariable focus;
        public TransformVariable cameraTransform;
        public TransformVariable pivotTransform;
        public CameraVariable camera;
        public FloatVariable delta, unscaledDelta;

        [Header("General Behaviour")]
        public OrbitCameraTypes cameraTypes = OrbitCameraTypes.ManualShoulderSwitch;
        [Tooltip("How fast the rig will adapt to the newly changed gravity?")]
        [SerializeField, Range(100f, 1000f)]
        private float m_GravityAdaptTurnSpeed = 500f;
        [Tooltip("How far away from the focus point should the camera be?")]
        [Range(1f, 20f)]
	    public float distance = 5f;
        [Tooltip("How fast should the camera align it's up vector to the player's?")]
        [Min(0f)]
        public float upAlignmentSpeed = 360f;
        [Tooltip("How fast should the camera follow the player?")]
        [Min(0f)]
        public float followSpeed = 9;
        [Tooltip("How fast should the camera handle rotations?")]
        [Min(0f), Range(1f, 360f)]
        public float rotationSpeed = 90f;

        [Header("Shoulder-focus Variables")]
        [Tooltip("How fast does the camera center to the shoulder focus when moving?")]
        [Range(0f, 1f)]
	    public float shoulderCentering = 0.5f;
        [Tooltip("How fast do we center on the newly switched shoulder?")]
        [Min(0f), Range(1f, 360f)]
        public float shoulderCenteringSpeed = 90f;
        [Range(0f, 90f)]
        public float shoulderAlignSmoothRange = 45f;
        [Tooltip("How fast does the camera catch-up to the shoulder focus when moving?")]
        [Range(0f, 1f)]
        public float catchupFactor = 0.2f;
        [Tooltip("How fast do we need to be going for the catch-up factor to take effect?")]
        [Range(0f, 100f)]
        public float catchupVeloThreshold = 10f;

        [Header("Camera obstructions")]
        [Range(-89f, 89f)]
        public float minVerticalAngle = -30f;
        [Range(-89f, 89f)]
        public float maxVerticalAngle = 60f;
        [Range(-89f, 89f)]
        public float minHorizontalAngle = 45f, maxHorizontalAngle = 75f;
        public LayerMask obstructionMask = -1;

        [Header("Automatic rotation")]
        [Min(0f)]
        public float alignDelay = 5f;
        [Range(0f, 90f)]
	    public float alignSmoothRange = 45f;

        [Header("Manual Offsets")]
        public Vector2 manualPositionOffset = Vector2.zero;
        public Vector3 manualRotationOffset = Vector3.zero;
        #endregion

        #region PRIVATE VARS
        private Vector3 focusPoint, previousFocusPoint;
        private Vector2 orbitAngles = new Vector2(45f, 0f);
        private float lastManualRotationTime;
        private float distanceThreshold;

        private Quaternion gravityAlignment = Quaternion.identity;
        private Quaternion orbitRotation;
        private Quaternion lookRotation;
        private Vector3 lookPosition;
        private Vector3 lookDirection;

        private IGravityService gravityService;

        // RAYCASTING
        private Vector3 currRay, prevRay;
        private Vector3 currHit, prevHitPoint;

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

        #endregion

        #region UNITY LIFECYCLE
        public override void Execute_Start()
        {
            focusPoint = focus.value.position;
            distanceThreshold = manualPositionOffset.x * 2f;
            gravityService = GravityService.Instance;
            gravityAlignment = Quaternion.identity;
            cameraTransform.value.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
        }

        public override void Execute()
        {
            OnValidate();
            UpdateGravityAlignment();
            UpdateFocusPoint();

            if (ManualRotation() /*|| AutomaticRotation()*/)
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }

            lookRotation = gravityAlignment * orbitRotation;
            lookDirection = lookRotation * Vector3.forward;
            lookPosition = focusPoint - lookDirection * distance;

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
       
            Quaternion manualRotation = Quaternion.Euler(manualRotationOffset);
            lookRotation *= manualRotation;

            cameraTransform.value.SetPositionAndRotation(lookPosition, lookRotation);
        }
        #endregion

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

            Gizmos.Cube(previousFocusPoint, Quaternion.identity, Vector3.one / 2f, Color.red);
            Gizmos.Cube(focusPoint, Quaternion.identity, Vector3.one / 2f, Color.magenta);
            Gizmos.Cube(targetPoint, Quaternion.identity, Vector3.one / 2f, Color.green);

            switch (cameraTypes)
            {
                case OrbitCameraTypes.SimpleCenterFollow:
                    focusPoint = targetPoint;
                    break;
                case OrbitCameraTypes.ManualShoulderSwitch:
                    if (Input.GetButtonDown("Fire3"))
                    {
                        distanceThreshold = 0.01f;
                        prevHitPoint = PlayerStateManager.Instance.RayHit.point;
                        prevRay = PlayerStateManager.Instance.charRay;
                        manualPositionOffset.x *= -1;
                    }

                    UpdateShoulderPosition(targetPoint);      
                    break;       
            }
        }

        void UpdateShoulderPosition(Vector3 targetPoint)
        {
            Vector3 alignedOffsetVector = lookRotation * manualPositionOffset;
            Vector3 targetPointOffset = targetPoint + alignedOffsetVector;

            Vector3 targetDirection = prevHitPoint - targetPointOffset;
            Gizmos.Line(targetPointOffset, prevHitPoint, Color.magenta);

            float distanceOffset =
                Vector3.Distance(targetPointOffset, focusPoint);            

            Gizmos.Cube(targetPointOffset, Quaternion.identity, Vector3.one / 2f, Color.blue);
            Gizmos.Cube(prevHitPoint, Quaternion.identity, Vector3.one * 4f, Color.black);

            if (distanceOffset > Mathf.Abs(distanceThreshold) && 
                shoulderCentering > 0f)
            {
                float movMag = PlayerStateManager.Instance.Rigidbody.velocity.sqrMagnitude;

                float factor = 1;
                if (movMag > catchupVeloThreshold)
                    factor = movMag * catchupFactor / 10f;
                 
                focusPoint = Vector3.Lerp(
                        targetPointOffset, focusPoint,
                        Mathf.Pow(1f - shoulderCentering,
                        unscaledDelta.value) / factor
                        );

                if (prevHitPoint != Vector3.zero)
                    AutomaticCentering();

                orbitRotation = Quaternion.Euler(orbitAngles);
            }
            else
            {
                focusPoint = targetPointOffset;
                distanceThreshold = manualPositionOffset.x * 2f;
            }
        }

        bool ManualRotation()
        {
            if (Time.timeScale < float.Epsilon)
                return false;

            var playerAction = InputManager.Instance.PlayerActions;

            float x = playerAction.Look.X;
            float y = playerAction.Look.Y;

            float horizontalAngle = x * delta.value * InputManager.Instance.MouseHorizontalSensitivity;
            float verticalAngle = -y * delta.value * InputManager.Instance.MouseVerticalSensitivity;

            const float e = 0.001f;
            if (horizontalAngle < -e || horizontalAngle > e || verticalAngle < -e || verticalAngle > e)
            {
                orbitAngles += rotationSpeed * unscaledDelta.value * new Vector2(verticalAngle, horizontalAngle);
                prevHitPoint = Vector3.zero;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        void AutomaticCentering ()
        {
            Vector3 hitPoint = PlayerStateManager.Instance.RayHit.point;
            currRay = PlayerStateManager.Instance.charRay;

            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * 
                (prevHitPoint - hitPoint);

            Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
                return;

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float deltaAbs =
                Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));

            float centeringSpeed = 
                shoulderCenteringSpeed / Vector3.Distance(focusPoint, hitPoint);

            float rotationChange = centeringSpeed *
                Mathf.Min(unscaledDelta.value, movementDeltaSqr);

            if (deltaAbs < shoulderAlignSmoothRange)
            {
                rotationChange *= deltaAbs / shoulderAlignSmoothRange;
            }
            else if (180f - deltaAbs < shoulderAlignSmoothRange)
            {
                rotationChange *= (180f - deltaAbs) / shoulderAlignSmoothRange;
            }

            orbitAngles.y =
                Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);                    
        }

        bool AutomaticRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < alignDelay)
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
                Mathf.Min(unscaledDelta.value, movementDeltaSqr);

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
    }
}
