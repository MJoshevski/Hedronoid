using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class NNLayerMaskExtension
{
    public static bool HasLayer(this LayerMask mask, int layerValue)
    {
        return mask == (mask | (1 << layerValue));
    }
}
