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
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid
{
    public class TurretNavigation : AIBaseNavigation, IGameplaySceneContextInjector
    {
        public GameplaySceneContext GameplaySceneContext { get; set; }
        public enum ETurretStates
        {
            AttackTarget = EStates.Highest + 1,
        }

        [HideInInspector]
        public Ray LookRay;
        [HideInInspector]
        public RaycastHit RayHit;
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

        protected HNDFiniteStateMachine playerFSM;
        protected UbhShotCtrl shotCtrl;
        protected TurretSensor m_TurretSensor;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            CreateState(ETurretStates.AttackTarget, OnAttackTargetUpdate, null, null);

            TryGetComponent(out shotCtrl);
            m_TurretSensor = (TurretSensor) m_Sensor;

            HNDEvents.Instance.AddListener<KillEvent>(OnKilled);
        }

        protected override void Start()
        {
            base.Start();

            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<KillEvent>(OnKilled);
        }

        public override void OnGoToTargetUpdate()
        {
            throw new NotImplementedException();
        }
        public virtual void OnAttackTargetUpdate()
        {
            if (m_Target == null)
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
                ChangeState(EStates.DefaultMovement);
                return;
            }

            LockOnTarget();
            Shoot();

            if (useLaser)
            {
                Laser();
            }
        }
        public override void OnReturnToDefaultUpdate()
        {
            throw new NotImplementedException();
        }
        public override void OnDefaultMovementUpdate()
        {
            if (m_Target)
                ChangeState(ETurretStates.AttackTarget);
        }

        void UpdateTarget()
        {
            m_Target = m_TurretSensor.GetTargetWithinReach(m_TurretSensor.SensorRange);

            foreach (UbhShotCtrl.ShotInfo ubs in shotCtrl.m_shotList)
            {
                ubs.m_shotObj.m_targetTransform = m_Target;
                ubs.m_shotObj.disableShooting = m_Target != null ? false : true;
            }
        }
        void LockOnTarget()
        {
            Vector3 dir = m_Target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);

            if (partToRotate)
            {
                Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
                partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            }
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
            lineRenderer.SetPosition(1, m_Target.position);

            Vector3 dir = transform.position - m_Target.position;

            impactEffect.transform.position = m_Target.position + dir.normalized;

            impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        }

        void Shoot()
        {
            //FMODUnity.RuntimeManager.PlayOneShot(m_enemyAudioData.bulletPrimary[0], transform.position);
        }

        private void OnKilled(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;
            StopAllCoroutines();
            gameObject.SetActive(false);
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
    }
}