// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotControlEntryPolicy 진입 정책을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotControlEntryPolicyTests
    {
        [SetUp]
        public void SetUp()
        {
            LocalSettingsStore.Clear();
            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.ResumeLastSession);
        }

        [TearDown]
        public void TearDown()
        {
            LocalSettingsStore.Clear();
            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.ResumeLastSession);
        }

        [Test]
        public void Apply_FreshStartForRobotControlV3_ClearsSavedState()
        {
            LocalSettingsStore.Save(new PendantV3LocalState
            {
                ActiveNavSection = "NavHelp",
                ActiveWorkTab = "TabTcpJog",
                ActiveTabletTab = "BottomTabStatus",
                CoordSystem = "Tool",
                SpeedPercent = 65,
                JogIncrement = 10,
                DesktopSplitRatio = 0.62f,
                IsTabletSheetExpanded = false,
            });

            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.FreshStart);

            var state = LocalSettingsStore.LoadOrDefault();
            Assert.That(state.ActiveNavSection, Is.EqualTo(PendantV3LocalState.DefaultNavSection));
            Assert.That(state.ActiveWorkTab, Is.EqualTo(PendantV3LocalState.DefaultWorkTab));
            Assert.That(state.ActiveTabletTab, Is.EqualTo(PendantV3LocalState.DefaultTabletTab));
            Assert.That(state.SpeedPercent, Is.EqualTo(PendantV3LocalState.DefaultSpeedPercent));
            Assert.That(RobotControlEntryPolicy.ShouldShowFirstRunGuide(), Is.True);
        }

        [Test]
        public void Apply_ResumeLastSessionForRobotControlV3_PreservesSavedState()
        {
            LocalSettingsStore.Save(new PendantV3LocalState
            {
                ActiveNavSection = "NavStatus",
                ActiveWorkTab = "TabJointJog",
                ActiveTabletTab = "BottomTabJointJog",
                CoordSystem = "User",
                SpeedPercent = 45,
                JogIncrement = 1,
                DesktopSplitRatio = 0.55f,
                IsTabletSheetExpanded = true,
            });

            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.ResumeLastSession);

            var state = LocalSettingsStore.LoadOrDefault();
            Assert.That(state.ActiveNavSection, Is.EqualTo("NavStatus"));
            Assert.That(state.ActiveWorkTab, Is.EqualTo("TabJointJog"));
            Assert.That(state.ActiveTabletTab, Is.EqualTo("BottomTabJointJog"));
            Assert.That(state.CoordSystem, Is.EqualTo("User"));
            Assert.That(state.SpeedPercent, Is.EqualTo(45));
            Assert.That(RobotControlEntryPolicy.ShouldShowFirstRunGuide(), Is.False);
        }

        [Test]
        public void Apply_FreshStart_PreservesFirstRunGuideSeenState()
        {
            LocalSettingsStore.Save(new PendantV3LocalState
            {
                HasShownFirstRunGuide = true,
                ActiveNavSection = "NavHelp",
                ActiveWorkTab = "TabPointMove",
                ActiveTabletTab = "BottomTabHelp",
            });

            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.FreshStart);

            var state = LocalSettingsStore.LoadOrDefault();
            Assert.That(state.HasShownFirstRunGuide, Is.True);
            Assert.That(RobotControlEntryPolicy.ShouldShowFirstRunGuide(), Is.False);
        }
    }
}
