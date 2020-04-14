using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Hedronoid;

public class CharacterController : MonoSingleton<CharacterController>, IMoveDirectionDependent
{
    [Header("Refs")]
    [SerializeField] LayerCollider FeetCollider;
    [SerializeField] Rigidbody Rigidbody;

    [Header("Settings")]
    [DisplayScriptableObjectPropertiesAttribute]
    [SerializeField] CharacterMoveSettings CharacterMoveSettings;
    [SerializeField] private float m_GravityRotationMultiplier = 5f;
    [SerializeField] bool _isGrounded;
    public bool IsGrounded { get { return _isGrounded; } set { _isGrounded = value; } }
    public Vector3 MoveDirection { get; set; }

    void FixedUpdate()
    {
        CheckGroundStatus();
        RotateTowardMovement();
        MoveForward();

        var gravityService = GravityService.Instance;

        //Apply adequate rotation
        if (Rigidbody.transform.up != gravityService.GravityUp)
        {
            Rigidbody.rotation = Quaternion.Slerp(
               Rigidbody.rotation,
               gravityService.GravityRotation,
               m_GravityRotationMultiplier * Time.deltaTime);
        }
    }


    void RotateTowardMovement()
    {
        if (MoveDirection.sqrMagnitude < .25f)
            return;
        var targetRotation = Quaternion.LookRotation(MoveDirection, GravityService.Instance.GravityUp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, CharacterMoveSettings.TurnSpeedMultiplier * Time.deltaTime);
    }

    void MoveForward()
    {
        var targetVelocity = MoveDirection * CharacterMoveSettings.MoveSpeedMultiplier;
        _movementVelocity = Vector3.Lerp(_movementVelocity, targetVelocity, Time.deltaTime * CharacterMoveSettings.MoveVeloctiyChangeRate);

        transform.position += _movementVelocity * Time.deltaTime;
    }

    void CheckGroundStatus()
    {
        IsGrounded = FeetCollider.IsColliding();
    }

    Vector3 _movementVelocity;
}
