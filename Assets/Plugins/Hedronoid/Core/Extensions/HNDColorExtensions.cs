using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NNColorExtensions
{
    public static Color FromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.cyan;
    }
}