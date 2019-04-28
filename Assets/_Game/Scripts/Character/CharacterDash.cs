using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MDKShooter;
using UnityEngine;

public class CharacterDash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CharacterController CharacterController;

    [Header("Settings")]
    [SerializeField] CharacterDashSettings CharacterDashSettings;

    void Start()
    {
        _playerAction = InputManager.Instance
            .PlayerActions
            .Actions
            .FirstOrDefault(a => a.Name == CharacterDashSettings.ActionName.Trim());
        if (_playerAction == null)
        {
            Debug.LogError("Could not find player action with name " + CharacterDashSettings.ActionName);
        }
    }

    void Update()
    {
        if (!_playerAction.WasPressed)
            return;

        if (CharacterDashSettings.ShouldBeGrounded && !CharacterController.IsGrounded)
            return;

        var gravityService = GravityService.Instance;
        var rigidBody = CharacterController.Rigidbody;

        var forceDirection = transform.TransformDirection(CharacterDashSettings.RelativeDirection);
        forceDirection.Normalize();

        Debug.DrawRay(transform.position, forceDirection, Color.green, 1f);
        rigidBody.AddForce(forceDirection * CharacterDashSettings.Power, ForceMode.Impulse);
    }

    InControl.PlayerAction _playerAction;
}
