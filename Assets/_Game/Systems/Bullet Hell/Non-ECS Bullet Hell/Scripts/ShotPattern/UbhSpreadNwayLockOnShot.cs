using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh spread nway lock on shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/Spread nWay Shot (Lock On)")]
public class UbhSpreadNwayLockOnShot : UbhSpreadNwayShot
{
    public override bool lockOnShot { get { return true; } }

    public override void Shot()
    {
        AimTarget();

        if (m_targetTransform)
            base.Shot();
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