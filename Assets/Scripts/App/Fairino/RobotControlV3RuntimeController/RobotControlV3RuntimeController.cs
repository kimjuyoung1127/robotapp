// Folder: App - Application controllers and services; single UnityEngine entry point.
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
    /// <summary>
    /// Pendant V3 실기/모의 런타임 상태, 시각화, 명령 진입점을 한 곳에 모읍니다.
    /// </summary>
    [DefaultExecutionOrder(-850)]
    public sealed partial class RobotControlV3RuntimeController : MonoBehaviour
    {
        private const float StageCameraFov = 32f;
        private const float StageCameraRotationSpeed = 0.25f;
        private const float StageCameraPanSpeed = 0.0018f;
        private const float StageCameraZoomSpeed = 0.08f;
        private const float StageCameraMinPitch = -80f;
        private const float StageCameraMaxPitch = 80f;
        private const double LiveEvidenceFreshnessWindowSeconds = 15d;
        private const string PgeaAttachmentResourcePath = "EndEffectors/PGEA_100_40";
        private const string PgeaAttachmentId = "PGEA_100_40";
        private static readonly Vector3 PgeaAttachmentLocalPosition = new(0.003f, 0.1676f, 0.031f);
        private static readonly Quaternion PgeaAttachmentLocalRotation = new(0f, 0f, -0.7169106f, 0.69716513f);
        private static readonly Vector3 PgeaTcpLocalPosition = new(-0.0677f, 0f, -0.0325f);
        private const float PgeaModelLocalZ = -0.031f;

        private readonly Stack<double[]> undoJointHistory = new();
        private readonly Stack<double[]> redoJointHistory = new();

        private RobotControlTemplateDefinition templateDefinition;
        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private RobotKinematicsFacade kinematicsFacade;
        private Transform runtimeRoot;
        private GameObject controlRobotInstance;
        private FairinoUrdfJointDriver jointDriver;
        private FrameGizmoFactory frameGizmoFactory;
        private FrameGizmo baseFrameGizmo;
        private FrameGizmo toolFrameGizmo;
        private EETrailRenderer eeTrailRenderer;
        private DisplacementArrow displacementArrow;
        private TargetMarkerVisual targetMarkerVisual;
        private GhostRobotVisual ghostRobotVisual;
        private PredictedPathRenderer predictedPathRenderer;
        private RobotStageFloorGrid stageFloorGrid;
        private RobotPartSelectionGizmo partSelectionGizmo;
        private SelectedLinkHighlighter selectedLinkHighlighter;
        private JointHighlightRing[] jointHighlightRings;
        private PresetTransitionAnimator presetAnimator;
        private WaypointCycleRunner waypointRunner;
        private RobotControlPeripheralFacade peripheralFacade;
        private LiveCommandSafetyGate liveCommandSafetyGate;
        private Fr5LiveStateRecorder liveStateRecorder;
        private ManualReadbackTeachingProbe manualReadbackTeachingProbe;
        private TeachingPointStoreAdapter teachingPointStoreAdapter;
        private TeachingSequenceRuntime teachingSequenceRuntime;
        private TeachingFunctionStore teachingFunctionStore;
        private TeachingBlockSequenceStore teachingBlockSequenceStore;
        private TeachingPathRecorder teachingPathRecorder;
        private WaypointSequence recordedPathSequence;
        private RobotKinematicsFacade previewKinematicsFacade;
        private FR5EndEffectorAttachment endEffectorAttachment;
        private Camera stageCamera;
        private Light stageLight;
        private Transform stageCameraPivot;
        private Vector3 stageCameraFocusPoint;
        private Vector3 stageCameraPanOffset;
        private float stageCameraDistance = 2.4f;
        private float stageCameraMinDistance = 0.35f;
        private float stageCameraMaxDistance = 8f;
        private float stageCameraYaw;
        private float stageCameraPitch;
        private bool stageCameraStateValid;
        private bool stageCameraUserAdjusted;
        private RobotControlV3RuntimeSnapshot snapshot = new();
        private FairinoRobotState currentState = FairinoRobotState.Zero();
        private bool hasCurrentPositionReadComplete;
        private double[] previewJointAnglesDeg;
        private double[] previewTcpPose;
        private double[] previewTcpVisualJointAnglesDeg;
        private bool previewUsesJointPose;
        private bool showBaseFrame;
        private bool showToolFrame;
        private bool showTrail = true;
        private bool showGhost;
        private bool showWorkspaceBoundary;
        private bool showCollision;
        private bool isPaused;
        private bool teachingLoopEnabled;
        private bool waypointRunnerEventsBound;
        private Coroutine liveWaypointSequenceCoroutine;
        private string liveWaypointSequenceName = string.Empty;
        private bool initialized;
        private bool isInitializing;
        private string lastInitializationError = string.Empty;
        private string lastSelectedPartName = "없음";
        private int activeJointHighlightIndex = -1;
        private float activeJointHighlightUntilTime;
        private bool requestStageRefocus;
        private int liveReadbackProbeUpdateCount;
        private double liveReadbackProbeFirstUpdateTime = -1d;
        private double liveReadbackProbeLastUpdateTime = -1d;
        private LiveCommandKind approvedLiveCommandKind = LiveCommandKind.ReadbackOnly;
        private DateTime approvedLiveCommandUntilUtc = DateTime.MinValue;
        private string approvedLiveCommandToken = string.Empty;
        private string approvedLiveCommandTargetKey = string.Empty;
        private LiveCommandKind pendingLiveApprovalKind = LiveCommandKind.ReadbackOnly;
        private DateTime pendingLiveApprovalUntilUtc = DateTime.MinValue;
        private string pendingLiveApprovalToken = string.Empty;
        private string pendingLiveApprovalTargetKey = string.Empty;
        private bool pendingLiveApprovalRequired;
        private bool hasPendingGripperOperatorCommand;
        private float pendingGripperOperatorPercent = 100f;
        private bool pendingGripperOperatorRestoreDryRun;
        private bool hasPendingSavedPointOperatorCommand;
        private string pendingSavedPointOperatorName = string.Empty;
        private double[] pendingSavedPointOperatorJointTarget;
        private string pendingSavedPointOperatorTargetKey = string.Empty;
        private bool pendingSavedPointOperatorRestoreDryRun;
        private bool hasPendingWaypointSequenceOperatorCommand;
        private string pendingWaypointSequenceName = string.Empty;
        private string pendingWaypointSequenceStartPointName = string.Empty;
        private bool pendingWaypointSequenceRestoreDryRun;
        private bool liveGripperWarmupAttemptedThisConnection;
        private LiveCommandSessionMode currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
        private PreparedLiveMotionContext preparedLiveMotionContext = new();
        private string lastControllerTruthSummary = "controller truth 대기";
        private string lastModeTransitionSummary = "모드 전환 시도 없음";
        private string lastModeTransitionReason = "아직 자동/수동 전환을 실행하지 않았다.";
        private string retainedOperatorBlockedReason = string.Empty;
        private string retainedOperatorFailureCategory = "ready";
        private string retainedOperatorNextAction = string.Empty;
        private DateTime lastControllerTruthChangedUtc = DateTime.MinValue;
        private int lastObservedRobotMode = int.MinValue;
        private bool? lastObservedDragTeach;
        private bool? lastObservedRobotEnabled;
        private bool isRefreshingSnapshot;
        private bool snapshotRefreshQueued;
        private const string RecordedPathSequenceName = "PendantV3RecordedPath";
        private const float JointHighlightHoldSeconds = 0.45f;

        internal event Action<RobotControlV3RuntimeSnapshot> SnapshotChanged;

        internal RobotControlV3RuntimeSnapshot CurrentSnapshot => snapshot.Clone();
        internal PendantV3PreviewState.Kind CurrentStateKind => ToPreviewKind(snapshot.StatusKind);
        internal Camera StageCamera => stageCamera;
        internal bool IsInitialized => initialized;
        internal FairinoConnectionService ConnectionServiceForDebug => connectionService;
        internal FairinoRobotState CurrentRobotStateForDebug => currentState;
        internal LiveCommandSessionMode CurrentLiveSessionModeForDebug => currentLiveSessionMode;
        public bool IsTeachingSequenceRunning => (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            || liveWaypointSequenceCoroutine != null
            || (teachingSequenceRuntime?.State.IsRunning ?? false);
        public bool IsTeachingLoopEnabled => teachingLoopEnabled;

        private void OnEnable()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            liveStateRecorder?.Detach();
            UnbindConnectionEvents();
            UnbindWaypointRunnerEvents();
            selectedLinkHighlighter?.Clear();
            partSelectionGizmo?.Clear();
            ClearJointHighlight();
            baseFrameGizmo?.SetVisible(false);
            toolFrameGizmo?.SetVisible(false);
            frameGizmoFactory?.SetVisible(false);
            initialized = false;
            isInitializing = false;
        }

        private void Update()
        {
            ApplyPendingReadbackStartUiIfNeeded();
            PollAsyncReadbackOperationCompletion();
            if (!HasPendingAsyncReadbackBackgroundTask())
            {
                connectionService?.Tick(Time.deltaTime);
            }

            if (teachingPathRecorder != null && teachingPathRecorder.Capture(currentState, Time.timeAsDouble))
            {
                RefreshSnapshot();
            }

            if (activeJointHighlightIndex >= 0 && Time.unscaledTime >= activeJointHighlightUntilTime)
            {
                ClearJointHighlight();
            }
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={initialized}; connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; dryRun={snapshot.DryRunEnabled}; session={currentLiveSessionMode}; pending={snapshot.PendingCommandSummary}; selected={lastSelectedPartName}; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; grid={(stageFloorGrid != null)}; gizmo={(partSelectionGizmo != null)}; initError={lastInitializationError}";
        }

        public string StartTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 시작"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Start(Time.timeAsDouble);
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            recordedPathSequence = null;
            PushFeedback("[Path Record] 기록 시작 · 현재 자세부터 샘플링");
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }

        public string StopTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 중지"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            teachingPathRecorder.Stop();
            recordedPathSequence = teachingPathRecorder.BuildSequence(RecordedPathSequenceName);
            var count = recordedPathSequence.waypoints?.Length ?? 0;
            if (count >= 2)
            {
                WaypointStore.Save(recordedPathSequence);
                PushFeedback($"[Path Record] 기록 저장 · {count}개 샘플 → {RecordedPathSequenceName}");
            }
            else
            {
                PushFeedback("[Path Record] 저장할 움직임이 부족하다. 최소 2개 자세가 필요함.");
            }

            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }

        public string CaptureTeachingPathFrameForDebug()
        {
            if (!EnsureReadyForCommand("경로 샘플 캡처"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }

        public string PlayRecordedTeachingPathOnce()
        {
            return PlayRecordedTeachingPath(loop: false);
        }

        public string PlayRecordedTeachingPathLoop()
        {
            return PlayRecordedTeachingPath(loop: true);
        }

        public string ExecuteWaypointSequenceOnce(string sequenceName)
        {
            return PlayNamedWaypointSequence(sequenceName, loop: false);
        }

        public string ExecuteWaypointSequenceLoop(string sequenceName)
        {
            return PlayNamedWaypointSequence(sequenceName, loop: true);
        }

        public string DeleteWaypointSequence(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                PushFeedback("[Sequence] 삭제할 실행 목록 이름이 비어 있다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Sequence] 실행 중에는 실행 목록을 삭제하지 않는다. Stop 후 다시 삭제해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var safeName = sequenceName.Trim();
            var ok = WaypointStore.Delete(safeName);
            if (string.Equals(safeName, RecordedPathSequenceName, StringComparison.OrdinalIgnoreCase))
            {
                recordedPathSequence = null;
            }

            PushFeedback(ok ? $"[Sequence] {safeName} 삭제" : $"[Sequence] {safeName} 삭제 실패");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }

        public string GetTeachingPathRecordingSummaryForDebug()
        {
            var recorder = teachingPathRecorder?.ToDebugSummary() ?? "recording=False; samples=0";
            var saved = ResolveRecordedPathSequence();
            var savedCount = saved?.waypoints?.Length ?? 0;
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            return $"{recorder}; saved={savedCount}; runner={runnerState}; sequence={RecordedPathSequenceName}; feedback={snapshot.LastFeedback}";
        }

        public string GetGripperVisualSummaryForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            var root = endEffectorAttachment.transform;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            var activeRendererCount = 0;
            var hasBounds = false;
            var bounds = new Bounds(root.position, Vector3.zero);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeRendererCount++;
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            var cameraVisible = false;
            if (stageCamera != null && hasBounds)
            {
                var view = stageCamera.WorldToViewportPoint(bounds.center);
                cameraVisible = view.z > 0f && view.x >= 0f && view.x <= 1f && view.y >= 0f && view.y <= 1f;
            }

            var local = root.localPosition;
            var euler = root.localEulerAngles;
            var viewport = stageCamera != null && hasBounds ? stageCamera.WorldToViewportPoint(bounds.center) : Vector3.zero;
            var tcpLocal = endEffectorAttachment.TcpFrame != null ? endEffectorAttachment.TcpFrame.localPosition : Vector3.zero;
            var modelLocal = endEffectorAttachment.ModelRoot != null ? endEffectorAttachment.ModelRoot.localPosition : Vector3.zero;
            var fingerLeftLocal = endEffectorAttachment.FingerLeft != null ? endEffectorAttachment.FingerLeft.localPosition : Vector3.zero;
            var fingerRightLocal = endEffectorAttachment.FingerRight != null ? endEffectorAttachment.FingerRight.localPosition : Vector3.zero;
            return $"attached=True; active={root.gameObject.activeInHierarchy}; renderers={renderers.Length}; activeRenderers={activeRendererCount}; meshFilters={meshFilters.Length}; local=({local.x:0.###},{local.y:0.###},{local.z:0.###}); rot=({euler.x:0.#},{euler.y:0.#},{euler.z:0.#}); scale=({root.localScale.x:0.###},{root.localScale.y:0.###},{root.localScale.z:0.###}); tcpLocal=({tcpLocal.x:0.####},{tcpLocal.y:0.####},{tcpLocal.z:0.####}); modelLocal=({modelLocal.x:0.####},{modelLocal.y:0.####},{modelLocal.z:0.####}); fingerLeft=({fingerLeftLocal.x:0.####},{fingerLeftLocal.y:0.####},{fingerLeftLocal.z:0.####}); fingerRight=({fingerRightLocal.x:0.####},{fingerRightLocal.y:0.####},{fingerRightLocal.z:0.####}); closure=({endEffectorAttachment.BuildClosureDebugSummary()}); boundsCenter=({bounds.center.x:0.###},{bounds.center.y:0.###},{bounds.center.z:0.###}); boundsSize=({bounds.size.x:0.###},{bounds.size.y:0.###},{bounds.size.z:0.###}); viewport=({viewport.x:0.###},{viewport.y:0.###},{viewport.z:0.###}); cameraVisible={cameraVisible}; openRatio={endEffectorAttachment.GripperOpenRatio:0.00}";
        }

        public string RecaptureGripperAuthoredOpenForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.RecaptureAuthoredOpenPose();
            ApplyGripperVisual(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
            return GetGripperVisualSummaryForDebug();
        }

        public string RecaptureGripperAuthoredClosedForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.RecaptureAuthoredClosedPose();
            ApplyGripperVisual(0f);
            return GetGripperVisualSummaryForDebug();
        }

        public string ClearGripperAuthoredClosedForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.ClearAuthoredClosedPose();
            ApplyGripperVisual(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
            return GetGripperVisualSummaryForDebug();
        }

        public string GetGripperSdkSummaryForDebug(bool includeReadback)
        {
            ForceInitialize();
            var summary = peripheralFacade != null
                ? peripheralFacade.GetGripperSdkSummary(includeReadback)
                : "sdkGripper=blocked; reason=peripheral facade missing";
            RefreshSnapshot();
            return summary;
        }

        public string GrantLiveCommandApprovalForDebug(string commandKind, int ttlSeconds = 15)
        {
            var kind = ParseLiveCommandKind(commandKind);
            GrantLiveCommandApproval(kind, "DEBUG", ttlSeconds, ResolvePreparedMotionTargetKey(kind));
            return $"approved={approvedLiveCommandKind}; token={approvedLiveCommandToken}; target={BuildApprovalTargetFingerprint(approvedLiveCommandTargetKey)}; expires={approvedLiveCommandUntilUtc:O}";
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
            currentLiveSessionMode = mode;
            if (currentLiveSessionMode == LiveCommandSessionMode.LiveControl)
            {
                InvalidateLiveApprovalContext();
            }
        }

        public string BeginLiveCommandApprovalForProduct(string commandKind, int ttlSeconds = 30)
        {
            var kind = ParseLiveCommandKind(commandKind);
            ClearPendingLiveApproval();
            if (kind == LiveCommandKind.ReadbackOnly)
            {
                return "approvalRequired=False; kind=ReadbackOnly; token=none; reason=no live command pending";
            }

            if (kind == LiveCommandKind.MoveJ
                && ShouldUseLiveMoveJOperatorPath())
            {
                SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);
            }
            else if (kind == LiveCommandKind.MoveGripper
                     && ShouldUseLiveGripperOperatorPath())
            {
                SetLiveSessionMode(LiveCommandSessionMode.GripperOnly);
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
                && string.IsNullOrWhiteSpace(pendingLiveApprovalTargetKey)
                && TryLoadLiveWaypointSequence(pendingWaypointSequenceName, out var pendingSequence, out _))
            {
                pendingLiveApprovalTargetKey = ResolveWaypointSequenceApprovalTargetKey(
                    pendingWaypointSequenceName,
                    pendingSequence);
            }

            if ((kind == LiveCommandKind.MoveJ || kind == LiveCommandKind.MoveL)
                && string.IsNullOrWhiteSpace(pendingLiveApprovalTargetKey))
            {
                return $"approvalRequired=False; kind={kind}; token=none; target=none; reason=no prepared target";
            }

            pendingLiveApprovalUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(ttlSeconds, 5, 90));
            if (snapshot.DryRunEnabled)
            {
                pendingLiveApprovalRequired = false;
                return $"approvalRequired=False; kind={kind}; token=DRYRUN; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; expires={pendingLiveApprovalUntilUtc:O}; reason=dry-run";
            }

            pendingLiveApprovalRequired = true;
            pendingLiveApprovalToken = CreateShortToken();
            PushFeedback($"[Live Confirm] {kind} 승인 토큰 {pendingLiveApprovalToken} 발급");
            RefreshSnapshot();
            return $"approvalRequired=True; kind={kind}; token={pendingLiveApprovalToken}; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; expires={pendingLiveApprovalUntilUtc:O}";
        }

        public bool TryConfirmLiveCommandApprovalForProduct(string token, out string summary)
        {
            if (!pendingLiveApprovalRequired)
            {
                summary = $"approved=False; approvalRequired=False; kind={pendingLiveApprovalKind}; token=DRYRUN; target={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}";
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

            GrantLiveCommandApproval(
                pendingLiveApprovalKind,
                pendingLiveApprovalToken,
                Mathf.Max(1, (int)(pendingLiveApprovalUntilUtc - DateTime.UtcNow).TotalSeconds),
                pendingLiveApprovalTargetKey);
            summary = $"approved=True; kind={approvedLiveCommandKind}; token={approvedLiveCommandToken}; target={BuildApprovalTargetFingerprint(approvedLiveCommandTargetKey)}; expires={approvedLiveCommandUntilUtc:O}";
            ClearPendingLiveApproval();
            PushFeedback($"[Live Confirm] {approvedLiveCommandKind} 1회 승인 토큰 확인");
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
            return $"pending={pendingActive}; pendingRequired={pendingLiveApprovalRequired}; pendingKind={pendingLiveApprovalKind}; pendingToken={pendingLiveApprovalToken}; pendingTarget={BuildApprovalTargetFingerprint(pendingLiveApprovalTargetKey)}; pendingExpires={pendingLiveApprovalUntilUtc:O}; approved={approvedActive}; approvedKind={approvedLiveCommandKind}; approvedToken={approvedLiveCommandToken}; approvedTarget={BuildApprovalTargetFingerprint(approvedLiveCommandTargetKey)}; approvedExpires={approvedLiveCommandUntilUtc:O}";
        }

        public string SimulateManualReadbackForDebug(double[] jointsDeg, double[] tcpMm)
        {
            ForceInitialize();
            manualReadbackTeachingProbe ??= new ManualReadbackTeachingProbe(connectionService);
            var result = manualReadbackTeachingProbe.SimulateManualMove(jointsDeg, tcpMm);
            RefreshSnapshot();
            return result.IsSuccess
                ? $"manualReadback=True; {FormatRobotStateForDebug(result.Value)}; {GetDebugSummary()}"
                : $"manualReadback=False; error={result.Message}; {GetDebugSummary()}";
        }

        public string GetTeachingPointStoreSummaryForDebug()
        {
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            return teachingPointStoreAdapter.BuildSummary();
        }

        public string LoadTeachingSequenceForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            return $"{teachingSequenceRuntime.ToDebugSummary()}; {GetTeachingLoopSummaryForDebug()}";
        }

        public string GetTeachingLoopSummaryForDebug()
        {
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            var runnerIndex = waypointRunner != null ? waypointRunner.CurrentIndex : -1;
            var runnerTotal = waypointRunner != null ? waypointRunner.TotalCount : 0;
            return $"loopEnabled={teachingLoopEnabled}; runnerState={runnerState}; runnerIndex={runnerIndex}; runnerTotal={runnerTotal}; isTeachingRunning={IsTeachingSequenceRunning}";
        }

        public bool SetTeachingLoopEnabled(bool enabled)
        {
            teachingLoopEnabled = enabled;
            PushFeedback(enabled ? "[Teaching Loop] 반복 실행 ON" : "[Teaching Loop] 반복 실행 OFF");
            RefreshSnapshot();
            return teachingLoopEnabled;
        }

        public bool ToggleTeachingLoopEnabled()
        {
            return SetTeachingLoopEnabled(!teachingLoopEnabled);
        }

        public string SelectTeachingPointForDebug(int index)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            return teachingSequenceRuntime.ToDebugSummary();
        }

        public string PreviewSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }

        public string ExecuteSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }

        public string ExecuteTeachingSequenceFromPoint(string pointName)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Teaching From] 실행할 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var startIndex = FindWaypointIndex(sequence, pointName);
            if (startIndex < 0)
            {
                PushFeedback($"[Teaching From] {pointName} 포인트를 찾지 못했다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Teaching From] 실행 중인 반복이 있다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var executed = 0;
            for (var index = startIndex; index < sequence.waypoints.Length; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = ExecuteTeachingWaypoint(sequence.waypoints[index]);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching From] {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                executed++;
            }

            PushFeedback($"[Teaching From] {startIndex + 1}/{sequence.waypoints.Length}부터 {executed}개 포인트 실행 완료");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }

        public string ExecuteTeachingSequenceFromPointForDebug(string pointName)
        {
            return ExecuteTeachingSequenceFromPoint(pointName);
        }

        public string CreateTeachingFunctionFromSequence(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function] 묶을 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var function = teachingFunctionStore.CreateFromSequence(teachingFunctionStore.BuildUniqueName(functionName), sequence);
            if (function == null || !teachingFunctionStore.Save(function))
            {
                PushFeedback("[Function] 함수 저장 실패");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            PushFeedback($"[Function] {function.name} 생성 · {function.steps.Length}개 포인트");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildDetail(function.name)}";
        }

        public string CreateTeachingFunctionFromPoints(string functionName, string[] pointNames)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function] 묶을 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (pointNames == null || pointNames.Length == 0)
            {
                return CreateTeachingFunctionFromSequence(functionName);
            }

            var filtered = new System.Collections.Generic.List<string>();
            for (var index = 0; index < pointNames.Length; index++)
            {
                var pointName = pointNames[index]?.Trim();
                if (string.IsNullOrWhiteSpace(pointName))
                {
                    continue;
                }

                if (FindWaypoint(sequence, pointName) == null)
                {
                    PushFeedback($"[Function] {pointName} 포인트를 찾지 못했다.");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                if (!filtered.Contains(pointName))
                {
                    filtered.Add(pointName);
                }
            }

            if (filtered.Count == 0)
            {
                return CreateTeachingFunctionFromSequence(functionName);
            }

            var function = teachingFunctionStore.CreateFromPointRefs(teachingFunctionStore.BuildUniqueName(functionName), filtered.ToArray(), TeachingPointStoreAdapter.DefaultSequenceName);
            if (function == null || !teachingFunctionStore.Save(function))
            {
                PushFeedback("[Function] 함수 저장 실패");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            PushFeedback($"[Function] {function.name} 생성 · 선택 {function.steps.Length}개 포인트");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {GetTeachingFunctionDetailForDebug(function.name)}";
        }

        public string GetTeachingFunctionSummaryForDebug()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            return teachingFunctionStore.BuildSummary();
        }

        public string[] GetTeachingFunctionNames()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            return teachingFunctionStore.LoadAllNames();
        }

        public string GetTeachingFunctionDetailForDebug(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var detail = teachingFunctionStore.BuildDetail(functionName);
            var function = teachingFunctionStore.Load(functionName);
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (function?.steps == null)
            {
                return detail;
            }

            var missing = new System.Collections.Generic.List<string>();
            for (var index = 0; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step == null || !step.enabled || !string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (FindWaypoint(sequence, step.refName) == null)
                {
                    missing.Add(step.refName);
                }
            }

            return $"{detail}; missingCount={missing.Count}; missing=[{string.Join(",", missing)}]";
        }

        public string RenameTeachingFunctionForDebug(string oldName, string newName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var ok = teachingFunctionStore.Rename(oldName, newName);
            PushFeedback(ok ? $"[Function] {oldName} -> {newName}" : "[Function] 이름 변경 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }

        public string DuplicateTeachingFunctionForDebug(string sourceName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var copy = teachingFunctionStore.Duplicate(sourceName);
            PushFeedback(copy != null ? $"[Function] {sourceName} 복사 -> {copy.name}" : "[Function] 복사 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }

        public string DeleteTeachingFunctionForDebug(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var ok = teachingFunctionStore.Delete(functionName);
            PushFeedback(ok ? $"[Function] {functionName} 삭제" : "[Function] 삭제 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }

        public string DeleteAllTeachingFunctionsForDebug()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var deleted = teachingFunctionStore.DeleteAll();
            PushFeedback($"[Bundle] 전체 묶음 {deleted}개 삭제");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }

        public string AddTeachingBlockPoint(string pointName)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (FindWaypoint(sequence, pointName) == null)
            {
                PushFeedback($"[Block Sequence] {pointName} 포인트를 찾지 못했다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var ok = teachingBlockSequenceStore.AddBlock(TeachingSequenceBlock.PointRefKind, pointName);
            PushFeedback(ok ? $"[Block Sequence] 포인트 {pointName} 추가" : "[Block Sequence] 포인트 추가 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string AddTeachingBlockBundle(string bundleName)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingFunctionStore ??= new TeachingFunctionStore();
            if (teachingFunctionStore.Load(bundleName) == null)
            {
                PushFeedback($"[Block Sequence] {bundleName} 묶음을 찾지 못했다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var ok = teachingBlockSequenceStore.AddBlock(TeachingSequenceBlock.BundleRefKind, bundleName);
            PushFeedback(ok ? $"[Block Sequence] 묶음 {bundleName} 추가" : "[Block Sequence] 묶음 추가 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string MoveTeachingBlock(int index, int direction)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var ok = teachingBlockSequenceStore.MoveBlock(index, direction);
            PushFeedback(ok ? $"[Block Sequence] {index}번 블록 이동" : "[Block Sequence] 블록 이동 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string DeleteTeachingBlock(int index)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var ok = teachingBlockSequenceStore.DeleteBlock(index);
            PushFeedback(ok ? $"[Block Sequence] {index}번 블록 삭제" : "[Block Sequence] 블록 삭제 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string ClearTeachingBlockSequenceForDebug()
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingBlockSequenceStore.Clear();
            PushFeedback("[Block Sequence] 작업 시퀀스 초기화");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string PreviewTeachingBlockSequence()
        {
            ForceInitialize();
            var sequence = ExpandTeachingBlockSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Block Preview] 미리보기할 작업 시퀀스가 없다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var result = PreviewTeachingWaypoint(sequence.waypoints[0]);
            PushFeedback(result.IsSuccess
                ? $"[Block Preview] 1/{sequence.waypoints.Length} {sequence.waypoints[0].name}"
                : result.Message);
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string ExecuteTeachingBlockSequenceDryRun()
        {
            ForceInitialize();
            var sequence = ExpandTeachingBlockSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Block Run] 실행할 작업 시퀀스가 없다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var restoreDryRun = !snapshot.DryRunEnabled;
            if (restoreDryRun)
            {
                ToggleDryRun();
            }

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Block Run] 이미 실행 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            waypointRunner.PlayOnce(sequence, dryRun: true);
            PushFeedback($"[Block Run] {sequence.waypoints.Length}개 포인트 DryRun 시작");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }

        public string GetTeachingBlockSequenceSummaryForDebug()
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var expanded = ExpandTeachingBlockSequence();
            var expandedCount = expanded?.waypoints?.Length ?? 0;
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            return $"{teachingBlockSequenceStore.BuildSummary()}; expanded={expandedCount}; runner={runnerState}; feedback={snapshot.LastFeedback}";
        }

        public string ExecuteTeachingFunctionOnceDryRun(string functionName)
        {
            return ExecuteTeachingFunctionDryRun(functionName, null);
        }

        public string ExecuteTeachingFunctionFromPointDryRun(string functionName, string pointName)
        {
            return ExecuteTeachingFunctionDryRun(functionName, pointName);
        }

        private string ExecuteTeachingFunctionDryRun(string functionName, string startPointName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var function = teachingFunctionStore.Load(functionName);
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (function?.steps == null || function.steps.Length == 0)
            {
                PushFeedback($"[Function Run] {functionName} 함수가 비어 있다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function Run] 참조할 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var startIndex = 0;
            if (!string.IsNullOrWhiteSpace(startPointName))
            {
                startIndex = FindFunctionStepIndex(function, startPointName);
                if (startIndex < 0)
                {
                    PushFeedback($"[Function Run] {function.name} 안에서 {startPointName} 참조를 찾지 못했다.");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }
            }

            var restoreDryRun = !snapshot.DryRunEnabled;
            if (restoreDryRun)
            {
                ToggleDryRun();
            }

            var executed = 0;
            for (var index = startIndex; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step == null || !step.enabled)
                {
                    continue;
                }

                if (!string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    PushFeedback($"[Function Run] {step.kind} step은 v1에서 제외다.");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                var point = FindWaypoint(sequence, step.refName);
                if (point == null)
                {
                    PushFeedback($"[Function Run] {step.refName} 포인트를 찾지 못했다.");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                var result = ExecuteTeachingWaypoint(point);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Function Run] {function.name} {index + 1}/{function.steps.Length} 실패 · {result.Message}");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                executed++;
            }

            if (restoreDryRun && snapshot.DryRunEnabled)
            {
                ToggleDryRun();
            }

            var prefix = string.IsNullOrWhiteSpace(startPointName)
                ? "[Function Run]"
                : "[Function From]";
            PushFeedback($"{prefix} {function.name} DryRun {executed}개 포인트 실행 완료");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }

        private WaypointSequence ExpandTeachingBlockSequence()
        {
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var blocks = teachingBlockSequenceStore.LoadOrCreate().blocks ?? Array.Empty<TeachingSequenceBlock>();
            var pointSequence = teachingPointStoreAdapter.LoadIfExists();
            var expanded = WaypointStore.CreateEmpty("PendantV3BlocksExpanded");
            for (var index = 0; index < blocks.Length; index++)
            {
                var block = blocks[index];
                if (block == null || !block.enabled || string.IsNullOrWhiteSpace(block.refName))
                {
                    continue;
                }

                if (string.Equals(block.kind, TeachingSequenceBlock.BundleRefKind, StringComparison.OrdinalIgnoreCase))
                {
                    ExpandBundleBlock(teachingFunctionStore.Load(block.refName), pointSequence, expanded);
                    continue;
                }

                var point = FindWaypoint(pointSequence, block.refName);
                if (point != null)
                {
                    WaypointStore.AddWaypoint(expanded, CloneWaypoint(point));
                }
            }

            return expanded;
        }

        private static void ExpandBundleBlock(TeachingFunction function, WaypointSequence pointSequence, WaypointSequence expanded)
        {
            var steps = function?.steps ?? Array.Empty<TeachingFunctionStep>();
            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                if (step == null
                    || !step.enabled
                    || !string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var point = FindWaypoint(pointSequence, step.refName);
                if (point != null)
                {
                    WaypointStore.AddWaypoint(expanded, CloneWaypoint(point));
                }
            }
        }

        private static Waypoint CloneWaypoint(Waypoint point)
        {
            return new Waypoint
            {
                name = point?.name ?? string.Empty,
                jointsDeg = point?.jointsDeg != null ? (double[])point.jointsDeg.Clone() : new double[6],
                tcpMm = point?.tcpMm != null ? (double[])point.tcpMm.Clone() : new double[6],
                moveType = point?.moveType ?? "MoveJ",
                speedPreset = point?.speedPreset ?? "medium",
                dwellSec = point?.dwellSec ?? 0.0
            };
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
            SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);
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
            SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);
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
            if (loop)
            {
                return false;
            }

            if (!ShouldUseLiveMoveJOperatorPath())
            {
                return false;
            }

            return TryLoadLiveWaypointSequence(sequenceName, out var sequence, out _)
                && SequenceSupportsTinyMoveJLive(sequence);
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
            SetLiveSessionMode(LiveCommandSessionMode.GripperOnly);
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
            if (loop)
            {
                const string lockedLoopMessage = "[Sequence] 반복 live 실행은 아직 잠겨 있다. 먼저 1회 실행 성공패턴을 사용해라.";
                PushFeedback(lockedLoopMessage);
                RefreshSnapshot();
                return lockedLoopMessage;
            }

            if (!TryLoadLiveWaypointSequence(sequenceName, out var sequence, out var loadMessage))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return loadMessage;
            }

            if (!SequenceSupportsTinyMoveJLive(sequence))
            {
                ClearPendingWaypointSequenceOperatorCommandState();
                const string unsupportedMessage = "[Sequence] live v1은 tiny MoveJ 범위의 MoveJ 포인트만 지원한다.";
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
            pendingWaypointSequenceName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            pendingWaypointSequenceStartPointName = safeStartPointName;
            pendingWaypointSequenceRestoreDryRun = snapshot.DryRunEnabled;
            snapshot.LiveBlockedReason = string.Empty;
            SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);

            if (snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = false;
                InvalidateLiveApprovalContext();
            }

            RefreshSnapshot();
            return $"pendingSequenceApproval=True; sequence={pendingWaypointSequenceName}; count={sequence.waypoints.Length}; dryRun={snapshot.DryRunEnabled}";
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
            ClearPendingWaypointSequenceOperatorCommandState();

            if (liveWaypointSequenceCoroutine != null)
            {
                var alreadyRunning = $"[Sequence Run] {liveWaypointSequenceName} 실행 중이다. Stop 후 다시 실행해라.";
                PushFeedback(alreadyRunning);
                RefreshSnapshot();
                return alreadyRunning;
            }

            if (!TryLoadLiveWaypointSequence(sequenceName, out var sequence, out var loadMessage))
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
                    var unsupported = $"[Sequence Run] {point.name} 실패 · live v1은 MoveJ만 지원한다.";
                    PushFeedback(unsupported);
                    RefreshSnapshot();
                    return unsupported;
                }
            }

            liveWaypointSequenceName = sequenceName;
            liveWaypointSequenceCoroutine = StartCoroutine(
                RunLiveWaypointSequence(sequenceName, sequence, startIndex, restoreDryRun));
            PushFeedback($"[Sequence Run] {sequenceName} live 실행 시작 · {sequence.waypoints.Length - startIndex}개 포인트");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }

        private IEnumerator RunLiveWaypointSequence(
            string sequenceName,
            WaypointSequence sequence,
            int startIndex,
            bool restoreDryRun)
        {
            const float arrivalPollSeconds = 0.1f;
            const float arrivalTimeoutSeconds = 30f;
            const double arrivalThresholdDeg = 1.0d;

            var executed = 0;
            try
            {
                for (var index = startIndex; index < sequence.waypoints.Length; index++)
                {
                    var point = sequence.waypoints[index];
                    if (point == null)
                    {
                        continue;
                    }

                    teachingSequenceRuntime?.Select(index);
                    PreviewJointAngles(point.jointsDeg, $"Sequence {point.name} MoveJ preview");
                    yield return null;

                    SetLiveSessionMode(LiveCommandSessionMode.TinyMoveJOnly);
                    GrantLiveCommandApproval(
                        LiveCommandKind.MoveJ,
                        "SEQ",
                        ttlSeconds: 15,
                        ResolvePreparedMotionTargetKey(LiveCommandKind.MoveJ));

                    var result = ExecuteTeachingWaypoint(point);
                    if (!result.IsSuccess)
                    {
                        PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                        RefreshSnapshot();
                        yield break;
                    }

                    executed++;
                    var elapsed = 0f;
                    var arrived = false;
                    while (elapsed < arrivalTimeoutSeconds)
                    {
                        if (connectionService != null)
                        {
                            var sync = connectionService.SyncCurrentState();
                            if (sync.IsSuccess && sync.Value.JointPosDeg != null)
                            {
                                HandleStateUpdated(sync.Value);
                                if (sync.Value.MotionQueueLength <= 0
                                    && HasArrivedAtWaypoint(sync.Value.JointPosDeg, point.jointsDeg, arrivalThresholdDeg))
                                {
                                    arrived = true;
                                    break;
                                }
                            }
                        }

                        yield return new WaitForSeconds(arrivalPollSeconds);
                        elapsed += arrivalPollSeconds;
                    }

                    if (!arrived)
                    {
                        PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} 실패 · {point.name} 도달 타임아웃");
                        RefreshSnapshot();
                        yield break;
                    }

                    PushFeedback($"[Sequence Run] {index + 1}/{sequence.waypoints.Length} {point.name} 도달");
                    RefreshSnapshot();
                    yield return null;
                }

                PushFeedback($"[Sequence Run] {sequenceName} live 1회 실행 완료 · {executed}개 포인트");
                RefreshSnapshot();
            }
            finally
            {
                if (restoreDryRun && !snapshot.DryRunEnabled)
                {
                    snapshot.DryRunEnabled = true;
                    InvalidateLiveApprovalContext();
                }

                liveWaypointSequenceCoroutine = null;
                liveWaypointSequenceName = string.Empty;
            }
        }

        private static bool HasArrivedAtWaypoint(double[] currentJointDeg, double[] targetJointDeg, double thresholdDeg)
        {
            if (currentJointDeg == null || targetJointDeg == null || currentJointDeg.Length < 6 || targetJointDeg.Length < 6)
            {
                return false;
            }

            for (var index = 0; index < 6; index++)
            {
                if (System.Math.Abs(currentJointDeg[index] - targetJointDeg[index]) > thresholdDeg)
                {
                    return false;
                }
            }

            return true;
        }

        public string GetLiveCommandSafetyGateSummaryForDebug(string commandKind)
        {
            var kind = ParseLiveCommandKind(commandKind);
            var result = EvaluateLiveCommandSafetyPreview(
                kind,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: kind == LiveCommandKind.MoveGripper);
            return result.ToSummary();
        }

        public string GetTinyMoveJGateSummaryForDebug()
        {
            var dedicatedTinyMoveJPath = HasDedicatedTinyMoveJLivePathConfigured();
            var withinTinyRange = TryEvaluateTinyMoveJRange(
                previewUsesJointPose ? previewJointAnglesDeg : null,
                out _,
                out _);
            var gate = EvaluateLiveCommandSafetyPreview(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: false,
                allowReadbackOnlyMotionPathOverride: dedicatedTinyMoveJPath,
                hasDedicatedTinyMoveJMotionPath: dedicatedTinyMoveJPath,
                isWithinTinyMoveRange: withinTinyRange);
            return $"status={gate.Status}; ready={gate.CanExecuteLive}; risk={gate.RiskLevel}; blocks={string.Join(" | ", gate.BlockReasons)}; cleared={string.Join(" | ", gate.ClearedReasons)}; readback={gate.ReadbackSummary}";
        }

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
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            ClearPreparedMotionContext();
            InvalidateLiveApprovalContext();
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

        public string RefreshLiveEvidenceForDebug()
        {
            if (!initialized)
            {
                TryInitialize();
            }

            EnsureRuntimeHelpers();
            liveStateRecorder?.SetConnectionInfo(templateDefinition.RobotId, config.defaultIp);
            var result = connectionService.SyncCurrentState();
            if (!result.IsSuccess)
            {
                RefreshSnapshot();
                return $"[LiveEvidence] {result.Message}";
            }

            currentState = result.Value;
            hasCurrentPositionReadComplete = true;
            templateDefinition.PosePresetProvider?.UpdateCurrent(result.Value.JointPosDeg);
            if (liveStateRecorder == null)
            {
                RefreshSnapshot();
                return "[LiveEvidence] recorder missing";
            }

            liveStateRecorder.RecordState(result.Value);
            ApplyVisualState();
            RefreshSnapshot();
            return $"[LiveEvidence] recorded tool={result.Value.ToolId:00} user={result.Value.UserId:00} coord={snapshot.CoordSystem}";
        }

        public string GetLiveEvidenceGateSummaryForDebug()
        {
            var evidence = ResolveTinyMoveJEvidenceGateState();
            return $"connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; tool={evidence.ToolId}; user={evidence.UserId}; coord={evidence.CoordSystem}; coordResolved={evidence.HasExplicitCoordSystem}; stateFresh={evidence.StateEvidenceFresh}; driftFresh={evidence.DriftEvidenceFresh}; driftPassed={evidence.DriftPassed}; sessionMatch={evidence.MatchesCurrentSession}; placeholder={IsPlaceholderLiveStateForDebug()}; stateReason={evidence.StateEvidenceReason}; driftReason={evidence.DriftEvidenceReason}";
        }

        public bool HasStableLiveEvidenceForDebug()
        {
            var evidence = ResolveTinyMoveJEvidenceGateState();
            return (connectionService?.Client.IsConnected ?? false)
                && evidence.ToolId > 0
                && evidence.UserId > 0
                && evidence.HasExplicitCoordSystem
                && evidence.StateEvidenceFresh
                && evidence.DriftEvidenceFresh
                && evidence.DriftPassed
                && evidence.MatchesCurrentSession
                && !IsPlaceholderLiveStateForDebug();
        }

        public void SetLivePollIntervalForDebug(float seconds)
        {
            connectionService?.SetPollInterval(seconds);
        }

        public void ResetLiveReadbackProbeForDebug()
        {
            liveReadbackProbeUpdateCount = 0;
            liveReadbackProbeFirstUpdateTime = -1d;
            liveReadbackProbeLastUpdateTime = -1d;
        }

        public void ForceNextReadFailuresForDebug(int count)
        {
            connectionService?.ForceNextReadFailuresForDebug(count, "forced debug read fail");
        }

        public string GetLiveReadbackProbeSummaryForDebug()
        {
            var elapsedSeconds = liveReadbackProbeUpdateCount > 1 && liveReadbackProbeFirstUpdateTime >= 0d && liveReadbackProbeLastUpdateTime >= liveReadbackProbeFirstUpdateTime
                ? liveReadbackProbeLastUpdateTime - liveReadbackProbeFirstUpdateTime
                : 0d;
            var observedHz = elapsedSeconds > 0d
                ? (liveReadbackProbeUpdateCount - 1) / elapsedSeconds
                : 0d;
            return $"poll={connectionService?.CurrentPollIntervalSeconds.ToString("0.###") ?? "-"}s; sampleMs={connectionService?.LastRealtimeStateSamplePeriodMs ?? 0}; count={liveReadbackProbeUpdateCount}; windowSec={elapsedSeconds:0.###}; hz={observedHz:0.##}; readErrors={connectionService?.ConsecutiveReadErrors ?? 0}; forcedReadFailures={connectionService?.ForcedReadFailuresRemaining ?? 0}";
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

        public FairinoResult StopMotion()
        {
            if (liveWaypointSequenceCoroutine != null)
            {
                StopCoroutine(liveWaypointSequenceCoroutine);
                liveWaypointSequenceCoroutine = null;
                liveWaypointSequenceName = string.Empty;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                waypointRunner.Stop();
            }

            var result = connectionService.StopMotion();
            PushFeedback(result.IsSuccess ? "[Stop] 모션 정지" : result.Message);
            RefreshSnapshot();
            return result;
        }

        public void TogglePause()
        {
            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                isPaused = !isPaused;
                if (isPaused)
                {
                    waypointRunner.Stop();
                    PushFeedback("[Pause] 시퀀스 일시정지");
                }
                else
                {
                    PushFeedback("[Pause] 시퀀스 재개 준비");
                }
            }
            else
            {
                PushFeedback("Pause는 현재 실행 중인 시퀀스가 있을 때만 의미가 있다.");
            }

            RefreshSnapshot();
        }

        public void StepBackward()
        {
            if (PreviewTeachingStep(delta: -1))
            {
                return;
            }

            PushFeedback("이전 티칭 포인트가 없다.");
            RefreshSnapshot();
        }

        public void StepForward()
        {
            if (PreviewTeachingStep(delta: 1))
            {
                return;
            }

            PushFeedback("다음 티칭 포인트가 없다.");
            RefreshSnapshot();
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

        public void SimulateGripper(bool open)
        {
            SetGripperOpen(open);
        }

        public FairinoResult SetGripperOpen(bool open)
        {
            return SetGripperPositionPercent(open ? 100f : 0f);
        }

        public string ProbeLiveGripperForDebug()
        {
            if (!EnsureReadyForCommand("gripper probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper probe unavailable: mock mode";
            }

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var expectedProfile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var capability = liveService.ProbeGripperCapability();
            var gripperConfig = liveService.ReadGripperConfig();
            var gripperStatus = liveService.ReadGripperStatus();
            var modeLabel = ResolveControllerModeLabel();
            var expectedSummary = $"{expectedProfile}";
            var configSummary = gripperConfig.IsSuccess
                ? $"{gripperConfig.Value}; matchesExpected={gripperConfig.Value.Matches(expectedProfile)}"
                : $"FAIL({gripperConfig.ErrorCode}:{gripperConfig.Message})";
            var statusSummary = gripperStatus.IsSuccess
                ? gripperStatus.Value.ToString()
                : $"FAIL({gripperStatus.ErrorCode}:{gripperStatus.Message})";
            var capabilitySummary = capability.IsSuccess
                ? capability.Value.ToString()
                : $"FAIL({capability.ErrorCode}:{capability.Message})";
            return $"mode={modeLabel}; expectedProfile=({expectedSummary}); capability=({capabilitySummary}); sdkConfig=({configSummary}); status=({statusSummary})";
        }

        public string ProbeLiveGripperIndexCandidatesForDebug(int minIndex = 1, int maxIndex = 4)
        {
            if (!EnsureReadyForCommand("gripper index probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper index probe unavailable: mock mode";
            }

            if (minIndex > maxIndex)
            {
                (minIndex, maxIndex) = (maxIndex, minIndex);
            }

            minIndex = Mathf.Clamp(minIndex, 1, 8);
            maxIndex = Mathf.Clamp(maxIndex, minIndex, 8);

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var baseProfile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var modeLabel = ResolveControllerModeLabel();
            var lines = new System.Text.StringBuilder();
            lines.Append($"mode={modeLabel}; baseProfile=({baseProfile})");
            for (var index = minIndex; index <= maxIndex; index++)
            {
                var candidate = new FairinoGripperProfile(
                    baseProfile.Company,
                    baseProfile.Device,
                    baseProfile.SoftVersion,
                    baseProfile.Bus,
                    index);
                lines.AppendLine();
                lines.Append($"index{index}: ");
                lines.Append(RobotControlPeripheralFacade.ProbeLiveGripperProfileForDebug(liveService, candidate));
            }

            return lines.ToString();
        }

        public string ProbeLiveGripperActivationSequencesForDebug()
        {
            if (!EnsureReadyForCommand("gripper activation sequence probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper activation sequence probe unavailable: mock mode";
            }

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var profile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var modeLabel = ResolveControllerModeLabel();
            return $"mode={modeLabel}; {RobotControlPeripheralFacade.ProbeLiveGripperActivationSequencesForDebug(liveService, profile)}";
        }

        public FairinoResult SetGripperPositionPercent(int positionPercent)
        {
            return SetGripperPositionPercent((float)positionPercent);
        }

        public FairinoResult SetGripperPositionPercent(float positionPercent)
        {
            var clampedPosition = ClampPercent(positionPercent);
            if (!EnsureReadyForCommand($"그리퍼 {clampedPosition}%"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            FairinoConnectionService liveGripperService = null;
            var hasGripperReadback = true;
            var requestedSpeedPercent = ResolveRequestedSpeedPercent();
            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                if (HasDedicatedLiveGripperSmokePathConfigured())
                {
                    var sibling = connectionService.CreateMotionSiblingSession();
                    if (!sibling.IsSuccess)
                    {
                        PushFeedback(sibling.Message);
                        RememberOperatorBlockedReason(sibling.Message);
                        RefreshSnapshot();
                        return new FairinoResult(sibling.ErrorCode, sibling.Message);
                    }

                    liveGripperService = sibling.Value;
                    var capability = liveGripperService.ProbeGripperCapability();
                    hasGripperReadback = capability.IsSuccess
                        && (capability.Value.CanReadPosition || capability.Value.CanReadMotion);
                    requestedSpeedPercent = Mathf.Min(requestedSpeedPercent, LiveCommandSafetyGate.DefaultLiveSpeedCapPercent);
                }

                var gate = EvaluateLiveCommandSafety(
                    LiveCommandKind.MoveGripper,
                    requestedSpeedPercent,
                    productionIkSafe: true,
                    boundaryReady: true,
                    collisionReady: true,
                    hasGripperReadback: hasGripperReadback,
                    gateConnectionService: liveGripperService);
                if (!gate.CanExecuteLive)
                {
                    return BlockLiveCommand(gate, "live-gripper-blocked");
                }
            }

            var objectDetected = TryResolveGripperObjectStopPercent(out var objectStopPercent);
            var result = peripheralFacade.SetGripperPosition(
                clampedPosition,
                snapshot.DryRunEnabled,
                objectDetected,
                objectStopPercent,
                liveGripperService);
            ApplyGripperVisual(peripheralFacade.Snapshot.GripperOpenRatio);
            PushFeedback(result.Message);
            if (result.IsSuccess)
            {
                ClearRememberedOperatorBlockedReason();
            }
            else
            {
                RememberOperatorBlockedReason(result.Message);
            }
            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.MoveGripper, result);
            RefreshSnapshot();
            return result;
        }

        public FairinoResult SetGripperPositionPercentFromOperator(int positionPercent)
        {
            return SetGripperPositionPercentFromOperator((float)positionPercent);
        }

        public FairinoResult SetGripperPositionPercentFromOperator(float positionPercent)
        {
            var restoreDryRun = false;
            if (ShouldUseLiveGripperOperatorPath())
            {
                var preflight = PreflightLiveGripperOperatorPath(allowWarmup: true);
                if (!preflight.IsSuccess)
                {
                    RememberOperatorBlockedReason(preflight.Message);
                    PushFeedback(preflight.Message);
                    RefreshSnapshot();
                    return preflight;
                }

                SetLiveSessionMode(LiveCommandSessionMode.GripperOnly);
                if (snapshot.DryRunEnabled)
                {
                    snapshot.DryRunEnabled = false;
                    restoreDryRun = true;
                }

                snapshot.LiveBlockedReason = string.Empty;
            }

            var result = SetGripperPositionPercent(positionPercent);

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result;
        }

        public FairinoResult SetRobotDigitalOutput(int channel, bool value)
        {
            if (!EnsureReadyForCommand($"DO{channel} {(value ? "ON" : "OFF")}"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                var gate = EvaluateLiveCommandSafety(
                    LiveCommandKind.RobotDo,
                    ResolveRequestedSpeedPercent(),
                    productionIkSafe: true,
                    boundaryReady: true,
                    collisionReady: true,
                    hasGripperReadback: false);
                if (!gate.CanExecuteLive)
                {
                    return BlockLiveCommand(gate, "live-robotdo-blocked");
                }
            }

            var result = peripheralFacade.SetRobotDigitalOutput(channel, value, snapshot.DryRunEnabled);
            PushFeedback(result.Message);
            snapshot.LiveBlockedReason = result.IsSuccess ? string.Empty : result.Message;
            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.RobotDo, result);
            RefreshSnapshot();
            return result;
        }

        public FairinoResult SetToolDigitalOutput(int channel, bool value)
        {
            if (!EnsureReadyForCommand($"ToolDO{channel} {(value ? "ON" : "OFF")}"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                var gate = EvaluateLiveCommandSafety(
                    LiveCommandKind.ToolDo,
                    ResolveRequestedSpeedPercent(),
                    productionIkSafe: true,
                    boundaryReady: true,
                    collisionReady: true,
                    hasGripperReadback: false);
                if (!gate.CanExecuteLive)
                {
                    return BlockLiveCommand(gate, "live-tooldo-blocked");
                }
            }

            var result = peripheralFacade.SetToolDigitalOutput(channel, value, snapshot.DryRunEnabled);
            PushFeedback(result.Message);
            snapshot.LiveBlockedReason = result.IsSuccess ? string.Empty : result.Message;
            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.ToolDo, result);
            RefreshSnapshot();
            return result;
        }

        public void SetBaseFrameVisible(bool visible)
        {
            showBaseFrame = visible;
            ApplyVisualState();
            RefreshSnapshot();
        }

        public void SetToolFrameVisible(bool visible)
        {
            showToolFrame = visible;
            ApplyVisualState();
            RefreshSnapshot();
        }

        public void SetTrailVisible(bool visible)
        {
            showTrail = visible;
            eeTrailRenderer?.SetVisible(visible);
            RefreshSnapshot();
        }

        public void SetGhostVisible(bool visible)
        {
            showGhost = visible;
            ApplyVisualState();
            RefreshSnapshot();
        }

        public void SetWorkspaceBoundaryVisible(bool visible)
        {
            showWorkspaceBoundary = visible;
            RefreshSnapshot();
        }

        public void SetCollisionVisible(bool visible)
        {
            showCollision = visible;
            RefreshSnapshot();
        }

        public void PulseJointHighlight(int jointIndex)
        {
            if (jointHighlightRings == null || jointHighlightRings.Length == 0)
            {
                return;
            }

            activeJointHighlightIndex = Mathf.Clamp(jointIndex, 0, jointHighlightRings.Length - 1);
            activeJointHighlightUntilTime = Time.unscaledTime + JointHighlightHoldSeconds;
            ApplyJointHighlightState();
        }

        public void ClearJointHighlight()
        {
            activeJointHighlightIndex = -1;
            activeJointHighlightUntilTime = 0f;
            ApplyJointHighlightState();
        }

        public void ExecutePrimaryAction()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ReadyToJog && TryExecutePendingPreview())
            {
                return;
            }

            switch (snapshot.StatusKind)
            {
                case RobotControlV3RuntimeStatusKind.Disconnected:
                    ConnectAndSyncDefaultAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedServoOff:
                    if (IsReadbackOnlyLiveClient())
                    {
                        SyncCurrentStateAsync();
                    }
                    else
                    {
                        EnableServo();
                    }
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedUnsynced:
                    SyncCurrentStateAsync();
                    break;
                case RobotControlV3RuntimeStatusKind.Fault:
                    ResetErrors();
                    break;
                default:
                    if (!TryRunTeachingSequenceOnce())
                    {
                        PushFeedback("실행할 저장 포인트가 없다.");
                    }

                    RefreshSnapshot();
                    break;
            }
        }

        public string ExecutePreparedPreviewForDebug()
        {
            var executed = TryExecutePendingPreview();
            RefreshSnapshot();
            return $"executed={executed}; status={snapshot.StatusKind}; dryRun={snapshot.DryRunEnabled}; feedback={snapshot.LastFeedback}";
        }

        public void ExecutePreparedPreviewForProduct()
        {
            TryExecutePendingPreview();
            RefreshSnapshot();
        }

        private bool TryExecutePendingPreview()
        {
            if (!EnsureReadyForCommand("실행"))
            {
                return false;
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                ApplyTinyMoveJ(previewJointAnglesDeg, "실행 버튼 tiny MoveJ");
                return true;
            }

            if (!previewUsesJointPose && previewTcpPose != null)
            {
                ApplyTcpPose(previewTcpPose, "실행 버튼 MoveL");
                return true;
            }

            return false;
        }

        private bool TryRunTeachingSequenceOnce()
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (teachingLoopEnabled)
            {
                var sequence = teachingPointStoreAdapter.LoadIfExists();
                if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
                {
                    return false;
                }

                if (waypointRunner == null)
                {
                    EnsureRuntimeHelpers();
                }

                if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
                {
                    PushFeedback("[Teaching Loop] 이미 반복 실행 중이다. Stop 후 다시 실행해라.");
                    RefreshSnapshot();
                    return true;
                }

                waypointRunner.PlayLoop(sequence, snapshot.DryRunEnabled || connectionService == null || connectionService.IsMockMode);
                PushFeedback($"[Teaching Loop] {teachingSequenceRuntime.Count}개 포인트 반복 실행 시작");
                RefreshSnapshot();
                return true;
            }

            for (var index = 0; index < teachingSequenceRuntime.Count; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching Run] {index + 1}/{teachingSequenceRuntime.Count} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return true;
                }
            }

            PushFeedback($"[Teaching Run] {teachingSequenceRuntime.Count}개 포인트 실행 완료");
            RefreshSnapshot();
            return true;
        }

        private string PlayRecordedTeachingPath(bool loop)
        {
            if (!EnsureReadyForCommand(loop ? "기록 루프 재생" : "기록 재생"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            var sequence = ResolveRecordedPathSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length < 2)
            {
                PushFeedback("[Path Replay] 재생할 기록 경로가 없다. 기록 시작 → 이동 → 기록 중지 순서로 먼저 저장해라.");
                RefreshSnapshot();
                return GetTeachingPathRecordingSummaryForDebug();
            }

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Path Replay] 이미 재생 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return GetTeachingPathRecordingSummaryForDebug();
            }

            if (loop)
            {
                waypointRunner.PlayLoop(sequence, dryRun: true);
                PushFeedback($"[Path Replay] 기록 경로 루프 시작 · {sequence.waypoints.Length}개 샘플");
            }
            else
            {
                waypointRunner.PlayOnce(sequence, dryRun: true);
                PushFeedback($"[Path Replay] 기록 경로 1회 재생 · {sequence.waypoints.Length}개 샘플");
            }

            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }

        private string PlayNamedWaypointSequence(string sequenceName, bool loop)
        {
            var commandName = loop ? "실행 목록 루프" : "실행 목록 재생";
            if (!EnsureReadyForCommand(commandName))
            {
                return snapshot.LastFeedback;
            }

            if (!TryLoadLiveWaypointSequence(sequenceName, out var sequence, out var loadMessage))
            {
                PushFeedback(loadMessage);
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Sequence] 이미 실행 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                if (loop)
                {
                    PushFeedback("[Sequence] 반복 live 실행은 아직 잠겨 있다. 먼저 1회 실행 성공패턴을 사용해라.");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                var executed = 0;
                for (var index = 0; index < sequence.waypoints.Length; index++)
                {
                    var result = ExecuteTeachingWaypoint(sequence.waypoints[index]);
                    if (!result.IsSuccess)
                    {
                        PushFeedback($"[Sequence Run] {safeName} {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                        RefreshSnapshot();
                        return snapshot.LastFeedback;
                    }

                    executed++;
                }

                PushFeedback($"[Sequence Run] {safeName} live 1회 실행 완료 · {executed}개");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var dryRun = true;
            if (loop)
            {
                waypointRunner.PlayLoop(sequence, dryRun);
                PushFeedback($"[Sequence Loop] {safeName} 루프 시작 · {sequence.waypoints.Length}개");
            }
            else
            {
                waypointRunner.PlayOnce(sequence, dryRun);
                PushFeedback($"[Sequence Run] {safeName} 1회 재생 · {sequence.waypoints.Length}개");
            }

            RefreshSnapshot();
            return snapshot.LastFeedback;
        }

        private bool TryLoadLiveWaypointSequence(string sequenceName, out WaypointSequence sequence, out string message)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            sequence = string.Equals(safeName, RecordedPathSequenceName, StringComparison.OrdinalIgnoreCase)
                ? ResolveRecordedPathSequence()
                : WaypointStore.Load(safeName);
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                message = $"[Sequence] {safeName} 실행할 포인트가 없다.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static string ResolveWaypointSequenceApprovalTargetKey(string sequenceName, WaypointSequence sequence)
        {
            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            var count = sequence?.waypoints?.Length ?? 0;
            return $"SEQ:{safeName}:{count}";
        }

        private static bool SequenceSupportsTinyMoveJLive(WaypointSequence sequence)
        {
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                return false;
            }

            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint == null
                    || !string.Equals(waypoint.moveType, "MoveJ", StringComparison.OrdinalIgnoreCase)
                    || waypoint.jointsDeg == null
                    || waypoint.jointsDeg.Length < 6)
                {
                    return false;
                }
            }

            return true;
        }

        private WaypointSequence ResolveRecordedPathSequence()
        {
            if (recordedPathSequence?.waypoints != null && recordedPathSequence.waypoints.Length > 0)
            {
                return recordedPathSequence;
            }

            if (!WaypointSequenceExists(RecordedPathSequenceName))
            {
                recordedPathSequence = null;
                return null;
            }

            recordedPathSequence = WaypointStore.Load(RecordedPathSequenceName);
            return recordedPathSequence;
        }

        private static bool WaypointSequenceExists(string sequenceName)
        {
            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], sequenceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool PreviewTeachingStep(int delta)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (delta >= 0)
            {
                teachingSequenceRuntime.SelectNext();
            }
            else
            {
                teachingSequenceRuntime.SelectPrevious();
            }

            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            PushFeedback(result.IsSuccess
                ? $"[Teaching Step] {teachingSequenceRuntime.State.SelectedIndex + 1}/{teachingSequenceRuntime.Count} 미리보기"
                : result.Message);
            RefreshSnapshot();
            return true;
        }

        private LiveCommandSafetyGateResult EvaluateLiveCommandSafety(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            string approvalTargetKey = "",
            bool hasMatchingPreparedTarget = true,
            bool allowReadbackOnlyMotionPathOverride = false,
            bool hasDedicatedTinyMoveJMotionPath = false,
            bool isWithinTinyMoveRange = true,
            FairinoConnectionService gateConnectionService = null)
        {
            var approvalState = ConsumeLiveCommandApproval(kind, approvalTargetKey);
            return EvaluateLiveCommandSafetyCore(
                kind,
                requestedSpeedPercent,
                productionIkSafe,
                boundaryReady,
                collisionReady,
                hasGripperReadback,
                approvalState,
                hasMatchingPreparedTarget,
                allowReadbackOnlyMotionPathOverride,
                hasDedicatedTinyMoveJMotionPath,
                isWithinTinyMoveRange,
                gateConnectionService);
        }

        private LiveCommandSafetyGateResult EvaluateLiveCommandSafetyPreview(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            bool allowReadbackOnlyMotionPathOverride = false,
            bool hasDedicatedTinyMoveJMotionPath = false,
            bool isWithinTinyMoveRange = true)
        {
            var preparedMotion = kind switch
            {
                LiveCommandKind.MoveJ => ResolvePreparedMotionContext(
                    kind,
                    previewUsesJointPose ? previewJointAnglesDeg : null,
                    null,
                    productionIkSafe),
                LiveCommandKind.MoveL => ResolvePreparedMotionContext(
                    kind,
                    null,
                    !previewUsesJointPose ? previewTcpPose : null,
                    productionIkSafe),
                _ => null,
            };

            return EvaluateLiveCommandSafetyCore(
                kind,
                requestedSpeedPercent,
                preparedMotion?.IsProductionIkSafe ?? productionIkSafe,
                preparedMotion?.IsBoundaryReady ?? boundaryReady,
                preparedMotion?.IsCollisionReady ?? collisionReady,
                hasGripperReadback,
                LiveCommandApprovalState.None,
                preparedMotion?.HasPreviewArtifact ?? true,
                allowReadbackOnlyMotionPathOverride,
                hasDedicatedTinyMoveJMotionPath,
                isWithinTinyMoveRange,
                gateConnectionService: null);
        }

        private LiveCommandSafetyGateResult EvaluateLiveCommandSafetyCore(
            LiveCommandKind kind,
            int requestedSpeedPercent,
            bool productionIkSafe,
            bool boundaryReady,
            bool collisionReady,
            bool hasGripperReadback,
            LiveCommandApprovalState approvalState,
            bool hasMatchingPreparedTarget,
            bool allowReadbackOnlyMotionPathOverride,
            bool hasDedicatedTinyMoveJMotionPath,
            bool isWithinTinyMoveRange,
            FairinoConnectionService gateConnectionService)
        {
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            var effectiveConnectionService = gateConnectionService ?? connectionService;
            var dedicatedTinyMoveJOverride =
                kind == LiveCommandKind.MoveJ
                && hasDedicatedTinyMoveJMotionPath
                && allowReadbackOnlyMotionPathOverride;
            var dedicatedGripperOverride =
                kind == LiveCommandKind.MoveGripper
                && currentLiveSessionMode == LiveCommandSessionMode.GripperOnly
                && HasDedicatedLiveGripperSmokePathConfigured();
            if (kind != LiveCommandKind.ReadbackOnly
                && IsReadbackOnlyLiveClient(effectiveConnectionService)
                && !dedicatedTinyMoveJOverride
                && !dedicatedGripperOverride)
            {
                var readbackOnlyResult = new LiveCommandSafetyGateResult
                {
                    Kind = kind,
                    RequestedSpeedPercent = requestedSpeedPercent,
                    SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                    RiskLevel = kind switch
                    {
                        LiveCommandKind.RobotDo or LiveCommandKind.ToolDo => LiveCommandRiskLevel.Medium,
                        LiveCommandKind.MoveGripper => LiveCommandRiskLevel.High,
                        _ => LiveCommandRiskLevel.Critical,
                    },
                    Status = LiveCommandGateStatus.ReadbackOnly,
                };

                var stateResult = effectiveConnectionService.SyncCurrentState();
                if (stateResult.IsSuccess)
                {
                    var state = stateResult.Value;
                    readbackOnlyResult.ReadbackSummary =
                        $"mode={state.RobotMode}; enabled={state.IsRobotEnabled}; queue={state.MotionQueueLength}; safety={state.SafetyCode}; fault={state.MainErrorCode}/{state.SubErrorCode}; eStop={state.IsEmergencyStop}; collision={state.IsCollisionDetected}; tool={state.ToolId}; user={state.UserId}";
                }

                readbackOnlyResult.BlockReasons.Add("live client is readback-only");
                readbackOnlyResult.ClearedReasons.Add("actual motion/IO/gripper commands remain locked on macOS live readback");
                return readbackOnlyResult;
            }

            var hasPreview = previewJointAnglesDeg != null || previewTcpPose != null;
            var evidence = ResolveTinyMoveJEvidenceGateState();
            var request = new LiveCommandSafetyGateRequest
            {
                Kind = kind,
                ConnectionService = effectiveConnectionService,
                AllowDryRun = snapshot.DryRunEnabled,
                OperatorConfirmed = approvalState == LiveCommandApprovalState.Consumed,
                HasMatchingPreparedTarget = hasMatchingPreparedTarget,
                HasMatchingApprovalContext = approvalState != LiveCommandApprovalState.TargetMismatch,
                SessionMode = currentLiveSessionMode,
                AllowReadbackOnlyMotionPathOverride = allowReadbackOnlyMotionPathOverride,
                AllowReadbackOnlyGripperPathOverride = dedicatedGripperOverride,
                HasDedicatedTinyMoveJMotionPath = hasDedicatedTinyMoveJMotionPath,
                IsWithinTinyMoveRange = isWithinTinyMoveRange,
                RequestedSpeedPercent = requestedSpeedPercent,
                SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                ToolId = evidence.ToolId,
                UserId = evidence.UserId,
                CoordSystem = evidence.CoordSystem,
                HasResolvedCoordSystem = evidence.HasExplicitCoordSystem,
                HasFreshLatestState = evidence.StateEvidenceFresh && evidence.MatchesCurrentSession,
                HasFreshLatestDrift = evidence.DriftEvidenceFresh && evidence.MatchesCurrentSession,
                IsDriftWithinThreshold = evidence.DriftPassed && evidence.MatchesCurrentSession,
                LatestStateTimestampUtc = evidence.LatestState?.timestampUtc ?? string.Empty,
                LatestDriftTimestampUtc = evidence.LatestDrift?.timestampUtc ?? string.Empty,
                HasDryRunPreviewArtifact = hasPreview,
                IsProductionIkSafe = productionIkSafe,
                IsBoundaryDataReady = boundaryReady,
                IsTargetWithinBoundary = boundaryReady,
                IsCollisionDataReady = collisionReady,
                IsPredictedPathCollisionFree = collisionReady,
                HasGripperReadback = hasGripperReadback,
            };

            return liveCommandSafetyGate.Evaluate(request);
        }

        private FairinoResult BlockLiveCommand(LiveCommandSafetyGateResult gate, string auditLabel)
        {
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            var artifactPath = liveCommandSafetyGate.WriteAudit(gate, auditLabel);
            var message = $"[Live Gate] {gate.Status}: {string.Join(" / ", gate.BlockReasons)} · artifact={artifactPath}";
            PushFeedback(message);
            RememberOperatorBlockedReason(message);
            RefreshSnapshot();
            return FairinoResult.Fail(-70, message);
        }

        private LiveCommandApprovalState ConsumeLiveCommandApproval(LiveCommandKind kind, string targetKey)
        {
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

        private void InvalidateLiveApprovalContext()
        {
            ClearPendingLiveApproval();
            ClearGrantedLiveApproval();
        }

        private void ClearPendingWaypointSequenceOperatorCommandState()
        {
            hasPendingWaypointSequenceOperatorCommand = false;
            pendingWaypointSequenceName = string.Empty;
            pendingWaypointSequenceStartPointName = string.Empty;
            pendingWaypointSequenceRestoreDryRun = false;
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

        private static string FormatRobotStateForDebug(FairinoRobotState state)
        {
            return $"joints=[{string.Join(",", FormatValues(state.JointPosDeg, "0.0"))}]; tcp=[{string.Join(",", FormatValues(state.TcpPose, "0.0"))}]; enabled={state.IsRobotEnabled}; mode={state.RobotMode}; fault={state.MainErrorCode}/{state.SubErrorCode}";
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

        private FairinoResult PreflightLiveGripperOperatorPath(bool allowWarmup)
        {
            if (!ShouldUseLiveGripperOperatorPath())
            {
                return FairinoResult.Ok("gripper preflight bypassed");
            }

            var siblingResult = connectionService.CreateMotionSiblingSession();
            if (!siblingResult.IsSuccess || siblingResult.Value == null)
            {
                return FairinoResult.Fail(
                    siblingResult.ErrorCode != 0 ? siblingResult.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · live gripper 세션을 만들지 못했다. {siblingResult.Message}");
            }

            var liveService = siblingResult.Value;
            var profile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var statusResult = liveService.ReadGripperStatus();
            if (!statusResult.IsSuccess)
            {
                return FairinoResult.Fail(
                    statusResult.ErrorCode != 0 ? statusResult.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · 상태 읽기 실패: {statusResult.Message}");
            }

            if (RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(statusResult.Value, profile))
            {
                return FairinoResult.Ok("gripper ready");
            }

            if (allowWarmup && !liveGripperWarmupAttemptedThisConnection)
            {
                liveGripperWarmupAttemptedThisConnection = true;
                var warmup = RobotControlPeripheralFacade.TryWarmUpLiveGripper(liveService, profile);
                var finalStatus = liveService.ReadGripperStatus();
                if (finalStatus.IsSuccess
                    && RobotControlPeripheralFacade.IsGripperActivationReadyForProfile(finalStatus.Value, profile))
                {
                    return FairinoResult.Ok(
                        warmup.IsSuccess
                            ? "[Live Gripper] warm-up 1회 완료"
                            : "[Live Gripper] warm-up 후 activation ready 확인");
                }

                if (finalStatus.IsSuccess)
                {
                    return FairinoResult.Fail(
                        warmup.ErrorCode != 0 ? warmup.ErrorCode : -86,
                        RobotControlPeripheralFacade.BuildOperatorGripperNotReadyMessage(finalStatus.Value, true));
                }

                return FairinoResult.Fail(
                    warmup.ErrorCode != 0 ? warmup.ErrorCode : -86,
                    $"그리퍼 준비 안 됨 · warm-up 후 상태 읽기 실패: {finalStatus.Message}");
            }

            return FairinoResult.Fail(
                -86,
                RobotControlPeripheralFacade.BuildOperatorGripperNotReadyMessage(statusResult.Value, false));
        }

        private PreparedLiveMotionContext ResolvePreparedMotionContext(
            LiveCommandKind kind,
            double[] jointTarget,
            double[] tcpTarget,
            bool productionIkSafeFallback)
        {
            var targetKey = BuildMotionTargetKey(kind, jointTarget, tcpTarget);
            var contextMatches = !string.IsNullOrWhiteSpace(targetKey)
                && preparedLiveMotionContext.Kind == kind
                && string.Equals(preparedLiveMotionContext.TargetKey, targetKey, StringComparison.Ordinal);

            if (contextMatches)
            {
                return preparedLiveMotionContext;
            }

            return new PreparedLiveMotionContext
            {
                Kind = kind,
                TargetKey = targetKey,
                HasPreviewArtifact = false,
                IsProductionIkSafe = productionIkSafeFallback,
                IsBoundaryReady = false,
                IsCollisionReady = false,
                Source = contextMatches ? preparedLiveMotionContext.Source : "mismatch",
            };
        }

        private bool HasDedicatedTinyMoveJLivePathConfigured()
        {
            if (!FairinoRobotClientFactory.IsTinyMoveJLiveEnabled())
            {
                return false;
            }

            var client = FairinoRobotClientFactory.CreateLive(new FairinoErrorTranslator(), preferMotionCapableDirect: true);
            return client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false };
        }

        private bool HasDedicatedLiveGripperSmokePathConfigured()
        {
            if (FairinoRobotClientFactory.IsLiveGripperSmokeEnabled())
            {
                return true;
            }

            return currentLiveSessionMode == LiveCommandSessionMode.GripperOnly
                && connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client.IsConnected
                && IsReadbackOnlyLiveClient();
        }

        private bool TryEvaluateTinyMoveJRange(double[] targetJointAnglesDeg, out double maxJointDeltaDeg, out int maxJointDeltaIndex)
        {
            maxJointDeltaDeg = 0d;
            maxJointDeltaIndex = -1;
            if (targetJointAnglesDeg == null || targetJointAnglesDeg.Length == 0)
            {
                return false;
            }

            var liveJointBaseline = connectionService?.LastState.JointPosDeg;
            if (liveJointBaseline == null || liveJointBaseline.Length == 0)
            {
                liveJointBaseline = currentState.JointPosDeg;
            }

            if (liveJointBaseline == null || liveJointBaseline.Length == 0)
            {
                return false;
            }

            var jointCount = templateDefinition?.JointCount ?? System.Math.Min(liveJointBaseline.Length, targetJointAnglesDeg.Length);
            var length = System.Math.Min(System.Math.Min(liveJointBaseline.Length, targetJointAnglesDeg.Length), jointCount);
            if (length <= 0)
            {
                return false;
            }

            for (var index = 0; index < length; index++)
            {
                var delta = System.Math.Abs(targetJointAnglesDeg[index] - liveJointBaseline[index]);
                if (delta > maxJointDeltaDeg)
                {
                    maxJointDeltaDeg = delta;
                    maxJointDeltaIndex = index;
                }
            }

            return maxJointDeltaDeg <= RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg
                + RobotControlMotionRuntime.TinyMoveJRangeToleranceDeg;
        }

        private string BuildMotionTargetKey(LiveCommandKind kind, double[] jointTarget, double[] tcpTarget)
        {
            var coord = string.IsNullOrWhiteSpace(snapshot.CoordSystem) ? "Base" : snapshot.CoordSystem;
            var session = liveStateRecorder?.SessionId;
            if (string.IsNullOrWhiteSpace(session))
            {
                session = ResolveTinyMoveJEvidenceGateState().LatestState?.sessionId ?? "no-session";
            }

            var speed = ResolveRequestedSpeedPercent();
            if (jointTarget != null)
            {
                return $"{kind}|speed={speed}|coord={coord}|session={session}|joint={FormatTargetValues(jointTarget)}";
            }

            if (tcpTarget != null)
            {
                return $"{kind}|speed={speed}|coord={coord}|session={session}|tcp={FormatTargetValues(tcpTarget)}";
            }

            return string.Empty;
        }

        private static string FormatTargetValues(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }

            var formatted = new string[values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                formatted[index] = values[index].ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Join(",", formatted);
        }

        private bool TryInitialize()
        {
            if (initialized && connectionService != null && config != null && templateDefinition != null)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            initialized = false;
            isInitializing = true;

            try
            {
                templateDefinition = RobotControlFactory.Create(RobotSelectionBridge.GetSelectedRobotId());
                config = FairinoRobotConfig.Load(templateDefinition.ConfigResourceName) ?? templateDefinition.FallbackConfigFactory();
                connectionService = templateDefinition.ConnectionServiceFactory(new FairinoErrorTranslator());
                connectionService.SetMockMode(false);
                connectionService.ApplyLiveDefaults(config.liveDefaults);
                liveStateRecorder = null;
                kinematicsFacade = templateDefinition.KinematicsFactory();
                previewKinematicsFacade = templateDefinition.KinematicsFactory();
                EnsureRobotSelection();
                EnsureRuntimeRoot();
                EnsureControlRobot();
                EnsureJointDriver();
                EnsureVisualizationHelpers();

                EnsureStageCameraRig();
                lastInitializationError = string.Empty;

                EnsureRuntimeHelpers();
                BindConnectionEvents();
                hasCurrentPositionReadComplete = false;
                currentState = new FairinoRobotState(templateDefinition.PosePresetProvider.GetReadyJointAnglesDeg(), ComputeTcpPoseFromJoints(templateDefinition.PosePresetProvider.GetReadyJointAnglesDeg()));
                initialized = true;
                ApplyVisualState();
                RefreshSnapshot();
                return true;
            }
            catch (Exception ex)
            {
                lastInitializationError = $"{ex.GetType().Name}:{ex.Message}";
                Debug.LogError($"[RobotControlV3RuntimeController] Init failed: {ex}");
                initialized = false;
                return false;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void EnsureRuntimeHelpers()
        {
            presetAnimator = EnsureComponent<PresetTransitionAnimator>(gameObject);
            waypointRunner = EnsureComponent<WaypointCycleRunner>(gameObject);
            waypointRunner.Inject(connectionService, config, presetAnimator);
            BindWaypointRunnerEvents();
            peripheralFacade ??= new RobotControlPeripheralFacade(connectionService, config);
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            liveStateRecorder ??= new Fr5LiveStateRecorder(
                connectionService,
                BuildDisplayStateForDrift,
                () => snapshot.CoordSystem,
                ApplyLiveDriftBlockedReason);
            liveStateRecorder.SetConnectionInfo(templateDefinition.RobotId, config.defaultIp);
            manualReadbackTeachingProbe ??= new ManualReadbackTeachingProbe(connectionService);
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
        }

        private FairinoResult PreviewTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-94, "teaching point missing");
            }

            if (string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase))
            {
                PreviewTcpPose(point.tcpMm, $"Teaching {point.name} MoveL preview");
                return FairinoResult.Ok($"preview MoveL {point.name}");
            }

            PreviewJointAngles(point.jointsDeg, $"Teaching {point.name} MoveJ preview");
            return FairinoResult.Ok($"preview MoveJ {point.name}");
        }

        private FairinoResult ExecuteTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-95, "teaching point missing");
            }

            return string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase)
                ? ApplyTcpPose(point.tcpMm, $"Teaching {point.name} MoveL")
                : ApplyTeachingMoveJ(point.jointsDeg, $"Teaching {point.name} MoveJ");
        }

        private static int FindWaypointIndex(WaypointSequence sequence, string pointName)
        {
            if (sequence?.waypoints == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint != null && string.Equals(waypoint.name, pointName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static Waypoint FindWaypoint(WaypointSequence sequence, string pointName)
        {
            var index = FindWaypointIndex(sequence, pointName);
            return index >= 0 ? sequence.waypoints[index] : null;
        }

        private static int FindFunctionStepIndex(TeachingFunction function, string pointName)
        {
            if (function?.steps == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            for (var index = 0; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step != null
                    && step.enabled
                    && string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(step.refName, pointName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private void BindWaypointRunnerEvents()
        {
            if (waypointRunner == null || presetAnimator == null || waypointRunnerEventsBound)
            {
                return;
            }

            waypointRunner.OnWaypointReached += OnWaypointRunnerReached;
            waypointRunner.OnSequenceComplete += OnWaypointRunnerComplete;
            waypointRunner.OnError += OnWaypointRunnerError;
            waypointRunner.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            presetAnimator.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            waypointRunnerEventsBound = true;
        }

        private void UnbindWaypointRunnerEvents()
        {
            if (!waypointRunnerEventsBound)
            {
                return;
            }

            if (waypointRunner != null)
            {
                waypointRunner.OnWaypointReached -= OnWaypointRunnerReached;
                waypointRunner.OnSequenceComplete -= OnWaypointRunnerComplete;
                waypointRunner.OnError -= OnWaypointRunnerError;
                waypointRunner.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            waypointRunnerEventsBound = false;
        }

        private void OnWaypointRunnerReached(int index, string pointName)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            PushFeedback($"[Teaching Loop] {index + 1}/{waypointRunner.TotalCount} {pointName} 도달");
            RefreshSnapshot();
        }

        private void OnWaypointRunnerComplete()
        {
            PushFeedback(teachingLoopEnabled ? "[Teaching Loop] 반복 실행 정지" : "[Teaching Run] 시퀀스 완료");
            RefreshSnapshot();
        }

        private void OnWaypointRunnerError(string message)
        {
            PushFeedback($"[Teaching Loop] {message}");
            RefreshSnapshot();
        }

        private void OnWaypointRunnerFrameUpdated(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                return;
            }

            currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
            templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            requestStageRefocus = true;
            ApplyVisualState();
            RefreshSnapshot();
        }

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
            RefreshSnapshot();
        }

        private void HandleConnectionStateChanged(bool _)
        {
            if (!connectionService.Client.IsConnected)
            {
                hasCurrentPositionReadComplete = false;
                liveGripperWarmupAttemptedThisConnection = false;
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
            PushFeedback("[Connection] 연결 끊김 감지");
            RefreshSnapshot();
        }

        private void HandleModeChanged(bool _)
        {
            hasCurrentPositionReadComplete = false;
            lastControllerTruthSummary = connectionService != null && connectionService.IsMockMode
                ? "controller truth unavailable in mock"
                : lastControllerTruthSummary;
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

        private FairinoRobotState BuildDisplayStateForDrift()
        {
            var lockToLiveBaseline =
                currentLiveSessionMode != LiveCommandSessionMode.LiveControl
                && (previewUsesJointPose || previewTcpPose != null);

            return new FairinoRobotState(
                currentState.JointPosDeg,
                lockToLiveBaseline
                    ? CopyPoseArray(currentState.TcpPose)
                    : ComputeDisplayedTcpPose(),
                isRobotEnabled: connectionService?.Client.IsEnabled ?? false);
        }

        private void ApplyLiveDriftBlockedReason(string message)
        {
            RememberOperatorBlockedReason(message);
        }

        private TinyMoveJEvidenceGateState ResolveTinyMoveJEvidenceGateState()
        {
            var state = new TinyMoveJEvidenceGateState
            {
                ToolId = ResolveLiveToolId(),
                UserId = ResolveLiveUserId(),
                CoordSystem = ResolveLiveCoordSystem(),
            };

            var root = liveStateRecorder?.RootDirectory;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(), "Artifacts", "live", "fr5");
            }

            state.StateFilePath = Path.Combine(root, "latest-state.json");
            state.DriftFilePath = Path.Combine(root, "latest-drift.json");

            state.StateEvidenceFresh = TryReadFreshJson(state.StateFilePath, out Fr5LiveStateRecord latestState, out var stateEvidenceReason);
            state.DriftEvidenceFresh = TryReadFreshJson(state.DriftFilePath, out Fr5LiveDriftRecord latestDrift, out var driftEvidenceReason);
            state.StateEvidenceReason = stateEvidenceReason;
            state.DriftEvidenceReason = driftEvidenceReason;

            state.LatestState = latestState;
            state.LatestDrift = latestDrift;
            if (!string.IsNullOrWhiteSpace(latestState?.coordSystem))
            {
                state.CoordSystem = latestState.coordSystem;
            }

            state.HasToolContext = state.ToolId > 0;
            state.HasUserContext = state.UserId > 0;
            state.HasExplicitCoordSystem = IsExplicitCoordSystem(state.CoordSystem);
            state.DriftPassed = latestDrift != null && string.Equals(latestDrift.severity, "ok", StringComparison.OrdinalIgnoreCase);
            if (!state.DriftPassed && string.IsNullOrWhiteSpace(state.DriftEvidenceReason))
            {
                state.DriftEvidenceReason = latestDrift == null
                    ? "latest-drift evidence missing"
                    : $"latest-drift severity {latestDrift.severity}";
            }

            var recorderSessionId = liveStateRecorder?.SessionId;
            state.MatchesCurrentSession = string.IsNullOrWhiteSpace(recorderSessionId)
                || (latestState != null && string.Equals(latestState.sessionId, recorderSessionId, StringComparison.Ordinal))
                    && (latestDrift != null && string.Equals(latestDrift.sessionId, recorderSessionId, StringComparison.Ordinal));

            if (!state.MatchesCurrentSession)
            {
                if (string.IsNullOrWhiteSpace(state.StateEvidenceReason))
                {
                    state.StateEvidenceReason = "latest-state session mismatch";
                }

                if (string.IsNullOrWhiteSpace(state.DriftEvidenceReason))
                {
                    state.DriftEvidenceReason = "latest-drift session mismatch";
                }
            }

            return state;
        }

        private int ResolveLiveToolId()
        {
            if (connectionService.LastState.ToolId > 0)
            {
                return connectionService.LastState.ToolId;
            }

            return connectionService.LastCoordContext.ToolId;
        }

        private int ResolveLiveUserId()
        {
            if (connectionService.LastState.UserId > 0)
            {
                return connectionService.LastState.UserId;
            }

            return connectionService.LastCoordContext.UserId;
        }

        private string ResolveLiveCoordSystem()
        {
            return string.IsNullOrWhiteSpace(snapshot.CoordSystem) ? string.Empty : snapshot.CoordSystem;
        }

        private static bool IsExplicitCoordSystem(string coordSystem)
        {
            return coordSystem is "Base" or "Tool" or "User";
        }

        private bool IsPlaceholderLiveStateForDebug()
        {
            return (connectionService?.Client.IsConnected ?? false)
                && ResolveLiveToolId() <= 0
                && ResolveLiveUserId() <= 0
                && IsAllZeroArray(currentState.JointPosDeg)
                && IsAllZeroArray(currentState.TcpPose);
        }

        private static bool IsAllZeroArray(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return true;
            }

            for (var index = 0; index < values.Length; index++)
            {
                if (System.Math.Abs(values[index]) > 0.0001d)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadFreshJson<T>(string path, out T value, out string reason) where T : class
        {
            value = null;
            reason = string.Empty;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                reason = $"{Path.GetFileName(path)} missing";
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                value = JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                reason = $"{Path.GetFileName(path)} parse failed: {ex.GetType().Name}";
                return false;
            }

            if (value == null)
            {
                reason = $"{Path.GetFileName(path)} parse failed";
                return false;
            }

            var timestampUtc = value switch
            {
                Fr5LiveStateRecord stateRecord => stateRecord.timestampUtc,
                Fr5LiveDriftRecord driftRecord => driftRecord.timestampUtc,
                _ => string.Empty,
            };

            if (!DateTime.TryParse(timestampUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedUtc))
            {
                reason = $"{Path.GetFileName(path)} timestamp invalid";
                return false;
            }

            var ageSeconds = System.Math.Abs((DateTime.UtcNow - parsedUtc.ToUniversalTime()).TotalSeconds);
            if (ageSeconds > LiveEvidenceFreshnessWindowSeconds)
            {
                reason = $"{Path.GetFileName(path)} stale";
                return false;
            }

            return true;
        }

        private enum LiveCommandApprovalState
        {
            None,
            Consumed,
            TargetMismatch,
        }

        private sealed class PreparedLiveMotionContext
        {
            public LiveCommandKind Kind { get; set; } = LiveCommandKind.ReadbackOnly;
            public string TargetKey { get; set; } = string.Empty;
            public bool HasPreviewArtifact { get; set; }
            public bool IsProductionIkSafe { get; set; }
            public bool IsBoundaryReady { get; set; }
            public bool IsCollisionReady { get; set; }
            public string Source { get; set; } = string.Empty;
        }

        private sealed class TinyMoveJEvidenceGateState
        {
            public int ToolId { get; set; }
            public int UserId { get; set; }
            public string CoordSystem { get; set; } = "Base";
            public bool HasToolContext { get; set; }
            public bool HasUserContext { get; set; }
            public bool HasExplicitCoordSystem { get; set; }
            public bool StateEvidenceFresh { get; set; }
            public bool DriftEvidenceFresh { get; set; }
            public bool DriftPassed { get; set; }
            public bool MatchesCurrentSession { get; set; } = true;
            public string StateEvidenceReason { get; set; } = string.Empty;
            public string DriftEvidenceReason { get; set; } = string.Empty;
            public string StateFilePath { get; set; } = string.Empty;
            public string DriftFilePath { get; set; } = string.Empty;
            public Fr5LiveStateRecord LatestState { get; set; }
            public Fr5LiveDriftRecord LatestDrift { get; set; }
        }

        private void EnsureRobotSelection()
        {
            RobotSelectionBridge.SetSelection(templateDefinition.RobotId, RobotSelectionBridge.RobotControlMode);
        }

        private void EnsureRuntimeRoot()
        {
            runtimeRoot = GameObject.Find(templateDefinition.RuntimeRootName)?.transform;
            if (runtimeRoot == null)
            {
                runtimeRoot = new GameObject(templateDefinition.RuntimeRootName).transform;
                runtimeRoot.position = new Vector3(0f, -1000f, 0f);
            }
        }

        private void EnsureControlRobot()
        {
            var existing = runtimeRoot.Find(templateDefinition.ControlRobotInstanceName);
            if (existing != null)
            {
                controlRobotInstance = existing.gameObject;
                StabilizeControlRobot(controlRobotInstance);
                EnsureEndEffectorAttachment();
                return;
            }

            var prefab = Resources.Load<GameObject>(templateDefinition.ControlPrefabResourcePath)
                ?? Resources.Load<GameObject>(templateDefinition.ShowroomPrefabResourcePath);
            if (prefab == null)
            {
                controlRobotInstance = new GameObject(templateDefinition.ControlRobotInstanceName);
                controlRobotInstance.transform.SetParent(runtimeRoot, false);
                return;
            }

            controlRobotInstance = Instantiate(prefab, runtimeRoot);
            controlRobotInstance.name = templateDefinition.ControlRobotInstanceName;
            controlRobotInstance.transform.localPosition = Vector3.zero;
            controlRobotInstance.transform.localRotation = Quaternion.identity;
            RepairVisualMeshes(controlRobotInstance);
            StabilizeControlRobot(controlRobotInstance);
            EnsureEndEffectorAttachment();
        }

        private void EnsureJointDriver()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            jointDriver = EnsureComponent<FairinoUrdfJointDriver>(controlRobotInstance);
            var baseLink = FindBaseLink(controlRobotInstance.transform);
            if (baseLink != null)
            {
                jointDriver.Inject(baseLink);
            }
        }

        private void EnsureVisualizationHelpers()
        {
            var gizmoHost = runtimeRoot.Find("FrameGizmos") ?? new GameObject("FrameGizmos").transform;
            gizmoHost.SetParent(runtimeRoot, false);
            frameGizmoFactory = EnsureComponent<FrameGizmoFactory>(gizmoHost.gameObject);
            frameGizmoFactory.SetVisible(false);

            var baseFrameHost = runtimeRoot.Find("BaseFrameGizmo") ?? new GameObject("BaseFrameGizmo").transform;
            baseFrameHost.SetParent(runtimeRoot, false);
            baseFrameGizmo = EnsureComponent<FrameGizmo>(baseFrameHost.gameObject);
            baseFrameGizmo.SetLength(0.12f);
            baseFrameGizmo.SetVisible(false);

            var toolFrameHost = runtimeRoot.Find("ToolFrameGizmo") ?? new GameObject("ToolFrameGizmo").transform;
            toolFrameHost.SetParent(runtimeRoot, false);
            toolFrameGizmo = EnsureComponent<FrameGizmo>(toolFrameHost.gameObject);
            toolFrameGizmo.SetLength(0.09f);
            toolFrameGizmo.SetVisible(false);

            var trailHost = runtimeRoot.Find("EETrail") ?? new GameObject("EETrail").transform;
            trailHost.SetParent(runtimeRoot, false);
            eeTrailRenderer = EnsureComponent<EETrailRenderer>(trailHost.gameObject);

            var arrowHost = runtimeRoot.Find("DisplacementArrow") ?? new GameObject("DisplacementArrow").transform;
            arrowHost.SetParent(runtimeRoot, false);
            displacementArrow = EnsureComponent<DisplacementArrow>(arrowHost.gameObject);

            targetMarkerVisual = EnsureComponent<TargetMarkerVisual>(runtimeRoot.gameObject);
            targetMarkerVisual.SetMarkersVisible(false);

            var gridHost = runtimeRoot.Find("StageFloorGrid") ?? new GameObject("StageFloorGrid").transform;
            gridHost.SetParent(runtimeRoot, false);
            stageFloorGrid = EnsureComponent<RobotStageFloorGrid>(gridHost.gameObject);
            stageFloorGrid.SetVisible(true);

            var selectionHost = runtimeRoot.Find("PartSelectionGizmo") ?? new GameObject("PartSelectionGizmo").transform;
            selectionHost.SetParent(runtimeRoot, false);
            partSelectionGizmo = EnsureComponent<RobotPartSelectionGizmo>(selectionHost.gameObject);
            partSelectionGizmo.SetAxisLength(0.08f);
            partSelectionGizmo.Clear();

            var ghostHost = runtimeRoot.Find("GhostRobotVisual") ?? new GameObject("GhostRobotVisual").transform;
            ghostHost.SetParent(runtimeRoot, false);
            ghostRobotVisual = EnsureComponent<GhostRobotVisual>(ghostHost.gameObject);
            ghostRobotVisual.EnsureGhost(controlRobotInstance, templateDefinition.BaseLinkName);

            var pathHost = runtimeRoot.Find("PredictedPath") ?? new GameObject("PredictedPath").transform;
            pathHost.SetParent(runtimeRoot, false);
            predictedPathRenderer = EnsureComponent<PredictedPathRenderer>(pathHost.gameObject);
            predictedPathRenderer.ClearPath();

            selectedLinkHighlighter = EnsureComponent<SelectedLinkHighlighter>(controlRobotInstance);
            EnsureJointHighlightRings();

            EnsureEndEffectorAttachment();
        }

        private void EnsureEndEffectorAttachment()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            if (endEffectorAttachment != null)
            {
                endEffectorAttachment.RemoveLegacyGripMarkers();
                peripheralFacade?.SetGripperVisualAttached(true);
                ResetStageCameraIfAutomatic();
                return;
            }

            var wrist = FindChildRecursive(controlRobotInstance.transform, "wrist3_link");
            if (wrist == null)
            {
                peripheralFacade?.SetGripperVisualAttached(false);
                return;
            }

            var toolMount = wrist.Find("ToolMount");
            if (toolMount == null)
            {
                toolMount = new GameObject("ToolMount").transform;
                toolMount.SetParent(wrist, false);
                toolMount.localPosition = Vector3.zero;
                toolMount.localRotation = Quaternion.identity;
                toolMount.localScale = Vector3.one;
            }

            var existing = toolMount.Find(PgeaAttachmentId);
            if (existing != null)
            {
                endEffectorAttachment = existing.GetComponent<FR5EndEffectorAttachment>()
                    ?? existing.gameObject.AddComponent<FR5EndEffectorAttachment>();
                ConfigureEndEffectorAttachment(existing);
                peripheralFacade?.SetGripperVisualAttached(true);
                ResetStageCameraIfAutomatic();
                return;
            }

            var prefab = Resources.Load<GameObject>(PgeaAttachmentResourcePath);
            if (prefab == null)
            {
                peripheralFacade?.SetGripperVisualAttached(false);
                return;
            }

            var instance = Instantiate(prefab, toolMount);
            instance.name = PgeaAttachmentId;
            ConfigureEndEffectorAttachment(instance.transform);
            peripheralFacade?.SetGripperVisualAttached(true);
            ResetStageCameraIfAutomatic();
        }

        private void ConfigureEndEffectorAttachment(Transform attachmentRoot)
        {
            attachmentRoot.localPosition = PgeaAttachmentLocalPosition;
            attachmentRoot.localRotation = PgeaAttachmentLocalRotation;
            attachmentRoot.localScale = Vector3.one;

            var visualRoot = attachmentRoot.Find("VisualRoot");
            var tcpFrame = attachmentRoot.Find("TcpFrame");
            var model = visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
            var fingerLeft = model != null ? model.Find("finger_left") : null;
            var fingerRight = model != null ? model.Find("finger_right") : null;
            if (tcpFrame != null)
            {
                tcpFrame.localPosition = PgeaTcpLocalPosition;
                tcpFrame.localRotation = Quaternion.identity;
            }

            if (model != null)
            {
                var modelLocal = model.localPosition;
                modelLocal.z = PgeaModelLocalZ;
                model.localPosition = modelLocal;
            }

            endEffectorAttachment = attachmentRoot.GetComponent<FR5EndEffectorAttachment>()
                ?? attachmentRoot.gameObject.AddComponent<FR5EndEffectorAttachment>();
            endEffectorAttachment.Configure(PgeaAttachmentId, visualRoot, tcpFrame);
            endEffectorAttachment.ResetDistortedFingerOffsetsForRuntime();
            endEffectorAttachment.SetFingers(fingerLeft, fingerRight);
            endEffectorAttachment.SetGripperOpen(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
        }

        private void ApplyGripperVisual(float openRatio)
        {
            if (endEffectorAttachment == null && controlRobotInstance != null)
            {
                endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            }

            if (endEffectorAttachment != null)
            {
                endEffectorAttachment.SetGripperOpen(openRatio);
                ResetStageCameraIfAutomatic();
            }

            peripheralFacade?.SetGripperVisualAttached(endEffectorAttachment != null);
        }

        private bool TryResolveGripperObjectStopPercent(out int stopPercent)
        {
            stopPercent = 0;
            if (endEffectorAttachment == null && controlRobotInstance != null)
            {
                endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            }

            if (endEffectorAttachment == null || !endEffectorAttachment.TryGetGripObjectStopRatio(out var stopRatio))
            {
                return false;
            }

            stopPercent = ClampPercent(Mathf.RoundToInt(stopRatio * 100f));
            return stopPercent > 0;
        }

        private static int ClampPercent(int value)
        {
            return value < 0 ? 0 : value > 100 ? 100 : value;
        }

        private static float ClampPercent(float value)
        {
            return value < 0f ? 0f : value > 100f ? 100f : value;
        }

        private void ResetStageCameraIfAutomatic()
        {
            if (!stageCameraUserAdjusted || !stageCameraStateValid)
            {
                ResetStageCamera();
            }
        }

        private void EnsureJointHighlightRings()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            var host = runtimeRoot.Find("JointHighlightRings") ?? new GameObject("JointHighlightRings").transform;
            host.SetParent(runtimeRoot, false);
            var colors = new[]
            {
                new Color(0.95f, 0.77f, 0.15f, 1f),
                new Color(0.29f, 0.56f, 0.85f, 1f),
                new Color(0.71f, 0.54f, 0.93f, 1f),
                new Color(0.36f, 0.86f, 0.72f, 1f),
                new Color(0.99f, 0.63f, 0.18f, 1f),
                new Color(0.90f, 0.35f, 0.42f, 1f)
            };
            var radii = new[] { 0.24f, 0.18f, 0.16f, 0.14f, 0.12f, 0.10f };

            var jointCount = templateDefinition != null ? templateDefinition.JointCount : 6;
            jointHighlightRings ??= new JointHighlightRing[jointCount];
            for (var index = 0; index < jointHighlightRings.Length; index++)
            {
                var ringTransform = host.Find($"JointHighlightRing_{index}") ?? new GameObject($"JointHighlightRing_{index}").transform;
                ringTransform.SetParent(host, false);
                var ring = EnsureComponent<JointHighlightRing>(ringTransform.gameObject);
                var jointTransform = jointDriver != null ? jointDriver.GetJointTransform(index) : null;
                if (jointTransform != null)
                {
                    ring.Bind(jointTransform, radii[Mathf.Min(index, radii.Length - 1)], colors[Mathf.Min(index, colors.Length - 1)]);
                }

                ring.SetVisible(false);
                jointHighlightRings[index] = ring;
            }
        }

        private void ApplyBaseAndToolFrameState()
        {
            frameGizmoFactory?.SetVisible(false);
            ApplyFrameGizmo(baseFrameGizmo, ResolveBaseFrameTransform(), showBaseFrame);
            ApplyFrameGizmo(toolFrameGizmo, ResolveToolFrameTransform(), showToolFrame);
        }

        private void ApplyFrameGizmo(FrameGizmo gizmo, Transform target, bool visible)
        {
            if (gizmo == null)
            {
                return;
            }

            var shouldShow = visible && target != null;
            if (shouldShow)
            {
                gizmo.transform.position = target.position;
                gizmo.transform.rotation = target.rotation;
                gizmo.transform.localScale = Vector3.one;
            }

            gizmo.SetVisible(shouldShow);
        }

        private Transform ResolveBaseFrameTransform()
        {
            return FindBaseLink(controlRobotInstance != null ? controlRobotInstance.transform : runtimeRoot)
                ?? controlRobotInstance?.transform
                ?? runtimeRoot;
        }

        private Transform ResolveToolFrameTransform()
        {
            if (endEffectorAttachment?.TcpFrame != null)
            {
                return endEffectorAttachment.TcpFrame;
            }

            var toolMount = controlRobotInstance != null ? FindChildRecursive(controlRobotInstance.transform, "ToolMount") : null;
            if (toolMount != null)
            {
                return toolMount;
            }

            var jointIndex = templateDefinition != null ? templateDefinition.JointCount - 1 : 5;
            return jointDriver?.GetJointTransform(jointIndex);
        }

        private void ApplyJointHighlightState()
        {
            if (jointHighlightRings == null)
            {
                return;
            }

            for (var index = 0; index < jointHighlightRings.Length; index++)
            {
                jointHighlightRings[index]?.SetVisible(index == activeJointHighlightIndex);
            }
        }

        private Transform ResolveSelectablePartTransform(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            var gripperPart = ResolveGripperSelectablePart(target);
            if (gripperPart != null)
            {
                return gripperPart;
            }

            var current = target;
            while (current != null && current != controlRobotInstance?.transform)
            {
                if (IsSelectableLinkTransform(current))
                {
                    return current;
                }

                current = current.parent;
            }

            return target;
        }

        private Transform ResolveGripperSelectablePart(Transform target)
        {
            if (target == null || endEffectorAttachment == null)
            {
                return null;
            }

            if (endEffectorAttachment.FingerLeft != null && target.IsChildOf(endEffectorAttachment.FingerLeft))
            {
                return endEffectorAttachment.FingerLeft;
            }

            if (endEffectorAttachment.FingerRight != null && target.IsChildOf(endEffectorAttachment.FingerRight))
            {
                return endEffectorAttachment.FingerRight;
            }

            if (endEffectorAttachment.ModelRoot != null && target.IsChildOf(endEffectorAttachment.ModelRoot))
            {
                return endEffectorAttachment.ModelRoot;
            }

            return null;
        }

        private static bool IsSelectableLinkTransform(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            var name = target.name;
            return name.IndexOf("link", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("tcp", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("tool", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("gripper", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureStageCameraRig()
        {
            stageCameraPivot = runtimeRoot.Find("V3StageCameraPivot");
            if (stageCameraPivot == null)
            {
                stageCameraPivot = new GameObject("V3StageCameraPivot").transform;
                stageCameraPivot.SetParent(runtimeRoot, false);
            }

            var mainCameraObject = GameObject.Find("Main Camera");
            if (mainCameraObject == null)
            {
                mainCameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            }

            var mainCamera = mainCameraObject.GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = mainCameraObject.AddComponent<Camera>();
            }

            mainCameraObject.tag = "MainCamera";
            mainCamera.targetTexture = null;
            mainCamera.enabled = true;

            var stageCameraTransform = stageCameraPivot.Find("V3StageCamera");
            var existingStageCamera = stageCameraTransform != null ? stageCameraTransform.GetComponent<Camera>() : null;
            if (stageCameraTransform != null && existingStageCamera == null)
            {
                DestroyCameraRigChild(stageCameraTransform.gameObject);
                stageCameraTransform = null;
            }

            if (stageCameraTransform == null)
            {
                stageCameraTransform = new GameObject("V3StageCamera", typeof(Camera)).transform;
                stageCameraTransform.SetParent(stageCameraPivot, false);
            }

            stageCamera = stageCameraTransform.GetComponent<Camera>();
            if (stageCamera == null)
            {
                stageCamera = stageCameraTransform.gameObject.AddComponent<Camera>();
            }

            stageCamera.transform.SetParent(stageCameraPivot, false);
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = new Color(0.12f, 0.12f, 0.16f, 1f);
            stageCamera.nearClipPlane = 0.01f;
            stageCamera.farClipPlane = 50f;
            stageCamera.allowHDR = false;
            stageCamera.allowMSAA = true;
            stageCamera.targetDisplay = 0;
            stageCamera.enabled = true;
            ResetStageCamera();

            var lightTransform = runtimeRoot.Find("V3StageLight");
            var existingLight = lightTransform != null ? lightTransform.GetComponent<Light>() : null;
            if (lightTransform != null && existingLight == null)
            {
                DestroyCameraRigChild(lightTransform.gameObject);
                lightTransform = null;
            }

            if (lightTransform == null)
            {
                lightTransform = new GameObject("V3StageLight", typeof(Light)).transform;
                lightTransform.SetParent(runtimeRoot, false);
            }

            stageLight = lightTransform.GetComponent<Light>();
            if (stageLight == null)
            {
                stageLight = lightTransform.gameObject.AddComponent<Light>();
            }

            stageLight.type = LightType.Directional;
            stageLight.intensity = 1.25f;
            lightTransform.rotation = Quaternion.Euler(36f, -35f, 0f);
        }

        private static void DestroyCameraRigChild(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target == null)
            {
                return null;
            }

            var component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }
    }
}
