using UnityEngine;
using System.Collections.Generic;
using System;

public static class NNEnumExtensions
{
    public static string ToNumberString(this Enum enVal)
    {
        return enVal.ToString("D");
    }

    public static int ToInt(this Enum enVal)
    {
        return Convert.ToInt32(enVal);
    }

    public static string ValueName(this Enum enVal)
    {
        return Enum.GetName(enVal.GetType(), enVal);
    }
}