// Folder: Editor/CliTools - unity-cli 커스텀 도구: Build Settings 검증
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Build Settings의 씬 목록을 검증하고 누락/중복을 보고합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate Build Settings scene list against known project scenes")]
    public static class BuildSettingsTool
    {
        private static readonly string[] expectedScenes =
        {
            "Boot", "Onboarding", "MathReadiness",
            "RobotLibrary", "Sandbox", "RobotControl"
        };

        public static object HandleCommand(JObject @params)
        {
            var buildScenes = EditorBuildSettings.scenes;
            var sceneList = new List<object>();
            var pathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();

            for (int i = 0; i < buildScenes.Length; i++)
            {
                string path = buildScenes[i].path;
                bool enabled = buildScenes[i].enabled;

                if (!pathSet.Add(path))
                    duplicates.Add(path);

                sceneList.Add(new
                {
                    index = i,
                    path,
                    enabled
                });
            }

            var missing = new List<string>();
            foreach (string expected in expectedScenes)
            {
                string expectedPath = $"Assets/Scenes/{expected}.unity";
                if (!pathSet.Contains(expectedPath))
                    missing.Add(expected);
            }

            bool valid = missing.Count == 0 && duplicates.Count == 0;

            return new SuccessResponse(
                valid
                    ? $"Build Settings valid: {buildScenes.Length} scenes registered."
                    : $"Build Settings issues: {missing.Count} missing, {duplicates.Count} duplicates.",
                new
                {
                    valid,
                    scene_count = buildScenes.Length,
                    scenes = sceneList,
                    missing = missing.Count > 0 ? missing : null,
                    duplicates = duplicates.Count > 0 ? duplicates : null
                });
        }
    }
}
