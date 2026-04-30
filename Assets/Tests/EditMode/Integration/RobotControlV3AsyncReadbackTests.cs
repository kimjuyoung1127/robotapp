// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using KineTutor3D.App.Fairino;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotControlV3AsyncReadbackTests
    {
        [Test]
        public void SyncCurrentStateAsync_ReturnsImmediatelyAndAppliesReadbackOnUpdate()
        {
            var client = new SlowConnectedClient(delayMs: 150);
            var controller = CreateController(client, out var runtimeObject);

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var started = controller.SyncCurrentStateAsync();
                stopwatch.Stop();

                Assert.That(started, Is.True);
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100), "sync 버튼 클릭은 SDK readback 완료를 기다리면 안 된다.");
                Assert.That(controller.CurrentSnapshot.SyncEnabled, Is.False);
                Assert.That(controller.CurrentSnapshot.StatusMotion, Is.EqualTo("읽는 중"));

                Thread.Sleep(220);
                InvokePrivateUpdate(controller);

                Assert.That(controller.CurrentSnapshot.CurrentPositionReadComplete, Is.True);
                Assert.That(controller.CurrentSnapshot.SyncEnabled, Is.True);
                Assert.That(controller.CurrentRobotStateForDebug.JointPosDeg[0], Is.EqualTo(1d));
                Assert.That(controller.CurrentRobotStateForDebug.TcpPose[0], Is.EqualTo(10d));
            }
            finally
            {
                Object.DestroyImmediate(runtimeObject);
            }
        }

        [Test]
        public void SyncCurrentStateAsync_RejectsDuplicateRequestsWhilePending()
        {
            var client = new SlowConnectedClient(delayMs: 150);
            var controller = CreateController(client, out var runtimeObject);

            try
            {
                var firstStarted = controller.SyncCurrentStateAsync();
                var secondStarted = controller.SyncCurrentStateAsync();

                Assert.That(firstStarted, Is.True);
                Assert.That(secondStarted, Is.False);

                Thread.Sleep(220);
                InvokePrivateUpdate(controller);

                Assert.That(client.ReadStateCallCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(runtimeObject);
            }
        }

        private static RobotControlV3RuntimeController CreateController(SlowConnectedClient client, out GameObject runtimeObject)
        {
            runtimeObject = new GameObject("RobotControlV3RuntimeControllerTests");
            var controller = runtimeObject.AddComponent<RobotControlV3RuntimeController>();
            var connectionService = new FairinoConnectionService();

            SetPrivateField(connectionService, "client", client);
            SetPrivateField(connectionService, "useMock", false);
            SetPrivateField(controller, "connectionService", connectionService);
            SetPrivateField(controller, "config", new FairinoRobotConfig
            {
                defaultIp = "192.168.57.2",
                defaultPort = 8080,
            });
            SetPrivateField(controller, "templateDefinition", new RobotControlTemplateDefinition
            {
                RobotId = "FAIRINO_FR5",
                DisplayName = "FAIRINO FR5",
                JointCount = 6,
            });

            return controller;
        }

        private static void InvokePrivateUpdate(RobotControlV3RuntimeController controller)
        {
            var method = typeof(RobotControlV3RuntimeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(controller, null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"field '{fieldName}' should exist");
            field.SetValue(target, value);
        }

        private sealed class SlowConnectedClient : IFairinoRobotClient
        {
            private readonly int delayMs;
            private readonly FairinoRobotState state = new(
                new[] { 1d, 2d, 3d, 4d, 5d, 6d },
                new[] { 10d, 20d, 30d, 40d, 50d, 60d },
                toolId: 1,
                userId: 1);

            public SlowConnectedClient(int delayMs)
            {
                this.delayMs = delayMs;
            }

            public int ReadStateCallCount { get; private set; }

            public bool IsConnected => true;
            public bool IsEnabled => true;
            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Ok("enabled");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Ok("movej");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Ok("servoj");

            public FairinoResult<FairinoRobotState> ReadState()
            {
                ReadStateCallCount++;
                Thread.Sleep(delayMs);
                return FairinoResult<FairinoRobotState>.Ok(state);
            }

            public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent) => FairinoResult.Ok("movel");
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
    }
}
