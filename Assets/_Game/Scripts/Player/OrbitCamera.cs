using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
using Hedronoid.Core;
using Hedronoid.Player;
using Hedronoid.Events;
using CameraShake;

namespace Hedronoid
{
    public enum OrbitCameraTypes
    {
        SimpleCenterFollow = 0,
        ManualShoulderSwitch = 1
    }

    public class OrbitCamera : HNDGameObject, IGameplaySceneContextInjector
    {
        #region PUBLIC/VISIBLE VARS
        public GameplaySceneContext GameplaySceneContext { get; set; }

        [HideInInspector]
        public Transform focus;
        [HideInInspector]
        public Camera orbitCamera;

        [Header("General Behaviour")]
        public OrbitCameraTypes cameraTypes = OrbitCameraTypes.ManualShoulderSwitch;
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
        [Tooltip("At what point between the rotation of angles should we engage full speed smoothing?")]
        [Range(0f, 90f)]
        public float shoulderAlignSmoothRange = 45f;
        [Tooltip("How fast does the camera catch-up to the shoulder focus when moving?")]
        [Range(0f, 1f)]
        public float catchupFactor = 0.2f;
        [Tooltip("How fast do we need to be going for the catch-up factor to take effect?")]
        [Range(0f, 100f)]
        public float catchupVeloThreshold = 10f;
        [Tooltip("How much mouse/camera look intensity we need to apply to interrupt shoulder centering and return camera control to player?")]
        [Range(0.001f, 10f)]
        public float lookReleaseThreshold = 10f;

        [Header("Camera obstructions")]
        [Tooltip("Minimum angle for the vertical orbit rotation")]
        [Range(-89f, 89f)]
        public float minVerticalAngle = -30f;
        [Tooltip("Maximum angle for the vertical orbit rotation")]
        [Range(-89f, 89f)]
        public float maxVerticalAngle = 60f;
        [Tooltip("Minimum angle for the horizontal orbit rotation")]
        [Range(-89f, 89f)]
        public float minHorizontalAngle = 45f;
        [Tooltip("Maximum angle for the horizontal orbit rotation")]
        [Range(-89f, 89f)]
        public float maxHorizontalAngle = 75f;
        [Tooltip("Which layers obstruct the camera?")]
        public LayerMask obstructionMask = -1;

        [Header("Automatic rotation")]
        [Tooltip("How long (seconds) do we wait before automatic focus alignment kicks in?")]
        [Min(0f)]
        public float alignDelay = 5f;
        [Tooltip("At what point between the rotation of angles should we engage full speed smoothing?")]
        [Range(0f, 90f)]
	    public float alignSmoothRange = 45f;

        // CURSOR
        [Header("Cursor Settings")]
        public bool LockCursor;

        [Header("Manual Offsets")]
        [SerializeField]
        private Vector2 manualPositionOffset = Vector3.zero;
        public Vector2 ManualPositionOffset
        {
            get { return manualPositionOffset; }
            private set { manualPositionOffset = value; }
        }

        [Header("Camera Shake Presets")]
        public CameraShakeCollection camerShakeCollection;
        public CameraShaker cameraShaker;
        #endregion

        #region PRIVATE/HIDDEN VARS
        private PlayerFSM Player;
        private Vector3 focusPoint, previousFocusPoint;
        private Vector2 orbitAngles;
        private float lastManualRotationTime;
        private float distanceThreshold;
        private bool shoulderFocused = true;
        private bool m_playerCreatedAndInitialized = false;

        private Quaternion gravityAlignment = Quaternion.identity;
        private Quaternion orbitRotation;
        private Quaternion lookRotation;
        public Quaternion LookRotation { get { return lookRotation; } }

        private Vector3 lookPosition;
        public Vector3 LookPosition {  get { return lookPosition; } }
        private Vector3 lookDirection;
        private float minVerticalAnglePrev;
        private float maxVerticalAnglePrev;

        // RAYCASTING
        private RaycastHit prevHitPoint;

        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y =
                    orbitCamera.nearClipPlane *
                    Mathf.Tan(0.5f * Mathf.Deg2Rad * orbitCamera.fieldOfView);
                halfExtends.x = halfExtends.y * orbitCamera.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        #endregion

        #region UNITY LIFECYCLE

