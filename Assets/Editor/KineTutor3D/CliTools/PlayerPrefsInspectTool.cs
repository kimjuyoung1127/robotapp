// Folder: Editor/CliTools - unity-cli 커스텀 도구: PlayerPrefs 상태 조회
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// KineTutor3D 관련 PlayerPrefs 키/값을 조회합니다.
    /// </summary>
    [UnityCliTool(Description = "Inspect KineTutor3D PlayerPrefs keys and values")]
    public static class PlayerPrefsInspectTool
    {
        private static readonly string[] knownKeys =
        {
            "KineTutor3D.HasVisited",
            "KineTutor3D.CurrentTrack",
            "KineTutor3D.SelectedRobotId",
            "KineTutor3D.SelectedMode",
            "KineTutor3D.SessionContextJson",
            "KineTutor3D.MathReadiness.LastCompletedStep",
            "KineTutor3D.PreKinematics.LastCompletedStep",
            "KineTutor3D.CoreKinematics.LastCompletedStep",
            "KineTutor3D.ReducedMotion"
        };

        public static object HandleCommand(JObject @params)
        {
            var entries = new List<object>();
            int setCount = 0;

            foreach (string key in knownKeys)
            {
                bool exists = PlayerPrefs.HasKey(key);
                string value = null;

                if (exists)
                {
                    setCount++;
                    // int 키 먼저 시도, 아니면 string
                    if (key.EndsWith("Step") || key == "KineTutor3D.HasVisited" || key == "KineTutor3D.ReducedMotion")
                        value = PlayerPrefs.GetInt(key, 0).ToString();
                    else
                        value = PlayerPrefs.GetString(key, "");
                }

                entries.Add(new
                {
                    key,
                    exists,
                    value
                });
            }

            // SessionContext JSON 파싱 시도
            string sessionJson = PlayerPrefs.GetString("KineTutor3D.SessionContextJson", "");
            object parsedSession = null;
            if (!string.IsNullOrEmpty(sessionJson))
            {
                try
                {
                    parsedSession = JObject.Parse(sessionJson);
                }
                catch
                {
                    parsedSession = "INVALID_JSON";
                }
            }

            return new SuccessResponse(
                $"PlayerPrefs: {setCount}/{knownKeys.Length} keys set.",
                new
                {
                    total_known_keys = knownKeys.Length,
                    keys_set = setCount,
                    entries,
                    session_context_parsed = parsedSession
                });
        }
    }
}
