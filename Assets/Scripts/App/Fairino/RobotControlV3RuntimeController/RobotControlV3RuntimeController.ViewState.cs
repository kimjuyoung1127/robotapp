// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private void ApplyVisualState()
        {
            var displayJointAngles = ShouldPrioritizeLiveReadbackDisplay()
                ? currentState.JointPosDeg
                : previewUsesJointPose && previewJointAnglesDeg != null
                    ? previewJointAnglesDeg
                    : currentState.JointPosDeg;

            if (displayJointAngles != null && displayJointAngles.Length >= templateDefinition.JointCount)
            {
                jointDriver?.ApplyJointAngles(displayJointAngles);
                kinematicsFacade?.SetJointAnglesDegrees(displayJointAngles);
                if (showTrail && kinematicsFacade != null)
                {
                    eeTrailRenderer?.AddPoint(kinematicsFacade.EndEffectorTransform);
                }

                if (kinematicsFacade != null)
                {
                    displacementArrow?.UpdateFromFK(kinematicsFacade.EndEffectorTransform);
                }
            }

            ApplyBaseAndToolFrameState();
            ApplyJointHighlightState();

            ghostRobotVisual?.SetVisible(false);
            predictedPathRenderer?.ClearPath();

            if (previewUsesJointPose && previewJointAnglesDeg != null && previewJointAnglesDeg.Length >= templateDefinition.JointCount)
            {
                ghostRobotVisual?.ApplyJointAngles(previewJointAnglesDeg);
                ghostRobotVisual?.SetVisible(showGhost);
                predictedPathRenderer?.RenderPath(BuildJointPreviewPath(currentState.JointPosDeg, previewJointAnglesDeg));
            }
            else if (previewTcpVisualJointAnglesDeg != null && previewTcpVisualJointAnglesDeg.Length >= templateDefinition.JointCount && previewTcpPose != null)
            {
                ghostRobotVisual?.ApplyJointAngles(previewTcpVisualJointAnglesDeg);
                ghostRobotVisual?.SetVisible(showGhost);
                predictedPathRenderer?.RenderPath(BuildJointPreviewPath(currentState.JointPosDeg, previewTcpVisualJointAnglesDeg));
            }
            else if (previewTcpPose != null && !previewUsesJointPose)
            {
                predictedPathRenderer?.RenderPath(BuildCartesianPreviewPath(currentState.TcpPose, previewTcpPose));
            }

            if (previewTcpPose != null && previewTcpPose.Length >= 3 && !previewUsesJointPose)
            {
                targetMarkerVisual?.SetMarkersVisible(true);
                if (targetMarkerVisual.TargetMarker != null)
                {
                    var pos = CoordConverter.ToUnityPosition(new Vec3D(previewTcpPose[0] / 1000.0, previewTcpPose[1] / 1000.0, previewTcpPose[2] / 1000.0));
                    targetMarkerVisual.TargetMarker.transform.position = pos;
                }
            }
            else
            {
                targetMarkerVisual?.SetMarkersVisible(false);
            }

            eeTrailRenderer?.SetVisible(showTrail);

            if (requestStageRefocus)
            {
                requestStageRefocus = false;
                ResetStageCameraIfAutomatic();
            }
        }

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
                    snapshot.PendingCommandSummary = previewUsesJointPose && previewJointAnglesDeg != null
                        ? "대기 명령: MoveJ"
                        : previewTcpPose != null
                            ? "대기 명령: MoveL"
                            : "대기 중인 명령 없음";
                    ApplyRetainedOperatorBlockedReasonToSnapshot();
                    if (string.IsNullOrWhiteSpace(snapshot.LiveBlockedReason)
                        && !string.IsNullOrWhiteSpace(snapshot.LastFeedback)
                        && snapshot.LastFeedback.StartsWith("[Live Gate] Blocked:", StringComparison.Ordinal))
                    {
                        snapshot.LiveBlockedReason = snapshot.LastFeedback;
                    }
                    ApplyMotionGateSnapshot();
                    snapshot.FailureCategory = BuildFailureCategory();
                    snapshot.OperatorNextAction = BuildOperatorNextAction();
                    snapshot.HeaderNextAction = busyAsyncReadback
                        ? $"다음 행동: {activeReadbackLabel} 완료 기다리기"
                        : $"다음 행동: {snapshot.OperatorNextAction}";
                    snapshot.QuickLiveArm = snapshot.MotionGateStatus;
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
                && currentLiveSessionMode == LiveCommandSessionMode.LiveControl;
        }

        private bool ShouldPrioritizeLiveReadbackDisplay()
        {
            return ShouldAutoFollowLiveReadback();
        }

        private void ClearPendingPreviewForLiveReadback()
        {
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            ClearPreparedMotionContext();
            if (currentLiveSessionMode == LiveCommandSessionMode.LiveControl)
            {
                InvalidateLiveApprovalContext();
            }
        }

        private void ApplyMotionGateSnapshot()
        {
            if (IsReadbackOnlyLiveClient())
            {
                snapshot.MotionGateReady = false;
                snapshot.MotionGateStatus = "실제 이동: 잠겨 있음";
                snapshot.MotionGateDetail = BuildReadbackOnlyGateDetail();
                snapshot.MotionGateWhyLocked = "잠금 이유: 현재 세션은 위치 확인 전용이라 실제 이동이 잠겨 있습니다.";
                snapshot.MotionGateUnlockWhen = "언제 풀리는지: readback-only가 아닌 세션에서 도구 설정, 작업 기준, 좌표 기준, 최신 기록, 마지막 확인이 모두 준비되면 tiny MoveJ 1회가 열립니다.";
                snapshot.MotionGateNextStep = hasCurrentPositionReadComplete
                    ? "다음 행동: latest-state와 latest-drift가 현재 세션 기준으로 갱신됐는지 확인한다."
                    : "다음 행동: 먼저 현재 위치 읽기를 완료해 실기와 화면 위치가 맞는지 확인한다.";
                snapshot.MotionGateConfirmTarget = "승인 대상: tiny MoveJ 1회";
                snapshot.MotionGateConfirmNote = "현재 세션은 위치 확인 전용이라 승인 토큰을 발급하지 않습니다.";
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
                LiveCommandGateStatus.RequiresConfirm => "실제 이동: 마지막 확인 필요",
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

            // readback-only live는 항상 "위치 확인" 축으로만 머물러야 한다.
            // background poll로 joints가 채워져도 운영자 primary action은 계속 현재 위치 읽기를 가리켜야 한다.
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

        private string ResolveQuickActionLabel()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete ? "연결 완료" : "현재 위치 다시 읽기";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "연결 + 위치 읽기",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "서보 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "동기화",
                RobotControlV3RuntimeStatusKind.Fault => "오류 초기화",
                _ => "조작 시작",
            };
        }

        private bool ResolveQuickActionEnabled()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff &&
                IsReadbackOnlyLiveClient() &&
                hasCurrentPositionReadComplete)
            {
                return false;
            }

            return snapshot.StatusKind != RobotControlV3RuntimeStatusKind.AutoReconnect;
        }

        private string BuildActionNow()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "지금 상태: 현재 위치 확인이 끝났습니다."
                    : "지금 상태: 연결은 됐고, 현재 위치 확인 전입니다.";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "지금 상태: 아직 미연결",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "지금 상태: 연결됨 / 서보 OFF",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "지금 상태: 서보 ON / 아직 미동기화",
                RobotControlV3RuntimeStatusKind.Fault => "지금 상태: Fault 발생",
                _ => snapshot.DryRunEnabled ? "지금 상태: DryRun 시뮬레이션 가능" : "지금 상태: 조작 가능",
            };
        }

        private string BuildActionPrimary()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "다음 행동: 연결 완료"
                    : "다음 행동: 현재 위치 다시 읽기";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "다음 행동: 연결하고 현재 위치 읽기",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "다음 행동: 서보를 먼저 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "다음 행동: 동기화 먼저",
                RobotControlV3RuntimeStatusKind.Fault => "다음 행동: 오류 초기화부터",
                _ => snapshot.PendingCommandSummary,
            };
        }

        private string BuildActionWhy()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "연결과 현재 위치 읽기가 함께 끝나서 화면과 실제 로봇 위치를 바로 비교할 수 있습니다."
                    : "지금은 실제로 움직이지 않고, 화면 위치와 실제 로봇 위치가 맞는지부터 확인하는 단계입니다.";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "현재 상태를 읽으려면 연결부터 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "실제 이동을 보내려면 서보가 먼저 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "첫 조작 전에 현재 자세를 읽는 게 덜 위험하다.",
                RobotControlV3RuntimeStatusKind.Fault => "초기화부터 누르면 같은 Fault를 다시 밟을 수 있다.",
                _ => snapshot.DryRunEnabled ? "지금은 실제 로봇 대신 화면 안에서만 미리보기 중입니다." : "지금 화면의 적용 버튼은 실제 로봇 동작으로 이어질 수 있습니다.",
            };
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

        private string BuildOperatorNextAction()
        {
            return BuildOperatorNextAction(BuildFailureCategory(), snapshot.MotionGateNextStep);
        }

        private static string BuildOperatorNextAction(string failureCategory, string fallbackAction)
        {
            return failureCategory switch
            {
                "network/SDK unavailable" => "8080 연결과 현재 위치 읽기를 다시 확인",
                "mode != 0" => "헤더 자동 버튼으로 자동 모드 전환",
                "drag/teach still on" => "티칭/드래그를 끄고 자동 모드를 다시 확인",
                "servo not ready" => "서보 ON 후 다시 미리보기/적용",
                "controller fault present" => "오류 초기화 후 현재 위치를 다시 읽기",
                "tool/user/coord missing" => "tool/user/coord를 다시 읽어 기준을 확정",
                "evidence stale" => "현재 위치 읽기와 latest-state/latest-drift를 다시 갱신",
                "gripper activation not ready" => "그리퍼 warm-up 뒤 다시 적용",
                "tiny range exceeded" => $"각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여 다시 적용",
                "sequence loop still locked" => "반복 대신 1회 실행만 사용",
                _ => StripActionPrefix(fallbackAction),
            };
        }

        private string BuildFailureCategory()
        {
            var liveBlocked = ResolveEffectiveOperatorBlockedReason();
            var lastFeedbackText = snapshot.LastFeedback ?? string.Empty;
            return ClassifyFailureCategory(liveBlocked, lastFeedbackText);
        }

        private string ClassifyFailureCategory(string liveBlocked, string lastFeedbackText)
        {
            if (!connectionService.Client.IsConnected
                || liveBlocked.Contains("8080")
                || liveBlocked.Contains("포트 확인 실패")
                || liveBlocked.Contains("not connected"))
            {
                return "network/SDK unavailable";
            }

            if (currentState.IsInDragTeach)
            {
                return "drag/teach still on";
            }

            if (!connectionService.IsMockMode && currentState.RobotMode != 0)
            {
                return "mode != 0";
            }

            if (connectionService.LastControllerFault.HasBlockingFault || liveBlocked.Contains("fault active"))
            {
                return "controller fault present";
            }

            if (!IsReadbackOnlyLiveClient() && !connectionService.Client.IsEnabled)
            {
                return "servo not ready";
            }

            if (liveBlocked.Contains("gripper activation not ready")
                || lastFeedbackText.Contains("gripper activation not ready"))
            {
                return "gripper activation not ready";
            }

            if (liveBlocked.Contains("tiny MoveJ range exceeded")
                || lastFeedbackText.Contains("tiny MoveJ range exceeded"))
            {
                return "tiny range exceeded";
            }

            if (liveBlocked.Contains("latest-state freshness failed")
                || liveBlocked.Contains("latest-drift freshness failed")
                || liveBlocked.Contains("state readback failed")
                || liveBlocked.Contains("drift threshold failed"))
            {
                return "evidence stale";
            }

            if (liveBlocked.Contains("toolId missing")
                || liveBlocked.Contains("userId missing")
                || liveBlocked.Contains("coordSystem unresolved"))
            {
                return "tool/user/coord missing";
            }

            if (liveBlocked.Contains("반복 live 실행은 아직 잠겨 있다")
                || lastFeedbackText.Contains("반복 live 실행은 아직 잠겨 있다"))
            {
                return "sequence loop still locked";
            }

            return "ready";
        }

        private static string StripActionPrefix(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return "먼저 연결";
            }

            const string prefix = "다음 행동: ";
            return action.StartsWith(prefix, StringComparison.Ordinal)
                ? action.Substring(prefix.Length)
                : action;
        }

        private string BuildReadbackOnlyGateDetail()
        {
            return $"지금은 실제 이동 없이 상태만 확인하는 단계입니다. {snapshot.StatusTool}, {snapshot.StatusUser}, 좌표 기준 {FormatCoordSystemDisplay(snapshot.CoordSystem)}으로 현재 위치가 맞는지 먼저 확인하세요.";
        }

        private string BuildMotionGateWhyLocked(LiveCommandSafetyGateResult gate)
        {
            if (gate == null)
            {
                return "잠금 이유: 게이트 상태를 아직 계산하지 못했습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "잠금 이유: 없음. tiny MoveJ 1회 기준을 모두 통과했습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "잠금 이유: 마지막 승인 토큰 확인이 남아 있습니다.";
            }

            if (gate.BlockReasons.Count > 0)
            {
                return $"잠금 이유: {string.Join(" / ", gate.BlockReasons.ConvertAll(TranslateGateReason))}";
            }

            return $"잠금 이유: {FormatMotionGateDetail(gate)}";
        }

        private string BuildMotionGateUnlockWhen(LiveCommandSafetyGateResult gate)
        {
            if (gate == null)
            {
                return "언제 풀리는지: 게이트 상태 확인 후 갱신됩니다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "언제 풀리는지: 지금 tiny MoveJ 1회를 저속으로 실행할 수 있습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "언제 풀리는지: 승인 토큰 확인이 끝나면 tiny MoveJ 1회가 열립니다.";
            }

            var remaining = BuildRemainingGateChecks(gate);
            return remaining.Count > 0
                ? $"언제 풀리는지: {string.Join(", ", remaining)} 준비 후 tiny MoveJ 1회가 열립니다."
                : "언제 풀리는지: 현재 위치 읽기, 최신 기록, 마지막 확인이 모두 준비되면 tiny MoveJ 1회가 열립니다.";
        }

        private string BuildMotionGateNextStep(LiveCommandSafetyGateResult gate)
        {
            if (!hasCurrentPositionReadComplete)
            {
                return "다음 행동: 현재 위치 읽기를 먼저 완료한다.";
            }

            if (gate == null)
            {
                return "다음 행동: 게이트 상태를 다시 계산한다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "다음 행동: tiny MoveJ 1회를 저속으로 실행하고 즉시 재잠금을 확인한다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "다음 행동: 승인 팝업에서 tiny MoveJ 1회 토큰 확인을 마친다.";
            }

            foreach (var reason in gate.BlockReasons)
            {
                switch (reason)
                {
                    case "toolId missing":
                        return "다음 행동: 도구 설정 번호를 먼저 확인한다.";
                    case "userId missing":
                        return "다음 행동: 작업 기준 번호를 먼저 확인한다.";
                    case "coordSystem unresolved":
                        return "다음 행동: 좌표 기준을 로봇 기준, 툴 기준, 작업 기준 중 하나로 확정한다.";
                    case "latest-state freshness failed":
                    case "state readback failed":
                        return "다음 행동: 현재 위치 읽기를 다시 실행해 latest-state를 갱신한다.";
                    case "latest-drift freshness failed":
                        return "다음 행동: latest-drift를 다시 만들어 최신 비교 기록을 확보한다.";
                    case "drift threshold failed":
                        return "다음 행동: 실기 위치와 화면 위치 차이를 먼저 줄인 뒤 다시 확인한다.";
                    case "tiny MoveJ range exceeded":
                        return $"다음 행동: 각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여 다시 미리보기한다.";
                    case "prepared target mismatch":
                        return "다음 행동: tiny MoveJ 대상을 다시 미리보기하고 동일한 대상으로 승인 절차를 다시 시작한다.";
                    case "dry-run preview artifact missing":
                        return "다음 행동: tiny MoveJ 미리보기를 먼저 확인한다.";
                    case "production IK guard not cleared":
                        return "다음 행동: tiny MoveJ 자세 계산 안전 확인을 먼저 통과시킨다.";
                    case "boundary data missing or target outside workspace":
                        return "다음 행동: 작은 범위 목표가 작업 범위 안인지 먼저 확인한다.";
                    case "collision data missing or predicted path unsafe":
                        return "다음 행동: tiny MoveJ 경로 충돌 확인을 먼저 끝낸다.";
                    case "operator approval target mismatch":
                        return "다음 행동: 승인 뒤 대상이 바뀌었으니 tiny MoveJ를 다시 미리보기하고 새 토큰으로 다시 승인한다.";
                    case "servo disabled":
                        return "다음 행동: 서보 상태를 확인하고 이동 가능 상태로 맞춘다.";
                    case "not connected":
                        return "다음 행동: 실제 로봇 연결을 다시 확인한다.";
                    case "operator confirm token required":
                        return "다음 행동: 승인 토큰 확인을 마친다.";
                }

                if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
                {
                    return "다음 행동: 오류를 초기화하고 현재 위치를 다시 확인한다.";
                }

                if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
                {
                    return "다음 행동: 이전 동작이 끝날 때까지 기다린다.";
                }
            }

            return "다음 행동: 잠금 이유를 확인하고 가장 먼저 막는 조건부터 해소한다.";
        }

        private string BuildMotionGateConfirmTarget(LiveCommandSafetyGateResult gate)
        {
            var label = "승인 대상: tiny MoveJ 1회";
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && pendingLiveApprovalRequired && pendingLiveApprovalKind == LiveCommandKind.MoveJ;
            var approvedActive = approvedLiveCommandUntilUtc > now && approvedLiveCommandKind == LiveCommandKind.MoveJ;

            if (approvedActive)
            {
                return $"{label} · 1회 승인됨";
            }

            if (pendingActive || gate?.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return $"{label} · 토큰 확인 대기";
            }

            return label;
        }

        private string BuildMotionGateConfirmNote(LiveCommandSafetyGateResult gate)
        {
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && pendingLiveApprovalRequired && pendingLiveApprovalKind == LiveCommandKind.MoveJ;
            var approvedActive = approvedLiveCommandUntilUtc > now && approvedLiveCommandKind == LiveCommandKind.MoveJ;

            if (approvedActive)
            {
                return "승인 토큰 확인 후 tiny MoveJ 1회만 허용되며, 실행 뒤에는 즉시 다시 잠깁니다.";
            }

            if (pendingActive || gate?.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "토큰은 승인 대상 다음에 나오는 확인값이며, 통과 후 tiny MoveJ 1회만 허용됩니다.";
            }

            return "승인 토큰은 모든 조건이 준비된 뒤 tiny MoveJ 1회 직전에만 발급됩니다.";
        }

        private List<string> BuildRemainingGateChecks(LiveCommandSafetyGateResult gate)
        {
            var remaining = new List<string>();
            if (gate == null)
            {
                return remaining;
            }

            foreach (var reason in gate.BlockReasons)
            {
                var label = reason switch
                {
                    "toolId missing" => "도구 설정 번호",
                    "userId missing" => "작업 기준 번호",
                    "coordSystem unresolved" => "좌표 기준",
                    "latest-state freshness failed" => "latest-state 최신성",
                    "latest-drift freshness failed" => "latest-drift 최신성",
                    "drift threshold failed" => "drift 기준 통과",
                    "tiny MoveJ range exceeded" => "작은 범위 기준",
                    "prepared target mismatch" => "동일 대상 미리보기",
                    "dry-run preview artifact missing" => "tiny MoveJ 미리보기",
                    "production IK guard not cleared" => "자세 계산 확인",
                    "boundary data missing or target outside workspace" => "작업 범위 확인",
                    "collision data missing or predicted path unsafe" => "충돌 확인",
                    "operator approval target mismatch" => "대상 재승인",
                    "operator confirm token required" => "승인 토큰 확인",
                    "servo disabled" => "서보 상태",
                    "not connected" => "실기 연결",
                    _ => string.Empty,
                };

                if (string.IsNullOrWhiteSpace(label))
                {
                    if (reason.StartsWith("state readback failed", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "현재 위치 읽기";
                    }
                    else if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "오류 초기화";
                    }
                    else if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "이전 동작 종료";
                    }
                }

                if (!string.IsNullOrWhiteSpace(label) && !remaining.Contains(label))
                {
                    remaining.Add(label);
                }
            }

            return remaining;
        }

        private string FormatMotionGateDetail(LiveCommandSafetyGateResult gate)
        {
            if (gate.Status == LiveCommandGateStatus.ReadbackOnly)
            {
                return BuildReadbackOnlyGateDetail();
            }

            if (gate.BlockReasons.Count > 0)
            {
                return string.Join(" / ", gate.BlockReasons.ConvertAll(TranslateGateReason));
            }

            if (gate.ClearedReasons.Count > 0)
            {
                return string.Join(" / ", gate.ClearedReasons.ConvertAll(TranslateGateReason));
            }

            return $"현재 기준: {snapshot.StatusTool}, {snapshot.StatusUser}, 좌표 기준 {FormatCoordSystemDisplay(snapshot.CoordSystem)}";
        }

        private string TranslateGateReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return string.Empty;
            }

            if (reason.StartsWith("speed ", StringComparison.OrdinalIgnoreCase))
            {
                return "현재 속도 설정이 안전 확인 기준보다 높습니다.";
            }

            if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
            {
                return "오류 코드가 남아 있어 먼저 초기화가 필요합니다.";
            }

            if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
            {
                return "이전 동작이 아직 끝나지 않았습니다.";
            }

            if (reason.StartsWith("state readback failed", StringComparison.OrdinalIgnoreCase))
            {
                return "현재 위치를 다시 읽지 못했습니다.";
            }

            return reason switch
            {
                "live client is readback-only" => "지금은 실제 로봇을 움직이지 않는 확인 단계입니다.",
                "actual motion/IO/gripper commands remain locked on macOS live readback" => "맥북 실기 연결은 현재 읽기 전용으로 잠겨 있습니다.",
                "toolId missing" => "도구 설정 번호를 먼저 확인해야 합니다.",
                "userId missing" => "작업 기준 번호를 먼저 확인해야 합니다.",
                "coordSystem unresolved" => "좌표 기준을 먼저 확정해야 합니다.",
                "latest-state freshness failed" => "최신 위치 증빙이 오래되어 현재 위치를 다시 읽어야 합니다.",
                "latest-drift freshness failed" => "최신 비교 증빙이 오래되어 다시 확인해야 합니다.",
                "drift threshold failed" => string.IsNullOrWhiteSpace(snapshot.LiveBlockedReason)
                    ? "실제 위치와 화면 위치 차이가 커서 이동이 잠겨 있습니다."
                    : snapshot.LiveBlockedReason,
                "tiny MoveJ range exceeded" => $"tiny MoveJ는 각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여야 합니다.",
                "prepared target mismatch" => "미리보기했던 tiny MoveJ 대상과 지금 실행 대상이 달라 다시 확인해야 합니다.",
                "operator approval target mismatch" => "승인 후 tiny MoveJ 대상이 바뀌어 새 승인 토큰이 필요합니다.",
                "operator confirm token required" => "실제 이동 전 마지막 확인이 필요합니다.",
                "operator confirm token accepted" => "실제 이동 전 마지막 확인이 끝났습니다.",
                "live preflight readback clear" => "현재 위치 읽기와 기본 점검이 끝났습니다.",
                "tiny MoveJ dedicated live path enabled" => "tiny MoveJ 전용 실기 통로가 열려 있습니다.",
                "tiny MoveJ range guard within 2.0deg" => $"tiny MoveJ 범위가 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 확인됐습니다.",
                "dry-run simulation" => "실제 로봇 대신 화면에서만 미리보기 중입니다.",
                "mock client" => "가상 로봇으로 시험 중입니다.",
                "not connected" => "먼저 로봇 연결이 필요합니다.",
                "servo disabled" => "실제 이동 전에는 서보를 켜야 합니다.",
                "emergency stop active" => "비상 정지 상태입니다.",
                "safety stop active" => "안전 정지 상태입니다.",
                "controller collision flag active" => "로봇이 충돌 위험 상태로 보고됐습니다.",
                "dry-run preview artifact missing" => "미리보기 확인이 아직 없습니다.",
                "production IK guard not cleared" => "자세 계산 확인이 아직 끝나지 않았습니다.",
                "boundary data missing or target outside workspace" => "이동 가능 범위 확인이 아직 끝나지 않았습니다.",
                "collision data missing or predicted path unsafe" => "충돌 위험 확인이 아직 끝나지 않았습니다.",
                "gripper readback missing" => "그리퍼 상태 확인이 아직 없습니다.",
                _ => reason,
            };
        }

        private static string FormatCoordSystemDisplay(string coordSystem)
        {
            return coordSystem switch
            {
                "Tool" => "툴 기준",
                "User" => "작업 기준",
                "Base" => "로봇 기준",
                _ => string.IsNullOrWhiteSpace(coordSystem) ? "--" : coordSystem,
            };
        }

        private string ResolveControllerModeLabel()
        {
            if (connectionService == null)
            {
                return "--";
            }

            if (connectionService.IsMockMode)
            {
                return "연습";
            }

            if (currentState.IsInDragTeach)
            {
                return "티칭";
            }

            return currentState.RobotMode switch
            {
                0 => "자동",
                1 => "수동",
                _ => $"모드 {currentState.RobotMode}",
            };
        }

        private string BuildControllerSessionSummary(bool readbackOnlyLive)
        {
            if (connectionService == null)
            {
                return "컨트롤러: --";
            }

            if (connectionService.IsMockMode)
            {
                return "컨트롤러: 연습 세션";
            }

            var sessionSummary = readbackOnlyLive
                ? "위치 확인 전용"
                : "실기 제어";
            var changedAt = lastControllerTruthChangedUtc == DateTime.MinValue
                ? "truth-change=unknown"
                : $"truth-change={lastControllerTruthChangedUtc.ToLocalTime():HH:mm:ss}";
            return $"컨트롤러: {sessionSummary} · {lastControllerTruthSummary} · {lastModeTransitionSummary} · {changedAt}";
        }

        private static string BuildLiveSessionModeDisplay(LiveCommandSessionMode mode)
        {
            return mode switch
            {
                LiveCommandSessionMode.LiveControl => "live-control",
                LiveCommandSessionMode.GripperOnly => "gripper-only",
                LiveCommandSessionMode.TinyMoveJOnly => "tiny-movej-only",
                _ => "readback-only",
            };
        }

        private static string BuildLiveSessionModeSummary(LiveCommandSessionMode mode)
        {
            return mode switch
            {
                LiveCommandSessionMode.LiveControl => "실기 세션: 통합 live 제어",
                LiveCommandSessionMode.GripperOnly => "실기 세션: gripper-only",
                LiveCommandSessionMode.TinyMoveJOnly => "실기 세션: tiny-movej-only",
                _ => "실기 세션: 위치 확인 전용",
            };
        }

        private string ResolveControllerModeChipClass()
        {
            if (connectionService == null)
            {
                return "rc-status-chip--muted";
            }

            if (connectionService.IsMockMode || currentState.IsInDragTeach || currentState.RobotMode == 1)
            {
                return "rc-status-chip--warning";
            }

            return "rc-status-chip--success";
        }

        private string ResolveControllerModeValueClass()
        {
            if (connectionService == null)
            {
                return "rc-status-value--muted";
            }

            if (connectionService.IsMockMode || currentState.IsInDragTeach || currentState.RobotMode == 1)
            {
                return "rc-status-value--warning";
            }

            return "rc-status-value--success";
        }

        private bool ResolveAutoModeSwitchEnabled()
        {
            return connectionService != null
                && connectionService.Client.IsConnected
                && !connectionService.IsMockMode
                && (currentState.IsInDragTeach || currentState.RobotMode != 0);
        }

        private bool ResolveManualModeSwitchEnabled()
        {
            return connectionService != null
                && connectionService.Client.IsConnected
                && !connectionService.IsMockMode
                && currentState.RobotMode != 1;
        }

        private static string FormatContextId(int id)
        {
            return id > 0 ? $"{id}번" : "미확인";
        }

        private static LiveCommandSessionMode ParseLiveCommandSessionMode(string sessionMode)
        {
            if (string.IsNullOrWhiteSpace(sessionMode))
            {
                return LiveCommandSessionMode.LiveControl;
            }

            return sessionMode.Trim().ToLowerInvariant() switch
            {
                "live-control" or "live" or "unified" => LiveCommandSessionMode.LiveControl,
                "gripper-only" or "gripper" => LiveCommandSessionMode.GripperOnly,
                "tiny-movej-only" or "tiny-movej" or "tinymovej-only" or "tinymovej" => LiveCommandSessionMode.TinyMoveJOnly,
                _ => LiveCommandSessionMode.LiveControl,
            };
        }

        private static string FormatToolDisplay(int toolId)
        {
            return toolId > 0 ? $"도구 {toolId}번" : "도구 미확인";
        }

        private static string FormatUserDisplay(int userId)
        {
            return userId > 0 ? $"작업 기준 {userId}번" : "작업 기준 미확인";
        }
    }
}
