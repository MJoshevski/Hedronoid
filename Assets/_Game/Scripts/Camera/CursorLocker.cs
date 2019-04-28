using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MDKShooter;

public class CursorLocker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    bool LockCursor;

    [Header("Refs")]
    [SerializeField]
    ThirdPersonCamera Camera;

    void Start()
    {
        Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void Update()
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
        Camera.enabled = true;
    }

    void UnlockCursorDisableCamera()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Camera.enabled = false;
    }
}
