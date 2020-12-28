using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    /// <summary>
    /// Builds all predefined bundles, caches and saves them
    /// </summary>
    [CreateAssetMenu(fileName = "BuildBundlesStep", menuName = "Build/Steps/Build Bundle Step")]
    public class BuildBundlesStep : HNDBuildStep
    {
        public const string ENV_CLEANUP_CACHE_DIR = "BuildBundlesStep_CleanupCacheDir";
        public const string ENV_CLEANUP_TARGET_DIR = "BuildBundlesStep_CleanupTargetDir";
        public const string ENV_USE_CACHED = "BuildBundlesStep_UseCached";

        [SerializeField]
        string m_BundlesOutputPath;

        [SerializeField]
        string m_BundlesCachePath;

        public override List<string> GetRequiredEnvValues()
        {
            return new List<string>
            {
                ENV_CLEANUP_CACHE_DIR,
                ENV_CLEANUP_TARGET_DIR,
                ENV_USE_CACHED,
                HNDBuildConstants.BUILD_TARGET,
                PrepareBundleStep.ENV_BUNDLE_DEFS
            };
        }

        public override void Execute(HNDBuildEnvironment environment)
        {
            // Cleanup all cached bundles
            if (environment.GetValueOrDefault(ENV_CLEANUP_CACHE_DIR, true))
            {
                if (Directory.Exists(m_BundlesCachePath))
                {
                    string[] dirs = Directory.GetDirectories(m_BundlesCachePath);
                    foreach (string d in dirs)
                    {
                        Directory.Delete(d, true);
                    }
                }
            }

            // Cleanup all bundles in the assets
            if (environment.GetValueOrDefault(ENV_CLEANUP_TARGET_DIR, true))
            {
                if (Directory.Exists(m_BundlesOutputPath))
                {
                    string[] files = Directory.GetFiles(m_BundlesOutputPath);
                    foreach (string f in files)
                    {
                        File.Delete(f);
                    }
                }
            }

            BuildTarget buildTarget = environment.GetValue<BuildTarget>(HNDBuildConstants.BUILD_TARGET);

            Dictionary<string, PrepareBundleStep.BundleDef> bundleDefs = environment.GetValueOrDefault<Dictionary<string, PrepareBundleStep.BundleDef>>(PrepareBundleStep.ENV_BUNDLE_DEFS);
            if (bundleDefs == null || bundleDefs.Count == 0)
            {
                environment.LogWarning("No bundles to build!");
                return;
            }

            // Prepare bundle cache path
            string cacheDir = Path.Combine(m_BundlesCachePath, ((int)buildTarget).ToString());
            string caheDirWithTarget = Application.dataPath.Replace("Assets", "") + cacheDir;
            if (!Directory.Exists(caheDirWithTarget))
            {
                Directory.CreateDirectory(caheDirWithTarget);
            }

            // Use cache if all bundles are in the cache?
            bool allFilesExist = false;
            if (environment.GetValueOrDefault(ENV_USE_CACHED, false))
            {
                allFilesExist = true;
                // Use cached bundles if available
                foreach (var def in bundleDefs)
                {
                    string cachePath = Path.Combine(caheDirWithTarget, def.Value.BundleBuild.assetBundleName.ToLowerInvariant());
                    if (!File.Exists(cachePath))
                    {
                        allFilesExist = false;
                        break;
                    }
                }

                if (allFilesExist)
                {
                    environment.Log("All bundles are using cache!");
                }
                else
                {
                    environment.LogWarning("Some bundles are missing from cache, rebuilding all!");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetBundleManifest abm = null;
            if (!allFilesExist)
            {
                // Build those bundles
                abm = BuildPipeline.BuildAssetBundles(caheDirWithTarget, bundleDefs.Values.Select(d => d.BundleBuild).ToArray(),
                     BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);

                if (abm == null)
                {
                    environment.LogError("Failed building asset bundles!");
                    return;
                }
            }

            // Make bundles dir if it doesn't exist
            if (!Directory.Exists(m_BundlesOutputPath))
            {
                Directory.CreateDirectory(m_BundlesOutputPath);
            }

            // Save built bundles to environment + copy bundles to the target dir
            Dictionary<string, string> bundles = new Dictionary<string, string>();
            foreach (var def in bundleDefs)
            {
                string bundleName = def.Value.BundleBuild.assetBundleName;

                // Bundles built but manifest for this bundle is missing?!
                if (abm != null && !abm.GetAllAssetBundles().Contains(bundleName.ToLowerInvariant()))
                {
                    environment.LogError("Bundle '" + bundleName + "' was not built even when if was defined!");
                    continue;
                }

                bundles.Add(bundleName, bundleName);

                string destination = Path.Combine(m_BundlesOutputPath, bundleName);
                File.Copy(Path.Combine(caheDirWithTarget, bundleName.ToLowerInvariant()), destination, true);

                environment.Log("Bundle '" + bundleName + "' built and copied to '" + destination + "'");
            }
            environment.SetValue("Bundles", bundles, true);
        }

    }
}