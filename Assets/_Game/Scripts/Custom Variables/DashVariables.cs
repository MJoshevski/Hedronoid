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
        public float DashCooldown = 3f;
        public int DashesMade = 0;
        public bool ContinuousInput = false;
        public PhysicalForceSettings PhysicalForce;
	}
}