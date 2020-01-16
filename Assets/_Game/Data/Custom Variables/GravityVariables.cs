using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class GravityVariables
	{
		[Header("Gravity Behavior")]

		[Range(1f, 4f)]
        public float GravityForceMultiplier = 1f;
		public float GravityRotationMultiplier = 5f;
	}
}