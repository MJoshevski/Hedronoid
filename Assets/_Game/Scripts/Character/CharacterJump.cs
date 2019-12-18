using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedronoid;
using UnityEngine;

public class CharacterJump : MonoBehaviour, IMoveDirectionDependent
{
    [Header("Scene Ref")]
    [SerializeField] CharacterWallRun CharacterWallRun;

    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] LayerCollider FeetCollider;

    [Header("Settings")]
    [DisplayScriptableObjectPropertiesAttribute]
    [SerializeField] CharacterDashSettings CharacterDashSettings;

    public Vector3 MoveDirection { get; set; }

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

        FeetCollider.CollisionEnter += (c) => _executions = 0;
    }


    protected virtual void FixedUpdate()
    {
        if (CharacterDashSettings.ContinuousInput
            && !_playerAction.IsPressed
            && _forceApplyCoroutine != null)
        {
            StopCoroutine(_forceApplyCoroutine);
            _forceApplyCoroutine = null;
            AfterApplyForce();
        }

        if (CharacterWallRun.WallRunning)
            return;

        if (!_playerAction.WasPressed)
            return;

        if (_executions >= CharacterDashSettings.ExecutionsBeforeReset)
            return;

        var forceSettings = CharacterDashSettings.PhysicalForce;
        _actionCoroutine = StartCoroutine(DoApplyForceOverTime());
    }

    IEnumerator DoApplyForceOverTime()
    {
        _executions++;
        var forceSettings = CharacterDashSettings.PhysicalForce;
        var gravityService = GravityService.Instance;

        var moveDirection = MoveDirection;
        if (moveDirection.sqrMagnitude < .25f)
            moveDirection = transform.forward;

        var forceDirection = Quaternion.FromToRotation(Vector3.forward, moveDirection)
            * GravityService.Instance.GravityRotation
            * forceSettings.Direction;
        forceDirection.Normalize();

        _forceApplyCoroutine = StartCoroutine(Rigidbody.ApplyForceContinuously(forceDirection, forceSettings));
        yield return _forceApplyCoroutine;

        AfterApplyForce();
    }

    void AfterApplyForce()
    {
        _forceApplyCoroutine = null;
    }

    Coroutine _forceApplyCoroutine = null;

    [SerializeField]
    int _executions = 0;
    InControl.PlayerAction _playerAction;
    Coroutine _actionCoroutine = null;
}