// Folder: StatusSafety - snapshot composition and peripheral/status projection for V3 diagnostics panels.
// Composes the operator-facing runtime snapshot and related readback-follow helpers.
// Gate wording and mode/session labels live in sibling StatusSafety partials.
using KineTutor3D.UI.RobotControlV3;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private void RefreshSnapshot()
        {
            if (isRefreshingSnapshot)
            {
                snapshotRefreshQueued = true;
                return;
            }

            isRefreshingSnapshot = true;
            try
            {
                do
                {
                    snapshotRefreshQueued = false;
                    var readbackOnlyLive = IsReadbackOnlyLiveClient();
                    var busyAsyncReadback = HasPendingAsyncReadbackOperation();
                    var activeReadbackLabel = !string.IsNullOrWhiteSpace(activeReadbackOperationLabel)
                        ? activeReadbackOperationLabel
                        : awaitingPolledReadbackLabel;
                    var prioritizeLiveReadback = ShouldPrioritizeLiveReadbackDisplay();
                    var jointValues = prioritizeLiveReadback
                        ? CopyJointArray(currentState.JointPosDeg)
                        : previewUsesJointPose && previewJointAnglesDeg != null
                            ? CopyJointArray(previewJointAnglesDeg)
                            : previewTcpVisualJointAnglesDeg != null && previewTcpPose != null
                                ? CopyJointArray(previewTcpVisualJointAnglesDeg)
                                : CopyJointArray(currentState.JointPosDeg);
                    var tcpValues = prioritizeLiveReadback
                        ? CopyPoseArray(currentState.TcpPose)
                        : ComputeDisplayedTcpPose();
                    snapshot.HasPendingPreview = previewUsesJointPose || previewTcpPose != null;
                    snapshot.StatusKind = ResolveStatusKind();
                    snapshot.RobotTitle = templateDefinition.DisplayName;
                    snapshot.IpAddress = $"IP: {config.defaultIp}";
                    snapshot.ConnectionCardStatus = "대표 상태: 미연결";
                    snapshot.QuickServo = readbackOnlyLive
                        ? "서보: 위치 확인 전용"
                        : connectionService.Client.IsEnabled ? "서보: 켜짐" : "서보: 꺼짐";
                    var controllerMode = ResolveControllerModeLabel();
                    snapshot.QuickMode = $"모드: {controllerMode}";
                    snapshot.CurrentPositionReadComplete = hasCurrentPositionReadComplete;
                    snapshot.QuickSync = busyAsyncReadback
                        ? $"현재 위치 읽음: {activeReadbackLabel} 진행 중"
                        : BuildCurrentPositionReadStatus();
                    snapshot.QuickControllerMode = BuildControllerSessionSummary(readbackOnlyLive);
                    snapshot.QuickSessionMode = BuildLiveSessionModeSummary(currentLiveSessionMode);
                    snapshot.CurrentSessionMode = currentLiveSessionMode.ToString();
                    snapshot.MixedLiveLoopRunning = liveWaypointSequenceLooping;
                    snapshot.MixedLiveLoopCycleCount = liveWaypointSequenceCycleCount;
                    snapshot.MixedLiveLoopTarget = string.IsNullOrWhiteSpace(liveWaypointCurrentTargetName) ? "대기" : liveWaypointCurrentTargetName;
                    snapshot.MixedLiveLoopGripperIntent = string.IsNullOrWhiteSpace(liveWaypointCurrentGripperIntent) ? "없음" : liveWaypointCurrentGripperIntent;
                    snapshot.MixedLiveLoopSummary = liveWaypointSequenceLooping
                        ? $"mixed live loop {liveWaypointSequenceCycleCount}사이클 · target {snapshot.MixedLiveLoopTarget} · gripper {snapshot.MixedLiveLoopGripperIntent}"
                        : string.IsNullOrWhiteSpace(liveWaypointBlockedReason)
                            ? "mixed live loop 대기"
                            : liveWaypointBlockedReason;
                    snapshot.AutoModeSwitchEnabled = !busyAsyncReadback && ResolveAutoModeSwitchEnabled();
                    snapshot.ManualModeSwitchEnabled = !busyAsyncReadback && ResolveManualModeSwitchEnabled();
                    snapshot.QuickActionLabel = ResolveQuickActionLabel();
                    snapshot.QuickActionEnabled = !busyAsyncReadback && ResolveQuickActionEnabled();
                    snapshot.ConnectEnabled = !busyAsyncReadback && !connectionService.Client.IsConnected;
                    snapshot.DisconnectEnabled = !busyAsyncReadback && connectionService.Client.IsConnected;
                    snapshot.ActionNow = busyAsyncReadback
                        ? $"{BuildActionNow()} · live 응답 대기 중"
                        : BuildActionNow();
                    snapshot.ActionPrimary = busyAsyncReadback
                        ? $"대기 중: {activeReadbackLabel}"
                        : BuildActionPrimary();
                    snapshot.ActionWhy = busyAsyncReadback
                        ? "같은 실기 readback 요청을 중복으로 보내지 않도록 현재 작업이 끝날 때까지 기다린다."
                        : BuildActionWhy();
                    snapshot.PrimaryActionLabel = ResolveQuickActionLabel();
                    snapshot.PrimaryActionEnabled = !busyAsyncReadback && ResolveQuickActionEnabled();
                    snapshot.ConnectionChip = "연결: 미연결";
                    snapshot.ModeChip = $"모드: {controllerMode}";
                    snapshot.SpeedChip = snapshot.StatusSpeed = $"{ResolveRequestedSpeedPercent()}%";
                    var liveToolId = ResolveLiveToolId();
                    var liveUserId = ResolveLiveUserId();
                    snapshot.CoordChip = $"좌표 기준: {FormatCoordSystemDisplay(snapshot.CoordSystem)}";
                    snapshot.ToolChip = $"도구 설정: {FormatContextId(liveToolId)}";
                    snapshot.UserChip = $"작업 기준: {FormatContextId(liveUserId)}";
                    snapshot.ConnectionClass = connectionService.Client.IsConnected ? "rc-status-chip--success" : "rc-status-chip--muted";
                    snapshot.ModeClass = ResolveControllerModeChipClass();
                    snapshot.SpeedClass = "rc-status-chip--muted";
                    snapshot.SafetyClass = snapshot.StatusKind == RobotControlV3RuntimeStatusKind.Fault ? "rc-status-chip--danger" : "rc-status-chip--success";
                    snapshot.FaultClass = snapshot.StatusKind == RobotControlV3RuntimeStatusKind.Fault ? "rc-status-chip--danger" : "rc-status-chip--muted";
                    snapshot.ServoEnabled = !busyAsyncReadback && connectionService.Client.IsConnected && !connectionService.Client.IsEnabled && !readbackOnlyLive;
                    snapshot.RunEnabled = !busyAsyncReadback && connectionService.Client.IsConnected;
                    snapshot.StopEnabled = !busyAsyncReadback && connectionService.Client.IsConnected;
                    snapshot.PauseEnabled = !busyAsyncReadback;
                    snapshot.SyncEnabled = !busyAsyncReadback && connectionService.Client.IsConnected;
                    snapshot.ResetEnabled = !busyAsyncReadback && connectionService.Client.IsConnected;
                    snapshot.StatusConnection = RobotControlV3OperatorStatusCopy.BuildConnectionStatusValue(
                        connectionService.Client.IsConnected,
                        hasCurrentPositionReadComplete);
                    snapshot.StatusMode = controllerMode;
                    snapshot.StatusServo = readbackOnlyLive
                        ? "위치 확인 전용"
                        : connectionService.Client.IsEnabled ? "켜짐" : "꺼짐";
                    snapshot.StatusMotion = busyAsyncReadback
                        ? "읽는 중"
                        : isPaused ? "일시정지" : (snapshot.HasPendingPreview ? "미리보기" : "대기");
                    snapshot.StatusFault = connectionService.LastControllerFault.HasBlockingFault ? $"F{connectionService.LastControllerFault.MainCode}" : "없음";
                    snapshot.StatusSafety = connectionService.LastControllerFault.IsSafetyStop ? "정지" : "정상";
                    snapshot.StatusTool = FormatToolDisplay(liveToolId);
                    snapshot.StatusUser = FormatUserDisplay(liveUserId);
                    snapshot.SafetyChip = $"안전: {snapshot.StatusSafety}";
                    snapshot.FaultChip = $"오류: {snapshot.StatusFault}";
                    snapshot.StatusConnectionClass = connectionService.Client.IsConnected ? "rc-status-value--success" : "rc-status-value--muted";
                    snapshot.StatusModeClass = ResolveControllerModeValueClass();
                    snapshot.StatusServoClass = readbackOnlyLive
                        ? "rc-status-value--muted"
                        : connectionService.Client.IsEnabled ? "rc-status-value--success" : "rc-status-value--warning";
                    snapshot.StatusMotionClass = snapshot.HasPendingPreview ? "rc-status-value--warning" : "rc-status-value--default";
                    snapshot.StatusFaultClass = connectionService.LastControllerFault.HasBlockingFault ? "rc-status-value--danger" : "rc-status-value--muted";
                    snapshot.StatusSafetyClass = connectionService.LastControllerFault.IsSafetyStop ? "rc-status-value--danger" : "rc-status-value--success";
                    snapshot.FaultDetailEnabled = true;
                    snapshot.SafetyDetailEnabled = true;
                    snapshot.JointValues = FormatValues(jointValues, "0.0");
                    snapshot.TcpValues = FormatValues(tcpValues, "0.0");
                    snapshot.CoordOverlayJointLine = $"J: {string.Join("  ", snapshot.JointValues)}";
                    snapshot.CoordOverlayTcpLine = $"T: {string.Join("  ", snapshot.TcpValues)}";
                    snapshot.PendingCommandSummary = liveWaypointSequenceLooping
                        ? $"대기 명령: mixed live loop {liveWaypointSequenceCycleCount}사이클 · target {snapshot.MixedLiveLoopTarget} · gripper {snapshot.MixedLiveLoopGripperIntent}"
                        : previewUsesJointPose && previewJointAnglesDeg != null
                            ? "대기 명령: MoveJ"
                            : previewTcpPose != null
                                ? "대기 명령: MoveL"
                                : "대기 중인 명령 없음";
                    ApplyRetainedOperatorBlockedReasonToSnapshot();
                    if (string.IsNullOrWhiteSpace(snapshot.LiveBlockedReason)
                        && !string.IsNullOrWhiteSpace(snapshot.LastFeedback)
                        && snapshot.LastFeedback.StartsWith("[Live Gate] Blocked:", System.StringComparison.Ordinal))
                    {
                        snapshot.LiveBlockedReason = snapshot.LastFeedback;
                    }

                    ApplyMotionGateSnapshot();
                    snapshot.FailureCategory = BuildFailureCategory();
                    snapshot.OperatorNextAction = BuildOperatorNextAction();
                    snapshot.HeaderNextAction = busyAsyncReadback
                        ? $"다음 행동: {activeReadbackLabel} 완료 기다리기"
                        : $"다음 행동: {snapshot.OperatorNextAction}";
                    snapshot.QuickLiveArm = liveWaypointSequenceLooping
                        ? $"실제 이동: mixed live loop 실행 중 · {snapshot.MixedLiveLoopTarget}"
                        : snapshot.MotionGateStatus;
                    snapshot.OperatorStatusHeadline = RobotControlV3OperatorStatusCopy.BuildRepresentativeStatus(
                        connectionService.Client.IsConnected,
                        hasCurrentPositionReadComplete,
                        !snapshot.MotionGateReady);
                    snapshot.ConnectionCardStatus = RobotControlV3OperatorStatusCopy.BuildConnectionCardStatus(
                        connectionService.Client.IsConnected,
                        hasCurrentPositionReadComplete,
                        !snapshot.MotionGateReady);
                    snapshot.ConnectionChip = BuildConnectionChip();
                    snapshot.LiveTrackingStatus = BuildLiveTrackingStatus(prioritizeLiveReadback, readbackOnlyLive);
                    snapshot.HasGhostPreview = ghostRobotVisual != null && ghostRobotVisual.HasGhost;
                    snapshot.HasPredictedPath = predictedPathRenderer != null && predictedPathRenderer.HasPath;
                    ApplyPeripheralSnapshot();
                    ApplySelectedPartSnapshot();
                    snapshot.LastFeedback = snapshot.LastFeedback;
                    SnapshotChanged?.Invoke(snapshot.Clone());
                }
                while (snapshotRefreshQueued);
            }
            finally
            {
                isRefreshingSnapshot = false;
            }
        }

        private bool ShouldAutoFollowLiveReadback()
        {
            return connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client != null
                && connectionService.Client.IsConnected
                && (currentLiveSessionMode == LiveCommandSessionMode.LiveControl
                    || currentLiveSessionMode == LiveCommandSessionMode.LoopRunning);
        }

        private bool ShouldPrioritizeLiveReadbackDisplay()
        {
            return ShouldAutoFollowLiveReadback();
        }

        private void ClearPendingPreviewForLiveReadback()
        {
            if (ShouldPreservePreparedLiveContextDuringReadbackFollow())
            {
                return;
            }

            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            ClearPreparedMotionContext();
            if (currentLiveSessionMode == LiveCommandSessionMode.LiveControl
                || currentLiveSessionMode == LiveCommandSessionMode.LoopRunning)
            {
                InvalidateLiveApprovalContext();
            }
        }

        private bool ShouldPreservePreparedLiveContextDuringReadbackFollow()
        {
            return pendingLiveApprovalRequired
                || hasPendingSavedPointOperatorCommand
                || hasPendingWaypointSequenceOperatorCommand
                || hasPendingGripperOperatorCommand;
        }

        private void ApplyMotionGateSnapshot()
        {
            if (IsReadbackOnlyLiveClient())
            {
                snapshot.MotionGateReady = false;
                snapshot.MotionGateStatus = "실제 이동: 잠겨 있음";
                snapshot.MotionGateDetail = BuildReadbackOnlyGateDetail();
                snapshot.MotionGateWhyLocked = "잠금 이유: 현재 세션은 위치 확인 전용이라 실제 이동이 잠겨 있습니다.";
                snapshot.MotionGateUnlockWhen = "언제 풀리는지: motion-capable 세션에서 도구 설정, 작업 기준, 좌표 기준, 최신 기록, 첫 실기 세션 승인까지 준비되면 실제 제어가 열립니다.";
                snapshot.MotionGateNextStep = hasCurrentPositionReadComplete
                    ? "다음 행동: latest-state와 latest-drift가 현재 세션 기준으로 갱신됐는지 확인한다."
                    : "다음 행동: 먼저 현재 위치 읽기를 완료해 실기와 화면 위치가 맞는지 확인한다.";
                snapshot.MotionGateConfirmTarget = "승인 대상: 이번 연결의 실기 live session";
                snapshot.MotionGateConfirmNote = "현재 세션은 위치 확인 전용이라 실기 시작 승인을 발급하지 않습니다.";
                return;
            }

            var gate = EvaluateLiveCommandSafetyPreview(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: false);
            snapshot.MotionGateReady = gate.Status == LiveCommandGateStatus.Allowed || gate.Status == LiveCommandGateStatus.RequiresConfirm;
            snapshot.MotionGateStatus = gate.Status switch
            {
                LiveCommandGateStatus.Allowed => "실제 이동: 가능",
                LiveCommandGateStatus.RequiresConfirm => "실제 이동: 첫 세션 승인 필요",
                LiveCommandGateStatus.ReadbackOnly => "실제 이동: 잠겨 있음",
                _ => "실제 이동: 아직 차단됨",
            };
            snapshot.MotionGateDetail = FormatMotionGateDetail(gate);
            snapshot.MotionGateWhyLocked = BuildMotionGateWhyLocked(gate);
            snapshot.MotionGateUnlockWhen = BuildMotionGateUnlockWhen(gate);
            snapshot.MotionGateNextStep = BuildMotionGateNextStep(gate);
            snapshot.MotionGateConfirmTarget = BuildMotionGateConfirmTarget(gate);
            snapshot.MotionGateConfirmNote = BuildMotionGateConfirmNote(gate);
        }

        private void ApplyPeripheralSnapshot()
        {
            if (peripheralFacade == null)
            {
                snapshot.GripperSummary = "Gripper: --";
                snapshot.GripperOpenRatio = 1f;
                snapshot.GripperCommandedPositionPercent = 100;
                snapshot.GripperActualPositionPercent = 100;
                snapshot.GripperRawCommandedPositionPercent = 100;
                snapshot.GripperRawActualPositionPercent = 100;
                snapshot.GripperSpeedPercent = 50;
                snapshot.GripperForcePercent = 50;
                snapshot.GripperObjectDetected = false;
                snapshot.GripperHoldingObject = false;
                snapshot.HasReliableGripperReadback = true;
                snapshot.GripperObjectStopPercent = 0;
                snapshot.GripperRawObjectStopPercent = 0;
                snapshot.GripperVisualAttached = false;
                snapshot.RobotDoSummary = "DO0 OFF / DO1 OFF";
                snapshot.ToolDoSummary = "ToolDO0 OFF / ToolDO1 OFF";
                snapshot.PeripheralFeedback = "주변장치 facade 없음";
                snapshot.GripperReadbackNote = string.Empty;
                return;
            }

            var peripheral = peripheralFacade.Snapshot;
            snapshot.GripperOpenRatio = peripheral.GripperOpenRatio;
            snapshot.GripperCommandedPositionPercent = peripheral.GripperCommandedPositionPercent;
            snapshot.GripperActualPositionPercent = peripheral.GripperActualPositionPercent;
            snapshot.GripperRawCommandedPositionPercent = peripheral.GripperRawCommandedPositionPercent;
            snapshot.GripperRawActualPositionPercent = peripheral.GripperRawActualPositionPercent;
            snapshot.GripperSpeedPercent = peripheral.GripperSpeedPercent;
            snapshot.GripperForcePercent = peripheral.GripperForcePercent;
            snapshot.GripperObjectDetected = peripheral.GripperObjectDetected;
            snapshot.GripperHoldingObject = peripheral.GripperHoldingObject;
            snapshot.HasReliableGripperReadback = peripheral.HasReliableGripperReadback;
            snapshot.GripperObjectStopPercent = peripheral.GripperObjectStopPercent;
            snapshot.GripperRawObjectStopPercent = peripheral.GripperRawObjectStopPercent;
            snapshot.GripperVisualAttached = peripheral.GripperVisualAttached;
            var holdSuffix = peripheral.GripperHoldingObject ? " / Object Hold" : string.Empty;
            snapshot.GripperSummary = peripheral.HasReliableGripperReadback
                ? $"Gripper: Cmd {peripheral.GripperCommandedPositionPercent:0.##}% / Actual {peripheral.GripperActualPositionPercent:0.##}%{holdSuffix} ({peripheral.GripperOpenRatio:0.00}) · raw {peripheral.GripperRawCommandedPositionPercent}%/{peripheral.GripperRawActualPositionPercent}%"
                : $"Gripper: Cmd {peripheral.GripperCommandedPositionPercent:0.##}% / Actual 확인 안 됨{holdSuffix} ({peripheral.GripperOpenRatio:0.00}) · raw {peripheral.GripperRawCommandedPositionPercent}%/{peripheral.GripperRawActualPositionPercent}%";
            snapshot.RobotDoSummary = $"DO0 {(peripheral.RobotDigitalOutputs[0] ? "ON" : "OFF")} / DO1 {(peripheral.RobotDigitalOutputs[1] ? "ON" : "OFF")}";
            snapshot.ToolDoSummary = $"ToolDO0 {(peripheral.ToolDigitalOutputs[0] ? "ON" : "OFF")} / ToolDO1 {(peripheral.ToolDigitalOutputs[1] ? "ON" : "OFF")}";
            snapshot.PeripheralFeedback = peripheral.LastPeripheralFeedback;
            snapshot.GripperSdkSummary = peripheral.LastGripperSdkSummary;
            snapshot.GripperReadbackNote = peripheral.LastGripperReadbackNote;
        }

        private RobotControlV3RuntimeStatusKind ResolveStatusKind()
        {
            if (!connectionService.Client.IsConnected)
            {
                return RobotControlV3RuntimeStatusKind.Disconnected;
            }

            if (connectionService.LastControllerFault.HasBlockingFault)
            {
                return RobotControlV3RuntimeStatusKind.Fault;
            }

            if (IsReadbackOnlyLiveClient())
            {
                return RobotControlV3RuntimeStatusKind.ConnectedServoOff;
            }

            if (!connectionService.Client.IsEnabled)
            {
                return RobotControlV3RuntimeStatusKind.ConnectedServoOff;
            }

            if (connectionService.LastState.JointPosDeg == null || connectionService.LastState.JointPosDeg.Length == 0)
            {
                return RobotControlV3RuntimeStatusKind.ConnectedUnsynced;
            }

            return RobotControlV3RuntimeStatusKind.ReadyToJog;
        }

        private string BuildCurrentPositionReadStatus()
        {
            return RobotControlV3OperatorStatusCopy.BuildCurrentPositionReadStatus(
                connectionService.Client.IsConnected,
                hasCurrentPositionReadComplete);
        }

        private string BuildConnectionChip()
        {
            return RobotControlV3OperatorStatusCopy.BuildConnectionChip(
                connectionService.Client.IsConnected,
                hasCurrentPositionReadComplete);
        }

        private string BuildLiveTrackingStatus(bool prioritizeLiveReadback, bool readbackOnlyLive)
        {
            return RobotControlV3OperatorStatusCopy.BuildLiveTrackingStatus(
                connectionService.Client.IsConnected,
                hasCurrentPositionReadComplete,
                prioritizeLiveReadback,
                snapshot.HasPendingPreview,
                readbackOnlyLive);
        }
    }
}
