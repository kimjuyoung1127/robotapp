// Folder: UI - HUD/view components only; no kinematics logic.
namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 패널 시안 프리셋 상태와 공용 표시 데이터를 정의합니다.
    /// </summary>
    internal static class PendantV3PreviewState
    {
        internal enum Kind
        {
            Disconnected,
            ConnectedServoOff,
            ConnectedUnsynced,
            ReadyToJog,
            Fault,
            AutoReconnect,
        }

        internal static Definition GetDefinition(Kind state)
        {
            return state switch
            {
                Kind.Disconnected => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: ○ 미연결",
                    "서보: -- → [연결 후 가능]", "모드: --", "현재 위치 읽음: 아직 안 함", "연결", false, true, false,
                    "지금 상태: 아직 미연결", "다음 행동: 먼저 연결",
                    "왜 먼저 하냐면 현재 상태를 읽으려면 로봇 연결부터 살아 있어야 함.", "연결 →", true,
                    "연결: 미연결", "모드: --", "속도: --", "좌표 기준: --", "안전: --", "오류: 없음", "도구 설정: --", "작업 기준: --",
                    "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--muted",
                    false, false, false, false, false, false,
                    "○ 미연결", "--", "--", "대기", "없음", "--", "--", "--", "--",
                    "rc-status-value--muted", "rc-status-value--muted", "rc-status-value--muted", "rc-status-value--muted", "rc-status-value--muted", "rc-status-value--muted",
                    false, false, "--",
                    new[] { "--", "--", "--", "--", "--", "--" },
                    new[] { "--", "--", "--", "--", "--", "--" },
                    "J: --  --  --  --  --  --",
                    "T: --  --  --  --  --  --"),
                Kind.ConnectedServoOff => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: ● 연결됨",
                    "서보: OFF → [서보 켜기]", "모드: 수동", "현재 위치 읽음: 아직 안 함", "서보 켜기", true, true, true,
                    "지금 상태: 연결됨 / 서보 OFF", "다음 행동: 서보를 먼저 켜기",
                    "왜 먼저 하냐면 서보가 살아야 실제 이동을 보낼 수 있음.", "서보 켜기 →", true,
                    "연결: 연결됨", "모드: 수동", "속도: 30%", "좌표 기준: 로봇 기준", "안전: 정상", "오류: 없음", "도구 설정: 1번", "작업 기준: 0번",
                    "rc-status-chip--success", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--success", "rc-status-chip--muted",
                    true, false, false, false, false, false,
                    "연결됨", "수동", "꺼짐", "정지", "없음", "정상", "도구 1번", "작업 기준 0번", "30%",
                    "rc-status-value--success", "rc-status-value--default", "rc-status-value--warning", "rc-status-value--default", "rc-status-value--muted", "rc-status-value--success",
                    false, false, "Base",
                    new[] { "0.0", "-32.0", "84.0", "0.0", "90.0", "0.0" },
                    new[] { "-497.0", "-130.0", "477.0", "180.0", "0.0", "90.0" },
                    "J: 0.0  -32.0  84.0  0.0  90.0  0.0",
                    "T: -497  -130  477  180.0  0.0  90.0"),
                Kind.ConnectedUnsynced => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: ● 연결됨 / 서보 ON",
                    "서보: ON", "모드: 수동", "현재 위치 읽음: 아직 안 함", "동기화", true, true, true,
                    "지금 상태: 서보 ON / 아직 미동기화", "다음 행동: 동기화 먼저",
                    "왜 먼저 하냐면 실제 로봇 자세를 먼저 읽어야 첫 조작이 덜 위험함.", "동기화 →", true,
                    "연결: 연결됨", "모드: 수동", "속도: 30%", "좌표 기준: 로봇 기준", "안전: 정상", "오류: 없음", "도구 설정: 1번", "작업 기준: 0번",
                    "rc-status-chip--success", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--success", "rc-status-chip--muted",
                    false, true, false, false, true, false,
                    "연결됨", "수동", "켜짐", "미동기화", "없음", "정상", "도구 1번", "작업 기준 0번", "30%",
                    "rc-status-value--success", "rc-status-value--default", "rc-status-value--success", "rc-status-value--warning", "rc-status-value--muted", "rc-status-value--success",
                    false, false, "Base",
                    new[] { "2.0", "-28.5", "79.0", "0.0", "88.0", "5.0" },
                    new[] { "-488.0", "-120.0", "472.0", "179.0", "2.0", "87.0" },
                    "J: 2.0  -28.5  79.0  0.0  88.0  5.0",
                    "T: -488  -120  472  179.0  2.0  87.0"),
                Kind.ReadyToJog => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: ● 조작 가능",
                    "서보: ON", "모드: 수동", "현재 위치 읽음: 완료", "동기화", true, true, true,
                    "지금 상태: 동기화 완료 / 첫 진입", "다음 행동: 쉬운 조작 시작",
                    "왜 먼저 하냐면 Home이나 Ready 같은 작은 동작이 제일 안전하게 감 잡기 좋음.", "쉬운 조작 시작 →", true,
                    "연결: 연결됨", "모드: 수동", "속도: 30%", "좌표 기준: 로봇 기준", "안전: 정상", "오류: 없음", "도구 설정: 1번", "작업 기준: 0번",
                    "rc-status-chip--success", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--success", "rc-status-chip--muted",
                    false, true, true, false, true, false,
                    "연결됨", "수동", "켜짐", "정지", "없음", "정상", "도구 1번", "작업 기준 0번", "30%",
                    "rc-status-value--success", "rc-status-value--default", "rc-status-value--success", "rc-status-value--default", "rc-status-value--muted", "rc-status-value--success",
                    false, false, "Base",
                    new[] { "0.0", "-32.0", "84.0", "0.0", "90.0", "0.0" },
                    new[] { "-497.0", "-130.0", "477.0", "180.0", "0.0", "90.0" },
                    "J: 0.0  -32.0  84.0  0.0  90.0  0.0",
                    "T: -497  -130  477  180.0  0.0  90.0"),
                Kind.Fault => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: ⛔ Fault",
                    "서보: 잠김", "모드: --", "현재 위치 읽음: 중단됨", "원인 보기", true, true, true,
                    "지금 상태: Fault 발생", "다음 행동: 원인부터 보기",
                    "왜 먼저 하냐면 초기화부터 누르면 같은 Fault를 바로 다시 밟을 수 있음.", "원인 보기 →", true,
                    "연결: 연결됨", "모드: --", "속도: --", "좌표 기준: 로봇 기준", "안전: 정지", "오류: F203", "도구 설정: 1번", "작업 기준: 0번",
                    "rc-status-chip--danger", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--danger", "rc-status-chip--danger",
                    false, false, false, false, false, true,
                    "연결됨", "--", "잠김", "오류", "F203", "정지", "도구 1번", "작업 기준 0번", "--",
                    "rc-status-value--success", "rc-status-value--muted", "rc-status-value--danger", "rc-status-value--danger", "rc-status-value--danger", "rc-status-value--danger",
                    true, true, "Base",
                    new[] { "0.0", "-31.5", "83.7", "0.0", "90.0", "0.0" },
                    new[] { "-497.0", "-130.0", "477.0", "180.0", "0.0", "90.0" },
                    "J: 0.0  -31.5  83.7  0.0  90.0  0.0",
                    "T: -497  -130  477  180.0  0.0  90.0"),
                _ => new Definition(
                    "FAIRINO FR5", "IP: 192.168.57.2", "상태: 재연결 시도 중",
                    "서보: 보류", "모드: --", "현재 위치 읽음: 대기", "대기 중", false, false, false,
                    "지금 상태: 자동 재연결 중", "다음 행동: 잠깐 기다리기",
                    "왜 먼저 하냐면 지금은 3초 간격 재시도 중이라 수동 연결보다 자동 복귀가 먼저임.", "재시도 중...", false,
                    "연결: 끊김", "모드: --", "속도: --", "좌표 기준: --", "안전: 대기", "오류: 통신 끊김", "도구 설정: --", "작업 기준: --",
                    "rc-status-chip--warning", "rc-status-chip--muted", "rc-status-chip--muted", "rc-status-chip--warning", "rc-status-chip--warning",
                    false, false, false, false, false, false,
                    "재연결 중", "--", "보류", "대기", "통신 끊김", "대기", "--", "--", "--",
                    "rc-status-value--warning", "rc-status-value--muted", "rc-status-value--warning", "rc-status-value--warning", "rc-status-value--warning", "rc-status-value--warning",
                    false, false, "--",
                    new[] { "--", "--", "--", "--", "--", "--" },
                    new[] { "--", "--", "--", "--", "--", "--" },
                    "J: --  --  --  --  --  --",
                    "T: --  --  --  --  --  --"),
            };
        }

        internal readonly struct Definition
        {
            internal Definition(
                string robotTitle,
                string ipAddress,
                string connectionCardStatus,
                string quickServo,
                string quickMode,
                string quickSync,
                string quickActionLabel,
                bool quickActionEnabled,
                bool connectEnabled,
                bool disconnectEnabled,
                string actionNow,
                string actionPrimary,
                string actionWhy,
                string primaryActionLabel,
                bool primaryActionEnabled,
                string connectionChip,
                string modeChip,
                string speedChip,
                string coordChip,
                string safetyChip,
                string faultChip,
                string toolChip,
                string userChip,
                string connectionClass,
                string modeClass,
                string speedClass,
                string safetyClass,
                string faultClass,
                bool servoEnabled,
                bool runEnabled,
                bool stopEnabled,
                bool pauseEnabled,
                bool syncEnabled,
                bool resetEnabled,
                string statusConnection,
                string statusMode,
                string statusServo,
                string statusMotion,
                string statusFault,
                string statusSafety,
                string statusTool,
                string statusUser,
                string statusSpeed,
                string statusConnectionClass,
                string statusModeClass,
                string statusServoClass,
                string statusMotionClass,
                string statusFaultClass,
                string statusSafetyClass,
                bool faultDetailEnabled,
                bool safetyDetailEnabled,
                string coordSystem,
                string[] jointValues,
                string[] tcpValues,
                string coordOverlayJointLine,
                string coordOverlayTcpLine)
            {
                RobotTitle = robotTitle;
                IpAddress = ipAddress;
                ConnectionCardStatus = connectionCardStatus;
                QuickServo = quickServo;
                QuickMode = quickMode;
                QuickSync = quickSync;
                QuickActionLabel = quickActionLabel;
                QuickActionEnabled = quickActionEnabled;
                ConnectEnabled = connectEnabled;
                DisconnectEnabled = disconnectEnabled;
                ActionNow = actionNow;
                ActionPrimary = actionPrimary;
                ActionWhy = actionWhy;
                PrimaryActionLabel = primaryActionLabel;
                PrimaryActionEnabled = primaryActionEnabled;
                ConnectionChip = connectionChip;
                ModeChip = modeChip;
                SpeedChip = speedChip;
                CoordChip = coordChip;
                SafetyChip = safetyChip;
                FaultChip = faultChip;
                ToolChip = toolChip;
                UserChip = userChip;
                ConnectionClass = connectionClass;
                ModeClass = modeClass;
                SpeedClass = speedClass;
                SafetyClass = safetyClass;
                FaultClass = faultClass;
                ServoEnabled = servoEnabled;
                RunEnabled = runEnabled;
                StopEnabled = stopEnabled;
                PauseEnabled = pauseEnabled;
                SyncEnabled = syncEnabled;
                ResetEnabled = resetEnabled;
                StatusConnection = statusConnection;
                StatusMode = statusMode;
                StatusServo = statusServo;
                StatusMotion = statusMotion;
                StatusFault = statusFault;
                StatusSafety = statusSafety;
                StatusTool = statusTool;
                StatusUser = statusUser;
                StatusSpeed = statusSpeed;
                StatusConnectionClass = statusConnectionClass;
                StatusModeClass = statusModeClass;
                StatusServoClass = statusServoClass;
                StatusMotionClass = statusMotionClass;
                StatusFaultClass = statusFaultClass;
                StatusSafetyClass = statusSafetyClass;
                FaultDetailEnabled = faultDetailEnabled;
                SafetyDetailEnabled = safetyDetailEnabled;
                CoordSystem = coordSystem;
                JointValues = jointValues;
                TcpValues = tcpValues;
                CoordOverlayJointLine = coordOverlayJointLine;
                CoordOverlayTcpLine = coordOverlayTcpLine;
            }

            internal string RobotTitle { get; }
            internal string IpAddress { get; }
            internal string ConnectionCardStatus { get; }
            internal string QuickServo { get; }
            internal string QuickMode { get; }
            internal string QuickSync { get; }
            internal string QuickActionLabel { get; }
            internal bool QuickActionEnabled { get; }
            internal bool ConnectEnabled { get; }
            internal bool DisconnectEnabled { get; }
            internal string ActionNow { get; }
            internal string ActionPrimary { get; }
            internal string ActionWhy { get; }
            internal string PrimaryActionLabel { get; }
            internal bool PrimaryActionEnabled { get; }
            internal string ConnectionChip { get; }
            internal string ModeChip { get; }
            internal string SpeedChip { get; }
            internal string CoordChip { get; }
            internal string SafetyChip { get; }
            internal string FaultChip { get; }
            internal string ToolChip { get; }
            internal string UserChip { get; }
            internal string ConnectionClass { get; }
            internal string ModeClass { get; }
            internal string SpeedClass { get; }
            internal string SafetyClass { get; }
            internal string FaultClass { get; }
            internal bool ServoEnabled { get; }
            internal bool RunEnabled { get; }
            internal bool StopEnabled { get; }
            internal bool PauseEnabled { get; }
            internal bool SyncEnabled { get; }
            internal bool ResetEnabled { get; }
            internal string StatusConnection { get; }
            internal string StatusMode { get; }
            internal string StatusServo { get; }
            internal string StatusMotion { get; }
            internal string StatusFault { get; }
            internal string StatusSafety { get; }
            internal string StatusTool { get; }
            internal string StatusUser { get; }
            internal string StatusSpeed { get; }
            internal string StatusConnectionClass { get; }
            internal string StatusModeClass { get; }
            internal string StatusServoClass { get; }
            internal string StatusMotionClass { get; }
            internal string StatusFaultClass { get; }
            internal string StatusSafetyClass { get; }
            internal bool FaultDetailEnabled { get; }
            internal bool SafetyDetailEnabled { get; }
            internal string CoordSystem { get; }
            internal string[] JointValues { get; }
            internal string[] TcpValues { get; }
            internal string CoordOverlayJointLine { get; }
            internal string CoordOverlayTcpLine { get; }
        }
    }
}
