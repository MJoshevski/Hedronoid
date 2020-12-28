using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hedronoid.Audio
{
    [SerializeField]
    public class AudioEchoFilterData
    {

        //
        // Summary:
        //     ///
        //     Echo decay per delay. 0 to 1. 1.0 = No decay, 0.0 = total decay (i.e. simple
        //     1 line delay). Default = 0.5.
        //     ///
        public float decayRatio;
        //
        // Summary:
        //     ///
        //     Echo delay in ms. 10 to 5000. Default = 500.
        //     ///
        public float delay;
        //
        // Summary:
        //     ///
        //     Volume of original signal to pass to output. 0.0 to 1.0. Default = 1.0.
        //     ///
        public float dryMix;
        //
        // Summary:
        //     ///
        //     Volume of echo signal to pass to output. 0.0 to 1.0. Default = 1.0.
        //     ///
        public float wetMix;

        public void SetToFilter(AudioEchoFilter filter)
        {
            filter.delay = delay;
            filter.decayRatio = decayRatio;
            filter.dryMix = dryMix;
            filter.wetMix = wetMix;
        }
    }
}