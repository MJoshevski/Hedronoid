using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerUtility : MonoBehaviour
{
    public List<GameObject> onTriggerEnter;
    public List<GameObject> onTriggerStay;
    public List<GameObject> onTriggerExit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        foreach (GameObject go in onTriggerEnter)
        {
            SplineFollower sf = go.GetComponent<SplineFollower>();
            if (!sf) sf = go.GetComponentInChildren<SplineFollower>();
            if (sf) sf.follow = !sf.follow;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

    }
}
