// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.Reflection;
using KineTutor3D.App.Fairino;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class LiveCommandSafetyGateTests
    {
        [Test]
        public void Evaluate_MoveJ_PrioritizesReadbackOnlyBeforeDryRunBypass()
        {
            var service = CreateService(new StubLiveClient(isReadbackOnly: true));
            var gate = new LiveCommandSafetyGate();
            var request = CreateAllowedMoveJRequest(service);
            request.AllowDryRun = true;
            request.OperatorConfirmed = true;

            var result = gate.Evaluate(request);

            Assert.That(result.Status, Is.EqualTo(LiveCommandGateStatus.ReadbackOnly));
            Assert.That(result.BlockReasons, Does.Contain("live client is readback-only"));
            Assert.That(result.ClearedReasons, Does.Contain("actual motion/IO/gripper commands remain locked on macOS live readback"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenToolIdMissing()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.ToolId = 0;
            });

            Assert.That(result.BlockReasons, Does.Contain("toolId missing"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenUserIdMissing()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.UserId = 0;
            });

            Assert.That(result.BlockReasons, Does.Contain("userId missing"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenCoordSystemUnresolved()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.CoordSystem = string.Empty;
                request.HasResolvedCoordSystem = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("coordSystem unresolved"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenLatestStateFreshnessFails()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.HasFreshLatestState = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("latest-state freshness failed"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenLatestDriftFreshnessFails()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.HasFreshLatestDrift = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("latest-drift freshness failed"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenDriftThresholdFails()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.IsDriftWithinThreshold = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("drift threshold failed"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenTinyRangeExceeded()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.IsWithinTinyMoveRange = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("tiny MoveJ range exceeded"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenPreparedTargetMismatches()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.HasMatchingPreparedTarget = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("prepared target mismatch"));
        }

        [Test]
        public void Evaluate_MoveJ_BlocksWhenApprovalTargetMismatches()
        {
            var result = EvaluateBlockedMoveJ(request =>
            {
                request.HasMatchingApprovalContext = false;
            });

            Assert.That(result.BlockReasons, Does.Contain("operator approval target mismatch"));
        }

        [Test]
        public void Evaluate_MoveJ_RequiresConfirmThenAllowsOnceConfirmed()
        {
            var service = CreateService(new StubLiveClient());
            var gate = new LiveCommandSafetyGate();
            var request = CreateAllowedMoveJRequest(service);
            request.OperatorConfirmed = false;

            var requiresConfirm = gate.Evaluate(request);
            Assert.That(requiresConfirm.Status, Is.EqualTo(LiveCommandGateStatus.RequiresConfirm));
            Assert.That(requiresConfirm.BlockReasons, Does.Contain("operator confirm token required"));

            request.OperatorConfirmed = true;
            var allowed = gate.Evaluate(request);

            Assert.That(allowed.Status, Is.EqualTo(LiveCommandGateStatus.Allowed));
            Assert.That(allowed.ClearedReasons, Does.Contain("operator confirm token accepted"));
            Assert.That(allowed.ClearedReasons, Does.Contain("latest-state freshness ok"));
            Assert.That(allowed.ClearedReasons, Does.Contain("latest-drift freshness ok"));
            Assert.That(allowed.ClearedReasons, Does.Contain("drift within threshold"));
        }

        [Test]
        public void Evaluate_GripperOnlySession_BlocksMoveJ()
        {
            var service = CreateService(new StubLiveClient());
            var gate = new LiveCommandSafetyGate();
            var request = CreateAllowedMoveJRequest(service);
            request.OperatorConfirmed = true;
            request.SessionMode = LiveCommandSessionMode.GripperOnly;

            var result = gate.Evaluate(request);

            Assert.That(result.Status, Is.EqualTo(LiveCommandGateStatus.Blocked));
            Assert.That(result.BlockReasons, Does.Contain("session mode GripperOnly does not allow MoveJ"));
        }

        [Test]
        public void Evaluate_GripperOnlySession_AllowsMoveGripperOnReadbackOnlyWhenOverrideEnabled()
        {
            var service = CreateService(new StubLiveClient(isReadbackOnly: true));
            var gate = new LiveCommandSafetyGate();
            var request = new LiveCommandSafetyGateRequest
            {
                Kind = LiveCommandKind.MoveGripper,
                ConnectionService = service,
                OperatorConfirmed = true,
                SessionMode = LiveCommandSessionMode.GripperOnly,
                AllowReadbackOnlyGripperPathOverride = true,
                RequestedSpeedPercent = 5,
                SpeedCapPercent = 10,
                ToolId = 1,
                UserId = 1,
                CoordSystem = "Base",
                HasResolvedCoordSystem = true,
                HasFreshLatestState = true,
                HasFreshLatestDrift = true,
                IsDriftWithinThreshold = true,
                HasGripperReadback = true,
            };

            var result = gate.Evaluate(request);

            Assert.That(result.Status, Is.EqualTo(LiveCommandGateStatus.Allowed));
            Assert.That(result.BlockReasons, Does.Not.Contain("live client is readback-only"));
            Assert.That(result.ClearedReasons, Does.Contain("gripper-only live path enabled"));
        }

        [Test]
        public void Evaluate_MoveJ_DedicatedTinyPathCanBypassReadbackOnlySummary()
        {
            var service = CreateService(new StubLiveClient(isReadbackOnly: true));
            var gate = new LiveCommandSafetyGate();
            var request = CreateAllowedMoveJRequest(service);
            request.OperatorConfirmed = true;
            request.AllowReadbackOnlyMotionPathOverride = true;
            request.HasDedicatedTinyMoveJMotionPath = true;
            request.IsBoundaryDataReady = false;
            request.IsTargetWithinBoundary = false;
            request.IsCollisionDataReady = false;
            request.IsPredictedPathCollisionFree = false;

            var result = gate.Evaluate(request);

            Assert.That(result.Status, Is.EqualTo(LiveCommandGateStatus.Allowed));
            Assert.That(result.BlockReasons, Does.Not.Contain("live client is readback-only"));
            Assert.That(result.ClearedReasons, Does.Contain("tiny MoveJ dedicated live path enabled"));
        }

        private static LiveCommandSafetyGateResult EvaluateBlockedMoveJ(System.Action<LiveCommandSafetyGateRequest> configure)
        {
            var service = CreateService(new StubLiveClient());
            var gate = new LiveCommandSafetyGate();
            var request = CreateAllowedMoveJRequest(service);
            request.OperatorConfirmed = true;
            configure?.Invoke(request);

            var result = gate.Evaluate(request);

            Assert.That(result.Status, Is.EqualTo(LiveCommandGateStatus.Blocked));
            return result;
        }

        private static LiveCommandSafetyGateRequest CreateAllowedMoveJRequest(FairinoConnectionService service)
        {
            return new LiveCommandSafetyGateRequest
            {
                Kind = LiveCommandKind.MoveJ,
                ConnectionService = service,
                SessionMode = LiveCommandSessionMode.TinyMoveJOnly,
                RequestedSpeedPercent = 5,
                SpeedCapPercent = 10,
                OperatorConfirmed = true,
                ToolId = 1,
                UserId = 1,
                CoordSystem = "Base",
                HasResolvedCoordSystem = true,
                HasFreshLatestState = true,
                HasFreshLatestDrift = true,
                IsDriftWithinThreshold = true,
                LatestStateTimestampUtc = "2026-04-28T00:00:00.0000000Z",
                LatestDriftTimestampUtc = "2026-04-28T00:00:00.0000000Z",
                HasDryRunPreviewArtifact = true,
                IsProductionIkSafe = true,
                IsBoundaryDataReady = true,
                IsTargetWithinBoundary = true,
                IsCollisionDataReady = true,
                IsPredictedPathCollisionFree = true,
            };
        }

        private static FairinoConnectionService CreateService(StubLiveClient client)
        {
            var service = new FairinoConnectionService();
            service.SetMockMode(false);
            SetPrivateField(service, "client", client);
            return service;
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private sealed class StubLiveClient : IFairinoRobotClient, IFairinoLiveClientDiagnostics
        {
            private readonly FairinoRobotState state = new(
                new[] { 1d, 2d, 3d, 4d, 5d, 6d },
                new[] { 10d, 20d, 30d, 40d, 50d, 60d },
                toolId: 1,
                userId: 1,
                isRobotEnabled: true);

            public StubLiveClient(bool isReadbackOnly = false)
            {
                IsReadbackOnly = isReadbackOnly;
            }

            public bool IsConnected => true;
            public bool IsEnabled => true;
            public string ClientMode => "direct";
            public string SdkLoadStatus => "direct-ready";
            public string SdkVersion => "StubSDK";
            public string SdkRuntime => "TestRuntime";
            public bool IsReadbackOnly { get; }

            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Ok("enabled");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Ok("movej");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Ok("servoj");
            public FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Ok(state);
            public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent) => FairinoResult.Ok("movel");
            public FairinoResult StopMotion() => FairinoResult.Ok("stop");
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
            public FairinoResult ConfigureGripper(FairinoGripperProfile profile) => FairinoResult.Ok("configure");
            public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate) => FairinoResult.Ok("activate");
            public FairinoResult MoveGripper(FairinoGripperCommand command) => FairinoResult.Ok("gripper");
        }
    }
}
