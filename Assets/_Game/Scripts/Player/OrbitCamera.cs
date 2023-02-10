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
    public class OrbitCamera : HNDGameObject, IGameplaySceneContextInjector
    {
        #region PUBLIC/VISIBLE VARS
        public GameplaySceneContext GameplaySceneContext { get; set; }

        [HideInInspector]
        public Transform focus;
        [HideInInspector]
        public Camera orbitCamera;

        [Header("General Behaviour")]
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
        [SerializeField, Min(0f)]
        float shoulderFocusRadius = 5f;

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

        // CURSOR
        [Header("Cursor Settings")]
        public bool LockCursor;

        [Header("Manual Offsets")]
        [SerializeField]
        private Vector2 manualPositionOffset = Vector3.zero;

        [Header("Camera Shake Presets")]
        public CameraShakeCollection camerShakeCollection;
        public CameraShaker cameraShaker;
        #endregion

        #region PRIVATE/HIDDEN VARS
        private PlayerFSM Player;
        private Vector3 focusPoint, previousFocusPoint;
        private Vector2 orbitAngles = new Vector2(45f, 0f);
        private float lastManualRotationTime;
        private bool m_playerCreatedAndInitialized = false;

        private Quaternion gravityAlignment = Quaternion.identity;
        private Quaternion orbitRotation;
        private Quaternion lookRotation;
        public Quaternion LookRotation { get { return lookRotation; } }

        private Vector3 lookPosition;
        public Vector3 LookPosition {  get { return lookPosition; } }
        private Vector3 lookDirection;

        // RAYCASTING
        private RaycastHit prevHitPoint;

        private PlayerActionSet playerAction;


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

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            HNDEvents.Instance.AddListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
            HNDEvents.Instance.AddListener<DebugMenuOpened>(OnDebugMenuOpened);
            HNDEvents.Instance.AddListener<DebugMenuClosed>(OnDebugMenuClosed);

            gravityAlignment = Quaternion.identity;
            orbitCamera = GetComponent<Camera>();
            orbitCamera.transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);

            playerAction = InputManager.Instance.PlayerActions;

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
            if (!m_playerCreatedAndInitialized) return;

            OnValidate();
            UpdateGravityAlignment();
            UpdateFocusPoint();
            ManualRotation();

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

            float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            float maxAngle = upAlignmentSpeed * Time.deltaTime;

            Quaternion upAlignment =
                Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

            if (angle <= maxAngle)
            {
                gravityAlignment = upAlignment;
            }
            else
            {
                gravityAlignment = Quaternion.SlerpUnclamped(
                    gravityAlignment, upAlignment, maxAngle / angle
                );
            }
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;

            if (playerAction.SwitchShoulder.WasPressed)
            {
                manualPositionOffset.x *= -1;
            }

            UpdateShoulderPosition(targetPoint);
        }

        void UpdateShoulderPosition(Vector3 targetPoint)
        {
            Vector3 alignedOffsetVector = lookRotation * manualPositionOffset;
            Vector3 targetPointOffset = targetPoint + alignedOffsetVector;

            if (shoulderFocusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                float t = 1f;

                if (distance > 0.01f && shoulderCentering > 0f)
                    t = Mathf.Pow(1f - shoulderCentering, Time.unscaledDeltaTime);

                if (distance > shoulderFocusRadius)
                    t = Mathf.Min(t, shoulderFocusRadius / distance);

                focusPoint = Vector3.Lerp(targetPointOffset, focusPoint, t);
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

        void ManualRotation()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            float x = playerAction.Look.X;
            float y = playerAction.Look.Y;

            float horizontalAngle = x * InputManager.Instance.MouseHorizontalSensitivity;
            float verticalAngle = -y * InputManager.Instance.MouseVerticalSensitivity;

            orbitAngles += Time.fixedUnscaledDeltaTime * new Vector2(verticalAngle, horizontalAngle);
            prevHitPoint.point = Vector3.zero;

            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
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
