using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    SplineComputer splineComputer;
    private void Awake()
    {
        if (!splineComputer) TryGetComponent(out splineComputer);
    }
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        SplineFollower sf = other.gameObject.GetComponent<SplineFollower>();
        if (sf)
        {
            sf.spline = splineComputer;
            sf.follow = true;
            sf.Follow();
        }
    }
}
