using System;
using MDKShooter;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


[RequireComponent(typeof(CharacterController))]
public class CharacterInput : MonoBehaviour
{
    void Start()
    {
        // get the transform of the main camera
        if (UnityEngine.Camera.main != null)
        {
            _camera = UnityEngine.Camera.main.transform;
        }
        else
        {
            Debug.LogError("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
        }

        // get the third person character ( this should never be null due to require component )
        _character = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        var playerActions = InputManager.Instance.PlayerActions;
        // read inputs
        float h = playerActions.Move.X;
        float v = playerActions.Move.Y;

        // calculate move direction to pass to character
        if (_camera != null)
        {
            // calculate camera relative direction to move:
            _camForward = Vector3.Scale(_camera.forward, new Vector3(1, 0, 1)).normalized;
            _move = v * _camForward + h * _camera.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            _move = v * Vector3.forward + h * Vector3.right;
        }
        // #if !MOBILE_INPUT
        //         // walk speed multiplier
        //         if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
        // #endif

        // bool crouch = Input.GetKey(KeyCode.C);
        bool crouch = false;
        // pass all parameters to the character control script
        _character.Move(_move, crouch);
    }

    CharacterController _character; // A reference to the ThirdPersonCharacter on the object
    Transform _camera;
    Vector3 _camForward;             // The current forward direction of the camera
    Vector3 _move;
}