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
        public const string DefaultPointName = "Point";
        public const string DefaultPointMotionKind = "MoveL";
        public const int DefaultSpeedPercent = 30;
        public const int DefaultJogIncrement = 5;
        public const float DefaultSplitRatio = 0.24f;
        public const float MinSplitRatio = 0.18f;
        public const float MaxSplitRatio = 0.60f;
        public const int PointAxisCount = 6;

        public string ActiveNavSection;
        public string ActiveWorkTab;
        public string ActiveTabletTab;
        public string CoordSystem;
        public int SpeedPercent;
        public int JogIncrement;
        public float DesktopSplitRatio;
        public bool IsTabletSheetExpanded;
        public string PointName;
        public string PointMotionKind;
        public float[] PointTcpDraftValues;
        public float[] PointJointDraftValues;
        public bool HasPointDraft;
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
                PointName = DefaultPointName,
                PointMotionKind = DefaultPointMotionKind,
                PointTcpDraftValues = CreateDefaultPointDraftValues(),
                PointJointDraftValues = CreateDefaultPointDraftValues(),
                HasPointDraft = false,
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
            state.PointName = NormalizePointName(state.PointName);
            state.PointMotionKind = NormalizePointMotionKind(state.PointMotionKind);
            state.PointTcpDraftValues = NormalizePointDraftValues(state.PointTcpDraftValues);
            state.PointJointDraftValues = NormalizePointDraftValues(state.PointJointDraftValues);
            return state;
        }

        public static PendantV3LocalState DeepCopy(PendantV3LocalState state)
        {
            state = Normalize(state);
            state.PointTcpDraftValues = ClonePointDraftValues(state.PointTcpDraftValues);
            state.PointJointDraftValues = ClonePointDraftValues(state.PointJointDraftValues);
            return state;
        }

        public static bool AreEquivalent(PendantV3LocalState left, PendantV3LocalState right)
        {
            left = Normalize(left);
            right = Normalize(right);
            return string.Equals(left.ActiveNavSection, right.ActiveNavSection, StringComparison.Ordinal)
                && string.Equals(left.ActiveWorkTab, right.ActiveWorkTab, StringComparison.Ordinal)
                && string.Equals(left.ActiveTabletTab, right.ActiveTabletTab, StringComparison.Ordinal)
                && string.Equals(left.CoordSystem, right.CoordSystem, StringComparison.Ordinal)
                && string.Equals(left.PointName, right.PointName, StringComparison.Ordinal)
                && string.Equals(left.PointMotionKind, right.PointMotionKind, StringComparison.Ordinal)
                && left.SpeedPercent == right.SpeedPercent
                && left.JogIncrement == right.JogIncrement
                && System.Math.Abs(left.DesktopSplitRatio - right.DesktopSplitRatio) < 0.0001f
                && left.IsTabletSheetExpanded == right.IsTabletSheetExpanded
                && left.HasPointDraft == right.HasPointDraft
                && left.HasShownFirstRunGuide == right.HasShownFirstRunGuide
                && ArePointDraftValuesEqual(left.PointTcpDraftValues, right.PointTcpDraftValues)
                && ArePointDraftValuesEqual(left.PointJointDraftValues, right.PointJointDraftValues);
        }

        public string ToDebugSummary()
        {
            return $"nav={ActiveNavSection}; work={ActiveWorkTab}; tablet={ActiveTabletTab}; coord={CoordSystem}; speed={SpeedPercent}; increment={JogIncrement}; split={DesktopSplitRatio:F2}; sheetExpanded={IsTabletSheetExpanded}; pointName={PointName}; pointMotion={PointMotionKind}; hasPointDraft={HasPointDraft}; firstRunGuide={HasShownFirstRunGuide}";
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

        private static string NormalizePointName(string pointName)
        {
            return string.IsNullOrWhiteSpace(pointName)
                ? DefaultPointName
                : pointName.Trim();
        }

        private static string NormalizePointMotionKind(string pointMotionKind)
        {
            return string.Equals(pointMotionKind, "MoveJ", StringComparison.Ordinal)
                ? "MoveJ"
                : DefaultPointMotionKind;
        }

        private static float[] NormalizePointDraftValues(float[] values)
        {
            var normalized = CreateDefaultPointDraftValues();
            if (values == null)
            {
                return normalized;
            }

            var count = System.Math.Min(values.Length, PointAxisCount);
            for (var index = 0; index < count; index++)
            {
                var value = values[index];
                normalized[index] = float.IsNaN(value) || float.IsInfinity(value)
                    ? 0f
                    : value;
            }

            return normalized;
        }

        private static bool ArePointDraftValuesEqual(float[] left, float[] right)
        {
            left = NormalizePointDraftValues(left);
            right = NormalizePointDraftValues(right);
            for (var index = 0; index < PointAxisCount; index++)
            {
                if (System.Math.Abs(left[index] - right[index]) >= 0.0001f)
                {
                    return false;
                }
            }

            return true;
        }

        private static float[] ClonePointDraftValues(float[] values)
        {
            var source = NormalizePointDraftValues(values);
            var clone = new float[PointAxisCount];
            Array.Copy(source, clone, PointAxisCount);
            return clone;
        }

        private static float[] CreateDefaultPointDraftValues()
        {
            return new float[PointAxisCount];
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
