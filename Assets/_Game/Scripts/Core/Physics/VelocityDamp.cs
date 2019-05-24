using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityDamp : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;

    [Header("Settings")]
    [DisplayScriptableObjectPropertiesAttribute]
    [SerializeField] VelocityDampSettings Settings;

    void FixedUpdate()
    {
        var magnitude = Rigidbody.velocity.magnitude;
        _dampForce = Settings.SpeedDampingCurve.Evaluate(magnitude);
        if (_dampForce > 0.1f)
        {
            var oppositeForce = -Rigidbody.velocity;
            oppositeForce = Vector3.ProjectOnPlane(oppositeForce, GravityService.Instance.GravityUp);
            oppositeForce.Normalize();
            Rigidbody.ApplyForce(oppositeForce * _dampForce);
        }
    }



    [Header("DebugInfo")]
    [SerializeField] float _dampForce;
}
