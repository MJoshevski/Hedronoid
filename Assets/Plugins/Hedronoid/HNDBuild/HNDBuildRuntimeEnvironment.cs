using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    /// <summary>
    /// Build environment accessible at runtime
    /// Values are added during the build and stored into json file that is loaded on first access.
    /// </summary>
    public class HNDBuildRuntimeEnvironment
    {
        public const string ENV_RESOURCE_DEFAULT_PATH = "HNDBuildRuntimeEnvironment";

        private static HNDBuildRuntimeEnvironment s_Instance;

        public static HNDBuildRuntimeEnvironment Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new HNDBuildRuntimeEnvironment();
                    s_Instance.LoadFromResources(ENV_RESOURCE_DEFAULT_PATH);
                }
                return s_Instance;
            }
        }

        private Dictionary<string, object> m_RuntimeEnvironment = new Dictionary<string, object>();

#if UNITY_EDITOR
        public static void SaveToResources(Dictionary<string, object> export, string filename)
        {
            string json = MiniJSON.Json.Serialize(export);
            Debug.Log("HNDBuild: Runtime build environment:\n" + json);
            string dir = Path.Combine(Application.dataPath, "Resources");

            // Needs to be .txt so unity treats it as a TextAsset
            filename = Path.Combine(dir, filename + ".txt");

            StreamWriter writer = new StreamWriter(filename);
            writer.Write(json);
            writer.Close();
            Debug.Log("HNDBuild: Build environment saved to: " + filename);
        }
#endif

        private void LoadFromResources(string path)
        {
            TextAsset e = (TextAsset)Resources.Load(path);
            Dictionary<string, object> parsed = (Dictionary<string, object>)MiniJSON.Json.Deserialize(e.text);
            Resources.UnloadAsset(e);
            m_RuntimeEnvironment = parsed;
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (!m_RuntimeEnvironment.ContainsKey(key))
            {
                return defaultValue;
            }

            object value = m_RuntimeEnvironment[key];
            if (!(value is T))
            {
                Debug.LogWarning("HNDBuild: Variable with key " + key + " is of unexpected type: " + value.GetType() + ". Expected type: " + typeof(T).Name);
                return defaultValue;
            }

            return (T)value;
        }
    }
}