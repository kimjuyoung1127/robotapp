// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
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
    public sealed class RobotControlV3RuntimeController : MonoBehaviour
    {
        private const float StageCameraFov = 32f;
        private const float StageCameraRotationSpeed = 0.25f;
        private const float StageCameraPanSpeed = 0.0018f;
        private const float StageCameraZoomSpeed = 0.08f;
        private const float StageCameraMinPitch = -80f;
        private const float StageCameraMaxPitch = 80f;
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
        private bool initialized;
        private string lastInitializationError = string.Empty;
        private string lastSelectedPartName = "없음";
        private int activeJointHighlightIndex = -1;
        private float activeJointHighlightUntilTime;
        private bool requestStageRefocus;
        private LiveCommandKind approvedLiveCommandKind = LiveCommandKind.ReadbackOnly;
        private DateTime approvedLiveCommandUntilUtc = DateTime.MinValue;
        private string approvedLiveCommandToken = string.Empty;
        private LiveCommandKind pendingLiveApprovalKind = LiveCommandKind.ReadbackOnly;
        private DateTime pendingLiveApprovalUntilUtc = DateTime.MinValue;
        private string pendingLiveApprovalToken = string.Empty;
        private bool pendingLiveApprovalRequired;
        private const string RecordedPathSequenceName = "PendantV3RecordedPath";
        private const float JointHighlightHoldSeconds = 0.45f;

        internal event Action<RobotControlV3RuntimeSnapshot> SnapshotChanged;

        internal RobotControlV3RuntimeSnapshot CurrentSnapshot => snapshot.Clone();
        internal PendantV3PreviewState.Kind CurrentStateKind => ToPreviewKind(snapshot.StatusKind);
        internal Camera StageCamera => stageCamera;
        internal bool IsInitialized => initialized;
        internal FairinoConnectionService ConnectionServiceForDebug => connectionService;
        public bool IsTeachingSequenceRunning => waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle
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
        }

        private void Update()
        {
            connectionService?.Tick(Time.deltaTime);
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
            return $"initialized={initialized}; connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; dryRun={snapshot.DryRunEnabled}; pending={snapshot.PendingCommandSummary}; selected={lastSelectedPartName}; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; grid={(stageFloorGrid != null)}; gizmo={(partSelectionGizmo != null)}; initError={lastInitializationError}";
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
            GrantLiveCommandApproval(kind, "DEBUG", ttlSeconds);
            return $"approved={approvedLiveCommandKind}; token={approvedLiveCommandToken}; expires={approvedLiveCommandUntilUtc:O}";
        }

        public string BeginLiveCommandApprovalForProduct(string commandKind, int ttlSeconds = 30)
        {
            var kind = ParseLiveCommandKind(commandKind);
            ClearPendingLiveApproval();
            if (kind == LiveCommandKind.ReadbackOnly)
            {
                return "approvalRequired=False; kind=ReadbackOnly; token=none; reason=no live command pending";
            }

            pendingLiveApprovalKind = kind;
            pendingLiveApprovalUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(ttlSeconds, 5, 90));
            if (snapshot.DryRunEnabled)
            {
                pendingLiveApprovalRequired = false;
                return $"approvalRequired=False; kind={kind}; token=DRYRUN; expires={pendingLiveApprovalUntilUtc:O}; reason=dry-run";
            }

            pendingLiveApprovalRequired = true;
            pendingLiveApprovalToken = CreateShortToken();
            PushFeedback($"[Live Confirm] {kind} 승인 토큰 {pendingLiveApprovalToken} 발급");
            RefreshSnapshot();
            return $"approvalRequired=True; kind={kind}; token={pendingLiveApprovalToken}; expires={pendingLiveApprovalUntilUtc:O}";
        }

        public bool TryConfirmLiveCommandApprovalForProduct(string token, out string summary)
        {
            if (!pendingLiveApprovalRequired)
            {
                summary = $"approved=False; approvalRequired=False; kind={pendingLiveApprovalKind}; token=DRYRUN";
                ClearPendingLiveApproval();
                return true;
            }

            if (string.IsNullOrWhiteSpace(pendingLiveApprovalToken)
                || pendingLiveApprovalUntilUtc <= DateTime.UtcNow
                || !string.Equals(pendingLiveApprovalToken, token, StringComparison.Ordinal))
            {
                summary = $"approved=False; reason=invalid-or-expired-token; expected={pendingLiveApprovalToken}; actual={token}";
                ClearPendingLiveApproval();
                PushFeedback("[Live Confirm] 승인 토큰이 만료되었거나 일치하지 않는다.");
                RefreshSnapshot();
                return false;
            }

            GrantLiveCommandApproval(pendingLiveApprovalKind, pendingLiveApprovalToken, Mathf.Max(1, (int)(pendingLiveApprovalUntilUtc - DateTime.UtcNow).TotalSeconds));
            summary = $"approved=True; kind={approvedLiveCommandKind}; token={approvedLiveCommandToken}; expires={approvedLiveCommandUntilUtc:O}";
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
            PushFeedback("[Live Confirm] 승인 요청 취소");
            RefreshSnapshot();
            return $"cancelled=True; before=[{summary}]";
        }

        public string GetLiveCommandApprovalSummaryForDebug()
        {
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && (pendingLiveApprovalRequired || pendingLiveApprovalKind != LiveCommandKind.ReadbackOnly);
            var approvedActive = approvedLiveCommandUntilUtc > now && approvedLiveCommandKind != LiveCommandKind.ReadbackOnly;
            return $"pending={pendingActive}; pendingRequired={pendingLiveApprovalRequired}; pendingKind={pendingLiveApprovalKind}; pendingToken={pendingLiveApprovalToken}; pendingExpires={pendingLiveApprovalUntilUtc:O}; approved={approvedActive}; approvedKind={approvedLiveCommandKind}; approvedToken={approvedLiveCommandToken}; approvedExpires={approvedLiveCommandUntilUtc:O}";
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

        public string GetLiveCommandSafetyGateSummaryForDebug(string commandKind)
        {
            var kind = ParseLiveCommandKind(commandKind);
            var result = EvaluateLiveCommandSafety(
                kind,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: kind is not LiveCommandKind.MoveJ,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: kind == LiveCommandKind.MoveGripper);
            return result.ToSummary();
        }

        public string CaptureStageCameraForDebug(string outputPath, int width = 1280, int height = 720)
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            ResetStageCamera();
            if (stageCamera == null)
            {
                return "stageCamera=missing";
            }

            width = Mathf.Clamp(width, 320, 4096);
            height = Mathf.Clamp(height, 240, 4096);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(Application.dataPath, "..", "Artifacts", "robotcontrolv3-stage-camera.png");
            }

            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var previousTarget = stageCamera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "RobotControlV3StageDebugCapture"
            };
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                stageCamera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                stageCamera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            }
            finally
            {
                stageCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                    Destroy(texture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                    DestroyImmediate(texture);
                }
            }

            return $"captured={fullPath}; {GetGripperVisualSummaryForDebug()}";
        }

        public void SetStageTargetTexture(RenderTexture texture)
        {
            if (stageCamera == null)
            {
                return;
            }

            stageCamera.targetTexture = texture;
        }

        public void RefreshStageCameraView()
        {
            if (stageCamera == null)
            {
                return;
            }

            stageCamera.fieldOfView = StageCameraFov;
            if (!stageCameraStateValid)
            {
                ResetStageCamera();
                return;
            }

            ApplyStageCameraState();
        }

        public void ResetStageCamera()
        {
            if (stageCameraPivot == null || stageCamera == null)
            {
                return;
            }

            stageCamera.transform.SetParent(stageCameraPivot, false);
            stageCamera.fieldOfView = StageCameraFov;
            if (TryGetControlBounds(out var bounds))
            {
                var focusPoint = bounds.center + new Vector3(0f, bounds.extents.y * 0.09f, 0f);
                var halfVerticalFovRad = stageCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                var halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * stageCamera.aspect);
                var distanceForWidth = bounds.extents.x / Mathf.Max(0.1f, Mathf.Tan(halfHorizontalFovRad));
                var distanceForHeight = bounds.extents.y / Mathf.Max(0.1f, Mathf.Tan(halfVerticalFovRad));
                var radius = bounds.extents.magnitude;
                var minHalfFovRad = Mathf.Min(halfVerticalFovRad, halfHorizontalFovRad);
                var distanceForRadius = radius / Mathf.Max(0.12f, Mathf.Sin(minHalfFovRad));
                var distance = Mathf.Max(distanceForWidth, distanceForHeight, distanceForRadius * 0.72f, bounds.extents.z * 1.28f) * 0.94f;
                var offsetDirection = new Vector3(0.14f, 0.12f, -1f).normalized;
                stageCamera.transform.position = focusPoint + offsetDirection * distance;
                stageCamera.transform.LookAt(focusPoint);
                SetStageCameraStateFromCurrentPose(focusPoint, bounds, distance);
                return;
            }

            var target = FindBaseLink(controlRobotInstance != null ? controlRobotInstance.transform : runtimeRoot) ?? runtimeRoot;
            var targetPosition = target != null ? target.position + new Vector3(0f, 0.8f, 0f) : Vector3.zero;
            stageCamera.transform.position = targetPosition + new Vector3(0f, 1.6f, -4.8f);
            stageCamera.transform.LookAt(targetPosition);
            SetStageCameraStateFromCurrentPose(targetPosition, null, 5.1f);
        }

        public void SetStageCameraPreset(string presetName)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(presetName))
            {
                return;
            }

            switch (presetName.Trim().ToUpperInvariant())
            {
                case "FRONT":
                    ApplyStageCameraPreset(0f, 8f, 1.02f);
                    break;
                case "RIGHT":
                    ApplyStageCameraPreset(90f, 8f, 1.02f);
                    break;
                case "TOP":
                    ApplyStageCameraPreset(0f, 88f, 1.12f);
                    break;
                case "ISO":
                    ResetStageCamera();
                    break;
            }
        }

        public void OrbitStageCamera(Vector2 deltaPixels)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            stageCameraYaw += deltaPixels.x * StageCameraRotationSpeed;
            stageCameraPitch = Mathf.Clamp(
                stageCameraPitch - (deltaPixels.y * StageCameraRotationSpeed),
                StageCameraMinPitch,
                StageCameraMaxPitch);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        public void PanStageCamera(Vector2 deltaPixels)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            var scale = StageCameraPanSpeed * Mathf.Max(stageCameraDistance, 0.01f);
            var cameraTransform = stageCamera.transform;
            stageCameraPanOffset -= cameraTransform.right * (deltaPixels.x * scale);
            stageCameraPanOffset -= cameraTransform.up * (deltaPixels.y * scale);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        public void ZoomStageCamera(float wheelDelta)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            var clampedDelta = Mathf.Clamp(wheelDelta, -12f, 12f);
            stageCameraDistance = Mathf.Clamp(
                stageCameraDistance + (clampedDelta * StageCameraZoomSpeed),
                stageCameraMinDistance,
                stageCameraMaxDistance);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        private bool EnsureStageCameraState()
        {
            if (stageCamera == null)
            {
                return false;
            }

            if (stageCameraStateValid)
            {
                return true;
            }

            ResetStageCamera();
            return stageCameraStateValid;
        }

        private void ApplyStageCameraState()
        {
            if (stageCamera == null || !stageCameraStateValid)
            {
                return;
            }

            var rotation = Quaternion.Euler(stageCameraPitch, stageCameraYaw, 0f);
            var focusPoint = stageCameraFocusPoint + stageCameraPanOffset;
            stageCamera.transform.position = focusPoint + (rotation * new Vector3(0f, 0f, -stageCameraDistance));
            stageCamera.transform.LookAt(focusPoint);
        }

        private void ApplyStageCameraPreset(float yawDeg, float pitchDeg, float distanceMultiplier)
        {
            stageCameraYaw = yawDeg;
            stageCameraPitch = Mathf.Clamp(pitchDeg, StageCameraMinPitch, StageCameraMaxPitch);
            stageCameraDistance = Mathf.Clamp(
                stageCameraDistance * Mathf.Max(0.85f, distanceMultiplier),
                stageCameraMinDistance,
                stageCameraMaxDistance);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        private void SetStageCameraStateFromCurrentPose(Vector3 focusPoint, Bounds? bounds, float fallbackDistance)
        {
            if (stageCamera == null)
            {
                stageCameraStateValid = false;
                return;
            }

            var offset = stageCamera.transform.position - focusPoint;
            var distance = offset.magnitude > 0.01f ? offset.magnitude : Mathf.Max(0.35f, fallbackDistance);
            var lookDirection = offset.sqrMagnitude > 0.0001f
                ? (-offset).normalized
                : stageCamera.transform.forward.normalized;
            stageCameraFocusPoint = focusPoint;
            stageCameraPanOffset = Vector3.zero;
            stageCameraDistance = Mathf.Max(0.01f, distance);
            if (bounds.HasValue)
            {
                var extentsMagnitude = bounds.Value.extents.magnitude;
                stageCameraMinDistance = Mathf.Max(0.25f, extentsMagnitude * 0.22f);
                stageCameraMaxDistance = Mathf.Max(stageCameraMinDistance + 1f, stageCameraDistance * 3.2f, extentsMagnitude * 4.5f);
            }
            else
            {
                stageCameraMinDistance = 0.35f;
                stageCameraMaxDistance = Mathf.Max(6f, stageCameraDistance * 2.5f);
            }

            stageCameraDistance = Mathf.Clamp(stageCameraDistance, stageCameraMinDistance, stageCameraMaxDistance);
            stageCameraYaw = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
            stageCameraPitch = Mathf.Clamp(
                -Mathf.Asin(Mathf.Clamp(lookDirection.y, -1f, 1f)) * Mathf.Rad2Deg,
                StageCameraMinPitch,
                StageCameraMaxPitch);
            stageCameraStateValid = true;
            stageCameraUserAdjusted = false;
        }

        public string SelectRobotPartAtViewport(Vector2 normalizedViewport)
        {
            if (stageCamera == null || controlRobotInstance == null)
            {
                return "camera-or-robot-missing";
            }

            var clampedViewport = new Vector3(
                Mathf.Clamp01(normalizedViewport.x),
                Mathf.Clamp01(normalizedViewport.y),
                0f);
            var ray = stageCamera.ViewportPointToRay(clampedViewport);
            if (Physics.Raycast(ray, out var hit, 20f) && hit.transform != null && hit.transform.IsChildOf(controlRobotInstance.transform))
            {
                var selected = ResolveSelectablePartTransform(hit.collider != null ? hit.collider.transform : hit.transform);
                partSelectionGizmo?.Select(selected);
                selectedLinkHighlighter?.Select(selected);
                lastSelectedPartName = selected.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            var fallbackTarget = FindRendererHit(ray);
            if (fallbackTarget != null)
            {
                var selected = ResolveSelectablePartTransform(fallbackTarget);
                partSelectionGizmo?.Select(selected);
                selectedLinkHighlighter?.Select(selected);
                lastSelectedPartName = selected.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            partSelectionGizmo?.Clear();
            selectedLinkHighlighter?.Clear();
            lastSelectedPartName = "없음";
            PushFeedback("[Select] 선택 해제");
            RefreshSnapshot();
            return lastSelectedPartName;
        }

        private Transform FindRendererHit(Ray ray)
        {
            if (controlRobotInstance == null)
            {
                return null;
            }

            var renderers = controlRobotInstance.GetComponentsInChildren<Renderer>(true);
            Transform bestTarget = null;
            var bestDistance = float.MaxValue;
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer == null || !renderer.bounds.IntersectRay(ray, out var distance) || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestTarget = renderer.transform;
            }

            return bestTarget;
        }

        private bool TryGetControlBounds(out Bounds bounds)
        {
            bounds = default;
            if (controlRobotInstance == null)
            {
                return false;
            }

            var renderers = controlRobotInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            var found = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderers[i].bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(renderers[i].bounds);
            }

            return found && bounds.size.sqrMagnitude > 0.0001f;
        }

        public FairinoResult ConnectDefault()
        {
            var result = connectionService.Connect(config.defaultIp, config.defaultPort);
            if (result.IsSuccess)
            {
                PushFeedback($"[Connect] {result.Message}");
            }

            RefreshSnapshot();
            return result;
        }

        public FairinoResult Disconnect()
        {
            var result = connectionService.Disconnect();
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
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

        public FairinoResult SyncCurrentState()
        {
            var result = connectionService.SyncCurrentState();
            if (result.IsSuccess)
            {
                currentState = result.Value;
                templateDefinition.PosePresetProvider?.UpdateCurrent(result.Value.JointPosDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
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

        public FairinoResult StopMotion()
        {
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

        public void ToggleDryRun()
        {
            snapshot.DryRunEnabled = !snapshot.DryRunEnabled;
            PushFeedback(snapshot.DryRunEnabled ? "[DryRun] ON" : "[DryRun] OFF");
            RefreshSnapshot();
        }

        public void SetCoordSystem(string coordSystem)
        {
            snapshot.CoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            RefreshSnapshot();
        }

        public void UndoPreview()
        {
            if (undoJointHistory.Count == 0)
            {
                PushFeedback("Undo 할 이력이 없다.");
                RefreshSnapshot();
                return;
            }

            redoJointHistory.Push(CopyJointArray(previewJointAnglesDeg ?? currentState.JointPosDeg));
            previewJointAnglesDeg = undoJointHistory.Pop();
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            ApplyVisualState();
            PushFeedback("[Undo] 이전 관절 프리뷰 복원");
            RefreshSnapshot();
        }

        public void RedoPreview()
        {
            if (redoJointHistory.Count == 0)
            {
                PushFeedback("Redo 할 이력이 없다.");
                RefreshSnapshot();
                return;
            }

            undoJointHistory.Push(CopyJointArray(previewJointAnglesDeg ?? currentState.JointPosDeg));
            previewJointAnglesDeg = redoJointHistory.Pop();
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            ApplyVisualState();
            PushFeedback("[Redo] 다음 관절 프리뷰 복원");
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

        public void PreviewPreset(string presetName)
        {
            if (!EnsureReadyForCommand("프리셋 미리보기"))
            {
                return;
            }

            var preset = ResolvePreset(presetName);
            if (!preset.HasValue)
            {
                PushFeedback($"{presetName} 프리셋을 찾지 못했다.");
                RefreshSnapshot();
                return;
            }

            var presetValue = preset.Value;
            previewJointAnglesDeg = presetValue.JointAnglesDeg;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            RecordUndo(previewJointAnglesDeg);
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {presetValue.Name} 프리셋");
            RefreshSnapshot();
        }

        public FairinoResult ApplyPreset(string presetName)
        {
            if (!EnsureReadyForCommand("프리셋 적용"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var preset = ResolvePreset(presetName);
            if (!preset.HasValue)
            {
                var fail = FairinoResult.Fail(-31, $"{presetName} 프리셋을 찾지 못했다.");
                PushFeedback(fail.Message);
                RefreshSnapshot();
                return fail;
            }

            var presetValue = preset.Value;
            return ApplyJointAngles(presetValue.JointAnglesDeg, $"{presetValue.Name} 프리셋");
        }

        public void PreviewJointAngles(double[] jointAnglesDeg, string reason = "관절 프리뷰")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return;
            }

            previewJointAnglesDeg = CopyJointArray(jointAnglesDeg);
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
        }

        public void RestoreJointPreview()
        {
            if (!EnsureReadyForCommand("관절 복원"))
            {
                return;
            }

            previewJointAnglesDeg = CopyJointArray(currentState.JointPosDeg);
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback("[Restore] 현재 관절값으로 복원");
            RefreshSnapshot();
        }

        public FairinoResult ApplyJointAngles(double[] jointAnglesDeg, string reason = "관절 적용", bool liveProductionIkEligible = true)
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                var invalid = FairinoResult.Fail(-32, "관절 적용 값이 부족하다.");
                PushFeedback(invalid.Message);
                RefreshSnapshot();
                return invalid;
            }

            RecordUndo(currentState.JointPosDeg);
            if (snapshot.DryRunEnabled)
            {
                currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
                templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[DryRun Apply] {reason}");
                RefreshSnapshot();
                return FairinoResult.Ok("DryRun 적용");
            }

            var gate = EvaluateLiveCommandSafety(
                LiveCommandKind.MoveJ,
                ResolveRequestedSpeedPercent(),
                liveProductionIkEligible,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: false);
            if (!gate.CanExecuteLive)
            {
                return BlockLiveCommand(gate, "live-movej-blocked");
            }

            var runtime = RobotControlMotionRuntime.CreateFromSelection();
            if (!runtime.IsSuccess)
            {
                PushFeedback(runtime.Message);
                RefreshSnapshot();
                return new FairinoResult(runtime.ErrorCode, runtime.Message);
            }

            var result = runtime.Value.DispatchMoveJ(jointAnglesDeg, ResolveRequestedSpeedPercent());
            if (result.IsSuccess)
            {
                currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
                templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[Dispatch] MoveJ 완료 · {reason}");
            }
            else
            {
                PushFeedback(result.Message);
            }

            RefreshSnapshot();
            return result;
        }

        public void PreviewTcpPose(double[] tcpPose, string reason = "TCP 프리뷰")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return;
            }

            previewTcpPose = CopyPoseArray(tcpPose);
            previewTcpVisualJointAnglesDeg = TrySolvePointMoveJoints(tcpPose, out var jointTarget).IsSuccess
                ? jointTarget
                : null;
            previewUsesJointPose = false;
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
        }

        public FairinoResult ApplyTcpPose(double[] tcpPose, string reason = "TCP 적용")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (tcpPose == null || tcpPose.Length < 6)
            {
                var invalid = FairinoResult.Fail(-33, "TCP 적용 값이 부족하다.");
                PushFeedback(invalid.Message);
                RefreshSnapshot();
                return invalid;
            }

            if (snapshot.DryRunEnabled)
            {
                RecordUndo(currentState.JointPosDeg);
                var solveResult = TrySolvePointMoveJoints(tcpPose, out var visualJointTarget);
                if (solveResult.IsSuccess)
                {
                    currentState = new FairinoRobotState(visualJointTarget, CopyPoseArray(tcpPose), isRobotEnabled: connectionService.Client.IsEnabled);
                    templateDefinition.PosePresetProvider?.UpdateCurrent(visualJointTarget);
                    previewJointAnglesDeg = null;
                    previewTcpPose = null;
                    previewTcpVisualJointAnglesDeg = null;
                    previewUsesJointPose = false;
                    requestStageRefocus = true;
                    ApplyVisualState();
                    PushFeedback($"[DryRun Apply] {reason} · visual IK");
                    RefreshSnapshot();
                    return FairinoResult.Ok("DryRun TCP 적용");
                }

                previewTcpPose = CopyPoseArray(tcpPose);
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[DryRun Apply] {reason} · 시각 IK 실패, 목표 마커만 표시");
                RefreshSnapshot();
                return FairinoResult.Ok("DryRun TCP 적용");
            }

            var gate = EvaluateLiveCommandSafety(
                LiveCommandKind.MoveL,
                ResolveRequestedSpeedPercent(),
                productionIkSafe: true,
                boundaryReady: false,
                collisionReady: false,
                hasGripperReadback: false);
            if (!gate.CanExecuteLive)
            {
                return BlockLiveCommand(gate, "live-movel-blocked");
            }

            var runtime = RobotControlMotionRuntime.CreateFromSelection();
            if (!runtime.IsSuccess)
            {
                PushFeedback(runtime.Message);
                RefreshSnapshot();
                return new FairinoResult(runtime.ErrorCode, runtime.Message);
            }

            var result = runtime.Value.DispatchMoveL(tcpPose, ResolveRequestedSpeedPercent());
            if (result.IsSuccess)
            {
                previewTcpPose = CopyPoseArray(tcpPose);
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[Dispatch] MoveL 완료 · {reason}");
            }
            else
            {
                PushFeedback(result.Message);
            }

            RefreshSnapshot();
            return result;
        }

        public FairinoResult PreviewPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 후보")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            previewJointAnglesDeg = jointTarget;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = true;
            requestStageRefocus = true;
            ApplyVisualState();
            PushFeedback($"[Preview] {reason}");
            RefreshSnapshot();
            return FairinoResult.Ok("Point MoveJ preview ready");
        }

        public FairinoResult ApplyPointMoveJ(double[] tcpPose, string reason = "포인트 MoveJ 적용")
        {
            if (!EnsureReadyForCommand(reason))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            var solveResult = TrySolvePointMoveJoints(tcpPose, out var jointTarget);
            if (!solveResult.IsSuccess)
            {
                PushFeedback(solveResult.Message);
                RefreshSnapshot();
                return solveResult;
            }

            return ApplyJointAngles(jointTarget, reason, liveProductionIkEligible: false);
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
            return SetGripperPositionPercent(open ? 100 : 0);
        }

        public FairinoResult SetGripperPositionPercent(int positionPercent)
        {
            var clampedPosition = ClampPercent(positionPercent);
            if (!EnsureReadyForCommand($"그리퍼 {clampedPosition}%"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                var gate = EvaluateLiveCommandSafety(
                    LiveCommandKind.MoveGripper,
                    ResolveRequestedSpeedPercent(),
                    productionIkSafe: true,
                    boundaryReady: true,
                    collisionReady: true,
                    hasGripperReadback: true);
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
                objectStopPercent);
            ApplyGripperVisual(peripheralFacade.Snapshot.GripperOpenRatio);
            PushFeedback(result.Message);
            snapshot.LiveBlockedReason = result.IsSuccess ? string.Empty : result.Message;
            RefreshSnapshot();
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
            if (TryExecutePendingPreview())
            {
                return;
            }

            switch (snapshot.StatusKind)
            {
                case RobotControlV3RuntimeStatusKind.Disconnected:
                    ConnectDefault();
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedServoOff:
                    EnableServo();
                    break;
                case RobotControlV3RuntimeStatusKind.ConnectedUnsynced:
                    SyncCurrentState();
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

        private bool TryExecutePendingPreview()
        {
            if (!EnsureReadyForCommand("실행"))
            {
                return false;
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                ApplyJointAngles(previewJointAnglesDeg, "실행 버튼 MoveJ");
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

            var safeName = string.IsNullOrWhiteSpace(sequenceName)
                ? TeachingPointStoreAdapter.DefaultSequenceName
                : sequenceName.Trim();
            var sequence = string.Equals(safeName, RecordedPathSequenceName, StringComparison.OrdinalIgnoreCase)
                ? ResolveRecordedPathSequence()
                : WaypointStore.Load(safeName);
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback($"[Sequence] {safeName} 실행할 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

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
                PushFeedback("[Sequence] named sequence live 실행은 v1에서 잠금이다. DryRun에서 먼저 확인하고 live gate 경로를 사용해라.");
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
            bool hasGripperReadback)
        {
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            var hasPreview = previewJointAnglesDeg != null || previewTcpPose != null;
            var request = new LiveCommandSafetyGateRequest
            {
                Kind = kind,
                ConnectionService = connectionService,
                AllowDryRun = snapshot.DryRunEnabled,
                OperatorConfirmed = ConsumeLiveCommandApproval(kind),
                RequestedSpeedPercent = requestedSpeedPercent,
                SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
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
            snapshot.LiveBlockedReason = message;
            RefreshSnapshot();
            return FairinoResult.Fail(-70, message);
        }

        private bool ConsumeLiveCommandApproval(LiveCommandKind kind)
        {
            if (approvedLiveCommandUntilUtc <= DateTime.UtcNow || approvedLiveCommandKind != kind)
            {
                return false;
            }

            approvedLiveCommandKind = LiveCommandKind.ReadbackOnly;
            approvedLiveCommandUntilUtc = DateTime.MinValue;
            approvedLiveCommandToken = string.Empty;
            return true;
        }

        private void GrantLiveCommandApproval(LiveCommandKind kind, string token, int ttlSeconds)
        {
            approvedLiveCommandKind = kind;
            approvedLiveCommandToken = string.IsNullOrWhiteSpace(token) ? CreateShortToken() : token;
            approvedLiveCommandUntilUtc = DateTime.UtcNow.AddSeconds(Mathf.Clamp(ttlSeconds, 1, 90));
        }

        private void ClearPendingLiveApproval()
        {
            pendingLiveApprovalKind = LiveCommandKind.ReadbackOnly;
            pendingLiveApprovalUntilUtc = DateTime.MinValue;
            pendingLiveApprovalToken = string.Empty;
            pendingLiveApprovalRequired = false;
        }

        private static string CreateShortToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
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

        private bool TryInitialize()
        {
            if (initialized && connectionService != null && config != null && templateDefinition != null)
            {
                return true;
            }

            initialized = false;

            try
            {
                templateDefinition = RobotControlFactory.Create(RobotSelectionBridge.GetSelectedRobotId());
                config = FairinoRobotConfig.Load(templateDefinition.ConfigResourceName) ?? templateDefinition.FallbackConfigFactory();
                connectionService = templateDefinition.ConnectionServiceFactory(new FairinoErrorTranslator());
                connectionService.SetMockMode(true);
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
                currentState = new FairinoRobotState(templateDefinition.PosePresetProvider.GetReadyJointAnglesDeg(), ComputeTcpPoseFromJoints(templateDefinition.PosePresetProvider.GetReadyJointAnglesDeg()));
                ApplyVisualState();
                RefreshSnapshot();
                initialized = true;
                return true;
            }
            catch (Exception ex)
            {
                lastInitializationError = $"{ex.GetType().Name}:{ex.Message}";
                Debug.LogError($"[RobotControlV3RuntimeController] Init failed: {ex}");
                return false;
            }
        }

        private void EnsureRuntimeHelpers()
        {
            presetAnimator = EnsureComponent<PresetTransitionAnimator>(gameObject);
            waypointRunner = EnsureComponent<WaypointCycleRunner>(gameObject);
            waypointRunner.Inject(connectionService, config, presetAnimator);
            BindWaypointRunnerEvents();
            peripheralFacade ??= new RobotControlPeripheralFacade(connectionService);
            liveCommandSafetyGate ??= new LiveCommandSafetyGate();
            liveStateRecorder ??= new Fr5LiveStateRecorder(
                connectionService,
                BuildDisplayStateForDrift,
                ApplyLiveDriftBlockedReason);
            liveStateRecorder.SetConnectionInfo(templateDefinition.RobotId, config.defaultIp);
            liveStateRecorder.Attach();
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
                : ApplyJointAngles(point.jointsDeg, $"Teaching {point.name} MoveJ");
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
            liveStateRecorder?.Attach();
            connectionService.OnStateUpdated += HandleStateUpdated;
            connectionService.OnConnectionStateChanged += HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged += HandleEnableStateChanged;
            connectionService.OnConnectionLost += HandleConnectionLost;
            connectionService.OnModeChanged += HandleModeChanged;
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
            currentState = state;
            ApplyVisualState();
            RefreshSnapshot();
        }

        private void HandleConnectionStateChanged(bool _)
        {
            RefreshSnapshot();
        }

        private void HandleEnableStateChanged(bool _)
        {
            RefreshSnapshot();
        }

        private void HandleConnectionLost()
        {
            PushFeedback("[Connection] 연결 끊김 감지");
            RefreshSnapshot();
        }

        private void HandleModeChanged(bool _)
        {
            RefreshSnapshot();
        }

        private FairinoRobotState BuildDisplayStateForDrift()
        {
            return new FairinoRobotState(
                currentState.JointPosDeg,
                ComputeDisplayedTcpPose(),
                isRobotEnabled: connectionService?.Client.IsEnabled ?? false);
        }

        private void ApplyLiveDriftBlockedReason(string message)
        {
            snapshot.LiveBlockedReason = message ?? string.Empty;
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

        private void ApplyVisualState()
        {
            var displayJointAngles = previewUsesJointPose && previewJointAnglesDeg != null
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
            var jointValues = previewUsesJointPose && previewJointAnglesDeg != null
                ? CopyJointArray(previewJointAnglesDeg)
                : previewTcpVisualJointAnglesDeg != null && previewTcpPose != null
                    ? CopyJointArray(previewTcpVisualJointAnglesDeg)
                : CopyJointArray(currentState.JointPosDeg);
            var tcpValues = ComputeDisplayedTcpPose();
            snapshot.HasPendingPreview = previewUsesJointPose || previewTcpPose != null;
            snapshot.StatusKind = ResolveStatusKind();
            snapshot.RobotTitle = templateDefinition.DisplayName;
            snapshot.IpAddress = $"IP: {config.defaultIp}";
            snapshot.ConnectionCardStatus = BuildConnectionCardStatus();
            snapshot.QuickServo = connectionService.Client.IsEnabled ? "서보: ON" : "서보: OFF";
            snapshot.QuickMode = connectionService.IsMockMode ? "모드: Mock" : "모드: Live";
            snapshot.QuickSync = $"마지막 동기화: {(connectionService.Client.IsConnected ? "사용 가능" : "--")}";
            snapshot.QuickActionLabel = ResolveQuickActionLabel();
            snapshot.QuickActionEnabled = ResolveQuickActionEnabled();
            snapshot.ConnectEnabled = !connectionService.Client.IsConnected;
            snapshot.DisconnectEnabled = connectionService.Client.IsConnected;
            snapshot.ActionNow = BuildActionNow();
            snapshot.ActionPrimary = BuildActionPrimary();
            snapshot.ActionWhy = BuildActionWhy();
            snapshot.PrimaryActionLabel = ResolveQuickActionLabel();
            snapshot.PrimaryActionEnabled = ResolveQuickActionEnabled();
            snapshot.ConnectionChip = connectionService.Client.IsConnected ? "연결: 연결됨" : "연결: 미연결";
            snapshot.ModeChip = connectionService.IsMockMode ? "모드: Mock" : "모드: Live";
            snapshot.SpeedChip = snapshot.StatusSpeed = $"{ResolveRequestedSpeedPercent()}%";
            snapshot.CoordChip = $"좌표계: {snapshot.CoordSystem}";
            snapshot.SafetyChip = $"안전: {snapshot.StatusSafety}";
            snapshot.FaultChip = $"Fault: {snapshot.StatusFault}";
            snapshot.ToolChip = $"Tool: {connectionService.LastCoordContext.ToolId:00}";
            snapshot.UserChip = $"User: {connectionService.LastCoordContext.UserId:00}";
            snapshot.ConnectionClass = connectionService.Client.IsConnected ? "rc-status-chip--success" : "rc-status-chip--muted";
            snapshot.ModeClass = connectionService.IsMockMode ? "rc-status-chip--warning" : "rc-status-chip--success";
            snapshot.SpeedClass = "rc-status-chip--muted";
            snapshot.SafetyClass = snapshot.StatusKind == RobotControlV3RuntimeStatusKind.Fault ? "rc-status-chip--danger" : "rc-status-chip--success";
            snapshot.FaultClass = snapshot.StatusKind == RobotControlV3RuntimeStatusKind.Fault ? "rc-status-chip--danger" : "rc-status-chip--muted";
            snapshot.ServoEnabled = connectionService.Client.IsConnected && !connectionService.Client.IsEnabled;
            snapshot.RunEnabled = connectionService.Client.IsConnected;
            snapshot.StopEnabled = connectionService.Client.IsConnected;
            snapshot.PauseEnabled = true;
            snapshot.SyncEnabled = connectionService.Client.IsConnected;
            snapshot.ResetEnabled = connectionService.Client.IsConnected;
            snapshot.StatusConnection = connectionService.Client.IsConnected ? "● 연결됨" : "○ 미연결";
            snapshot.StatusMode = connectionService.IsMockMode ? "Mock" : "Live";
            snapshot.StatusServo = connectionService.Client.IsEnabled ? "ON" : "OFF";
            snapshot.StatusMotion = isPaused ? "일시정지" : (snapshot.HasPendingPreview ? "미리보기" : "대기");
            snapshot.StatusFault = connectionService.LastControllerFault.HasBlockingFault ? $"F{connectionService.LastControllerFault.MainCode}" : "없음";
            snapshot.StatusSafety = connectionService.LastControllerFault.IsSafetyStop ? "정지" : "정상";
            snapshot.StatusTool = $"{connectionService.LastCoordContext.ToolId:00}";
            snapshot.StatusUser = $"{connectionService.LastCoordContext.UserId:00}";
            snapshot.StatusConnectionClass = connectionService.Client.IsConnected ? "rc-status-value--success" : "rc-status-value--muted";
            snapshot.StatusModeClass = connectionService.IsMockMode ? "rc-status-value--warning" : "rc-status-value--success";
            snapshot.StatusServoClass = connectionService.Client.IsEnabled ? "rc-status-value--success" : "rc-status-value--warning";
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
            snapshot.HasGhostPreview = ghostRobotVisual != null && ghostRobotVisual.HasGhost;
            snapshot.HasPredictedPath = predictedPathRenderer != null && predictedPathRenderer.HasPath;
            ApplyPeripheralSnapshot();
            ApplySelectedPartSnapshot();
            snapshot.LastFeedback = snapshot.LastFeedback;
            SnapshotChanged?.Invoke(snapshot.Clone());
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
                snapshot.GripperObjectStopPercent = 0;
                snapshot.GripperRawObjectStopPercent = 0;
                snapshot.GripperVisualAttached = false;
                snapshot.RobotDoSummary = "DO0 OFF / DO1 OFF";
                snapshot.ToolDoSummary = "ToolDO0 OFF / ToolDO1 OFF";
                snapshot.PeripheralFeedback = "주변장치 facade 없음";
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
            snapshot.GripperObjectStopPercent = peripheral.GripperObjectStopPercent;
            snapshot.GripperRawObjectStopPercent = peripheral.GripperRawObjectStopPercent;
            snapshot.GripperVisualAttached = peripheral.GripperVisualAttached;
            var holdSuffix = peripheral.GripperHoldingObject ? " / Object Hold" : string.Empty;
            snapshot.GripperSummary = $"Gripper: Cmd {peripheral.GripperCommandedPositionPercent}% / Actual {peripheral.GripperActualPositionPercent}%{holdSuffix} ({peripheral.GripperOpenRatio:0.00}) · raw {peripheral.GripperRawCommandedPositionPercent}%/{peripheral.GripperRawActualPositionPercent}%";
            snapshot.RobotDoSummary = $"DO0 {(peripheral.RobotDigitalOutputs[0] ? "ON" : "OFF")} / DO1 {(peripheral.RobotDigitalOutputs[1] ? "ON" : "OFF")}";
            snapshot.ToolDoSummary = $"ToolDO0 {(peripheral.ToolDigitalOutputs[0] ? "ON" : "OFF")} / ToolDO1 {(peripheral.ToolDigitalOutputs[1] ? "ON" : "OFF")}";
            snapshot.PeripheralFeedback = peripheral.LastPeripheralFeedback;
            snapshot.GripperSdkSummary = peripheral.LastGripperSdkSummary;
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

        private string BuildConnectionCardStatus()
        {
            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "상태: ○ 미연결",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "상태: ● 연결됨 / 서보 OFF",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "상태: ● 연결됨 / 미동기화",
                RobotControlV3RuntimeStatusKind.Fault => "상태: ⛔ Fault",
                _ => "상태: ● 조작 가능",
            };
        }

        private string ResolveQuickActionLabel()
        {
            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "연결",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "서보 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "동기화",
                RobotControlV3RuntimeStatusKind.Fault => "오류 초기화",
                _ => "조작 시작",
            };
        }

        private bool ResolveQuickActionEnabled()
        {
            return snapshot.StatusKind != RobotControlV3RuntimeStatusKind.AutoReconnect;
        }

        private string BuildActionNow()
        {
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
            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "다음 행동: 먼저 연결",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "다음 행동: 서보를 먼저 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "다음 행동: 동기화 먼저",
                RobotControlV3RuntimeStatusKind.Fault => "다음 행동: 오류 초기화부터",
                _ => snapshot.PendingCommandSummary,
            };
        }

        private string BuildActionWhy()
        {
            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "현재 상태를 읽으려면 연결부터 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "실제 이동을 보내려면 서보가 먼저 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "첫 조작 전에 현재 자세를 읽는 게 덜 위험하다.",
                RobotControlV3RuntimeStatusKind.Fault => "초기화부터 누르면 같은 Fault를 다시 밟을 수 있다.",
                _ => snapshot.DryRunEnabled ? "DryRun이면 실기 명령 대신 Unity 내부 시뮬레이션만 수행한다." : "현재 화면의 적용 버튼은 실제 mock/live 경로를 탄다.",
            };
        }

        private void PushFeedback(string message)
        {
            snapshot.LastFeedback = string.IsNullOrWhiteSpace(message) ? "..." : message;
        }

        private int ResolveRequestedSpeedPercent()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? Mathf.Clamp(shellState.GetStateSnapshot().SpeedPercent, 1, 100)
                : 30;
        }

        private void RecordUndo(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null)
            {
                return;
            }

            undoJointHistory.Push(CopyJointArray(jointAnglesDeg));
            redoJointHistory.Clear();
        }

        private double[] ComputeDisplayedTcpPose()
        {
            if (previewTcpPose != null && !previewUsesJointPose)
            {
                return CopyPoseArray(previewTcpPose);
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                return ComputeTcpPoseFromJoints(previewJointAnglesDeg);
            }

            return currentState.TcpPose != null && currentState.TcpPose.Length >= 6
                ? CopyPoseArray(currentState.TcpPose)
                : ComputeTcpPoseFromJoints(currentState.JointPosDeg);
        }

        private double[] ComputeTcpPoseFromJoints(double[] jointAnglesDeg)
        {
            if (kinematicsFacade == null || jointAnglesDeg == null)
            {
                return new double[6];
            }

            kinematicsFacade.SetJointAnglesDegrees(jointAnglesDeg);
            var ee = kinematicsFacade.EndEffectorTransform;
            var position = ee.ExtractPosition();
            return new[]
            {
                position.X * 1000.0,
                position.Y * 1000.0,
                position.Z * 1000.0,
                180.0,
                0.0,
                90.0,
            };
        }

        private FairinoResult TrySolvePointMoveJoints(double[] targetTcpPose, out double[] jointTarget)
        {
            jointTarget = null;
            if (targetTcpPose == null || targetTcpPose.Length < 6)
            {
                return FairinoResult.Fail(-34, "Point MoveJ 대상 TCP 값이 부족하다.");
            }

            if (previewKinematicsFacade == null)
            {
                return FairinoResult.Fail(-35, "Point MoveJ IK 계산기가 아직 준비되지 않았다.");
            }

            var targetPosition = new Vec3D(
                targetTcpPose[0] / 1000.0,
                targetTcpPose[1] / 1000.0,
                targetTcpPose[2] / 1000.0);
            var seed = previewUsesJointPose && previewJointAnglesDeg != null
                ? previewJointAnglesDeg
                : currentState.JointPosDeg;
            var candidate = ClampJointTarget(CopyJointArray(seed));
            var bestErrorMm = ComputePreviewPositionErrorMm(candidate, targetPosition);
            var stepDeg = 12.0;

            for (var iteration = 0; iteration < 72 && bestErrorMm > 2.0; iteration++)
            {
                var improved = false;
                for (var jointIndex = 0; jointIndex < templateDefinition.JointCount; jointIndex++)
                {
                    improved |= TryImproveJoint(candidate, jointIndex, stepDeg, targetPosition, ref bestErrorMm);
                }

                if (!improved)
                {
                    stepDeg *= 0.55;
                    if (stepDeg < 0.05)
                    {
                        break;
                    }
                }
            }

            if (bestErrorMm > 8.0)
            {
                return FairinoResult.Fail(-36, $"Point MoveJ IK 실패 · 위치 오차 {bestErrorMm:0.0}mm");
            }

            jointTarget = candidate;
            return FairinoResult.Ok($"Point MoveJ IK 완료 · 위치 오차 {bestErrorMm:0.0}mm");
        }

        private bool TryImproveJoint(double[] candidate, int jointIndex, double stepDeg, Vec3D targetPosition, ref double bestErrorMm)
        {
            var original = candidate[jointIndex];
            var bestValue = original;
            var improved = false;

            for (var direction = -1; direction <= 1; direction += 2)
            {
                candidate[jointIndex] = ClampJointValue(jointIndex, original + (direction * stepDeg));
                var errorMm = ComputePreviewPositionErrorMm(candidate, targetPosition);
                if (errorMm + 0.001 < bestErrorMm)
                {
                    bestErrorMm = errorMm;
                    bestValue = candidate[jointIndex];
                    improved = true;
                }
            }

            candidate[jointIndex] = bestValue;
            return improved;
        }

        private double ComputePreviewPositionErrorMm(double[] jointsDeg, Vec3D targetPosition)
        {
            previewKinematicsFacade.SetJointAnglesDegrees(jointsDeg);
            var position = previewKinematicsFacade.EndEffectorTransform.ExtractPosition();
            return (position - targetPosition).Magnitude() * 1000.0;
        }

        private double[] ClampJointTarget(double[] jointsDeg)
        {
            var result = jointsDeg ?? new double[templateDefinition.JointCount];
            for (var index = 0; index < templateDefinition.JointCount && index < result.Length; index++)
            {
                result[index] = ClampJointValue(index, result[index]);
            }

            return result;
        }

        private double ClampJointValue(int jointIndex, double value)
        {
            if (config?.jointLimits != null && jointIndex < config.jointLimits.Length && config.jointLimits[jointIndex] != null)
            {
                var limit = config.jointLimits[jointIndex];
                return System.Math.Max(limit.minDeg, System.Math.Min(limit.maxDeg, value));
            }

            return System.Math.Max(-360.0, System.Math.Min(360.0, value));
        }

        private List<Vector3> BuildJointPreviewPath(double[] startJointAnglesDeg, double[] endJointAnglesDeg)
        {
            var result = new List<Vector3>();
            if (previewKinematicsFacade == null || startJointAnglesDeg == null || endJointAnglesDeg == null)
            {
                return result;
            }

            const int sampleCount = 24;
            var lerped = new double[templateDefinition.JointCount];
            for (var sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex++)
            {
                var t = sampleIndex / (double)sampleCount;
                for (var jointIndex = 0; jointIndex < templateDefinition.JointCount; jointIndex++)
                {
                    var start = jointIndex < startJointAnglesDeg.Length ? startJointAnglesDeg[jointIndex] : 0d;
                    var end = jointIndex < endJointAnglesDeg.Length ? endJointAnglesDeg[jointIndex] : start;
                    lerped[jointIndex] = Mathf.Lerp((float)start, (float)end, (float)t);
                }

                previewKinematicsFacade.SetJointAnglesDegrees(lerped);
                result.Add(CoordConverter.ToUnityPosition(previewKinematicsFacade.EndEffectorTransform.ExtractPosition()));
            }

            return result;
        }

        private List<Vector3> BuildCartesianPreviewPath(double[] startTcpPose, double[] endTcpPose)
        {
            var result = new List<Vector3>();
            if (startTcpPose == null || endTcpPose == null || startTcpPose.Length < 3 || endTcpPose.Length < 3)
            {
                return result;
            }

            const int sampleCount = 18;
            var start = new Vec3D(startTcpPose[0] / 1000.0, startTcpPose[1] / 1000.0, startTcpPose[2] / 1000.0);
            var end = new Vec3D(endTcpPose[0] / 1000.0, endTcpPose[1] / 1000.0, endTcpPose[2] / 1000.0);
            for (var sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex++)
            {
                var t = sampleIndex / (double)sampleCount;
                var point = new Vec3D(
                    start.X + ((end.X - start.X) * t),
                    start.Y + ((end.Y - start.Y) * t),
                    start.Z + ((end.Z - start.Z) * t));
                result.Add(CoordConverter.ToUnityPosition(point));
            }

            return result;
        }

        private void ApplySelectedPartSnapshot()
        {
            var selectedTarget = partSelectionGizmo != null ? partSelectionGizmo.SelectedTarget : null;
            snapshot.HasSelectedPart = selectedTarget != null;
            snapshot.SelectedPartName = selectedTarget != null ? selectedTarget.name : "선택된 파츠 없음";
            snapshot.SelectedPartHint = selectedTarget != null
                ? "선택 링크를 강조하고 작은 좌표축만 붙인다. 자세한 값은 여기서 본다."
                : "메인 로봇 메시를 클릭하면 선택 파츠 정보를 여기서 본다.";

            if (selectedTarget == null)
            {
                snapshot.SelectedPartPose = "XYZ -- / ROT --";
                return;
            }

            var roboticsPosition = CoordConverter.FromUnityPosition(selectedTarget.position);
            var rotation = selectedTarget.rotation.eulerAngles;
            snapshot.SelectedPartPose =
                $"XYZ {roboticsPosition.X * 1000.0:0.#}, {roboticsPosition.Y * 1000.0:0.#}, {roboticsPosition.Z * 1000.0:0.#} / ROT {rotation.x:0.#}, {rotation.y:0.#}, {rotation.z:0.#}";
        }

        private static string[] FormatValues(double[] values, string format)
        {
            var result = new string[6];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = values != null && i < values.Length
                    ? values[i].ToString(format, System.Globalization.CultureInfo.InvariantCulture)
                    : "--";
            }

            return result;
        }

        private FR5PosePresets.Preset? ResolvePreset(string presetName)
        {
            foreach (var preset in FR5PosePresets.All)
            {
                if (string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase))
                {
                    return preset;
                }
            }

            return null;
        }

        private static double[] CopyJointArray(double[] source)
        {
            return source != null ? (double[])source.Clone() : new double[6];
        }

        private static double[] CopyPoseArray(double[] source)
        {
            return source != null ? (double[])source.Clone() : new double[6];
        }

        private Transform FindBaseLink(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            var baseLinkName = !string.IsNullOrWhiteSpace(templateDefinition?.BaseLinkName)
                ? templateDefinition.BaseLinkName
                : "base_link";
            if (root.name == baseLinkName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindBaseLink(root.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RepairVisualMeshes(GameObject controlRoot)
        {
            if (controlRoot == null)
            {
                return;
            }

            var meshFilters = controlRoot.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var meshFilter = meshFilters[i];
                if (meshFilter == null)
                {
                    continue;
                }

                var mesh = meshFilter.sharedMesh != null ? meshFilter.sharedMesh : meshFilter.mesh;
                if (mesh == null)
                {
                    continue;
                }

                mesh.RecalculateBounds();
                if (meshFilter.sharedMesh == null)
                {
                    meshFilter.sharedMesh = mesh;
                }
            }
        }

        private void StabilizeControlRobot(GameObject controlRoot)
        {
            if (controlRoot == null)
            {
                return;
            }

            var components = controlRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                {
                    continue;
                }

                if (component.GetType().FullName == "Unity.Robotics.UrdfImporter.Control.Controller")
                {
                    component.enabled = false;
                }
            }

            var articulationBodies = controlRoot.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < articulationBodies.Length; i++)
            {
                var body = articulationBodies[i];
                if (body == null)
                {
                    continue;
                }

                body.useGravity = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                if (!body.isRoot)
                {
                    body.enabled = false;
                }
            }

            var baseLink = FindBaseLink(controlRoot.transform);
            var baseBody = baseLink != null ? baseLink.GetComponent<ArticulationBody>() : null;
            if (baseBody != null)
            {
                baseBody.immovable = true;
            }
        }

        private static PendantV3PreviewState.Kind ToPreviewKind(RobotControlV3RuntimeStatusKind kind)
        {
            return kind switch
            {
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => PendantV3PreviewState.Kind.ConnectedServoOff,
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => PendantV3PreviewState.Kind.ConnectedUnsynced,
                RobotControlV3RuntimeStatusKind.ReadyToJog => PendantV3PreviewState.Kind.ReadyToJog,
                RobotControlV3RuntimeStatusKind.Fault => PendantV3PreviewState.Kind.Fault,
                RobotControlV3RuntimeStatusKind.AutoReconnect => PendantV3PreviewState.Kind.AutoReconnect,
                _ => PendantV3PreviewState.Kind.Disconnected,
            };
        }
    }
}
