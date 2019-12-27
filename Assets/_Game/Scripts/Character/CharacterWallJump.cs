using System.Collections;
using UnityEngine;
using Hedronoid;

public class CharacterWallJump : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] CharacterWallRun CharacterWallRun;

    [Header("Settings")]
    [SerializeField] CharacterDashSettings JumpSettingsForInput;
    [SerializeField] PhysicalForceSettings WallJumpForce;

    void Start()
    {
        _playerAction = JumpSettingsForInput.GetPlayerAction();
    }

    void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.contacts[i];
            var dot = Vector3.Dot(GravityService.Instance.GravityUp, contact.normal);
            if (Mathf.Approximately(0f, dot))
            {
                _collisionNormal = contact.normal;
                return;
            }
        }
    }

    void FixedUpdate()
    {
        if (!_playerAction.WasPressed)
            return;
        if (!CharacterWallRun.WallRunning)
            return;

        Debug.LogFormat("[{0}] Activated ", GetType().Name);

        var forceDirection = Quaternion.FromToRotation(Vector3.forward, _collisionNormal)
            * GravityService.Instance.GravityRotation
            * WallJumpForce.Direction;
        forceDirection.Normalize();
        StartCoroutine(Rigidbody.ApplyForceContinuously(forceDirection, WallJumpForce));
    }


    [Header("DebugView")]
    [SerializeField]
    Vector3 _collisionNormal;

    InControl.PlayerAction _playerAction;
}