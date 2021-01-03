using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAnimator : MonoBehaviour
{
    Vector3 basePos = Vector3.zero;
    public float radius = 15.0f;
    Transform cachedTransform;
    Vector3 targetPos = Vector3.zero;
    public float changeRate = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        basePos = transform.parent.position + Vector3.up * 9.0f;
        cachedTransform = transform;
        targetPos = basePos;
        InvokeRepeating("BlinkTarget", 0.0f, changeRate);
    }

    // Update is called once per frame
    void BlinkTarget ()
    {
        targetPos = basePos + Random.onUnitSphere * radius;
    }

    private void Update()
    {
        cachedTransform.position = Vector3.MoveTowards(cachedTransform.position, targetPos, 0.15f * Time.deltaTime/0.011f);
    }

}
