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
            var rotation = state.Rotation;
            //var start = rotation - arcLength / 2;

            for (int i = 0; i < coloumnCount; i++)
            {
                //var angle = start + i * (arcLength / (coloumnCount - 1));
                var currentState = state;
                //currentState.Position = state.Position + (radius * RotationUtiliity.ToUnitVector(angle));
                //currentState.Rotation = angle;
                Subfire(currentState);
            }
        }
    }
}