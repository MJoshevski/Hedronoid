using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.AI;
using Hedronoid;
using UnityEngine.AI;
using Hedronoid.Health;
using Hedronoid.Events;
using Hedronoid.Weapons;

namespace Hedronoid.AI
{
    public class BullDash : AIBaseMotor
    {
        protected Rigidbody m_BullRb;
        protected Vector3 upAxis;

        [Header("Dash controls")]
        [SerializeField]
        [Tooltip("The grunt will turn towards you before dashing. This is how far it turns.")]
        protected float m_turnRate;

        public float TurnRate
        {
            get { return m_turnRate; }
        }

        [SerializeField]
        [Tooltip("After turning, The grunt will wait a bit before dashing.")]
        protected float m_windupTime = 1;
        [SerializeField]
        protected float m_dashSpeed = 15;
        [SerializeField]
        [Tooltip("Dash for this long")]
        protected float m_dashTime = 1;
        [SerializeField]
        [Tooltip("After dashing, The bull will wait a bit before resuming navigation.")]
        protected float m_cooldownTime = 5;
        [SerializeField]
        [Tooltip("After dashing, The bull will be vulnerable for a while.")]
        protected float m_VulnerableTimeAfterDash = 4f;
        [Tooltip("Distance of the ground contact probe.")]
        [Min(0f)]
        public float m_dashProbeDistance = 5f;
        [Tooltip("Layer mask for ground contact probing and stairs.")]
        public LayerMask m_probeMask = -1;

        protected BullNavigation m_BullNavigation;
        protected BullSensor m_BullSensor;
        protected Material m_sharedMaterial;
        protected DamageHandler m_DamageHandler;

        [SerializeField]
        protected Animator animator;

        [SerializeField]
        protected bool dashInProgress = false;

        public bool DashInProgress
        {
            get { return dashInProgress; }
        }

        protected bool dashDamage = false;
        [SerializeField]
        protected float m_Damage = 1f;
        [SerializeField]
        protected GameObject m_DashStartParticle;
        protected GameObject m_InstantiatedDashStartParticle;

        private float m_tempStorageRate;

