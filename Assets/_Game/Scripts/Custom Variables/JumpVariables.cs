using System;
using UnityEngine;
using Animancer;

namespace Hedronoid
{
	[System.Serializable]
	public class JumpVariables
	{
		public bool JumpMade = false;
		public int MaxJumps = 2;
		public int JumpsMade = 0;
        public PhysicalForceSettings firstJumpForceSettings;
        public PhysicalForceSettings secondJumpForceSettings;


	}
}