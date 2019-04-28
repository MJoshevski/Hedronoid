using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursorDisableCamera();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            LockCursorEnableCamera();
        }

        bool zeroTimeScale = Mathf.Approximately(0, Time.timeScale);
        if (zeroTimeScale)
        {
            UnlockCursorDisableCamera();
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
