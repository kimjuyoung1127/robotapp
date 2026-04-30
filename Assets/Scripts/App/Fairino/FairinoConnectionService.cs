// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Mock↔Live 클라이언트 전환과 상태 폴링을 관리하는 서비스입니다.
    /// </summary>
    public sealed class FairinoConnectionService
    {
        private const int ConnectionLostThreshold = 3;
        private const float DefaultLivePollIntervalSeconds = 0.033f;
        private const float FallbackLivePollIntervalSeconds = 0.05f;
        private const int FastPollFallbackErrorThreshold = 2;
        private const int DefaultModeVerificationRetryCount = 6;
        private const int DefaultModeVerificationDelayMs = 150;

        private IFairinoRobotClient client;
        private readonly FairinoErrorTranslator errorTranslator;
        private readonly object readStateGate = new();
        private float preferredPollInterval = DefaultLivePollIntervalSeconds;
        private float pollInterval = DefaultLivePollIntervalSeconds;
        private float pollTimer;
        private FairinoRobotState lastState;
        private FairinoCoordContext lastCoordContext = FairinoCoordContext.Default();
        private FairinoControllerFault lastControllerFault = FairinoControllerFault.None();
        private int consecutiveErrors;
        private int forcedReadFailuresRemaining;
        private string forcedReadFailureMessage = "forced debug read fail";
        private bool useMock = true;
        private int lastSafetyCode;
        private int lastRealtimeStateSamplePeriodMs;
        private FairinoRobotConfig.LiveDefaultsBlock liveDefaults = new FairinoRobotConfig.LiveDefaultsBlock();
        private Task<FairinoResult<FairinoRobotState>> activePollReadTask;
        private bool pollSuspendedForDisabledRobot;
        private string pollSuspendedReason = string.Empty;

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
        /// 방금 성공한 명령의 target을 다음 live gate baseline으로 반영합니다.
        /// readback이 늦게 따라오는 짧은 구간의 tiny motion 연속 조작 안정화에 사용합니다.
        /// </summary>
        public void SeedLastState(FairinoRobotState state)
        {
            lastState = state;
        }

        /// <summary>
        /// 마지막으로 읽은 safety code입니다.
        /// </summary>
        public int LastSafetyCode => lastSafetyCode;

        /// <summary>
        /// 마지막으로 읽은 실시간 상태 주기(ms)입니다.
        /// </summary>
        public int LastRealtimeStateSamplePeriodMs => lastRealtimeStateSamplePeriodMs;

        /// <summary>
        /// 현재 Unity 폴링 간격(초)입니다.
        /// </summary>
        public float CurrentPollIntervalSeconds => pollInterval;
        public int ConsecutiveReadErrors => consecutiveErrors;
        public int ForcedReadFailuresRemaining => forcedReadFailuresRemaining;
        public bool IsPollSuspendedForDisabledRobot => pollSuspendedForDisabledRobot;
        public string PollSuspendedReason => pollSuspendedReason;

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

        private FairinoConnectionService(IFairinoRobotClient initialClient, FairinoErrorTranslator translator, bool initialMockMode)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
            client = initialClient ?? new MockFairinoClient();
            useMock = initialMockMode;
            if (!initialMockMode)
            {
                pollInterval = preferredPollInterval;
            }
        }

        /// <summary>
        /// Live 연결 기본 정책을 적용합니다.
        /// </summary>
        public void ApplyLiveDefaults(FairinoRobotConfig.LiveDefaultsBlock defaults)
        {
            if (defaults != null)
            {
                liveDefaults = defaults;
                var configuredPollInterval = Mathf.Max(DefaultLivePollIntervalSeconds, defaults.realtimeSampleMs / 1000f);
                preferredPollInterval = configuredPollInterval;
                pollInterval = configuredPollInterval;
                pollTimer = 0f;
            }
        }

        /// <summary>
        /// Mock↔Live 모드를 전환합니다.
        /// </summary>
        public void SetMockMode(bool mock, bool preferMotionCapableDirect = false)
        {
            var currentlyUsingMockClient = client is MockFairinoClient;
            if (useMock == mock && currentlyUsingMockClient == mock)
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
                : FairinoRobotClientFactory.CreateLive(errorTranslator, preferMotionCapableDirect);
            if (mock)
            {
                preferredPollInterval = DefaultLivePollIntervalSeconds;
                pollInterval = DefaultLivePollIntervalSeconds;
            }
            else
            {
                pollInterval = preferredPollInterval;
            }

            lastState = FairinoRobotState.Zero();
            lastCoordContext = FairinoCoordContext.Default();
            lastControllerFault = FairinoControllerFault.None();
            lastSafetyCode = 0;
            lastRealtimeStateSamplePeriodMs = 0;
            pollTimer = 0f;
            activePollReadTask = null;
            ClearPollSuspension();
            OnModeChanged?.Invoke(useMock);
            OnConnectionStateChanged?.Invoke(client.IsConnected);
            OnEnableStateChanged?.Invoke(client.IsEnabled);
            OnStateUpdated?.Invoke(lastState);
        }

        public FairinoResult<FairinoConnectionService> CreateMotionSiblingSession()
        {
            if (useMock)
            {
                return FairinoResult<FairinoConnectionService>.Fail(-84, "mock 세션에서는 motion-capable live sibling을 만들 수 없다.");
            }

            IFairinoRobotClient motionClient = null;
            if (client is IFairinoMotionSessionProvider provider
                && provider.TryGetMotionCapableClient(out var providedClient)
                && providedClient != null)
            {
                motionClient = providedClient;
            }
            else if (client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false })
            {
                motionClient = client;
            }

            if (motionClient == null)
            {
                return FairinoResult<FairinoConnectionService>.Fail(-85, "현재 live 세션에서 재사용 가능한 motion-capable client를 찾지 못했다.");
            }

            var sibling = new FairinoConnectionService(motionClient, errorTranslator, initialMockMode: false)
            {
                liveDefaults = liveDefaults,
                preferredPollInterval = preferredPollInterval,
                pollInterval = preferredPollInterval,
                lastState = lastState,
                lastCoordContext = lastCoordContext,
                lastControllerFault = lastControllerFault,
                lastSafetyCode = lastSafetyCode,
                lastRealtimeStateSamplePeriodMs = lastRealtimeStateSamplePeriodMs,
            };
            return FairinoResult<FairinoConnectionService>.Ok(sibling, "기존 live 세션 재사용 준비 완료");
        }

        /// <summary>
        /// 로봇에 연결합니다.
        /// </summary>
        public FairinoResult Connect(
            string ip,
            int port,
            bool applyLiveBringupPolicies = true,
            bool emitConnectionStateChanged = true,
            bool emitEnableStateChanged = true,
            bool emitInitialState = true,
            bool emitError = true)
        {
            var result = client.Connect(ip, port);
            if (!result.IsSuccess)
            {
                if (emitError)
                {
                    OnError?.Invoke(result);
                }

                if (emitConnectionStateChanged)
                {
                    OnConnectionStateChanged?.Invoke(false);
                }

                if (emitEnableStateChanged)
                {
                    OnEnableStateChanged?.Invoke(client.IsEnabled);
                }

                return result;
            }

            consecutiveErrors = 0;
            pollInterval = preferredPollInterval;
            pollTimer = 0f;
            activePollReadTask = null;
            ClearPollSuspension();
            if (applyLiveBringupPolicies
                && !useMock
                && client is not IFairinoLiveClientDiagnostics { IsReadbackOnly: true })
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
            if (emitConnectionStateChanged)
            {
                OnConnectionStateChanged?.Invoke(client.IsConnected);
            }

            if (emitEnableStateChanged)
            {
                OnEnableStateChanged?.Invoke(client.IsEnabled);
            }

            if (emitInitialState)
            {
                EmitCurrentState();
            }

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
            activePollReadTask = null;
            ClearPollSuspension();
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
                ClearPollSuspension();
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

        public FairinoResult<FairinoRobotState> RequestControllerModeWithVerification(
            int mode,
            bool exitDragTeachFirst = false,
            int retryCount = DefaultModeVerificationRetryCount,
            int retryDelayMs = DefaultModeVerificationDelayMs)
        {
            if (!client.IsConnected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            FairinoResult lastFailure = FairinoResult.Ok();
            if (exitDragTeachFirst)
            {
                var dragResult = client.ExitDragTeach();
                if (!dragResult.IsSuccess)
                {
                    lastFailure = dragResult;
                    OnError?.Invoke(dragResult);
                }
            }

            var modeResult = client.SetMode(mode);
            if (!modeResult.IsSuccess)
            {
                OnError?.Invoke(modeResult);
                return FairinoResult<FairinoRobotState>.Fail(modeResult.ErrorCode, modeResult.Message);
            }

            lastFailure = modeResult;
            FairinoRobotState lastObservedState = LastState;
            var attempts = Mathf.Max(1, retryCount);
            for (var attempt = 0; attempt < attempts; attempt++)
            {
                if (attempt > 0 && retryDelayMs > 0)
                {
                    Thread.Sleep(retryDelayMs);
                }

                var stateResult = SyncCurrentState();
                if (!stateResult.IsSuccess)
                {
                    lastFailure = new FairinoResult(stateResult.ErrorCode, stateResult.Message);
                    continue;
                }

                lastObservedState = stateResult.Value;
                if (MatchesRequestedControllerMode(lastObservedState, mode))
                {
                    return FairinoResult<FairinoRobotState>.Ok(
                        lastObservedState,
                        BuildModeVerificationSuccessMessage(mode, lastObservedState));
                }
            }

            var failCode = lastFailure.IsSuccess ? -7 : lastFailure.ErrorCode;
            return FairinoResult<FairinoRobotState>.Fail(
                failCode,
                BuildModeVerificationFailureMessage(mode, lastObservedState, lastFailure));
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

        public FairinoResult<FairinoGripperConfigState> ReadGripperConfig()
        {
            var result = client.ReadGripperConfig();
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
        public FairinoResult<FairinoRobotState> SyncCurrentState(
            bool emitStateUpdated = true,
            bool emitError = true)
        {
            if (!client.IsConnected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            ClearPollSuspension();
            var result = ReadStateExclusive(skipIfBusy: false, busyErrorCode: -992, busyMessage: "현재 위치 읽기 대기 중입니다.");
            if (result.IsSuccess)
            {
                lastState = result.Value;
                lastSafetyCode = result.Value.SafetyCode;
                lastRealtimeStateSamplePeriodMs = result.Value.RealtimeStateSamplePeriodMs;
                lastCoordContext = new FairinoCoordContext(result.Value.ToolId, result.Value.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
                lastControllerFault = new FairinoControllerFault(result.Value.MainErrorCode, result.Value.SubErrorCode, result.Value.IsSafetyStop);
                if (emitStateUpdated)
                {
                    OnStateUpdated?.Invoke(lastState);
                }
            }
            else
            {
                MaybeSuspendPollingForDisabledRobot(result);
                if (emitError)
                {
                    OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));
                }
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
            preferredPollInterval = Mathf.Max(DefaultLivePollIntervalSeconds, seconds);
            pollInterval = preferredPollInterval;
            pollTimer = 0f;
        }

        public void RequestImmediatePoll()
        {
            ClearPollSuspension();
            pollTimer = pollInterval;
        }

        /// <summary>
        /// Debug 검증용으로 다음 ReadState 실패 횟수를 강제로 주입합니다.
        /// </summary>
        public void ForceNextReadFailuresForDebug(int count, string message = null)
        {
            forcedReadFailuresRemaining = Mathf.Max(0, count);
            forcedReadFailureMessage = string.IsNullOrWhiteSpace(message) ? "forced debug read fail" : message.Trim();
        }

        /// <summary>
        /// MonoBehaviour.Update에서 호출하여 주기적으로 상태를 읽습니다.
        /// </summary>
        public void Tick(float deltaTime)
        {
            CompletePendingPollReadIfReady();
            if (!client.IsConnected) return;
            if (activePollReadTask != null) return;
            if (pollSuspendedForDisabledRobot) return;

            pollTimer += deltaTime;
            if (pollTimer < pollInterval) return;
            pollTimer = 0f;

            if (forcedReadFailuresRemaining > 0)
            {
                forcedReadFailuresRemaining--;
                HandlePollReadResult(FairinoResult<FairinoRobotState>.Fail(-991, forcedReadFailureMessage));
                return;
            }

            activePollReadTask = Task.Run(
                () => ReadStateExclusive(skipIfBusy: true, busyErrorCode: -993, busyMessage: "poll skipped: readback busy"));
        }

        private void CompletePendingPollReadIfReady()
        {
            var task = activePollReadTask;
            if (task == null || !task.IsCompleted)
            {
                return;
            }

            activePollReadTask = null;
            if (task.IsFaulted)
            {
                var message = task.Exception?.GetBaseException().Message ?? "poll read task faulted";
                HandlePollReadResult(FairinoResult<FairinoRobotState>.Fail(-994, message));
                return;
            }

            HandlePollReadResult(task.Result);
        }

        private void HandlePollReadResult(FairinoResult<FairinoRobotState> result)
        {
            if (result.ErrorCode == -993)
            {
                return;
            }

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
                MaybeSuspendPollingForDisabledRobot(result);
                consecutiveErrors++;
                MaybeFallbackPollInterval();
                OnError?.Invoke(new FairinoResult(result.ErrorCode, result.Message));

                if (pollSuspendedForDisabledRobot)
                {
                    OnEnableStateChanged?.Invoke(false);
                    return;
                }

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

        private void MaybeFallbackPollInterval()
        {
            if (useMock || pollInterval >= FallbackLivePollIntervalSeconds)
            {
                return;
            }

            if (consecutiveErrors < FastPollFallbackErrorThreshold)
            {
                return;
            }

            pollInterval = FallbackLivePollIntervalSeconds;
            pollTimer = 0f;
        }

        private void MaybeSuspendPollingForDisabledRobot(FairinoResult<FairinoRobotState> result)
        {
            if (useMock || result.IsSuccess)
            {
                return;
            }

            if (!IsDisabledRobotReadFailure(result.Message))
            {
                return;
            }

            pollSuspendedForDisabledRobot = true;
            pollSuspendedReason = result.Message ?? string.Empty;
            activePollReadTask = null;
            pollTimer = 0f;
        }

        private void ClearPollSuspension()
        {
            pollSuspendedForDisabledRobot = false;
            pollSuspendedReason = string.Empty;
        }

        private static bool IsDisabledRobotReadFailure(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return message.Contains("비활성 상태", StringComparison.OrdinalIgnoreCase)
                || message.Contains("Enable 버튼", StringComparison.OrdinalIgnoreCase);
        }

        private void EmitCurrentState()
        {
            var result = ReadStateExclusive(skipIfBusy: false, busyErrorCode: -992, busyMessage: "초기 현재 위치 읽기 대기 중입니다.");
            if (result.IsSuccess)
            {
                UpdateCachedState(result.Value);
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

        private FairinoResult<FairinoRobotState> ReadStateExclusive(bool skipIfBusy, int busyErrorCode, string busyMessage)
        {
            var lockTaken = false;
            try
            {
                if (skipIfBusy)
                {
                    lockTaken = Monitor.TryEnter(readStateGate);
                    if (!lockTaken)
                    {
                        return FairinoResult<FairinoRobotState>.Fail(busyErrorCode, busyMessage);
                    }
                }
                else
                {
                    Monitor.Enter(readStateGate, ref lockTaken);
                }

                return client.ReadState();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(readStateGate);
                }
            }
        }

        private void UpdateCachedState(FairinoRobotState state)
        {
            lastState = state;
            lastSafetyCode = state.SafetyCode;
            lastRealtimeStateSamplePeriodMs = state.RealtimeStateSamplePeriodMs;
            lastCoordContext = new FairinoCoordContext(state.ToolId, state.UserId, lastCoordContext.ToolPose, lastCoordContext.WObjPose);
            lastControllerFault = new FairinoControllerFault(state.MainErrorCode, state.SubErrorCode, state.IsSafetyStop);
        }

        private static bool MatchesRequestedControllerMode(FairinoRobotState state, int requestedMode)
        {
            if (requestedMode == 0)
            {
                return state.RobotMode == 0 && !state.IsInDragTeach;
            }

            return state.RobotMode == requestedMode;
        }

        private static string BuildModeVerificationSuccessMessage(int mode, FairinoRobotState state)
        {
            return $"모드 truth 확인 완료 · requested={DescribeControllerMode(mode)} · actual={DescribeControllerMode(state.RobotMode)} · drag={(state.IsInDragTeach ? "on" : "off")} · enabled={(state.IsRobotEnabled ? "on" : "off")}";
        }

        private static string BuildModeVerificationFailureMessage(int requestedMode, FairinoRobotState state, FairinoResult lastFailure)
        {
            var failureSummary = lastFailure.IsSuccess ? "readback mismatch" : lastFailure.Message;
            return $"모드 전환 확인 실패 · requested={DescribeControllerMode(requestedMode)} · actual={DescribeControllerMode(state.RobotMode)} · drag={(state.IsInDragTeach ? "on" : "off")} · enabled={(state.IsRobotEnabled ? "on" : "off")} · reason={failureSummary}";
        }

        private static string DescribeControllerMode(int mode)
        {
            return mode switch
            {
                0 => "auto(0)",
                1 => "manual(1)",
                _ => $"mode({mode})",
            };
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
