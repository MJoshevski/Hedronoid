using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HedronoidSP
{
    public static class Layers
    {
        public static LayerMask _ignoreLayersController;
        static Layers()
        {
            _ignoreLayersController = ~(1 << 3 | 1 << 8);
        }
    }
}
