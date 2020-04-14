using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;

namespace Hedronoid
{
    public class OnEnable_AssignCamera : MonoBehaviour
    {
        public CameraVariable cameraVariable;

		private void OnEnable()
		{
            cameraVariable.value = GetComponent<Camera>();
            if(!cameraVariable.value)
                cameraVariable.value = GetComponentInChildren<Camera>();

			Destroy(this);
		}

	}
}
