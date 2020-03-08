using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Footsteps : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayRunEvent ()
    {
        FMOD.Studio.EventInstance Run = FMODUnity.RuntimeManager.CreateInstance("event:/Foley/Footsteps");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Run, transform, GetComponent<Rigidbody>());
        Run.start();
        Run.release();
    }
}
