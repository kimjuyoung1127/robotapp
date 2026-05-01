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
        private LiveCommandKind approvedLiveSessionKind = LiveCommandKind.ReadbackOnly;
        private DateTime approvedLiveSessionUntilUtc = DateTime.MinValue;
        private string approvedLiveSessionToken = string.Empty;
        private string approvedLiveSessionKey = string.Empty;
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
        private bool pendingWaypointSequenceLoop;
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
        private const string HomePoint1LoopSequenceName = "HomePoint1Loop";
        private const string DefaultHomePointName = "Home";
        private const string DefaultPoint1Name = "LIVE_P1";
        private const float JointHighlightHoldSeconds = 0.45f;
        private const double LiveSnapshotRefreshIntervalSeconds = 0.1d;
        private DateTime approvedLiveLoopUntilUtc = DateTime.MinValue;
        private string approvedLiveLoopContextKey = string.Empty;
        private bool liveWaypointSequenceLooping;
        private int liveWaypointSequenceCycleCount;
        private string liveWaypointCurrentTargetName = string.Empty;
        private string liveWaypointCurrentGripperIntent = string.Empty;
        private string liveWaypointBlockedReason = string.Empty;
        private bool liveLoopApprovalExecutionContext;
        private bool pendingLiveSnapshotRefresh;
        private double nextAllowedLiveSnapshotRefreshTime;

        private enum LiveCommandApprovalState
        {
            None,
            Consumed,
            SessionActive,
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

            FlushPendingLiveSnapshotRefresh();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={initialized}; connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; dryRun={snapshot.DryRunEnabled}; session={currentLiveSessionMode}; pending={snapshot.PendingCommandSummary}; selected={lastSelectedPartName}; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; grid={(stageFloorGrid != null)}; gizmo={(partSelectionGizmo != null)}; initError={lastInitializationError}";
        }

        public FairinoResult StopMotion()
        {
            if (liveWaypointSequenceCoroutine != null)
            {
                StopCoroutine(liveWaypointSequenceCoroutine);
                liveWaypointSequenceCoroutine = null;
                liveWaypointSequenceName = string.Empty;
            }

            liveWaypointSequenceLooping = false;
            liveWaypointSequenceCycleCount = 0;
            liveWaypointCurrentTargetName = string.Empty;
            liveWaypointCurrentGripperIntent = string.Empty;
            liveWaypointBlockedReason = string.Empty;
            InvalidateLiveApprovalContext();
            currentLiveSessionMode = LiveCommandSessionMode.LiveControl;

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

    }
}
