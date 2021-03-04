using Hedronoid;
using Hedronoid.AI;
using Hedronoid.Health;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBase : HealthBase
{
    private Vector3 upAxis, rightAxis;

    protected virtual void FixedUpdate()
    {
        upAxis = GravityService.GetUpAxis(transform.position);
        rightAxis = GravityService.GetRightAxis(transform.position);
    }

    protected override void UpdateHealthBarOrientation()
    {
        if (!m_healthBar) return;
        m_healthBar.position = m_rootTransform.position + (upAxis * textOffset.y) + (rightAxis * textOffset.x);
        m_healthBar.position = Vector3.MoveTowards(m_healthBar.transform.position, Camera.main.transform.position, textOffset.z);
        m_healthBar.LookAt(2 * m_healthBar.position - Camera.main.transform.position, upAxis);
    }
}
