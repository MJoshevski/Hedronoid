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
        public float MoveSpeedMultiplier = 1f; 
        public float MoveVeloctiyChangeRate = 1f;

        [Header("Rotation")]
        public float TurnSpeedMultiplier = 2f;

        [Header("Gravity Behavior")]
        public float GravityRotationMultiplier = 5f;

        [Header("Monitoring")]
        public float MoveAmount;
        public Vector3 MoveDirection;        
	}
}