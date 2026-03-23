// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App
{
    /// <summary>
    /// FR5 슬림 템플릿 데모와 증거 캡처에서 공용으로 사용하는 대표 포즈 카탈로그입니다.
    /// </summary>
    public static class FR5TemplatePoseCatalog
    {
        public const string NeutralName = "Neutral";
        public const string ReadyName = "Ready";
        public const string ShowcaseName = "Showcase";
        public const string WristTurnName = "WristTurn";

        private static readonly string[] PoseNames =
        {
            NeutralName,
            ReadyName,
            ShowcaseName,
            WristTurnName
        };

        private static readonly double[] NeutralPose = { 0d, 0d, 0d, 0d, 0d, 0d };
        private static readonly double[] ReadyPose = { 0d, -45d, 0d, -59d, -92d, -42d };
        private static readonly double[] ShowcasePose = { 30d, -55d, 45d, -70d, -70d, 15d };
        private static readonly double[] WristTurnPose = { 30d, -55d, 45d, -70d, -70d, 120d };

        /// <summary>
        /// 지원하는 포즈 이름 목록을 반환합니다.
        /// </summary>
        public static string[] GetNames()
        {
            return (string[])PoseNames.Clone();
        }

        /// <summary>
        /// 포즈 이름에 대응하는 관절 각도 배열을 반환합니다.
        /// </summary>
        public static bool TryGetPose(string poseName, out double[] jointAnglesDeg)
        {
            switch (poseName)
            {
                case NeutralName:
                    jointAnglesDeg = (double[])NeutralPose.Clone();
                    return true;
                case ReadyName:
                    jointAnglesDeg = (double[])ReadyPose.Clone();
                    return true;
                case ShowcaseName:
                    jointAnglesDeg = (double[])ShowcasePose.Clone();
                    return true;
                case WristTurnName:
                    jointAnglesDeg = (double[])WristTurnPose.Clone();
                    return true;
                default:
                    jointAnglesDeg = Array.Empty<double>();
                    return false;
            }
        }
    }
}
