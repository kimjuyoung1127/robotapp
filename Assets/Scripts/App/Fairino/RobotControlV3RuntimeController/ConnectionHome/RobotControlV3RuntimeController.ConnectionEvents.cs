// Folder: ConnectionHome - connection event wiring and controller truth tracking for the V3 connection panel.
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
    // Handles connection state events, polled readback refresh timing, and controller truth tracking.
    // Stage/runtime visuals and evidence summaries are delegated to Stage and StatusSafety partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        private void BindConnectionEvents()
        {
            UnbindConnectionEvents();
            connectionService.OnStateUpdated += HandleStateUpdated;
            connectionService.OnConnectionStateChanged += HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged += HandleEnableStateChanged;
            connectionService.OnConnectionLost += HandleConnectionLost;
            connectionService.OnModeChanged += HandleModeChanged;
            // Subscribe the recorder after runtime handlers so initial live readback updates
            // currentState/visuals before drift comparison runs.
            liveStateRecorder?.Attach();
        }


        private void UnbindConnectionEvents()
        {
            if (connectionService == null)
            {
                return;
            }

            liveStateRecorder?.Detach();
            connectionService.OnStateUpdated -= HandleStateUpdated;
            connectionService.OnConnectionStateChanged -= HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged -= HandleEnableStateChanged;
            connectionService.OnConnectionLost -= HandleConnectionLost;
            connectionService.OnModeChanged -= HandleModeChanged;
        }


        private void HandleStateUpdated(FairinoRobotState state)
        {
            if (ShouldAutoFollowLiveReadback())
            {
                var now = Time.realtimeSinceStartupAsDouble;
                if (liveReadbackProbeUpdateCount == 0)
                {
                    liveReadbackProbeFirstUpdateTime = now;
                }

                liveReadbackProbeUpdateCount++;
                liveReadbackProbeLastUpdateTime = now;
            }

            currentState = state;
            UpdateControllerTruthTracking(state);
            templateDefinition.PosePresetProvider?.UpdateCurrent(state.JointPosDeg);
            if (ShouldAutoFollowLiveReadback())
            {
                hasCurrentPositionReadComplete = true;
                ClearPendingPreviewForLiveReadback();
            }

            CompleteAwaitingPolledReadbackIfNeeded();
            ApplyVisualState();
            if (ShouldAutoFollowLiveReadback())
            {
                pendingLiveSnapshotRefresh = true;
                FlushPendingLiveSnapshotRefresh();
                return;
            }

            RefreshSnapshot();
        }


        private void FlushPendingLiveSnapshotRefresh()
        {
            if (!pendingLiveSnapshotRefresh)
            {
                return;
            }

            var now = Time.realtimeSinceStartupAsDouble;
            if (now < nextAllowedLiveSnapshotRefreshTime)
            {
                return;
            }

            pendingLiveSnapshotRefresh = false;
            nextAllowedLiveSnapshotRefreshTime = now + LiveSnapshotRefreshIntervalSeconds;
            RefreshSnapshot();
        }


        private void HandleConnectionStateChanged(bool _)
        {
            if (!connectionService.Client.IsConnected)
            {
                hasCurrentPositionReadComplete = false;
                liveGripperWarmupAttemptedThisConnection = false;
                liveWaypointSequenceLooping = false;
                liveWaypointSequenceCycleCount = 0;
                liveWaypointCurrentTargetName = string.Empty;
                liveWaypointCurrentGripperIntent = string.Empty;
                liveWaypointBlockedReason = "[Connection] 연결이 끊겨 mixed live session 종료";
                InvalidateLiveApprovalContext(resetSessionApproval: true);
                currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            }

            RefreshSnapshot();
        }


        private void HandleEnableStateChanged(bool _)
        {
            RefreshSnapshot();
        }


        private void HandleConnectionLost()
        {
            hasCurrentPositionReadComplete = false;
            liveGripperWarmupAttemptedThisConnection = false;
            liveWaypointSequenceLooping = false;
            liveWaypointSequenceCycleCount = 0;
            liveWaypointCurrentTargetName = string.Empty;
            liveWaypointCurrentGripperIntent = string.Empty;
            liveWaypointBlockedReason = "[Connection] 연결 끊김 감지";
            InvalidateLiveApprovalContext(resetSessionApproval: true);
            currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            PushFeedback("[Connection] 연결 끊김 감지");
            RefreshSnapshot();
        }


        private void HandleModeChanged(bool _)
        {
            hasCurrentPositionReadComplete = false;
            lastControllerTruthSummary = connectionService != null && connectionService.IsMockMode
                ? "controller truth unavailable in mock"
                : lastControllerTruthSummary;
            if (connectionService != null
                && !connectionService.IsMockMode
                && currentState.RobotMode != 0
                && liveWaypointSequenceLooping)
            {
                liveWaypointBlockedReason = $"[Mode] mode={currentState.RobotMode} drift 감지";
                InvalidateLiveApprovalContext(resetSessionApproval: true);
                currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            }
            RefreshSnapshot();
        }


        private void UpdateControllerTruthTracking(FairinoRobotState state)
        {
            var changed = lastObservedRobotMode != state.RobotMode
                || !lastObservedDragTeach.HasValue
                || lastObservedDragTeach.Value != state.IsInDragTeach
                || !lastObservedRobotEnabled.HasValue
                || lastObservedRobotEnabled.Value != state.IsRobotEnabled;

            lastObservedRobotMode = state.RobotMode;
            lastObservedDragTeach = state.IsInDragTeach;
            lastObservedRobotEnabled = state.IsRobotEnabled;
            lastControllerTruthSummary =
                $"controller truth · mode={DescribeControllerMode(state.RobotMode)} · drag={(state.IsInDragTeach ? "on" : "off")} · servo={(state.IsRobotEnabled ? "on" : "off")}";

            if (changed)
            {
                lastControllerTruthChangedUtc = DateTime.UtcNow;
                if (!connectionService.IsMockMode)
                {
                    lastModeTransitionSummary = "외부/실기 controller 상태 변화 감지";
                    lastModeTransitionReason = $"{lastControllerTruthSummary} · observedAt={lastControllerTruthChangedUtc:O}";
                }
            }
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

    }
}
