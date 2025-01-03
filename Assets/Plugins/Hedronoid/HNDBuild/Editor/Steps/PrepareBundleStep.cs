﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    /// <summary>
    /// Step for defining and preparing an asset bundle
    /// Needs BuildBundlesStep later in the build pipeline to actually build bundles.
    /// </summary>
    [CreateAssetMenu(fileName = "PrepareBundleStep", menuName = "Build/Steps/Prepare Bundle Step")]
    public class PrepareBundleStep : HNDBuildStep
    {
        public const string ENV_BUNDLE_DEFS = "BUNDLE_DEFS";

        [SerializeField]
        protected string m_BundleName;

        [SerializeField]
        protected UnityEditor.BuildTarget[] m_BuildTargets;

        [SerializeField]
        protected string[] m_NamesOfInputFolders;

        [SerializeField]
        protected string[] m_ExcludeFoldersNamed;

        [SerializeField]
        protected string[] m_IncludeOnlyFoldersNamed;

        [SerializeField]
        protected string[] m_FilterOnlyExtensions;

        public class BundleDef
        {
            public string BundleName;
            public AssetBundleBuild BundleBuild;
        }

        public override List<string> GetRequiredEnvValues()
        {
            return new List<string>
            {
                HNDBuildConstants.BUILD_TARGET,
            };
        }

        public override void Execute(HNDBuildEnvironment environment)
        {
            // Check if we should make this bundle
            if (ShouldBuild(environment) == false)
            {
                return;
            }

            AssetBundleBuild bundleDef = PrepareBundle(environment, m_BundleName, m_NamesOfInputFolders, m_ExcludeFoldersNamed, m_IncludeOnlyFoldersNamed, m_FilterOnlyExtensions);

            if (string.IsNullOrEmpty(bundleDef.assetBundleName))
            {
                environment.LogError("Bundle '" + m_BundleName + "' build failed");
            }
        }

        protected bool ShouldBuild(HNDBuildEnvironment environment)
        {
            BuildTarget buildTarget = environment.GetValue<BuildTarget>(HNDBuildConstants.BUILD_TARGET);
            if (m_BuildTargets == null || !m_BuildTargets.Contains(buildTarget))
            {
                environment.Log("Skipping bundle '" + m_BundleName + "' as it's not for this build target: " + buildTarget);
                return false;
            }
            return true;
        }

        protected AssetBundleBuild PrepareBundle(HNDBuildEnvironment environment, string bundleName, string[] namesOfInputFolders,
            string[] excludeFoldersNamed, string[] includeOnlyFoldersNamed, string[] extensionsFilter)
        {
            if (extensionsFilter == null)
            {
                extensionsFilter = new string[0];
            }
            extensionsFilter = extensionsFilter.Select(e => e.ToLowerInvariant()).ToArray();

            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();

            // Collect all assets
            List<string> allAssetPaths = new List<string>();
            HashSet<string> allNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            // take only first part, as GetDirectionries can't take composite paths as a folder name
            foreach (string folderPath in namesOfInputFolders)
            {
                // Add all excluded folders for nonlocalized bundles
                string[] excludes = excludeFoldersNamed;

                List<string> paths = GetAssetPathsForFolder(folderPath, excludes, includeOnlyFoldersNamed);
                // We have to remove duplicate paths. Some assets, such as sprites, are found twice, but the asset path always refer to the same object.
                // So in cases where we have to identical paths, it's safe to remove one.
                paths = paths.Distinct().ToList();
                for (int i = 0; i < paths.Count; i++)
                {
                    string path = paths[i];

                    // Check for filename conflicts in the bundle
                    string filename = Path.GetFileName(path);

                    // skip invalid extensions
                    if (extensionsFilter.Length > 0)
                    {
                        string ext = Path.GetExtension(filename).ToLowerInvariant();
                        if (!extensionsFilter.Contains(ext))
                        {
                            continue;
                        }
                    }

                    if (allNames.Contains(filename))
                    {
                        environment.LogError("Multiple files named '" + filename + "' in path '" + path + "' for the bundle '" + bundleName + "'");
                        return default(AssetBundleBuild);
                    }
                    allNames.Add(filename);
                    allAssetPaths.Add(path);
                }
            }

            if (allAssetPaths.Count == 0)
            {
                environment.LogWarning("Bundle '" + bundleName + "' was not built, since it would contain no assets.");
                return default(AssetBundleBuild);

            }

            assetBundleBuild.assetNames = new string[allAssetPaths.Count];
            assetBundleBuild.addressableNames = new string[allAssetPaths.Count];
            for (int i = 0; i < allAssetPaths.Count; i++)
            {
                assetBundleBuild.assetNames[i] = allAssetPaths[i];
                assetBundleBuild.addressableNames[i] = assetBundleBuild.assetNames[i];
            }
            assetBundleBuild.assetBundleName = bundleName;
            assetBundleBuild.assetBundleVariant = "";

            Dictionary<string, BundleDef> bundles = new Dictionary<string, BundleDef>();
            bundles = environment.GetValueOrDefault(ENV_BUNDLE_DEFS, bundles);

            bundles[bundleName] = new BundleDef { BundleName = bundleName, BundleBuild = assetBundleBuild };
            environment.SetValue(ENV_BUNDLE_DEFS, bundles, false);

            environment.Log("Asset Bundle '" + bundleName + "' ready.");

            return assetBundleBuild;
        }

        protected static List<string> GetAssetPathsForFolder(string folderPath, string[] excludes, string[] includesOnly)
        {
            string folderName = folderPath.Split('/').Last();
            string[] dirs = Directory.GetDirectories(Application.dataPath, folderName, SearchOption.AllDirectories);
            List<string> assets = new List<string>();

            foreach (string dir in dirs)
            {
                string unityPath = dir.Replace(Application.dataPath, "Assets");
                unityPath = unityPath.Replace("\\", "/");

                if (!unityPath.Contains(folderPath))
                {
                    continue;
                }

                string[] names = AssetDatabase.FindAssets(null, new string[] { unityPath });
                names.ToList().ForEach(n =>
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(n);

                    // skip directories
                    if (Directory.Exists(assetPath))
                    {
                        return;
                    }

                    if (includesOnly.Length > 0)
                    {
                        if (!includesOnly.Any(i => assetPath.Contains("/" + i + "/")))
                        {
                            //Debug.LogWarning("Skipping (includes only) " + assetPath);
                            return;
                        }
                    }

                    if (excludes.Length > 0)
                    {
                        if (excludes.Any(i => assetPath.Contains("/" + i + "/")))
                        {
                            //Debug.LogWarning("Skipping (excludes) " + assetPath);
                            return;
                        }
                    }

                    assets.Add(assetPath);
                });
            }

            return assets;
        }
    }
}