// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    internal enum RobotControlV3RuntimeStatusKind
    {
        Disconnected,
        ConnectedServoOff,
        ConnectedUnsynced,
        ReadyToJog,
        Fault,
        AutoReconnect,
    }

    /// <summary>
    /// Pendant V3 UI가 소비하는 단일 런타임 상태 스냅샷입니다.
    /// </summary>
    internal sealed class RobotControlV3RuntimeSnapshot
    {
        public RobotControlV3RuntimeStatusKind StatusKind { get; set; } = RobotControlV3RuntimeStatusKind.Disconnected;
        public string RobotTitle { get; set; } = "FAIRINO FR5";
        public string IpAddress { get; set; } = "IP: 192.168.57.2";
        public string ConnectionCardStatus { get; set; } = "대표 상태: 미연결";
        public string QuickServo { get; set; } = "서보: --";
        public string QuickMode { get; set; } = "모드: --";
        public string QuickSync { get; set; } = "현재 위치 읽음: 아직 안 함";
        public string QuickControllerMode { get; set; } = "컨트롤러: --";
        public string QuickSessionMode { get; set; } = "실기 세션: 읽기 전용";
        public string QuickLiveArm { get; set; } = "실제 이동: 잠겨 있음";
        public string HeaderNextAction { get; set; } = "다음 행동: 먼저 연결";
        public bool AutoModeSwitchEnabled { get; set; }
        public bool ManualModeSwitchEnabled { get; set; }
        public bool CurrentPositionReadComplete { get; set; }
        public string QuickActionLabel { get; set; } = "연결";
        public bool QuickActionEnabled { get; set; }
        public bool ConnectEnabled { get; set; } = true;
        public bool DisconnectEnabled { get; set; }
        public string ActionNow { get; set; } = "지금 상태: 미연결";
        public string ActionPrimary { get; set; } = "다음 행동: 먼저 연결";
        public string ActionWhy { get; set; } = "왜 먼저 하냐면 현재 상태를 읽으려면 연결부터 살아 있어야 함.";
        public string PrimaryActionLabel { get; set; } = "연결 →";
        public bool PrimaryActionEnabled { get; set; } = true;
        public string ConnectionChip { get; set; } = "연결: 미연결";
        public string ModeChip { get; set; } = "모드: --";
        public string SpeedChip { get; set; } = "속도: 30%";
        public string CoordChip { get; set; } = "좌표 기준: --";
        public string SafetyChip { get; set; } = "안전: --";
        public string FaultChip { get; set; } = "오류: 없음";
        public string ToolChip { get; set; } = "도구 설정: 미확인";
        public string UserChip { get; set; } = "작업 기준: 미확인";
        public string ConnectionClass { get; set; } = "rc-status-chip--muted";
        public string ModeClass { get; set; } = "rc-status-chip--muted";
        public string SpeedClass { get; set; } = "rc-status-chip--muted";
        public string SafetyClass { get; set; } = "rc-status-chip--muted";
        public string FaultClass { get; set; } = "rc-status-chip--muted";
        public bool ServoEnabled { get; set; }
        public bool RunEnabled { get; set; }
        public bool StopEnabled { get; set; }
        public bool PauseEnabled { get; set; }
        public bool SyncEnabled { get; set; }
        public bool ResetEnabled { get; set; }
        public string StatusConnection { get; set; } = "미연결";
        public string StatusMode { get; set; } = "--";
        public string StatusServo { get; set; } = "--";
        public string StatusMotion { get; set; } = "대기";
        public string StatusFault { get; set; } = "없음";
        public string StatusSafety { get; set; } = "--";
        public string StatusTool { get; set; } = "도구 미확인";
        public string StatusUser { get; set; } = "작업 기준 미확인";
        public string StatusSpeed { get; set; } = "30%";
        public string StatusConnectionClass { get; set; } = "rc-status-value--muted";
        public string StatusModeClass { get; set; } = "rc-status-value--muted";
        public string StatusServoClass { get; set; } = "rc-status-value--muted";
        public string StatusMotionClass { get; set; } = "rc-status-value--muted";
        public string StatusFaultClass { get; set; } = "rc-status-value--muted";
        public string StatusSafetyClass { get; set; } = "rc-status-value--muted";
        public bool FaultDetailEnabled { get; set; }
        public bool SafetyDetailEnabled { get; set; }
        public string CoordSystem { get; set; } = "Base";
        public string[] JointValues { get; set; } = CreateDefaultValues();
        public string[] TcpValues { get; set; } = CreateDefaultValues();
        public string CoordOverlayJointLine { get; set; } = "J: --  --  --  --  --  --";
        public string CoordOverlayTcpLine { get; set; } = "T: --  --  --  --  --  --";
        public bool DryRunEnabled { get; set; }
        public bool HasPendingPreview { get; set; }
        public string OperatorStatusHeadline { get; set; } = "미연결";
        public string LiveTrackingStatus { get; set; } = "실시간 추적 상태: 대기";
        public string PendingCommandSummary { get; set; } = "대기 중인 명령 없음";
        public string LastFeedback { get; set; } = "아직 실행한 명령이 없다.";
        public string LiveBlockedReason { get; set; } = string.Empty;
        public string OperatorNextAction { get; set; } = "먼저 연결";
        public string FailureCategory { get; set; } = "ready";
        public string MotionGateStatus { get; set; } = "실제 이동: 잠겨 있음";
        public string MotionGateDetail { get; set; } = "첫 실기 세션은 현재 위치 읽기 evidence부터 확인한다.";
        public string MotionGateWhyLocked { get; set; } = "잠금 이유: 현재 위치 읽기와 최신 기록을 먼저 확인한다.";
        public string MotionGateUnlockWhen { get; set; } = "언제 풀리는지: 현재 위치 읽음 완료, 최신 기록 확인, 첫 실기 시작 승인 후.";
        public string MotionGateNextStep { get; set; } = "다음 행동: 현재 위치 읽기 → 잠금 이유 확인 → 실기 세션 승인";
        public string MotionGateConfirmTarget { get; set; } = "승인 대상: 이번 연결의 실기 live session";
        public string MotionGateConfirmNote { get; set; } = "토큰은 첫 실기 시작 승인에만 사용한다.";
        public string CurrentSessionMode { get; set; } = "readback-only";
        public bool MotionGateReady { get; set; }
        public bool MixedLiveLoopRunning { get; set; }
        public int MixedLiveLoopCycleCount { get; set; }
        public string MixedLiveLoopTarget { get; set; } = "대기";
        public string MixedLiveLoopGripperIntent { get; set; } = "없음";
        public string MixedLiveLoopSummary { get; set; } = "mixed live loop 대기";
        public bool HasGhostPreview { get; set; }
        public bool HasPredictedPath { get; set; }
        public bool HasSelectedPart { get; set; }
        public string SelectedPartName { get; set; } = "선택된 파츠 없음";
        public string SelectedPartPose { get; set; } = "XYZ -- / ROT --";
        public string SelectedPartHint { get; set; } = "메인 로봇을 클릭하면 선택 파츠 정보를 여기서 본다.";
        public string GripperSummary { get; set; } = "Gripper: --";
        public float GripperOpenRatio { get; set; }
        public float GripperCommandedPositionPercent { get; set; } = 100f;
        public float GripperActualPositionPercent { get; set; } = 100f;
        public int GripperRawCommandedPositionPercent { get; set; } = 100;
        public int GripperRawActualPositionPercent { get; set; } = 100;
        public int GripperSpeedPercent { get; set; } = 50;
        public int GripperForcePercent { get; set; } = 50;
        public bool GripperObjectDetected { get; set; }
        public bool GripperHoldingObject { get; set; }
        public bool HasReliableGripperReadback { get; set; } = true;
        public float GripperObjectStopPercent { get; set; }
        public int GripperRawObjectStopPercent { get; set; }
        public bool GripperVisualAttached { get; set; }
        public string RobotDoSummary { get; set; } = "DO0 OFF / DO1 OFF";
        public string ToolDoSummary { get; set; } = "ToolDO0 OFF / ToolDO1 OFF";
        public string PeripheralFeedback { get; set; } = "주변장치 조작 전";
        public string GripperSdkSummary { get; set; } = "SDK gripper 비교 전";
        public string GripperReadbackNote { get; set; } = string.Empty;

        public RobotControlV3RuntimeSnapshot Clone()
        {
            return new RobotControlV3RuntimeSnapshot
            {
                StatusKind = StatusKind,
                RobotTitle = RobotTitle,
                IpAddress = IpAddress,
                ConnectionCardStatus = ConnectionCardStatus,
                QuickServo = QuickServo,
                QuickMode = QuickMode,
                QuickSync = QuickSync,
                QuickControllerMode = QuickControllerMode,
                QuickSessionMode = QuickSessionMode,
                QuickLiveArm = QuickLiveArm,
                HeaderNextAction = HeaderNextAction,
                AutoModeSwitchEnabled = AutoModeSwitchEnabled,
                ManualModeSwitchEnabled = ManualModeSwitchEnabled,
                CurrentPositionReadComplete = CurrentPositionReadComplete,
                QuickActionLabel = QuickActionLabel,
                QuickActionEnabled = QuickActionEnabled,
                ConnectEnabled = ConnectEnabled,
                DisconnectEnabled = DisconnectEnabled,
                ActionNow = ActionNow,
                ActionPrimary = ActionPrimary,
                ActionWhy = ActionWhy,
                PrimaryActionLabel = PrimaryActionLabel,
                PrimaryActionEnabled = PrimaryActionEnabled,
                ConnectionChip = ConnectionChip,
                ModeChip = ModeChip,
                SpeedChip = SpeedChip,
                CoordChip = CoordChip,
                SafetyChip = SafetyChip,
                FaultChip = FaultChip,
                ToolChip = ToolChip,
                UserChip = UserChip,
                ConnectionClass = ConnectionClass,
                ModeClass = ModeClass,
                SpeedClass = SpeedClass,
                SafetyClass = SafetyClass,
                FaultClass = FaultClass,
                ServoEnabled = ServoEnabled,
                RunEnabled = RunEnabled,
                StopEnabled = StopEnabled,
                PauseEnabled = PauseEnabled,
                SyncEnabled = SyncEnabled,
                ResetEnabled = ResetEnabled,
                StatusConnection = StatusConnection,
                StatusMode = StatusMode,
                StatusServo = StatusServo,
                StatusMotion = StatusMotion,
                StatusFault = StatusFault,
                StatusSafety = StatusSafety,
                StatusTool = StatusTool,
                StatusUser = StatusUser,
                StatusSpeed = StatusSpeed,
                StatusConnectionClass = StatusConnectionClass,
                StatusModeClass = StatusModeClass,
                StatusServoClass = StatusServoClass,
                StatusMotionClass = StatusMotionClass,
                StatusFaultClass = StatusFaultClass,
                StatusSafetyClass = StatusSafetyClass,
                FaultDetailEnabled = FaultDetailEnabled,
                SafetyDetailEnabled = SafetyDetailEnabled,
                CoordSystem = CoordSystem,
                JointValues = (string[])JointValues.Clone(),
                TcpValues = (string[])TcpValues.Clone(),
                CoordOverlayJointLine = CoordOverlayJointLine,
                CoordOverlayTcpLine = CoordOverlayTcpLine,
                DryRunEnabled = DryRunEnabled,
                HasPendingPreview = HasPendingPreview,
                OperatorStatusHeadline = OperatorStatusHeadline,
                LiveTrackingStatus = LiveTrackingStatus,
                PendingCommandSummary = PendingCommandSummary,
                LastFeedback = LastFeedback,
                LiveBlockedReason = LiveBlockedReason,
                OperatorNextAction = OperatorNextAction,
                FailureCategory = FailureCategory,
                MotionGateStatus = MotionGateStatus,
                MotionGateDetail = MotionGateDetail,
                MotionGateWhyLocked = MotionGateWhyLocked,
                MotionGateUnlockWhen = MotionGateUnlockWhen,
                MotionGateNextStep = MotionGateNextStep,
                MotionGateConfirmTarget = MotionGateConfirmTarget,
                MotionGateConfirmNote = MotionGateConfirmNote,
                CurrentSessionMode = CurrentSessionMode,
                MotionGateReady = MotionGateReady,
                MixedLiveLoopRunning = MixedLiveLoopRunning,
                MixedLiveLoopCycleCount = MixedLiveLoopCycleCount,
                MixedLiveLoopTarget = MixedLiveLoopTarget,
                MixedLiveLoopGripperIntent = MixedLiveLoopGripperIntent,
                MixedLiveLoopSummary = MixedLiveLoopSummary,
                HasGhostPreview = HasGhostPreview,
                HasPredictedPath = HasPredictedPath,
                HasSelectedPart = HasSelectedPart,
                SelectedPartName = SelectedPartName,
                SelectedPartPose = SelectedPartPose,
                SelectedPartHint = SelectedPartHint,
                GripperSummary = GripperSummary,
                GripperOpenRatio = GripperOpenRatio,
                GripperCommandedPositionPercent = GripperCommandedPositionPercent,
                GripperActualPositionPercent = GripperActualPositionPercent,
                GripperRawCommandedPositionPercent = GripperRawCommandedPositionPercent,
                GripperRawActualPositionPercent = GripperRawActualPositionPercent,
                GripperSpeedPercent = GripperSpeedPercent,
                GripperForcePercent = GripperForcePercent,
                GripperObjectDetected = GripperObjectDetected,
                GripperHoldingObject = GripperHoldingObject,
                HasReliableGripperReadback = HasReliableGripperReadback,
                GripperObjectStopPercent = GripperObjectStopPercent,
                GripperRawObjectStopPercent = GripperRawObjectStopPercent,
                GripperVisualAttached = GripperVisualAttached,
                RobotDoSummary = RobotDoSummary,
                ToolDoSummary = ToolDoSummary,
                PeripheralFeedback = PeripheralFeedback,
                GripperSdkSummary = GripperSdkSummary,
                GripperReadbackNote = GripperReadbackNote,
            };
        }

        private static string[] CreateDefaultValues()
        {
            return new[] { "--", "--", "--", "--", "--", "--" };
        }
    }
}
