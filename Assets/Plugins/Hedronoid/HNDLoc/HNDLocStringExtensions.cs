using System.Collections;
using System.Collections.Generic;
using Hedronoid.HNDLoc;
using UnityEngine;

public static class HNDLocStringExtensions
{
    public static string HNDLocalized(this string source)
    {
        return HNDLoc.Instance.GetLocalizedString(source);
    }

    public static string HNDLocalized(this string source, params object[] list)
    {
        return HNDLoc.Instance.GetLocalizedStringFormat(source, list);
    }

    public static string HNDLocalizedUpper(this string source)
    {
        return HNDLoc.Instance.GetLocalizedUpperString(source);
    }   
}
