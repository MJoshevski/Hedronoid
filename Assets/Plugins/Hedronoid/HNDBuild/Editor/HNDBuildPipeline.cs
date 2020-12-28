using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    [CreateAssetMenu(fileName = "BuildPipeline", menuName = "Build/Pipeline")]
    public class HNDBuildPipeline : ScriptableObject
    {
        [SerializeField]
        private bool m_RunPostBuildEvenOnFail = true;

        [SerializeField]
        private List<HNDBuildStep> m_PrebuildSteps = new List<HNDBuildStep>();

        [SerializeField]
        private List<HNDBuildStep> m_BuildSteps = new List<HNDBuildStep>();

        [SerializeField]
        private List<HNDBuildStep> m_PostBuildSteps = new List<HNDBuildStep>();

        int m_StepCount = 0;
        int m_CurrentStep = 0;
        List<string> m_StepNames = new List<string>();

#if UNITY_EDITOR
        public static HNDBuildPipeline LoadPipeline(string assetName)
        {
            string assetGUID = UnityEditor.AssetDatabase.FindAssets(assetName)[0];
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
            HNDBuildPipeline pipeline = UnityEditor.AssetDatabase.LoadAssetAtPath<HNDBuildPipeline>(assetPath);
            return pipeline;
        }

        private void UpdateProgressBar(string stepName, int current, int max)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Building (" + current + " / " + max + ")",
                "Building step " + stepName,
                (float)current / max))
            {
                throw new Exception("Build cancelled!");
            }
        }

        public void Build(HNDBuildEnvironment environment = null)
        {
            if (environment == null)
            {
                environment = new HNDBuildEnvironment();
            }

            environment.CurrentBuildState = HNDBuildEnvironment.BuildStage.PreBuild;

            m_CurrentStep = 0;
            m_StepCount = m_PrebuildSteps.Count + m_BuildSteps.Count + m_PostBuildSteps.Count;

            // Need to chache as the GameObject parts of build steps get destroyed during the build, weird
            m_StepNames.Clear();
            m_StepNames.AddRange(m_PrebuildSteps.Select(s => s.name));
            m_StepNames.AddRange(m_BuildSteps.Select(s => s.name));
            m_StepNames.AddRange(m_PostBuildSteps.Select(s => s.name));

            UpdateProgressBar("", m_CurrentStep, m_StepCount);

            Exception buildException = null;

            try
            {
                // Execute each prebuild step
                environment.Log("Starting prebuild steps!");
                foreach (HNDBuildStep s in m_PrebuildSteps)
                {
                    environment.Log("Starting build step " + m_StepNames[m_CurrentStep]);
                    UpdateProgressBar(m_StepNames[m_CurrentStep], m_CurrentStep++, m_StepCount);
                    s.Execute(environment);
                }

                // Extract runtime environment
                environment.Log("Exporting runtime evnironment!");
                Dictionary<string, object> export = environment.AllVariables.Where(kv => kv.Value.ExportToRuntime)
                   .ToDictionary(kv => kv.Key, kv => kv.Value.Value);
                HNDBuildRuntimeEnvironment.SaveToResources(export, HNDBuildRuntimeEnvironment.ENV_RESOURCE_DEFAULT_PATH);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                environment.WriteEnabled = false;
                environment.CurrentBuildState = HNDBuildEnvironment.BuildStage.Build;

                // Execute build
                environment.Log("Starting build steps!");
                foreach (HNDBuildStep s in m_BuildSteps)
                {
                    environment.Log("Starting build step " + m_StepNames[m_CurrentStep]);
                    UpdateProgressBar(m_StepNames[m_CurrentStep], m_CurrentStep++, m_StepCount);
                    s.Execute(environment);
                }
            }
            catch (Exception e)
            {
                // Prebuild or build exception
                environment.LogError("Build exception: " + e);
                environment.LogError("Stack trace: " + e.StackTrace);
                buildException = e;
            }
            finally
            {
                try
                {
                    if (m_RunPostBuildEvenOnFail || buildException == null)
                    {
                        ExecutePostBuildSteps(environment);
                    }
                }
                catch (Exception e)
                {
                    // Post build exception
                    if (buildException != null)
                    {
                        environment.LogError("Post build exception: " + e);
                        environment.LogError("Stack trace: " + e.StackTrace);
                    }
                    else
                    {
                        buildException = e;
                    }
                }

                FinishBuild();
            }

            if (buildException != null)
            {
                environment.LogError("Build failed! - " + buildException);
                environment.LogError("Stack trace: " + buildException.StackTrace);
                throw buildException;
            }
            else
            {
                environment.Log("Build completed!");
            }
        }

        private void FinishBuild()
        {
            EditorUtility.ClearProgressBar();
        }

        private void ExecutePostBuildSteps(HNDBuildEnvironment environment)
        {
            // Execute post build
            environment.CurrentBuildState = HNDBuildEnvironment.BuildStage.PostBuild;
            environment.Log("Starting post build steps!");
            m_CurrentStep = m_PrebuildSteps.Count + m_BuildSteps.Count;
            foreach (HNDBuildStep s in m_PostBuildSteps)
            {
                environment.Log("Starting build step " + m_StepNames[m_CurrentStep]);
                UpdateProgressBar(m_StepNames[m_CurrentStep], m_CurrentStep++, m_StepCount);
                s.Execute(environment);
            }
        }

#endif
    }
}