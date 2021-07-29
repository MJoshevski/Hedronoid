using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Hedronoid.Health;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    /// <summary>
    /// This is used for Blockehad grunts to dash forward towards a target.
    /// A call to move will trigger the dash.
    /// </summary>
    public class ArcherShoot : AIBaseMotor
    {
        [Header("Ranged controls")]
        [SerializeField]
        private float m_turnRate;
        [SerializeField]
        [Tooltip("After turning, The blockhead will wait a bit before dashing.")]
        private float m_windupTime = 1;
        
        private GameObject[] m_Players;

        [SerializeField]
        [Tooltip("This is the gameObject we spawn when firing.")]
        private GameObject m_Projectile;
        [SerializeField]
        private Transform m_Muzzle;

        private Animator animator;

        private bool shotInProgress = false;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            m_Players = GameObject.FindGameObjectsWithTag("Player");
        }

        public override void Move(Vector3 target)        
        {
        }

        public bool DoShoot(Transform target)
        {
            if (!shotInProgress && target && !m_Navigation.IsFrozen)
            {
                StartCoroutine(Shoot(target));
                return true;
            }
            return false;
        }

        private IEnumerator Shoot(Transform target)
        {

            //if (shotInProgress || m_Navigation.IsFrozen) yield return null;
            //Turn invulnerable at dash start
            shotInProgress = true;

            //animator.SetTrigger("Shoot");
                        
            // Turn towards the target direction while winding up, and then go, remove y so we don't look up or down.
            var remainingTime = m_windupTime;
            while ((remainingTime -= Time.fixedDeltaTime) > 0)
            {
                TurnTowardsTarget(target);
                yield return new WaitForFixedUpdate();
            }

            // Take the shot!
            GameObject projectile = Instantiate(m_Projectile);
            projectile.transform.position = m_Muzzle.position;
            shotInProgress = false;
            Arrow aproject = projectile.GetComponent<Arrow>();
            if (aproject)
                aproject.ShootAt(target);
        }

        private Vector3 TurnTowardsTarget(Transform target)
        {
            var targetPos = target.position;
            targetPos.y = transform.position.y;
            var lookDir = (targetPos - transform.position).normalized;
            var lookRot = Quaternion.LookRotation(lookDir);
            cachedRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, m_turnRate * Time.fixedDeltaTime);
            return lookDir;
        }
    }
}