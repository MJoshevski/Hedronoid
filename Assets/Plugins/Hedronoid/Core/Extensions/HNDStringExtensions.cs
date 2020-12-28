using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NNStringExtensions
{
    /// <summary>
    /// Replaces all types on newline charaters and combinations with a given string
    /// </summary>
    public static string ReplaceNewlinesWith(this string str, string replaceWith)
    {
        if (str == null) return null;
        return str.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
    }
}
