// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KineTutor3D.App.Fairino;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class Fr5LiveReadbackTests
    {
        [Test]
        public void Recorder_WritesLatestStateAndSessionLogs()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var service = new FairinoConnectionService();
            service.Connect("192.168.58.2", 8080);
            var recorder = new Fr5LiveStateRecorder(service, () => service.LastState, null, null, root);

            var drift = recorder.RecordState(service.LastState);

            Assert.That(drift.severity, Is.EqualTo("ok"));
            Assert.That(File.Exists(Path.Combine(root, "latest-state.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "latest-drift.json")), Is.True);
            Assert.That(Directory.GetFiles(Path.Combine(root, "sessions"), "*-readback.ndjson").Length, Is.EqualTo(1));
        }

        [Test]
        public void Recorder_PreservesLastConnectedLatestStateAfterDisconnect()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var service = new FairinoConnectionService();
            var recorder = new Fr5LiveStateRecorder(service, () => service.LastState, null, null, root);
            recorder.Attach();

            service.Connect("192.168.58.2", 8080);
            var latestPath = Path.Combine(root, "latest-state.json");
            Assert.That(File.Exists(latestPath), Is.True);

            var beforeDisconnect = File.ReadAllText(latestPath);
            Assert.That(beforeDisconnect, Does.Contain("\"connected\": true"));

            service.Disconnect();

            var afterDisconnect = File.ReadAllText(latestPath);
            Assert.That(afterDisconnect, Is.EqualTo(beforeDisconnect));
            Assert.That(afterDisconnect, Does.Contain("\"connected\": true"));
        }

        [Test]
        public void Recorder_PreservesLastConnectedLatestStateWhenPlaceholderZeroStateArrives()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var service = new FairinoConnectionService();
            service.Connect("192.168.58.2", 8080);
            var recorder = new Fr5LiveStateRecorder(service, () => service.LastState, null, null, root);

            recorder.RecordState(new FairinoRobotState(
                new[] { 1d, 2d, 3d, 4d, 5d, 6d },
                new[] { 10d, 20d, 30d, 40d, 50d, 60d },
                toolId: 1,
                userId: 1));
            var latestPath = Path.Combine(root, "latest-state.json");
            var beforePlaceholder = File.ReadAllText(latestPath);

            recorder.RecordState(FairinoRobotState.Zero());

            var afterPlaceholder = File.ReadAllText(latestPath);
            Assert.That(afterPlaceholder, Is.EqualTo(beforePlaceholder));
            Assert.That(afterPlaceholder, Does.Contain("\"toolId\": 1"));
            Assert.That(afterPlaceholder, Does.Contain("\"userId\": 1"));
        }

        [Test]
        public void Recorder_DetectsDangerDrift()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var live = new FairinoRobotState(new[] { 10d, 0d, 0d, 0d, 0d, 0d }, new[] { 20d, 0d, 0d, 0d, 0d, 0d });
            var screen = FairinoRobotState.Zero();
            var service = new FairinoConnectionService();
            var recorder = new Fr5LiveStateRecorder(service, () => screen, null, null, root);

            var drift = recorder.RecordState(live);

            Assert.That(drift.severity, Is.EqualTo("danger"));
            Assert.That(drift.liveBlockedReason, Does.Contain("실기 이동 차단"));
        }

        [Test]
        public void Recorder_ClearsBlockedReasonWhenDriftReturnsOk()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var liveDanger = new FairinoRobotState(
                new[] { 10d, 0d, 0d, 0d, 0d, 0d },
                new[] { 100d, 0d, 0d, 0d, 0d, 0d },
                toolId: 1,
                userId: 1);
            var liveOk = FairinoRobotState.Zero();
            var screen = FairinoRobotState.Zero();
            var latestReason = "seed";
            var service = new FairinoConnectionService();
            service.Connect("192.168.58.2", 8080);
            var recorder = new Fr5LiveStateRecorder(service, () => screen, null, value => latestReason = value, root);

            recorder.RecordState(liveDanger);
            recorder.RecordState(liveOk);
            Assert.That(latestReason, Is.Empty);
        }

        [Test]
        public void BridgeClient_LiveCommandsAreBlockedReadbackOnly()
        {
            var client = new FairinoBridgeClient("http://127.0.0.1:5055");

            Assert.That(client.Enable().IsSuccess, Is.False);
            Assert.That(client.MoveJ(new double[6], 10, 10).Message, Does.Contain("readback-only"));
            Assert.That(client.MoveL(new double[6], 10, 10).Message, Does.Contain("readback-only"));
        }

        [Test]
        public void BridgeClient_ReadState_MapsJsonToRobotState()
        {
            using var server = BridgeTestServer.Start();
            var client = new FairinoBridgeClient(server.Url);

            Assert.That(client.Connect("192.168.58.2", 8080).IsSuccess, Is.True);
            Assert.That(((IFairinoLiveClientDiagnostics)client).SdkVersion, Is.EqualTo("BridgeSDK-1.2.3"));
            var state = client.ReadState();

            Assert.That(state.IsSuccess, Is.True);
            Assert.That(state.Value.JointPosDeg[1], Is.EqualTo(-20d));
            Assert.That(state.Value.TcpPose[2], Is.EqualTo(300d));
            Assert.That(state.Value.ToolId, Is.EqualTo(2));
            Assert.That(state.Value.UserId, Is.EqualTo(3));
        }

        [Test]
        public void DirectReadbackClient_LiveCommandsAreBlockedReadbackOnly()
        {
            var report = FairinoSdkCompatibilityProbe.Probe();
            var client = new DirectReadbackFairinoClient(new LiveFairinoClient(), report);

            Assert.That(client.Enable().IsSuccess, Is.False);
            Assert.That(client.MoveJ(new double[6], 10, 10).Message, Does.Contain("readback-only"));
            Assert.That(client.MoveL(new double[6], 10, 10).Message, Does.Contain("readback-only"));
        }

        [Test]
        public void TinyMoveJGateSummary_PrefersReadbackOnlyOverDryRun()
        {
            var originalTinyMoveJLive = System.Environment.GetEnvironmentVariable(FairinoRobotClientFactory.TinyMoveJLiveEnvironmentVariable);
            var runtimeGo = new GameObject("RobotControlV3RuntimeControllerTest");
            runtimeGo.SetActive(false);
            var runtime = runtimeGo.AddComponent<RobotControlV3RuntimeController>();
            var service = new FairinoConnectionService();
            service.SetMockMode(false);

            try
            {
                System.Environment.SetEnvironmentVariable(FairinoRobotClientFactory.TinyMoveJLiveEnvironmentVariable, null);
                SetPrivateField(service, "client", new FakeReadbackOnlyClient());
                SetPrivateField(runtime, "connectionService", service);

                var summary = runtime.GetTinyMoveJGateSummaryForDebug();

                Assert.That(summary, Does.Contain("status=ReadbackOnly"));
                Assert.That(summary, Does.Contain("live client is readback-only"));
                Assert.That(summary, Does.Contain("tool=1"));
                Assert.That(summary, Does.Contain("user=1"));
            }
            finally
            {
                System.Environment.SetEnvironmentVariable(FairinoRobotClientFactory.TinyMoveJLiveEnvironmentVariable, originalTinyMoveJLive);
                Object.DestroyImmediate(runtimeGo);
            }
        }

        [Test]
        public void SmokeRunner_UsesBridgeFactoryPathWhenBridgeEnvIsSet()
        {
            using var server = BridgeTestServer.Start();
            var originalBridgeUrl = System.Environment.GetEnvironmentVariable(FairinoRobotClientFactory.BridgeUrlEnvironmentVariable);
            try
            {
                System.Environment.SetEnvironmentVariable(FairinoRobotClientFactory.BridgeUrlEnvironmentVariable, server.Url);

                var result = FairinoLiveSmokeRunner.Run("192.168.58.2", 8080);

                Assert.That(result, Does.Contain("CONNECT_OK"));
                Assert.That(result, Does.Contain("client=bridge"));
                Assert.That(result, Does.Contain("sdkLoadStatus=bridge"));
                Assert.That(result, Does.Contain("BridgeSDK-1.2.3"));
                Assert.That(result, Does.Contain("joints=[10, -20, 30, 40, 50, 60]"));
            }
            finally
            {
                System.Environment.SetEnvironmentVariable(FairinoRobotClientFactory.BridgeUrlEnvironmentVariable, originalBridgeUrl);
            }
        }

        private sealed class BridgeTestServer : System.IDisposable
        {
            private readonly HttpListener listener;
            private readonly CancellationTokenSource cancellation = new();
            private readonly Task loop;

            private BridgeTestServer(HttpListener httpListener, string url)
            {
                listener = httpListener;
                Url = url;
                loop = Task.Run(HandleLoop);
            }

            public string Url { get; }

            public static BridgeTestServer Start()
            {
                var port = FindFreePort();
                var url = $"http://127.0.0.1:{port}/";
                var listener = new HttpListener();
                listener.Prefixes.Add(url);
                try
                {
                    listener.Start();
                }
                catch (HttpListenerException ex)
                {
                    Assert.Ignore($"HttpListener unavailable: {ex.Message}");
                }

                return new BridgeTestServer(listener, url.TrimEnd('/'));
            }

            public void Dispose()
            {
                cancellation.Cancel();
                listener.Stop();
                listener.Close();
                try
                {
                    loop.Wait(1000);
                }
                catch
                {
                }

                cancellation.Dispose();
            }

            private async Task HandleLoop()
            {
                while (!cancellation.IsCancellationRequested && listener.IsListening)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await listener.GetContextAsync();
                    }
                    catch
                    {
                        return;
                    }

                    var path = context.Request.Url?.AbsolutePath ?? string.Empty;
                    var json = path == "/state"
                        ? "{\"ok\":true,\"message\":\"state\",\"jointsDeg\":[10,-20,30,40,50,60],\"tcpMmDeg\":[100,200,300,1,2,3],\"mode\":0,\"motionQueueLength\":1,\"safetyCode\":0,\"realtimeStateSamplePeriodMs\":100,\"mainErrorCode\":0,\"subErrorCode\":0,\"toolId\":2,\"userId\":3,\"connected\":true,\"enabled\":false}"
                        : path == "/version"
                            ? "{\"ok\":true,\"message\":\"version\",\"sdkVersion\":\"BridgeSDK-1.2.3\",\"firmwareVersion\":\"FW\",\"softwareVersion\":\"SW\",\"controllerVersion\":\"CTRL\",\"hardwareVersion\":\"HW\"}"
                            : "{\"ok\":true,\"message\":\"ok\"}";
                    var bytes = Encoding.UTF8.GetBytes(json);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    context.Response.Close();
                }
            }

            private static int FindFreePort()
            {
                var tcp = new TcpListener(IPAddress.Loopback, 0);
                tcp.Start();
                var port = ((IPEndPoint)tcp.LocalEndpoint).Port;
                tcp.Stop();
                return port;
            }
        }

        private sealed class FakeReadbackOnlyClient : IFairinoRobotClient, IFairinoLiveClientDiagnostics
        {
            private readonly FairinoRobotState state = new(
                new[] { 1d, 2d, 3d, 4d, 5d, 6d },
                new[] { 10d, 20d, 30d, 40d, 50d, 60d },
                toolId: 1,
                userId: 1,
                isRobotEnabled: false);

            public bool IsConnected => true;
            public bool IsEnabled => false;
            public string ClientMode => "direct";
            public string SdkLoadStatus => "direct-ready";
            public string SdkVersion => "FakeSDK";
            public string SdkRuntime => "TestRuntime";
            public bool IsReadbackOnly => true;

            public FairinoResult Connect(string ip, int port) => FairinoResult.Ok("connected");
            public FairinoResult Disconnect() => FairinoResult.Ok("disconnected");
            public FairinoResult Enable() => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult Disable() => FairinoResult.Ok("disabled");
            public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent) => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult ServoJ(double[] jointPosDeg) => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult<FairinoRobotState> ReadState() => FairinoResult<FairinoRobotState>.Ok(state);
            public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent) => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult StopMotion() => FairinoResult.Ok("stopped");
            public FairinoResult<FairinoVersionInfo> GetVersion() => FairinoResult<FairinoVersionInfo>.Ok(default);
            public FairinoResult<int> GetSafetyCode() => FairinoResult<int>.Ok(0);
            public FairinoResult<int> GetRealtimeStateSamplePeriod() => FairinoResult<int>.Ok(100);
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
            public FairinoResult<FairinoGripperStatus> ReadGripperStatus() => FairinoResult<FairinoGripperStatus>.Fail(-80, "readback-only");
            public FairinoResult<FairinoGripperConfigState> ReadGripperConfig() => FairinoResult<FairinoGripperConfigState>.Fail(-80, "readback-only");
            public FairinoResult ConfigureGripper(FairinoGripperProfile profile) => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate) => FairinoResult.Fail(-80, "readback-only");
            public FairinoResult MoveGripper(FairinoGripperCommand command) => FairinoResult.Fail(-80, "readback-only");
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }
    }
}
