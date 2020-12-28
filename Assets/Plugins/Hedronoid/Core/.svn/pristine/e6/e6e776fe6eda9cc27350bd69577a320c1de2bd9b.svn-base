using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NNParticleSystemExtensions 
{

    #region Main Module

    public static void SetEmissionEnabled(this ParticleSystem particles, bool enable)
    {
        var emission = particles.emission;
        emission.enabled = enable;
    }


    public static void SetDuration(this ParticleSystem particles, float duration)
    {
        var main = particles.main;
        main.duration = duration;
    }

    public static float GetDuration(this ParticleSystem particles)
    {
        var main = particles.main;
        return main.duration;
    }

    public static bool IsLooping(this ParticleSystem particles)
    {
        var main = particles.main;
        return main.loop;
    }

    public static void SetStartLifetime(this ParticleSystem particles, float startLifetime)
    {
        var main = particles.main;
        main.startLifetime = startLifetime;
    }

    public static float GetStartLifetime(this ParticleSystem particles)
    {
        var main = particles.main;
        return main.startLifetime.constant;
    }

    public static void SetStartSize(this ParticleSystem particles, float startSize)
    {
        var main = particles.main;
        main.startSize = startSize;
    }

    public static float GetStartSize(this ParticleSystem particles)
    {
        var main = particles.main;
        return main.startSize.constant;
    }
    #endregion Main Module


    #region Emission Module
    public static void SetRateOverTime(this ParticleSystem particles, float rate)
    {
        var emission = particles.emission;
        emission.rateOverTime = rate;
    }
    #endregion Emission Module
}