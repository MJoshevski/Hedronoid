using Hedronoid;
using UnityEngine;

public class CharacterWallRun : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;

    [Header("Settings")]
    [DisplayScriptableObjectPropertiesAttribute]
    [SerializeField] CharacterWallRunSettings Settings;

    public bool WallRunning { get { return _wallRunning; } private set { _wallRunning = value; } }

    void OnCollisionEnter(Collision collision)
    {
        CheckIfWallRunning(collision);
        if (WallRunning)
        {
            _runCounter = Settings.Duration;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        CheckIfWallRunning(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        WallRunning = false;
    }

    void FixedUpdate()
    {
        if (WallRunning)
        {
            _runCounter -= Time.fixedDeltaTime;

            if (_runCounter > 0f)
            {
                ApplyNegativeGravity();
            }
        }
    }

    void CheckIfWallRunning(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.contacts[i];
            var dot = Vector3.Dot(GravityService.Instance.GravityUp, contact.normal);
            if (Mathf.Approximately(0f, dot))
            {
                WallRunning = true;
            }
        }
    }

    void ApplyNegativeGravity()
    {
        var gravityService = GravityService.Instance;
        Rigidbody.ApplyForce(gravityService.GravityUp * gravityService.Gravity * Settings.GravityNegateMultiplier);
    }

    [Header("DebugView")]
    [SerializeField] float _runCounter = 0f;
    [SerializeField] bool _wallRunning;
}