        void OnValidate()
        {
            if (maxVerticalAngle < minVerticalAngle)
            {
                maxVerticalAngle = minVerticalAngle;
            }
        }
        float timeStart;
        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            timeStart = Time.realtimeSinceStartup;
            HNDEvents.Instance.AddListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
            HNDEvents.Instance.AddListener<DebugMenuOpened>(OnDebugMenuOpened);
            HNDEvents.Instance.AddListener<DebugMenuClosed>(OnDebugMenuClosed);

            distanceThreshold = manualPositionOffset.x * 2f;
            gravityAlignment = Quaternion.identity;
            orbitCamera = GetComponent<Camera>();
            orbitCamera.transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
            minVerticalAnglePrev = minVerticalAngle;
            maxVerticalAnglePrev = maxVerticalAngle;

            if (!cameraShaker) TryGetComponent(out cameraShaker);
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
            HNDEvents.Instance.RemoveListener<DebugMenuOpened>(OnDebugMenuOpened);
            HNDEvents.Instance.RemoveListener<DebugMenuClosed>(OnDebugMenuClosed);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
            HNDEvents.Instance.RemoveListener<DebugMenuOpened>(OnDebugMenuOpened);
            HNDEvents.Instance.RemoveListener<DebugMenuClosed>(OnDebugMenuClosed);
        }

        public void LateUpdate()
        {
            if (Time.realtimeSinceStartup - timeStart < 1.5f)
                orbitAngles = new Vector2(45f, GameplaySceneContext.PlayerSpawner.m_SpawnPoints[0].rotation.eulerAngles.y);

            if (!m_playerCreatedAndInitialized) return;
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
            Vector3 rectOffset = lookDirection * orbitCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focusPoint;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;

            if (Physics.BoxCast(
                castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
                lookRotation, castDistance - orbitCamera.nearClipPlane, obstructionMask, 
                QueryTriggerInteraction.Ignore))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }
            //

            orbitCamera.transform.SetPositionAndRotation(lookPosition, lookRotation);

