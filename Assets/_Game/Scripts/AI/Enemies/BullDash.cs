using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid.AI;
using Hedronoid;
using UnityEngine.AI;

namespace Hedronoid.AI
{
    public class BullDash : GruntDash
    {
        protected Rigidbody m_GruntRb;
        protected NavMeshAgent agent;
        protected Vector3 upAxis;

        protected override void Awake()
        {
            base.Awake();
            m_GruntRb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            GravityService.CurrentGravity =
             GravityService.GetGravity(m_GruntRb.position, out upAxis);
        }

        protected override IEnumerator Dash(Transform target)
        {
            //Turn invulnerable at dash start
            if (dashInProgress)
                yield break;
            dashInProgress = true;

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

            if (distanceToTarget > m_GruntSensor.SensorCutoffRange)
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

            var lookRot = Quaternion.LookRotation(targetDir, upAxis);
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

        protected override Vector3 TurnTowardsTarget(Transform target)
        {
            var targetPos = target.position;
            var lookDir = (targetPos - transform.position).normalized;
            var lookRot = Quaternion.LookRotation(lookDir, upAxis);
            cachedRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, m_turnRate * Time.deltaTime);
            return lookDir;
        }
    }
}
