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
        private EETrailRenderer eeTrailRenderer;
        private DisplacementArrow displacementArrow;
        private TargetMarkerVisual targetMarkerVisual;
        private GhostRobotVisual ghostRobotVisual;
        private PredictedPathRenderer predictedPathRenderer;
        private RobotStageFloorGrid stageFloorGrid;
        private RobotPartSelectionGizmo partSelectionGizmo;
        private PresetTransitionAnimator presetAnimator;
        private WaypointCycleRunner waypointRunner;
        private RobotControlPeripheralFacade peripheralFacade;
        private LiveCommandSafetyGate liveCommandSafetyGate;
        private ManualReadbackTeachingProbe manualReadbackTeachingProbe;
        private TeachingPointStoreAdapter teachingPointStoreAdapter;
        private TeachingSequenceRuntime teachingSequenceRuntime;
        private RobotKinematicsFacade previewKinematicsFacade;
        private FR5EndEffectorAttachment endEffectorAttachment;
        private Camera stageCamera;
        private Light stageLight;
        private Transform stageCameraPivot;
        private RobotControlV3RuntimeSnapshot snapshot = new();
        private FairinoRobotState currentState = FairinoRobotState.Zero();
        private double[] previewJointAnglesDeg;
        private double[] previewTcpPose;
        private bool previewUsesJointPose;
        private bool showBaseFrame = true;
        private bool showToolFrame = true;
        private bool showTrail = true;
        private bool showGhost = true;
        private bool showWorkspaceBoundary;
        private bool showCollision;
        private bool isPaused;
        private bool teachingLoopEnabled;
        private bool waypointRunnerEventsBound;
        private bool initialized;
        private string lastInitializationError = string.Empty;
        private string lastSelectedPartName = "없음";
        private bool requestStageRefocus;
        private LiveCommandKind approvedLiveCommandKind = LiveCommandKind.ReadbackOnly;
        private DateTime approvedLiveCommandUntilUtc = DateTime.MinValue;
        private string approvedLiveCommandToken = string.Empty;
        private LiveCommandKind pendingLiveApprovalKind = LiveCommandKind.ReadbackOnly;
        private DateTime pendingLiveApprovalUntilUtc = DateTime.MinValue;
        private string pendingLiveApprovalToken = string.Empty;
        private bool pendingLiveApprovalRequired;

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
            UnbindConnectionEvents();
            UnbindWaypointRunnerEvents();
            initialized = false;
        }

        private void Update()
        {
            connectionService?.Tick(Time.deltaTime);
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={initialized}; connected={connectionService?.Client.IsConnected ?? false}; enabled={connectionService?.Client.IsEnabled ?? false}; dryRun={snapshot.DryRunEnabled}; pending={snapshot.PendingCommandSummary}; selected={lastSelectedPartName}; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; grid={(stageFloorGrid != null)}; gizmo={(partSelectionGizmo != null)}; initError={lastInitializationError}";
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
            return $"attached=True; active={root.gameObject.activeInHierarchy}; renderers={renderers.Length}; activeRenderers={activeRendererCount}; meshFilters={meshFilters.Length}; local=({local.x:0.###},{local.y:0.###},{local.z:0.###}); rot=({euler.x:0.#},{euler.y:0.#},{euler.z:0.#}); scale=({root.localScale.x:0.###},{root.localScale.y:0.###},{root.localScale.z:0.###}); tcpLocal=({tcpLocal.x:0.####},{tcpLocal.y:0.####},{tcpLocal.z:0.####}); modelLocal=({modelLocal.x:0.####},{modelLocal.y:0.####},{modelLocal.z:0.####}); fingerLeft=({fingerLeftLocal.x:0.####},{fingerLeftLocal.y:0.####},{fingerLeftLocal.z:0.####}); fingerRight=({fingerRightLocal.x:0.####},{fingerRightLocal.y:0.####},{fingerRightLocal.z:0.####}); boundsCenter=({bounds.center.x:0.###},{bounds.center.y:0.###},{bounds.center.z:0.###}); boundsSize=({bounds.size.x:0.###},{bounds.size.y:0.###},{bounds.size.z:0.###}); viewport=({viewport.x:0.###},{viewport.y:0.###},{viewport.z:0.###}); cameraVisible={cameraVisible}; openRatio={endEffectorAttachment.GripperOpenRatio:0.00}";
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
                return;
            }

            var target = FindBaseLink(controlRobotInstance != null ? controlRobotInstance.transform : runtimeRoot) ?? runtimeRoot;
            var targetPosition = target != null ? target.position + new Vector3(0f, 0.8f, 0f) : Vector3.zero;
            stageCamera.transform.position = targetPosition + new Vector3(0f, 1.6f, -4.8f);
            stageCamera.transform.LookAt(targetPosition);
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
                var selected = hit.collider != null ? hit.collider.transform : hit.transform;
                partSelectionGizmo?.Select(selected);
                lastSelectedPartName = selected.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            var fallbackTarget = FindRendererHit(ray);
            if (fallbackTarget != null)
            {
                partSelectionGizmo?.Select(fallbackTarget);
                lastSelectedPartName = fallbackTarget.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            partSelectionGizmo?.Clear();
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
                previewTcpPose = CopyPoseArray(tcpPose);
                previewUsesJointPose = false;
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback($"[DryRun Apply] {reason}");
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
            if (!EnsureReadyForCommand(open ? "그리퍼 열기" : "그리퍼 닫기"))
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

            var result = peripheralFacade.SetGripperOpen(open, snapshot.DryRunEnabled);
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
            if (initialized)
            {
                return true;
            }

            try
            {
                templateDefinition = RobotControlFactory.Create(RobotSelectionBridge.GetSelectedRobotId());
                config = FairinoRobotConfig.Load(templateDefinition.ConfigResourceName) ?? templateDefinition.FallbackConfigFactory();
                connectionService = templateDefinition.ConnectionServiceFactory(new FairinoErrorTranslator());
                connectionService.SetMockMode(true);
                connectionService.ApplyLiveDefaults(config.liveDefaults);
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
            manualReadbackTeachingProbe ??= new ManualReadbackTeachingProbe(connectionService);
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
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
        }

        private void UnbindConnectionEvents()
        {
            if (connectionService == null)
            {
                return;
            }

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
            partSelectionGizmo.Clear();

            var ghostHost = runtimeRoot.Find("GhostRobotVisual") ?? new GameObject("GhostRobotVisual").transform;
            ghostHost.SetParent(runtimeRoot, false);
            ghostRobotVisual = EnsureComponent<GhostRobotVisual>(ghostHost.gameObject);
            ghostRobotVisual.EnsureGhost(controlRobotInstance, templateDefinition.BaseLinkName);

            var pathHost = runtimeRoot.Find("PredictedPath") ?? new GameObject("PredictedPath").transform;
            pathHost.SetParent(runtimeRoot, false);
            predictedPathRenderer = EnsureComponent<PredictedPathRenderer>(pathHost.gameObject);
            predictedPathRenderer.ClearPath();

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
                peripheralFacade?.SetGripperVisualAttached(true);
                ResetStageCamera();
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
                ResetStageCamera();
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
            ResetStageCamera();
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
            endEffectorAttachment.SetFingers(fingerLeft, fingerRight);
            endEffectorAttachment.SetGripperOpen(peripheralFacade?.Snapshot.GripperOpenRatio ?? 0f);
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
                ResetStageCamera();
            }

            peripheralFacade?.SetGripperVisualAttached(endEffectorAttachment != null);
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

                frameGizmoFactory?.SetVisible(showBaseFrame || showToolFrame);
                if (frameGizmoFactory != null && kinematicsFacade != null)
                {
                    frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
                }
            }

            ghostRobotVisual?.SetVisible(false);
            predictedPathRenderer?.ClearPath();

            if (previewUsesJointPose && previewJointAnglesDeg != null && previewJointAnglesDeg.Length >= templateDefinition.JointCount)
            {
                ghostRobotVisual?.ApplyJointAngles(previewJointAnglesDeg);
                ghostRobotVisual?.SetVisible(showGhost);
                predictedPathRenderer?.RenderPath(BuildJointPreviewPath(currentState.JointPosDeg, previewJointAnglesDeg));
            }
            else if (previewTcpPose != null && !previewUsesJointPose)
            {
                predictedPathRenderer?.RenderPath(BuildCartesianPreviewPath(currentState.TcpPose, previewTcpPose));
            }

            if (previewTcpPose != null && !previewUsesJointPose)
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
                ResetStageCamera();
            }
        }

        private void RefreshSnapshot()
        {
            var jointValues = previewUsesJointPose && previewJointAnglesDeg != null
                ? CopyJointArray(previewJointAnglesDeg)
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
                snapshot.GripperOpenRatio = 0f;
                snapshot.GripperVisualAttached = false;
                snapshot.RobotDoSummary = "DO0 OFF / DO1 OFF";
                snapshot.ToolDoSummary = "ToolDO0 OFF / ToolDO1 OFF";
                snapshot.PeripheralFeedback = "주변장치 facade 없음";
                return;
            }

            var peripheral = peripheralFacade.Snapshot;
            snapshot.GripperOpenRatio = peripheral.GripperOpenRatio;
            snapshot.GripperVisualAttached = peripheral.GripperVisualAttached;
            snapshot.GripperSummary = $"Gripper: Cmd {(peripheral.GripperOpen ? "Open" : "Close")} / Visual {(peripheral.GripperOpenRatio > 0.001f ? "Open" : "Closed")} ({peripheral.GripperOpenRatio:0.00})";
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
                ? "선택 파츠 기준 XYZ 기즈모는 메인 로봇 위에만 두고, 자세한 값은 여기서 본다."
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

            if (root.name == templateDefinition.BaseLinkName)
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
