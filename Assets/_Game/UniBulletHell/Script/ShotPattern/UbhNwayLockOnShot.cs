using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Ubh nway lock on shot.
/// </summary>
[AddComponentMenu("UniBulletHell/Shot Pattern/nWay Shot (Lock On)")]
public class UbhNwayLockOnShot : UbhNwayShot
{
    [Header("===== NwayLockOnShot Settings =====")]
    // "Set a target with tag name."
    [FormerlySerializedAs("_SetTargetFromTag")]
    public bool m_setTargetFromTag = true;
    // "Set a unique tag name of target at using SetTargetFromTag."
    [FormerlySerializedAs("_TargetTagName"), UbhConditionalHide("m_setTargetFromTag")]
    public string m_targetTagName = "Player";
    // "Flag to randomly select from GameObjects of the same tag."
    public bool m_randomSelectTagTarget;
    // "Transform of lock on target."
    // "It is not necessary if you want to specify target in tag."
    // "Overwrite CenterAngle in direction of target to Transform.position."
    [FormerlySerializedAs("_TargetTransform")]
    public Transform m_targetTransform;
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
        if (m_targetTransform == null && m_setTargetFromTag)
        {
            m_targetTransform = UbhUtil.GetTransformFromTagName(m_targetTagName, m_randomSelectTagTarget);
        }
        if (m_targetTransform != null)
        {
            Quaternion rot = Quaternion.LookRotation((m_targetTransform.position - m_bulletOrigin.position), transform.up);
            m_verticalAngle = rot.eulerAngles.x;
            m_horizontalAngle = rot.eulerAngles.y;
        }
    }
}