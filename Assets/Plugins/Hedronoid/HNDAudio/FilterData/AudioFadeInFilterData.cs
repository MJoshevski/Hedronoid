﻿
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Audio
{
    [Serializable]
    public class AudioFadeInFilterData
    {
        public AnimationCurve Fade;

        [Serializable]
        public enum EFadeType
        {
            VOLUME = 0,
            PITCH = 1,

        }

        public EFadeType Type;
    }
}

