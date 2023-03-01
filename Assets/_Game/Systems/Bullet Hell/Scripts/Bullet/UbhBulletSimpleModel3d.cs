using UnityEngine;

/// <summary>
/// Ubh bullet for sprite2d and rigidbody2d prefabs.
/// </summary>
public class UbhBulletSimpleModel3d : UbhBullet
{
    [SerializeField]
    private MeshRenderer[] m_meshRenderers;

    private bool m_isActive;

    /// <summary>
    /// Activate/Inactivate flag
    /// Override this property when you want to change the behavior at Active / Inactive.
    /// </summary>
    public override bool isActive { get { return m_isActive; } }

    /// <summary>
    /// Activate/Inactivate Bullet
    /// </summary>
    public override void SetActive(bool isActive)
    {
        m_isActive = isActive;

        if (m_meshRenderers != null && m_meshRenderers.Length > 0)
        {
            for (int i = 0; i < m_meshRenderers.Length; i++)
            {
                m_meshRenderers[i].enabled = isActive;
            }
        }
    }
}
