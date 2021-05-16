using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hedronoid.DependencyChecker
{
    /// <summary>
    /// Searches for all unused assets. Inspired by DependencyPro plugin.
    /// </summary>
    public class DependencyChecker
    {

        public static readonly string[] AssetExtensionsWithoutReference = { ".fbx", ".png", ".jpg", ".bmp", ".tga", ".jpeg", ".psd", ".guiskin", ".txt", ".anim" };

        public static readonly string[] ScriptAssetExtensions = { ".cs", ".js" };

        public static readonly string[] TextAssetExtensions = { ".txt", ".html", ".csv", ".doc", ".pdf" };

        public static readonly string[] AudioAssetExtensions = { ".ogg", ".wav", ".mp3" };

        public static readonly string[] ShaderAssetExtensions = { ".cginc", ".shader", ".mp3" };

        private static bool ShouldSkipPath(string path, string[] extensions)
        {
            //return path.Contains(".unity") || AssetExtensionsWithoutReference.Any(t => path.Contains(t));
            return Directory.Exists(path) || extensions.Any(t => path.Contains(t));
        }

        public static Dictionary<string, HashSet<string>> BuildReferenceMap(string[] extensionsToSkip)
        {
            if (extensionsToSkip == null)
            {
                extensionsToSkip = AssetExtensionsWithoutReference;
            }

            // all objects
            string[] objectGuids = AssetDatabase.FindAssets("t:Object");
            string[] objectPaths = objectGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => !ShouldSkipPath(p, extensionsToSkip)).ToArray();
            long total = objectPaths.LongLength;

            // prepare map
            Dictionary<string, HashSet<string>> map = new Dictionary<string, HashSet<string>>();
            for (int i = 0; i < total; i++)
            {
                var assetPath = objectPaths[i];
                if (!map.ContainsKey(assetPath))
                {
                    map.Add(assetPath, new HashSet<string>(StringComparer.Ordinal));
                }
            }

            // search in assets
            for (int i = 0; i < total; i++)
            {
                var assetPath = objectPaths[i];

                foreach (string dependency in AssetDatabase.GetDependencies(assetPath))
                {
                    if (map.ContainsKey(dependency))
                    {
                        map[dependency].Add(assetPath);
                    }
                }
                //HashSet<string> references = new HashSet<string>(AssetDatabase.GetDependencies(assetPath), StringComparer.Ordinal);
                //map[assetPath] = references;

                if (EditorUtility.DisplayCancelableProgressBar("Building reference map...", assetPath, (float)i / total))
                {
                    break;
                }
            }

            EditorUtility.ClearProgressBar();

            return map;
        }

        public static string[] GetUnusedAssets(Dictionary<string, HashSet<string>> map)
        {
            // Debug.Log("No references for:");
            // foreach (var kv in map)
            // {
            //     if (kv.Value.Count == 1)
            //     {
            //         Debug.Log(kv.Key);
            //     }
            // }
            // Debug.Log("That's it!");

            return map.Where(kv => kv.Value.Count == 1).Select(kv => kv.Key).ToArray();
        }


    }
}