// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KineTutor3D.App.Fairino;
using NUnit.Framework;

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
            var recorder = new Fr5LiveStateRecorder(service, () => service.LastState, null, root);

            var drift = recorder.RecordState(service.LastState);

            Assert.That(drift.severity, Is.EqualTo("ok"));
            Assert.That(File.Exists(Path.Combine(root, "latest-state.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "latest-drift.json")), Is.True);
            Assert.That(Directory.GetFiles(Path.Combine(root, "sessions"), "*-readback.ndjson").Length, Is.EqualTo(1));
        }

        [Test]
        public void Recorder_DetectsDangerDrift()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var live = new FairinoRobotState(new[] { 10d, 0d, 0d, 0d, 0d, 0d }, new[] { 20d, 0d, 0d, 0d, 0d, 0d });
            var screen = FairinoRobotState.Zero();
            var service = new FairinoConnectionService();
            var recorder = new Fr5LiveStateRecorder(service, () => screen, null, root);

            var drift = recorder.RecordState(live);

            Assert.That(drift.severity, Is.EqualTo("danger"));
            Assert.That(drift.liveBlockedReason, Does.Contain("실기 이동 차단"));
        }

        [Test]
        public void Recorder_ClearsBlockedReasonWhenDriftReturnsOk()
        {
            var root = Path.Combine(Path.GetTempPath(), "fr5-live-recorder-tests", System.Guid.NewGuid().ToString("N"));
            var liveDanger = new FairinoRobotState(new[] { 10d, 0d, 0d, 0d, 0d, 0d }, new double[6]);
            var liveOk = FairinoRobotState.Zero();
            var screen = FairinoRobotState.Zero();
            var latestReason = "seed";
            var service = new FairinoConnectionService();
            var recorder = new Fr5LiveStateRecorder(service, () => screen, value => latestReason = value, root);

            recorder.RecordState(liveDanger);
            Assert.That(latestReason, Does.Contain("실기 이동 차단"));

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
    }
}
