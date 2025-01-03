using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisplayScriptableObjectPropertiesAttribute))]
public class DisplayScriptableObjectPropertiesDrawer : PropertyDrawer
{
    bool _showProperty = false;
    float _drawerHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int indent = EditorGUI.indentLevel;

        _drawerHeight = 0;
        position.height = 16;

        EditorGUI.PropertyField(position, property);

        _showProperty = EditorGUI.Foldout(position, _showProperty, "");

        position.y += 20;

        position.x += 20;
        position.width -= 40;

        if (!_showProperty)
            return;

        var so = new SerializedObject(property.objectReferenceValue);
        so.Update();
        var prop = so.GetIterator();
        prop.NextVisible(true);
        int depthChilden = 0;
        bool showChilden = false;
        while (prop.NextVisible(true))
        {
            if (prop.depth == 0)
            {
                showChilden = false;
                depthChilden = 0;
            }
            if (showChilden && prop.depth > depthChilden)
            {
                continue;
            }
            position.height = 16;
            EditorGUI.indentLevel = indent + prop.depth;
            if (EditorGUI.PropertyField(position, prop))
            {
                showChilden = false;
            }
            else
            {
                showChilden = true;
                depthChilden = prop.depth;
            }
            position.y += 20;
            _drawerHeight += 20;
        }


        if (GUI.changed)
        {
            so.ApplyModifiedProperties();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = base.GetPropertyHeight(property, label);
        height += _drawerHeight;
        return height;
    }
}