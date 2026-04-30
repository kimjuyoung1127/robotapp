// Folder: Editor/CliTools - Project-side unityctl bridge fallback bootstrap
using System.IO;
using UnityEditor;
using UnityEngine;
using Unityctl.Plugin.Editor.Ipc;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Re-starts the unityctl IPC bridge from the project side when the package bootstrap
    /// does not reattach cleanly after editor restart.
    /// </summary>
    [InitializeOnLoad]
    internal static class UnityctlBridgeProjectBootstrap
    {
        private static bool scheduled;

        static UnityctlBridgeProjectBootstrap()
        {
            Initialize();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += ScheduleStart;
        }

        private static void ScheduleStart()
        {
            if (scheduled)
            {
                return;
            }

            scheduled = true;
            EditorApplication.update += WaitForEditorReady;
        }

        private static void WaitForEditorReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            EditorApplication.update -= WaitForEditorReady;
            scheduled = false;
            TryStartBridge();
        }

        private static void TryStartBridge()
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                Debug.LogWarning("[unityctl] Project bootstrap skipped: project path missing");
                return;
            }

            var settingsPath = Path.Combine(projectPath, "ProjectSettings", "UnityctlSettings.asset");
            if (!File.Exists(settingsPath))
            {
                Debug.LogWarning($"[unityctl] Project bootstrap skipped: settings file missing at {settingsPath}");
                return;
            }

            if (!File.ReadAllText(settingsPath).Contains("\"Enabled\": true"))
            {
                Debug.Log("[unityctl] Project bootstrap skipped: UnityctlSettings disabled");
                return;
            }

            try
            {
                IpcServer.Instance.Start(projectPath);
                Debug.Log("[unityctl] Project bootstrap ensured IPC bridge start");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[unityctl] Project bootstrap failed: {ex}");
            }
        }
    }
}
