using System.Collections;
using System.Collections.Generic;
using Hedronoid.Audio;
using UnityEngine;

namespace Hedronoid.Audio
{
    public class WaitUntilAudioClipFinished : CustomYieldInstruction
    {
        private bool m_Wait = true;

        public void StopWaiting()
        {
            m_Wait = false;
        }

        public WaitUntilAudioClipFinished(AudioClipPlayer player)
        {
            m_Wait = player != null;
        }

        public override bool keepWaiting
        {
            get { return m_Wait; }
        }
    }
}