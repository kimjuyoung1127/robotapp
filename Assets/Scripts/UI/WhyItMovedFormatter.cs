// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.Math;

namespace KineTutor3D.UI
{
    /// <summary>
    /// WhyItMovedState를 사용자 표시용 문자열로 변환하는 정적 유틸리티입니다.
    /// </summary>
    public static class WhyItMovedFormatter
    {
        private const string Fmt = "F3";

        /// <summary>
        /// 관절 변화를 평문 한국어 설명으로 변환합니다.
        /// </summary>
        public static string FormatPlainLanguage(WhyItMovedState state)
        {
            if (state == null || !state.IsMeaningfulChange)
            {
                return string.Empty;
            }

            if (state.UpdateCause != RuntimeUpdateCause.JointAngleChange)
            {
                return string.Empty;
            }

            var jointLabel = $"관절{state.ChangedJointIndex + 1}";
            var absDelta = System.Math.Abs(state.DeltaDeg);
            var direction = state.DeltaDeg > 0 ? "양(+) 방향" : "음(-) 방향";
            var dist = state.EEDistanceMoved.ToString(Fmt, CultureInfo.InvariantCulture);
            var eeDirection = DescribeEEDirection(state.EEDisplacement);

            return $"{jointLabel}을(를) {absDelta.ToString(Fmt, CultureInfo.InvariantCulture)}도 {direction}으로 돌리니 끝점이 {eeDirection} {dist}m 움직였어요.";
        }

        /// <summary>
        /// 관절 변화량 텍스트를 반환합니다 (예: "+15.000 deg").
        /// </summary>
        public static string FormatDeltaText(double deltaDeg)
        {
            var sign = deltaDeg >= 0 ? "+" : "";
            return $"{sign}{deltaDeg.ToString(Fmt, CultureInfo.InvariantCulture)} deg";
        }

        /// <summary>
        /// 끝점 변위를 x/y/z 형식으로 반환합니다.
        /// </summary>
        public static string FormatEEChange(Vec3D displacement)
        {
            return $"x: {FormatComponent(displacement.X)}  y: {FormatComponent(displacement.Y)}  z: {FormatComponent(displacement.Z)}";
        }

        /// <summary>
        /// 관절 이전/현재 각도를 라디안에서 도 단위 문자열로 반환합니다.
        /// </summary>
        public static string FormatAngleTransition(double prevRad, double currRad)
        {
            var prevDeg = prevRad * (180.0 / System.Math.PI);
            var currDeg = currRad * (180.0 / System.Math.PI);
            return $"{prevDeg.ToString(Fmt, CultureInfo.InvariantCulture)} -> {currDeg.ToString(Fmt, CultureInfo.InvariantCulture)} deg";
        }

        /// <summary>
        /// 영향받는 링크 이름을 쉼표로 결합합니다.
        /// </summary>
        public static string FormatAffectedLinks(string[] linkNames)
        {
            if (linkNames == null || linkNames.Length == 0)
            {
                return "-";
            }

            return string.Join(", ", linkNames);
        }

        /// <summary>
        /// 변화량이 양이면 true를 반환합니다 (색상 구분용).
        /// </summary>
        public static bool IsDeltaPositive(double deltaDeg)
        {
            return deltaDeg >= 0;
        }

        private static string FormatComponent(double v)
        {
            var sign = v >= 0 ? "+" : "";
            return $"{sign}{v.ToString(Fmt, CultureInfo.InvariantCulture)}";
        }

        private static string DescribeEEDirection(Vec3D displacement)
        {
            var absX = System.Math.Abs(displacement.X);
            var absY = System.Math.Abs(displacement.Y);
            var absZ = System.Math.Abs(displacement.Z);

            if (absX < 1e-6 && absY < 1e-6 && absZ < 1e-6)
            {
                return "거의 같은 자리에서";
            }

            var parts = new System.Collections.Generic.List<string>();
            if (absX > 1e-4)
            {
                parts.Add(displacement.X > 0 ? "오른쪽" : "왼쪽");
            }
            if (absY > 1e-4)
            {
                parts.Add(displacement.Y > 0 ? "위" : "아래");
            }
            if (absZ > 1e-4)
            {
                parts.Add(displacement.Z > 0 ? "앞" : "뒤");
            }

            return parts.Count > 0 ? string.Join(" ", parts) + "으로" : "거의 같은 자리에서";
        }
    }
}
