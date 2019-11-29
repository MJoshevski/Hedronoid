using System;
using UnityEngine;

namespace HedronoidSP
{
	[System.Serializable]
	public class MovementVariables
	{
		public float Horizontal;
		public float Vertical;
		public float MoveAmount;
        public float TurnSpeedMultiplier = 2f;
        public float MoveSpeedMultiplier = 1f;
        public float MoveVeloctiyChangeRate = 1f;
        public Vector3 MoveDirection;
	}
}