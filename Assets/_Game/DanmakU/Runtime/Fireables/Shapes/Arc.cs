using System;
using System.Collections.Generic;
using UnityEngine;

namespace DanmakU.Fireables
{
    [Serializable]
    public class Arc : Fireable
    {
        public Range Coloumns = 1;
        public Range Rows = 1;
        [Radians] public Range ArcLength;
        [Radians] public Range ArcHeight;
        public Range Radius;

        public Arc(Range coloumns, Range rows, Range arcLength, Range arcHeight, Range radius)
        {
            Coloumns = coloumns;
            Rows = rows;
            ArcLength = arcLength;
            ArcHeight = arcHeight;
            Radius = radius;
        }

        public override void Fire(DanmakuConfig state)
        {
            float radius = Radius.GetValue();
            int coloumnCount = Mathf.RoundToInt(Coloumns.GetValue());
            int rowCount = Mathf.RoundToInt(Rows.GetValue());
            if (coloumnCount == 0) return;
            if (coloumnCount == 1)
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

            for (int j = 0; j < rowCount; j++)
            {
                var anglePitch = startHeight + j * (arcHeight / (coloumnCount - 1));

                for (int i = 0; i < coloumnCount; i++)
                {
                    var angleYaw = startLength + i * (arcLength / (coloumnCount - 1));
                    var currentState = state;
                    currentState.Position = state.Position + (radius * RotationUtiliity.ToUnitVector(angleYaw, anglePitch));
                    currentState.Yaw = angleYaw;
                    currentState.Pitch = anglePitch;
                    Subfire(currentState);
                }
            }
        }
    }
}