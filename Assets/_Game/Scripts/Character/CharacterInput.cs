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

        _move = v * _camera.forward + h * _camera.right;

        Debug.DrawRay(transform.position, _move, Color.yellow);

        _character.Move(_move, crouch: false);
    }

    CharacterController _character; // A reference to the ThirdPersonCharacter on the object
    Transform _camera;
    Vector3 _move;
}