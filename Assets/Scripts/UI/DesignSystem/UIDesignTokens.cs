// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 모든 시각 상수의 단일 진입점. 색상, 타이포그래피, 간격, 크기, 애니메이션 토큰을 정의합니다.
    /// 새 UI 코드는 매직넘버 대신 이 토큰을 참조해야 합니다.
    /// </summary>
    public static class UIDesignTokens
    {
        // ── Colors ───────────────────────────────────────────────────────

        /// <summary>시맨틱 색상 토큰.</summary>
        public static class Colors
        {
            // Surface
            public static readonly Color SurfaceBase       = new Color(0.08f, 0.09f, 0.14f, 1f);
            public static readonly Color SurfaceRaised     = new Color(0.10f, 0.11f, 0.17f, 0.92f);
            public static readonly Color SurfaceRaisedAlt  = new Color(0.13f, 0.14f, 0.22f, 0.94f);
            public static readonly Color SurfaceCard       = new Color(0.16f, 0.18f, 0.28f, 0.95f);
            public static readonly Color SurfaceOverlay    = new Color(0.08f, 0.09f, 0.14f, 0.85f);
            public static readonly Color SurfaceInput      = new Color(0.12f, 0.13f, 0.20f, 0.95f);

            // Accent
            public static readonly Color AccentPrimary     = new Color(0.29f, 0.56f, 0.85f, 1f);
            public static readonly Color AccentSecondary   = new Color(0.95f, 0.77f, 0.15f, 1f);
            public static readonly Color AccentSuccess     = new Color(0.30f, 0.85f, 0.45f, 1f);
            public static readonly Color AccentDanger      = new Color(0.90f, 0.35f, 0.30f, 1f);
            public static readonly Color AccentWarning     = new Color(0.95f, 0.65f, 0.20f, 1f);

            // Text
            public static readonly Color TextPrimary       = new Color(0.92f, 0.93f, 0.96f, 1f);
            public static readonly Color TextSecondary     = new Color(0.72f, 0.76f, 0.84f, 1f);
            public static readonly Color TextMuted         = new Color(0.55f, 0.60f, 0.72f, 1f);
            public static readonly Color TextOnAccent      = new Color(1f, 1f, 1f, 1f);

            // Border
            public static readonly Color BorderSoft        = new Color(0.29f, 0.56f, 0.85f, 0.18f);
            public static readonly Color BorderFocus       = new Color(0.29f, 0.56f, 0.85f, 0.55f);

            // Difficulty
            public static readonly Color DifficultyEasy    = new Color(0.30f, 0.85f, 0.45f, 1f);
            public static readonly Color DifficultyMedium  = new Color(0.95f, 0.77f, 0.15f, 1f);
            public static readonly Color DifficultyHard    = new Color(0.90f, 0.35f, 0.30f, 1f);

            // Slider
            public static readonly Color SliderTrack       = new Color(0.20f, 0.22f, 0.30f, 1f);
            public static readonly Color SliderFill        = new Color(0.29f, 0.56f, 0.85f, 1f);
            public static readonly Color SliderHandle      = new Color(0.92f, 0.93f, 0.96f, 1f);

            // Toast
            public static readonly Color ToastInfo         = new Color(0.29f, 0.56f, 0.85f, 0.92f);
            public static readonly Color ToastSuccess      = new Color(0.20f, 0.55f, 0.30f, 0.92f);
            public static readonly Color ToastWarning      = new Color(0.60f, 0.45f, 0.10f, 0.92f);

            // Scene navigation
            public static readonly Color NavCurrentScene   = new Color(0.23f, 0.27f, 0.40f, 0.95f);

            // Danger muted (disabled/placeholder)
            public static readonly Color DangerMuted       = new Color(0.42f, 0.18f, 0.15f, 0.92f);

            // Showroom
            public static readonly Color PedestalSurface   = new Color(0.14f, 0.16f, 0.24f, 0.95f);
            public static readonly Color PreviewPlaceholder = new Color(0.40f, 0.42f, 0.48f, 0.80f);

            // Scene overlay
            public static readonly Color SceneOverlayLight = new Color(0.03f, 0.04f, 0.08f, 0.12f);
            public static readonly Color TopBarBackground  = new Color(0.05f, 0.06f, 0.10f, 0.92f);

            // Text shadow / outline
            public static readonly Color TextShadow      = new Color(0f, 0f, 0f, 0.28f);

            // MathReadiness concept themes
            public static readonly Color ConceptOrange   = new Color(0.95f, 0.60f, 0.20f, 1f);
            public static readonly Color ConceptBlue     = new Color(0.29f, 0.56f, 0.85f, 1f);
            public static readonly Color ConceptPurple   = new Color(0.62f, 0.38f, 0.85f, 1f);
            public static readonly Color ConceptGreen    = new Color(0.30f, 0.75f, 0.45f, 1f);

            // FK Diagram
            public static readonly Color DiagramLink1      = new Color(0.29f, 0.56f, 0.85f, 1f);
            public static readonly Color DiagramLink2      = new Color(0.95f, 0.77f, 0.15f, 1f);
            public static readonly Color DiagramLink3      = new Color(0.30f, 0.85f, 0.45f, 1f);
            public static readonly Color DiagramLink4      = new Color(0.90f, 0.35f, 0.30f, 1f);
            public static readonly Color DiagramLink5      = new Color(0.70f, 0.40f, 0.90f, 1f);
            public static readonly Color DiagramLink6      = new Color(0.95f, 0.55f, 0.25f, 1f);
            public static readonly Color DiagramJoint      = new Color(0.92f, 0.93f, 0.96f, 1f);
            public static readonly Color DiagramGrid       = new Color(0.20f, 0.22f, 0.30f, 0.40f);
            public static readonly Color DiagramAxis       = new Color(0.55f, 0.60f, 0.72f, 0.70f);
            public static readonly Color DiagramEE         = new Color(0.30f, 0.85f, 0.45f, 1f);

            // Axis triplet
            public static readonly Color AxisX             = new Color(0.90f, 0.30f, 0.25f, 1f);
            public static readonly Color AxisY             = new Color(0.30f, 0.85f, 0.35f, 1f);
            public static readonly Color AxisZ             = new Color(0.29f, 0.56f, 0.85f, 1f);
        }

        // ── Typography ───────────────────────────────────────────────────

        /// <summary>폰트 크기 7단계 modular scale.</summary>
        public static class Type
        {
            public const int DisplayLg  = 28;
            public const int DisplaySm  = 22;
            public const int HeadingLg  = 18;
            public const int HeadingSm  = 16;
            public const int Body       = 14;
            public const int Caption    = 12;
            public const int Tiny       = 10;
        }

        // ── Spacing ──────────────────────────────────────────────────────

        /// <summary>간격 7단계, 4px base.</summary>
        public static class Space
        {
            public const float Xxs = 4f;
            public const float Xs  = 8f;
            public const float Sm  = 12f;
            public const float Md  = 16f;
            public const float Lg  = 24f;
            public const float Xl  = 32f;
            public const float Xxl = 48f;
        }

        // ── Component Sizes ──────────────────────────────────────────────

        /// <summary>컴포넌트 치수 상수.</summary>
        public static class Size
        {
            // Touch / interaction
            public const float TouchTargetMin   = 44f;

            // Buttons
            public const float ButtonHeightSm   = 28f;
            public const float ButtonHeightMd   = 36f;
            public const float ButtonHeightLg   = 44f;

            // Layout regions
            public const float TopBarHeight     = 60f;
            public const float LeftPanelWidth   = 356f;
            public const float RightPanelWidth  = 400f;

            // Cards
            public const float CardWidth        = 280f;
            public const float CardHeight       = 220f;

            // Controls
            public const float SliderHeight     = 28f;
            public const float InputFieldWidth  = 72f;

            // Icons
            public const float IconSm           = 16f;
            public const float IconMd           = 24f;
            public const float IconLg           = 32f;

            // Radius
            public const float RadiusSm         = 4f;
            public const float RadiusMd         = 8f;
            public const float RadiusLg         = 12f;

            // Modal
            public const float ModalWidth       = 720f;
            public const float ModalHeight      = 420f;

            // Grid
            public const float GridSpacing      = 20f;

            // FK Diagram
            public const int DiagramResolution  = 512;

            // Showroom
            public const float PedestalRadius         = 0.35f;
            public const float PedestalHeight         = 0.02f;
            public const float PodSpacing             = 1.2f;
            public const float ShowroomViewportRatio  = 0.55f;

            // Badge
            public const float BadgeWidth       = 70f;
            public const float BadgeHeight      = 22f;
        }

        // ── Animation ────────────────────────────────────────────────────

        /// <summary>전환 시간 상수 (초).</summary>
        public static class Anim
        {
            public const float FadeFast   = 0.15f;
            public const float FadeNormal = 0.25f;
            public const float SlideIn    = 0.30f;
            public const float PresetTransition = 1.5f;
            public const float ConnectionSync   = 0.8f;
        }

        // ── RobotControl V2 Theme ──────────────────────────────────────

        /// <summary>
        /// RobotControlV2 전용 theme 토큰.
        /// authored-first 셸과 상단 상태 바가 동일한 팔레트를 공유할 때 사용합니다.
        /// </summary>
        public static class RobotControlV2
        {
            public static class Type
            {
                public const int UniformText = 15;
            }

            public static class Colors
            {
                public static readonly Color Backdrop         = new Color(0.08f, 0.09f, 0.11f, 1f);
                public static readonly Color SafeArea         = new Color(0.10f, 0.11f, 0.14f, 1f);
                public static readonly Color LeftRail         = new Color(0.13f, 0.14f, 0.17f, 0.98f);
                public static readonly Color CenterViewport   = new Color(0.10f, 0.11f, 0.13f, 0.96f);
                public static readonly Color RightRail        = new Color(0.13f, 0.14f, 0.17f, 0.98f);
                public static readonly Color BottomSheet      = new Color(0.16f, 0.17f, 0.20f, 0.98f);
                public static readonly Color Card            = new Color(0.18f, 0.19f, 0.23f, 0.98f);
                public static readonly Color CardAlt         = new Color(0.15f, 0.16f, 0.19f, 0.98f);
                public static readonly Color SurfaceStroke   = new Color(0.36f, 0.33f, 0.30f, 1f);

                public static readonly Color Accent          = new Color(0.78f, 0.64f, 0.49f, 1f);
                public static readonly Color AccentSoft      = new Color(0.29f, 0.24f, 0.20f, 1f);
                public static readonly Color Success         = new Color(0.38f, 0.68f, 0.58f, 1f);
                public static readonly Color Warning         = new Color(0.84f, 0.67f, 0.37f, 1f);
                public static readonly Color Danger          = new Color(0.76f, 0.44f, 0.39f, 1f);

                public static readonly Color TitleText       = new Color(0.95f, 0.92f, 0.88f, 1f);
                public static readonly Color MutedText       = new Color(0.80f, 0.75f, 0.69f, 1f);
                public static readonly Color Border          = new Color(0.33f, 0.30f, 0.27f, 1f);

                public static readonly Color TopBarBackground = new Color(0.12f, 0.13f, 0.16f, 0.98f);
                public static readonly Color TopBarSecondary  = new Color(0.19f, 0.20f, 0.24f, 1f);
            }

            public static class Size
            {
                public const float LeftRailWidth = 360f;
                public const float RightRailWidth = 360f;
                public const float BottomSheetHeight = 240f;
                public const float StatusBarHeight = 72f;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// 주어진 normal 색상으로 버튼 ColorBlock을 생성합니다.
        /// AccentPrimary와 블렌딩하여 highlighted/pressed 상태를 자동 생성합니다.
        /// </summary>
        public static ColorBlock ButtonColors(Color normal)
        {
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor      = normal;
            cb.highlightedColor = Color.Lerp(normal, Colors.AccentPrimary, 0.25f);
            cb.pressedColor     = Color.Lerp(normal, Colors.AccentPrimary, 0.45f);
            cb.disabledColor    = new Color(normal.r, normal.g, normal.b, 0.35f);
            cb.selectedColor    = cb.highlightedColor;
            return cb;
        }

        /// <summary>
        /// difficulty 문자열에 해당하는 색상을 반환합니다.
        /// </summary>
        public static Color GetDifficultyColor(string difficulty)
        {
            switch (difficulty)
            {
                case "Easy":   return Colors.DifficultyEasy;
                case "Hard":   return Colors.DifficultyHard;
                default:       return Colors.DifficultyMedium;
            }
        }
    }
}
