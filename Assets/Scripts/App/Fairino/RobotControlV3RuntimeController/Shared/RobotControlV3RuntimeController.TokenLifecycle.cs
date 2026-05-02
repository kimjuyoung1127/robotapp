// Folder: Shared - live approval token lifecycle, session approval state, and approval summaries shared across V3 panels.
using System;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles token issuance/confirmation/cancellation and session approval bookkeeping.
    // Pending command preparation and execution remain in CommandApproval and LoopApproval partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string GrantLiveCommandApprovalForDebug(string commandKind, int ttlSeconds = 15)
        {
            var kind = ParseLiveCommandKind(commandKind);
            GrantLiveSessionApproval(kind, "DEBUG", ttlSeconds);
            return $"sessionApproved={HasActiveLiveCommandSessionApproval()}; kind={approvedLiveSessionKind}; token={approvedLiveSessionToken}; session={BuildApprovalTargetFingerprint(approvedLiveSessionKey)}; expires={approvedLiveSessionUntilUtc:O}";
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


        private static string CreateShortToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
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
    }
}
