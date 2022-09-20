using Hedronoid.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ShotPattern : ScriptableObject
{
    public Transform m_transformCache;
    public UbhBaseShot_Burst m_parentBaseShot;
    public GameObject m_prefab;
    public float m_speed;
    public float m_angleHorizontal;
    public float m_angleVertical;
    public float m_accelSpeed;
    public float m_accelTurn;
    public bool m_homing;
    public Transform m_homingTarget;
    public float m_homingAngleSpeed;
    public bool m_sinWave;
    public float m_sinWaveSpeed;
    public float m_sinWaveRangeSize;
    public bool m_sinWaveInverse;
    public bool m_pauseAndResume;
    public float m_pauseTime;
    public float m_resumeTime;
    public bool m_useAutoRelease;
    public float m_autoReleaseTime;
    public bool m_useMaxSpeed;
    public float m_maxSpeed;
    public bool m_useMinSpeed;
    public float m_minSpeed;
    public FMOD.Studio.EventInstance m_eventInstance;

    public Vector2 m_baseAngles;
    public float m_selfFrameCnt;
    public float m_selfTimeCount;

    public UbhTentacleBullet m_tentacleBullet;
    public BulletPoolManager BulletPoolManager;

    public bool m_shooting;

    public virtual void Initialize() { }
    public virtual void Update() { }
    public virtual Vector3 GetResults() { return Vector3.zero; }
}

[System.Serializable]
public class ShotPatternInstance
{
    public ShotPattern m_Pattern;
}
