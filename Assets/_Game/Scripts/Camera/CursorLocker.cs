using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Actions/Camera/Cursor Locker")]
    public class CursorLocker : Action
    {
        [Header("Settings")]
        [SerializeField]
        bool LockCursor;

        [Header("Refs")]
        [SerializeField]
        TransformVariable Camera;

        public override void Execute_Start()
        {
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public override void Execute()
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


        //TODO:(Matej) needs reimplementing using Actions
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
            Camera.value.gameObject.SetActive(true);
        }

        void UnlockCursorDisableCamera()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Camera.value.gameObject.SetActive(false);
        }
    }
}
