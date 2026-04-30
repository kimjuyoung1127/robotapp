// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using System.Text;
using KineTutor3D.App.Fairino;
using KineTutor3D.UI.RobotControlV3;
using UnityEngine;

namespace KineTutor3D.App
{
    public static partial class RobotControlV3DebugBridge
    {
        [Serializable]
        private sealed class LiveQaArtifact
        {
            public string generatedAt;
            public string label;
            public string commandKind;
            public bool autoApprove;
            public string setupSummary;
            public string baselineEvidence;
            public string baselineMovement;
            public string baselineApproval;
            public string baselinePopup;
            public string prepareSummary;
            public string movementAfterPrepare;
            public string approvalAfterPrepare;
            public string popupAfterPrepare;
            public string executeSummary;
            public string approvalBeforeConfirm;
            public string popupBeforeConfirm;
            public string confirmSummary;
            public string afterEvidence;
            public string afterMovement;
            public string afterApproval;
            public string afterPopup;
            public string latestStatePath;
            public string latestDriftPath;
            public string latestEventsPath;
            public string latestReadbackPath;
            public string latestStateJson;
            public string latestDriftJson;
            public string latestEventsTail;
            public string latestReadbackTail;
            public float requestedDeltaDeg;
            public float actualDeltaDeg;
            public string requestedSequenceKind;
            public string operatorNextAction;
            public string failureCategory;
        }

        public static string CaptureLiveQaSnapshotForDebug(string label = "manual")
        {
            return RunLiveQaCommand(
                commandKind: "snapshot",
                label: label,
                autoApprove: false,
                setupAction: null,
                prepareAction: null,
                executeAction: null);
        }

        public static string RunLiveGripperQaForDebug(int positionPercent, bool autoApprove = true)
        {
            return RunLiveQaCommand(
                commandKind: "gripper",
                label: $"gripper-{Mathf.Clamp(positionPercent, 0, 100)}",
                autoApprove: autoApprove,
                setupAction: () => SetShellSelection("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"),
                prepareAction: () => SetEasyMotionGripperInputForDebug(positionPercent, apply: false),
                executeAction: () => SetEasyMotionGripperInputForDebug(positionPercent, apply: true));
        }

        public static string RunLiveJointNudgeQaForDebug(int axisNumber, int direction, bool autoApprove = true)
        {
            var deltaDeg = direction >= 0 ? 1f : -1f;
            return RunLiveJointDeltaQaForDebug(axisNumber, deltaDeg, autoApprove);
        }

        public static string RunLiveJointDeltaQaForDebug(int axisNumber, float requestedDeltaDeg, bool autoApprove = true)
        {
            return RunLiveQaCommand(
                commandKind: "joint-nudge",
                label: BuildJointDeltaArtifactLabel(axisNumber, requestedDeltaDeg),
                autoApprove: autoApprove,
                setupAction: () =>
                {
                    SetShellSelection("NavMotion", "TabJointJog", "BottomTabJointJog");
                    GetJointJogController().ForceInitialize();
                    var session = SetLiveSessionModeForDebug("tiny-movej-only");
                    return $"{session} | {GetJointJogController().GetDebugSummary()}";
                },
                prepareAction: () => PreviewJointDeltaForDebug(axisNumber, requestedDeltaDeg),
                executeAction: () => GetJointJogController().ApplyCurrentValuesForDebug(),
                requestedDeltaDeg: requestedDeltaDeg,
                jointAxisNumber: Mathf.Clamp(axisNumber, 1, 6));
        }

        public static string RunLiveTcpNudgeQaForDebug(string axisLabel, int direction, bool autoApprove = true, string coordSystem = "Base")
        {
            return RunLiveQaCommand(
                commandKind: "tcp-nudge",
                label: $"tcp-{NormalizeArtifactLabel(axisLabel)}-{(direction >= 0 ? "plus" : "minus")}",
                autoApprove: autoApprove,
                setupAction: () =>
                {
                    SetShellSelection("NavMotion", "TabTcpJog", "BottomTabTcpJog");
                    var tcpJog = GetTcpJogController();
                    tcpJog.ForceInitialize();
                    if (!string.IsNullOrWhiteSpace(coordSystem))
                    {
                        tcpJog.SetCoordSystemForDebug(coordSystem);
                    }

                    return tcpJog.GetDebugSummary();
                },
                prepareAction: () => NudgeTcpAxisForDebug(axisLabel, direction),
                executeAction: () => GetTcpJogController().ApplyCurrentPoseForDebug());
        }

