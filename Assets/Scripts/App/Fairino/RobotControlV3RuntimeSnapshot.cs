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
        public string IpAddress { get; set; } = "IP: 192.168.58.2";
        public string ConnectionCardStatus { get; set; } = "상태: ○ 미연결";
        public string QuickServo { get; set; } = "서보: --";
        public string QuickMode { get; set; } = "모드: --";
        public string QuickSync { get; set; } = "마지막 동기화: --";
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
        public string CoordChip { get; set; } = "좌표계: Base";
        public string SafetyChip { get; set; } = "안전: --";
        public string FaultChip { get; set; } = "Fault: 없음";
        public string ToolChip { get; set; } = "Tool: 00";
        public string UserChip { get; set; } = "User: 00";
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
        public string StatusConnection { get; set; } = "--";
        public string StatusMode { get; set; } = "--";
        public string StatusServo { get; set; } = "--";
        public string StatusMotion { get; set; } = "대기";
        public string StatusFault { get; set; } = "없음";
        public string StatusSafety { get; set; } = "--";
        public string StatusTool { get; set; } = "00";
        public string StatusUser { get; set; } = "00";
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
        public bool DryRunEnabled { get; set; } = true;
        public bool HasPendingPreview { get; set; }
        public string PendingCommandSummary { get; set; } = "대기 중인 명령 없음";
        public string LastFeedback { get; set; } = "아직 실행한 명령이 없다.";
        public string LiveBlockedReason { get; set; } = string.Empty;
        public bool HasGhostPreview { get; set; }
        public bool HasPredictedPath { get; set; }
        public bool HasSelectedPart { get; set; }
        public string SelectedPartName { get; set; } = "선택된 파츠 없음";
        public string SelectedPartPose { get; set; } = "XYZ -- / ROT --";
        public string SelectedPartHint { get; set; } = "메인 로봇을 클릭하면 선택 파츠 정보를 여기서 본다.";
        public string GripperSummary { get; set; } = "Gripper: --";
        public float GripperOpenRatio { get; set; }
        public bool GripperVisualAttached { get; set; }
        public string RobotDoSummary { get; set; } = "DO0 OFF / DO1 OFF";
        public string ToolDoSummary { get; set; } = "ToolDO0 OFF / ToolDO1 OFF";
        public string PeripheralFeedback { get; set; } = "주변장치 조작 전";

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
                PendingCommandSummary = PendingCommandSummary,
                LastFeedback = LastFeedback,
                LiveBlockedReason = LiveBlockedReason,
                HasGhostPreview = HasGhostPreview,
                HasPredictedPath = HasPredictedPath,
                HasSelectedPart = HasSelectedPart,
                SelectedPartName = SelectedPartName,
                SelectedPartPose = SelectedPartPose,
                SelectedPartHint = SelectedPartHint,
                GripperSummary = GripperSummary,
                GripperOpenRatio = GripperOpenRatio,
                GripperVisualAttached = GripperVisualAttached,
                RobotDoSummary = RobotDoSummary,
                ToolDoSummary = ToolDoSummary,
                PeripheralFeedback = PeripheralFeedback,
            };
        }

        private static string[] CreateDefaultValues()
        {
            return new[] { "--", "--", "--", "--", "--", "--" };
        }
    }
}
