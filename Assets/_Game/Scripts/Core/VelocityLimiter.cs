using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VelocityLimiter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float Magnitude;

    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;

    void FixedUpdate()
    {
        var magnitude = Rigidbody.velocity.magnitude;
        _trigger = magnitude > Magnitude;
        Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, Magnitude);
    }

    void OnDrawGizmos()
    {
        if (_trigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, .5f);
        }
    }

    bool _trigger;
}
