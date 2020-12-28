using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    [CreateAssetMenu(fileName = "PlayerSettingsPlatformOverrider", menuName = "Build/Steps/PlayerSettings Platform Overrider")]
    public class PlayerSettingsPlatformOverriderStep : HNDBuildStep
    {
        [Serializable]
        public class Override
        {
            public BuildTargetGroup BuildTargetGroup;
            public ColorSpace ColorSpace;
            public ShaderVariantCollection PreloadedShaders;
            public bool GraphicsJobs = true;
            public bool MultithreadedRendering = true;
            public bool BuiltInMotionVectors = true;
            public bool BuiltInDepthNormals = true;
            public bool BuiltInScreenSpaceShadows = true;
            public Shader ScreenSpaceShadowsShader = null;
        }

        [SerializeField]
        List<Override> m_Overrides;

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

            Override o = m_Overrides.FirstOrDefault(ov => ov.BuildTargetGroup == buildTargetGroup);
            if (o != null)
            {
                PlayerSettings.colorSpace = o.ColorSpace;
                PlayerSettings.graphicsJobs = o.GraphicsJobs;

                // Only set this for mobile platfroms
                if (buildTargetGroup == BuildTargetGroup.Android || buildTargetGroup == BuildTargetGroup.iOS)
                {
                    PlayerSettings.SetMobileMTRendering(buildTargetGroup, o.MultithreadedRendering);
                }
                
                UpdateGraphicsSettings(environment, o);
            }
        }

        private void UpdateGraphicsSettings(HNDBuildEnvironment environment, Override o)
        {
            // From https://support.unity3d.com/hc/en-us/articles/115000177803-How-can-I-modify-Project-Settings-via-scripting-

            const string graphicsSettingsAssetPath = "ProjectSettings/GraphicsSettings.asset";
            SerializedObject graphicsManager = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(graphicsSettingsAssetPath)[0]);

            // Preloaded shaders
            {
                SerializedProperty preloadedShadersProp = graphicsManager.FindProperty("m_PreloadedShaders");
                environment.Log("Cleaning preloaded shaders");
                preloadedShadersProp.ClearArray();
                if (o.PreloadedShaders != null)
                {
                    environment.Log("Inserting preloaded shaders");
                    preloadedShadersProp.InsertArrayElementAtIndex(0);
                    SerializedProperty m_ShaderCollectionElement = preloadedShadersProp.GetArrayElementAtIndex(0);
                    m_ShaderCollectionElement.objectReferenceValue = o.PreloadedShaders;
                }
            }

            // Motion vectors (no support = 0, built in = 1)
            {
                SerializedProperty mainProp = graphicsManager.FindProperty("m_MotionVectors");
                SerializedProperty modeProp = mainProp.FindPropertyRelative("m_Mode");
                modeProp.intValue = o.BuiltInMotionVectors ? 1 : 0;
            }

            // Depth normals (no support = 0, built in = 1)
            {
                SerializedProperty mainProp = graphicsManager.FindProperty("m_DepthNormals");
                SerializedProperty modeProp = mainProp.FindPropertyRelative("m_Mode");
                modeProp.intValue = o.BuiltInDepthNormals ? 1 : 0;
            }

            // Screen space shadows shader
            {
                SerializedProperty mainProp = graphicsManager.FindProperty("m_ScreenSpaceShadows");
                SerializedProperty modeProp = mainProp.FindPropertyRelative("m_Mode");
                modeProp.intValue = o.BuiltInScreenSpaceShadows ? 1 : 0;

                // Custom shader?
                if (!o.BuiltInScreenSpaceShadows && o.ScreenSpaceShadowsShader != null)
                {
                    modeProp.intValue = 2;
                    SerializedProperty shaderProp = mainProp.FindPropertyRelative("m_Shader");
                    shaderProp.objectReferenceValue = o.ScreenSpaceShadowsShader;
                }
            }

            graphicsManager.ApplyModifiedProperties();
        }

    }
}