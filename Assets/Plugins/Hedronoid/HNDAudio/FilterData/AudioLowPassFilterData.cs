using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Hedronoid.Audio
{
    [Serializable]
    public class AudioLowPassFilterData
    {


        //
        // Summary:
        //     ///
        //     Returns or sets the current custom frequency cutoff curve.
        //     ///
        public AnimationCurve customCutoffCurve;
        //
        // Summary:
        //     ///
        //     Lowpass cutoff frequency in hz. 10.0 to 22000.0. Default = 5000.0.
        //     ///
        public float cutoffFrequency;
        //
        // Summary:
        //     ///
        //     Determines how much the filter's self-resonance is dampened.
        //     ///
        public float lowpassResonanceQ;

        public void SetToFilter(AudioLowPassFilter filter)
        {
            filter.cutoffFrequency = cutoffFrequency;
            filter.lowpassResonanceQ = lowpassResonanceQ;
            filter.customCutoffCurve = customCutoffCurve;
        }
    }
}