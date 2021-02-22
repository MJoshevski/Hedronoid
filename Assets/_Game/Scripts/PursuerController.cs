using Hedronoid;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Gizmos = Popcron.Gizmos;
[RequireComponent(typeof(Pursuer))]


public class PursuerController : MonoBehaviour
{
    // TheGrapIsReady() will be executed when the gameObject receives the
    // "TheGraphIsReady" message
    Pursuer thisPursuerInstance;
    public Transform target;
    Vector3 targetOldPos;
    bool graphIsReady = false;
    public float targetLesionAreaRadius;
    public float targetPathUpdateOffset;
    public float circleRadius = 30;
    public float detonationRadius = 15f;
    public float detonationCountdown = 2f;
    public float detonationForce = 150f;
    public ForceMode detonationForceMode = ForceMode.Impulse;
    public ParticleSystem deathPfx;
    public GameObject blastFX;

    private Rigidbody mineRb;
    private MeshRenderer[] mineMeshRenderers;
    private bool detonationStarted = false;

    private void Start()
    {
        thisPursuerInstance = gameObject.GetComponent<Pursuer>();
        mineRb = GetComponent<Rigidbody>();
        mineMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        graphIsReady = false;
        deathPfx.Stop();
    }

    public void TheGraphIsReady()
    {
        graphIsReady = true;
        targetOldPos = target.transform.position;
        targetPathUpdateOffset = 8;
        if (!detonationStarted) thisPursuerInstance.MoveTo(target.transform);
    }

    public void Update()
    {
        if (!graphIsReady) return;

        var targetDistance = Vector3.Distance(transform.position, target.position);

        if (targetDistance <= detonationRadius && !detonationStarted)
            StartDetonationSequence();

        if (Vector3.Distance(targetOldPos, target.position) > targetPathUpdateOffset)
        {
            targetOldPos = target.position;
            if (thisPursuerInstance.GetCurCondition() == "Movement")
                thisPursuerInstance.RefinePath(target);
            else if (!detonationStarted)
                thisPursuerInstance.MoveTo(target, true);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            if (deathPfx) deathPfx.Play();

            TrashMan.despawnAfterDelay(gameObject, 0.5f);
        }
    }

    public void StartDetonationSequence()
    {
        detonationStarted = true;

        StartCoroutine(Detonate());
    }

    private IEnumerator Detonate()
    {
        float elapsedTime = 0f;
        thisPursuerInstance.ResetCondition();

        while (elapsedTime < detonationCountdown)
        {
            blastFX.transform.localScale =
                Vector3.Lerp(blastFX.transform.localScale, Vector3.one * detonationRadius * 2f, elapsedTime / detonationCountdown);
            elapsedTime += Time.deltaTime;

            // Yield here
            yield return null;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, detonationRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(detonationForce, transform.position, detonationRadius, 3.0f, detonationForceMode);
        }

        if (deathPfx) deathPfx.Play();

        foreach (MeshRenderer mr in mineMeshRenderers)
            mr.enabled = false;

        blastFX.gameObject.SetActive(false);
        mineRb.isKinematic = true;

        yield return new WaitForSeconds(2f);

        TrashMan.despawnAfterDelay(gameObject, 0.0f);
        blastFX.gameObject.SetActive(true);
        blastFX.transform.localScale = Vector3.one * 0.01f;
        mineRb.isKinematic = false;

        foreach (MeshRenderer mr in mineMeshRenderers)
            mr.enabled = true;

    }

    public void EventTargetReached()
    {
        transform.position = RandomCirclePos();
        if (!detonationStarted) thisPursuerInstance.MoveTo(target);
    }

    Vector3 RandomCirclePos()
    {
        float alpha = Random.Range(0f, 360f);
        float radAlpha = (alpha * (float)Mathf.PI) / 180.0F;
        float x = circleRadius * (float)Mathf.Cos(radAlpha);
        float z = circleRadius * (float)Mathf.Sin(radAlpha);
        return new Vector3(x, 0, z);
    }
}