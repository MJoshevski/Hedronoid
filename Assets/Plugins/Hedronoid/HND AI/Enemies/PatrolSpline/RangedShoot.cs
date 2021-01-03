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
    public class RangedShoot : AIBaseMotor
    {
        [Header("Ranged controls")]
        [SerializeField]
        [Tooltip("The blockhead will turn towards you before dashing. This is how far it turns.")]
        private float m_turnRate;
        [SerializeField]
        [Tooltip("After turning, The blockhead will wait a bit before dashing.")]
        private float m_windupTime = 1;
        [SerializeField]
        private float m_dashSpeed = 15;
        [SerializeField]
        [Tooltip("Dash for this long")]
        private float m_dashTime = 1;
        [SerializeField]
        [Tooltip("After dashing, The blockhead will wait a bit before resuming navigation.")]
        private float m_cooldownTime = 5;
        [SerializeField]
        [Tooltip("After dashing, The blockhead will be vulnerable for a while.")]
        private float m_VulnerableTimeAfterDash = 4f;
        
        private GameObject[] m_Players;

        [SerializeField]
        [Tooltip("This is the gameObject we spawn when firing.")]
        private GameObject m_Projectile;

        [SerializeField]
        private int m_BurstCounter = 0;
        [SerializeField]
        private int m_NumberOfBursts = 3;
        [SerializeField]
        private int m_NumberOfTurboBursts = 12;
        [SerializeField]
        private int m_DoTurboBurstsEverSoOften = 3;
        [SerializeField]
        private float m_DelayBetweenBursts = .21f;
        [SerializeField]
        private float m_DelayBetweenTurboBursts = .17f;
        [SerializeField]
        private float m_Spread = 3f;
        [SerializeField]
        private float m_MaxSeparationOfProjectileTurboBurst = 2.5f;
        [SerializeField]
        private float m_ExtraWaitingTimeBeforeTurboBurst = .5f;
        [SerializeField]
        private float m_SafetyDistance = 0f;

        private int m_NumberOfTimesFired = 0;

        private DamageHandler m_DamageHandler;

        private Animator animator;

        private bool shotInProgress = false;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            m_DamageHandler = GetComponent<DamageHandler>();
            m_Players = GameObject.FindGameObjectsWithTag("Player");
        }

        public override void Move(Vector3 target)        
        {
        }

        public void DoShoot(Transform target)
        {
            if (!shotInProgress && target && !m_Navigation.IsFrozen)
            {
                StartCoroutine(Shoot(target));
            }                       
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
            m_NumberOfTimesFired++;
            m_BurstCounter = 0;
            /*
            m_NumberOfBursts = 3;
            m_NumberOfTurboBursts = 12;
            m_DoTurboBurstsEverSoOften = 3;
            m_DelayBetweenBursts = .21f;
            m_DelayBetweenTurboBursts = .17f;
            m_Spread = 3f;
            m_MaxSeparationOfProjectileTurboBurst = 2.5f;
            m_ExtraWaitingTimeBeforeTurboBurst = .5f;
            */
            var lastPosition = target.position;
            float dist = 0f;
            var offset = Vector3.zero;

            // SoundRouter.CutsceneAudioFilter("Grenade_SetMark");

            //Debug.Log("m_NumberOfTimesFired!  "+ m_NumberOfTimesFired);
            if (m_NumberOfTimesFired % m_DoTurboBurstsEverSoOften == 0)
            {
                yield return new WaitForSeconds(m_ExtraWaitingTimeBeforeTurboBurst);
                //Turbo bursts
                //Debug.Log("TurboBurst!");
                while (m_BurstCounter < m_NumberOfTurboBursts)
                {
                    m_BurstCounter++;
                    if (Vector3.Distance(target.position, transform.position) < m_SafetyDistance)
                        continue;
                    GameObject p1 = target.gameObject;
                    GameObject p2;

                    Vector3[] burstTargets = new Vector3[m_NumberOfTurboBursts];

                    TurnTowardsTarget(target);
                    if (m_Players.Length > 1)
                    {
                        if (target.gameObject == m_Players[0])
                        {
                            p2 = m_Players[1];
                        }
                        else
                        {
                            p2 = m_Players[0];
                        }
                        dist = Vector3.Distance(p1.transform.position, p2.transform.position);

                        //Debug.Log("set dist " + dist + "   ");
                        var tempTarget = Vector3.zero;
                        //burstTargets[0] = p1.transform.position;
                        burstTargets[0] = new Vector3(p1.transform.position.x, p1.transform.position.y, p1.transform.position.z);

                        for (int i = 1;i < burstTargets.Length; i++)
                        {
                            float nOtBf = (float)m_NumberOfTurboBursts;
                            //Debug.Log("This dist: " + (i / nOtBf) * dist);
                            tempTarget = Vector3.MoveTowards(p1.transform.position, p2.transform.position, Mathf.Min(m_MaxSeparationOfProjectileTurboBurst,(i / nOtBf) *dist));
                            burstTargets[i] = new Vector3(tempTarget.x, tempTarget.y, tempTarget.z);// tempTarget.position;// Vector3.Lerp(, p2.transform.position, i / numberOfTurboBursts);

                            //Debug.Log("set burstTargets[" + i + "] " + tempTarget+ " tempTarget.position");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < burstTargets.Length; i++)
                        {
                            burstTargets[i] = target.position;
                            //Debug.Log("set burstTargets["+i+"] "+target.position + " target.position");
                        }
                    }
                    /*
                    offset.x = UnityEngine.Random.Range(-spread /2f, spread / 2f);
                    offset.z = UnityEngine.Random.Range(-spread / 2f, spread / 2f);
                    */
                    //var projectile = GameObject.Instantiate(m_Projectile, target.position + offset, Quaternion.identity);
                    //Debug.Log(burstTargets[m_BurstCounter - 1]+" WTFWTFWTWF");
                    if (!m_Navigation.IsFrozen)
                    {
                        var projectile = GameObject.Instantiate(m_Projectile, burstTargets[m_BurstCounter - 1] + offset, Quaternion.identity);
                        projectile.GetComponent<RangedAttack>().Init(transform);
                        
                        // SoundRouter.CutsceneAudioFilter("Throw");
                    }
                    

                    yield return new WaitForSeconds(m_DelayBetweenTurboBursts);                
                }
            }
            else
            {
                //Normal bursts
                var pos = target.position;
                while (m_BurstCounter < m_NumberOfBursts)
                {
                    if(m_BurstCounter > 0)
                    {
                        //offset = Random.Range()
                        offset.x = UnityEngine.Random.Range(-m_Spread /2f, m_Spread / 2f);
                        offset.z = UnityEngine.Random.Range(-m_Spread / 2f, m_Spread / 2f);
                    }
                    m_BurstCounter++;

                    if (Vector3.Distance(target.position, transform.position) < m_SafetyDistance)
                        continue;
                    TurnTowardsTarget(target);
                    if (!m_Navigation.IsFrozen)
                    {
                        var projectile = GameObject.Instantiate(m_Projectile, target.position + offset, Quaternion.identity);
                        projectile.GetComponent<RangedAttack>().Init(transform);
                    }

                    yield return new WaitForSeconds(m_DelayBetweenBursts);
                }
            }
            
            
            //cachedRigidbody.velocity = Vector3.zero;
            
            shotInProgress = false;
            
            // I don't think i need thsi /Oll3
            if (m_Navigation is RangedNavigation)
            {
                (m_Navigation as RangedNavigation).ShotDone();
            }

            // if (m_Navigation is FredNavigation)
            // {
            //     (m_Navigation as FredNavigation).ShotDone();
            // }

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