// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotControlMotionRuntime 선택/dispatch 계약을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotControlMotionRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            RobotSelectionBridge.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            RobotSelectionBridge.Clear();
        }

        [Test]
        public void CreateFromSelection_WithoutRobotSelection_Fails()
        {
            var result = RobotControlMotionRuntime.CreateFromSelection();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("선택된 로봇"));
        }

        [Test]
        public void DispatchMoveL_WithFairinoSelection_UsesMockRuntimeSuccessfully()
        {
            RobotSelectionBridge.SetSelection("FAIRINO_FR5", RobotSelectionBridge.RobotControlMode);
            var createResult = RobotControlMotionRuntime.CreateFromSelection();

            Assert.That(createResult.IsSuccess, Is.True, createResult.Message);
            Assert.That(createResult.Value.RobotId, Is.EqualTo("FAIRINO_FR5"));

            var dispatchResult = createResult.Value.DispatchMoveL(
                new[] { -497d, -130d, 477d, 180d, 0d, 90d },
                30);

            Assert.That(dispatchResult.IsSuccess, Is.True, dispatchResult.Message);
        }

        [Test]
        public void DispatchMoveJ_WithFairinoSelection_UsesMockRuntimeSuccessfully()
        {
            RobotSelectionBridge.SetSelection("FAIRINO_FR5", RobotSelectionBridge.RobotControlMode);
            var createResult = RobotControlMotionRuntime.CreateFromSelection();

            Assert.That(createResult.IsSuccess, Is.True, createResult.Message);
            Assert.That(createResult.Value.RobotId, Is.EqualTo("FAIRINO_FR5"));

            var dispatchResult = createResult.Value.DispatchMoveJ(
                new[] { 0d, -32d, 84d, 0d, 90d, 0d },
                30);

            Assert.That(dispatchResult.IsSuccess, Is.True, dispatchResult.Message);
        }
    }
}
