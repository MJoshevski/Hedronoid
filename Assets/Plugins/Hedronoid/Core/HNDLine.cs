using UnityEngine;

namespace Hedronoid
{
    public struct NNLine
    {
        public Vector3 Start { get; private set; }
        public Vector3 Direction { get; private set; }
        
        public NNLine(Vector3 start, Vector3 direction) : this()
        {
            Start = start;
            Direction = direction;
        }

        /// <summary>
        /// Returns closest point on line to passed point
        /// </summary>
        /// <param name="???"></param>
        /// <returns></returns>
        public Vector3 ClosestPoint(Vector3 point)
        {
            var startToPoint = point - Start;

            var normalizedDot = Mathf.Clamp01(Vector3.Dot(startToPoint, Direction) / Vector3.Dot(Direction, Direction));            
            
            return Start + (normalizedDot * Direction);
        }

        public void ClosestPoints(NNLine otherLine, out Vector3 pointOnThisLine, out Vector3 pointOnOtherLine)
        {
            var w0 = this.Start - otherLine.Start;
            float a = Vector3.Dot(this.Direction, this.Direction);
            float b = Vector3.Dot(this.Direction, otherLine.Direction);
            float c = Vector3.Dot(otherLine.Direction, otherLine.Direction);
            float d = Vector3.Dot(this.Direction, w0);
            float e = Vector3.Dot(otherLine.Direction, w0);

            float denom = a * c - b * b;
            if (denom == 0)
            {
                pointOnThisLine = this.Start;
                pointOnOtherLine = otherLine.Start + (e / c) * otherLine.Direction;
            }
            else
            {
                pointOnThisLine = this.Start + ((b * e - c * d) / denom) * this.Direction;
                pointOnOtherLine = otherLine.Start + ((a * e - b * d) / denom) * otherLine.Direction;
            }
        }
    }
}