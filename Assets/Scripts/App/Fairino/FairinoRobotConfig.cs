// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO FR5 JSON 설정 파일의 역직렬화 클래스입니다.
    /// </summary>
    [Serializable]
    public sealed class FairinoRobotConfig
    {
        public string robotId;
        public string displayName;
        public string defaultIp;
        public int defaultPort;
        public int dof;
        public DHParameterEntry[] dhParameters;
        public JointLimitEntry[] jointLimits;
        public SpeedPresetsBlock speedPresets;
        public ErrorMessageEntry[] errorMessages;
        public UiTabEntry[] uiTabs;
        public LiveDefaultsBlock liveDefaults;

        /// <summary>
        /// Resources/LearningTabs/ 경로에서 JSON 설정을 로드합니다.
        /// </summary>
        public static FairinoRobotConfig Load(string resourceName = "LearningTabs/FAIRINO_FR5")
        {
            var asset = Resources.Load<TextAsset>(resourceName);
            if (asset == null)
            {
                Debug.LogWarning($"[FairinoRobotConfig] 리소스를 찾을 수 없습니다: {resourceName}");
                return null;
            }

            var config = JsonUtility.FromJson<FairinoRobotConfig>(asset.text);
            if (config != null)
            {
                config.ParseErrorMessages(asset.text);
            }

            return config;
        }

        /// <summary>
        /// JSON 문자열에서 에러 메시지 매핑을 파싱합니다.
        /// JsonUtility는 Dictionary를 지원하지 않으므로 수동 파싱합니다.
        /// </summary>
        private void ParseErrorMessages(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            var wrapper = JsonUtility.FromJson<ErrorMessagesWrapper>(
                "{\"items\":" + ExtractJsonBlock(json, "errorMessages") + "}");

            // JsonUtility 한계로 Dictionary 파싱이 안되므로 배열 기반 대체 구조 사용
            // errorMessages 필드는 Load 전에 null-safe
        }

        private static string ExtractJsonBlock(string json, string key)
        {
            int start = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (start < 0) return "[]";

            int braceStart = json.IndexOf('{', start);
            if (braceStart < 0) return "[]";

            int depth = 0;
            for (int i = braceStart; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}') depth--;

                if (depth == 0)
                {
                    return json.Substring(braceStart, i - braceStart + 1);
                }
            }

            return "[]";
        }

        /// <summary>
        /// Medium 속도/가속 프리셋을 반환합니다. 설정이 없으면 기본값(30, 50)을 사용합니다.
        /// </summary>
        public (int speed, int acc) GetMediumSpeedAcc()
        {
            if (speedPresets?.medium != null)
            {
                return (speedPresets.medium.jointSpeedPercent, speedPresets.medium.accPercent);
            }

            return (30, 50);
        }

        /// <summary>
        /// 프리셋 이름(slow/medium/fast)에 해당하는 속도/가속을 반환합니다.
        /// </summary>
        public (int speed, int acc) GetSpeedAcc(string preset)
        {
            switch (preset)
            {
                case "slow":
                    return speedPresets?.slow != null
                        ? (speedPresets.slow.jointSpeedPercent, speedPresets.slow.accPercent)
                        : (10, 20);
                case "fast":
                    return speedPresets?.fast != null
                        ? (speedPresets.fast.jointSpeedPercent, speedPresets.fast.accPercent)
                        : (60, 80);
                default:
                    return GetMediumSpeedAcc();
            }
        }

        /// <summary>
        /// DH 파라미터 JSON 항목입니다.
        /// </summary>
        [Serializable]
        public sealed class DHParameterEntry
        {
            public double theta;
            public double d;
            public double a;
            public double alpha;
        }

        /// <summary>
        /// 관절 제한 JSON 항목입니다.
        /// </summary>
        [Serializable]
        public sealed class JointLimitEntry
        {
            public double minDeg;
            public double maxDeg;
        }

        /// <summary>
        /// 속도 프리셋 블록입니다.
        /// </summary>
        [Serializable]
        public sealed class SpeedPresetsBlock
        {
            public SpeedPreset slow;
            public SpeedPreset medium;
            public SpeedPreset fast;
        }

        /// <summary>
        /// 속도 프리셋 항목입니다.
        /// </summary>
        [Serializable]
        public sealed class SpeedPreset
        {
            public int jointSpeedPercent;
            public int accPercent;
        }

        /// <summary>
        /// 에러 메시지 JSON 항목입니다.
        /// </summary>
        [Serializable]
        public sealed class ErrorMessageEntry
        {
            public int code;
            public string message;
        }

        /// <summary>
        /// 에러 메시지 래퍼 (JsonUtility 파싱용)입니다.
        /// </summary>
        [Serializable]
        private sealed class ErrorMessagesWrapper
        {
            public ErrorMessageEntry[] items;
        }

        /// <summary>
        /// UI 탭 JSON 항목입니다.
        /// </summary>
        [Serializable]
        public sealed class UiTabEntry
        {
            public string id;
            public string label;
            public string icon;
        }

        /// <summary>
        /// Live 연결 기본 정책입니다.
        /// </summary>
        [Serializable]
        public sealed class LiveDefaultsBlock
        {
            public int realtimeSampleMs = 100;
            public bool reconnectEnabled = true;
            public int reconnectTimeoutMs = 30000;
            public int reconnectPeriodMs = 500;
        }
    }
}
