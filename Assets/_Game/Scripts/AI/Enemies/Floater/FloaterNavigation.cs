using Hedronoid;
using Hedronoid.AI;
using Hedronoid.Core;
using Hedronoid.Events;
using Hedronoid.Health;
using Hedronoid.HNDFSM;
using System;
using System.Collections;
using UnityEngine;

namespace UnityMovementAI
{
    public class FloaterNavigation : AIBaseNavigation, IGameplaySceneContextInjector
    {
        [SerializeField]
        protected float detonationRadius = 15f;
        [SerializeField]
        protected float detonationCountdown = 2f;
        [SerializeField]
        protected float detonationForce = 150f;
        [SerializeField]
        protected ForceMode detonationForceMode = ForceMode.Impulse;
        [SerializeField]
        protected GameObject model;
        [SerializeField]
        protected GameObject blastFX;
        [SerializeField]
        protected ParticleSystem deathPfx;

        protected FloaterSensor m_FloaterSensor;

        private SteeringBasics steeringBasics;
        private WallAvoidance wallAvoidance;
        private bool detonationStarted = false;
        public enum EFloaterStates
        {
            AttackTarget = EStates.Highest + 1,
        }
        public GameplaySceneContext GameplaySceneContext { get; set; }

        protected override void Awake()
        {
            base.Awake();
            this.Inject(gameObject);

            TryGetComponent(out m_FloaterSensor);
            TryGetComponent(out wallAvoidance);
            CreateState(EFloaterStates.AttackTarget, OnAttackTargetUpdate, null, null);

            HNDEvents.Instance.AddListener<KillEvent>(OnKilled);

        }
        public override void OnGoToTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public virtual void OnAttackTargetUpdate()
        {
            if (m_Target)
            {
                float distance = Vector3.Distance(m_Target.position, transform.position);

                if (!detonationStarted)
                {
                    if (distance <= steeringBasics.targetRadius)
                    {
                        steeringBasics.Arrive(transform.position);
                        StartDetonationSequence();
                    }
                    else
                    {

                        Vector3 accel = steeringBasics.Arrive(m_Target.position);

                        steeringBasics.Steer(accel);
                        steeringBasics.LookWhereYoureGoing();
                        accel = wallAvoidance.GetSteering();
                        steeringBasics.Steer(accel);

                    }
                }
            }
            else
                ChangeState(EStates.DefaultMovement);

        }

        public override void OnReturnToDefaultUpdate()
        {
            throw new NotImplementedException();
        }

        public override void OnDefaultMovementUpdate()
        {
            if (m_Target)
                ChangeState(EFloaterStates.AttackTarget);
            else steeringBasics.Arrive(transform.position);
        }
        protected override void Start()
        {
            base.Start();

            steeringBasics = GetComponent<SteeringBasics>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            m_Target = m_FloaterSensor.GetTargetWithinReach(m_FloaterSensor.SensorRange);
        }

        private void OnKilled(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;
            StopAllCoroutines();
            this.enabled = false;
        }

        public void StartDetonationSequence()
        {
            detonationStarted = true;
            StartCoroutine(Detonate());
        }

        private IEnumerator Detonate()
        {
            float elapsedTime = 0f;

            while (elapsedTime < detonationCountdown)
            {
                blastFX.transform.localScale =
                    Vector3.Lerp(blastFX.transform.localScale, Vector3.one * detonationRadius, elapsedTime / detonationCountdown);
                elapsedTime += Time.deltaTime;

                // Yield here
                yield return null;
            }

            Collider[] colliders = Physics.OverlapSphere(
                transform.position, detonationRadius, HNDAI.Settings.PlayerLayer | HNDAI.Settings.EnemyLayer);

            foreach (Collider hit in colliders)
            {
                Rigidbody rb;
                HealthBase hb;
                hit.TryGetComponent(out rb);
                hit.TryGetComponent(out hb);

                if (rb != null && rb.gameObject != gameObject)
                {
                    rb.AddExplosionForce(detonationForce, transform.position, detonationRadius, 3.0f, detonationForceMode);

                    if (hb) hb.InstaKill();
                }
            }

            if (deathPfx) deathPfx.Play();
            model.SetActive(false);

            yield return new WaitForSeconds(deathPfx.main.duration);

            m_damageHandler.Die();
        }
    }
}