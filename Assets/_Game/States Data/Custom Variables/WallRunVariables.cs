using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class WallRunVariables
	{
        [Tooltip("To negate gravity must be equal to GravityApplier on object")]
        public float GravityNegateMultiplier = 3f;

        [Tooltip("How long the wall run applies.")]
        public float Duration = 0.75f;

        [Header("Debug")]
        public bool WallRunning = false;
    }
}