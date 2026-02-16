using System;
using System.IO;
using MascotDesktop.Runtime.Avatar;
using UnityEditor;
using UnityEngine;

namespace MascotDesktop.Editor
{
    public static class Automation
    {
        [MenuItem("MascotDesktop/Automation/Ping")]
        public static void Ping()
        {
            Debug.Log("[Automation] Ping ok");
        }

        public static void LogPaths()
        {
            var assetsDir = Path.GetFullPath(Application.dataPath);
            var projectRoot = Directory.GetParent(assetsDir)?.FullName ?? string.Empty;
            var unityPjRoot = Directory.GetParent(projectRoot)?.FullName ?? string.Empty;
            var canonicalAssets = Path.Combine(unityPjRoot, "data", "assets_user");

            Debug.Log($"[Automation] Application.dataPath={assetsDir}");
            Debug.Log($"[Automation] projectRoot={projectRoot}");
            Debug.Log($"[Automation] unityPjRoot={unityPjRoot}");
            Debug.Log($"[Automation] canonicalAssets={canonicalAssets}");
        }

        [MenuItem("MascotDesktop/Automation/LogDefaultModelPath")]
        public static void LogDefaultModelPath()
        {
            var go = new GameObject("AutomationProbe");
            try
            {
                var config = go.AddComponent<SimpleModelConfig>();
                Debug.Log($"[Automation] Default modelRelativePath={config.modelRelativePath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        public static void ValidateDefaultModelPath()
        {
            var assetsDir = Path.GetFullPath(Application.dataPath);
            var projectRoot = Directory.GetParent(assetsDir)?.FullName ?? string.Empty;
            var unityPjRoot = Directory.GetParent(projectRoot)?.FullName ?? string.Empty;
            var canonicalAssets = Path.Combine(unityPjRoot, "data", "assets_user");

            var go = new GameObject("AutomationProbe");
            try
            {
                var config = go.AddComponent<SimpleModelConfig>();
                var absolutePath = Path.Combine(canonicalAssets, config.modelRelativePath);
                var exists = File.Exists(absolutePath);
                Debug.Log($"[Automation] Default model absolutePath={absolutePath}");
                if (exists)
                {
                    Debug.Log("[Automation] Default model file exists");
                }
                else
                {
                    Debug.LogError("[Automation] Default model file not found");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
