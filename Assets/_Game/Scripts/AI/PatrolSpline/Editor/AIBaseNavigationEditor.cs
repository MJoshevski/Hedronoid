using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Hedronoid.AI;

namespace Hedronoid.AI
{
    [CustomEditor(typeof(AIBaseNavigation))]
    public class AIBaseNavigationEditor : Editor
    {
        protected void OnSceneGUI()
        {
            var navigation = (AIBaseNavigation)target;
            if (navigation && navigation.DefaultTarget)
            {
                for (int i = 0; i < navigation.DefaultTarget.childCount; i++)
                {
                    var waypoint = navigation.DefaultTarget.GetChild(i);
                    EditorGUI.BeginChangeCheck();
                    var newWaypointPos = Handles.PositionHandle(waypoint.position, waypoint.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(waypoint, "Moving waypoint");
                        waypoint.position = newWaypointPos;
                    }
                }
            }
        }
    }
}
