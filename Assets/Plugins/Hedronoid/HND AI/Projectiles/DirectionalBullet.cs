using UnityEngine;
using System.Collections.Generic;
using Hedronoid.AI;
using System.Collections;
using Hedronoid;
using Hedronoid.Events;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.Weapons
{
    public class DirectionalBullet : HNDGameObject
    {
        private float m_BulletTime = 10f;
        private float m_BulletDamage = 0f;
        private float m_BornStamp;
        [SerializeField]
        private Transform m_Target = null;
        private float m_Force;
        private HerdMemberNavigation m_HMN;
        private int m_layerToHit = -1;
        private GameObject m_TrailRenderer;
        private GameObject m_TrailRendererParticle;
        private GameObject m_ImpactParticle;

        private bool m_Fired = false;
        private bool m_IsObstacle = false;
        private float m_RotatePreference;
        private float m_UpwardsForce = 4f;
        private float m_UpwardsTime = 1f;

        private AIBaseSensor m_Sensor;
        private AIBaseNavigation m_Navigation;
        private AIBasePlanner m_Planner;
        private AIBaseMotor m_Motor;
        private int m_TargetLayer;
        private int m_SenderLayer = -1;

        private float m_Reverse = 1f;
        private float m_TargetUpwardsForce = 0f;

        public float BulletDamage
        {
            get { return m_BulletDamage; }
            set { m_BulletDamage = value; }
        }

        public Transform Target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;
                m_TargetLayer = m_Target.gameObject.layer;
            }
        }

        public float Force
        {
            get { return m_Force; }
            set { m_Force = value; }
        }

        public float BulletTime
        {
            get { return m_BulletTime; }
            set { m_BulletTime = value; }
        }

        public int LayerToHit
        {
            get
            {
                if (m_layerToHit < 0)
                    m_layerToHit = LayerMask.NameToLayer("Enemies");
                return m_layerToHit;
            }
            set { m_layerToHit = value; }
        }

        public bool Fired
        {
            get { return m_Fired; }
            set { m_Fired = value; }
        }

        public GameObject TrailRenderer
        {
            get { return m_TrailRenderer; }
            set { m_TrailRenderer = value; }
        }

        public GameObject ImpactParticle
        {
            get { return m_ImpactParticle; }
            set { m_ImpactParticle = value; }
        }

        public float UpwardsForce
        {
            get { return m_UpwardsForce; }
            set { m_UpwardsForce = value; }
        }

        // private AutoSwordSlash m_SwordSlash;
        // public AutoSwordSlash SwordSlash
        // {
        //     get { return m_SwordSlash; }
        //     set { m_SwordSlash = value; }
        // }

        public float UpwardsTime
        {
            get { return m_UpwardsTime; }
            set { m_UpwardsTime = value; }
        }

        public int SenderLayer
        {
            get { return m_SenderLayer; }
            set { m_SenderLayer = value; }
        }

        public float TargetUpwardsForce
        {
            get { return m_TargetUpwardsForce; }
            set { m_TargetUpwardsForce = value; }
        }

        public GameObject TrailRendererParticle
        {
            get
            {
                return m_TrailRendererParticle;
            }

            set
            {
                m_TrailRendererParticle = value;
            }
        }

        public Material TrailMaterial;

        protected override void Start()
        {
            base.Start();
            m_BornStamp = Time.time;
            if (!m_HMN)
                m_HMN = GetComponent<HerdMemberNavigation>();
            if (m_HMN)
                m_HMN.DeactivateHerd(true);
            if (TrailRendererParticle)
            {

                TrailRenderer = Instantiate(TrailRendererParticle);
                TrailRenderer.transform.position = transform.position;
                TrailRenderer.transform.parent = transform;
            }

            m_Planner = gameObject.GetComponent<AIBasePlanner>();
            if (m_Planner) m_Planner.enabled = false;

            m_Navigation = gameObject.GetComponent<AIBaseNavigation>();
            if (m_Navigation) m_Navigation.enabled = false;

            m_Sensor = gameObject.GetComponent<AIBaseSensor>();
            if (m_Sensor) m_Sensor.enabled = false;

            m_Motor = gameObject.GetComponent<AIBaseMotor>();
            if (m_Motor) m_Motor.enabled = false;
        }

        public void FireMagicBullet(bool reverse = false)
        {
            if (!cachedRigidbody)
            {
                DestroyBullet();
                return;
            }
            if (m_HMN)
                m_HMN.DeactivateHerd(true);
            if (reverse)
                m_Reverse = -1;
            StartCoroutine(FireMagic());
        }

        IEnumerator FireMagic()
        {
            if (cachedRigidbody)
            {
                cachedRigidbody.velocity = Vector3.zero;
                cachedRigidbody.angularVelocity = Vector3.zero;
            }
            if (m_Target != null && m_Target.gameObject.activeSelf)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation((m_Target.position - transform.position) * m_Reverse), 0f);
                if (cachedRigidbody)
                    cachedRigidbody.AddForce(Vector3.up * UpwardsForce + (m_Target.position - transform.position).normalized * TargetUpwardsForce, ForceMode.VelocityChange);
            }
            else
            {
                if (cachedRigidbody)
                    cachedRigidbody.AddForce(Vector3.up * UpwardsForce, ForceMode.VelocityChange);
            }
            yield return new WaitForSeconds(UpwardsTime);
            if (cachedRigidbody)
                cachedRigidbody.angularVelocity = Vector3.zero;

            // if (SwordSlash)
            //     SwordSlash.Slash();

            Fired = true;
        }
        public void DestroyBullet()
        {
            if (m_HMN)
                m_HMN.ActivateHerd(true);
            if (TrailRenderer)
                Destroy(TrailRenderer);
            if (m_Planner) m_Planner.enabled = true;

            if (m_Navigation) m_Navigation.enabled = true;

            if (m_Sensor) m_Sensor.enabled = true;

            if (m_Motor) m_Motor.enabled = true;

            Destroy(this);
        }
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerToHit || (collision.transform == Target && m_BulletDamage > 0))
            {
                //Debug.Log("Collided with 1: " + collision.gameObject.name + " " + BulletDamage);
                HNDEvents.Instance.Raise(new DoDamage
                {
                    Damage = BulletDamage,
                    GOID = collision.gameObject.GetInstanceID(),
                    sender = gameObject,
                    Skill = "",
                    Particle = null,
                    DamageDirection = Vector3.zero,
                    Type = EDamageType.Fire
                });
                if (m_ImpactParticle)
                {
                    GameObject particle = Instantiate(m_ImpactParticle);
                    particle.transform.position = transform.position;
                    Destroy(particle, 2f);
                }
                DestroyBullet();
            }
            else if (m_BulletDamage == 0 || !m_Target || !m_Target.gameObject.activeSelf)
            {
                //Debug.Log("Collided with 2: " + collision.gameObject.name + m_BulletDamage + (m_Target ==null));

                if (m_ImpactParticle)
                {
                    GameObject particle = Instantiate(m_ImpactParticle);
                    particle.transform.position = transform.position;
                    Destroy(particle, 2f);
                }
                DestroyBullet();
            }
        }

        private void FixedUpdate()
        {
            if (m_BornStamp + BulletTime < Time.time)
            {
                //Debug.Log(" - Destroy bullet - " + m_BornStamp + " " + BulletTime + " " + Time.time);

                DestroyBullet();
            }
            if (m_Target != null && m_Target.gameObject.activeSelf && Fired)
            {
                if (m_TargetLayer == m_SenderLayer && Vector3.Distance(transform.position, m_Target.position) < 2.4f)
                {
                    Debug.Log("Delivered!");
                    if (cachedRigidbody)
                    {
                        cachedRigidbody.angularVelocity = Vector3.zero;
                        cachedRigidbody.velocity = Vector3.zero;
                    }
                    DestroyBullet();
                    return;
                }
                //cachedRigidbody.angularVelocity = Vector3.zero;
                Vector3 forward = (m_Target.position - transform.position).normalized * m_Reverse;
                if (!m_IsObstacle)
                {
                    m_RotatePreference = Random.Range(0, 1f) > 0.5f ? 1f : -1f;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation((m_Target.position - transform.position) * m_Reverse), Time.fixedDeltaTime);
                }
                //Use Phyics.RayCast to detect the obstacle
                RaycastHit hit;
                RaycastHit hit2;
                //forward *= -1;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                // Now Two More RayCast At The End of Object to detect that object has already pass the obsatacle.
                // Just making this boolean variable false it means there is nothing in front of object.
                if (Physics.Raycast(transform.position - (forward * 0.7f), right, out hit, 1f, LayerMask.GetMask(new string[] { "Default", "Ground" })) ||
                 Physics.Raycast(transform.position - (forward * 0.7f), -right, out hit, 1f, LayerMask.GetMask(new string[] { "Default", "Ground" })))
                {
                    m_IsObstacle = false;
                }

                bool hitbool = Physics.Raycast(transform.position + (right * 0.7f), forward, out hit, 1f, LayerMask.GetMask(new string[] { "Default", "Ground" }));
                bool hit2bool = Physics.Raycast(transform.position - (right * 0.7f), forward, out hit2, 1f, LayerMask.GetMask(new string[] { "Default", "Ground" }));
                if (hitbool || hit2bool)
                {
                    m_IsObstacle = true;
                    transform.Rotate(m_RotatePreference * Vector3.up * Time.deltaTime * 2f);   // rotation speed 2f
                    if (m_Reverse > -1)
                    {
                        if (Target.position.y > transform.position.y)
                            transform.Rotate(m_RotatePreference * Vector3.right * Time.deltaTime * 1f);
                        if (Target.position.y < transform.position.y)
                            transform.Rotate(m_RotatePreference * -Vector3.right * Time.deltaTime * 0.9f);
                    }
                }

                if (m_Reverse < 1)
                {
                    forward.y = -.2f;
                }
                // Use to debug the Physics.RayCast.
                Debug.DrawRay(transform.position + (right * 0.7f), forward * 1f, Color.red);
                Debug.DrawRay(transform.position - (right * 0.7f), forward * 1f, Color.red);
                Debug.DrawRay(transform.position - (forward * 0.7f), -right * 1f, Color.yellow);
                Debug.DrawRay(transform.position - (forward * 0.7f), right * 1f, Color.yellow);

                //cachedRigidbody.velocity = (m_Target.position - transform.position).normalized * m_Force;

                cachedRigidbody.velocity = forward * m_Force;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (m_Target)
                Gizmos.DrawLine(transform.position, transform.position + (m_Target.position - transform.position).normalized * m_Force);
        }
#endif

    }
}