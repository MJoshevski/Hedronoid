using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanmakU.Fireables
{
    [Serializable]
    public class Arc : Fireable
    {
        public Range Count = 1;
        [Radians] public Range ArcLength;
        [Radians] public Range ArcHeight;
        public Range Radius;

        public Arc(Range count, Range arcLength, Range arcHeight, Range radius)
        {
            Count = count;
            ArcLength = arcLength;
            ArcHeight = arcHeight;
            Radius = radius;
        }

        public override void Fire(DanmakuConfig state)
        {
            float radius = Radius.GetValue();
            int count = Mathf.RoundToInt(Count.GetValue());
            if (count == 0) return;
            if (count == 1)
            {
                Subfire(state);
                return;
            }
            float arcLength = ArcLength.GetValue();
            float arcHeight = ArcHeight.GetValue();
            var rotationYaw = state.Yaw.GetValue();
            var rotationPitch = state.Pitch.GetValue();
            var startLength = rotationYaw - arcLength / 2;
            var startHeight = rotationPitch - arcHeight / 2;

            for (int i = 0; i < count; i++)
            {
                var angleYaw = startLength + i * (arcLength / (count - 1));
                var anglePitch = startHeight + i * (arcHeight / (count - 1));
                var currentState = state;
                currentState.Position = state.Position + (radius * RotationUtiliity.ToUnitVector(angleYaw + anglePitch));
                currentState.Yaw = angleYaw;
                currentState.Pitch = anglePitch;
                Subfire(currentState);
            }
        }
    }
}