// Folder: ConnectionHome - live connect, sync, mode truth entry points for the V3 connection panel.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles connect/disconnect, sync, reset, mode requests, and other connection-home operator commands.
    // Connection event wiring lives in ConnectionEvents and diagnostics/evidence helpers live in StatusSafety.
    public sealed partial class RobotControlV3RuntimeController
    {
        public FairinoResult ConnectDefault()
        {
            hasCurrentPositionReadComplete = false;
            liveGripperWarmupAttemptedThisConnection = false;
            var result = connectionService.Connect(
                config.defaultIp,
                config.defaultPort,
                applyLiveBringupPolicies: false,
                emitConnectionStateChanged: false,
                emitEnableStateChanged: false,
                emitInitialState: false);
            if (result.IsSuccess)
            {
                PushFeedback($"[Connect] {result.Message}");
            }

            RefreshSnapshot();
            return result;
        }


        public FairinoResult ConnectAndSyncDefault()
        {
            var connectResult = ConnectDefault();
            if (!connectResult.IsSuccess)
            {
                return connectResult;
            }

            var syncResult = SyncCurrentState();
            return syncResult.IsSuccess
                ? FairinoResult.Ok("[Connect] 연결과 현재 위치 읽기 완료")
                : FairinoResult.Fail(syncResult.ErrorCode, syncResult.Message);
        }


        public string SetMockModeForDebug(bool useMock)
        {
            if (connectionService == null)
            {
                return "connectionService missing";
            }

            if (connectionService.Client.IsConnected)
            {
                connectionService.Disconnect();
            }

            connectionService.SetMockMode(useMock);
            if (!useMock && config != null)
            {
                connectionService.ApplyLiveDefaults(config.liveDefaults);
            }

            hasCurrentPositionReadComplete = false;
            liveGripperWarmupAttemptedThisConnection = false;
            currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            liveWaypointSequenceLooping = false;
            liveWaypointSequenceCycleCount = 0;
            liveWaypointCurrentTargetName = string.Empty;
            liveWaypointCurrentGripperIntent = string.Empty;
            liveWaypointBlockedReason = string.Empty;
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            ClearPreparedMotionContext();
            InvalidateLiveApprovalContext(resetSessionApproval: true);
            ApplyVisualState();
            PushFeedback(useMock ? "[Mode] Mock" : "[Mode] Live");
            RefreshSnapshot();
            return $"mock={connectionService.IsMockMode}; connected={connectionService.Client.IsConnected}; sessionMode={currentLiveSessionMode}";
        }


        public FairinoResult Disconnect()
        {
            var result = connectionService.Disconnect();
            hasCurrentPositionReadComplete = false;
            liveGripperWarmupAttemptedThisConnection = false;
            currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            liveWaypointSequenceLooping = false;
            liveWaypointSequenceCycleCount = 0;
            liveWaypointCurrentTargetName = string.Empty;
            liveWaypointCurrentGripperIntent = string.Empty;
            liveWaypointBlockedReason = string.Empty;
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            ClearPreparedMotionContext();
            InvalidateLiveApprovalContext();
            ApplyVisualState();
            PushFeedback($"[Disconnect] {result.Message}");
            RefreshSnapshot();
            return result;
        }


        public FairinoResult EnableServo()
        {
            var result = connectionService.Enable();
            PushFeedback(result.IsSuccess ? "[Servo] 서보 ON 완료" : result.Message);
            RefreshSnapshot();
            return result;
        }


        public FairinoResult RequestAutoMode()
        {
            return RequestControllerMode(0, "자동");
        }


        public FairinoResult RequestManualMode()
        {
            return RequestControllerMode(1, "수동");
        }


        public FairinoResult SyncCurrentState()
        {
            var result = connectionService.SyncCurrentState();
            if (result.IsSuccess)
            {
                hasCurrentPositionReadComplete = true;
                currentState = result.Value;
                templateDefinition.PosePresetProvider?.UpdateCurrent(result.Value.JointPosDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback("[Sync] 현재 자세 동기화 완료");
            }
            else
            {
                PushFeedback(result.Message);
            }

            RefreshSnapshot();
            return new FairinoResult(result.ErrorCode, result.Message);
        }


        public FairinoResult ResetErrors()
        {
            var result = connectionService.ResetErrors();
            PushFeedback(result.IsSuccess ? "[Reset] 오류 초기화 완료" : result.Message);
            RefreshSnapshot();
            return result;
        }


        private FairinoResult RequestControllerMode(int mode, string label)
        {
            if (!EnsureReadyForCommand($"{label} 모드 전환"))
            {
                lastModeTransitionSummary = $"{label} 모드 전환 준비 실패";
                lastModeTransitionReason = lastInitializationError;
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (!connectionService.Client.IsConnected)
            {
                var disconnected = FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
                lastModeTransitionSummary = $"{label} 모드 전환 실패";
                lastModeTransitionReason = disconnected.Message;
                PushFeedback(disconnected.Message);
                RefreshSnapshot();
                return disconnected;
            }

            var verifiedResult = connectionService.RequestControllerModeWithVerification(
                mode,
                exitDragTeachFirst: mode == 0 && !connectionService.IsMockMode);
            if (!verifiedResult.IsSuccess)
            {
                lastModeTransitionSummary = $"{label} 모드 전환 실패";
                lastModeTransitionReason = verifiedResult.Message;
                PushFeedback($"[Mode] {label} 모드 전환 실패 · {verifiedResult.Message}");
                RefreshSnapshot();
                return new FairinoResult(verifiedResult.ErrorCode, verifiedResult.Message);
            }

            currentState = verifiedResult.Value;
            hasCurrentPositionReadComplete = true;
            templateDefinition.PosePresetProvider?.UpdateCurrent(verifiedResult.Value.JointPosDeg);
            UpdateControllerTruthTracking(verifiedResult.Value);
            lastModeTransitionSummary = $"{label} 모드 전환 확인 완료";
            lastModeTransitionReason = verifiedResult.Message;
            PushFeedback($"[Mode] {label} 모드 전환 확인 완료 · {verifiedResult.Message}");
            RefreshSnapshot();
            return FairinoResult.Ok($"[Mode] {label} 모드 전환 확인 완료");
        }


        private bool EnsureReadyForCommand(string commandName)
        {
            if (TryInitialize())
            {
                return true;
            }

            var reason = string.IsNullOrWhiteSpace(lastInitializationError)
                ? "runtime 초기화 실패"
                : lastInitializationError;
            PushFeedback($"[{commandName}] {reason}");
            RefreshSnapshot();
            return false;
        }

    }
}
