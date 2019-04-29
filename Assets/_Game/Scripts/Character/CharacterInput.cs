using System;
using MDKShooter;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


[RequireComponent(typeof(CharacterController))]
public class CharacterInput : MonoBehaviour
{
    void Start()
    {
        if (UnityEngine.Camera.main != null)
        {
            _camera = UnityEngine.Camera.main.transform;
        }
        else
        {
            Debug.LogError("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
        }
        _character = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        var playerActions = InputManager.Instance.PlayerActions;
        float h = playerActions.Move.X;
        float v = playerActions.Move.Y;

        var moveDirection = v * _camera.forward + h * _camera.right;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, GravityService.Instance.GravityUp);
        moveDirection.Normalize();

        Debug.DrawRay(transform.position, moveDirection, Color.yellow);

        _character.MoveDirection = moveDirection;
    }

    CharacterController _character;
    Transform _camera;
}