using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid 
{
    public class HNDEnumBitMaskAttribute : PropertyAttribute
    {
        public System.Type propType;

        public HNDEnumBitMaskAttribute() { }

        public HNDEnumBitMaskAttribute(System.Type aType)
        {
            propType = aType;
        }

    }
}
