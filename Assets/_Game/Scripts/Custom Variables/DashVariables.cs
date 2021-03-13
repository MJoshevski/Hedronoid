using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class DashVariables
	{
        public int MaxDashes = 1;
        public float MaxDistance = 30f;
        public float MaxTime = 2f;
        public float DashCooldown = 1f;
        public int DashesMade = 0;
        public PhysicalForceSettings PhysicalForce;
	}
}