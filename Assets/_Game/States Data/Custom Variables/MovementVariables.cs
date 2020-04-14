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
	}
}