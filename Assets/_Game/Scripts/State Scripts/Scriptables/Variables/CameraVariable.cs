using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid
{
    [CreateAssetMenu(menuName = "Variables/Camera")]
    public class CameraVariable : ScriptableObject
    {
        public Camera value;

        public void Set(Camera v)
        {
            value = v;
        }

        public void Set(CameraVariable v)
        {
            value = v.value;
        }

        public void Clear()
        {
            value = null;
        }
    }
}
