// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using UnityEngine;

namespace KineTutor3D.UI
{
    internal static class DHTableValueFormatter
    {
        public static bool TryParseFinite(string raw, out double value)
        {
            value = 0.0;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }

            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static string FormatDouble(double value, int digits = 4)
        {
            var safeDigits = Mathf.Clamp(digits, 0, 10);
            return value.ToString($"F{safeDigits}", CultureInfo.InvariantCulture);
        }
    }
}
