using Hedronoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODAudioManager : HNDGameObject
{
    [FMODUnity.EventRef]
    public string MusicEventPath;
    FMOD.Studio.EventInstance EventInst;

    // Start is called before the first frame update
    protected override void Start()
    {
        EventInst = FMODUnity.RuntimeManager.CreateInstance(MusicEventPath);

        // Start the music
        EventInst.start();
        EventInst.release();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
