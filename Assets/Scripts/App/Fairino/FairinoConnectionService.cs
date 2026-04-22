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
        private FairinoCoordContext lastCoordContext = FairinoCoordContext.Default();
        private FairinoControllerFault lastControllerFault = FairinoControllerFault.None();
        private int consecutiveErrors;
        private bool useMock = true;
        private int lastSafetyCode;
        private int lastRealtimeStateSamplePeriodMs;
        private FairinoRobotConfig.LiveDefaultsBlock liveDefaults = new FairinoRobotConfig.LiveDefaultsBlock();

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
        /// 마지막으로 읽은 safety code입니다.
        /// </summary>
        public int LastSafetyCode => lastSafetyCode;

        /// <summary>
        /// 마지막으로 읽은 실시간 상태 주기(ms)입니다.
        /// </summary>
        public int LastRealtimeStateSamplePeriodMs => lastRealtimeStateSamplePeriodMs;

        /// <summary>
        /// 마지막으로 읽은 tool/user 좌표 문맥입니다.
        /// </summary>
        public FairinoCoordContext LastCoordContext => lastCoordContext;

        /// <summary>
        /// 마지막으로 읽은 컨트롤러 fault 상태입니다.
        /// </summary>
        public FairinoControllerFault LastControllerFault => lastControllerFault;

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
        /// 초기 mock 클라이언트를 주입받아 서비스를 생성합니다.
        /// 다른 로봇 벤더의 mock 클라이언트를 사용할 때 활용합니다.
        /// </summary>
        public FairinoConnectionService(IFairinoRobotClient initialMockClient, FairinoErrorTranslator translator = null)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
            client = initialMockClient ?? new MockFairinoClient();
        }

        /// <summary>
        /// Live 연결 기본 정책을 적용합니다.
        /// </summary>
        public void ApplyLiveDefaults(FairinoRobotConfig.LiveDefaultsBlock defaults)
        {
            if (defaults != null)
            {
                liveDefaults = defaults;
            }
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
            lastCoordContext = FairinoCoordContext.Default();
            lastControllerFault = FairinoControllerFault.None();
            lastSafetyCode = 0;
            lastRealtimeStateSamplePeriodMs = 0;
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
            if (!useMock)
            {
                BestEffortInvoke(() => client.SetReconnect(
                    liveDefaults.reconnectEnabled,
                    liveDefaults.reconnectTimeoutMs,
                    liveDefaults.reconnectPeriodMs));
                BestEffortInvoke(() => client.SetRealtimeStateSamplePeriod(liveDefaults.realtimeSampleMs));
                BestEffortInvoke(client.ExitDragTeach);
                BestEffortInvoke(client.EnsureAutoMode);
            }

            RefreshAuxiliaryState();
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
            lastCoordContext = FairinoCoordContext.Default();
            lastControllerFault = FairinoControllerFault.None();
            lastSafetyCode = 0;
            lastRealtimeStateSamplePeriodMs = 0;
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
            else
            {
                RefreshAuxiliaryState();
            }

            OnEnableStateChanged?.Invoke(client.IsEnabled);
            return result;
        }

        /// <summary>
        /// 컨트롤러 모드를 설정합니다.
        /// </summary>
        public FairinoResult SetMode(int mode)
        {
            var result = client.SetMode(mode);
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

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
        /// 현재 safety code를 읽어옵니다.
        /// </summary>
        public FairinoResult<int> GetSafetyCode()
        {
            var result = client.GetSafetyCode();
            if (result.IsSuccess)
            {
                lastSafetyCode = result.Value;
            }
            else
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        /// <summary>
        /// 실시간 상태 주기를 읽어옵니다.
        /// </summary>
        public FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            var result = client.GetRealtimeStateSamplePeriod();
            if (result.IsSuccess)
            {
                lastRealtimeStateSamplePeriodMs = result.Value;
            }
            else
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        /// <summary>
        /// 실시간 상태 주기를 설정합니다.
        /// </summary>
        public FairinoResult SetRealtimeStateSamplePeriod(int periodMs)
        {
            var result = client.SetRealtimeStateSamplePeriod(periodMs);
            if (result.IsSuccess)
            {
                lastRealtimeStateSamplePeriodMs = periodMs;
            }
            else
            {
                OnError?.Invoke(result);
            }

            return result;
        }

        /// <summary>
        /// 모션 큐를 비웁니다.
        /// </summary>
        public FairinoResult ClearMotionQueue()
        {
            var result = client.ClearMotionQueue();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            return result;
        }

        /// <summary>
        /// 컨트롤러 fault를 리셋합니다.
        /// </summary>
        public FairinoResult ResetErrors()
        {
            var result = client.ResetErrors();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
                return result;
            }

            RefreshAuxiliaryState();
            return result;
        }

        public FairinoResult<FairinoGripperCapability> ProbeGripperCapability()
        {
            var result = client.ProbeGripperCapability();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        public FairinoResult<FairinoGripperStatus> ReadGripperStatus()
        {
            var result = client.ReadGripperStatus();
            if (!result.IsSuccess)
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        public FairinoResult ConfigureGripper(FairinoGripperProfile profile)
        {
            var result = client.ConfigureGripper(profile);
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            return result;
        }

        public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate)
        {
            var result = client.ActivateGripper(profile, activate);
            if (!result.IsSuccess)
            {
                OnError?.Invoke(result);
            }

            return result;
        }

        public FairinoResult MoveGripper(FairinoGripperCommand command)
        {
            var result = client.MoveGripper(command);
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
                lastSafetyCode = result.Value.SafetyCode;
                lastRealtimeStateSamplePeriodMs = result.Value.RealtimeStateSamplePeriodMs;
                lastCoordContext = new FairinoCoordContext(result.Value.ToolId, result.Value.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
                lastControllerFault = new FairinoControllerFault(result.Value.MainErrorCode, result.Value.SubErrorCode, result.Value.IsSafetyStop);
                OnStateUpdated?.Invoke(lastState);
            }
            else
            {
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
            }

            return result;
        }

        /// <summary>
        /// Mock에서 외부 수동 이동 readback을 재현하고 기존 상태 갱신 이벤트로 발행합니다.
        /// </summary>
        public FairinoResult<FairinoRobotState> SimulateExternalReadbackForDebug(double[] jointsDeg, double[] tcpPose)
        {
            if (client is not MockFairinoClient mockClient)
            {
                return FairinoResult<FairinoRobotState>.Fail(-81, "manual readback simulation은 Mock 클라이언트에서만 지원합니다.");
            }

            var applyResult = mockClient.SimulateExternalReadback(jointsDeg, tcpPose);
            if (!applyResult.IsSuccess)
            {
                OnError?.Invoke(applyResult);
                return FairinoResult<FairinoRobotState>.Fail(applyResult.ErrorCode, applyResult.Message);
            }

            var result = client.ReadState();
            if (result.IsSuccess)
            {
                lastState = result.Value;
                lastSafetyCode = result.Value.SafetyCode;
                lastRealtimeStateSamplePeriodMs = result.Value.RealtimeStateSamplePeriodMs;
                lastCoordContext = new FairinoCoordContext(result.Value.ToolId, result.Value.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
                lastControllerFault = new FairinoControllerFault(result.Value.MainErrorCode, result.Value.SubErrorCode, result.Value.IsSafetyStop);
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
                lastSafetyCode = result.Value.SafetyCode;
                lastRealtimeStateSamplePeriodMs = result.Value.RealtimeStateSamplePeriodMs;
                lastCoordContext = new FairinoCoordContext(result.Value.ToolId, result.Value.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
                lastControllerFault = new FairinoControllerFault(result.Value.MainErrorCode, result.Value.SubErrorCode, result.Value.IsSafetyStop);
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
                    lastState = FairinoRobotState.Zero();
                    lastCoordContext = FairinoCoordContext.Default();
                    lastControllerFault = FairinoControllerFault.None();
                    lastSafetyCode = 0;
                    lastRealtimeStateSamplePeriodMs = 0;
                    OnConnectionLost?.Invoke();
                    OnConnectionStateChanged?.Invoke(false);
                    OnEnableStateChanged?.Invoke(false);
                    OnStateUpdated?.Invoke(lastState);
                }
            }
        }

        private void EmitCurrentState()
        {
            var result = client.ReadState();
            if (result.IsSuccess)
            {
                lastState = result.Value;
                lastSafetyCode = result.Value.SafetyCode;
                lastRealtimeStateSamplePeriodMs = result.Value.RealtimeStateSamplePeriodMs;
                lastCoordContext = new FairinoCoordContext(result.Value.ToolId, result.Value.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
                lastControllerFault = new FairinoControllerFault(result.Value.MainErrorCode, result.Value.SubErrorCode, result.Value.IsSafetyStop);
                OnStateUpdated?.Invoke(lastState);
                return;
            }

            OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
        }

        private void RefreshAuxiliaryState()
        {
            var safetyResult = client.GetSafetyCode();
            if (safetyResult.IsSuccess)
            {
                lastSafetyCode = safetyResult.Value;
            }

            var periodResult = client.GetRealtimeStateSamplePeriod();
            if (periodResult.IsSuccess)
            {
                lastRealtimeStateSamplePeriodMs = periodResult.Value;
            }

            var contextResult = client.ReadCoordContext();
            if (contextResult.IsSuccess)
            {
                lastCoordContext = contextResult.Value;
            }

            var faultResult = client.ReadControllerFault();
            if (faultResult.IsSuccess)
            {
                lastControllerFault = faultResult.Value;
            }
        }

        private void BestEffortInvoke(System.Func<FairinoResult> action)
        {
            try
            {
                var result = action.Invoke();
                if (!result.IsSuccess)
                {
                    OnError?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(FairinoResult.Fail(-6, ex.Message));
            }
        }
    }
}
