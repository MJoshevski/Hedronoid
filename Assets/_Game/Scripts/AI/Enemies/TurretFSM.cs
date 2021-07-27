using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.HNDFSM;
using Hedronoid.Core;
using Hedronoid.AI;
using Hedronoid.Weapons;

/// <summary>
/// 
/// </summary>
namespace Hedronoid
{
    public class TurretFSM : HNDFiniteStateMachine, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }

        private HNDFiniteStateMachine playerFSM;
        private UbhShotCtrl shotCtrl;
        private Transform target;

        [Header("General")]
        public float range = 15f;

        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;
        private Rigidbody rb_auto;
        private System.Action onDespawnAction;

        [Header("Use Laser")]
        public bool useLaser = false;
        public int damageOverTime = 30;
        public float slowAmount = .5f;
        public LineRenderer lineRenderer;
        public ParticleSystem impactEffect;
        public Light impactLight;

        [Header("Unity Setup Fields")]
        public Transform partToRotate;
        public float turnSpeed = 10f;

        [Header("FMOD Audio Data")]
        public NPCAudioData m_enemyAudioData;

        // Use this for initialization

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            TryGetComponent(out shotCtrl);
        }

        protected override void Start()
        {
            base.Start();

            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }

        void UpdateTarget()
        {
            if (GameplaySceneContext.Player == null) return;

            playerFSM = GameplaySceneContext.Player;

            float distanceToPlayer = Vector3.Distance(transform.position, playerFSM.cachedTransform.position);

            foreach (UbhShotCtrl.ShotInfo ubs in shotCtrl.m_shotList)
            {
                if (ubs.m_shotObj is UbhLinearLockOnShot)
                {
                    UbhLinearLockOnShot lockOnShot = (UbhLinearLockOnShot)ubs.m_shotObj;

                    if (distanceToPlayer <= range)
                    {
                        lockOnShot.m_targetTransform = playerFSM.transform;
                    }
                    else
                    {
                        lockOnShot.m_targetTransform = null;
                    }
                }
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            UpdateTarget();

            if (target == null)
            {
                if (useLaser)
                {
                    if (lineRenderer.enabled)
                    {
                        lineRenderer.enabled = false;
                        impactEffect.Stop();
                        impactLight.enabled = false;
                    }
                }

                return;
            }

            LockOnTarget();
            Shoot();

            if (useLaser)
            {
                Laser();
            }
        }

        void LockOnTarget()
        {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        void Laser()
        {
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
                impactEffect.Play();
                impactLight.enabled = true;
            }

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.position);

            Vector3 dir = transform.position - target.position;

            impactEffect.transform.position = target.position + dir.normalized;

            impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        }

        void Shoot()
        {
            FMODUnity.RuntimeManager.PlayOneShot(m_enemyAudioData.bulletPrimary[0], transform.position);
        }

        private System.Action onDespawnReset(GameObject bullet)
        {
            bullet.SetActive(false);

            // Zero out previous velocity
            Rigidbody bulletRb = bullet.gameObject.GetComponent<Rigidbody>();
            if (bulletRb)
                bulletRb.velocity = Vector3.zero;

            // Return bullet pos/rot to origin
            bullet.transform.SetPositionAndRotation(transform.position, Quaternion.identity);

            bullet.SetActive(true);

            return null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}