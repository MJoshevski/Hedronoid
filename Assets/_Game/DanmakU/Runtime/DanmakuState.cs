using System;
using UnityEngine;

namespace DanmakU
{
    /// <summary>
    /// A config for creating <see cref="DanmakU.DanmakuState"/>.
    /// </summary>
    [Serializable]
    public struct DanmakuConfig
    {
        public Vector3 Position;
        [Radians] public Range Yaw;
        [Radians] public Range Pitch;
        public Range Speed;
        [Radians] public Range AngularSpeed;
        public Color Color;

        /// <summary>
        /// Creates an state based on the config.
        /// </summary>
        /// <returns>a sampled state from the config's state space.</returns>
        public DanmakuState CreateState()
        {
            return new DanmakuState
            {
                Position = Position,
                Yaw = Yaw.GetValue(),
                Pitch = Pitch.GetValue(),
                Speed = Speed.GetValue(),
                AngularSpeed = AngularSpeed.GetValue(),
                Color = Color
            };
        }

    }

    /// <summary>
    /// A snapshot of a <see cref="DanmakU.Danmaku"/>'s state.
    /// </summary>
    [Serializable]
    public struct DanmakuState
    {
        public Vector3 Position;
        [Radians] public float Yaw;
        [Radians] public float Pitch;
        public float Speed;
        [Radians] public float AngularSpeed;
        public Color Color;
    }

}