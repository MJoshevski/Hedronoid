using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class DashVariables
	{
        public int MaxDashes = 1;
        public float DashDistance = 30f;
        public float DashDuration = 0.25f;
        public int DashesMade = 0;
        public bool ContinuousInput = false;
        public PhysicalForceSettings PhysicalForce;
	}
}