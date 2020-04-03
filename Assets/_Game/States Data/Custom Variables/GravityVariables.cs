using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class GravityVariables
	{
		[Header("Gravity Behavior")]
        public float GravityForceMultiplier = 1f;
		public float GravityRotationMultiplier = 5f;

        [Header("Gravity Switch")]
        [Tooltip("How much time (seconds) should we wait before we can switch again?")]
        public float GravitySwitchCooldown = 1.5f;

        [Tooltip("If checked, gravity switching will be handled by key presses. If uncheked, switching will be triggered on wall collision.")]
        public bool ChangeOnKeyPress = true;
    }
}