        protected override void Awake()
        {
            base.Awake();
            //animator = GetComponentInChildren<Animator>();

            m_tempStorageRate = m_turnRate;
            m_DamageHandler = GetComponent<DamageHandler>();
            m_BullNavigation = GetComponent<BullNavigation>();
            m_BullSensor = GetComponent<BullSensor>();
            m_BullRb = GetComponent<Rigidbody>();

            //m_sharedMaterial = m_BullNavigation.GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
            
            if (m_DamageHandler)
                m_DamageHandler.IsInvulnerable = false;

            HNDEvents.Instance.AddListener<KillEvent>(KillEvent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            HNDEvents.Instance.RemoveListener<KillEvent>(KillEvent);

        }

        protected override void OnDisable()
        {
            base.OnDisable();

            HNDEvents.Instance.RemoveListener<KillEvent>(KillEvent);

        }

        void KillEvent(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;
            ReturnToIdle();
        }

        protected override void Update()
        {
            base.Update();

            m_turnRate = m_tempStorageRate;
        }

        public void DoDash(Transform target)
        {
            if (!gameObject.activeInHierarchy) return;

            if (!dashInProgress)
            {
                StartCoroutine(Dash(target));
            }
        }

        protected void ReturnToIdle()
        {
            StopCoroutine("Dash");

            SetAnimatorTrigger("ForceIdle");
            dashInProgress = false;
            dashDamage = false;
            if (m_DamageHandler)
                m_DamageHandler.IsInvulnerable = false;

            if (m_InstantiatedDashStartParticle != null) Destroy(m_InstantiatedDashStartParticle);
            if (m_Navigation is BullNavigation)
            {
                (m_Navigation as BullNavigation).DashDone();
            }
        }

        protected IEnumerator DashParticle()
        {
            m_InstantiatedDashStartParticle = Instantiate(m_DashStartParticle, transform.position + transform.forward * 0.5f, Quaternion.identity);
            m_InstantiatedDashStartParticle.transform.parent = transform;
            float timestamp = Time.time;
            while (timestamp + 0.8f > Time.time)
            {
                if (m_InstantiatedDashStartParticle != null)
                {
                    if (cachedRigidbody.velocity.magnitude == 0f)
                    {
                        m_InstantiatedDashStartParticle.transform.rotation = cachedRigidbody.transform.rotation;

                    }
                    else if (cachedRigidbody.velocity != Vector3.zero)
                    {
                        m_InstantiatedDashStartParticle.transform.rotation = Quaternion.LookRotation(cachedRigidbody.velocity);

                    }
                    m_InstantiatedDashStartParticle.transform.localScale = Vector3.one * 4f;
                }
                else
                {
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }
            Destroy(m_InstantiatedDashStartParticle);
            yield return null;
        }
        protected virtual IEnumerator Dash(Transform target)
        {
            //Turn invulnerable at dash start
            if (dashInProgress)
                yield break;
            dashInProgress = true;
            
            // SoundRouter.CutsceneAudioFilter("Blockhead_Prepare");

            SetAnimatorTrigger("DoDash");

            yield return new WaitForSeconds(1.2f);

            // Turn towards the target direction while winding up, and then go, remove y so we don't look up or down.
            var remainingTime = m_windupTime;
            var distanceToTarget = 0f;
            float t = 0f;

            while ((remainingTime -= Time.deltaTime) > 0)
            {
                if (t <= 1)
                {
                    // if end color not reached yet...
                    t += Time.deltaTime / m_windupTime; // advance timer at the right speed
                    //m_sharedMaterial.color = Color.Lerp(Color.white, Color.red, t);
                }

                TurnTowardsTarget(target);
                distanceToTarget = Vector3.Distance(transform.position, target.position);
                yield return null;
            }

            if (distanceToTarget > m_BullSensor.SensorCutoffRange)
            {
                //animator.SetTrigger("ForceIdle");

                if (m_InstantiatedDashStartParticle != null) Destroy(m_InstantiatedDashStartParticle);
                dashDamage = false;

                if (m_DamageHandler)
                    m_DamageHandler.IsInvulnerable = false;
                dashInProgress = false;

                if (!m_Navigation.OnImpact)
                {
                    cachedRigidbody.isKinematic = true;
                    cachedRigidbody.velocity = Vector3.zero;
                    cachedRigidbody.angularVelocity = Vector3.zero;
                    cachedRigidbody.isKinematic = false;
                }

                if (m_Navigation is BullNavigation)
                {
                    (m_Navigation as BullNavigation).DashDone();
                }

                yield break;
            }

            // Do the dash!            
            // SoundRouter.CutsceneAudioFilter("Blockhead_Charge");
            FMODUnity.RuntimeManager.PlayOneShotAttached(
                m_BullNavigation.m_EnemyAudioData.primaryAttack[
                    Random.Range(0, m_BullNavigation.m_EnemyAudioData.primaryAttack.Length - 1)], gameObject);

            SetAnimatorTrigger("DashAttack");
            if (m_DashStartParticle && gameObject.activeInHierarchy)
            {
                StartCoroutine(DashParticle());
            }
            remainingTime = m_dashTime;
            var targetDir = TurnTowardsTarget(target);

            var lookRot = Quaternion.LookRotation(targetDir);
            while ((remainingTime -= Time.fixedDeltaTime) > 0 && dashInProgress)
            {
                if (!m_Navigation.OnImpact)
                {
                    cachedRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, m_turnRate * Time.fixedDeltaTime);
                    cachedRigidbody.angularVelocity = Vector3.zero;
                    dashDamage = true;
                    
                    if (dashInProgress)
                    {
                        cachedRigidbody.transform.position +=
                            targetDir * Time.fixedDeltaTime * m_dashSpeed;
                        //cachedRigidbody.AddForce(targetDir * Time.fixedDeltaTime * m_dashSpeed, ForceMode.VelocityChange);
                    }
                    yield return new WaitForFixedUpdate();
                }
            }

            dashDamage = false;
            if (!m_Navigation.OnImpact)
            {
                cachedRigidbody.velocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }

            //Turn vulnerable after dash and enable the death touch
            // SoundRouter.CutsceneAudioFilter("Blockhead_Confused");

            SetAnimatorTrigger("TurnVulnerable");
            if (!dashInProgress)
            {
                float vunerableTimeStamp = Time.time;

                while (vunerableTimeStamp + m_VulnerableTimeAfterDash > Time.time)
                {
                    if (!m_Navigation.OnImpact)
                    {
                        cachedRigidbody.velocity = new Vector3(0f, cachedRigidbody.velocity.y, 0f);// Vector3.zero;
                        cachedRigidbody.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }
            }

            //m_sharedMaterial.color = Color.white;

            SetAnimatorTrigger("TurnInvulnerable");
            yield return new WaitForSeconds(1f); // giving animation one second to stand up without turning when no cooldown is used... or whatever.

            dashInProgress = false;
            if (m_Navigation is BullNavigation)
            {
                (m_Navigation as BullNavigation).DashDone();
            }
        }

        void SnapToGround()
        {
            if (!Physics.Raycast(
                m_BullRb.position, -upAxis, out RaycastHit hit,
                m_dashProbeDistance, m_probeMask
            ))
            {
                return;
            }

            float upDot = Vector3.Dot(-upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
            {
                return;
            }
            m_BullRb.position = hit.point;
            Popcron.Gizmos.Circle(hit.point, 1f, Camera.main, Color.red);
        }

        private float minGroundDotProduct, minStairsDotProduct;
        public float GetMinDot(int layer)
        {
            return (m_probeMask & (1 << layer)) == 0 ?
                minGroundDotProduct : minStairsDotProduct;
        }
        protected virtual Vector3 TurnTowardsTarget(Transform target)
        {
            var targetPos = target.position;

            var lookDir = (targetPos - transform.position).normalized;
            var lookRot = Quaternion.LookRotation(lookDir);
            cachedRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, m_turnRate * Time.deltaTime);
            return lookDir;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag(HNDAI.Settings.PlayerTag) || !dashInProgress || !dashDamage)
            {
                return;
            }

            if (!m_Navigation.OnImpact)
            {
                cachedRigidbody.isKinematic = true;
                cachedRigidbody.velocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
                cachedRigidbody.isKinematic = false;
                dashInProgress = false;
            }

            HNDEvents.Instance.Raise(new DoDamagePlayer
            {
                Damage = m_Damage,
                RecieverGOID = collision.gameObject.GetInstanceID(),
                SenderGOID = gameObject.GetInstanceID(),
                Collision = collision
            });
        }

        protected void SetAnimatorTrigger(string name)
        {
            if (animator != null)
            {
                animator.SetTrigger(name);
            }
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            SnapToGround();

            GravityService.CurrentGravity =
             GravityService.GetGravity(m_BullRb.position, out upAxis);
        }
    }
}
