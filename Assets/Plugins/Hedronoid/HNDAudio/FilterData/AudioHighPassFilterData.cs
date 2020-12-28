using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Audio
{
    [Serializable]
    public class AudioHighPassFilterData
    {

        //
        // Summary:
        //     ///
        //     Highpass cutoff frequency in hz. 10.0 to 22000.0. Default = 5000.0.
        //     ///
        public float cutoffFrequency;
        //
        // Summary:
        //     ///
        //     Determines how much the filter's self-resonance isdampened.
        //     ///
        public float highpassResonanceQ;

        public void SetToFilter(AudioHighPassFilter filter)
        {
            filter.cutoffFrequency = cutoffFrequency;
            filter.highpassResonanceQ = highpassResonanceQ;
        }
    }
}
