using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Hedronoid
{
    public enum OrbitCameraTypes
    {
        SimpleCenterFollow = 0,
        ManualShoulderSwitch = 1,
        AutomaticShoulderSwitch = 2,
        FocusPointOriented = 3
    }

    [CreateAssetMenu(menuName = "Actions/Camera/Orbit Camera")]
    public class OrbitCamera : Action
    {
        public TransformVariable focus;
        public TransformVariable cameraTransform;
        public TransformVariable pivotTransform;
        public CameraVariable camera;
        public FloatVariable delta, unscaledDelta;
        public OrbitCameraTypes cameraTypes = OrbitCameraTypes.ManualShoulderSwitch;

        // How fast the rig will adapt to the newly changed gravity
        [SerializeField, Range(100f, 1000f)]
        private float m_GravityAdaptTurnSpeed = 500f;

        [Range(1f, 20f)]
	    public float distance = 5f;
        [Min(0f)]
        public float focusXYRadius = 1f;
        [Min(0f)]
        public float focusZRadius = 0.2f;
        [Header("Shoulder-focus Variables")]
        [Tooltip("How fast does the camera center to the shoulder focus when moving?")]
        [Range(0f, 1f)]
	    public float focusCentering = 0.5f;
        [Tooltip("How fast does the camera catch-up to the shoulder focus when moving?")]
        [Range(0f, 1f)]
        public float catchupFactor = 0.2f;
        [Tooltip("How fast do we need to be going for the catch-up factor to take effect?")]
        [Range(0f, 100f)]
        public float catchupVeloThreshold = 10f;
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
        public Vector2 manualPositionOffset = Vector2.zero;
        public Vector3 manualRotationOffset = Vector3.zero;

        [SerializeField]
        private Vector3 focusPoint, previousFocusPoint;
        [SerializeField]
        private Vector2 orbitAngles = new Vector2(45f, 0f);
        private float lastManualRotationTime;

        private Quaternion gravityAlignment = Quaternion.identity;
        [SerializeField]
        private Quaternion orbitRotation;
        [SerializeField]
        private Quaternion lookRotation;
        [SerializeField]
        private Vector3 lookPosition;
        [SerializeField]
        private Vector3 lookDirection;


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

            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                prevHitPoint = Vector3.zero;
                //orbitRotation = Quaternion.Euler(orbitAngles);
            }

            orbitRotation = Quaternion.Euler(orbitAngles);
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
                        manualPositionOffset.x *= -1;
                        prevHitPoint = PlayerStateManager.Instance.RayHit.point;
                        prevRay = PlayerStateManager.Instance.LookRay;
                    }

                    UpdateShoulderPosition(targetPoint);      
                    break;

                case OrbitCameraTypes.AutomaticShoulderSwitch:
                    manualPositionOffset.x = 0;
                    AutomaticShoulderSwitching(targetPoint);
                    break;

                case OrbitCameraTypes.FocusPointOriented:
                    manualPositionOffset.x = 0;
                    FocusPointOrientation(targetPoint);
                    break;           
            }
        }
        //[SerializeField]
        private float distanceThreshold = 0;
        //[SerializeField]
        private float distanceOffset;
        //[SerializeField]
        private float movMag;
        [SerializeField]
        private Vector3 targetPointOffset;
        [SerializeField]
        Vector3 prevHitPoint;
        [SerializeField]
        Vector3 hitPoint;
        Ray prevRay, currRay;

        void UpdateShoulderPosition(Vector3 targetPoint)
        {
            Vector3 alignedOffsetVector = lookRotation * manualPositionOffset;
            targetPointOffset = targetPoint + alignedOffsetVector;

            Vector3 targetDirection = prevHitPoint - targetPointOffset;
            Gizmos.Line(targetPointOffset, prevHitPoint, Color.magenta);

            distanceOffset =
                Vector3.Distance(targetPointOffset, focusPoint);            

            Gizmos.Cube(targetPointOffset, Quaternion.identity, Vector3.one / 2f, Color.blue);
            Gizmos.Cube(prevHitPoint, Quaternion.identity, Vector3.one * 4f, Color.black);

            if (distanceOffset > Mathf.Abs(distanceThreshold) && 
                focusCentering > 0f)
            {
                movMag = PlayerStateManager.Instance.Rigidbody.velocity.sqrMagnitude;

                float factor = 1;
                if (movMag > catchupVeloThreshold)
                    factor = movMag * catchupFactor / 10f;
                 
                focusPoint = Vector3.Lerp(
                        targetPointOffset, focusPoint,
                        Mathf.Pow(1f - focusCentering,
                        unscaledDelta.value) / factor
                        );

                AutomaticCentering();
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
                lastManualRotationTime = unscaledDelta.value;
                return true;
            }

            return false;
        }

        void AutomaticCentering ()
        {
            hitPoint = PlayerStateManager.Instance.RayHit.point;

            if (prevHitPoint != Vector3.zero)
            {
                Vector3 dir = (prevHitPoint - hitPoint).normalized;
                float distance = Vector3.Distance(hitPoint, prevHitPoint);
                dir.z = 0;

                if (distance > 1f)
                {
                    float headingAngle = GetAngle(new Vector2(dir.x, dir.y));
                    float deltaAbs =
                        Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
                    float rotationChange = rotationSpeed * unscaledDelta.value;

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
                }
                //else prevHitPoint = Vector3.zero;
            }
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

        public void AutomaticShoulderSwitching(Vector3 targetPoint)
        {
            Vector3 alignedOffsetVector = lookRotation * manualPositionOffset;
            Vector3 lookPosOffset = lookPosition + alignedOffsetVector;

            float distanceOffset =
                Vector3.Distance(targetPoint + alignedOffsetVector, focusPoint);

            if (distanceOffset > 0.01f)
                focusPoint =
                        Vector3.Lerp(targetPoint + alignedOffsetVector, focusPoint, 1f / distanceOffset);
            //else
            //    lookPosition = lookPositionOffset;
        }

        public void FocusPointOrientation(Vector3 targetPoint)
        {
            Vector3 alignedTargetPoint = lookRotation * targetPoint;
            Vector3 alignedFocusPoint = lookRotation * focusPoint;
            float alignedZDistance = Mathf.Abs(
                Mathf.Abs(alignedTargetPoint.z) - Mathf.Abs(alignedFocusPoint.z)
                );

            Vector2 alignedTargetXY = 
                new Vector2(alignedTargetPoint.x, alignedTargetPoint.y);
            Vector2 alignedFocusXY = 
                new Vector2(alignedFocusPoint.x, alignedFocusPoint.y);
            float alignedXYDistance = 
                Vector2.Distance(alignedTargetXY, alignedFocusXY);

            if (focusXYRadius > 0f && focusZRadius > 0f)
            {
                if (alignedZDistance >= focusZRadius &&
                    alignedXYDistance <= focusXYRadius)
                {
                    focusPoint = Quaternion.Inverse(lookRotation) * new Vector3(
                        alignedFocusPoint.x,
                        alignedFocusPoint.y,
                        Mathf.Lerp(alignedTargetPoint.z, alignedFocusPoint.z, focusZRadius / alignedZDistance));
                }
                else if (alignedZDistance >= focusZRadius &&
                    alignedXYDistance > focusXYRadius)
                {
                    focusPoint = Quaternion.Inverse(lookRotation) * new Vector3(
                       Mathf.Lerp(alignedTargetPoint.x, alignedFocusPoint.x, focusXYRadius / alignedXYDistance),
                       Mathf.Lerp(alignedTargetPoint.y, alignedFocusPoint.y, focusXYRadius / alignedXYDistance),
                       Mathf.Lerp(alignedTargetPoint.z, alignedFocusPoint.z, focusZRadius / alignedZDistance));
                }
                else if (alignedZDistance < focusZRadius &&
                    alignedXYDistance > focusXYRadius)
                {
                    focusPoint = Quaternion.Inverse(lookRotation) * new Vector3(
                       Mathf.Lerp(alignedTargetPoint.x, alignedFocusPoint.x, focusXYRadius / alignedXYDistance),
                       Mathf.Lerp(alignedTargetPoint.y, alignedFocusPoint.y, focusXYRadius / alignedXYDistance),
                       alignedFocusPoint.z);
                }
                Debug.LogErrorFormat("TP: {0}, FP: {1}", alignedTargetPoint.ToString(), alignedFocusPoint.ToString());
                
                // CENTERING: Keeping just in case we decide to have some of it
                //if (distance > 0.01f && focusCentering > 0f)
                //{
                //    focusPoint = Vector3.Lerp(
                //        targetPoint, focusPoint,
                //        Mathf.Pow(1f - focusCentering, unscaledDelta.value)
                //    );
                //}
            }
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
