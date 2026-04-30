// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.Collections.Generic;
using System.Reflection;
using KineTutor3D.App.Fairino;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class RobotControlPeripheralFacadeTests
    {
        [Test]
        public void SetGripperPosition_LiveCommandResetsAndActivatesBeforeMoveWhenNotReady()
        {
            var client = new TrackingGripperClient();
            client.EnqueueStatus(new FairinoGripperStatus(
                motionFault: 1,
                motionDone: 0,
                activationFault: 1,
                activationMask: 0,
                positionFault: 1,
                positionPercent: 0,
                speedFault: 1,
                speedPercent: 0,
                currentFault: 1,
                currentPercent: 0,
                voltageFault: 1,
                voltage: 0,
                temperatureFault: 1,
                temperature: 0));
            client.EnqueueStatus(new FairinoGripperStatus(
                motionFault: 0,
                motionDone: 0,
                activationFault: 0,
                activationMask: 2,
                positionFault: 0,
                positionPercent: 100,
                speedFault: 0,
                speedPercent: 50,
                currentFault: 0,
                currentPercent: 10,
                voltageFault: 0,
                voltage: 24,
                temperatureFault: 0,
                temperature: 30));
            client.EnqueueStatus(new FairinoGripperStatus(
                motionFault: 0,
                motionDone: 1,
                activationFault: 0,
                activationMask: 2,
                positionFault: 0,
                positionPercent: 98,
                speedFault: 0,
                speedPercent: 50,
                currentFault: 0,
                currentPercent: 10,
                voltageFault: 0,
                voltage: 24,
                temperatureFault: 0,
                temperature: 30));

            var liveService = CreateLiveService(client);
            var previewService = new FairinoConnectionService();
            var facade = new RobotControlPeripheralFacade(previewService);

            var result = facade.SetGripperPosition(95, allowDryRun: false, objectDetected: false, objectStopPercent: 0, liveService);

            Assert.That(result.IsSuccess, Is.True, result.Message);
            Assert.That(client.Calls, Is.EqualTo(new[]
            {
                "probe",
                "read",
                "configure",
                "activate:False",
                "activate:True",
                "read",
                "move",
                "read",
                "probe",
                "read",
            }));
        }

        [Test]
        public void SetGripperPosition_LiveCommandMarksUnreliableReadbackWhenStatusFaultsRemain()
        {
            var client = new TrackingGripperClient();
            client.EnqueueStatus(new FairinoGripperStatus(
                motionFault: 0,
                motionDone: 1,
                activationFault: 0,
                activationMask: 2,
                positionFault: 0,
                positionPercent: 100,
                speedFault: 0,
                speedPercent: 50,
                currentFault: 0,
                currentPercent: 10,
                voltageFault: 0,
                voltage: 24,
                temperatureFault: 0,
                temperature: 30));
            client.EnqueueStatus(new FairinoGripperStatus(
                motionFault: 1,
                motionDone: 0,
                activationFault: 0,
                activationMask: 2,
                positionFault: 1,
                positionPercent: 0,
                speedFault: 1,
                speedPercent: 0,
                currentFault: 1,
                currentPercent: 0,
                voltageFault: 1,
                voltage: 0,
                temperatureFault: 1,
                temperature: 0));

            var liveService = CreateLiveService(client);
            var previewService = new FairinoConnectionService();
            var facade = new RobotControlPeripheralFacade(previewService);

            var result = facade.SetGripperPosition(95, allowDryRun: false, objectDetected: false, objectStopPercent: 0, liveService);

            Assert.That(result.IsSuccess, Is.True, result.Message);
            Assert.That(facade.Snapshot.HasReliableGripperReadback, Is.False);
            Assert.That(facade.Snapshot.LastPeripheralFeedback, Does.Contain("readback 확인 안 됨"));
        }

        private static FairinoConnectionService CreateLiveService(IFairinoRobotClient client)
        {
            var service = new FairinoConnectionService();
            service.SetMockMode(false);
            var field = typeof(FairinoConnectionService).GetField("client", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            field.SetValue(service, client);
            return service;
        }

        private sealed class TrackingGripperClient : IFairinoRobotClient
        {
            private readonly Queue<FairinoGripperStatus> statuses = new();

            public List<string> Calls { get; } = new();
            public bool IsConnected => true;
            public bool IsEnabled => false;

            public void EnqueueStatus(FairinoGripperStatus status)
            {
                statuses.Enqueue(status);
            }

            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Ok("enabled");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Ok("movej");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Ok("servoj");
            public FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Ok(default);
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
            public FairinoResult<FairinoCoordContext> ReadCoordContext() => FairinoResult<FairinoCoordContext>.Ok(FairinoCoordContext.Default());
            public FairinoResult<FairinoControllerFault> ReadControllerFault() => FairinoResult<FairinoControllerFault>.Ok(FairinoControllerFault.None());
            public FairinoResult ResetErrors() => FairinoResult.Ok("reset");

            public FairinoResult<FairinoGripperCapability> ProbeGripperCapability()
            {
                Calls.Add("probe");
                return FairinoResult<FairinoGripperCapability>.Ok(new FairinoGripperCapability(true, true, true, true, true, true, true, true, true, true));
            }

            public FairinoResult<FairinoGripperConfigState> ReadGripperConfig()
            {
                Calls.Add("config-read");
                return FairinoResult<FairinoGripperConfigState>.Ok(new FairinoGripperConfigState(4, 0, 0, 2));
            }

            public FairinoResult<FairinoGripperStatus> ReadGripperStatus()
            {
                Calls.Add("read");
                return statuses.Count > 0
                    ? FairinoResult<FairinoGripperStatus>.Ok(statuses.Dequeue())
                    : FairinoResult<FairinoGripperStatus>.Fail(-90, "status queue empty");
            }

            public FairinoResult ConfigureGripper(FairinoGripperProfile profile)
            {
                Calls.Add("configure");
                return FairinoResult.Ok("configure");
            }

            public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate)
            {
                Calls.Add($"activate:{activate}");
                return FairinoResult.Ok("activate");
            }

            public FairinoResult MoveGripper(FairinoGripperCommand command)
            {
                Calls.Add("move");
                return FairinoResult.Ok("move");
            }
        }
    }
}
