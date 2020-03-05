using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
	[System.Serializable]
	public class CollisionVariables
	{
        public float RaySize = 2f;

        [HideInInspector]
        public Dictionary<string, RaycastHit> RaycastHitDictionary;
        [HideInInspector]
        public Dictionary<string, bool> RaycastFlagDictionary;
        [HideInInspector]
        public RaycastHit[] ArrayHits;
        [HideInInspector]
        public bool[] ArrayFlags;
    }
}