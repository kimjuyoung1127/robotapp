// Folder: UI - HUD/view components only; no kinematics logic.
using TMPro;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 타이포그래피 프리셋 열거형.
    /// </summary>
    public enum TypographyPreset
    {
        DisplayLg,
        DisplaySm,
        HeadingLg,
        HeadingSm,
        Body,
        Caption,
        Tiny
    }

    /// <summary>
    /// TMP 폰트 해석 및 프리셋 적용 유틸리티.
    /// Legacy Text 기반 기존 코드와 공존하면서 점진적 TMP 전환을 지원합니다.
    /// </summary>
    public static class UITypography
    {
        /// <summary>
        /// preferred가 null이면 TMP 기본 폰트(Settings)를 반환합니다.
        /// </summary>
        public static TMP_FontAsset ResolveFont(TMP_FontAsset preferred)
        {
            if (preferred != null)
            {
                return preferred;
            }

            var settings = TMP_Settings.defaultFontAsset;
            if (settings != null)
            {
                return settings;
            }

            return Resources.Load<TMP_FontAsset>("Fonts/DefaultTMP");
        }

        /// <summary>
        /// TextMeshProUGUI에 프리셋을 적용합니다.
        /// </summary>
        public static void ApplyPreset(TextMeshProUGUI text, TypographyPreset preset, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = GetFontSize(preset);
            text.fontStyle = GetTmpStyle(preset);
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
        }

        /// <summary>
        /// 프리셋에 해당하는 폰트 크기를 반환합니다.
        /// </summary>
        public static int GetFontSize(TypographyPreset preset)
        {
            switch (preset)
            {
                case TypographyPreset.DisplayLg: return UIDesignTokens.Type.DisplayLg;
                case TypographyPreset.DisplaySm: return UIDesignTokens.Type.DisplaySm;
                case TypographyPreset.HeadingLg: return UIDesignTokens.Type.HeadingLg;
                case TypographyPreset.HeadingSm: return UIDesignTokens.Type.HeadingSm;
                case TypographyPreset.Body:      return UIDesignTokens.Type.Body;
                case TypographyPreset.Caption:   return UIDesignTokens.Type.Caption;
                case TypographyPreset.Tiny:      return UIDesignTokens.Type.Tiny;
                default:                         return UIDesignTokens.Type.Body;
            }
        }

        /// <summary>
        /// 프리셋에 해당하는 Legacy FontStyle을 반환합니다.
        /// </summary>
        public static FontStyle GetLegacyStyle(TypographyPreset preset)
        {
            switch (preset)
            {
                case TypographyPreset.DisplayLg:
                case TypographyPreset.DisplaySm:
                case TypographyPreset.HeadingLg:
                case TypographyPreset.HeadingSm:
                    return FontStyle.Bold;
                default:
                    return FontStyle.Normal;
            }
        }

        /// <summary>
        /// Legacy Text 컴포넌트에 프리셋을 적용합니다.
        /// </summary>
        public static void ApplyPresetLegacy(UnityEngine.UI.Text text, TypographyPreset preset, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = GetFontSize(preset);
            text.fontStyle = GetLegacyStyle(preset);
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
        }

        private static FontStyles GetTmpStyle(TypographyPreset preset)
        {
            switch (preset)
            {
                case TypographyPreset.DisplayLg:
                case TypographyPreset.DisplaySm:
                case TypographyPreset.HeadingLg:
                case TypographyPreset.HeadingSm:
                    return FontStyles.Bold;
                default:
                    return FontStyles.Normal;
            }
        }
    }
}
