﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music_Player : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string EventPath;
    FMOD.Studio.EventInstance MusicInst;
    // Start is called before the first frame update
    void Start()
    {
        MusicInst = FMODUnity.RuntimeManager.CreateInstance(EventPath);
        MusicInst.start();
        MusicInst.release();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        MusicInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

    }

}