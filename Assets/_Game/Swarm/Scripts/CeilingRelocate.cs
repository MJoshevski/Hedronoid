using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingRelocate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        FlockingAgent r = other.GetComponent<FlockingAgent>();
        if (r != null)
        {
            Vector3 rel = r.transform.position;
            rel.y = 0.0f;
            rel.x = Random.value * 10.0f;
            rel.z = Random.value * 10.0f;
            r.transform.rotation = Random.rotation;

            r.transform.position = rel;
         }
    }
}
