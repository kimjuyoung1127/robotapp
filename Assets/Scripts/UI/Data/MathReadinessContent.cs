// Folder: UI - HUD/view data only; no kinematics logic.
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    public enum MathReadinessContent
    {
        None,
        AngleDirection,
        LengthAngleToPoint,
        DiagonalIntuition,
        TwoLinkComposition
    }

    /// <summary>
    /// MathReadinessContent 컨셉별 테마 색상 매핑.
    /// </summary>
    public static class MathReadinessContentTheme
    {
        private static readonly Color OrangeAccent = new Color(0.95f, 0.60f, 0.20f, 1f);
        private static readonly Color BlueAccent   = new Color(0.29f, 0.56f, 0.85f, 1f);
        private static readonly Color PurpleAccent = new Color(0.62f, 0.38f, 0.85f, 1f);
        private static readonly Color GreenAccent  = new Color(0.30f, 0.75f, 0.45f, 1f);

        /// <summary>컨셉에 해당하는 테마 accent 색상을 반환합니다.</summary>
        public static Color GetAccentColor(MathReadinessContent content)
        {
            switch (content)
            {
                case MathReadinessContent.AngleDirection:      return OrangeAccent;
                case MathReadinessContent.LengthAngleToPoint:  return BlueAccent;
                case MathReadinessContent.DiagonalIntuition:   return PurpleAccent;
                case MathReadinessContent.TwoLinkComposition:  return GreenAccent;
                default:                                       return BlueAccent;
            }
        }

        /// <summary>컨셉에 해당하는 stripe용 반투명 색상을 반환합니다.</summary>
        public static Color GetStripeColor(MathReadinessContent content)
        {
            var c = GetAccentColor(content);
            return new Color(c.r, c.g, c.b, 0.22f);
        }
    }
}
