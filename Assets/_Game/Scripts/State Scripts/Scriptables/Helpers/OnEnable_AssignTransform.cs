using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    public class OnEnable_AssignTransform : MonoBehaviour
    {
        public TransformVariable transformVariable;

		private void OnEnable()
		{
			transformVariable.value = this.transform;
			Destroy(this);
		}

	}
}
