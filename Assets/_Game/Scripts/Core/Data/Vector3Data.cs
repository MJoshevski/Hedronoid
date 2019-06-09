using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Vector3")]
public class Vector3Data : ScriptableObject
{
    public Vector3 Value;

    [UnityEngine.ContextMenu("Copy")]
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = Value.ToString();
    }

    [UnityEngine.ContextMenu("Paste")]
    public void PasteFromClipboard()
    {
        Value = GetVector3(GUIUtility.systemCopyBuffer);
    }

    public Quaternion AsRotation()
    {
        return Quaternion.Euler(Value.x, Value.y, Value.z);
    }

    Vector3 GetVector3(string text)
    {
        string[] temp = text.Substring(1, text.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        Vector3 value = new Vector3(x, y, z);
        return value;
    }
}
