using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class DashVariables
	{
        public int ExecutionsBeforeReset = 0;
        public bool ContinuousInput = false;
        public PhysicalForceSettings PhysicalForce;
	}
}