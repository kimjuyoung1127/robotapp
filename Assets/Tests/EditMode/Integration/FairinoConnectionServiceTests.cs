// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// FairinoConnectionService 동작을 검증하는 EditMode 테스트입니다.
using System.Reflection;
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

        [Test]
        public void ApplyLiveDefaults_Uses33MsAsDefaultPollInterval()
        {
            var service = new FairinoConnectionService();

            service.ApplyLiveDefaults(new FairinoRobotConfig.LiveDefaultsBlock
            {
                realtimeSampleMs = 33,
            });

            Assert.That(service.CurrentPollIntervalSeconds, Is.EqualTo(0.033f).Within(0.0001f));
        }

        [Test]
        public void Tick_FallsBackTo50MsAfterRepeatedFastPollErrors()
        {
            var service = new FairinoConnectionService();
            service.SetMockMode(false);
            service.SetPollInterval(0.033f);
            SetPrivateField(service, "client", new FailingLiveClient());

            service.Tick(0.033f);
            Assert.That(service.CurrentPollIntervalSeconds, Is.EqualTo(0.033f).Within(0.0001f));

            service.Tick(0.033f);
            Assert.That(service.CurrentPollIntervalSeconds, Is.EqualTo(0.05f).Within(0.0001f));
        }

        [Test]
        public void Tick_ForcedDebugFailures_AlsoFallBackTo50Ms()
        {
            var service = new FairinoConnectionService();
            service.SetMockMode(false);
            service.SetPollInterval(0.033f);
            SetPrivateField(service, "client", new RecoveringLiveClient());
            service.ForceNextReadFailuresForDebug(2, "forced debug read fail");

            service.Tick(0.033f);
            Assert.That(service.CurrentPollIntervalSeconds, Is.EqualTo(0.033f).Within(0.0001f));
            Assert.That(service.ConsecutiveReadErrors, Is.EqualTo(1));
            Assert.That(service.ForcedReadFailuresRemaining, Is.EqualTo(1));

            service.Tick(0.033f);
            Assert.That(service.CurrentPollIntervalSeconds, Is.EqualTo(0.05f).Within(0.0001f));
            Assert.That(service.ConsecutiveReadErrors, Is.EqualTo(2));
            Assert.That(service.ForcedReadFailuresRemaining, Is.EqualTo(0));

            service.Tick(0.05f);
            Assert.That(service.ConsecutiveReadErrors, Is.EqualTo(0));
        }

        [Test]
        public void CreateMotionSiblingSession_ReusesMotionCapableClientFromReadbackWrapper()
        {
            var motionClient = new RecoveringMotionCapableClient();
            var service = new FairinoConnectionService();
            SetPrivateField(service, "useMock", false);
            SetPrivateField(service, "client", new ReadbackWrapperClient(motionClient));

            var siblingResult = service.CreateMotionSiblingSession();

            Assert.That(siblingResult.IsSuccess, Is.True, siblingResult.Message);
            Assert.That(ReferenceEquals(siblingResult.Value.Client, motionClient), Is.True);
            Assert.That(siblingResult.Value.IsMockMode, Is.False);
        }

        private sealed class FailingLiveClient : IFairinoRobotClient
        {
            public bool IsConnected => true;
            public bool IsEnabled => false;
            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Fail(-1, "blocked");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Fail(-99, "read fail");
            public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult StopMotion() => FairinoResult.Ok("stopped");
            public FairinoResult<FairinoVersionInfo> GetVersion() => FairinoResult<FairinoVersionInfo>.Ok(default);
            public FairinoResult<int> GetSafetyCode() => FairinoResult<int>.Ok(0);
            public FairinoResult<int> GetRealtimeStateSamplePeriod() => FairinoResult<int>.Ok(33);
            public FairinoResult SetRealtimeStateSamplePeriod(int periodMs) => FairinoResult.Ok("sample");
            public FairinoResult ClearMotionQueue() => FairinoResult.Ok("queue");
            public FairinoResult SetMode(int mode) => FairinoResult.Ok("mode");
            public FairinoResult SetReconnect(bool enable, int timeoutMs, int periodMs) => FairinoResult.Ok("reconnect");
            public FairinoResult ExitDragTeach() => FairinoResult.Ok("drag");
            public FairinoResult EnsureAutoMode() => FairinoResult.Ok("auto");
            public FairinoResult<FairinoCoordContext> ReadCoordContext() => FairinoResult<FairinoCoordContext>.Ok(FairinoCoordContext.Default());
            public FairinoResult<FairinoControllerFault> ReadControllerFault() => FairinoResult<FairinoControllerFault>.Ok(FairinoControllerFault.None());
            public FairinoResult ResetErrors() => FairinoResult.Ok("reset");
            public FairinoResult<FairinoGripperCapability> ProbeGripperCapability() => FairinoResult<FairinoGripperCapability>.Ok(default);
            public FairinoResult<FairinoGripperStatus> ReadGripperStatus() => FairinoResult<FairinoGripperStatus>.Ok(default);
            public FairinoResult<FairinoGripperConfigState> ReadGripperConfig() => FairinoResult<FairinoGripperConfigState>.Ok(default);
            public FairinoResult ConfigureGripper(FairinoGripperProfile profile) => FairinoResult.Ok("config");
            public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate) => FairinoResult.Ok("activate");
            public FairinoResult MoveGripper(FairinoGripperCommand command) => FairinoResult.Ok("move");
        }

        private class RecoveringLiveClient : IFairinoRobotClient
        {
            private readonly FairinoRobotState state = new(
                new[] { 1d, 2d, 3d, 4d, 5d, 6d },
                new[] { 10d, 20d, 30d, 40d, 50d, 60d },
                toolId: 1,
                userId: 1);

            public bool IsConnected => true;
            public bool IsEnabled => false;
            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Fail(-1, "blocked");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Ok(state);
            public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent) => FairinoResult.Fail(-1, "blocked");
            public FairinoResult StopMotion() => FairinoResult.Ok("stopped");
            public FairinoResult<FairinoVersionInfo> GetVersion() => FairinoResult<FairinoVersionInfo>.Ok(default);
            public FairinoResult<int> GetSafetyCode() => FairinoResult<int>.Ok(0);
            public FairinoResult<int> GetRealtimeStateSamplePeriod() => FairinoResult<int>.Ok(33);
            public FairinoResult SetRealtimeStateSamplePeriod(int periodMs) => FairinoResult.Ok("sample");
            public FairinoResult ClearMotionQueue() => FairinoResult.Ok("queue");
            public FairinoResult SetMode(int mode) => FairinoResult.Ok("mode");
            public FairinoResult SetReconnect(bool enable, int timeoutMs, int periodMs) => FairinoResult.Ok("reconnect");
            public FairinoResult ExitDragTeach() => FairinoResult.Ok("drag");
            public FairinoResult EnsureAutoMode() => FairinoResult.Ok("auto");
            public FairinoResult<FairinoCoordContext> ReadCoordContext() => FairinoResult<FairinoCoordContext>.Ok(new FairinoCoordContext(1, 1, null, null));
            public FairinoResult<FairinoControllerFault> ReadControllerFault() => FairinoResult<FairinoControllerFault>.Ok(FairinoControllerFault.None());
            public FairinoResult ResetErrors() => FairinoResult.Ok("reset");
            public FairinoResult<FairinoGripperCapability> ProbeGripperCapability() => FairinoResult<FairinoGripperCapability>.Ok(default);
            public FairinoResult<FairinoGripperStatus> ReadGripperStatus() => FairinoResult<FairinoGripperStatus>.Ok(default);
            public FairinoResult<FairinoGripperConfigState> ReadGripperConfig() => FairinoResult<FairinoGripperConfigState>.Ok(default);
            public FairinoResult ConfigureGripper(FairinoGripperProfile profile) => FairinoResult.Ok("config");
            public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate) => FairinoResult.Ok("activate");
            public FairinoResult MoveGripper(FairinoGripperCommand command) => FairinoResult.Ok("move");
        }

        private sealed class RecoveringMotionCapableClient : RecoveringLiveClient, IFairinoLiveClientDiagnostics, IFairinoMotionSessionProvider
        {
            public string ClientMode => "direct-motion";
            public string SdkLoadStatus => "direct-ready";
            public string SdkVersion => "test";
            public string SdkRuntime => "test";
            public bool IsReadbackOnly => false;

            public bool TryGetMotionCapableClient(out IFairinoRobotClient motionClient)
            {
                motionClient = this;
                return true;
            }
        }

        private sealed class ReadbackWrapperClient : FairinoReadbackOnlyClientBase, IFairinoMotionSessionProvider
        {
            private readonly IFairinoRobotClient motionClient;

            public ReadbackWrapperClient(IFairinoRobotClient motionClient)
            {
                this.motionClient = motionClient;
            }

            public override bool IsConnected => true;
            public override string ClientMode => "direct";
            public override string SdkLoadStatus => "direct-ready";
            public override string SdkVersion => "test";
            public override string SdkRuntime => "test";
            public override FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public override FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public override FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Ok(default);
            public override FairinoResult<FairinoVersionInfo> GetVersion() => FairinoResult<FairinoVersionInfo>.Ok(default);

            public bool TryGetMotionCapableClient(out IFairinoRobotClient motionCapableClient)
            {
                motionCapableClient = motionClient;
                return motionCapableClient != null;
            }
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }
    }
}
