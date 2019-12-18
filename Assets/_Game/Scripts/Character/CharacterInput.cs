using System;
using Hedronoid;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CharacterInput : MonoBehaviour
{
    [SerializeField] Transform Camera;

    void Start()
    {
        if (Camera == null)
        {
            Debug.LogError("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
        }
    }

    void FixedUpdate()
    {
        var playerActions = InputManager.Instance.PlayerActions;
        float h = playerActions.Move.X;
        float v = playerActions.Move.Y;

        var moveDirection = v * Camera.forward + h * Camera.right;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, GravityService.Instance.GravityUp);
        moveDirection.Normalize();

        Debug.DrawRay(transform.position, moveDirection, Color.yellow);

        var moveDirectionDependentComponents = GetComponents<IMoveDirectionDependent>();
        for (int i = 0; i < moveDirectionDependentComponents.Length; i++)
        {
            moveDirectionDependentComponents[i].MoveDirection = moveDirection;
        }
    }
}