using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.AI
{
	[CreateAssetMenu]
	public class HNDAISettings : ScriptableObject 
	{
		public const string AssetName = "HNDAISettings";

		[Header("Layers & Tags settings")]
		public LayerMask GroundLayer;

		public LayerMask WalkableLayer;

		public LayerMask PlayerLayer;
		public string PlayerTag;

		public LayerMask EnemyLayer;
		public string EnemyTag;

        [Header("Debug")]
		public bool DrawGizmos = true;
        public bool VisualizeNavigation = true;
        public bool VisualizeStates = true;
	}
}
