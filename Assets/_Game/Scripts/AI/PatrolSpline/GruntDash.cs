using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid.Health;
using Hedronoid.Events;
using Hedronoid.Weapons;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    /// <summary>
    /// This is used for Blockehad grunts to dash forward towards a target.
    /// A call to move will trigger the dash.
    /// </summary>
    public class GruntDash : AIBaseMotor
    {
        [Header("Dash controls")]
        [SerializeField]
        [Tooltip("The blockhead will turn towards you before dashing. This is how far it turns.")]
        protected float m_turnRate;

        public float TurnRate
        {
            get { return m_turnRate; }
        }

        [SerializeField]
        [Tooltip("After turning, The blockhead will wait a bit before dashing.")]
        protected float m_windupTime = 1;
        [SerializeField]
        protected float m_dashSpeed = 15;
        [SerializeField]
        [Tooltip("Dash for this long")]
        protected float m_dashTime = 1;
        [SerializeField]
        [Tooltip("After dashing, The blockhead will wait a bit before resuming navigation.")]
        protected float m_cooldownTime = 5;
        [SerializeField]
        [Tooltip("After dashing, The blockhead will be vulnerable for a while.")]
        protected float m_VulnerableTimeAfterDash = 4f;

        protected GruntNavigation m_GruntNavigation;
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
            m_GruntNavigation = GetComponent<GruntNavigation>();
            m_sharedMaterial = m_GruntNavigation.GetComponentInChildren<SkinnedMeshRenderer>().materials[1];
            if (m_DamageHandler)
                m_DamageHandler.IsInvulnerable = false;
            HNDEvents.Instance.AddListener<KillEvent>(KillEvent);
        }

        void KillEvent(KillEvent e)
        {
            if (e.GOID != gameObject.GetInstanceID()) return;
            ReturnToIdle();
        }

        public override void Move(Vector3 target)        
        {
        }

        protected virtual void Update()
        {
            if (m_GruntNavigation.m_GruntFreeze)
                m_turnRate = 0;
            else
                m_turnRate = m_tempStorageRate;
        }

        public void PointSpear(Transform target)
        {
            if (!dashInProgress)
                StartCoroutine(AntiAir(target));
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
            if (m_Navigation is GruntNavigation)
            {
                (m_Navigation as GruntNavigation).DashDone();
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
                    else if(cachedRigidbody.velocity != Vector3.zero)
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

        private IEnumerator AntiAir(Transform target)
        {
            if (dashInProgress)
                yield break;
          
            SetAnimatorTrigger("DashAttackWarning");

            yield return new WaitForSeconds(3f);

            SetAnimatorTrigger("ForceIdle");

            
            if (m_Navigation is GruntNavigation)
            {
                (m_Navigation as GruntNavigation).PointUpDone();
            }
        }


        protected virtual IEnumerator Dash(Transform target)
        {
            //Turn invulnerable at dash start
            if(dashInProgress)
                yield break;
            dashInProgress = true;

            // SoundRouter.CutsceneAudioFilter("Blockhead_Prepare");

            SetAnimatorTrigger("DashAttackWarning");

            yield return new WaitForSeconds(.5f);

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
                    m_sharedMaterial.color = Color.Lerp(Color.white, Color.red, t);
                }

                TurnTowardsTarget(target);
                distanceToTarget = Vector3.Distance(transform.position, target.position);
                yield return null;
            }

            if (distanceToTarget > m_GruntNavigation.SensorCutoffRange)
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

                if (m_Navigation is GruntNavigation)
                {
                    (m_Navigation as GruntNavigation).DashDone();
                }

                yield break;
            }

            // Do the dash!            
            // SoundRouter.CutsceneAudioFilter("Blockhead_Charge");

            SetAnimatorTrigger("DashAttack");
            if (m_DashStartParticle && gameObject.activeInHierarchy)
            {
                StartCoroutine(DashParticle());
            }
            remainingTime = m_dashTime;
            var targetDir = TurnTowardsTarget(target);
            targetDir.y = 0f;

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
                        cachedRigidbody.AddForce(targetDir * Time.fixedDeltaTime * m_dashSpeed, ForceMode.VelocityChange);
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

            m_sharedMaterial.color = Color.white;

            SetAnimatorTrigger("TurnInvulnerable");
            yield return new WaitForSeconds(1f); // giving animation one second to stand up without turning when no cooldown is used... or whatever.

            dashInProgress = false;
            if (m_Navigation is GruntNavigation)
            {
                (m_Navigation as GruntNavigation).DashDone();
            }
        }

        protected virtual Vector3 TurnTowardsTarget(Transform target)
        {
            var targetPos = target.position;
            targetPos.y = transform.position.y;
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

            HNDEvents.Instance.Raise(new DoDamagePlayer { 
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
    }
}