        public static string RunLivePointMoveQaForDebug(
            string motionKind,
            float x,
            float y,
            float z,
            float rx,
            float ry,
            float rz,
            bool autoApprove = true)
        {
            return RunLiveQaCommand(
                commandKind: "point-move",
                label: $"point-{NormalizeArtifactLabel(motionKind)}",
                autoApprove: autoApprove,
                setupAction: () =>
                {
                    SetShellSelection("NavPoints", "TabTcpJog", "BottomTabPointMove");
                    var pointMove = GetPointMoveController();
                    pointMove.ForceInitialize();
                    var builder = new StringBuilder();
                    builder.Append(SetPointMoveMotionKindForDebug(string.IsNullOrWhiteSpace(motionKind) ? "MoveJ" : motionKind));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("X", x));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("Y", y));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("Z", z));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("RX", rx));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("RY", ry));
                    builder.Append(" | ").Append(SetPointMoveValueForDebug("RZ", rz));
                    return builder.ToString();
                },
                prepareAction: PreviewPointMoveForDebug,
                executeAction: ApplyPointMoveForDebug,
                requestedSequenceKind: string.IsNullOrWhiteSpace(motionKind) ? "MoveJ" : motionKind);
        }

        private static string RunLiveQaCommand(
            string commandKind,
            string label,
            bool autoApprove,
            Func<string> setupAction,
            Func<string> prepareAction,
            Func<string> executeAction,
            float requestedDeltaDeg = 0f,
            int jointAxisNumber = 0,
            string requestedSequenceKind = "")
        {
            var runtime = GetRuntimeController();
            var artifact = new LiveQaArtifact
            {
                generatedAt = DateTime.Now.ToString("O"),
                label = string.IsNullOrWhiteSpace(label) ? commandKind : label,
                commandKind = commandKind ?? "unknown",
                autoApprove = autoApprove,
                requestedDeltaDeg = requestedDeltaDeg,
                requestedSequenceKind = requestedSequenceKind ?? string.Empty,
            };
            var beforeJoint = CopyJointStateForQa(runtime);

            artifact.setupSummary = PrepareLiveQaRuntime(runtime);
            artifact.baselineEvidence = runtime.RefreshLiveEvidenceForDebug();
            artifact.baselineMovement = GetMovementStateSummaryForDebug();
            artifact.baselineApproval = GetLiveCommandApprovalSummaryForDebug();
            artifact.baselinePopup = GetPopupCoordinatorSummary();

            var liveEvidenceReady = runtime.HasStableLiveEvidenceForDebug();

            if (!liveEvidenceReady && executeAction != null)
            {
                artifact.prepareSummary = $"skipped=live-evidence-not-ready | {runtime.GetLiveEvidenceGateSummaryForDebug()}";
                artifact.movementAfterPrepare = GetMovementStateSummaryForDebug();
                artifact.approvalAfterPrepare = GetLiveCommandApprovalSummaryForDebug();
                artifact.popupAfterPrepare = GetPopupCoordinatorSummary();
                artifact.executeSummary = "skipped=live-evidence-not-ready";
            }
            else if (setupAction != null && prepareAction != null)
            {
                artifact.prepareSummary = $"{SafeInvoke(setupAction)} | {SafeInvoke(prepareAction)}";
                artifact.movementAfterPrepare = GetMovementStateSummaryForDebug();
                artifact.approvalAfterPrepare = GetLiveCommandApprovalSummaryForDebug();
                artifact.popupAfterPrepare = GetPopupCoordinatorSummary();
                artifact.executeSummary = SafeInvoke(executeAction);
            }
            else
            {
                artifact.prepareSummary = SafeInvoke(prepareAction ?? setupAction);
                artifact.movementAfterPrepare = GetMovementStateSummaryForDebug();
                artifact.approvalAfterPrepare = GetLiveCommandApprovalSummaryForDebug();
                artifact.popupAfterPrepare = GetPopupCoordinatorSummary();
                artifact.executeSummary = SafeInvoke(executeAction);
            }
            artifact.approvalBeforeConfirm = GetLiveCommandApprovalSummaryForDebug();
            artifact.popupBeforeConfirm = GetPopupCoordinatorSummary();
            artifact.confirmSummary = autoApprove ? TryConfirmPopupForQa() : "popup=left-open";
            var postConfirmJoint = CopyJointStateForQa(runtime);
            if (jointAxisNumber > 0)
            {
                artifact.actualDeltaDeg = ComputeActualJointDelta(beforeJoint, postConfirmJoint, jointAxisNumber);
            }

            artifact.afterEvidence = runtime.RefreshLiveEvidenceForDebug();
            artifact.afterMovement = GetMovementStateSummaryForDebug();
            artifact.afterApproval = GetLiveCommandApprovalSummaryForDebug();
            artifact.afterPopup = GetPopupCoordinatorSummary();
            artifact.operatorNextAction = runtime.CurrentSnapshot?.OperatorNextAction ?? string.Empty;
            artifact.failureCategory = runtime.CurrentSnapshot?.FailureCategory ?? string.Empty;

            PopulateEvidenceSnapshot(artifact);
            var artifactPath = WriteLiveQaArtifact(artifact);
            return $"qa={artifact.commandKind}; label={artifact.label}; artifact={artifactPath}; execute={artifact.executeSummary}; confirm={artifact.confirmSummary}; after={artifact.afterMovement}";
        }

        private static string BuildJointDeltaArtifactLabel(int axisNumber, float requestedDeltaDeg)
        {
            var axis = Mathf.Clamp(axisNumber, 1, 6);
            var direction = requestedDeltaDeg >= 0f ? "plus" : "minus";
            var magnitude = Mathf.Abs(requestedDeltaDeg).ToString("0.###").Replace('.', '_');
            return $"joint-j{axis}-{direction}-{magnitude}";
        }

        private static double[] CopyJointStateForQa(RobotControlV3RuntimeController runtime)
        {
            var state = runtime?.CurrentRobotStateForDebug.JointPosDeg;
            return state != null ? (double[])state.Clone() : null;
        }

        private static float ComputeActualJointDelta(double[] beforeJoint, double[] afterJoint, int jointAxisNumber)
        {
            var axisIndex = Mathf.Clamp(jointAxisNumber - 1, 0, 5);
            if (beforeJoint == null
                || afterJoint == null
                || beforeJoint.Length <= axisIndex
                || afterJoint.Length <= axisIndex)
            {
                return 0f;
            }

            return (float)(afterJoint[axisIndex] - beforeJoint[axisIndex]);
        }

        private static string PrepareLiveQaRuntime(RobotControlV3RuntimeController runtime)
        {
            runtime.StopMotion();
            var mode = runtime.SetMockModeForDebug(false);
            var disconnect = runtime.Disconnect();
            var connect = runtime.ConnectAndSyncDefault();
            var enable = runtime.EnableServo();
            runtime.SetTeachingLoopEnabled(false);
            if (runtime.CurrentSnapshot.DryRunEnabled)
            {
                runtime.ToggleDryRun();
            }

            var builder = new StringBuilder();
            builder.Append($"mode={mode}; disconnect={disconnect.Message}; connect={connect.Message}; servo={enable.Message}; dryRun={runtime.CurrentSnapshot.DryRunEnabled}");
            for (var attempt = 1; attempt <= 4; attempt++)
            {
                var sync = runtime.SyncCurrentState();
                var evidence = runtime.RefreshLiveEvidenceForDebug();
                var ready = runtime.HasStableLiveEvidenceForDebug();
                builder.Append($" | attempt{attempt}=sync:{sync.Message}; evidence:{evidence}; ready={ready}; summary:{runtime.GetLiveEvidenceGateSummaryForDebug()}");
                if (ready)
                {
                    break;
                }
            }

            return builder.ToString();
        }

        private static string ExecuteTinyMoveJPrimaryActionForDebug(bool autoApprove)
        {
            var runtime = GetRuntimeController();
            var before = runtime.GetTinyMoveJGateSummaryForDebug();
            var approval = "approval=skipped";
            if (autoApprove && before.Contains("operator confirm token required"))
            {
                approval = runtime.GrantLiveCommandApprovalForDebug("MoveJ");
            }

            var approved = runtime.GetTinyMoveJGateSummaryForDebug();
            var execute = ExecutePreparedPreviewForDebug();
            var after = runtime.GetTinyMoveJGateSummaryForDebug();
            return $"before={before} | approve={approval} | approved={approved} | execute={execute} | after={after}";
        }

        private static string TryConfirmPopupForQa()
        {
            var popup = GetPopupCoordinatorSummary();
            if (!popup.Contains("popupOpen=True"))
            {
                return "popup=not-open";
            }

            return ConfirmPopupForDebug();
        }

        private static void PopulateEvidenceSnapshot(LiveQaArtifact artifact)
        {
            var evidenceRoot = ResolveLiveEvidenceRoot();
            artifact.latestStatePath = Path.Combine(evidenceRoot, "latest-state.json");
            artifact.latestDriftPath = Path.Combine(evidenceRoot, "latest-drift.json");
            artifact.latestStateJson = SafeReadText(artifact.latestStatePath, 12000);
            artifact.latestDriftJson = SafeReadText(artifact.latestDriftPath, 12000);

            var sessionsRoot = Path.Combine(evidenceRoot, "sessions");
            artifact.latestEventsPath = FindLatestFile(sessionsRoot, "*-events.ndjson");
            artifact.latestReadbackPath = FindLatestFile(sessionsRoot, "*-readback.ndjson");
            artifact.latestEventsTail = ReadTailText(artifact.latestEventsPath, 24, 12000);
            artifact.latestReadbackTail = ReadTailText(artifact.latestReadbackPath, 24, 12000);
        }

        private static string WriteLiveQaArtifact(LiveQaArtifact artifact)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            var artifactDirectory = Path.Combine(projectRoot, "Artifacts", "live", "qa");
            Directory.CreateDirectory(artifactDirectory);
            var fileName = $"{DateTime.Now:yyyyMMdd-HHmmss}-{NormalizeArtifactLabel(artifact.label)}.json";
            var artifactPath = Path.Combine(artifactDirectory, fileName);
            File.WriteAllText(artifactPath, JsonUtility.ToJson(artifact, true), Encoding.UTF8);
            return artifactPath;
        }

        private static string ResolveLiveEvidenceRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            return Path.Combine(projectRoot, "Artifacts", "live", "fr5");
        }

        private static string FindLatestFile(string directoryPath, string pattern)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return string.Empty;
            }

            var files = Directory.GetFiles(directoryPath, pattern);
            if (files == null || files.Length == 0)
            {
                return string.Empty;
            }

            Array.Sort(files, StringComparer.Ordinal);
            return files[files.Length - 1];
        }

        private static string ReadTailText(string path, int maxLines, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return string.Empty;
            }

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0)
            {
                return string.Empty;
            }

            var start = Mathf.Max(0, lines.Length - Mathf.Max(1, maxLines));
            var builder = new StringBuilder();
            for (var index = start; index < lines.Length; index++)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(lines[index]);
            }

            return TrimToLength(builder.ToString(), maxChars);
        }

        private static string SafeReadText(string path, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return string.Empty;
            }

            return TrimToLength(File.ReadAllText(path), maxChars);
        }

        private static string TrimToLength(string value, int maxChars)
        {
            if (string.IsNullOrEmpty(value) || maxChars <= 0 || value.Length <= maxChars)
            {
                return value ?? string.Empty;
            }

            return value.Substring(0, maxChars);
        }

        private static string NormalizeArtifactLabel(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "qa";
            }

            var builder = new StringBuilder(raw.Length);
            foreach (var character in raw)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
                else if (character is '-' or '_')
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('-');
                }
            }

            return builder.Length == 0 ? "qa" : builder.ToString().Trim('-');
        }

        private static string SafeInvoke(Func<string> action)
        {
            if (action == null)
            {
                return string.Empty;
            }

            try
            {
                return action() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return $"{ex.GetType().Name}: {ex.Message}";
            }
        }
    }
}
