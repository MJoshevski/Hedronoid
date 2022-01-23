using Hedronoid;
using Hedronoid.Core;
using Hedronoid.Weapons;
using UnityEngine;

/// <summary>
/// Ubh bullet.
/// </summary>
[DisallowMultipleComponent]
public class UbhBullet : HNDMonoBehaviour, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    private Transform m_transformCache;
    private UbhBaseShot m_parentBaseShot;
    private float m_speed;
    private float m_angleHorizontal;
    private float m_angleVertical;
    private float m_accelSpeed;
    private float m_accelTurn;
    private bool m_homing;
    private Transform m_homingTarget;
    private float m_homingAngleSpeed;
    private bool m_sinWave;
    private float m_sinWaveSpeed;
    private float m_sinWaveRangeSize;
    private bool m_sinWaveInverse;
    private bool m_pauseAndResume;
    private float m_pauseTime;
    private float m_resumeTime;
    private bool m_useAutoRelease;
    private float m_autoReleaseTime;
    private bool m_useMaxSpeed;
    private float m_maxSpeed;
    private bool m_useMinSpeed;
    private float m_minSpeed;
    private FMOD.Studio.EventInstance m_eventInstance;

    private Vector2 m_baseAngles;
    private float m_selfFrameCnt;
    private float m_selfTimeCount;

    private UbhTentacleBullet m_tentacleBullet;
    private BulletPoolManager BulletPoolManager;

    private bool m_shooting;

    public UbhBaseShot parentShot { get { return m_parentBaseShot; } }

    /// <summary>
    /// Activate/Inactivate flag
    /// Override this property when you want to change the behavior at Active / Inactive.
    /// </summary>
    public virtual bool isActive { get { return gameObject.activeSelf; } }

    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);

        BulletPoolManager = GameplaySceneContext.BulletPoolManager;
        m_transformCache = transform;
        m_tentacleBullet = GetComponent<UbhTentacleBullet>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_shooting == false)
        {
            return;
        }
        OnFinishedShot();
    }

    /// <summary>
    /// Activate/Inactivate Bullet
    /// Override this method when you want to change the behavior at Active / Inactive.
    /// </summary>
    public virtual void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Finished Shot
    /// </summary>
    public void OnFinishedShot()
    {
        if (m_shooting == false)
        {
            return;
        }
        m_shooting = false;

        m_eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        m_parentBaseShot = null;
        m_homingTarget = null;
        m_transformCache.ResetPosition();
        m_transformCache.ResetRotation();
        BulletPoolManager.ReturnObject(gameObject);
    }

    /// <summary>
    /// Bullet Shot
    /// </summary>
    public void Shot(UbhBaseShot parentBaseShot,
                     float speed, float angleHorizontal, float angleVertical, float accelSpeed, float accelTurn,
                     bool homing, Transform homingTarget, float homingAngleSpeed,
                     bool sinWave, float sinWaveSpeed, float sinWaveRangeSize, bool sinWaveInverse,
                     bool pauseAndResume, float pauseTime, float resumeTime,
                     bool useAutoRelease, float autoReleaseTime,
                     bool inheritAngle, bool useMaxSpeed, 
                     float maxSpeed, bool useMinSpeed, float minSpeed, FMOD.Studio.EventInstance eventInstance)
    {
        if (m_shooting)
        {
            return;
        }
        m_shooting = true;

        m_parentBaseShot = parentBaseShot;

        m_speed = speed;
        m_angleHorizontal = angleHorizontal;
        m_angleVertical = angleVertical;
        m_accelSpeed = accelSpeed;
        m_accelTurn = accelTurn;
        m_homing = homing;
        m_homingTarget = homingTarget;
        m_homingAngleSpeed = homingAngleSpeed;
        m_sinWave = sinWave;
        m_sinWaveSpeed = sinWaveSpeed;
        m_sinWaveRangeSize = sinWaveRangeSize;
        m_sinWaveInverse = sinWaveInverse;
        m_pauseAndResume = pauseAndResume;
        m_pauseTime = pauseTime;
        m_resumeTime = resumeTime;
        m_useAutoRelease = useAutoRelease;
        m_autoReleaseTime = autoReleaseTime;
        m_useMaxSpeed = useMaxSpeed;
        m_maxSpeed = maxSpeed;
        m_useMinSpeed = useMinSpeed;
        m_minSpeed = minSpeed;
        m_eventInstance = eventInstance;

        m_baseAngles = Vector2.zero;
        if (inheritAngle && m_parentBaseShot.lockOnShot == false)
        {
            m_baseAngles.x = m_parentBaseShot.shotCtrl.transform.eulerAngles.x;
            m_baseAngles.y = m_parentBaseShot.shotCtrl.transform.eulerAngles.y;
        }

        m_transformCache.SetEulerAngles(
            m_baseAngles.x + m_angleVertical,
            m_baseAngles.y + m_angleHorizontal,
            0f);

        m_selfFrameCnt = 0f;
        m_selfTimeCount = 0f;
    }

    /// <summary>
    /// Update Move
    /// </summary>
    public void Update()
    {
        var deltaTime = UbhTimer.instance.deltaTime;

        if (m_shooting == false)
        {
            return;
        }

        m_selfTimeCount += UbhTimer.instance.deltaTime;

        // auto release check
        if (m_useAutoRelease && m_autoReleaseTime > 0f)
        {
            if (m_selfTimeCount >= m_autoReleaseTime)
            {
                // Release
                OnFinishedShot();
                return;
            }
        }

        // pause and resume.
        if (m_pauseAndResume && m_pauseTime >= 0f && m_resumeTime > m_pauseTime)
        {
            if (m_pauseTime <= m_selfTimeCount && m_selfTimeCount < m_resumeTime)
            {
                return;
            }
        }

        Vector3 myAngles = m_transformCache.rotation.eulerAngles;

        Quaternion newRotation = m_transformCache.rotation;
        if (m_homing)
        {
            // homing target.
            if (m_homingTarget != null && 0f < m_homingAngleSpeed)
            {
                Quaternion rotation = Quaternion.LookRotation((m_homingTarget.position - m_transformCache.position).normalized);

                Quaternion toRotation = 
                    Quaternion.RotateTowards(transform.rotation, rotation, deltaTime * m_homingAngleSpeed);

                newRotation = toRotation;
            }
        }
        else if (m_sinWave)
        {
            //// acceleration turning.
            m_angleHorizontal += (m_accelTurn * deltaTime);
            m_angleVertical += (m_accelTurn * deltaTime);

            // sin wave.
            if (0f < m_sinWaveSpeed && 0f < m_sinWaveRangeSize)
            {
                float waveAngleXZ = m_angleHorizontal + (m_sinWaveRangeSize / 2f * (Mathf.Sin(m_selfFrameCnt * m_sinWaveSpeed / 100f) * (m_sinWaveInverse ? -1f : 1f)));

                newRotation = Quaternion.Euler(
                    m_baseAngles.x + m_angleVertical, m_baseAngles.y + waveAngleXZ, myAngles.z);

            }
            m_selfFrameCnt += UbhTimer.instance.deltaFrameCount;
        }
        else
        {
            // acceleration turning.
            float addAngle = m_accelTurn * deltaTime;

            newRotation = Quaternion.Euler(
                myAngles.x, myAngles.y - addAngle, myAngles.z + addAngle);
        }

        // acceleration speed.
        m_speed += (m_accelSpeed * deltaTime);

        if (m_useMaxSpeed && m_speed > m_maxSpeed)
        {
            m_speed = m_maxSpeed;
        }

        if (m_useMinSpeed && m_speed < m_minSpeed)
        {
            m_speed = m_minSpeed;
        }

        // move.
        Vector3 newPosition;
        newPosition = m_transformCache.position + 
            (m_transformCache.forward * (m_speed * deltaTime));


        // set new position and rotation
        m_transformCache.SetPositionAndRotation(newPosition, newRotation);

        if (m_tentacleBullet != null)
        {
            // Update tentacles
            m_tentacleBullet.UpdateRotate();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 5;

        Gizmos.DrawRay(transform.position, direction);
    }
#endif

}