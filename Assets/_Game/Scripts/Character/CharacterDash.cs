using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MDKShooter;
using UnityEngine;

public class CharacterDash : MonoBehaviour, IMoveDirectionDependent
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] LayerCollider Collider;

    [Header("Settings")]
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

        if (Collider != null)
        {
            Collider.CollisionEnter += OnCollision;
        }
    }

    void OnDestroy()
    {
        if (Collider != null)
        {
            Collider.CollisionEnter -= OnCollision;
        }
    }

    void FixedUpdate()
    {
        if (!_playerAction.WasPressed)
            return;

        if (_executions >= CharacterDashSettings.ExecutionsBeforeReset)
            return;

        var forceSettings = CharacterDashSettings.PhysicalForce;
        _actionCoroutine = StartCoroutine(DoApplyForceOverTime());
    }

    void OnCollision(Collider other)
    {
        _executions = 0;
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

        float time = 0f;
        AnimationCurve powerOverTime = forceSettings.PowerOverTime;
        Keyframe lastKeyFrame = powerOverTime[powerOverTime.length - 1];
        while (time <= lastKeyFrame.time
            && (!CharacterDashSettings.ContinuousInput || (CharacterDashSettings.ContinuousInput && _playerAction.IsPressed)))
        {
            float power = powerOverTime.Evaluate(time);

            Debug.DrawRay(transform.position, forceDirection, Color.green);

            Rigidbody.AddForce(forceDirection * power, forceSettings.ForceMode);

            yield return new WaitForFixedUpdate();

            time += Time.fixedDeltaTime;
        }

        if (!Collider)
        {
            _executions--;
        }
    }


    int _executions = 0;
    InControl.PlayerAction _playerAction;
    Coroutine _actionCoroutine = null;
}
