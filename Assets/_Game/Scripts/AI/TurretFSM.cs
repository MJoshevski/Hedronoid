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
        private Transform target;

        [Header("General")]

        public float range = 15f;

        [Header("Shooting Variables")]
        [Tooltip("Spawn position of each bullet.")]
        public Transform bulletOrigin;
        [Tooltip("Seconds between each shot.")]
        public float fireRatePrimary = 0.2f, fireRateSecondary = 2f, fireRateTertiary = 5f;
        [Tooltip("Force of each shot.")]
        public float shootForcePrimary = 8000f, shootForceSecondary = 5000f, shootForceTertiary = 100000f;
        [Tooltip("Prefab of the respective weapon's bullet.")]
        public GameObject bulletPrimary, bulletSecondary, bulletTertiary;

        // SHOOTING
        private float lastFired_Auto = 0f;
        private float lastFired_Shotgun = 0f;
        private float lastFired_Rail = 0f;
        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;
        private Rigidbody rb_auto;
        private System.Action onDespawnAction;
        private float fireCountdown = 0f;

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
        }

        protected override void Start()
        {
            base.Start();
            playerFSM = GameplaySceneContext.Player;

            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }

        void UpdateTarget()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerFSM.transform.position);

            if (distanceToPlayer <= range)
            {
                target = playerFSM.transform;
            }
            else
            {
                target = null;
            }

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

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

            lineRenderer.SetPosition(0, bulletOrigin.position);
            lineRenderer.SetPosition(1, target.position);

            Vector3 dir = bulletOrigin.position - target.position;

            impactEffect.transform.position = target.position + dir.normalized;

            impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        }

        void Shoot()
        {
            Vector3 shootDirection;
            if (target) shootDirection = target.position - bulletOrigin.position;
            else return;

            if (Time.realtimeSinceStartup - lastFired_Auto > fireRatePrimary)
            {
                BulletPoolManager.BulletConfig bulletConf = new BulletPoolManager.BulletConfig();
                bulletConf.Prefab = bulletPrimary;
                bulletConf.Position = bulletOrigin.position;
                bulletConf.Rotation = Quaternion.identity;
                bulletConf.Parent = null;
                bulletConf.Duration = 5f;

                GameObject auto = GameplaySceneContext.BulletPoolManager.GetBulletToFire(bulletConf);

                rb_auto = auto.GetComponent<Rigidbody>();
                rb_auto.AddForce(shootDirection.normalized * shootForcePrimary);
                lastFired_Auto = Time.realtimeSinceStartup;

                FMODUnity.RuntimeManager.PlayOneShot(m_enemyAudioData.bulletPrimary[0], transform.position);
            }
        }

        private System.Action onDespawnReset(GameObject bullet)
        {
            bullet.SetActive(false);

            // Zero out previous velocity
            Rigidbody bulletRb = bullet.gameObject.GetComponent<Rigidbody>();
            if (bulletRb)
                bulletRb.velocity = Vector3.zero;

            // Return bullet pos/rot to origin
            bullet.transform.SetPositionAndRotation(bulletOrigin.position, Quaternion.identity);

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