// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FR5 Live 클라이언트 생성 정책을 한 곳에 모읍니다.
    /// </summary>
    public static class FairinoRobotClientFactory
    {
        public const string BridgeUrlEnvironmentVariable = "FAIRINO_BRIDGE_URL";
        public const string ForceReadbackOnlyEnvironmentVariable = "FAIRINO_FORCE_READBACK_ONLY";
        public const string TinyMoveJLiveEnvironmentVariable = "FAIRINO_ENABLE_TINY_MOVEJ_LIVE";
        public const string LiveGripperSmokeEnvironmentVariable = "FAIRINO_ENABLE_LIVE_GRIPPER_SMOKE";

        public static IFairinoRobotClient CreateLive(FairinoErrorTranslator translator = null, bool preferMotionCapableDirect = false)
        {
            var bridgeUrl = Environment.GetEnvironmentVariable(BridgeUrlEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(bridgeUrl))
            {
                return new FairinoBridgeClient(bridgeUrl.Trim());
            }

            var report = FairinoSdkCompatibilityProbe.Probe();
            if (!report.IsDirectUsable)
            {
                return new FairinoUnavailableClient(report);
            }

            if (IsTruthy(Environment.GetEnvironmentVariable(ForceReadbackOnlyEnvironmentVariable)))
            {
                return new DirectReadbackFairinoClient(new LiveFairinoClient(translator), report);
            }

            // V3 live는 단일 motion-capable 세션을 기본으로 사용한다.
            // 지속 readback은 LiveFairinoClient 자체 polling으로 유지한다.
            return new LiveFairinoClient(translator);
        }

        public static bool IsTinyMoveJLiveEnabled()
        {
            return IsTruthy(Environment.GetEnvironmentVariable(TinyMoveJLiveEnvironmentVariable));
        }

        public static bool IsLiveGripperSmokeEnabled()
        {
            return IsTruthy(Environment.GetEnvironmentVariable(LiveGripperSmokeEnvironmentVariable));
        }

        private static bool IsTruthy(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "1":
                case "true":
                case "yes":
                case "on":
                    return true;
                default:
                    return false;
            }
        }
    }
}
