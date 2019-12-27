using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class WallRunVariables
	{
        [Tooltip("To negate gravity must be equal to GravityApplier on object")]
        [Range(0f, 10f)]
        public float GravityNegateMultiplier;

        [Tooltip("How long the wall run applies.")]
        public float Duration;

        [Header("Debug")]
        public bool isWallRunning; 
	}
}