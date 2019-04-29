using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityApplier : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody Rigidbody;

    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 1f;

    void FixedUpdate()
    {
        var gravityService = GravityService.Instance;
        Rigidbody.AddForce(gravityService.GravityUp * gravityService.Gravity * m_GravityMultiplier);
    }
}
