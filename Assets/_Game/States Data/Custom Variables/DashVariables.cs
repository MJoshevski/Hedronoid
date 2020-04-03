using System;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class DashVariables
	{
        public bool DashMade = false;
        public int MaxDashes = 1;
        public int DashesMade = 0;
        public bool ContinuousInput = false;
        public PhysicalForceSettings PhysicalForce;
	}
}