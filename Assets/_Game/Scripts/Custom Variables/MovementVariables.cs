using Animancer;
using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class MovementVariables
	{
        [Header("Axes")]
		public float Horizontal;
		public float Vertical;

        [Header("Movement")]
        public float MovementSpeed = 2f;
        [Range(0f, 100f)]
        public float MaxAcceleration = 10f;
        [HideInInspector]
        public Vector3 desiredVelocity;

        [Header("Rotation")]
        public float TurnSpeedMultiplier = 2f;

        [Header("Monitoring")]
        public float MoveAmount;
        public Vector3 MoveDirection;

        [Header("Animation")]
        public LinearMixerTransition MovementMixer;
        public MixerTransition2D DirectionalMovementMixer;
        public AnimationClip FallAnimation;
        public AnimationClip LandAnimation;
        public AnimationClip LandRollAnimation;
        public AnimationClip HangAnimation;
        public AnimationClip FlyingAnimation;
        public AnimationClip JumpAnimation;
        public AnimationClip DoubleJumpAnimation;
        public AnimationClip DashAnimation;
        public AnimationClip DashEndAnimation;


        public int DefaultLayer = 0;
        public int LocomotionLayer = 1;
        public int IKLayer = 2;
        public int ImportantLayer = 3;
    }
}