using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Hedronoid.TriggerSystem
{
    [CustomPropertyDrawer(typeof(HNDAction))]
    public class HNDActionDrawer : PropertyDrawer
    {

        private const int _buttonWidth = 20;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {

            EditorGUI.ObjectField(new Rect(pos.x, pos.y, pos.width - _buttonWidth * 2, pos.height), prop, label);

            // Copy
            if (GUI.Button(new Rect(pos.x + pos.width - _buttonWidth * 2, pos.y, _buttonWidth, pos.height), ""))
            {
                HNDActionSelectorWindow.Init(prop);
            }
            GUI.Label(
                new Rect(pos.x + pos.width - _buttonWidth * 2, pos.y, _buttonWidth, pos.height),
                "..."
                );

        } // OnGUI()

    } // class CurvePropertyDrawer

    public class HNDActionSelectorWindow : EditorWindow
    {

        public static SerializedProperty m_prop;
        private bool[] detailsFoldout;

        public static void Init(SerializedProperty prop)
        {
            // Get existing open window or if none, make a new one:
            HNDActionSelectorWindow window = (HNDActionSelectorWindow)EditorWindow.GetWindow(typeof(HNDActionSelectorWindow));
            window.titleContent = new GUIContent("Select action");
            window.autoRepaintOnSceneChange = true;

            m_prop = prop;
        }

        void OnSelectionChange()
        {
            Close();
        }

        void OnGUI()
        {
            //EditorGUI.ObjectField (new Rect (0f, 0f, 100f, 20f), m_action, new GUIContent ("Action"));
            m_prop.objectReferenceValue = EditorGUILayout.ObjectField("Action", m_prop.objectReferenceValue, typeof(HNDAction), true);

            if (m_prop.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No object selected. Drag the object you want to select an action on to the field above.", MessageType.None);
                return;
            }

            EditorGUILayout.LabelField("Showing all actions available on '" + m_prop.objectReferenceValue.name + "'.");
            EditorGUILayout.LabelField("Object highlighted in green is the currently selected action.");

            HNDAction currentAction = m_prop.objectReferenceValue as HNDAction;

            HNDAction[] allActions = currentAction.cachedGameObject.GetComponents<HNDAction>();
            if (detailsFoldout == null || detailsFoldout.Length != allActions.Length)
            {
                detailsFoldout = new bool[allActions.Length];
            }

            for (int i = 0; i < allActions.Length; i++)
            {
                if (currentAction == allActions[i])
                    GUI.color = Color.green;
                EditorGUILayout.BeginHorizontal();
                string name = allActions[i].GetType().ToString();
                int lastDotIndex = name.LastIndexOf(".");
                name = name.Substring(lastDotIndex + 1);
                detailsFoldout[i] = EditorGUILayout.Foldout(detailsFoldout[i], name);
                if (GUILayout.Button("Select"))
                    m_prop.objectReferenceValue = allActions[i];
                EditorGUILayout.EndHorizontal();

                if (detailsFoldout[i])
                {
                    GUI.enabled = false;
                    EditorGUI.indentLevel++;
                    FieldInfo[] fis = allActions[i].GetType().GetFields();
                    SerializedObject so = new SerializedObject(allActions[i]);
                    for (int j = 0; j < fis.Length; j++)
                    {
                        //Debug.Log("Property #" + j + " = " + pis[j].Name);
                        SerializedProperty sp = so.FindProperty(fis[j].Name);
                        if (sp != null)
                        {
                            EditorGUILayout.PropertyField(sp);
                        }
                    }
                    EditorGUI.indentLevel--;
                    GUI.enabled = true;
                }

                GUI.color = Color.white;
            }

            m_prop.serializedObject.ApplyModifiedProperties();
        }
    }
}