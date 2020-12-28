using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    /// <summary>
    /// Build environment during the build.
    /// Use to store and read values between steps
    /// Use to export values to runtime build environment.
    /// </summary>
    public class HNDBuildEnvironment
    {
        public class EnvProperty
        {
            public bool ExportToRuntime;
            public object Value;
        }

        private Dictionary<string, EnvProperty> m_EnvironmentVariables = new Dictionary<string, EnvProperty>();

        public bool WriteEnabled { get; set; }

        public enum BuildStage
        {
            PreBuild,
            Build,
            PostBuild,
        }

        public BuildStage CurrentBuildState { get; set; }

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (!m_EnvironmentVariables.ContainsKey(key))
            {
                Log("Variable with key " + key + " is not present in the environment!");
                return defaultValue;
            }

            EnvProperty prop = m_EnvironmentVariables[key];
            if (!(prop.Value is T))
            {
                string currentPropName = "";
                if (prop.Value != null) currentPropName = prop.Value.GetType().Name;

                LogError("Variable with key " + key + " is of unexpected type. Expected type: '" + typeof(T).Name + "'. Current type: '" + currentPropName + "'");
                return defaultValue;
            }

            return (T)(prop.Value);
        }

        public T GetValue<T>(string key)
        {
            if (!m_EnvironmentVariables.ContainsKey(key))
            {
                throw new Exception("Variable with key " + key + " is not present in the environment!");
            }

            EnvProperty prop = m_EnvironmentVariables[key];
            if (!(prop.Value is T))
            {
                LogError("Variable with key " + key + " is of unexpected type. Expected type: '" + typeof(T).Name + "'. Current type: '" + prop.Value.GetType().Name + "'");
                return default(T);
            }

            return (T)(prop.Value);
        }

        public void SetValue(string key, object value, bool exportToRuntime = false)
        {
            m_EnvironmentVariables[key] = new EnvProperty { Value = value, ExportToRuntime = exportToRuntime };
        }

        public void LogError(string message)
        {
            Debug.LogError("HNDBuild [Error]: " + message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning("HNDBuild [Warn]: " + message);
        }

        public void Log(string message)
        {
            Debug.Log("HNDBuild [Info]: " + message);
        }

        public void LogErrorFormat(string message, params object[] pars)
        {
            Debug.LogErrorFormat("HNDBuild [Error]: " + message, pars);
        }

        public void LogWarningFormat(string message, params object[] pars)
        {
            Debug.LogWarningFormat("HNDBuild [Warn]: " + message, pars);
        }

        public void LogFormat(string message, params object[] pars)
        {
            Debug.LogFormat("HNDBuild [Info]: " + message, pars);
        }

#if UNITY_EDITOR
        public Dictionary<string, EnvProperty> AllVariables { get { return m_EnvironmentVariables; } }
#endif
    }
}