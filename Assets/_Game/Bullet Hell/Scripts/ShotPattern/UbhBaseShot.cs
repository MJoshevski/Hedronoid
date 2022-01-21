using Hedronoid;
using Hedronoid.Core;
using Hedronoid.Weapons;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Ubh base shot.
/// Each shot pattern classes inherit this class.
/// </summary>
public abstract class UbhBaseShot : HNDMonoBehaviour, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    [Header("===== Common Settings =====")]
    [FormerlySerializedAs("_BulletConfig")]
    public BulletPoolManager.BulletConfig m_bulletConfig;
    [FormerlySerializedAs("_TargetTransform")]
    public Transform m_targetTransform;
    // "Set a bullet prefab for the shot. (ex. sprite or model)"
    [FormerlySerializedAs("_BulletPrefab")]
    public GameObject m_bulletPrefab;
    [FormerlySerializedAs("_BulletPrefabAlt")]
    public GameObject m_bulletPrefabAlt;
    // "Set a bullet origin for the shot."
    [FormerlySerializedAs("_BulletOrigin")]
    public Transform m_bulletOrigin;
    // "Set a bullet number of shot."
    [FormerlySerializedAs("_BulletNum")]
    public int m_bulletNum = 10;
    // "Set a bullet base speed of shot."
    [FormerlySerializedAs("_BulletSpeed")]
    public float m_bulletSpeed = 2f;
    // "Set an acceleration of bullet speed."
    [FormerlySerializedAs("_AccelerationSpeed")]
    public float m_accelerationSpeed = 0f;
    // "Use max speed flag."
    public bool m_useMaxSpeed = false;
    // "Set a bullet max speed of shot."
    [UbhConditionalHide("m_useMaxSpeed")]
    public float m_maxSpeed = 0f;
    // "Use min speed flag"
    public bool m_useMinSpeed = false;
    // "Set a bullet min speed of shot."
    [UbhConditionalHide("m_useMinSpeed")]
    public float m_minSpeed = 0f;
    // "Set an acceleration of bullet turning."
    [FormerlySerializedAs("_AccelerationTurn")]
    public float m_accelerationTurn = 0f;
    // "This flag is pause and resume bullet at specified time."
    [FormerlySerializedAs("_UsePauseAndResume")]
    public bool m_usePauseAndResume = false;
    // "Set a time to pause bullet."
    [FormerlySerializedAs("_PauseTime"), UbhConditionalHide("m_usePauseAndResume")]
    public float m_pauseTime = 0f;
    // "Set a time to resume bullet."
    [FormerlySerializedAs("_ResumeTime"), UbhConditionalHide("m_usePauseAndResume")]
    public float m_resumeTime = 0f;
    // "This flag is automatically release the bullet GameObject at the specified time."
    [FormerlySerializedAs("_UseAutoRelease")]
    public bool m_useAutoRelease = false;
    // "Set a time to automatically release after the shot at using UseAutoRelease. (sec)"
    [FormerlySerializedAs("_AutoReleaseTime"), UbhConditionalHide("m_useAutoRelease")]
    public float m_autoReleaseTime = 10f;
    [FormerlySerializedAs("_ShootParticles")]
    public ParticleList.ParticleSystems ShootParticles = new ParticleList.ParticleSystems();


    [Space(10)]

    // "Set a callback method fired shot."
    public UnityEvent m_shotFiredCallbackEvents = new UnityEvent();
    // "Set a callback method after shot."
    public UnityEvent m_shotFinishedCallbackEvents = new UnityEvent();

    protected bool m_shooting;
    protected bool m_disableShooting = false;

    private UbhShotCtrl m_shotCtrl;
    private BulletPoolManager BulletPoolManager;
    public UbhShotCtrl shotCtrl
    {
        get
        {
            if (m_shotCtrl == null)
            {
                m_shotCtrl = transform.GetComponentInParent<UbhShotCtrl>();

                if (m_shotCtrl == null)
                {
                    m_shotCtrl = transform.parent.GetComponentInParent<UbhShotCtrl>();
                }
            }
            return m_shotCtrl;
        }
    }

    private TurretNavigation m_turretNavigation;
    public TurretNavigation TurretNavigation
    {
        get
        {
            
            return m_turretNavigation;
        }
    }

    /// <summary>
    /// is shooting flag.
    /// </summary>
    public bool shooting { get { return m_shooting; } }

    /// <summary>
    /// is lock on shot flag.
    /// </summary>
    public virtual bool lockOnShot { get { return false; } }

    public virtual bool disableShooting { get { return m_disableShooting; } set { m_disableShooting = value; } }

    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);

        BulletPoolManager = GameplaySceneContext.BulletPoolManager;
        m_bulletConfig = new BulletPoolManager.BulletConfig();

        if (m_turretNavigation == null)
        {
            m_turretNavigation = transform.GetComponentInParent<TurretNavigation>();

            if (m_turretNavigation == null)
            {
                m_turretNavigation = transform.parent.GetComponentInParent<TurretNavigation>();
            }
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        UbhShotCtrl.ShotInfo si = new UbhShotCtrl.ShotInfo();

        si.m_afterDelay = 2;
        si.m_shotObj = this;

        if(!shotCtrl.m_shotList.Contains(si))
            shotCtrl.m_shotList.Add(si);
    }
    protected override void Start()
    {
        base.Start();

        // If we haven't assigned a bullet origin, we fire shots
        // from the game object's transform itself.

        if (m_bulletOrigin == null)
            m_bulletOrigin = transform;
    }

    /// <summary>
    /// Call from override OnDisable method in inheriting classes.
    /// Example : protected override void OnDisable () { base.OnDisable (); }
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();

        m_shooting = false;

        UbhShotCtrl.ShotInfo si = new UbhShotCtrl.ShotInfo();

        si.m_afterDelay = 2;
        si.m_shotObj = this;

        if (shotCtrl.m_shotList.Contains(si))
            shotCtrl.m_shotList.Remove(si);
    }

    /// <summary>
    /// Abstract shot method.
    /// </summary>
    public abstract void Shot();

    /// <summary>
    /// UbhShotCtrl setter.
    /// </summary>
    public void SetShotCtrl(UbhShotCtrl shotCtrl)
    {
        m_shotCtrl = shotCtrl;
    }

    /// <summary>
    /// Fired shot.
    /// </summary>
    protected void FiredShot()
    {
        m_shotFiredCallbackEvents.Invoke();
    }

    /// <summary>
    /// Finished shot.
    /// </summary>
    public void FinishedShot()
    {
        m_shooting = false;
        m_shotFinishedCallbackEvents.Invoke();
    }

    /// <summary>
    /// Get UbhBullet object from object pool.
    /// </summary>
    protected UbhBullet GetBullet(Vector3 position, bool forceInstantiate = false)
    {
        UbhBullet bullet = null;

        if (TurretNavigation.Target == null) 
        {
            //Debug.LogError("No target found!");
            return bullet;
        }

        if (m_bulletPrefab == null)
        {
            Debug.LogWarning("Cannot generate a bullet because BulletPrefab is not set.");
            return bullet;
        }
        GameObject chosenBullet = m_bulletPrefab;

        if(m_bulletPrefabAlt != null)
        {
            float rand = Random.Range(0f, 1f);

            if (rand > 0.5f)
                chosenBullet = m_bulletPrefabAlt;
        }

        // get UbhBullet from ObjectPool
        m_bulletConfig.Prefab = chosenBullet;
        m_bulletConfig.Position = m_bulletOrigin.position;
        m_bulletConfig.Rotation = Quaternion.identity;
        m_bulletConfig.Parent = null;
        m_bulletConfig.Duration = -1;

        BulletPoolManager.GetBulletToFire(m_bulletConfig).TryGetComponent(out bullet);

        //if (bullet == null)
        //{
        //    return null;
        //}

        return bullet;
    }

    /// <summary>
    /// Get GameObject bullet from object pool.
    /// </summary>
    protected GameObject GetBulletGO(Vector3 position, bool forceInstantiate = false)
    {
        if (TurretNavigation.Target == null) 
        {
            //Debug.LogError("GetBulletGO: " + TurretNavigation.Target.gameObject.name);
            return null;
        }

        if (m_bulletPrefab == null)
        {
            Debug.LogWarning("Cannot generate a bullet because BulletPrefab is not set.");
            return null;
        }
         
        // get UbhBullet from ObjectPool
        m_bulletConfig.Prefab = m_bulletPrefab;
        m_bulletConfig.Position = m_bulletOrigin.position;
        m_bulletConfig.Rotation = Quaternion.identity;
        m_bulletConfig.Parent = null;
        m_bulletConfig.Duration = m_autoReleaseTime;

        GameObject bullet = BulletPoolManager.GetBulletToFire(m_bulletConfig);

        if (bullet == null)
        {
            return null;
        }

        return bullet;
    }

    /// <summary>
    /// Shot UbhBullet object.
    /// </summary>
    protected void ShotBullet(UbhBullet bullet, float speed, float angleH, float angleV,
                               bool homing = false, Transform homingTarget = null, float homingAngleSpeed = 0f,
                               bool sinWave = false, float sinWaveSpeed = 0f, float sinWaveRangeSize = 0f, bool sinWaveInverse = false)
    {
        if (TurretNavigation.Target == null) 
        {
            return;
        }

        if (bullet == null || m_disableShooting)
        {
            return;
        }

        bullet.Shot(this,
                    speed, angleH, angleV, m_accelerationSpeed, m_accelerationTurn,
                    homing, homingTarget, homingAngleSpeed,
                    sinWave, sinWaveSpeed, sinWaveRangeSize, sinWaveInverse,
                    m_usePauseAndResume, m_pauseTime, m_resumeTime,
                    m_useAutoRelease, m_autoReleaseTime, m_shotCtrl.m_inheritAngle,
                    m_useMaxSpeed, m_maxSpeed, m_useMinSpeed, m_minSpeed);
    }
}