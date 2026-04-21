// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// 씬 이름과 내비게이션 메타데이터를 제공합니다.
    /// </summary>
    public static class SceneCatalog
    {
        private static readonly SceneEntry[] Entries =
        {
            new(SceneId.Boot, "Boot", "Boot", false),
            new(SceneId.Onboarding, "Onboarding", "Onboarding", false),
            new(SceneId.MathReadiness, "MathReadiness", "Math Readiness", true),
            new(SceneId.RobotLibrary, "RobotLibrary", "Robot Library", true),
            new(SceneId.Sandbox, "Sandbox", "Sandbox", true),
            new(SceneId.RobotControl, "RobotControl", "Robot Control", true),
            new(SceneId.RobotControlV2, "RobotControlV2", "로봇 제어 V2", true),
            new(SceneId.RobotControlV3, "RobotControlV3", "로봇 제어 V3", true),
        };

        public static IReadOnlyList<SceneEntry> GetNavigableEntries()
        {
            return Array.FindAll(Entries, entry => entry.ShowInNavigation);
        }

        public static string GetSceneName(SceneId sceneId)
        {
            foreach (var entry in Entries)
            {
                if (entry.Id == sceneId)
                {
                    return entry.SceneName;
                }
            }

            return string.Empty;
        }

        public static string GetDisplayName(SceneId sceneId)
        {
            foreach (var entry in Entries)
            {
                if (entry.Id == sceneId)
                {
                    return entry.DisplayName;
                }
            }

            return sceneId.ToString();
        }

        public static bool TryGetSceneId(string sceneName, out SceneId sceneId)
        {
            foreach (var entry in Entries)
            {
                if (string.Equals(entry.SceneName, sceneName, StringComparison.Ordinal))
                {
                    sceneId = entry.Id;
                    return true;
                }
            }

            sceneId = default;
            return false;
        }

        public static bool IsKnownScene(string sceneName)
        {
            return TryGetSceneId(sceneName, out _);
        }

        public static SceneId GetCurrentSceneId()
        {
            var activeScene = SceneManager.GetActiveScene();
            return TryGetSceneId(activeScene.name, out var sceneId) ? sceneId : SceneId.Sandbox;
        }
    }

    /// <summary>
    /// 씬 내비게이션 항목입니다.
    /// </summary>
    public readonly struct SceneEntry
    {
        public SceneEntry(SceneId id, string sceneName, string displayName, bool showInNavigation)
        {
            Id = id;
            SceneName = sceneName;
            DisplayName = displayName;
            ShowInNavigation = showInNavigation;
        }

        public SceneId Id { get; }
        public string SceneName { get; }
        public string DisplayName { get; }
        public bool ShowInNavigation { get; }
    }
}
