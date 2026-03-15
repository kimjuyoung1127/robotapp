// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Mock↔Live 클라이언트 전환과 상태 폴링을 관리하는 서비스입니다.
    /// </summary>
    public sealed class FairinoConnectionService
    {
        private const int ConnectionLostThreshold = 3;

        private IFairinoRobotClient client;
        private readonly FairinoErrorTranslator errorTranslator;
        private float pollInterval = 0.1f;
        private float pollTimer;
        private FairinoRobotState lastState;
        private int consecutiveErrors;
        private bool useMock = true;

        /// <summary>
        /// 현재 사용 중인 클라이언트입니다.
        /// </summary>
        public IFairinoRobotClient Client => client;

        /// <summary>
        /// Mock 모드 여부입니다.
        /// </summary>
        public bool IsMockMode => useMock;

        /// <summary>
        /// 마지막으로 읽은 로봇 상태입니다.
        /// </summary>
        public FairinoRobotState LastState => lastState;

        /// <summary>
        /// 상태가 갱신될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<FairinoRobotState> OnStateUpdated;

        /// <summary>
        /// 에러가 발생할 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<FairinoResult> OnError;

        /// <summary>
        /// 연결 상태가 바뀔 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<bool> OnConnectionStateChanged;

        /// <summary>
        /// 서보 활성 상태가 바뀔 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<bool> OnEnableStateChanged;

        /// <summary>
        /// Mock/Live 모드가 바뀔 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action<bool> OnModeChanged;

        /// <summary>
        /// 연속 폴링 실패로 연결이 끊어진 것으로 판단될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action OnConnectionLost;

        /// <summary>
        /// 서비스를 생성합니다.
        /// </summary>
        public FairinoConnectionService(FairinoErrorTranslator translator = null)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
            client = new MockFairinoClient();
        }

        /// <summary>
        /// Mock↔Live 모드를 전환합니다.
        /// </summary>
        public void SetMockMode(bool mock)
        {
            if (useMock == mock)
            {
                return;
            }

            if (client.IsConnected)
            {
                client.Disconnect();
            }

            useMock = mock;
            client = mock
                ? (IFairinoRobotClient)new MockFairinoClient()
                : new LiveFairinoClient(errorTranslator);
            lastState = FairinoRobotState.Zero();
            pollTimer = 0f;
            OnModeChanged?.Invoke(useMock);
            OnConnectionStateChanged?.Invoke(client.IsConnected);
            OnEnableStateChanged?.Invoke(client.IsEnabled);
            OnStateUpdated?.Invoke(lastState);
        }

        /// <summary>
        /// 로봇에 연결합니다.
        /// </summary>
        public FairinoResult Connect(string ip, int port)
        {
            var result = client.Connect(ip, port);
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
                OnConnectionStateChanged?.Invoke(false);
                OnEnableStateChanged?.Invoke(client.IsEnabled);
                return result;
            }

            consecutiveErrors = 0;
            OnConnectionStateChanged?.Invoke(client.IsConnected);
            OnEnableStateChanged?.Invoke(client.IsEnabled);
            EmitCurrentState();
            return result;
        }

        /// <summary>
        /// 연결을 해제합니다.
        /// </summary>
        public FairinoResult Disconnect()
        {
            var result = client.Disconnect();
            lastState = FairinoRobotState.Zero();
            OnConnectionStateChanged?.Invoke(client.IsConnected);
            OnEnableStateChanged?.Invoke(client.IsEnabled);
            OnStateUpdated?.Invoke(lastState);
            return result;
        }

        /// <summary>
        /// 로봇 서보를 활성화합니다.
        /// </summary>
        public FairinoResult Enable()
        {
            var result = client.Enable();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            OnEnableStateChanged?.Invoke(client.IsEnabled);
            return result;
        }

        /// <summary>
        /// 로봇 서보를 비활성화합니다.
        /// </summary>
        public FairinoResult Disable()
        {
            var result = client.Disable();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            OnEnableStateChanged?.Invoke(client.IsEnabled);
            return result;
        }

        /// <summary>
        /// 현재 모션을 정지합니다.
        /// </summary>
        public FairinoResult StopMotion()
        {
            var result = client.StopMotion();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            return result;
        }

        /// <summary>
        /// 현재 로봇 상태를 읽어 반환합니다. Live 모드에서 관절 동기화용입니다.
        /// </summary>
        public FairinoResult<FairinoRobotState> SyncCurrentState()
        {
            if (!client.IsConnected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var result = client.ReadState();
            if (result.IsSuccess)
            {
                lastState = result.Value;
                OnStateUpdated?.Invoke(lastState);
            }
            else
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        /// <summary>
        /// 상태 폴링 간격을 설정합니다 (초 단위).
        /// </summary>
        public void SetPollInterval(float seconds)
        {
            pollInterval = Mathf.Max(0.05f, seconds);
        }

        /// <summary>
        /// MonoBehaviour.Update에서 호출하여 주기적으로 상태를 읽습니다.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!client.IsConnected) return;

            pollTimer += deltaTime;
            if (pollTimer < pollInterval) return;
            pollTimer = 0f;

            var result = client.ReadState();
            if (result.IsSuccess)
            {
                consecutiveErrors = 0;
                lastState = result.Value;
                OnStateUpdated?.Invoke(lastState);
            }
            else
            {
                consecutiveErrors++;
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));

                if (!useMock && consecutiveErrors >= ConnectionLostThreshold)
                {
                    consecutiveErrors = 0;
                    client.Disconnect();
                    OnConnectionLost?.Invoke();
                    OnConnectionStateChanged?.Invoke(false);
                }
            }
        }

        private void EmitCurrentState()
        {
            var result = client.ReadState();
            if (result.IsSuccess)
            {
                lastState = result.Value;
                OnStateUpdated?.Invoke(lastState);
                return;
            }

            OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
        }
    }
}