            cameraShaker.ProcessActiveShakes();
        }
        #endregion

        #region METHODS
        void UpdateGravityAlignment()
        {
            Vector3 fromUp = gravityAlignment * Vector3.up;
            Vector3 toUp = GravityService.GetUpAxis(focus.position);

            Vector3 fromForward = gravityAlignment * Vector3.forward;
            Vector3 toForward = GravityService.GetForwardAxis(focus.position);

            float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            float maxAngle = upAlignmentSpeed * Time.deltaTime;

            Quaternion upAlignment =
                Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

            Quaternion forwardAlignment =
                Quaternion.FromToRotation(fromForward, toForward) * gravityAlignment;

            if (angle <= maxAngle)
            {
                //gravityAlignment = forwardAlignment;
                gravityAlignment = upAlignment;
            }
            else
            {
                gravityAlignment = Quaternion.SlerpUnclamped(
                    gravityAlignment, upAlignment, maxAngle / angle
                );
                //gravityAlignment = Quaternion.SlerpUnclamped(
                //    gravityAlignment, forwardAlignment, maxAngleF / angleF
                //);
            }
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;

            //Gizmos.Cube(previousFocusPoint, Quaternion.identity, Vector3.one / 2f, Color.red);
            //Gizmos.Cube(focusPoint, Quaternion.identity, Vector3.one / 2f, Color.magenta);
            //Gizmos.Cube(targetPoint, Quaternion.identity, Vector3.one / 2f, Color.green);

            switch (cameraTypes)
            {
                case OrbitCameraTypes.SimpleCenterFollow:
                    focusPoint = targetPoint;
                    break;
                case OrbitCameraTypes.ManualShoulderSwitch:
                    if (Input.GetButtonDown("Fire3"))
                    {
                        distanceThreshold = 0.01f;
                        prevHitPoint = Player.RayHit;
                        shoulderFocused = false;
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

            Vector3 targetDirection = prevHitPoint.point - targetPointOffset;
            //Gizmos.Line(targetPointOffset, prevHitPoint.point, Color.magenta);

            float distanceOffset =
                Vector3.Distance(targetPointOffset, focusPoint);

            //Gizmos.Cube(targetPointOffset, Quaternion.identity, Vector3.one / 2f, Color.blue);
            //Gizmos.Cube(prevHitPoint.point, Quaternion.identity, Vector3.one * 4f, Color.black);

            if (distanceOffset > Mathf.Abs(distanceThreshold) && 
                shoulderCentering > 0f)
            {
                float movMag = Player.Rigidbody.velocity.sqrMagnitude;

                float factor = 1;
                if (movMag > catchupVeloThreshold)
                    factor = movMag * catchupFactor / 10f;
                 
                focusPoint = Vector3.Lerp(
                        targetPointOffset, focusPoint,
                        Mathf.Pow(1f - shoulderCentering,
                        Time.unscaledDeltaTime) / factor
                        );

                if (prevHitPoint.point != Vector3.zero)
                    AutomaticCentering();

                orbitRotation = Quaternion.Euler(orbitAngles);
            }
            else
            {
                focusPoint = targetPointOffset;
                shoulderFocused = true;
                distanceThreshold = manualPositionOffset.x * 2f;
            }
        }

        public void CursorLock()
        {
            bool zeroTimeScale = Mathf.Approximately(0, Time.timeScale);
            if (zeroTimeScale)
            {
                UnlockCursorDisableCamera();
            }
            else
            {
                LockCursorEnableCamera();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                LockCursorEnableCamera();
            }
            else
            {
                UnlockCursorDisableCamera();
            }
        }

        void LockCursorEnableCamera()
        {
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !LockCursor;
            //Camera.value.gameObject.SetActive(true);
        }

        void UnlockCursorDisableCamera()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //Camera.value.gameObject.SetActive(false);
        }

        bool ManualRotation()
        {
            if (Time.timeScale < float.Epsilon)
                return false;

            var playerAction = InputManager.Instance.PlayerActions;

            float x = playerAction.Look.X;
            float y = playerAction.Look.Y;

            float horizontalAngle = x * InputManager.Instance.MouseHorizontalSensitivity;
            float verticalAngle = -y * InputManager.Instance.MouseVerticalSensitivity;

            float e = lookReleaseThreshold;

            if (!shoulderFocused && 
                (horizontalAngle < -e || horizontalAngle > e || verticalAngle < -e || verticalAngle > e))
                shoulderFocused = true;

            if (!shoulderFocused) return false;

            orbitAngles += Time.fixedUnscaledDeltaTime * new Vector2(verticalAngle, horizontalAngle);
            lastManualRotationTime = Time.unscaledTime;
            prevHitPoint.point = Vector3.zero;
            return true;
        }

        void AutomaticCentering ()
        {
            // Change last hit ray magnitude to be identical to the previous hit ray
            Ray hitRay = Player.LookRay;
            Vector3 hitPoint = hitRay.GetPoint(prevHitPoint.distance);

            // Aligned delta/direction between the two hit points
            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * 
                (prevHitPoint.point - hitPoint);

            // Movement direction magnitude of XZ (horizontal) plane
            Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.000001f)
                return;

            // Angle from movement direction
            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

            // Delta/smallest angle required to rotate to reaching heading
            float deltaAbs =
                Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));

            // Scale centering speed with distance between the hit point and focus
            float focusHitDis = Vector3.Distance(focusPoint, prevHitPoint.point);

            float centeringSpeed;

            // If the focus-hitPoint distance is less than 0, than multiply factor to avoid
            // really fast rotations at close proximity
            if (focusHitDis > 1f)
                centeringSpeed = shoulderCenteringSpeed / focusHitDis;
            else centeringSpeed = shoulderCenteringSpeed * focusHitDis;

            // Rotation change is the product of the centering factor plus the minimum
            // between delta time and movement direction delta
            float rotationChange = centeringSpeed *
                Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

            // Proper smooth aligning according to a set threshold angle
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
        #endregion

        #region EVENT HANDLERS
        private void OnPlayerCreatedAndInitialized(PlayerCreatedAndInitialized e)
        {
            Player = GameplaySceneContext.Player;
            focus = Player.cachedGameObject.transform;
            focusPoint = Player.cachedGameObject.transform.position;

            m_playerCreatedAndInitialized = true;
        }

        private void OnDebugMenuOpened(DebugMenuOpened e)
        {
            LockCursor = false;
        }

        private void OnDebugMenuClosed(DebugMenuClosed e)
        {
            LockCursor = true;
        }
        #endregion
    }
}
