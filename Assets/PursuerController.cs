using UnityEngine;
using UnityEngine.UI;

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
    private void Start()
    {
        thisPursuerInstance = gameObject.GetComponent<Pursuer>();
        graphIsReady = false;
    }

    public void TheGraphIsReady()
    {
        graphIsReady = true;
        targetOldPos = target.transform.position;
        targetPathUpdateOffset = 8;
        thisPursuerInstance.MoveTo(target.transform);
    }

    public void Update()
    {
        if (!graphIsReady) return;

        if (Vector3.Distance(targetOldPos, target.position) > targetPathUpdateOffset)
        {
            targetOldPos = target.position;
            if (thisPursuerInstance.GetCurCondition() == "Movement")
                thisPursuerInstance.RefinePath(target);
            else
                thisPursuerInstance.MoveTo(target, true);
        }
    }

    public void EventTargetReached()
    {
        transform.position = RandomCirclePos();
        thisPursuerInstance.MoveTo(target);
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