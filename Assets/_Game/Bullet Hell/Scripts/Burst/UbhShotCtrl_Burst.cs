using Hedronoid;
using Hedronoid.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Hedronoid.Weapons;
using Hedronoid.Core;

/// <summary>
/// Ubh shot ctrl.
/// </summary>
[AddComponentMenu("UniBulletHell/Controller/Shot Controller_Burst")]
public sealed class UbhShotCtrl_Burst : HNDMonoBehaviour, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    private enum UpdateStep
    {
        StartDelay,
        StartShot,
        WaitDelay,
        UpdateIndex,
        FinishShot,
    }

    [Serializable]
    public class ShotInfo
    {
        // "Set a delay time to starting next shot pattern. (sec)"
        [FormerlySerializedAs("_AfterDelay")]
        public float m_afterDelay;
        public ShotPattern m_ShotPattern;
    }

    // "Flag that inherits angle of UbhShotCtrl."
    public bool m_inheritAngle = false;
    // "This flag starts a shot routine at same time as instantiate."
    [FormerlySerializedAs("_StartOnAwake")]
    public bool m_startOnAwake = true;
    // "Set a delay time at using Start On Awake. (sec)"
    [FormerlySerializedAs("_StartOnAwakeDelay"), UbhConditionalHide("m_startOnAwake")]
    public float m_startOnAwakeDelay = 1f;
    // "This flag starts a shot routine at same time as enabled."
    public bool m_startOnEnable = false;
    // "Set a delay time at using Start On Enable. (sec)"
    [UbhConditionalHide("m_startOnEnable")]
    public float m_startOnEnableDelay = 1f;
    // "Flag that repeats a shot routine."
    [FormerlySerializedAs("_Loop")]
    public bool m_loop = true;
    // "Flag that makes a shot routine randomly."
    [FormerlySerializedAs("_AtRandom")]
    public bool m_atRandom = false;
    // "List of shot information. this size is necessary at least 1 or more."
    [FormerlySerializedAs("_ShotList")]
    public List<ShotInfo> m_shotList = new List<ShotInfo>();

    [Space(10)]

    // "Set a callback method after shot routine."
    public UnityEvent m_shotRoutineFinishedCallbackEvents = new UnityEvent();

    private bool m_shooting;
    private UpdateStep m_updateStep;
    private int m_nowIndex;
    private float m_delayTimer;
    private List<ShotInfo> m_randomShotList = new List<ShotInfo>(32);

    private AIBaseNavigation aiNavigation;
    private BulletPoolManager m_bulletPoolManager;
    private BulletPoolManager.BulletConfig m_bulletConfig;

    // BURST
    public TransformAccessArray m_BulletTransformsAccessArray;
    public NativeArray<Vector3> m_BulletPositionsNativeArray;
    public NativeArray<Vector3> m_BulletHeadingsNativeArray;
    public NativeArray<quaternion> m_BulletRotationsNativeArray;
    public NativeArray<RaycastCommand> m_BulletRaycasts;
    public NativeArray<RaycastHit> m_BulletRayhits;

    public JobHandle updateBulletsPositionsArrayJobHandle;
    public JobHandle updateBulletsTransformsJobHandle;
    public JobHandle updateBulletsJobHandle;
    public JobHandle buildRaycastCommandsJobHandle;
    public JobHandle sensoryRaycastsJobHandle;

    public LayerMask environmentLayerMask;

    // hack var
    int m_bulletNum;
    //

    /// <summary>
    /// is shooting flag.
    /// </summary>
    public bool shooting { get { return m_shooting; } }
    public static GameObject[] bullets;
    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);

        aiNavigation = GetComponent<AIBaseNavigation>();
        m_bulletPoolManager = GameplaySceneContext.BulletPoolManager;

        // HACK: Take bullet quantities sequentially or randomly from the shot list.
        m_bulletNum = m_shotList[0].m_ShotPattern.m_bulletNum;

        m_bulletConfig = new BulletPoolManager.BulletConfig();
        bullets = new GameObject[m_bulletNum];

        for (int k = 0; k < m_bulletNum; k++)
        {
            bullets[k] = GetBulletGO(Vector3.zero);
        }
    }
    protected override void Start()
    {
        base.Start();

        Transform[] bulletTransforms = new Transform[m_bulletNum];

        for (int k = 0; k < m_bulletNum; k++)
            bulletTransforms[k] = bullets[k].transform;

        m_BulletTransformsAccessArray = new TransformAccessArray(bulletTransforms);

        m_BulletPositionsNativeArray = new NativeArray<Vector3>(m_bulletNum, Allocator.Persistent);
        m_BulletHeadingsNativeArray = new NativeArray<Vector3>(m_bulletNum, Allocator.Persistent);
        m_BulletRotationsNativeArray = new NativeArray<quaternion>(m_bulletNum, Allocator.Persistent);

        m_BulletRaycasts = new NativeArray<RaycastCommand>(m_bulletNum, Allocator.Persistent);
        m_BulletRayhits = new NativeArray<RaycastHit>(m_bulletNum, Allocator.Persistent);
        //if (m_startOnAwake)
        //{
        //    StartShotRoutine(m_startOnAwakeDelay);
        //}
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        //if (m_startOnEnable)
        //{
        //    StartShotRoutine(m_startOnEnableDelay);
        //}
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        updateBulletsJobHandle.Complete();

        m_BulletTransformsAccessArray.Dispose();
        m_BulletPositionsNativeArray.Dispose();
        m_BulletRotationsNativeArray.Dispose();
        m_BulletHeadingsNativeArray.Dispose();
        m_BulletRaycasts.Dispose();
        m_BulletRayhits.Dispose();

        m_shooting = false;

        UbhShotManager shotMgr = UbhShotManager.instance;
        if (shotMgr != null)
        {
            //shotMgr.RemoveShot(this);
        }
    }

    protected override void OnDestroy()
    {
        updateBulletsJobHandle.Complete();

        m_BulletTransformsAccessArray.Dispose();
        m_BulletPositionsNativeArray.Dispose();
        m_BulletRotationsNativeArray.Dispose();
        m_BulletHeadingsNativeArray.Dispose();
        m_BulletRaycasts.Dispose();
        m_BulletRayhits.Dispose();
    }

    private void Update()
    {
        updateBulletsJobHandle.Complete();
        updateBulletsTransformsJobHandle.Complete();

        UpdateBulletPositionsArrayJob updateBulletPositionsArrayJob = new UpdateBulletPositionsArrayJob()
        {
            bulletPositions = m_BulletPositionsNativeArray,
            bulletHeadings = m_BulletHeadingsNativeArray
        };

        updateBulletsPositionsArrayJobHandle = updateBulletPositionsArrayJob.Schedule(
            m_BulletTransformsAccessArray, updateBulletsTransformsJobHandle);

        BuildSensorRaycastCommandsJob buildSensorRaycastCommandsJob = new BuildSensorRaycastCommandsJob()
        {
            bulletPositions = m_BulletPositionsNativeArray,
            bulletHeadings = m_BulletHeadingsNativeArray,
            maxDist = 3.0f,
            envLayerMask = environmentLayerMask,
            sensorRaycasts = m_BulletRaycasts
        };

        buildRaycastCommandsJobHandle = buildSensorRaycastCommandsJob.Schedule(m_bulletNum, 64, updateBulletsPositionsArrayJobHandle);
        sensoryRaycastsJobHandle = RaycastCommand.ScheduleBatch(m_BulletRaycasts, m_BulletRayhits, 32, buildRaycastCommandsJobHandle);

        UpdateBulletsJob updateBulletsJob = new UpdateBulletsJob()
        {
            bulletHeadings = m_BulletHeadingsNativeArray,

            timeNorm = (Time.deltaTime > 0.016f ? 0.016f : Time.deltaTime) / 0.02f,

            bulletRotations = m_BulletRotationsNativeArray,
            bulletPositions = m_BulletPositionsNativeArray,

            rayHits = m_BulletRayhits,

            targetPosition = new Vector3 (10, 10, 10),
            targetRotation = Quaternion.identity,
            speed = m_shotList[0].m_ShotPattern.m_speed,
            angleHorizontal = m_shotList[0].m_ShotPattern.m_angleHorizontal,
            angleVertical = m_shotList[0].m_ShotPattern.m_angleVertical,
            accelSpeed = m_shotList[0].m_ShotPattern.m_accelSpeed,
            accelTurn = m_shotList[0].m_ShotPattern.m_accelTurn,
            homing = m_shotList[0].m_ShotPattern.m_homing,
            homingTargetPosition = new Vector3(10, 10, 10),
            homingTargetRotation = Quaternion.identity,
            homingAngleSpeed = m_shotList[0].m_ShotPattern.m_homingAngleSpeed,
            sinWave = m_shotList[0].m_ShotPattern.m_sinWave,
            sinWaveSpeed = m_shotList[0].m_ShotPattern.m_sinWaveSpeed,
            sinWaveRangeSize = m_shotList[0].m_ShotPattern.m_sinWaveRangeSize,
            sinWaveInverse = m_shotList[0].m_ShotPattern.m_sinWaveInverse,
            pauseAndResume = m_shotList[0].m_ShotPattern.m_pauseAndResume,
            pauseTime = m_shotList[0].m_ShotPattern.m_pauseTime,
            resumeTime = m_shotList[0].m_ShotPattern.m_resumeTime,
            useAutoRelease = m_shotList[0].m_ShotPattern.m_useAutoRelease,
            autoReleaseTime = m_shotList[0].m_ShotPattern.m_autoReleaseTime,
            useMaxSpeed = m_shotList[0].m_ShotPattern.m_useMaxSpeed,
            maxSpeed = m_shotList[0].m_ShotPattern.m_maxSpeed,
            useMinSpeed = m_shotList[0].m_ShotPattern.m_useMinSpeed,
            minSpeed = m_shotList[0].m_ShotPattern.m_minSpeed,
            baseAngles = m_shotList[0].m_ShotPattern.m_baseAngles,
            selfFrameCount = m_shotList[0].m_ShotPattern.m_selfFrameCnt,
            selfTimeCount = m_shotList[0].m_ShotPattern.m_selfTimeCount,
            shooting = m_shotList[0].m_ShotPattern.m_shooting
        };

        updateBulletsJobHandle = updateBulletsJob.Schedule(m_bulletNum, 64, sensoryRaycastsJobHandle);

        JobHandle.ScheduleBatchedJobs();

        UpdateBulletTransformsArrayJob updateAgentTransformsJob = new UpdateBulletTransformsArrayJob()
        {
            bulletPositions = m_BulletPositionsNativeArray,
            bulletRotations = m_BulletRotationsNativeArray
        };
        updateBulletsTransformsJobHandle = updateAgentTransformsJob.Schedule(
            m_BulletTransformsAccessArray, updateBulletsJobHandle);
    }

    private GameObject GetBulletGO(Vector3 position, bool forceInstantiate = false)
    {
        if (m_shotList[0].m_ShotPattern.m_prefab == null)
        {
            Debug.LogWarning("Cannot generate a bullet because BulletPrefab is not set.");
            return null;
        }

        // get UbhBullet from ObjectPool
        m_bulletConfig.Prefab = m_shotList[0].m_ShotPattern.m_prefab;
        m_bulletConfig.Position = position;
        m_bulletConfig.Rotation = Quaternion.identity;
        m_bulletConfig.Parent = null;
        m_bulletConfig.Duration = -1;

        GameObject bullet = m_bulletPoolManager.GetBulletToFire(m_bulletConfig);

        if (bullet == null)
        {
            return null;
        }

        return bullet;
    }

    //public void UpdateShot(float deltaTime)
    //{
    //    if (m_shooting == false)
    //    {
    //        return;
    //    }

    //    if (m_updateStep == UpdateStep.StartDelay)
    //    {
    //        if (m_delayTimer > 0f)
    //        {
    //            m_delayTimer -= deltaTime;
    //            return;
    //        }
    //        else
    //        {
    //            m_delayTimer = 0f;
    //            m_updateStep = UpdateStep.StartShot;
    //        }
    //    }

    //    ShotInfo nowShotInfo = m_atRandom ? m_randomShotList[m_nowIndex] : m_shotList[m_nowIndex];

    //    if (m_updateStep == UpdateStep.StartShot)
    //    {
    //        if (nowShotInfo.m_shotObj != null)
    //        {
    //            //nowShotInfo.m_shotObj.SetShotCtrl(this);
    //            nowShotInfo.m_shotObj.Shot();
    //        }

    //        m_delayTimer = 0f;
    //        m_updateStep = UpdateStep.WaitDelay;
    //    }

    //    if (m_updateStep == UpdateStep.WaitDelay)
    //    {
    //        if (nowShotInfo.m_afterDelay > 0 && nowShotInfo.m_afterDelay > m_delayTimer)
    //        {
    //            m_delayTimer += deltaTime;
    //        }
    //        else
    //        {
    //            m_delayTimer = 0f;
    //            m_updateStep = UpdateStep.UpdateIndex;
    //        }
    //    }

    //    if (m_updateStep == UpdateStep.UpdateIndex)
    //    {
    //        if (m_atRandom)
    //        {
    //            m_randomShotList.RemoveAt(m_nowIndex);

    //            if (m_loop && m_randomShotList.Count <= 0)
    //            {
    //                m_randomShotList.AddRange(m_shotList);
    //            }

    //            if (m_randomShotList.Count > 0)
    //            {
    //                m_nowIndex = UnityEngine.Random.Range(0, m_randomShotList.Count);
    //                m_updateStep = UpdateStep.StartShot;
    //            }
    //            else
    //            {
    //                m_updateStep = UpdateStep.FinishShot;
    //            }
    //        }
    //        else
    //        {
    //            if (m_loop || m_nowIndex < m_shotList.Count - 1)
    //            {
    //                m_nowIndex = (int)Mathf.Repeat(m_nowIndex + 1f, m_shotList.Count);
    //                m_updateStep = UpdateStep.StartShot;
    //            }
    //            else
    //            {
    //                m_updateStep = UpdateStep.FinishShot;
    //            }
    //        }
    //    }

    //    if (m_updateStep == UpdateStep.StartShot)
    //    {
    //        UpdateShot(deltaTime);
    //    }
    //    else if (m_updateStep == UpdateStep.FinishShot)
    //    {
    //        m_shooting = false;
    //        m_shotRoutineFinishedCallbackEvents.Invoke();
    //    }
    //}

    /// <summary>
    /// Start the shot routine.
    /// </summary>
    //public void StartShotRoutine(float startDelay = 0f)
    //{
    //    if (m_shotList == null || m_shotList.Count <= 0)
    //    {
    //        Debug.LogWarning("Cannot shot because ShotList is null or empty.");
    //        return;
    //    }

    //    bool enableShot = false;
    //    for (int i = 0; i < m_shotList.Count; i++)
    //    {
    //        if (m_shotList[i].m_shotObj != null)
    //        {
    //            enableShot = true;
    //            break;
    //        }
    //    }
    //    if (enableShot == false)
    //    {
    //        Debug.LogWarning("Cannot shot because all ShotObj of ShotList is not set.");
    //        return;
    //    }

    //    if (m_loop)
    //    {
    //        bool enableDelay = false;
    //        for (int i = 0; i < m_shotList.Count; i++)
    //        {
    //            if (0f < m_shotList[i].m_afterDelay)
    //            {
    //                enableDelay = true;
    //                break;
    //            }
    //        }
    //        if (enableDelay == false)
    //        {
    //            Debug.LogWarning("Cannot shot because loop is true and all AfterDelay of ShotList is zero.");
    //            return;
    //        }
    //    }

    //    if (m_shooting)
    //    {
    //        Debug.LogWarning("Already shooting.");
    //        return;
    //    }

    //    m_shooting = true;
    //    m_delayTimer = startDelay;
    //    m_updateStep = m_delayTimer > 0f ? UpdateStep.StartDelay : UpdateStep.StartShot;
    //    if (m_atRandom)
    //    {
    //        m_randomShotList.Clear();
    //        m_randomShotList.AddRange(m_shotList);
    //        m_nowIndex = UnityEngine.Random.Range(0, m_randomShotList.Count);
    //    }
    //    else
    //    {
    //        m_nowIndex = 0;
    //    }
    //}

    /// <summary>
    /// Stop the shot routine.
    /// </summary>
    //public void StopShotRoutine()
    //{
    //    m_shooting = false;
    //}

    /// <summary>
    /// Stop the shot routine and playing shot.
    /// </summary>
    //public void StopShotRoutineAndPlayingShot()
    //{
    //    m_shooting = false;

    //    if (m_shotList == null || m_shotList.Count <= 0)
    //    {
    //        return;
    //    }

    //    for (int i = 0; i < m_shotList.Count; i++)
    //    {
    //        if (m_shotList[i].m_shotObj != null)
    //        {
    //            m_shotList[i].m_shotObj.FinishedShot();
    //        }
    //    }
    //}


    [BurstCompile]
    struct UpdateBulletPositionsArrayJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<Vector3> bulletPositions;
        [WriteOnly]
        public NativeArray<Vector3> bulletHeadings;

        public void Execute(int index, TransformAccess transform)
        {
            bulletPositions[index] = transform.position;
            bulletHeadings[index] = transform.rotation * Vector3.forward;
        }
    }

    [BurstCompile]
    struct UpdateBulletTransformsArrayJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<Vector3> bulletPositions;
        [ReadOnly]
        public NativeArray<quaternion> bulletRotations;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = bulletPositions[index];
            transform.rotation = bulletRotations[index];
        }
    }

    [BurstCompile]
    struct UpdateBulletsJob : IJobParallelFor
    {
        [ReadOnly]
        public float timeNorm;
        [ReadOnly]
        public NativeArray<Vector3> bulletHeadings;
        [ReadOnly]
        public NativeArray<RaycastHit> rayHits;


        public NativeArray<quaternion> bulletRotations;
        public NativeArray<Vector3> bulletPositions;

        public float3 targetPosition;
        public quaternion targetRotation;
        public float speed;
        public float angleHorizontal;
        public float angleVertical;
        public float accelSpeed;
        public float accelTurn;
        public bool homing;
        public float3 homingTargetPosition;
        public quaternion homingTargetRotation;
        public float homingAngleSpeed;
        public bool sinWave;
        public float sinWaveSpeed;
        public float sinWaveRangeSize;
        public bool sinWaveInverse;
        public bool pauseAndResume;
        public float pauseTime;
        public float resumeTime;
        public bool useAutoRelease;
        public float autoReleaseTime;
        public bool useMaxSpeed;
        public float maxSpeed;
        public bool useMinSpeed;
        public float minSpeed; 
        public float2 baseAngles;
        public float selfFrameCount;
        public float selfTimeCount;
        public bool shooting;

        public void Execute(int index)
        {
            var deltaTime = timeNorm;

            Vector3 myAngles = targetRotation.EulerAngles();
            float3 zero = float3(0);
            Quaternion newRotation = targetRotation;
            if (homing)
            {
                // homing target.
                if (!homingTargetPosition.Equals(zero) &&  0f < homingAngleSpeed)
                {
                    Quaternion rotation = Quaternion.LookRotation(math.normalizesafe(homingTargetPosition - targetPosition));

                    Quaternion toRotation =
                        Quaternion.RotateTowards(bulletRotations[index], rotation, deltaTime * homingAngleSpeed);

                    newRotation = toRotation;
                }
            }
            else if (sinWave)
            {
                //// acceleration turning.
                angleHorizontal += (accelTurn * deltaTime);
                angleVertical += (accelTurn * deltaTime);

                // sin wave.
                if (0f < sinWaveSpeed && 0f < sinWaveRangeSize)
                {
                    float waveAngleXZ = angleHorizontal + (sinWaveRangeSize / 2f * (Mathf.Sin(selfFrameCount * sinWaveSpeed / 100f) * (sinWaveInverse ? -1f : 1f)));

                    newRotation = Quaternion.Euler(
                        baseAngles.x + angleVertical, baseAngles.y + waveAngleXZ, myAngles.z);

                }
                //selfFrameCount += UbhTimer.instance.deltaFrameCount;
            }
            else
            {
                // acceleration turning.
                float addAngle = accelTurn * deltaTime;

                newRotation = Quaternion.Euler(
                    myAngles.x, myAngles.y - addAngle, myAngles.z + addAngle);
            }

            // acceleration speed.
            speed += (accelSpeed * deltaTime);

            if (useMaxSpeed && speed > maxSpeed)
            {
                speed = maxSpeed;
            }

            if (useMinSpeed && speed < minSpeed)
            {
                speed = minSpeed;
            }

            // move.
            bulletPositions[index] = float3(bulletPositions[index].x, bulletPositions[index].y, bulletPositions[index].z) +
                (math.mul(targetRotation, float3(0,0,1)) * (speed * deltaTime));

            bulletRotations[index] = newRotation;
        }
    }

    [BurstCompile]
    public struct BuildSensorRaycastCommandsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> bulletPositions;
        [ReadOnly]
        public NativeArray<Vector3> bulletHeadings;
        [ReadOnly]
        public float maxDist;
        [ReadOnly]
        public int envLayerMask;

        [WriteOnly]
        public NativeArray<RaycastCommand> sensorRaycasts;

        public void Execute(int idx)
        {
            RaycastCommand rc = new RaycastCommand();
            rc.distance = maxDist;
            rc.direction = bulletHeadings[idx];
            rc.from = bulletPositions[idx];
            rc.layerMask = envLayerMask;
            rc.maxHits = 1;

            sensorRaycasts[idx] = rc;
        }

    }
}