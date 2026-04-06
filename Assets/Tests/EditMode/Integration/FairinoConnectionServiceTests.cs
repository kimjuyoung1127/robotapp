// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// FairinoConnectionService 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App.Fairino;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// FairinoConnectionService의 상태 이벤트와 Mock 기본 동작을 검증합니다.
    /// </summary>
    public class FairinoConnectionServiceTests
    {
        [Test]
        public void SetMockMode_EmitsModeAndStateEvents()
        {
            var service = new FairinoConnectionService();
            bool? modeChanged = null;
            bool? connectionChanged = null;
            bool? enableChanged = null;
            FairinoRobotState latestState = default;

            service.OnModeChanged += value => modeChanged = value;
            service.OnConnectionStateChanged += value => connectionChanged = value;
            service.OnEnableStateChanged += value => enableChanged = value;
            service.OnStateUpdated += value => latestState = value;

            // 생성자에서 이미 Mock=true이므로, false→true로 전환해야 이벤트 발생
            service.SetMockMode(false);
            service.SetMockMode(true);

            Assert.That(modeChanged, Is.True);
            Assert.That(connectionChanged, Is.False);
            Assert.That(enableChanged, Is.False);
            Assert.That(latestState.JointPosDeg, Is.Not.Null);
            Assert.That(latestState.JointPosDeg.Length, Is.EqualTo(6));
        }

        [Test]
        public void Connect_EmitsConnectionAndState()
        {
            var service = new FairinoConnectionService();
            bool? connectionChanged = null;
            FairinoRobotState latestState = default;

            service.OnConnectionStateChanged += value => connectionChanged = value;
            service.OnStateUpdated += value => latestState = value;

            var result = service.Connect("192.168.58.2", 8080);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(connectionChanged, Is.True);
            Assert.That(latestState.JointPosDeg, Is.Not.Null);
        }

        [Test]
        public void EnableAndDisable_EmitServoState()
        {
            var service = new FairinoConnectionService();
            bool? enableChanged = null;

            service.Connect("192.168.58.2", 8080);
            service.OnEnableStateChanged += value => enableChanged = value;

            var enableResult = service.Enable();
            Assert.That(enableResult.IsSuccess, Is.True);
            Assert.That(enableChanged, Is.True);

            var disableResult = service.Disable();
            Assert.That(disableResult.IsSuccess, Is.True);
            Assert.That(enableChanged, Is.False);
        }

        [Test]
        public void Connect_CachesCoordContextAndFaultState()
        {
            var service = new FairinoConnectionService();

            var result = service.Connect("192.168.58.2", 8080);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(service.LastCoordContext.ToolId, Is.EqualTo(0));
            Assert.That(service.LastCoordContext.UserId, Is.EqualTo(0));
            Assert.That(service.LastControllerFault.MainCode, Is.EqualTo(0));
            Assert.That(service.LastControllerFault.SubCode, Is.EqualTo(0));
        }
    }
}
