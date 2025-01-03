﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Audio
{
    //Unscaled wait-for-seconds coroutine
    public class WaitForPausedAudioRealtime : CustomYieldInstruction
    {
        private float m_EndTime;
		
        public WaitForPausedAudioRealtime(float duration)
        {
            m_EndTime = GlobalAudioManager.Instance.PausableAudioTime + duration;
        }

        public override bool keepWaiting
        {
            get { return GlobalAudioManager.Instance.PausableAudioTime < m_EndTime; }
        }
    }
}