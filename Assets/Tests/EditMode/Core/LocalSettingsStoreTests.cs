// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// Pendant V3 로컬 상태 저장 계약을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class LocalSettingsStoreTests
    {
        [SetUp]
        public void SetUp()
        {
            LocalSettingsStore.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            LocalSettingsStore.Clear();
        }

        [Test]
        public void SaveLoad_Roundtrip()
        {
            LocalSettingsStore.Save(new PendantV3LocalState
            {
                ActiveNavSection = "NavStatus",
                ActiveWorkTab = "TabTcpJog",
                ActiveTabletTab = "BottomTabStatus",
                CoordSystem = "Tool",
                SpeedPercent = 67,
                JogIncrement = 10,
                DesktopSplitRatio = 0.62f,
                IsTabletSheetExpanded = false,
                PointName = "Pick-01",
                PointMotionKind = "MoveJ",
                PointTcpDraftValues = new[] { -497f, -130f, 477f, 180f, 0f, 90f },
                PointJointDraftValues = new[] { 0f, -32f, 84f, 0f, 90f, 0f },
                HasPointDraft = true,
                HasShownFirstRunGuide = true,
            });

            Assert.That(LocalSettingsStore.TryLoad(out var state), Is.True);
            Assert.That(state.ActiveNavSection, Is.EqualTo("NavStatus"));
            Assert.That(state.ActiveWorkTab, Is.EqualTo("TabTcpJog"));
            Assert.That(state.ActiveTabletTab, Is.EqualTo("BottomTabStatus"));
            Assert.That(state.CoordSystem, Is.EqualTo("Tool"));
            Assert.That(state.SpeedPercent, Is.EqualTo(67));
            Assert.That(state.JogIncrement, Is.EqualTo(10));
            Assert.That(state.DesktopSplitRatio, Is.EqualTo(0.62f).Within(0.0001f));
            Assert.That(state.IsTabletSheetExpanded, Is.False);
            Assert.That(state.PointName, Is.EqualTo("Pick-01"));
            Assert.That(state.PointMotionKind, Is.EqualTo("MoveJ"));
            Assert.That(state.PointTcpDraftValues[0], Is.EqualTo(-497f).Within(0.0001f));
            Assert.That(state.PointJointDraftValues[1], Is.EqualTo(-32f).Within(0.0001f));
            Assert.That(state.HasPointDraft, Is.True);
            Assert.That(state.HasShownFirstRunGuide, Is.True);
        }

        [Test]
        public void TryLoad_InvalidValues_NormalizesToDefaults()
        {
            LocalSettingsStore.Save(new PendantV3LocalState
            {
                ActiveNavSection = string.Empty,
                ActiveWorkTab = null,
                ActiveTabletTab = string.Empty,
                CoordSystem = "Bad",
                SpeedPercent = 0,
                JogIncrement = 42,
                DesktopSplitRatio = -1f,
                IsTabletSheetExpanded = true,
                PointName = null,
                PointMotionKind = "Bad",
                PointTcpDraftValues = new[] { 0f, float.NaN, 2f },
                PointJointDraftValues = null,
            });

            Assert.That(LocalSettingsStore.TryLoad(out var state), Is.True);
            Assert.That(state.ActiveNavSection, Is.EqualTo(PendantV3LocalState.DefaultNavSection));
            Assert.That(state.ActiveWorkTab, Is.EqualTo(PendantV3LocalState.DefaultWorkTab));
            Assert.That(state.ActiveTabletTab, Is.EqualTo(PendantV3LocalState.DefaultTabletTab));
            Assert.That(state.CoordSystem, Is.EqualTo(PendantV3LocalState.DefaultCoordSystem));
            Assert.That(state.SpeedPercent, Is.EqualTo(PendantV3LocalState.DefaultSpeedPercent));
            Assert.That(state.JogIncrement, Is.EqualTo(PendantV3LocalState.DefaultJogIncrement));
            Assert.That(state.DesktopSplitRatio, Is.EqualTo(PendantV3LocalState.DefaultSplitRatio).Within(0.0001f));
            Assert.That(state.PointName, Is.EqualTo(PendantV3LocalState.DefaultPointName));
            Assert.That(state.PointMotionKind, Is.EqualTo(PendantV3LocalState.DefaultPointMotionKind));
            Assert.That(state.PointTcpDraftValues.Length, Is.EqualTo(PendantV3LocalState.PointAxisCount));
            Assert.That(state.PointTcpDraftValues[1], Is.EqualTo(0f).Within(0.0001f));
            Assert.That(state.PointJointDraftValues.Length, Is.EqualTo(PendantV3LocalState.PointAxisCount));
        }
    }
}
