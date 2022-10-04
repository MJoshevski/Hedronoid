using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanmakU.Fireables
{
    [Serializable]
    public class Ring : Fireable
    {
        public Range Count = 1;
        public Range Radius;

        public Ring(Range count, Range radius)
        {
            Count = count;
            Radius = radius;
        }

        public override void Fire(DanmakuConfig state)
        {
            float radius = Radius.GetValue();
            int count = Mathf.RoundToInt(Count.GetValue());
            var rotationYaw = state.Yaw.GetValue();
            var rotationPitch = state.Pitch.GetValue();
            var currentState = state;
            for (int i = 0; i < count; i++)
            {
                var angleYaw = rotationYaw + i * (Mathf.PI * 2 / count);
                var anglePitch = rotationPitch + i * (Mathf.PI * 2 / count);
                currentState.Position = state.Position + (radius * RotationUtiliity.ToUnitVector(angleYaw, anglePitch));
                currentState.Yaw = angleYaw;
                currentState.Pitch = anglePitch;
                Subfire(currentState);
            }
        }
    }
}