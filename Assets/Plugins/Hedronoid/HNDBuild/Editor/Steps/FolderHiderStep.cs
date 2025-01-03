﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    [CreateAssetMenu(fileName = "FolderHiderStep", menuName = "Build/Steps/Folder hider")]
    public class FolderHiderStep : HNDBuildStep
    {

        [Serializable]
        public class PlatformFolderSpec
        {
            public string FolderPath;
            public UnityEditor.BuildTargetGroup TargetGroup;
        }

        [SerializeField]
        List<PlatformFolderSpec> m_ExludedFolders;

        [SerializeField]
        string m_HidePath = ".Hidden";

        public override List<string> GetRequiredEnvValues()
        {
            return new List<string>
            {
                HNDBuildConstants.BUILD_TARGET_GROUP,
            };
        }

        public override void Execute(HNDBuildEnvironment environment)
        {
            BuildTargetGroup buildTargetGroup = environment.GetValue<BuildTargetGroup>(HNDBuildConstants.BUILD_TARGET_GROUP);

            foreach (PlatformFolderSpec spec in m_ExludedFolders)
            {
                if (spec.TargetGroup == buildTargetGroup)
                {
                    if (environment.CurrentBuildState == HNDBuildEnvironment.BuildStage.PreBuild)
                    {
                        HideFolders(environment, spec.FolderPath, m_HidePath);
                    }
                    else if (environment.CurrentBuildState == HNDBuildEnvironment.BuildStage.PostBuild)
                    {
                        ShowFolders(environment, spec.FolderPath, m_HidePath);
                    }
                }
            }

            // Delete hidden dir when the build is done
            if (environment.CurrentBuildState == HNDBuildEnvironment.BuildStage.PostBuild)
            {
                var hiddenPathRoot = Path.Combine(Application.dataPath, m_HidePath);
                if (Directory.Exists(hiddenPathRoot))
                {
                    Directory.Delete(hiddenPathRoot, true);
                }

                // Leave just the empty directory
                Directory.CreateDirectory(hiddenPathRoot);
            }
        }

        private static void HideFolders(HNDBuildEnvironment environment, string folderPath, string to)
        {
            var globalPath = Application.dataPath;
            var hiddenPathRoot = Path.Combine(globalPath, to);

            if (!Directory.Exists(hiddenPathRoot))
            {
                Directory.CreateDirectory(hiddenPathRoot);
            }

            var folderFrom = Path.Combine(globalPath, folderPath);
            var folderTo = Path.Combine(hiddenPathRoot, folderPath);

            environment.Log("Hiding folder '" + folderFrom + "' to: " + folderTo);

            if (!Directory.Exists(folderTo))
            {
                // this will create the recursive path
                Directory.CreateDirectory(folderTo);
                // this will only delete the last directory in path
                Directory.Delete(folderTo);

                if (Directory.Exists(folderFrom))
                {
                    Directory.Move(folderFrom, folderTo);
                }
                else
                {
                    environment.LogWarning("Folder'" + folderFrom + "' does not exist!");
                }

                if (File.Exists(folderFrom + ".meta"))
                {
                    File.Move(folderFrom + ".meta", folderTo + ".meta");
                }
                else
                {
                    environment.LogWarning("'" + folderFrom + ".meta'" + " does not exist!");
                }
            }
            else
            {
                environment.LogWarning("Path already exists: " + folderTo);
            }
        }

        public static void ShowFolders(HNDBuildEnvironment environment, string folderPath, string from)
        {
            var globalPath = Application.dataPath;
            var hiddenPathRoot = Path.Combine(globalPath, from);

            var folderFrom = Path.Combine(hiddenPathRoot, folderPath);
            var folderTo = Path.Combine(globalPath, folderPath);

            UnityEngine.Debug.Log("Showing folder '" + folderTo + "' from: " + folderFrom);

            if (!Directory.Exists(folderTo))
            {
                if (Directory.Exists(folderFrom))
                {
                    Directory.Move(folderFrom, folderTo);
                }
                else
                {
                    environment.LogWarning("'" + folderFrom + "' does not exist!");
                }

                if (File.Exists(folderFrom + ".meta"))
                {
                    File.Move(folderFrom + ".meta", folderTo + ".meta");
                }
                else
                {
                    environment.LogWarning("'" + folderFrom + ".meta'" + " does not exist!");
                }
            }
            else
            {
                if (Directory.Exists(folderFrom))
                {
                    environment.LogWarning("Path already exists: " + folderTo + ". Deleting hidden one.");
                    Directory.Delete(folderFrom, true);
                }
                if (File.Exists(folderFrom + ".meta"))
                {
                    environment.LogWarning("Path already exists: " + folderTo + ". Deleting hidden one.");
                    File.Delete(folderFrom + ".meta");
                }
            }
        }
    }
}