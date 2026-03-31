// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 관절 각도 숫자 입력을 검증하고 표준 포맷으로 정리합니다.
    /// </summary>
    public static class JointInputValidator
    {
        public static bool TryParseDegrees(string raw, float min, float max, out float value, out string error)
        {
            value = 0f;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Joint angle is required.";
                return false;
            }

            var styles = NumberStyles.Float | NumberStyles.AllowLeadingSign;
            if (!float.TryParse(raw, styles, CultureInfo.CurrentCulture, out value) &&
                !float.TryParse(raw, styles, CultureInfo.InvariantCulture, out value))
            {
                error = "Enter a valid number.";
                return false;
            }

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                error = "Angle must be finite.";
                return false;
            }

            if (value < min || value > max)
            {
                error = $"Angle must stay between {FormatDegrees(min)} and {FormatDegrees(max)}.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static string FormatDegrees(float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}
