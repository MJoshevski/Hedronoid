﻿using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh linear lock on shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Linear Shot (Lock On)")]
public class UbhLinearLockOnShot : UbhLinearShot
{
    [Header("===== LinearLockOnShot Settings =====")]
    // "Always aim to target."
    [FormerlySerializedAs("_Aiming")]
    public bool m_aiming;

    /// <summary>
    /// is lock on shot flag.
    /// </summary>
    public override bool lockOnShot { get { return true; } }

    public override void Shot()
    {
        AimTarget();

        if (m_targetTransform)
            base.Shot();
    }

    protected override void Update()
    {
        if (m_shooting && m_aiming)
        {
            AimTarget();
        }

        base.Update();
    }

    private void AimTarget()
    {
        if (m_targetTransform != null)
        {
            Quaternion rot = Quaternion.LookRotation((m_targetTransform.position - m_bulletOrigin.position), transform.up);
            m_verticalAngle = rot.eulerAngles.x;
            m_horizontalAngle = rot.eulerAngles.y;
        }
    }
}