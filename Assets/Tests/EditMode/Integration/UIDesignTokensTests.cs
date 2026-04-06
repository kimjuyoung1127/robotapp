// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// UIDesignTokens 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// UIDesignTokens 토큰 값 유효성을 검증합니다.
    /// </summary>
    public class UIDesignTokensTests
    {
        // ── Color Alpha ──────────────────────────────────────────────────

        [Test]
        public void Colors_AccentPrimary_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.AccentPrimary.a, 0f);
        }

        [Test]
        public void Colors_AccentSecondary_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.AccentSecondary.a, 0f);
        }

        [Test]
        public void Colors_AccentSuccess_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.AccentSuccess.a, 0f);
        }

        [Test]
        public void Colors_AccentDanger_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.AccentDanger.a, 0f);
        }

        [Test]
        public void Colors_TextPrimary_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.TextPrimary.a, 0f);
        }

        [Test]
        public void Colors_SurfaceRaised_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.SurfaceRaised.a, 0f);
        }

        [Test]
        public void Colors_SurfaceCard_AlphaPositive()
        {
            Assert.Greater(UIDesignTokens.Colors.SurfaceCard.a, 0f);
        }

        // ── Difficulty Colors ────────────────────────────────────────────

        [Test]
        public void GetDifficultyColor_Easy_ReturnsGreen()
        {
            Assert.AreEqual(UIDesignTokens.Colors.DifficultyEasy, UIDesignTokens.GetDifficultyColor("Easy"));
        }

        [Test]
        public void GetDifficultyColor_Hard_ReturnsRed()
        {
            Assert.AreEqual(UIDesignTokens.Colors.DifficultyHard, UIDesignTokens.GetDifficultyColor("Hard"));
        }

        [Test]
        public void GetDifficultyColor_Medium_ReturnsYellow()
        {
            Assert.AreEqual(UIDesignTokens.Colors.DifficultyMedium, UIDesignTokens.GetDifficultyColor("Medium"));
        }

        [Test]
        public void GetDifficultyColor_Unknown_DefaultsToMedium()
        {
            Assert.AreEqual(UIDesignTokens.Colors.DifficultyMedium, UIDesignTokens.GetDifficultyColor("Unknown"));
        }

        // ── Type Scale Monotonic ─────────────────────────────────────────

        [Test]
        public void Type_DisplayLg_GreaterThan_DisplaySm()
        {
            Assert.Greater(UIDesignTokens.Type.DisplayLg, UIDesignTokens.Type.DisplaySm);
        }

        [Test]
        public void Type_DisplaySm_GreaterThan_HeadingLg()
        {
            Assert.Greater(UIDesignTokens.Type.DisplaySm, UIDesignTokens.Type.HeadingLg);
        }

        [Test]
        public void Type_HeadingLg_GreaterThan_HeadingSm()
        {
            Assert.Greater(UIDesignTokens.Type.HeadingLg, UIDesignTokens.Type.HeadingSm);
        }

        [Test]
        public void Type_HeadingSm_GreaterThan_Body()
        {
            Assert.Greater(UIDesignTokens.Type.HeadingSm, UIDesignTokens.Type.Body);
        }

        [Test]
        public void Type_Body_GreaterThan_Caption()
        {
            Assert.Greater(UIDesignTokens.Type.Body, UIDesignTokens.Type.Caption);
        }

        [Test]
        public void Type_Caption_GreaterThan_Tiny()
        {
            Assert.Greater(UIDesignTokens.Type.Caption, UIDesignTokens.Type.Tiny);
        }

        [Test]
        public void Type_Tiny_Positive()
        {
            Assert.Greater(UIDesignTokens.Type.Tiny, 0);
        }

        // ── Space Scale Monotonic ────────────────────────────────────────

        [Test]
        public void Space_Xxl_GreaterThan_Xl()
        {
            Assert.Greater(UIDesignTokens.Space.Xxl, UIDesignTokens.Space.Xl);
        }

        [Test]
        public void Space_Xl_GreaterThan_Lg()
        {
            Assert.Greater(UIDesignTokens.Space.Xl, UIDesignTokens.Space.Lg);
        }

        [Test]
        public void Space_Lg_GreaterThan_Md()
        {
            Assert.Greater(UIDesignTokens.Space.Lg, UIDesignTokens.Space.Md);
        }

        [Test]
        public void Space_Md_GreaterThan_Sm()
        {
            Assert.Greater(UIDesignTokens.Space.Md, UIDesignTokens.Space.Sm);
        }

        [Test]
        public void Space_Sm_GreaterThan_Xs()
        {
            Assert.Greater(UIDesignTokens.Space.Sm, UIDesignTokens.Space.Xs);
        }

        [Test]
        public void Space_Xs_GreaterThan_Xxs()
        {
            Assert.Greater(UIDesignTokens.Space.Xs, UIDesignTokens.Space.Xxs);
        }

        [Test]
        public void Space_Xxs_Positive()
        {
            Assert.Greater(UIDesignTokens.Space.Xxs, 0f);
        }

        // ── Size Constraints ─────────────────────────────────────────────

        [Test]
        public void Size_TouchTargetMin_AtLeast44()
        {
            Assert.GreaterOrEqual(UIDesignTokens.Size.TouchTargetMin, 44f);
        }

        [Test]
        public void Size_ButtonHeightLg_MeetsTouchTarget()
        {
            Assert.GreaterOrEqual(UIDesignTokens.Size.ButtonHeightLg, UIDesignTokens.Size.TouchTargetMin);
        }

        [Test]
        public void Size_ButtonHeightMd_GreaterThan_ButtonHeightSm()
        {
            Assert.Greater(UIDesignTokens.Size.ButtonHeightMd, UIDesignTokens.Size.ButtonHeightSm);
        }

        [Test]
        public void Size_ButtonHeightLg_GreaterThan_ButtonHeightMd()
        {
            Assert.Greater(UIDesignTokens.Size.ButtonHeightLg, UIDesignTokens.Size.ButtonHeightMd);
        }

        [Test]
        public void Size_IconSizes_Monotonic()
        {
            Assert.Greater(UIDesignTokens.Size.IconLg, UIDesignTokens.Size.IconMd);
            Assert.Greater(UIDesignTokens.Size.IconMd, UIDesignTokens.Size.IconSm);
            Assert.Greater(UIDesignTokens.Size.IconSm, 0f);
        }

        // ── ButtonColors Helper ──────────────────────────────────────────

        [Test]
        public void ButtonColors_NormalColor_MatchesInput()
        {
            var input = UIDesignTokens.Colors.AccentPrimary;
            var cb = UIDesignTokens.ButtonColors(input);
            Assert.AreEqual(input, cb.normalColor);
        }

        [Test]
        public void ButtonColors_DisabledColor_HasReducedAlpha()
        {
            var input = UIDesignTokens.Colors.AccentPrimary;
            var cb = UIDesignTokens.ButtonColors(input);
            Assert.Less(cb.disabledColor.a, input.a);
        }

        // ── Anim ─────────────────────────────────────────────────────────

        [Test]
        public void Anim_AllPositive()
        {
            Assert.Greater(UIDesignTokens.Anim.FadeFast, 0f);
            Assert.Greater(UIDesignTokens.Anim.FadeNormal, 0f);
            Assert.Greater(UIDesignTokens.Anim.SlideIn, 0f);
        }

        [Test]
        public void Anim_FadeFast_LessThan_FadeNormal()
        {
            Assert.Less(UIDesignTokens.Anim.FadeFast, UIDesignTokens.Anim.FadeNormal);
        }

        // ── Typography Preset Mapping ────────────────────────────────────

        [Test]
        public void Typography_GetFontSize_ReturnsTokenValues()
        {
            Assert.AreEqual(UIDesignTokens.Type.DisplayLg, UITypography.GetFontSize(TypographyPreset.DisplayLg));
            Assert.AreEqual(UIDesignTokens.Type.Body, UITypography.GetFontSize(TypographyPreset.Body));
            Assert.AreEqual(UIDesignTokens.Type.Tiny, UITypography.GetFontSize(TypographyPreset.Tiny));
        }

        [Test]
        public void Typography_GetLegacyStyle_HeadingsAreBold()
        {
            Assert.AreEqual(FontStyle.Bold, UITypography.GetLegacyStyle(TypographyPreset.DisplayLg));
            Assert.AreEqual(FontStyle.Bold, UITypography.GetLegacyStyle(TypographyPreset.HeadingLg));
        }

        [Test]
        public void Typography_GetLegacyStyle_BodyIsNormal()
        {
            Assert.AreEqual(FontStyle.Normal, UITypography.GetLegacyStyle(TypographyPreset.Body));
            Assert.AreEqual(FontStyle.Normal, UITypography.GetLegacyStyle(TypographyPreset.Caption));
        }

        [Test]
        public void RobotControlV2_UniformText_Is15()
        {
            Assert.AreEqual(15, UIDesignTokens.RobotControlV2.Type.UniformText);
        }
    }
}
