using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Footsteps : MonoBehaviour
{
    
    void Update()
    {
        
    }

    void PlayRunEvent (string EventPath)
    {
        FMOD.Studio.EventInstance Run = FMODUnity.RuntimeManager.CreateInstance(EventPath);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(Run, transform, GetComponent<Rigidbody>());
        Run.start();
        Run.release();
    }
}
