using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MDKShooter;
using UnityEngine;

public class CharacterDash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] LayerCollider Collider;

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

        float time = 0f;
        AnimationCurve powerOverTime = forceSettings.PowerOverTime;
        Keyframe lastKeyFrame = powerOverTime[powerOverTime.length - 1];
        while (time <= lastKeyFrame.time)
        {
            var forceDirection = transform.TransformDirection(forceSettings.Direction);
            forceDirection.Normalize();
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
