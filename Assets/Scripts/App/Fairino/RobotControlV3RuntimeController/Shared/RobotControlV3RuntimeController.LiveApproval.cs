// Folder: Shared - live approval, session mode, and pending operator command helpers shared across V3 panels.
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
    // Handles shared live approval/session state for gripper, joint, and point/sequence operator flows.
    // Connection truth and evidence/gate summaries stay in ConnectionHome and StatusSafety partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string GrantLiveCommandApprovalForDebug(string commandKind, int ttlSeconds = 15)
        {
            var kind = ParseLiveCommandKind(commandKind);
            GrantLiveSessionApproval(kind, "DEBUG", ttlSeconds);
            return $"sessionApproved={HasActiveLiveCommandSessionApproval()}; kind={approvedLiveSessionKind}; token={approvedLiveSessionToken}; session={BuildApprovalTargetFingerprint(approvedLiveSessionKey)}; expires={approvedLiveSessionUntilUtc:O}";
        }


        public string SetLiveSessionModeForDebug(string sessionMode)
        {
            SetLiveSessionMode(ParseLiveCommandSessionMode(sessionMode));
            PushFeedback($"[Live Session] {BuildLiveSessionModeDisplay(currentLiveSessionMode)}");
            RefreshSnapshot();
            return $"sessionMode={currentLiveSessionMode}; summary={BuildLiveSessionModeSummary(currentLiveSessionMode)}";
        }


        public string GetLiveSessionModeSummaryForDebug()
        {
            return $"sessionMode={currentLiveSessionMode}; summary={BuildLiveSessionModeSummary(currentLiveSessionMode)}";
        }


        internal void SetLiveSessionMode(LiveCommandSessionMode mode)
        {
            if (currentLiveSessionMode == mode)
            {
                return;
            }

            currentLiveSessionMode = mode;
            InvalidateLiveApprovalContext();
        }


        public bool HasActiveLiveSessionApprovalForProduct()
        {
            return HasActiveLiveCommandSessionApproval();
        }


        public bool ShouldRequireLiveApprovalPopupForProduct(string commandKind)
        {
            var kind = ParseLiveCommandKind(commandKind);
            return kind != LiveCommandKind.ReadbackOnly
                && !snapshot.DryRunEnabled
                && !HasActiveLiveCommandSessionApproval();
        }


        public string BeginLiveCommandApprovalForProduct(string commandKind, int ttlSeconds = 30)
        {
            var kind = ParseLiveCommandKind(commandKind);
            ClearPendingLiveApproval();
            if (kind == LiveCommandKind.ReadbackOnly)
            {
                return "approvalRequired=False; kind=ReadbackOnly; token=none; reason=no live command pending";
            }

            if (kind == LiveCommandKind.MoveJ && ShouldUseSavedPointMoveJOperatorPath())
            {
                SetLiveSessionMode(hasPendingWaypointSequenceOperatorCommand && pendingWaypointSequenceLoop
                    ? LiveCommandSessionMode.LoopRunning
                    : LiveCommandSessionMode.LiveControl);
            }
            else if (kind == LiveCommandKind.MoveGripper && ShouldUseLiveGripperOperatorPath())
            {
                SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            }

            pendingLiveApprovalKind = kind;
            pendingLiveApprovalTargetKey = ResolvePreparedMotionTargetKey(kind);
            if (kind == LiveCommandKind.MoveJ
                && hasPendingSavedPointOperatorCommand
                && string.IsNullOrWhiteSpace(pendingLiveApprovalTargetKey))
            {
                pendingLiveApprovalTargetKey = pendingSavedPointOperatorTargetKey;
            }

            if (kind == LiveCommandKind.MoveJ
                && hasPendingWaypointSequenceOperatorCommand
                && string.IsNullOrWhiteSpace(pendingLiveApprovalTargetKey))
            {
                if (pendingWaypointSequenceLoop)
                {
                    pendingLiveApprovalTargetKey = ResolveLiveLoopContextKey(pendingWaypointSequenceName);
                }
                else if (TryLoadLiveWaypointSequence(pendingWaypointSequenceName, out var pendingSequence, out _))
                {
                    pendingLiveApprovalTargetKey = ResolveWaypointSequenceApprovalTargetKey(
                        pendingWaypointSequenceName,
                        pendingSequence);
                }
            }

            if ((kind == LiveCommandKind.MoveJ || kind == LiveCommandKind.MoveL)
                && string.IsNullOrWhiteSpace(pendingLiveApprovalTargetKey))
            {
                return $"approvalRequired=False; kind={kind}; token=none; target=none; reason=no prepared target";
            }

            var sessionKey = ResolveCurrentLiveApprovalSessionKey();
            if (HasActiveLiveCommandSessionApproval())
            {
                pendingLiveApprovalUntilUtc = approvedLiveSessionUntilUtc;
                pendingLiveApprovalToken = approvedLiveSessionToken;
                return $"approvalRequired=False; kind={kind}; token=SESSION; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; session={BuildApprovalTargetFingerprint(sessionKey)}; expires={approvedLiveSessionUntilUtc:O}; reason=session-approved";
            }

            pendingLiveApprovalUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(ttlSeconds, 5, 90));
            if (snapshot.DryRunEnabled)
            {
                pendingLiveApprovalRequired = false;
                return $"approvalRequired=False; kind={kind}; token=DRYRUN; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; session={BuildApprovalTargetFingerprint(sessionKey)}; expires={pendingLiveApprovalUntilUtc:O}; reason=dry-run";
            }

            pendingLiveApprovalRequired = true;
            pendingLiveApprovalToken = CreateShortToken();
            PushFeedback($"[Live Confirm] {kind} 시작 승인 토큰 {pendingLiveApprovalToken} 발급");
            RefreshSnapshot();
            return $"approvalRequired=True; kind={kind}; token={pendingLiveApprovalToken}; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; session={BuildApprovalTargetFingerprint(sessionKey)}; expires={pendingLiveApprovalUntilUtc:O}";
        }


        public bool TryConfirmLiveCommandApprovalForProduct(string token, out string summary)
        {
            if (!pendingLiveApprovalRequired)
            {
                summary = $"approved={HasActiveLiveCommandSessionApproval()}; approvalRequired=False; kind={pendingLiveApprovalKind}; token=DRYRUN; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; session={BuildApprovalTargetFingerprint(ResolveCurrentLiveApprovalSessionKey())}";
                ClearPendingLiveApproval();
                return true;
            }

            if (string.IsNullOrWhiteSpace(pendingLiveApprovalToken)
                || pendingLiveApprovalUntilUtc <= DateTime.UtcNow
                || !string.Equals(pendingLiveApprovalToken, token, StringComparison.Ordinal))
            {
                summary = $"approved=False; reason=invalid-or-expired-token; expected={pendingLiveApprovalToken}; actual={token}; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}";
                ClearPendingLiveApproval();
                PushFeedback("[Live Confirm] 승인 토큰이 만료되었거나 일치하지 않는다.");
                RefreshSnapshot();
                return false;
            }

            GrantLiveSessionApproval(
                pendingLiveApprovalKind,
                pendingLiveApprovalToken,
                Mathf.Max(1, (int)(pendingLiveApprovalUntilUtc - DateTime.UtcNow).TotalSeconds));
            summary = $"approved=True; kind={approvedLiveSessionKind}; token={approvedLiveSessionToken}; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; session={BuildApprovalTargetFingerprint(approvedLiveSessionKey)}; expires={approvedLiveSessionUntilUtc:O}";
            ClearPendingLiveApproval();
            PushFeedback($"[Live Confirm] {approvedLiveSessionKind} 시작 승인 완료 · 이번 연결의 live session을 유지한다.");
            RefreshSnapshot();
            return true;
        }


        public string ConfirmLiveCommandApprovalForDebug(string token)
        {
            return TryConfirmLiveCommandApprovalForProduct(token, out var summary)
                ? summary
                : summary;
        }


        public string CancelLiveCommandApprovalForProduct()
        {
            var summary = GetLiveCommandApprovalSummaryForDebug();
            ClearPendingLiveApproval();
            CancelPendingSavedPointOperatorCommand();
            CancelPendingGripperOperatorCommand();
            CancelPendingWaypointSequenceOperatorCommand();
            PushFeedback("[Live Confirm] 승인 요청 취소");
            RefreshSnapshot();
            return $"cancelled=True; before=[{summary}]";
        }


        public string GetLiveCommandApprovalSummaryForDebug()
        {
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && (pendingLiveApprovalRequired || pendingLiveApprovalKind != LiveCommandKind.ReadbackOnly);
            var approvedActive = approvedLiveCommandUntilUtc > now && approvedLiveCommandKind != LiveCommandKind.ReadbackOnly;
            var sessionApproved = HasActiveLiveCommandSessionApproval();
            return $"pending={pendingActive}; pendingRequired={pendingLiveApprovalRequired}; pendingKind={pendingLiveApprovalKind}; pendingToken={pendingLiveApprovalToken}; pendingTarget={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; pendingExpires={pendingLiveApprovalUntilUtc:O}; approved={approvedActive}; approvedKind={approvedLiveCommandKind}; approvedToken={approvedLiveCommandToken}; approvedTarget={BuildApprovalTargetFingerprint(approvedLiveCommandTargetKey)}; approvedExpires={approvedLiveCommandUntilUtc:O}; sessionApproved={sessionApproved}; sessionKind={approvedLiveSessionKind}; sessionToken={approvedLiveSessionToken}; sessionKey={BuildApprovalTargetFingerprint(approvedLiveSessionKey)}; sessionExpires={approvedLiveSessionUntilUtc:O}";
        }


        public string ResolvePendingLiveCommandKindForProduct()
        {
            if (hasPendingGripperOperatorCommand)
            {
                return LiveCommandKind.MoveGripper.ToString();
            }

            if (hasPendingSavedPointOperatorCommand)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (hasPendingWaypointSequenceOperatorCommand)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                return LiveCommandKind.MoveJ.ToString();
            }

            if (!previewUsesJointPose && previewTcpPose != null)
            {
                return LiveCommandKind.MoveL.ToString();
            }

            return LiveCommandKind.ReadbackOnly.ToString();
        }


        public bool ShouldRouteGripperOperatorThroughLiveApproval()
        {
            return ShouldUseLiveGripperOperatorPath();
        }


        public bool CanIssueLiveGripperOperatorWrite()
        {
            return ShouldUseLiveGripperOperatorPath() && snapshot.MotionGateReady;
        }


        public bool HasPendingGripperOperatorApproval()
        {
            return hasPendingGripperOperatorCommand;
        }


        public bool HasPendingWaypointSequenceOperatorApproval()
        {
            return hasPendingWaypointSequenceOperatorCommand;
        }


        public bool HasPendingSavedPointOperatorApproval()
        {
            return hasPendingSavedPointOperatorCommand;
        }


        public bool ShouldRouteMoveJOperatorThroughLiveApproval()
        {
            return ShouldUseLiveMoveJOperatorPath();
        }


        public bool ShouldRouteSavedPointMoveJOperatorThroughLiveApproval()
        {
            return ShouldUseSavedPointMoveJOperatorPath();
        }


        public string PrepareMoveJOperatorApprovalSession()
        {
            if (!ShouldUseLiveMoveJOperatorPath())
            {
                return "moveJOperatorApproval=False; reason=live operator path disabled";
            }

            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"moveJOperatorApproval=True; session={currentLiveSessionMode}; dryRun={snapshot.DryRunEnabled}";
        }


        public string PrepareSavedPointMoveJOperatorApproval(string pointName, double[] jointAnglesDeg)
        {
            if (!ShouldUseSavedPointMoveJOperatorPath())
            {
                return "savedPointMoveJOperatorApproval=False; reason=live operator path disabled";
            }

            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                return "savedPointMoveJOperatorApproval=False; reason=target missing";
            }

            hasPendingSavedPointOperatorCommand = true;
            pendingSavedPointOperatorName = string.IsNullOrWhiteSpace(pointName) ? "Point" : pointName.Trim();
            pendingSavedPointOperatorJointTarget = CopyJointArray(jointAnglesDeg);
            pendingSavedPointOperatorTargetKey = BuildMotionTargetKey(LiveCommandKind.MoveJ, pendingSavedPointOperatorJointTarget, null);
            pendingSavedPointOperatorRestoreDryRun = snapshot.DryRunEnabled;
            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingSavedPointApproval=True; point={pendingSavedPointOperatorName}; dryRun={snapshot.DryRunEnabled}";
        }


        public bool ShouldRouteWaypointSequenceThroughLiveApproval(string sequenceName, bool loop)
        {
            if (!ShouldUseSavedPointMoveJOperatorPath())
            {
                return false;
            }

            if (loop)
            {
                return CanBuildHomePoint1LoopSequence(out _, out _, out _);
            }

            return TryLoadLiveWaypointSequence(sequenceName, out var sequence, out _)
                && SequenceSupportsDirectLiveSequence(sequence);
        }


        public string PrepareGripperOperatorApproval(float positionPercent)
        {
            var clamped = Mathf.Clamp(positionPercent, 0f, 100f);
            var preflight = PreflightLiveGripperOperatorPath(allowWarmup: true);
            if (!preflight.IsSuccess)
            {
                ClearPendingGripperOperatorCommandState();
                RememberOperatorBlockedReason(preflight.Message);
                PushFeedback(preflight.Message);
                RefreshSnapshot();
                return preflight.Message;
            }

            hasPendingGripperOperatorCommand = true;
            pendingGripperOperatorPercent = clamped;
            pendingGripperOperatorRestoreDryRun = snapshot.DryRunEnabled;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
            snapshot.LiveBlockedReason = string.Empty;

            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingGripperApproval=True; percent={clamped:0.##}; dryRun={snapshot.DryRunEnabled}";
        }


        public string ExecutePendingGripperOperatorCommand()
        {
            if (!hasPendingGripperOperatorCommand)
            {
                return "pendingGripperApproval=False";
            }

            var clamped = pendingGripperOperatorPercent;
            var restoreDryRun = pendingGripperOperatorRestoreDryRun;
            ClearPendingGripperOperatorCommandState();
            var result = SetGripperPositionPercent(clamped);

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result.Message;
        }


        public string ExecutePendingSavedPointOperatorCommand()
        {
            if (!hasPendingSavedPointOperatorCommand || pendingSavedPointOperatorJointTarget == null)
            {
                return "pendingSavedPointApproval=False";
            }

            var pointName = pendingSavedPointOperatorName;
            var jointTarget = CopyJointArray(pendingSavedPointOperatorJointTarget);
            var restoreDryRun = pendingSavedPointOperatorRestoreDryRun;
            ClearPendingSavedPointOperatorCommandState();
            var result = ApplyTeachingMoveJ(jointTarget, $"저장 위치 {pointName} 저장된 관절 이동 적용");

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result.Message;
        }


        public string PrepareWaypointSequenceOperatorApproval(string sequenceName, bool loop, string startPointName = "")
        {
            WaypointSequence sequence;
            string resolvedSequenceName;
            string loadMessage;
            if (loop)
            {
                if (!CanBuildHomePoint1LoopSequence(out sequence, out resolvedSequenceName, out loadMessage))
                {
                    ClearPendingWaypointSequenceOperatorCommandState();
                    PushFeedback(loadMessage);
                    RefreshSnapshot();
                    return loadMessage;
                }
            }
            else if (!TryLoadLiveWaypointSequence(sequenceName, out sequence, out loadMessage))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return loadMessage;
            }
            else
            {
                resolvedSequenceName = string.IsNullOrWhiteSpace(sequenceName)
                    ? TeachingPointStoreAdapter.DefaultSequenceName
                    : sequenceName.Trim();
            }

            if (!SequenceSupportsDirectLiveSequence(sequence))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                const string unsupportedMessage = "[Sequence] direct live는 MoveJ 저장 포인트만 지원한다.";
                PushFeedback(unsupportedMessage);
                RefreshSnapshot();
                return unsupportedMessage;
            }

            var safeStartPointName = string.IsNullOrWhiteSpace(startPointName)
                ? string.Empty
                : startPointName.Trim();
            if (!string.IsNullOrWhiteSpace(safeStartPointName)
                && FindWaypointIndex(sequence, safeStartPointName) < 0)
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                var missingStartMessage = $"[Sequence] {safeStartPointName} 포인트를 찾지 못했다.";
                PushFeedback(missingStartMessage);
                RefreshSnapshot();
                return missingStartMessage;
            }

            hasPendingWaypointSequenceOperatorCommand = true;
            pendingWaypointSequenceName = resolvedSequenceName;
            pendingWaypointSequenceStartPointName = safeStartPointName;
            pendingWaypointSequenceRestoreDryRun = snapshot.DryRunEnabled;
            pendingWaypointSequenceLoop = loop;
            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.LiveControl);

            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingSequenceApproval=True; sequence={pendingWaypointSequenceName}; count={sequence.waypoints.Length}; loop={pendingWaypointSequenceLoop}; dryRun={snapshot.DryRunEnabled}";
        }


        public string ExecutePendingWaypointSequenceOperatorCommand()
        {
            if (!hasPendingWaypointSequenceOperatorCommand)
            {
                return "pendingSequenceApproval=False";
            }

            var sequenceName = pendingWaypointSequenceName;
            var startPointName = pendingWaypointSequenceStartPointName;
            var restoreDryRun = pendingWaypointSequenceRestoreDryRun;
            var loop = pendingWaypointSequenceLoop;
            ClearPendingWaypointSequenceOperatorCommandState();
            if (loop || string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                BeginLiveLoopApprovalContext(sequenceName);
            }

            if (liveWaypointSequenceCoroutine != null)
            {
                var alreadyRunning = $"[Sequence Run] {liveWaypointSequenceName} 실행 중이다. Stop 후 다시 실행해라.";
                PushFeedback(alreadyRunning);
                RefreshSnapshot();
                return alreadyRunning;
            }

            WaypointSequence sequence;
            string loadMessage;
            if (string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                if (!CanBuildHomePoint1LoopSequence(out sequence, out _, out loadMessage))
                {
                    PushFeedback(loadMessage);
                    RefreshSnapshot();
                    return loadMessage;
                }
            }
            else if (!TryLoadLiveWaypointSequence(sequenceName, out sequence, out loadMessage))
            {
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return loadMessage;
            }

            var startIndex = string.IsNullOrWhiteSpace(startPointName)
                ? 0
                : FindWaypointIndex(sequence, startPointName);
            if (startIndex < 0)
            {
                var startPointMissing = $"[Sequence Run] {startPointName} 포인트를 찾지 못했다.";
                PushFeedback(startPointMissing);
                RefreshSnapshot();
                return startPointMissing;
            }

            for (var index = startIndex; index < sequence.waypoints.Length; index++)
            {
                var point = sequence.waypoints[index];
                if (point == null)
                {
                    continue;
                }

                if (!string.Equals(point.moveType, "MoveJ", StringComparison.OrdinalIgnoreCase))
                {
                    var unsupported = $"[Sequence Run] {point.name} 실패 · direct live는 MoveJ만 지원한다.";
                    PushFeedback(unsupported);
                    RefreshSnapshot();
                    return unsupported;
                }
            }

            liveWaypointSequenceName = sequenceName;
            liveWaypointSequenceCoroutine = StartCoroutine(
                RunLiveWaypointSequence(
                    sequenceName,
                    sequence,
                    startIndex,
                    restoreDryRun,
                    loop,
                    applyGripperPattern: string.Equals(sequenceName, HomePoint1LoopSequenceName, StringComparison.OrdinalIgnoreCase)));
            PushFeedback(loop
                ? $"[Sequence Loop] {sequenceName} mixed live 루프 시작 · {sequence.waypoints.Length - startIndex}개 포인트"
                : $"[Sequence Run] {sequenceName} live 실행 시작 · {sequence.waypoints.Length - startIndex}개 포인트");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        private LiveCommandApprovalState ConsumeLiveCommandApproval(LiveCommandKind kind, string targetKey)
        {
            if (HasActiveLiveCommandSessionApproval())
            {
                return LiveCommandApprovalState.SessionActive;
            }

            if (approvedLiveCommandUntilUtc <= DateTime.UtcNow || approvedLiveCommandKind != kind)
            {
                ClearGrantedLiveApproval();
                return LiveCommandApprovalState.None;
            }

            if (!string.IsNullOrWhiteSpace(approvedLiveCommandTargetKey)
                && !string.Equals(approvedLiveCommandTargetKey, targetKey, StringComparison.Ordinal))
            {
                ClearGrantedLiveApproval();
                return LiveCommandApprovalState.TargetMismatch;
            }

            ClearGrantedLiveApproval();
            return LiveCommandApprovalState.Consumed;
        }


        private void GrantLiveCommandApproval(LiveCommandKind kind, string token, int ttlSeconds, string targetKey)
        {
            approvedLiveCommandKind = kind;
            approvedLiveCommandToken = string.IsNullOrWhiteSpace(token) ? CreateShortToken() : token;
            approvedLiveCommandTargetKey = targetKey ?? string.Empty;
            approvedLiveCommandUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(ttlSeconds, 1, 90));
        }


        private void GrantLiveSessionApproval(LiveCommandKind kind, string token, int ttlSeconds)
        {
            approvedLiveSessionKind = kind;
            approvedLiveSessionToken = string.IsNullOrWhiteSpace(token) ? CreateShortToken() : token;
            approvedLiveSessionKey = ResolveCurrentLiveApprovalSessionKey();
            approvedLiveSessionUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(Mathf.Max(ttlSeconds, 300), 300, 28800));
        }


        private void ClearPendingLiveApproval()
        {
            pendingLiveApprovalKind = LiveCommandKind.ReadbackOnly;
            pendingLiveApprovalUntilUtc = DateTime.MinValue;
            pendingLiveApprovalToken = string.Empty;
            pendingLiveApprovalTargetKey = string.Empty;
            pendingLiveApprovalRequired = false;
        }


        private void ClearGrantedLiveApproval()
        {
            approvedLiveCommandKind = LiveCommandKind.ReadbackOnly;
            approvedLiveCommandUntilUtc = DateTime.MinValue;
            approvedLiveCommandToken = string.Empty;
            approvedLiveCommandTargetKey = string.Empty;
        }


        private void ClearGrantedLiveSessionApproval()
        {
            approvedLiveSessionKind = LiveCommandKind.ReadbackOnly;
            approvedLiveSessionUntilUtc = DateTime.MinValue;
            approvedLiveSessionToken = string.Empty;
            approvedLiveSessionKey = string.Empty;
        }


        private bool HasActiveLiveCommandSessionApproval()
        {
            if (approvedLiveSessionUntilUtc <= DateTime.UtcNow || approvedLiveSessionKind == LiveCommandKind.ReadbackOnly)
            {
                return false;
            }

            var currentKey = ResolveCurrentLiveApprovalSessionKey();
            return !string.IsNullOrWhiteSpace(currentKey)
                && string.Equals(approvedLiveSessionKey, currentKey, StringComparison.Ordinal);
        }


        private void InvalidateLiveApprovalContext(bool resetSessionApproval = false)
        {
            ClearPendingLiveApproval();
            ClearGrantedLiveApproval();
            ClearLiveLoopApprovalContext();
            if (resetSessionApproval)
            {
                ClearGrantedLiveSessionApproval();
            }
        }


        private void ClearPendingWaypointSequenceOperatorCommandState()
        {
            hasPendingWaypointSequenceOperatorCommand = false;
            pendingWaypointSequenceName = string.Empty;
            pendingWaypointSequenceStartPointName = string.Empty;
            pendingWaypointSequenceRestoreDryRun = false;
            pendingWaypointSequenceLoop = false;
        }


        private void BeginLiveLoopApprovalContext(string sequenceName)
        {
            approvedLiveLoopContextKey = ResolveLiveLoopContextKey(sequenceName);
            approvedLiveLoopUntilUtc = DateTime.UtcNow.AddSeconds(90);
        }


        private bool HasActiveLiveLoopApproval()
        {
            return liveLoopApprovalExecutionContext
                && !string.IsNullOrWhiteSpace(approvedLiveLoopContextKey)
                && approvedLiveLoopUntilUtc > DateTime.UtcNow;
        }


        private void ClearLiveLoopApprovalContext()
        {
            approvedLiveLoopUntilUtc = DateTime.MinValue;
            approvedLiveLoopContextKey = string.Empty;
        }


        private static string ResolveLiveLoopContextKey(string sequenceName)
        {
            return string.IsNullOrWhiteSpace(sequenceName)
                ? HomePoint1LoopSequenceName
                : sequenceName.Trim();
        }


        private static string CreateShortToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }


        private string ResolvePreparedMotionTargetKey(LiveCommandKind kind)
        {
            return preparedLiveMotionContext.Kind == kind
                ? preparedLiveMotionContext.TargetKey ?? string.Empty
                : string.Empty;
        }


        private string ResolveCurrentLiveApprovalSessionKey()
        {
            var session = liveStateRecorder?.SessionId;
            if (string.IsNullOrWhiteSpace(session))
            {
                session = ResolveTinyMoveJEvidenceGateState().LatestState?.sessionId;
            }

            if (!string.IsNullOrWhiteSpace(session))
            {
                return session.Trim();
            }

            if (connectionService?.Client?.IsConnected == true)
            {
                return $"connected:{config.defaultIp}:{config.defaultPort}";
            }

            return string.Empty;
        }


        private string BuildApprovalTargetFingerprint(string targetKey)
        {
            return string.IsNullOrWhiteSpace(targetKey) ? "none" : ComputeStableFingerprint(targetKey);
        }


        private static string ComputeStableFingerprint(string value)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                var text = value ?? string.Empty;
                for (var index = 0; index < text.Length; index++)
                {
                    hash ^= text[index];
                    hash *= 1099511628211UL;
                }

                return hash.ToString("X8");
            }
        }


        private static LiveCommandKind ParseLiveCommandKind(string commandKind)
        {
            return commandKind switch
            {
                "MoveJ" => LiveCommandKind.MoveJ,
                "MoveL" => LiveCommandKind.MoveL,
                "RobotDo" => LiveCommandKind.RobotDo,
                "DO" => LiveCommandKind.RobotDo,
                "ToolDo" => LiveCommandKind.ToolDo,
                "ToolDO" => LiveCommandKind.ToolDo,
                "MoveGripper" => LiveCommandKind.MoveGripper,
                "Gripper" => LiveCommandKind.MoveGripper,
                _ => LiveCommandKind.ReadbackOnly,
            };
        }


        private void CapturePreparedMotionContext(
            LiveCommandKind kind,
            double[] jointTarget,
            double[] tcpTarget,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            string source)
        {
            preparedLiveMotionContext = new PreparedLiveMotionContext
            {
                Kind = kind,
                TargetKey = BuildMotionTargetKey(kind, jointTarget, tcpTarget),
                HasPreviewArtifact = jointTarget != null || tcpTarget != null,
                IsProductionIkSafe = productionIkSafe,
                IsBoundaryReady = boundaryReady,
                IsCollisionReady = collisionReady,
                Source = source ?? string.Empty,
            };
        }


        private void ClearPreparedMotionContext()
        {
            preparedLiveMotionContext = new PreparedLiveMotionContext();
        }


        private void CancelPendingGripperOperatorCommand()
        {
            var restoreDryRun = pendingGripperOperatorRestoreDryRun;
            ClearPendingGripperOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }


        private void CancelPendingSavedPointOperatorCommand()
        {
            var restoreDryRun = pendingSavedPointOperatorRestoreDryRun;
            ClearPendingSavedPointOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }


        private void CancelPendingWaypointSequenceOperatorCommand()
        {
            var restoreDryRun = pendingWaypointSequenceRestoreDryRun;
            ClearPendingWaypointSequenceOperatorCommandState();
            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
            }
        }


        private void ClearPendingGripperOperatorCommandState()
        {
            hasPendingGripperOperatorCommand = false;
            pendingGripperOperatorPercent = 100f;
            pendingGripperOperatorRestoreDryRun = false;
        }


        private void ClearPendingSavedPointOperatorCommandState()
        {
            hasPendingSavedPointOperatorCommand = false;
            pendingSavedPointOperatorName = string.Empty;
            pendingSavedPointOperatorJointTarget = null;
            pendingSavedPointOperatorTargetKey = string.Empty;
            pendingSavedPointOperatorRestoreDryRun = false;
        }


    }
}
