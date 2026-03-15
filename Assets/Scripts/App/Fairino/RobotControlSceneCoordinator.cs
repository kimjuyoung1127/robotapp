// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControl 씬의 초기화를 담당하는 코디네이터입니다.
    /// 연결 서비스, 패널, 설정 로드, FR5 control prefab 복원을 조율합니다.
    /// 3D 관절 미러, 슬라이더 프리뷰, TCP 제어, 기즈모/트레일, 프리셋, 핸들을 통합합니다.
    /// </summary>
    public class RobotControlSceneCoordinator : MonoBehaviour
    {
        private const string FairinoRobotId = "FAIRINO_FR5";
        private const string RuntimeRootName = "FR5_RuntimeRoot";
        private const string ControlRobotInstanceName = "FR5_UrdfInstance";
        private const string ControlPrefabResourcePath = "Robots/FAIRINO_FR5_Control";
        private const string ShowroomPrefabResourcePath = "Robots/FAIRINO_FR5";
        private const float TargetRobotHeight = 1.5f;
        private const int JointCount = 6;

        [SerializeField] private FairinoConnectionPanel connectionPanel;
        [SerializeField] private FairinoJointControlPanel jointControlPanel;
        [SerializeField] private FairinoStatePanel statePanel;
        [SerializeField] private FairinoTcpControlPanel tcpPanel;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Transform runtimeRoot;
        [SerializeField] private GameObject controlRobotInstance;

        private FairinoConnectionService connectionService;
        private FairinoErrorTranslator errorTranslator;
        private FairinoRobotConfig config;
        private FairinoUrdfJointDriver jointDriver;
        private FR5KinematicsFacade kinematicsFacade;
        private FrameGizmoFactory frameGizmoFactory;
        private EETrailRenderer eeTrailRenderer;
        private DisplacementArrow displacementArrow;
        private FairinoWhyItMovedLabel whyItMovedLabel;
        private FairinoMoveConfirmDialog moveConfirmDialog;
        private PresetTransitionAnimator presetAnimator;
        private JointRotationHandle[] jointHandles;
        private OrbitCameraController orbitCamera;
        private Toggle gizmoToggle;
        private Button clearTrailButton;
        private bool listenersBound;
        private bool isFirstStateAfterConnect;

        private void Awake()
        {
            FairinoRobotControlViewBuilder.EnsureEventSystem();
            canvas = FairinoRobotControlViewBuilder.EnsureCanvas(canvas, fallbackFont);
            FairinoRobotControlViewBuilder.EnsureCamera();
            FairinoRobotControlViewBuilder.EnsureLight();
            FairinoRobotControlViewBuilder.EnsureLayout(canvas, fallbackFont,
                out connectionPanel, out jointControlPanel, out statePanel,
                out tcpPanel, out whyItMovedLabel, out moveConfirmDialog,
                out gizmoToggle, out clearTrailButton);

            errorTranslator = new FairinoErrorTranslator();
            connectionService = new FairinoConnectionService(errorTranslator);
            connectionService.SetMockMode(true);
            config = FairinoRobotConfig.Load() ?? BuildFallbackConfig();

            kinematicsFacade = new FR5KinematicsFacade();

            EnsureRobotSelection();
            EnsureRuntimeRoot();
            EnsureControlRobot();
            EnsureJointDriver();
            EnsureVisualizationHelpers();
            EnsureOrbitCamera();
            EnsureJointHandles();
            EnsurePresetAnimator();
            InjectDependencies();
            BindListeners();
        }

        private void OnEnable()
        {
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void Update()
        {
            connectionService?.Tick(Time.deltaTime);
        }

        private void InjectDependencies()
        {
            connectionPanel?.Inject(connectionService, config);
            jointControlPanel?.Inject(connectionService, config);
            statePanel?.Inject(connectionService, errorTranslator, kinematicsFacade);
            tcpPanel?.Inject(connectionService, config, kinematicsFacade);
            whyItMovedLabel?.Inject(kinematicsFacade);
            jointControlPanel?.InjectMoveConfirmDialog(moveConfirmDialog);
            tcpPanel?.InjectMoveConfirmDialog(moveConfirmDialog);

            // Mock 모드에서는 Sync 버튼 비활성화
            jointControlPanel?.SetSyncEnabled(!connectionService.IsMockMode);
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            if (connectionService != null)
            {
                connectionService.OnStateUpdated += OnStateUpdated;
                connectionService.OnModeChanged += OnModeChanged;
                connectionService.OnConnectionLost += OnConnectionLost;
                connectionService.OnConnectionStateChanged += OnConnectionStateChanged;
            }

            if (jointControlPanel != null)
            {
                jointControlPanel.OnJointSliderPreview += OnJointSliderPreview;
                jointControlPanel.OnPresetApplied += OnPresetApplied;
                jointControlPanel.OnSyncRequested += OnSyncRequested;
            }

            if (tcpPanel != null)
            {
                tcpPanel.OnTcpMoveRequested += OnTcpMoveRequested;
            }

            if (gizmoToggle != null)
            {
                gizmoToggle.onValueChanged.AddListener(OnGizmoToggleChanged);
            }

            if (clearTrailButton != null)
            {
                clearTrailButton.onClick.AddListener(OnClearTrailClicked);
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated += OnAnimationFrameUpdated;
                presetAnimator.OnTransitionComplete += OnAnimationComplete;
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            if (connectionService != null)
            {
                connectionService.OnStateUpdated -= OnStateUpdated;
                connectionService.OnModeChanged -= OnModeChanged;
                connectionService.OnConnectionLost -= OnConnectionLost;
                connectionService.OnConnectionStateChanged -= OnConnectionStateChanged;
            }

            if (jointControlPanel != null)
            {
                jointControlPanel.OnJointSliderPreview -= OnJointSliderPreview;
                jointControlPanel.OnPresetApplied -= OnPresetApplied;
                jointControlPanel.OnSyncRequested -= OnSyncRequested;
            }

            if (tcpPanel != null)
            {
                tcpPanel.OnTcpMoveRequested -= OnTcpMoveRequested;
            }

            if (gizmoToggle != null)
            {
                gizmoToggle.onValueChanged.RemoveListener(OnGizmoToggleChanged);
            }

            if (clearTrailButton != null)
            {
                clearTrailButton.onClick.RemoveListener(OnClearTrailClicked);
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated -= OnAnimationFrameUpdated;
                presetAnimator.OnTransitionComplete -= OnAnimationComplete;
            }

            UnbindHandleListeners();

            listenersBound = false;
        }

        private void UnbindHandleListeners()
        {
            if (jointHandles == null)
            {
                return;
            }

            for (var i = 0; i < jointHandles.Length; i++)
            {
                if (jointHandles[i] != null)
                {
                    jointHandles[i].OnHandleDragged -= OnHandleDragged;
                    jointHandles[i].OnDragStateChanged -= OnHandleDragStateChanged;
                }
            }
        }

        private void OnStateUpdated(FairinoRobotState state)
        {
            // 연결 직후 첫 상태: 현재 포즈에서 실제 포즈로 부드럽게 전환
            if (isFirstStateAfterConnect && presetAnimator != null)
            {
                isFirstStateAfterConnect = false;
                var currentAngles = GetCurrentAnglesDeg();
                presetAnimator.StartTransition(currentAngles, state.JointPosDeg, UIDesignTokens.Anim.ConnectionSync);
                return;
            }

            if (jointDriver != null)
            {
                jointDriver.ApplyJointAngles(state);
            }

            jointControlPanel?.SetSliderValues(state.JointPosDeg);

            kinematicsFacade?.SetJointAnglesDegrees(state.JointPosDeg);

            whyItMovedLabel?.HandleStateUpdated(state);

            if (frameGizmoFactory != null && frameGizmoFactory.IsVisible && kinematicsFacade != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }

            SyncHandleAngles(state.JointPosDeg);
        }

        private void OnJointSliderPreview(double[] jointAnglesDeg)
        {
            if (presetAnimator != null && presetAnimator.IsAnimating)
            {
                presetAnimator.Cancel();
            }

            if (jointDriver != null)
            {
                jointDriver.ApplyJointAngles(jointAnglesDeg);
            }

            kinematicsFacade?.SetJointAnglesDegrees(jointAnglesDeg);

            if (eeTrailRenderer != null && kinematicsFacade != null)
            {
                eeTrailRenderer.AddPoint(kinematicsFacade.EndEffectorTransform);
            }

            if (displacementArrow != null && kinematicsFacade != null)
            {
                displacementArrow.UpdateFromFK(kinematicsFacade.EndEffectorTransform);
            }

            if (frameGizmoFactory != null && frameGizmoFactory.IsVisible && kinematicsFacade != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }

            SyncHandleAngles(jointAnglesDeg);
        }

        private void OnPresetApplied(double[] jointAnglesDeg)
        {
            eeTrailRenderer?.Clear();
            displacementArrow?.Clear();

            if (presetAnimator != null && kinematicsFacade != null)
            {
                var currentAngles = GetCurrentAnglesDeg();
                presetAnimator.StartTransition(currentAngles, jointAnglesDeg, UIDesignTokens.Anim.PresetTransition);
                return;
            }

            // Fallback: 즉시 적용
            ApplyJointSnapshot(jointAnglesDeg);
        }

        private void OnTcpMoveRequested(double[] tcpPose)
        {
            // TCP 이동 후 FK 결과로 3D 업데이트 (실제 로봇 상태는 폴링으로 반영)
            Debug.Log($"[Coordinator] TCP Move requested: X={tcpPose[0]:F1} Y={tcpPose[1]:F1} Z={tcpPose[2]:F1}");
        }

        private void OnSyncRequested()
        {
            if (connectionService == null)
            {
                return;
            }

            var result = connectionService.SyncCurrentState();
            if (result.IsSuccess)
            {
                FR5PosePresets.UpdateCurrent(result.Value.JointPosDeg);
            }
        }

        private void OnModeChanged(bool isMock)
        {
            jointControlPanel?.SetSyncEnabled(!isMock);
        }

        private void OnConnectionLost()
        {
            presetAnimator?.Cancel();
            jointControlPanel?.SetControlsEnabled(false);
            tcpPanel?.SetControlsEnabled(false);
            connectionPanel?.ShowConnectionLost();
            Debug.LogWarning("[RobotControlSceneCoordinator] 연결 끊김 감지 — 패널 비활성화");
        }

        private void OnConnectionStateChanged(bool connected)
        {
            if (connected)
            {
                jointControlPanel?.SetControlsEnabled(true);
                tcpPanel?.SetControlsEnabled(true);
                isFirstStateAfterConnect = true;
            }
        }

        private void OnGizmoToggleChanged(bool value)
        {
            frameGizmoFactory?.SetVisible(value);
            if (value && kinematicsFacade != null && frameGizmoFactory != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }
        }

        private void OnClearTrailClicked()
        {
            eeTrailRenderer?.Clear();
            displacementArrow?.Clear();
        }

        private void OnHandleDragged(int jointIndex, float angleDeg)
        {
            if (presetAnimator != null && presetAnimator.IsAnimating)
            {
                presetAnimator.Cancel();
            }

            if (jointControlPanel == null || kinematicsFacade == null)
            {
                return;
            }

            // 현재 FK의 관절값을 기준으로 해당 관절만 업데이트
            var currentRad = kinematicsFacade.JointValuesRad;
            var values = new double[JointCount];
            for (var i = 0; i < JointCount && i < currentRad.Length; i++)
            {
                values[i] = currentRad[i] * (180.0 / System.Math.PI);
            }

            values[jointIndex] = angleDeg;
            jointControlPanel.SetSliderValues(values);

            // 슬라이더 이벤트를 직접 트리거하여 3D/FK 동기화
            OnJointSliderPreview(values);
        }

        private void OnHandleDragStateChanged(bool isDragging)
        {
            if (orbitCamera != null)
            {
                orbitCamera.OrbitEnabled = !isDragging;
            }
        }

        /// <summary>
        /// Mock↔Live 모드를 전환합니다.
        /// </summary>
        public void SetMockMode(bool mock)
        {
            connectionService?.SetMockMode(mock);
        }

        /// <summary>
        /// 프레임 기즈모 가시성을 토글합니다.
        /// </summary>
        public void SetFrameGizmosVisible(bool visible)
        {
            frameGizmoFactory?.SetVisible(visible);
        }

        /// <summary>
        /// EE 트레일을 초기화합니다.
        /// </summary>
        public void ClearTrail()
        {
            eeTrailRenderer?.Clear();
        }

        private void SyncHandleAngles(double[] jointAnglesDeg)
        {
            if (jointHandles == null || jointAnglesDeg == null)
            {
                return;
            }

            for (var i = 0; i < JointCount && i < jointAnglesDeg.Length && i < jointHandles.Length; i++)
            {
                if (jointHandles[i] != null)
                {
                    jointHandles[i].SetAngle((float)jointAnglesDeg[i]);
                }
            }
        }

        private void EnsureJointDriver()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            jointDriver = controlRobotInstance.GetComponent<FairinoUrdfJointDriver>()
                ?? controlRobotInstance.AddComponent<FairinoUrdfJointDriver>();

            var baseLink = controlRobotInstance.transform.Find("base_link");
            if (baseLink != null)
            {
                jointDriver.Inject(baseLink);
            }
        }

        private void EnsureVisualizationHelpers()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            var gizmoHost = runtimeRoot.Find("FrameGizmos");
            if (gizmoHost == null)
            {
                var go = new GameObject("FrameGizmos");
                go.transform.SetParent(runtimeRoot, false);
                gizmoHost = go.transform;
            }

            frameGizmoFactory = gizmoHost.GetComponent<FrameGizmoFactory>()
                ?? gizmoHost.gameObject.AddComponent<FrameGizmoFactory>();
            frameGizmoFactory.SetVisible(false);

            var trailHost = runtimeRoot.Find("EETrail");
            if (trailHost == null)
            {
                var go = new GameObject("EETrail");
                go.transform.SetParent(runtimeRoot, false);
                trailHost = go.transform;
            }

            eeTrailRenderer = trailHost.GetComponent<EETrailRenderer>()
                ?? trailHost.gameObject.AddComponent<EETrailRenderer>();

            var arrowHost = runtimeRoot.Find("DisplacementArrow");
            if (arrowHost == null)
            {
                var go = new GameObject("DisplacementArrow");
                go.transform.SetParent(runtimeRoot, false);
                arrowHost = go.transform;
            }

            displacementArrow = arrowHost.GetComponent<DisplacementArrow>()
                ?? arrowHost.gameObject.AddComponent<DisplacementArrow>();
        }

        private void EnsureOrbitCamera()
        {
            var camera = Camera.main;
            if (camera == null || controlRobotInstance == null)
            {
                return;
            }

            orbitCamera = camera.GetComponent<OrbitCameraController>();
            if (orbitCamera == null)
            {
                orbitCamera = camera.gameObject.AddComponent<OrbitCameraController>();
            }

            var baseLink = controlRobotInstance.transform.Find("base_link");
            orbitCamera.SetTarget(baseLink != null ? baseLink : controlRobotInstance.transform);
        }

        private void EnsureJointHandles()
        {
            if (jointDriver == null || runtimeRoot == null)
            {
                return;
            }

            var handleHost = runtimeRoot.Find("JointHandles");
            if (handleHost == null)
            {
                var go = new GameObject("JointHandles");
                go.transform.SetParent(runtimeRoot, false);
                handleHost = go.transform;
            }

            jointHandles = new JointRotationHandle[JointCount];
            var jointColors = new[]
            {
                UIDesignTokens.Colors.DiagramLink1,
                UIDesignTokens.Colors.DiagramLink2,
                UIDesignTokens.Colors.DiagramLink3,
                UIDesignTokens.Colors.DiagramLink4,
                UIDesignTokens.Colors.DiagramLink5,
                UIDesignTokens.Colors.DiagramLink6
            };

            for (var i = 0; i < JointCount; i++)
            {
                var jointTransform = jointDriver.GetJointTransform(i);
                if (jointTransform == null)
                {
                    continue;
                }

                var handleName = $"Handle_J{i + 1}";
                var existing = handleHost.Find(handleName);
                GameObject handleGo;

                if (existing != null)
                {
                    handleGo = existing.gameObject;
                }
                else
                {
                    handleGo = new GameObject(handleName);
                    handleGo.transform.SetParent(jointTransform, false);
                    handleGo.transform.localPosition = Vector3.zero;
                }

                jointHandles[i] = handleGo.GetComponent<JointRotationHandle>()
                    ?? handleGo.AddComponent<JointRotationHandle>();

                var axis = jointDriver.GetJointRotationAxis(i);
                jointHandles[i].Initialize(i, axis, jointColors[i]);
                jointHandles[i].OnHandleDragged += OnHandleDragged;
                jointHandles[i].OnDragStateChanged += OnHandleDragStateChanged;
            }
        }

        private void EnsureRobotSelection()
        {
            var selectedRobotId = RobotSelectionBridge.GetSelectedRobotId();
            var selectedMode = RobotSelectionBridge.GetSelectedMode();
            if (selectedRobotId == FairinoRobotId && selectedMode == RobotSelectionBridge.RobotControlMode)
            {
                return;
            }

            RobotSelectionBridge.SetSelection(FairinoRobotId, RobotSelectionBridge.RobotControlMode);
        }

        private void EnsureRuntimeRoot()
        {
            if (runtimeRoot != null)
            {
                return;
            }

            var existing = FindSceneRuntimeRoot();
            if (existing != null)
            {
                runtimeRoot = existing;
                return;
            }

            runtimeRoot = new GameObject(RuntimeRootName).transform;
            runtimeRoot.localPosition = Vector3.zero;
            runtimeRoot.localRotation = Quaternion.identity;
        }

        private void EnsureControlRobot()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            if (controlRobotInstance != null)
            {
                StabilizeControlRobot(controlRobotInstance);
                TeleportRootUpright(controlRobotInstance);
                return;
            }

            var existing = runtimeRoot.Find(ControlRobotInstanceName);
            if (existing != null)
            {
                controlRobotInstance = existing.gameObject;
                StabilizeControlRobot(controlRobotInstance);
                TeleportRootUpright(controlRobotInstance);
                return;
            }

            if (!TryLoadControlPrefab(out var prefab, out var diagnostic, out var meshFilterCount, out var meshRendererCount))
            {
                Debug.LogWarning($"[RobotControlSceneCoordinator] {diagnostic}");
                if (TryLoadShowroomFallback(out prefab, out meshFilterCount, out meshRendererCount))
                {
                    Debug.Log($"[RobotControlSceneCoordinator] Using showroom prefab as visual fallback ({meshFilterCount} meshes).");
                }
                else
                {
                    controlRobotInstance = CreatePlaceholderControlRobot(runtimeRoot);
                    return;
                }
            }

            controlRobotInstance = InstantiateAndSetup(prefab);
            RepairVisualMeshes(controlRobotInstance);
            StabilizeControlRobot(controlRobotInstance);
            TeleportRootUpright(controlRobotInstance);

            if (!HasVisibleBounds(controlRobotInstance))
            {
                Debug.LogWarning("[RobotControlSceneCoordinator] Instantiated prefab has zero render bounds. Trying showroom fallback.");
                Destroy(controlRobotInstance);
                controlRobotInstance = null;
                if (TryLoadShowroomFallback(out var fallback, out meshFilterCount, out meshRendererCount))
                {
                    controlRobotInstance = InstantiateAndSetup(fallback);
                    StabilizeControlRobot(controlRobotInstance);
                    TeleportRootUpright(controlRobotInstance);
                    NormalizeScale(controlRobotInstance, TargetRobotHeight);
                    Debug.Log($"[RobotControlSceneCoordinator] Showroom fallback loaded with {meshFilterCount} meshes.");
                }
                else
                {
                    controlRobotInstance = CreatePlaceholderControlRobot(runtimeRoot);
                }
                return;
            }

            Debug.Log($"[RobotControlSceneCoordinator] Loaded FR5 control prefab with {meshFilterCount} MeshFilter(s) and {meshRendererCount} MeshRenderer(s). No scale applied (ArticulationBody).");
        }

        private static void TeleportRootUpright(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var baseLink = root.transform.Find("base_link");
            if (baseLink == null)
            {
                return;
            }

            var baseBody = baseLink.GetComponent<ArticulationBody>();
            if (baseBody != null)
            {
                baseBody.TeleportRoot(baseLink.position, baseLink.rotation);
            }
        }

        private static Transform FindSceneRuntimeRoot()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == RuntimeRootName)
                {
                    return roots[i].transform;
                }
            }

            return null;
        }

        private static bool TryLoadControlPrefab(out GameObject prefab, out string diagnostic, out int meshFilterCount, out int meshRendererCount)
        {
            prefab = Resources.Load<GameObject>(ControlPrefabResourcePath);
            meshFilterCount = 0;
            meshRendererCount = 0;

            if (prefab == null)
            {
                diagnostic = $"Control prefab missing at Resources/{ControlPrefabResourcePath}. Run QA import first.";
                return false;
            }

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            meshFilterCount = meshFilters.Length;
            meshRendererCount = prefab.GetComponentsInChildren<MeshRenderer>(true).Length;
            if (meshFilterCount <= 0 || meshRendererCount <= 0)
            {
                diagnostic = $"Control prefab at Resources/{ControlPrefabResourcePath} has no visible meshes. Re-run FR5 import/repair.";
                return false;
            }

            var totalVertices = 0;
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var mesh = meshFilters[i] != null ? meshFilters[i].sharedMesh : null;
                if (mesh != null)
                {
                    totalVertices += mesh.vertexCount;
                }
            }

            if (totalVertices <= 0)
            {
                diagnostic = $"Control prefab at Resources/{ControlPrefabResourcePath} has {meshFilterCount} MeshFilter(s) but 0 vertices. Re-run FR5 import/repair.";
                return false;
            }

            diagnostic = $"Loaded control prefab '{prefab.name}'.";
            return true;
        }

        private GameObject InstantiateAndSetup(GameObject prefab)
        {
            var instance = Instantiate(prefab, runtimeRoot);
            instance.name = ControlRobotInstanceName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        private static void NormalizeScale(GameObject root, float targetHeight)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxSize > 0.0001f)
            {
                var scale = targetHeight / maxSize;
                root.transform.localScale *= scale;
            }

            renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                root.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            }
        }

        private static bool HasVisibleBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].bounds.size.sqrMagnitude > 0.0001f)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryLoadShowroomFallback(out GameObject prefab, out int meshFilterCount, out int meshRendererCount)
        {
            prefab = Resources.Load<GameObject>(ShowroomPrefabResourcePath);
            meshFilterCount = 0;
            meshRendererCount = 0;
            if (prefab == null)
            {
                return false;
            }

            meshFilterCount = prefab.GetComponentsInChildren<MeshFilter>(true).Length;
            meshRendererCount = prefab.GetComponentsInChildren<MeshRenderer>(true).Length;
            return meshFilterCount > 0 && meshRendererCount > 0;
        }

        private static GameObject CreatePlaceholderControlRobot(Transform parent)
        {
            var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placeholder.name = ControlRobotInstanceName;
            placeholder.transform.SetParent(parent, false);
            placeholder.transform.localScale = new Vector3(0.3f, 0.12f, 0.3f);
            placeholder.transform.localPosition = Vector3.zero;
            return placeholder;
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

            var meshColliders = controlRoot.GetComponentsInChildren<MeshCollider>(true);
            for (var i = 0; i < meshColliders.Length; i++)
            {
                var meshCollider = meshColliders[i];
                if (meshCollider == null || meshCollider.sharedMesh != null)
                {
                    continue;
                }

                var siblingFilter = meshCollider.GetComponent<MeshFilter>();
                if (siblingFilter == null)
                {
                    continue;
                }

                var mesh = siblingFilter.sharedMesh != null ? siblingFilter.sharedMesh : siblingFilter.mesh;
                if (mesh != null)
                {
                    meshCollider.sharedMesh = mesh;
                }
            }
        }

        private static void StabilizeControlRobot(GameObject controlRoot)
        {
            if (controlRoot == null)
            {
                return;
            }

            var components = controlRoot.GetComponents<MonoBehaviour>();
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

            var baseLink = controlRoot.transform.Find("base_link");
            var baseBody = baseLink != null ? baseLink.GetComponent<ArticulationBody>() : null;
            if (baseBody != null)
            {
                baseBody.immovable = true;
            }
        }

        private void EnsurePresetAnimator()
        {
            presetAnimator = gameObject.GetComponent<PresetTransitionAnimator>()
                ?? gameObject.AddComponent<PresetTransitionAnimator>();
        }

        private double[] GetCurrentAnglesDeg()
        {
            if (kinematicsFacade == null)
            {
                return new double[JointCount];
            }

            var currentRad = kinematicsFacade.JointValuesRad;
            var values = new double[JointCount];
            for (var i = 0; i < JointCount && i < currentRad.Length; i++)
            {
                values[i] = currentRad[i] * (180.0 / System.Math.PI);
            }

            return values;
        }

        private void ApplyJointSnapshot(double[] jointAnglesDeg)
        {
            if (jointDriver != null)
            {
                jointDriver.ApplyJointAngles(jointAnglesDeg);
            }

            jointControlPanel?.SetSliderValues(jointAnglesDeg);
            kinematicsFacade?.SetJointAnglesDegrees(jointAnglesDeg);

            if (frameGizmoFactory != null && frameGizmoFactory.IsVisible && kinematicsFacade != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }

            SyncHandleAngles(jointAnglesDeg);
        }

        private void OnAnimationFrameUpdated(double[] jointAnglesDeg)
        {
            if (jointDriver != null)
            {
                jointDriver.ApplyJointAngles(jointAnglesDeg);
            }

            jointControlPanel?.SetSliderValues(jointAnglesDeg);
            kinematicsFacade?.SetJointAnglesDegrees(jointAnglesDeg);

            if (eeTrailRenderer != null && kinematicsFacade != null)
            {
                eeTrailRenderer.AddPoint(kinematicsFacade.EndEffectorTransform);
            }

            if (displacementArrow != null && kinematicsFacade != null)
            {
                displacementArrow.UpdateFromFK(kinematicsFacade.EndEffectorTransform);
            }

            if (frameGizmoFactory != null && frameGizmoFactory.IsVisible && kinematicsFacade != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }

            SyncHandleAngles(jointAnglesDeg);
        }

        private void OnAnimationComplete(double[] jointAnglesDeg)
        {
            if (whyItMovedLabel != null)
            {
                var syntheticState = new FairinoRobotState(jointAnglesDeg, new double[6]);
                whyItMovedLabel.HandleStateUpdated(syntheticState);
            }
        }

        private static FairinoRobotConfig BuildFallbackConfig()
        {
            return new FairinoRobotConfig
            {
                robotId = FairinoRobotId,
                displayName = "FAIRINO FR5",
                defaultIp = "192.168.58.2",
                defaultPort = 8080,
                dof = 6,
                jointLimits = new[]
                {
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -175d, maxDeg = 175d },
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -265d, maxDeg = 85d },
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -162d, maxDeg = 162d },
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -265d, maxDeg = 85d },
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -175d, maxDeg = 175d },
                    new FairinoRobotConfig.JointLimitEntry { minDeg = -360d, maxDeg = 360d }
                },
                speedPresets = new FairinoRobotConfig.SpeedPresetsBlock
                {
                    slow = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 10, accPercent = 20 },
                    medium = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 30, accPercent = 50 },
                    fast = new FairinoRobotConfig.SpeedPreset { jointSpeedPercent = 60, accPercent = 80 }
                }
            };
        }
    }
}
