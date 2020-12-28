using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Audio
{
    /// <summary>
    /// 3D data per sound, not finished (only used for a default value now)
    /// </summary>
    [Serializable]
    public class AudioSource3DData
    {

        // Summary:
        //     ///
        //     Sets the Doppler scale for this AudioSource.
        //     ///
        public float dopplerLevel;
        //
        // Summary:
        //     ///
        //     Within the Min distance the AudioSource will cease to grow louder in volume.
        //     ///
        public float minDistance;
        //
        // Summary:
        //     ///
        //     (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
        //     ///
        public float maxDistance;
        //
        // Summary:
        //     ///
        //     Pans a playing sound in a stereo way (left or right). This only applies to sounds
        //     that are Mono or Stereo.
        //     ///
        public float panStereo;
        //
        // Summary:
        //     ///
        //     Sets/Gets how the AudioSource attenuates over distance.
        //     ///
        public AudioRolloffMode rolloffMode;
        //
        // Summary:
        //     ///
        //     Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker
        //     space.
        //     ///
        public float spread;
        //
        // Summary:
        //     ///
        //     Sets how much this AudioSource is affected by 3D spatialisation calculations
        //     (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
        //     ///
        public float spatialBlend;
        //
        // Summary:
        //     ///
        //     Enables or disables spatialization.
        //     ///
        public bool spatialize;
        //
        // Summary:
        //     ///
        //     Determines if the spatializer effect is inserted before or after the effect filters.
        //     ///
        public bool spatializePostEffects;

        public AudioSource3DData() { }

        public AudioSource3DData(float dopplerLevel, float minDistance, float maxDistance, float panStereo, AudioRolloffMode rolloffMode, float spread, float spatialBlend, bool spatialize, bool spatializePostEffects)
        {
            this.dopplerLevel = dopplerLevel;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
            this.panStereo = panStereo;
            this.rolloffMode = rolloffMode;
            this.spread = spread;
            this.spatialize = spatialize;
            this.spatialBlend = spatialBlend;
            this.spatializePostEffects = spatializePostEffects;
        }
    }
}