using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPlayer : MonoBehaviour {

    public Transform trackedPlayer;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(trackedPlayer.position - transform.position, Vector3.up), 0.02f);
		
	}
}
