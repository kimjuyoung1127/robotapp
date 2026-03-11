// Folder: App - application orchestration and runtime state.
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// ???대쫫怨??대퉬寃뚯씠??硫뷀??곗씠?곕? ?쒓났?⑸땲??
    /// </summary>
    public static class SceneCatalog
    {
        private static readonly SceneEntry[] Entries =
        {
            new(SceneId.Boot, "Boot", "Boot", false),
            new(SceneId.Onboarding, "Onboarding", "Onboarding", true),
            new(SceneId.Main, "Main", "Main", true),
            new(SceneId.RobotLibrary, "RobotLibrary", "Robot Library", true),
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
            return TryGetSceneId(activeScene.name, out var sceneId) ? sceneId : SceneId.Main;
        }
    }

    /// <summary>
    /// ???ㅻ퉬寃뚯씠????ぉ?낅땲??
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

