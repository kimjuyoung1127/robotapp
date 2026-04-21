// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App
{
    /// <summary>
    /// Pendant V3 셸의 로컬 UI 상태를 저장하는 직렬화 계약입니다.
    /// </summary>
    [Serializable]
    public struct PendantV3LocalState
    {
        public const string DefaultNavSection = "NavMotion";
        public const string DefaultWorkTab = "TabEasyMotion";
        public const string DefaultTabletTab = "BottomTabEasyMotion";
        public const string DefaultCoordSystem = "Base";
        public const int DefaultSpeedPercent = 30;
        public const int DefaultJogIncrement = 5;
        public const float DefaultSplitRatio = 0.20f;
        public const float MinSplitRatio = 0.14f;
        public const float MaxSplitRatio = 0.24f;

        public string ActiveNavSection;
        public string ActiveWorkTab;
        public string ActiveTabletTab;
        public string CoordSystem;
        public int SpeedPercent;
        public int JogIncrement;
        public float DesktopSplitRatio;
        public bool IsTabletSheetExpanded;
        public bool HasShownFirstRunGuide;

        public static PendantV3LocalState Default()
        {
            return new PendantV3LocalState
            {
                ActiveNavSection = DefaultNavSection,
                ActiveWorkTab = DefaultWorkTab,
                ActiveTabletTab = DefaultTabletTab,
                CoordSystem = DefaultCoordSystem,
                SpeedPercent = DefaultSpeedPercent,
                JogIncrement = DefaultJogIncrement,
                DesktopSplitRatio = DefaultSplitRatio,
                IsTabletSheetExpanded = true,
                HasShownFirstRunGuide = false,
            };
        }

        public static PendantV3LocalState Normalize(PendantV3LocalState state)
        {
            if (string.IsNullOrWhiteSpace(state.ActiveNavSection))
            {
                state.ActiveNavSection = DefaultNavSection;
            }

            if (string.IsNullOrWhiteSpace(state.ActiveWorkTab))
            {
                state.ActiveWorkTab = DefaultWorkTab;
            }

            if (string.IsNullOrWhiteSpace(state.ActiveTabletTab))
            {
                state.ActiveTabletTab = DefaultTabletTab;
            }

            state.CoordSystem = NormalizeCoordSystem(state.CoordSystem);
            state.SpeedPercent = Clamp(state.SpeedPercent, 1, 100, DefaultSpeedPercent);
            state.JogIncrement = NormalizeIncrement(state.JogIncrement);
            state.DesktopSplitRatio = Clamp(state.DesktopSplitRatio, MinSplitRatio, MaxSplitRatio, DefaultSplitRatio);
            return state;
        }

        public static PendantV3LocalState DeepCopy(PendantV3LocalState state)
        {
            return Normalize(new PendantV3LocalState
            {
                ActiveNavSection = state.ActiveNavSection,
                ActiveWorkTab = state.ActiveWorkTab,
                ActiveTabletTab = state.ActiveTabletTab,
                CoordSystem = state.CoordSystem,
                SpeedPercent = state.SpeedPercent,
                JogIncrement = state.JogIncrement,
                DesktopSplitRatio = state.DesktopSplitRatio,
                IsTabletSheetExpanded = state.IsTabletSheetExpanded,
                HasShownFirstRunGuide = state.HasShownFirstRunGuide,
            });
        }

        public string ToDebugSummary()
        {
            return $"nav={ActiveNavSection}; work={ActiveWorkTab}; tablet={ActiveTabletTab}; coord={CoordSystem}; speed={SpeedPercent}; increment={JogIncrement}; split={DesktopSplitRatio:F2}; sheetExpanded={IsTabletSheetExpanded}; firstRunGuide={HasShownFirstRunGuide}";
        }

        private static string NormalizeCoordSystem(string coordSystem)
        {
            return string.Equals(coordSystem, "Tool", StringComparison.Ordinal) ||
                string.Equals(coordSystem, "User", StringComparison.Ordinal)
                ? coordSystem
                : DefaultCoordSystem;
        }

        private static int NormalizeIncrement(int increment)
        {
            return increment == 1 || increment == 10
                ? increment
                : DefaultJogIncrement;
        }

        private static int Clamp(int value, int min, int max, int fallback)
        {
            if (value == 0)
            {
                return fallback;
            }

            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        private static float Clamp(float value, float min, float max, float fallback)
        {
            if (value <= 0f)
            {
                return fallback;
            }

            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
