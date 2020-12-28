using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hedronoid.Spawners
{
    /// <summary>
    /// Custom editor for the ObjectSpawnerController, that prints what the current configuration will spawn
    /// </summary>
    [CustomEditor(typeof(ObjectSpawnerController))]
    public class ObjectSpawnerControllerEditor : Editor
    {
        ObjectSpawnerController thisOSC;

        void OnEnable()
        {
            thisOSC = target as ObjectSpawnerController;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawSpawnInfo(thisOSC.GetSpawnInfo());
        }

        void DrawSpawnInfo(SpawnInfo spawnInfo)
        {
            string startSpawnString = "";
            if (spawnInfo.SpawnAtRoundStart)
            {
                startSpawnString = "spawn " + spawnInfo.SpawnAtRoundStartCount + " objects on spawn manager(s) at start of round (" + (spawnInfo.SpawnAtRoundStartCount).ToString() + " in total)";
            }
            else
            {
                startSpawnString = "not spawn any objects at start of round";
            }

            string intervalSpawnString = "";
            if (spawnInfo.SpawnAtInterval)
            {
                intervalSpawnString = "spawn " + spawnInfo.SpawnAtIntervalCount + " objects on spawn manager(s) at intervals between " + spawnInfo.MinInterval + " and " + spawnInfo.MaxInterval + " seconds with an initial delay of " + spawnInfo.InitialInterval + " seconds";
            }
            else
            {
                intervalSpawnString = "not spawn any objects";
            }

            EditorGUILayout.HelpBox("'" + spawnInfo.name + "' will " + startSpawnString + ".\n - During game, it will " + intervalSpawnString + ".", MessageType.Info);
        }
    }
}