﻿using UnityEngine;

/// <summary>
/// Ubh tentacle bullet.
/// </summary>
[DisallowMultipleComponent]
public sealed class UbhTentacleBullet : MonoBehaviour
{
    // "Center and Original bullet object."
    public GameObject m_centerBullet;
    // "Number of tentacles."
    [Range(2, 360)]
    public int m_numOfTentacles = 3;
    // "Number of bullets for one tentacle."
    [Range(1, 50)]
    public int m_numOfBulletsForOneTentacle = 3;
    // "Distance of between bullets."
    public float m_distanceBetweenBullets = 0.5f;
    // "Enable or disable center bullet."
    public bool m_enableCenterBullet = true;
    // "Rotation Speed of tentacles."
    public float m_rotationSpeed = 90f;

    private Transform m_rootTransform;

    private void Awake()
    {
        m_rootTransform = new GameObject("Root").GetComponent<Transform>();
        m_rootTransform.SetParent(transform, false);

        if (m_numOfTentacles < 2)
        {
            Debug.LogWarning("NumOfTentacles need 2 or more.");
            return;
        }

        float addAngle = 360f / m_numOfTentacles;

        for (int i = 0; i < m_numOfTentacles; i++)
        {
            Quaternion quat = Quaternion.identity;

            quat = Quaternion.Euler(new Vector3(0f, addAngle * i, addAngle * i));


            for (int k = 0; k < m_numOfBulletsForOneTentacle; k++)
            {
                var transBullet = ((GameObject)Instantiate(m_centerBullet, m_rootTransform)).GetComponent<Transform>();
                
                transBullet.position += (quat * Vector3.up * ((k + 1) * m_distanceBetweenBullets));
                //XY
                //transBullet.position += (quat * Vector3.forward * ((k + 1) * m_distanceBetweenBullets));
            }
        }

        m_centerBullet.SetActive(m_enableCenterBullet);
    }

    /// <summary>
    /// Update Rotate
    /// </summary>
    public void UpdateRotate()
    {
        m_rootTransform.AddEulerAnglesZ(m_rotationSpeed * UbhTimer.instance.deltaTime);
        //XY
        //m_rootTransform.AddEulerAnglesY(-m_rotationSpeed * UbhTimer.instance.deltaTime);

    }
}
