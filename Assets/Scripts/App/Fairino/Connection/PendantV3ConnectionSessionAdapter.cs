// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Templates;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FairinoConnectionService를 Pendant V3가 소비하는 세션 상태로 변환합니다.
    /// </summary>
    [DefaultExecutionOrder(-920)]
    public sealed class PendantV3ConnectionSessionAdapter : MonoBehaviour
    {
        private const string DefaultRobotId = "FAIRINO_FR5";
        private const float ReconnectIntervalSeconds = 3f;
        private const int MaxReconnectAttempts = 10;

        [SerializeField] private bool mockModeByDefault = false;

        private FairinoConnectionService connectionService;
        private RobotControlTemplateDefinition templateDefinition;
        private FairinoRobotConfig robotConfig;
        private string robotId = DefaultRobotId;
        private string currentIpAddress = string.Empty;
        private int currentPort;
        private bool hasSynced;
        private bool reconnectActive;
        private bool reconnectFailed;
        private int reconnectAttempt;
        private float reconnectSecondsUntilRetry;
        private string reconnectFailureSummary = string.Empty;
        private string lastErrorSummary = string.Empty;
        private string actualMoveBlockReason = "Live Arm을 먼저 켜라.";
        private bool liveArmActive;
        private bool popupBlockActive;
        private bool debugOverrideActive;
        private PendantV3ConnectionDisplayKind debugDisplayKind = PendantV3ConnectionDisplayKind.ConnectedServoOff;
        private bool isInitialized;

        public event Action<PendantV3ConnectionSessionState> StateChanged;

        public PendantV3ConnectionSessionState CurrentState { get; private set; } = PendantV3ConnectionSessionState.DefaultDisconnected();

        public FairinoRobotState CurrentRobotState => connectionService != null
            ? connectionService.LastState
            : FairinoRobotState.Zero();

        public bool IsLiveArmActive => liveArmActive;

        public bool IsActualMoveAllowed => CurrentState.ActualMoveAllowed;

        public string ActualMoveBlockReason => CurrentState.ActualMoveBlockReason;

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={isInitialized}; robotId={robotId}; ip={currentIpAddress}; port={currentPort}; {CurrentState.ToDebugSummary()}";
        }

        public void ApplyServoEnablePolicy()
        {
            if (debugOverrideActive)
            {
                SetDebugDisplayKind(PendantV3ConnectionDisplayKind.ConnectedUnsynced);
                return;
            }

            if (!TryInitialize())
            {
                return;
            }

            var result = connectionService.Enable();
            if (result.IsSuccess)
            {
                hasSynced = false;
                reconnectFailed = false;
                reconnectFailureSummary = string.Empty;
            }
            else
            {
                lastErrorSummary = result.Message;
            }

            RefreshState();
        }

        public void ApplySyncPolicy()
        {
            if (debugOverrideActive)
            {
                SetDebugDisplayKind(PendantV3ConnectionDisplayKind.ReadyToJog);
                return;
            }

            if (!TryInitialize())
            {
                return;
            }

            var result = connectionService.SyncCurrentState();
            if (result.IsSuccess)
            {
                hasSynced = true;
                reconnectFailed = false;
                reconnectFailureSummary = string.Empty;
            }
            else
            {
                lastErrorSummary = result.Message;
            }

            RefreshState();
        }

        public void ApplyRunPolicy()
        {
            if (debugOverrideActive)
            {
                SetDebugDisplayKind(PendantV3ConnectionDisplayKind.ReadyToJog);
                return;
            }

            RefreshState();
        }

        public void ApplyResetErrorPolicy()
        {
            if (debugOverrideActive)
            {
                SetDebugDisplayKind(PendantV3ConnectionDisplayKind.ConnectedServoOff);
                return;
            }

            if (!TryInitialize())
            {
                return;
            }

            var resetResult = connectionService.ResetErrors();
            if (resetResult.IsSuccess)
            {
                connectionService.Disable();
                hasSynced = false;
                reconnectFailed = false;
                reconnectFailureSummary = string.Empty;
            }
            else
            {
                lastErrorSummary = resetResult.Message;
            }

            RefreshState();
        }

        public void ConnectNow()
        {
            debugOverrideActive = false;
            reconnectActive = false;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = 0f;
            reconnectFailureSummary = string.Empty;
            hasSynced = false;
            liveArmActive = false;

            if (!TryInitialize())
            {
                return;
            }

            var result = connectionService.Connect(currentIpAddress, currentPort);
            if (!result.IsSuccess)
            {
                lastErrorSummary = result.Message;
            }
            else
            {
                lastErrorSummary = string.Empty;
            }

            RefreshState();
        }

        public void DisconnectNow()
        {
            debugOverrideActive = false;
            reconnectActive = false;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = 0f;
            reconnectFailureSummary = string.Empty;
            hasSynced = false;
            liveArmActive = false;

            if (!TryInitialize())
            {
                return;
            }

            connectionService.Disconnect();
            RefreshState();
        }

        public void SetDebugDisplayKind(PendantV3ConnectionDisplayKind displayKind)
        {
            debugOverrideActive = true;
            debugDisplayKind = displayKind;
            reconnectActive = displayKind == PendantV3ConnectionDisplayKind.AutoReconnect;
            reconnectFailed = false;
            reconnectAttempt = reconnectActive ? 1 : 0;
            reconnectSecondsUntilRetry = reconnectActive ? ReconnectIntervalSeconds : 0f;
            reconnectFailureSummary = string.Empty;
            liveArmActive = false;
            RefreshState();
        }

        public void ClearDebugOverride()
        {
            debugOverrideActive = false;
            RefreshState();
        }

        public void TriggerConnectionLostForDebug()
        {
            if (!TryInitialize())
            {
                return;
            }

            debugOverrideActive = false;
            connectionService.Disconnect();
            reconnectActive = true;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = ReconnectIntervalSeconds;
            reconnectFailureSummary = string.Empty;
            lastErrorSummary = "디버그 연결 끊김";
            hasSynced = false;
            liveArmActive = false;
            RefreshState();
        }

        public void AdvanceReconnectTickForDebug(float seconds)
        {
            if (!TryInitialize())
            {
                return;
            }

            if (!reconnectActive)
            {
                RefreshState();
                return;
            }

            reconnectSecondsUntilRetry -= Mathf.Max(0f, seconds);
            if (reconnectSecondsUntilRetry <= 0f)
            {
                AttemptReconnect();
                return;
            }

            RefreshState();
        }

        public void CompleteReconnectForDebug(bool success)
        {
            if (!TryInitialize())
            {
                return;
            }

            debugOverrideActive = false;
            reconnectActive = false;
            reconnectSecondsUntilRetry = 0f;
            if (success)
            {
                reconnectFailed = false;
                reconnectFailureSummary = string.Empty;
                hasSynced = false;
                liveArmActive = false;
                var result = connectionService.Connect(currentIpAddress, currentPort);
                lastErrorSummary = result.IsSuccess ? string.Empty : result.Message;
            }
            else
            {
                connectionService.Disconnect();
                reconnectFailed = true;
                reconnectAttempt = MaxReconnectAttempts;
                reconnectFailureSummary = "자동 재연결 실패. 수동 연결을 시도해라.";
                lastErrorSummary = "자동 재연결 실패";
                liveArmActive = false;
            }

            RefreshState();
        }

        public void SetMockMode(bool useMockMode)
        {
            mockModeByDefault = useMockMode;
            if (!TryInitialize())
            {
                return;
            }

            liveArmActive = false;
            debugOverrideActive = false;
            connectionService.SetMockMode(useMockMode);
            hasSynced = false;
            reconnectActive = false;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = 0f;
            reconnectFailureSummary = string.Empty;
            ConnectBaseline();
            RefreshState();
        }

        public bool SetLiveArmState(bool armed)
        {
            if (!armed)
            {
                liveArmActive = false;
                RefreshState();
                return true;
            }

            if (!CanArmLive(out _))
            {
                liveArmActive = false;
                RefreshState();
                return false;
            }

            liveArmActive = true;
            RefreshState();
            return true;
        }

        public void SetPopupBlockActive(bool active)
        {
            popupBlockActive = active;
            if (active)
            {
                liveArmActive = false;
            }

            RefreshState();
        }

        public bool CanRunActualMotion(out string reason)
        {
            if (connectionService == null)
            {
                reason = "연결 세션이 아직 준비되지 않았다.";
                return false;
            }

            if (connectionService.IsMockMode)
            {
                reason = "Mock 모드라 실제 이동을 잠가 둔다.";
                return false;
            }

            if (!liveArmActive)
            {
                reason = "Live Arm을 먼저 켜라.";
                return false;
            }

            if (!CurrentState.IsConnected)
            {
                reason = "실기 연결이 아직 안 붙었다.";
                return false;
            }

            if (!CurrentState.IsEnabled)
            {
                reason = "서보가 아직 OFF 상태다.";
                return false;
            }

            if (CurrentState.ReconnectActive)
            {
                reason = "재연결 중에는 실제 이동을 잠근다.";
                return false;
            }

            if (CurrentState.ReconnectFailed)
            {
                reason = "자동 복구가 실패했으니 수동 연결부터 다시 해라.";
                return false;
            }

            if (CurrentState.DisplayKind == PendantV3ConnectionDisplayKind.Fault)
            {
                reason = "Fault 상태라 실제 이동을 잠근다.";
                return false;
            }

            if (popupBlockActive)
            {
                reason = "팝업 확인 중에는 실제 이동을 잠근다.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void OnEnable()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            UnbindService();
            isInitialized = false;
        }

        private void Update()
        {
            if (!TryInitialize() || debugOverrideActive)
            {
                return;
            }

            connectionService.Tick(Time.unscaledDeltaTime);
            if (!reconnectActive)
            {
                return;
            }

            reconnectSecondsUntilRetry = Mathf.Max(0f, reconnectSecondsUntilRetry - Time.unscaledDeltaTime);
            if (reconnectSecondsUntilRetry <= 0f)
            {
                AttemptReconnect();
                return;
            }

            RefreshState();
        }

        private bool TryInitialize()
        {
            if (isInitialized && connectionService != null)
            {
                return true;
            }

            robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrWhiteSpace(robotId))
            {
                robotId = DefaultRobotId;
            }

            templateDefinition = RobotControlFactory.Create(robotId);
            if (templateDefinition == null)
            {
                return false;
            }

            connectionService = templateDefinition.ConnectionServiceFactory?.Invoke(new FairinoErrorTranslator());
            if (connectionService == null)
            {
                return false;
            }

            robotConfig = FairinoRobotConfig.Load(templateDefinition.ConfigResourceName) ?? templateDefinition.FallbackConfigFactory?.Invoke();
            if (robotConfig == null)
            {
                return false;
            }

            currentIpAddress = robotConfig.defaultIp;
            currentPort = robotConfig.defaultPort;
            hasSynced = false;
            reconnectActive = false;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = 0f;
            reconnectFailureSummary = string.Empty;
            lastErrorSummary = string.Empty;
            actualMoveBlockReason = mockModeByDefault ? "Mock 모드라 실제 이동을 잠가 둔다." : "Live Arm을 먼저 켜라.";
            liveArmActive = false;
            popupBlockActive = false;
            debugOverrideActive = false;
            connectionService.ApplyLiveDefaults(robotConfig.liveDefaults);
            connectionService.SetMockMode(mockModeByDefault);
            BindService();
            isInitialized = true;
            ConnectBaseline();
            RefreshState();
            return true;
        }

        private void ConnectBaseline()
        {
            if (string.IsNullOrWhiteSpace(currentIpAddress) || currentPort <= 0)
            {
                return;
            }

            var result = connectionService.Connect(currentIpAddress, currentPort);
            if (!result.IsSuccess)
            {
                lastErrorSummary = result.Message;
            }
            else
            {
                lastErrorSummary = string.Empty;
            }
        }

        private void BindService()
        {
            connectionService.OnStateUpdated += HandleStateUpdated;
            connectionService.OnConnectionStateChanged += HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged += HandleEnableStateChanged;
            connectionService.OnConnectionLost += HandleConnectionLost;
            connectionService.OnError += HandleError;
            connectionService.OnModeChanged += HandleModeChanged;
        }

        private void UnbindService()
        {
            if (connectionService == null)
            {
                return;
            }

            connectionService.OnStateUpdated -= HandleStateUpdated;
            connectionService.OnConnectionStateChanged -= HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged -= HandleEnableStateChanged;
            connectionService.OnConnectionLost -= HandleConnectionLost;
            connectionService.OnError -= HandleError;
            connectionService.OnModeChanged -= HandleModeChanged;
        }

        private void HandleStateUpdated(FairinoRobotState state)
        {
            lastErrorSummary = string.Empty;
            RefreshState();
        }

        private void HandleConnectionStateChanged(bool isConnected)
        {
            if (!isConnected && !reconnectActive)
            {
                hasSynced = false;
            }

            RefreshState();
        }

        private void HandleEnableStateChanged(bool isEnabled)
        {
            if (!isEnabled)
            {
                hasSynced = false;
            }

            RefreshState();
        }

        private void HandleConnectionLost()
        {
            reconnectActive = true;
            reconnectFailed = false;
            reconnectAttempt = 0;
            reconnectSecondsUntilRetry = ReconnectIntervalSeconds;
            reconnectFailureSummary = string.Empty;
            hasSynced = false;
            liveArmActive = false;
            RefreshState();
        }

        private void HandleError(FairinoResult result)
        {
            lastErrorSummary = result.Message;
            RefreshState();
        }

        private void HandleModeChanged(bool useMockMode)
        {
            liveArmActive = false;
            RefreshState();
        }

        private void AttemptReconnect()
        {
            reconnectAttempt++;
            reconnectSecondsUntilRetry = 0f;
            var result = connectionService.Connect(currentIpAddress, currentPort);
            if (result.IsSuccess)
            {
                reconnectActive = false;
                reconnectFailed = false;
                reconnectFailureSummary = string.Empty;
                hasSynced = false;
                liveArmActive = false;
                lastErrorSummary = string.Empty;
                RefreshState();
                return;
            }

            lastErrorSummary = result.Message;
            if (reconnectAttempt >= MaxReconnectAttempts)
            {
                reconnectActive = false;
                reconnectFailed = true;
                reconnectFailureSummary = "자동 재연결 실패. 수동 연결을 시도해라.";
                liveArmActive = false;
            }
            else
            {
                reconnectSecondsUntilRetry = ReconnectIntervalSeconds;
            }

            RefreshState();
        }

        private void RefreshState()
        {
            var nextState = BuildState();
            if (CurrentState.Equals(nextState))
            {
                return;
            }

            CurrentState = nextState;
            StateChanged?.Invoke(CurrentState);
        }

        private PendantV3ConnectionSessionState BuildState()
        {
            if (debugOverrideActive)
            {
                return BuildDebugState();
            }

            if (connectionService == null)
            {
                return PendantV3ConnectionSessionState.DefaultDisconnected();
            }

            var fault = connectionService.LastControllerFault;
            var isConnected = connectionService.Client.IsConnected;
            var isEnabled = connectionService.Client.IsEnabled;
            var isMockMode = connectionService.IsMockMode;
            var actualMoveAllowed = CanRunActualMotion(out var blockReason);
            actualMoveBlockReason = blockReason;
            var kind = reconnectActive
                ? PendantV3ConnectionDisplayKind.AutoReconnect
                : fault.HasBlockingFault
                    ? PendantV3ConnectionDisplayKind.Fault
                    : !isConnected
                        ? PendantV3ConnectionDisplayKind.Disconnected
                        : !isEnabled
                            ? PendantV3ConnectionDisplayKind.ConnectedServoOff
                            : !hasSynced
                                ? PendantV3ConnectionDisplayKind.ConnectedUnsynced
                                : PendantV3ConnectionDisplayKind.ReadyToJog;

            if (reconnectFailed && !isConnected && !reconnectActive)
            {
                kind = PendantV3ConnectionDisplayKind.Disconnected;
            }

            return new PendantV3ConnectionSessionState(
                kind,
                isConnected,
                isEnabled,
                isMockMode,
                liveArmActive,
                actualMoveAllowed,
                hasSynced,
                connectionService.LastCoordContext.ToolId,
                connectionService.LastCoordContext.UserId,
                connectionService.LastSafetyCode,
                isConnected ? "연결됨" : "미연결",
                isMockMode ? "수동 / Mock" : "수동 / Live",
                isEnabled ? "ON" : "OFF",
                DescribeMotionSummary(kind),
                liveArmActive ? "Armed" : "Disarmed",
                actualMoveBlockReason,
                DescribeSafetySummary(connectionService.LastSafetyCode, fault, reconnectActive, reconnectFailed),
                DescribeFaultSummary(fault, reconnectFailed),
                currentIpAddress,
                reconnectAttempt,
                MaxReconnectAttempts,
                reconnectSecondsUntilRetry,
                reconnectActive,
                reconnectFailed,
                reconnectFailureSummary,
                lastErrorSummary);
        }

        private PendantV3ConnectionSessionState BuildDebugState()
        {
            var isConnected = debugDisplayKind is not PendantV3ConnectionDisplayKind.Disconnected and not PendantV3ConnectionDisplayKind.AutoReconnect;
            var isEnabled = debugDisplayKind is PendantV3ConnectionDisplayKind.ConnectedUnsynced or PendantV3ConnectionDisplayKind.ReadyToJog;
            var isFault = debugDisplayKind == PendantV3ConnectionDisplayKind.Fault;
            var isAutoReconnect = debugDisplayKind == PendantV3ConnectionDisplayKind.AutoReconnect;
            return new PendantV3ConnectionSessionState(
                debugDisplayKind,
                isConnected,
                isEnabled,
                true,
                false,
                false,
                debugDisplayKind == PendantV3ConnectionDisplayKind.ReadyToJog,
                1,
                0,
                isFault ? 203 : 0,
                isConnected ? "연결됨" : "미연결",
                "수동 / Mock",
                isEnabled ? "ON" : "OFF",
                DescribeMotionSummary(debugDisplayKind),
                "Disarmed",
                "Mock 디버그 상태에서는 실제 이동을 잠근다.",
                isAutoReconnect
                    ? "자동 재연결 진행 중"
                    : isFault
                        ? "안전 정지"
                        : "정상",
                isFault ? "F203" : "없음",
                currentIpAddress,
                reconnectAttempt,
                MaxReconnectAttempts,
                reconnectSecondsUntilRetry,
                isAutoReconnect,
                reconnectFailed,
                reconnectFailureSummary,
                lastErrorSummary);
        }

        private static string DescribeMotionSummary(PendantV3ConnectionDisplayKind kind)
        {
            return kind switch
            {
                PendantV3ConnectionDisplayKind.ConnectedUnsynced => "미동기화",
                PendantV3ConnectionDisplayKind.ReadyToJog => "정지",
                PendantV3ConnectionDisplayKind.Fault => "Fault",
                PendantV3ConnectionDisplayKind.AutoReconnect => "재연결 대기",
                _ => "대기",
            };
        }

        private static string DescribeSafetySummary(int safetyCode, FairinoControllerFault fault, bool reconnectActive, bool reconnectFailed)
        {
            if (fault.IsSafetyStop)
            {
                return "정지";
            }

            if (reconnectActive)
            {
                return "재연결 중";
            }

            if (reconnectFailed)
            {
                return "수동 연결 필요";
            }

            return safetyCode == 0 ? "정상" : $"Safety {safetyCode}";
        }

        private static string DescribeFaultSummary(FairinoControllerFault fault, bool reconnectFailed)
        {
            if (fault.MainCode != 0 || fault.SubCode != 0)
            {
                return $"F{fault.MainCode}/{fault.SubCode}";
            }

            return reconnectFailed ? "통신 복구 실패" : "없음";
        }

        private bool CanArmLive(out string reason)
        {
            if (connectionService == null)
            {
                reason = "연결 세션이 아직 준비되지 않았다.";
                return false;
            }

            if (connectionService.IsMockMode)
            {
                reason = "Mock 모드에서는 Live Arm을 켤 수 없다.";
                return false;
            }

            if (!connectionService.Client.IsConnected)
            {
                reason = "실기 연결부터 먼저 붙여라.";
                return false;
            }

            if (!connectionService.Client.IsEnabled)
            {
                reason = "서보를 먼저 켜라.";
                return false;
            }

            if (reconnectActive || reconnectFailed)
            {
                reason = "재연결 상태에서는 arm을 잠근다.";
                return false;
            }

            if (connectionService.LastControllerFault.HasBlockingFault)
            {
                reason = "Fault 상태를 먼저 풀어라.";
                return false;
            }

            if (popupBlockActive)
            {
                reason = "팝업을 먼저 닫아라.";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}
