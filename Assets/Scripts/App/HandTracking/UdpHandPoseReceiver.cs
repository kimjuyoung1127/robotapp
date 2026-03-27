// Folder: App - External hand-tracking input payloads and adapters for RobotControl.
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace KineTutor3D.App.HandTracking
{
    /// <summary>
    /// 폰이 보내는 UDP JSON payload를 받아 main thread에서 손 샘플로 변환합니다.
    /// 1차 파일럿은 preview-only teaching 흐름을 위한 최소 수신기입니다.
    /// </summary>
    public sealed class UdpHandPoseReceiver : MonoBehaviour, IHandPoseSource
    {
        private readonly ConcurrentQueue<QueuedPacket> pendingPackets = new ConcurrentQueue<QueuedPacket>();

        [SerializeField] private int listenPort = 5005;
        [SerializeField] private bool autoStartOnEnable = true;
        [SerializeField] private bool restrictSenderIp = false;
        [SerializeField] private string allowedSenderIp = string.Empty;
        [SerializeField] private float sampleTimeoutSeconds = 0.5f;
        [SerializeField] private bool logPackets;

        private UdpClient udpClient;
        private Thread listenerThread;
        private volatile bool running;
        private HandPoseSample latestSample;
        private float lastReceiveRealtime = float.NegativeInfinity;
        private string lastSenderIp = string.Empty;

        public event Action<HandPoseSample> OnSampleReceived;

        public int ListenPort => listenPort;

        public bool IsListening => running;

        public bool RestrictSenderIp => restrictSenderIp;

        public string AllowedSenderIp => allowedSenderIp;

        public bool HasFreshSample =>
            latestSample != null && Time.realtimeSinceStartup - lastReceiveRealtime <= sampleTimeoutSeconds;

        public HandPoseSample LatestSample => latestSample;

        public float SampleTimeoutSeconds => sampleTimeoutSeconds;

        public string LastSenderIp => lastSenderIp;

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                StartListening();
            }
        }

        private void Update()
        {
            while (pendingPackets.TryDequeue(out var packet))
            {
                if (restrictSenderIp && !string.IsNullOrWhiteSpace(allowedSenderIp) &&
                    !string.Equals(packet.senderIp, allowedSenderIp, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!HandPoseSample.TryParse(packet.json, out var parsed))
                {
                    if (logPackets)
                    {
                        Debug.LogWarning($"[UdpHandPoseReceiver] Invalid payload from {packet.senderIp}: {packet.json}");
                    }

                    continue;
                }

                latestSample = parsed;
                lastSenderIp = packet.senderIp;
                lastReceiveRealtime = Time.realtimeSinceStartup;

                if (logPackets)
                {
                    Debug.Log($"[UdpHandPoseReceiver] seq={parsed.seq} tracked={parsed.tracked} x={parsed.handX:F2} y={parsed.handY:F2} pinch={parsed.pinch:F2}");
                }

                OnSampleReceived?.Invoke(parsed);
            }
        }

        private void OnDisable()
        {
            StopListening();
        }

        private void OnApplicationQuit()
        {
            StopListening();
        }

        public void StartListening()
        {
            if (running)
            {
                return;
            }

            try
            {
                udpClient = new UdpClient(listenPort);
                udpClient.Client.ReceiveTimeout = 200;
                running = true;
                listenerThread = new Thread(ListenLoop)
                {
                    IsBackground = true,
                    Name = "UdpHandPoseReceiver"
                };
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                running = false;
                Debug.LogError($"[UdpHandPoseReceiver] Failed to start on port {listenPort}: {ex.Message}");
            }
        }

        public void StopListening()
        {
            running = false;

            if (udpClient != null)
            {
                try
                {
                    udpClient.Close();
                }
                catch (ObjectDisposedException)
                {
                }

                udpClient = null;
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(500);
            }

            listenerThread = null;
        }

        public bool TryGetLatestSample(out HandPoseSample sample)
        {
            sample = HasFreshSample ? latestSample : null;
            return sample != null;
        }

        private void ListenLoop()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (running)
            {
                try
                {
                    var bytes = udpClient.Receive(ref remoteEndPoint);
                    if (bytes == null || bytes.Length == 0)
                    {
                        continue;
                    }

                    var json = Encoding.UTF8.GetString(bytes);
                    pendingPackets.Enqueue(new QueuedPacket(json, remoteEndPoint.Address.ToString()));
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut && running)
                    {
                        Debug.LogWarning($"[UdpHandPoseReceiver] Receive failed: {ex.Message}");
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (running)
                    {
                        Debug.LogWarning($"[UdpHandPoseReceiver] Listener loop error: {ex.Message}");
                    }
                }
            }
        }

        private readonly struct QueuedPacket
        {
            public readonly string json;
            public readonly string senderIp;

            public QueuedPacket(string json, string senderIp)
            {
                this.json = json;
                this.senderIp = senderIp;
            }
        }
    }
}
