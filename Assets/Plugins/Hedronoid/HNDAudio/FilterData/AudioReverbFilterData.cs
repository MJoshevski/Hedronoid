using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Audio
{
    [Serializable]
    public class AudioReverbFilterData
    {


        //
        // Summary:
        //     ///
        //     Decay HF Ratio : High-frequency to low-frequency decay time ratio. Ranges from
        //     0.1 to 2.0. Default is 0.5.
        //     ///
        public float decayHFRatio;
        //
        // Summary:
        //     ///
        //     Reverberation decay time at low-frequencies in seconds. Ranges from 0.1 to 20.0.
        //     Default is 1.0.
        //     ///
        public float decayTime;
        //
        // Summary:
        //     ///
        //     Reverberation density (modal density) in percent. Ranges from 0.0 to 100.0. Default
        //     is 100.0.
        //     ///
        public float density;
        //
        // Summary:
        //     ///
        //     Reverberation diffusion (echo density) in percent. Ranges from 0.0 to 100.0.
        //     Default is 100.0.
        //     ///
        public float diffusion;
        //
        // Summary:
        //     ///
        //     Mix level of dry signal in output in mB. Ranges from -10000.0 to 0.0. Default
        //     is 0.
        //     ///
        public float dryLevel;
        //
        // Summary:
        //     ///
        //     Reference high frequency in Hz. Ranges from 20.0 to 20000.0. Default is 5000.0.
        //     ///
        public float hfReference;
        //
        // Summary:
        //     ///
        //     Reference low-frequency in Hz. Ranges from 20.0 to 1000.0. Default is 250.0.
        //     ///
        public float lfReference;
        //
        // Summary:
        //     ///
        //     Late reverberation level relative to room effect in mB. Ranges from -10000.0
        //     to 2000.0. Default is 0.0.
        //     ///
        public float reflectionsDelay;
        //
        // Summary:
        //     ///
        //     Early reflections level relative to room effect in mB. Ranges from -10000.0 to
        //     1000.0. Default is -10000.0.
        //     ///
        public float reflectionsLevel;
        //
        // Summary:
        //     ///
        //     Late reverberation delay time relative to first reflection in seconds. Ranges
        //     from 0.0 to 0.1. Default is 0.04.
        //     ///
        public float reverbDelay;
        //
        // Summary:
        //     ///
        //     Late reverberation level relative to room effect in mB. Ranges from -10000.0
        //     to 2000.0. Default is 0.0.
        //     ///
        public float reverbLevel;
        //
        // Summary:
        //     ///
        //     Set/Get reverb preset properties.
        //     ///
        public AudioReverbPreset reverbPreset;
        //
        // Summary:
        //     ///
        //     Room effect level at low frequencies in mB. Ranges from -10000.0 to 0.0. Default
        //     is 0.0.
        //     ///
        public float room;
        //
        // Summary:
        //     ///
        //     Room effect high-frequency level re. low frequency level in mB. Ranges from -10000.0
        //     to 0.0. Default is 0.0.
        //     ///
        public float roomHF;
        //
        // Summary:
        //     ///
        //     Room effect low-frequency level in mB. Ranges from -10000.0 to 0.0. Default is
        //     0.0.
        //     ///
        public float roomLF;


        public void SetToFilter(AudioReverbFilter filter)
        {
            filter.decayHFRatio = decayHFRatio;
            filter.decayTime = decayTime;
            filter.density = density;
            filter.diffusion = diffusion;
            filter.dryLevel = dryLevel;
            filter.hfReference = hfReference;
            filter.lfReference = lfReference;
            filter.reflectionsDelay = reflectionsDelay;
            filter.reflectionsLevel = reflectionsLevel;
            filter.reverbDelay = reverbDelay;
            filter.reverbLevel = reverbLevel;
            filter.reverbPreset = reverbPreset;
            filter.room = room;
            filter.roomHF = roomHF;
            filter.roomLF = roomLF;
        }
    }
}
