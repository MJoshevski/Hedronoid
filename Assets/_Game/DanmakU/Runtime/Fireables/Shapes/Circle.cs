using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanmakU.Fireables
{
    [Serializable]
    public class Circle : Fireable
    {
        public Range Count = 1;
        public Range Radius;

        public Circle(Range count, Range radius)
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

            var origin = state.Position;
            state.Yaw = rotationYaw;
            state.Pitch = rotationPitch;
            for (int i = 0; i < count; i++)
            {
                var angleYaw = rotationYaw + i * (Mathf.PI * 2 / count);
                var anglePitch = rotationPitch + i * (Mathf.PI * 2 / count);
                state.Position = origin + (radius * RotationUtiliity.ToUnitVector(angleYaw, anglePitch));
                Subfire(state);
            }
        }
    }
}