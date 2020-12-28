using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EAudioFilter
{
    None = 0,
    LowPass = 1 << 0,
    HighPass = 1 << 1,
    Reverb = 1 << 2,
    Echo = 1 << 3,
    Distortion = 1 << 4,
    Chorus = 1 << 5,
    FadeIn = 1 << 6,
    FadeOut = 1 << 7,
}
