using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hedronoid 
{
#if UNITY_EDITOR
    public static class EditorExtension
    {
        public static int DrawBitMaskField(Rect aPosition, int aMask, System.Type aType, GUIContent aLabel)
        {
            var itemNames = System.Enum.GetNames(aType);
            var itemValues = System.Enum.GetValues(aType) as int[];

            int val = aMask;
            int maskVal = 0;
            for (int i = 0; i < itemValues.Length; i++)
            {
                if (itemValues[i] != 0)
                {
                    if ((val & itemValues[i]) == itemValues[i])
                        maskVal |= 1 << i;
                }
                else if (val == 0)
                    maskVal |= 1 << i;
            }
            int newMaskVal = EditorGUI.MaskField(aPosition, aLabel, maskVal, itemNames);
            int changes = maskVal ^ newMaskVal;

            for (int i = 0; i < itemValues.Length; i++)
            {
                if ((changes & (1 << i)) != 0)            // has this list item changed?
                {
                    if ((newMaskVal & (1 << i)) != 0)     // has it been set?
                    {
                        if (itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
                        {
                            val = 0;
                            break;
                        }
                        else
                            val |= itemValues[i];
                    }
                    else                                  // it has been reset
                    {
                        val &= ~itemValues[i];
                    }
                }
            }
            return val;
        }
    }

    [CustomPropertyDrawer(typeof(HNDEnumBitMaskAttribute))]
    public class EnumBitMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            Enum targetEnum = GetBaseProperty<Enum>(prop);

            // Add the actual int value behind the field name
            label.text = label.text + "(" + prop.intValue + ")";
            prop.intValue = EditorExtension.DrawBitMaskField(position, prop.intValue, targetEnum.GetType(), label);
        }

        static T GetBaseProperty<T>(SerializedProperty prop)
        {
            // Separate the steps it takes to get to this property
            string[] separatedPaths = prop.propertyPath.Split('.');

            // Go down to the root of this serialized property
            System.Object reflectionTarget = prop.serializedObject.targetObject as object;
            // Walk down the path to get the target object
            foreach (var path in separatedPaths)
            {
                FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField);
                reflectionTarget = fieldInfo.GetValue(reflectionTarget);
            }
            return (T)reflectionTarget;
        }
    }
#endif
}