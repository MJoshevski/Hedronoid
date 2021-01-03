using Hedronoid.Events;
using Hedronoid.AI;
using Hedronoid.Fire;
using Hedronoid.Health;
using Hedronoid.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Tooltip("This is shown as soon as the attack has a position")]
    [SerializeField]
    GameObject m_InitialMark;
    
    [Tooltip("This will grow until impact")]
    [SerializeField]
    GameObject m_Shadow;

    [Tooltip("The impact particle")]
    [SerializeField]
    GameObject m_ImpactParticle;
    
    [Tooltip("If we burn the players when they get hit")]
    [SerializeField]
    bool m_SetOnFire = true;

    [Tooltip("The size of the impact area")]
    [SerializeField]
    float m_Radius = 4f;

    [SerializeField]
    [Tooltip("This is the gameObject that travels from the shooter towards the target.")]
    private GameObject m_VisualProjectile;

    [SerializeField]
    private LayerMask m_groundLayer;
    private const float GroundDetectDistance = 300f;

    [SerializeField]
    float m_ImpactForce;
    [SerializeField]
    float m_Damage;
    [SerializeField]
    protected float m_WarningTime = 2f;

    [SerializeField]
    protected float m_BulletsTargetDistance = 10;
    [SerializeField]
    protected float m_BulletsShootStrength = 10;
    [SerializeField]
    protected float m_BulletsDamage = 1;

    [SerializeField]
    protected AnimationCurve m_bulletHeightCurve;
    [SerializeField]
    protected float m_bulletHeightAtApex = 4;
    
    protected string m_ThrowAudio = "Throw";
    protected string m_ImpactAudio = "Impact";

    // Use this for initialization
    public void Init(Transform shooterTransform)
    {
        //Debug.Log("Ranged A-to the double T to the ack!");
        m_ShooterPosition = shooterTransform.position;

        // Make sure that we are grounded
        // First make sure that the destination is grounded
        RaycastHit rh;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out rh, GroundDetectDistance, m_groundLayer))
        {
            transform.position = rh.point;
        }
        StartCoroutine(RangedAttackSequence());
    }


    GameObject m_DamageParticleEffects;
    string m_Skill;

    Vector3 m_ShooterPosition;
    
    // Update is called once per frame
    IEnumerator RangedAttackSequence()
    {
        //m_InitialMark.SetActive(false);
        //m_Shadow.SetActive(false);

        m_InitialMark.transform.localScale = new Vector3(m_Radius, m_InitialMark.transform.localScale.y, m_Radius);
        m_Shadow.transform.localScale = new Vector3(0f, m_Shadow.transform.localScale.y, 0f);

        var visuals = GameObject.Instantiate(m_VisualProjectile, transform.position, transform.rotation);
        float t = 0;
        var remainingTime = m_WarningTime;

        var startY = visuals.transform.position.y;
        
        // SoundRouter.CutsceneAudioFilter(m_ThrowAudio);

        // This is where the projectile is flying. It flies in a curved trajectory controlled by the bulletHeightCurve and bulletHeightAtApex.
        while ((remainingTime -= Time.fixedDeltaTime) > 0)
        {
            var progress = (m_WarningTime - remainingTime) / m_WarningTime;
            var yModifier = m_bulletHeightCurve.Evaluate(progress) * m_bulletHeightAtApex;
            t += Time.fixedDeltaTime;
            var shadowRadius = Mathf.Lerp(0, m_Radius, progress);
            m_Shadow.transform.localScale = new Vector3(shadowRadius, m_Shadow.transform.localScale.y, shadowRadius);
            visuals.transform.position = Vector3.Lerp(m_ShooterPosition ,new Vector3(transform.position.x, transform.position.y + yModifier, transform.position.z), progress);
            yield return new WaitForFixedUpdate();
        }
        m_Shadow.SetActive(false);
        Destroy(visuals);
        
        // SoundRouter.CutsceneAudioFilter(m_ImpactAudio);

        //Spawn fire
        var fire = GameObject.Instantiate(m_ImpactParticle, transform.position, Quaternion.identity);
        Destroy(fire, 1.2f);

        List<Collider> collisionsTarget = new List<Collider>(Physics.OverlapSphere(transform.position, m_Radius, LayerMask.GetMask(new string[] {  "Players", "NPCs", "Enemies" }),QueryTriggerInteraction.Ignore));
        if (collisionsTarget.Contains(gameObject.GetComponent<Collider>()))
        {
            collisionsTarget.Remove(gameObject.GetComponent<Collider>());
        }

        // List<NPC> npcs = new List<NPC>();
        Transform damageTransform;
        Vector3 damageDirection;
        // NPC npcAux;

        //Debug.Log("RangedAttackSequence 2 ");
        
        foreach (Collider col in collisionsTarget)
        {
            damageTransform = col.transform;
            if (col.attachedRigidbody)
            {
                damageTransform = col.attachedRigidbody.transform;
            }
            if (m_SetOnFire)
            {
                OnFire colFire = damageTransform.GetComponent<OnFire>();
                if (colFire)
                    colFire.SetOnFire();
            }

            damageDirection = Vector3.zero;
            damageDirection = (damageTransform.position - transform.position).normalized;
           
            if (col.attachedRigidbody)
                col.attachedRigidbody.AddForce(damageDirection * m_ImpactForce, ForceMode.Impulse);
            
            //Debug.Log("Yeah I will oblitterate you. ID: "+ damageTransform.gameObject.GetInstanceID());
            if(col.gameObject.tag == "Enemy")
            {
                var hp = col.gameObject.GetComponent<HealthBase>();
                if (hp && hp.RecieveFriendlyFire && hp.CanBeOneShotted)
                {
                    HNDEvents.Instance.Raise(new DoDamage { Damage = hp.MaxHealth, GOID = damageTransform.gameObject.GetInstanceID()});
                }
            }
            else
            {
                HNDEvents.Instance.Raise(new DoDamagePlayer { Damage = m_Damage, RecieverGOID = damageTransform.gameObject.GetInstanceID(), SenderGOID = gameObject.GetInstanceID() });

            }

            m_InitialMark.SetActive(false);
        }
        
        yield return new WaitForSeconds(.5f);

        //Debug.Log("Ranged attack sequence ended");

        Destroy(gameObject, .1f);
        yield break;
    }
}

