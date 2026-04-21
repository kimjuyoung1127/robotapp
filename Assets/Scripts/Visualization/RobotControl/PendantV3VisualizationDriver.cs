// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// Pendant V3 visualization state를 실제 donor/ghost/marker/trail primitive에 반영합니다.
    /// </summary>
    [DefaultExecutionOrder(-910)]
    [RequireComponent(typeof(PendantV3VisualizationOrchestrator))]
    public sealed class PendantV3VisualizationDriver : MonoBehaviour
    {
        private const int ViewportLayer = 23;

        [SerializeField] private PendantV3VisualizationOrchestrator orchestrator;

        private Transform runtimeRoot;
        private GameObject actualRobotInstance;
        private GameObject ghostRobotInstance;
        private FairinoUrdfJointDriver actualJointDriver;
        private FairinoUrdfJointDriver ghostJointDriver;
        private RobotKinematicsFacade actualKinematics;
        private RobotKinematicsFacade ghostKinematics;
        private FrameGizmoFactory frameGizmoFactory;
        private EETrailRenderer eeTrailRenderer;
        private DisplacementArrow displacementArrow;
        private TargetMarkerVisual targetMarkerVisual;
        private LinkHighlighter linkHighlighter;
        private OrbitCameraController orbitCamera;
        private Camera viewportCamera;
        private UIDocument document;
        private VisualElement viewportHostElement;
        private VisualElement viewportToolbarHostElement;
        private VisualElement viewportRenderSurfaceElement;
        private RenderTexture viewportTexture;
        private Rect lastViewportRect = new Rect(0f, 0f, 1f, 1f);
        private Rect lastViewportHostBounds;
        private bool cameraFramed;
        private bool cameraProfileApplied;
        private bool isInitialized;

        private void OnEnable()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            ReleaseViewportTexture();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            var actualVisible = actualRobotInstance != null && actualRobotInstance.activeInHierarchy;
            var ghostVisible = ghostRobotInstance != null && ghostRobotInstance.activeInHierarchy;
            var targetName = orbitCamera != null && orbitCamera.Target != null ? orbitCamera.Target.name : "none";
            var cameraPosition = viewportCamera != null ? viewportCamera.transform.position.ToString("F2") : "none";
            var toolbarBounds = TryGetToolbarBounds(out var bounds) ? bounds.ToString() : "none";
            var pivotOffset = orbitCamera != null ? orbitCamera.PivotOffset.ToString("F2") : "none";
            var textureSize = viewportTexture != null ? $"{viewportTexture.width}x{viewportTexture.height}" : "none";
            return $"initialized={isInitialized}; runtimeRoot={(runtimeRoot != null)}; actual={(actualRobotInstance != null)}; actualVisible={actualVisible}; ghost={(ghostRobotInstance != null)}; ghostVisible={ghostVisible}; trail={(eeTrailRenderer != null)}; marker={(targetMarkerVisual != null)}; cameraReady={(viewportCamera != null)}; cameraTarget={targetName}; cameraPos={cameraPosition}; cameraRect={lastViewportRect}; hostBounds={lastViewportHostBounds}; toolbarBounds={toolbarBounds}; pivotOffset={pivotOffset}; texture={textureSize}; cameraFramed={cameraFramed}";
        }

        private bool TryInitialize()
        {
            orchestrator ??= GetComponent<PendantV3VisualizationOrchestrator>();
            if (orchestrator == null)
            {
                return false;
            }

            EnsureRuntimeRoot();
            EnsureRobotInstances();
            EnsureHelpers();
            CacheViewportHost();
            EnsureViewportCameraFraming();
            orchestrator.StateChanged -= Apply;
            orchestrator.StateChanged += Apply;
            isInitialized = true;
            Apply(orchestrator.CurrentState);
            return true;
        }

        private void EnsureRuntimeRoot()
        {
            runtimeRoot ??= GameObject.Find("PendantV3RuntimeRoot")?.transform;
            runtimeRoot ??= transform.Find("PendantV3RuntimeRoot");
            if (runtimeRoot == null)
            {
                var go = new GameObject("PendantV3RuntimeRoot");
                runtimeRoot = go.transform;
            }

            if (runtimeRoot.parent != null)
            {
                runtimeRoot.SetParent(null, true);
            }

            runtimeRoot.position = Vector3.zero;
            runtimeRoot.rotation = Quaternion.identity;
            runtimeRoot.localScale = Vector3.one;
            SetLayerRecursive(runtimeRoot.gameObject, ViewportLayer);
        }

        private void EnsureRobotInstances()
        {
            if (actualRobotInstance == null)
            {
                actualRobotInstance = EnsureRobotInstance("RobotActual", false);
                actualJointDriver = actualRobotInstance != null ? actualRobotInstance.GetComponent<FairinoUrdfJointDriver>() : null;
                linkHighlighter = actualRobotInstance != null ? actualRobotInstance.GetComponent<LinkHighlighter>() : null;
                actualKinematics ??= new RobotKinematicsFacade(KineTutor3D.Templates.TemplateFAIRINO_FR5.Create());
            }

            if (ghostRobotInstance == null)
            {
                ghostRobotInstance = EnsureRobotInstance("RobotGhost", true);
                ghostJointDriver = ghostRobotInstance != null ? ghostRobotInstance.GetComponent<FairinoUrdfJointDriver>() : null;
                ghostKinematics ??= new RobotKinematicsFacade(KineTutor3D.Templates.TemplateFAIRINO_FR5.Create());
            }
        }

        private GameObject EnsureRobotInstance(string objectName, bool ghost)
        {
            var existing = runtimeRoot.Find(objectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var prefab = Resources.Load<GameObject>("Robots/FAIRINO_FR5_Control");
            if (prefab == null)
            {
                return null;
            }

            var instance = Instantiate(prefab, runtimeRoot);
            instance.name = objectName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            var jointDriver = instance.GetComponent<FairinoUrdfJointDriver>() ?? instance.AddComponent<FairinoUrdfJointDriver>();
            var baseLink = FindBaseLink(instance.transform);
            if (baseLink != null)
            {
                jointDriver.Inject(baseLink);
            }

            var highlighter = instance.GetComponent<LinkHighlighter>() ?? instance.AddComponent<LinkHighlighter>();
            if (baseLink != null)
            {
                highlighter.Configure(
                    jointDriver.GetJointTransform(0),
                    jointDriver.GetJointTransform(1),
                    jointDriver.GetJointTransform(2),
                    jointDriver.GetJointTransform(5));
            }

            if (ghost)
            {
                ApplyGhostLook(instance);
            }

            return instance;
        }

        private void EnsureHelpers()
        {
            frameGizmoFactory ??= (runtimeRoot.Find("FrameGizmos")?.GetComponent<FrameGizmoFactory>());
            if (frameGizmoFactory == null)
            {
                var go = runtimeRoot.Find("FrameGizmos")?.gameObject ?? new GameObject("FrameGizmos");
                go.transform.SetParent(runtimeRoot, false);
                frameGizmoFactory = go.GetComponent<FrameGizmoFactory>() ?? go.AddComponent<FrameGizmoFactory>();
            }

            eeTrailRenderer ??= (runtimeRoot.Find("EETrail")?.GetComponent<EETrailRenderer>());
            if (eeTrailRenderer == null)
            {
                var go = runtimeRoot.Find("EETrail")?.gameObject ?? new GameObject("EETrail");
                go.transform.SetParent(runtimeRoot, false);
                eeTrailRenderer = go.GetComponent<EETrailRenderer>() ?? go.AddComponent<EETrailRenderer>();
            }

            displacementArrow ??= (runtimeRoot.Find("DisplacementArrow")?.GetComponent<DisplacementArrow>());
            if (displacementArrow == null)
            {
                var go = runtimeRoot.Find("DisplacementArrow")?.gameObject ?? new GameObject("DisplacementArrow");
                go.transform.SetParent(runtimeRoot, false);
                displacementArrow = go.GetComponent<DisplacementArrow>() ?? go.AddComponent<DisplacementArrow>();
            }

            targetMarkerVisual ??= runtimeRoot.GetComponentInChildren<TargetMarkerVisual>(true);
            if (targetMarkerVisual == null)
            {
                var go = new GameObject("TargetMarker", typeof(TargetMarkerVisual));
                go.transform.SetParent(runtimeRoot, false);
                targetMarkerVisual = go.GetComponent<TargetMarkerVisual>();
            }

            EnsureViewportCamera();
            SetLayerRecursive(runtimeRoot.gameObject, ViewportLayer);
        }

        private void CacheViewportHost()
        {
            document ??= GetComponent<UIDocument>();
            var root = document?.rootVisualElement;
            if (root == null)
            {
                viewportHostElement = null;
                viewportToolbarHostElement = null;
                return;
            }

            if (viewportHostElement == null || viewportHostElement.panel == null)
            {
                viewportHostElement = root.Q<VisualElement>("ViewportHost");
            }

            if (viewportToolbarHostElement == null || viewportToolbarHostElement.panel == null)
            {
                viewportToolbarHostElement = root.Q<VisualElement>("ViewportToolbarHost");
            }

            EnsureViewportRenderSurface();
        }

        private void Apply(PendantV3VisualizationState state)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            EnsureViewportCameraFraming();
            ApplyRobotPose(actualJointDriver, actualKinematics, state.CurrentJointAnglesDeg);
            ApplyRobotPose(ghostJointDriver, ghostKinematics, state.GhostJointAnglesDeg);
            if (actualRobotInstance != null)
            {
                actualRobotInstance.SetActive(true);
            }
            ApplyGhostVisibility(state.ShowGhost && state.CurrentPreviewKind is PendantV3VisualizationState.PreviewKind.JointGhost or PendantV3VisualizationState.PreviewKind.MoveJGhost);
            ApplyJointHighlight(state.ActiveJointIndex);
            ApplyFrames(state);
            ApplyTrailAndDisplacement(state);
            ApplyTargetMarker(state);
        }

        private void ApplyRobotPose(FairinoUrdfJointDriver driver, RobotKinematicsFacade facade, double[] jointAnglesDeg)
        {
            if (driver == null || facade == null || jointAnglesDeg == null || jointAnglesDeg.Length < 6)
            {
                return;
            }

            driver.ApplyJointAngles(jointAnglesDeg);
            facade.SetJointAnglesDegrees(jointAnglesDeg);
        }

        private void ApplyGhostVisibility(bool visible)
        {
            if (ghostRobotInstance != null)
            {
                ghostRobotInstance.SetActive(visible);
            }
        }

        private void ApplyJointHighlight(int activeJointIndex)
        {
            if (linkHighlighter == null)
            {
                return;
            }

            if (activeJointIndex < 0)
            {
                linkHighlighter.ClearHighlight();
                return;
            }

            var mappedIndex = Mathf.Clamp(activeJointIndex, 0, 3);
            linkHighlighter.HighlightJoint(mappedIndex);
        }

        private void ApplyFrames(PendantV3VisualizationState state)
        {
            if (frameGizmoFactory == null)
            {
                return;
            }

            frameGizmoFactory.SetVisible(state.ShowBaseFrame || state.ShowToolFrame);
            if (!frameGizmoFactory.IsVisible || actualKinematics == null)
            {
                return;
            }

            frameGizmoFactory.ApplyFrames(actualKinematics.CumulativeTransforms);
        }

        private void ApplyTrailAndDisplacement(PendantV3VisualizationState state)
        {
            if (eeTrailRenderer != null)
            {
                eeTrailRenderer.SetVisible(state.ShowTrail);
                if (state.ShowTrail && actualKinematics != null)
                {
                    eeTrailRenderer.AddPoint(actualKinematics.EndEffectorTransform);
                }
                else if (!state.ShowTrail)
                {
                    eeTrailRenderer.Clear();
                }
            }

            if (displacementArrow == null)
            {
                return;
            }

            displacementArrow.SetVisible(state.CurrentPreviewKind != PendantV3VisualizationState.PreviewKind.None);
            if (state.CurrentPreviewKind is PendantV3VisualizationState.PreviewKind.TcpTarget or PendantV3VisualizationState.PreviewKind.MoveLTarget)
            {
                displacementArrow.Clear();
                displacementArrow.UpdateWorldPosition(ToUnityPosition(state.TargetTcpPose));
                return;
            }

            if (ghostKinematics != null && state.CurrentPreviewKind is PendantV3VisualizationState.PreviewKind.JointGhost or PendantV3VisualizationState.PreviewKind.MoveJGhost)
            {
                displacementArrow.UpdateFromFK(ghostKinematics.EndEffectorTransform);
                return;
            }

            displacementArrow.Clear();
        }

        private void ApplyTargetMarker(PendantV3VisualizationState state)
        {
            if (targetMarkerVisual == null)
            {
                return;
            }

            var showMarker = state.CurrentPreviewKind != PendantV3VisualizationState.PreviewKind.None;
            targetMarkerVisual.SetMarkersVisible(showMarker);
            if (!showMarker)
            {
                return;
            }

            if (state.ShowCollision)
            {
                targetMarkerVisual.ShowWarning();
            }
            else
            {
                targetMarkerVisual.ClearFeedback();
            }

            if (state.CurrentPreviewKind is PendantV3VisualizationState.PreviewKind.TcpTarget or PendantV3VisualizationState.PreviewKind.MoveLTarget)
            {
                targetMarkerVisual.transform.position = ToUnityPosition(state.TargetTcpPose);
            }
            else if (ghostKinematics != null)
            {
                targetMarkerVisual.transform.position = CoordConverter.ToUnityPosition(ghostKinematics.EndEffectorTransform.ExtractPosition());
            }
        }

        private static Transform FindBaseLink(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == "base_link")
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var child = FindBaseLink(root.GetChild(index));
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static void ApplyGhostLook(GameObject instance)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                var materials = renderer.materials;
                for (var index = 0; index < materials.Length; index++)
                {
                    var material = materials[index];
                    if (material == null)
                    {
                        continue;
                    }

                    material = new Material(material);
                    material.color = new Color(0.33f, 0.75f, 1f, 0.28f);
                    material.SetFloat("_Surface", 1f);
                    material.SetFloat("_Blend", 0f);
                    material.SetFloat("_ZWrite", 0f);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.renderQueue = 3000;
                    materials[index] = material;
                }

                renderer.materials = materials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static Vector3 ToUnityPosition(double[] tcpPose)
        {
            if (tcpPose == null || tcpPose.Length < 3)
            {
                return Vector3.zero;
            }

            return CoordConverter.ToUnityPosition(new Vec3D(tcpPose[0], tcpPose[1], tcpPose[2]));
        }

        private void EnsureViewportCameraFraming()
        {
            EnsureViewportCamera();
            if (viewportCamera == null)
            {
                return;
            }

            if (!cameraProfileApplied)
            {
                SceneCameraDirector.ConfigureForScene(SceneId.RobotControlV3, viewportCamera);
                var sceneCamera = Camera.main ?? Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
                if (sceneCamera != null)
                {
                    sceneCamera.cullingMask &= ~(1 << ViewportLayer);
                }
                cameraProfileApplied = true;
            }

            orbitCamera = viewportCamera.GetComponent<OrbitCameraController>();
            if (orbitCamera == null)
            {
                orbitCamera = viewportCamera.gameObject.AddComponent<OrbitCameraController>();
            }

            var target = ResolveCameraTarget();
            if (target == null)
            {
                return;
            }

            UpdateViewportRenderTarget();

            orbitCamera.SetDistanceLimits(0.6f, 4.5f);
            var initialDistance = ComputeInitialCameraDistance();
            if (!cameraFramed || orbitCamera.Target == null)
            {
                orbitCamera.SetTarget(target, initialDistance);
                orbitCamera.SetPivotOffset(ComputeCameraPivotOffset(target));
                cameraFramed = true;
                return;
            }

            if (orbitCamera.Target != target)
            {
                orbitCamera.SetTarget(target);
            }

            orbitCamera.SetPivotOffset(ComputeCameraPivotOffset(target));
        }

        private Transform ResolveCameraTarget()
        {
            if (actualRobotInstance == null)
            {
                return null;
            }

            var baseLink = FindBaseLink(actualRobotInstance.transform);
            return baseLink != null ? baseLink : actualRobotInstance.transform;
        }

        private void UpdateCameraViewportRect()
        {
            CacheViewportHost();
            if (viewportHostElement == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var bounds = viewportHostElement.worldBound;
            if (!IsFinite(bounds.xMin)
                || !IsFinite(bounds.yMin)
                || !IsFinite(bounds.width)
                || !IsFinite(bounds.height)
                || bounds.width <= 1f
                || bounds.height <= 1f)
            {
                return;
            }

            lastViewportHostBounds = new Rect(bounds.xMin, bounds.yMin, bounds.width, bounds.height);
            lastViewportRect = new Rect(0f, 0f, 1f, 1f);
        }

        private void UpdateViewportRenderTarget()
        {
            UpdateCameraViewportRect();
            if (viewportCamera == null || viewportHostElement == null)
            {
                return;
            }

            var bounds = lastViewportHostBounds;
            if (bounds.width <= 1f || bounds.height <= 1f)
            {
                return;
            }

            var width = Mathf.Max(512, Mathf.RoundToInt(bounds.width));
            var height = Mathf.Max(512, Mathf.RoundToInt(bounds.height));
            if (viewportTexture == null || viewportTexture.width != width || viewportTexture.height != height)
            {
                ReleaseViewportTexture();
                viewportTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
                {
                    name = "PendantV3ViewportRT"
                };
                viewportTexture.Create();
                viewportCamera.targetTexture = viewportTexture;

                // RobotStageRenderSurface owns the active UI Toolkit render surface in V3.
                // This legacy driver keeps the camera target texture only.
            }

            viewportCamera.aspect = (float)width / height;
            viewportCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private float ComputeInitialCameraDistance()
        {
            if (!TryGetToolbarBounds(out var toolbarBounds))
            {
                return 2.4f;
            }

            var viewportBounds = lastViewportHostBounds;
            if (viewportBounds.width <= 1f || viewportBounds.height <= 1f)
            {
                return 2.4f;
            }

            var occupiedWidth = Mathf.Clamp01((toolbarBounds.xMax - viewportBounds.xMin) / viewportBounds.width);
            var occupiedHeight = Mathf.Clamp01((toolbarBounds.yMax - viewportBounds.yMin) / viewportBounds.height);
            return Mathf.Clamp(3.4f + (occupiedWidth * 2.2f) + (occupiedHeight * 0.7f), 3.4f, 5.0f);
        }

        private Vector3 ComputeCameraPivotOffset(Transform target)
        {
            var offset = Vector3.zero;
            if (target != null && TryGetRobotBounds(actualRobotInstance, out var bounds))
            {
                offset += bounds.center - target.position;
            }

            offset += ComputeToolbarSafeAreaOffset();
            return offset;
        }

        private Vector3 ComputeToolbarSafeAreaOffset()
        {
            if (viewportCamera == null || !TryGetToolbarBounds(out var toolbarBounds))
            {
                return Vector3.zero;
            }

            var viewportBounds = lastViewportHostBounds;
            if (viewportBounds.width <= 1f || viewportBounds.height <= 1f)
            {
                return Vector3.zero;
            }

            var occupiedWidth = Mathf.Clamp01((toolbarBounds.xMax - viewportBounds.xMin) / viewportBounds.width);
            var occupiedHeight = Mathf.Clamp01((toolbarBounds.yMax - viewportBounds.yMin) / viewportBounds.height);
            if (occupiedWidth <= 0.01f && occupiedHeight <= 0.01f)
            {
                return Vector3.zero;
            }

            var distance = orbitCamera != null ? orbitCamera.Distance : 2.4f;
            var aspect = lastViewportRect.height > 0.01f
                ? lastViewportRect.width / lastViewportRect.height
                : Mathf.Max(0.45f, viewportBounds.width / Mathf.Max(1f, viewportBounds.height));
            var visibleHeight = 2f * distance * Mathf.Tan(viewportCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var visibleWidth = visibleHeight * aspect;
            var rightBias = Mathf.Clamp(occupiedWidth * 1.20f, 0f, 0.82f);
            var downBias = Mathf.Clamp(occupiedHeight * 0.40f, 0f, 0.32f);

            return (viewportCamera.transform.right * (visibleWidth * rightBias))
                - (viewportCamera.transform.up * (visibleHeight * downBias));
        }

        private bool TryGetToolbarBounds(out Rect bounds)
        {
            bounds = default;
            CacheViewportHost();
            if (viewportToolbarHostElement == null)
            {
                return false;
            }

            var worldBound = viewportToolbarHostElement.worldBound;
            if (!IsFinite(worldBound.xMin)
                || !IsFinite(worldBound.yMin)
                || !IsFinite(worldBound.width)
                || !IsFinite(worldBound.height)
                || worldBound.width <= 1f
                || worldBound.height <= 1f)
            {
                return false;
            }

            bounds = new Rect(worldBound.xMin, worldBound.yMin, worldBound.width, worldBound.height);
            return true;
        }

        private static bool TryGetRobotBounds(GameObject robot, out Bounds bounds)
        {
            bounds = default;
            if (robot == null)
            {
                return false;
            }

            var renderers = robot.GetComponentsInChildren<Renderer>(true);
            var found = false;
            foreach (var renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return found;
        }

        private void EnsureViewportCamera()
        {
            var existing = runtimeRoot != null ? runtimeRoot.Find("ViewportCamera") : null;
            var cameraGo = existing != null ? existing.gameObject : new GameObject("ViewportCamera");
            if (existing == null)
            {
                cameraGo.transform.SetParent(runtimeRoot, false);
            }

            viewportCamera = cameraGo.GetComponent<Camera>();
            if (viewportCamera == null)
            {
                viewportCamera = cameraGo.AddComponent<Camera>();
            }

            viewportCamera.clearFlags = CameraClearFlags.SolidColor;
            viewportCamera.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);
            viewportCamera.nearClipPlane = 0.01f;
            viewportCamera.farClipPlane = 30f;
            viewportCamera.allowHDR = false;
            viewportCamera.allowMSAA = true;
            viewportCamera.cullingMask = 1 << ViewportLayer;
            viewportCamera.depth = 10f;
            viewportCamera.enabled = true;
        }

        private void EnsureViewportRenderSurface()
        {
            if (viewportHostElement == null)
            {
                viewportRenderSurfaceElement = null;
                return;
            }

            if (viewportRenderSurfaceElement == null || viewportRenderSurfaceElement.panel == null)
            {
                viewportRenderSurfaceElement = viewportHostElement.Q<VisualElement>("ViewportRenderSurface");
            }

            if (viewportRenderSurfaceElement != null)
            {
                return;
            }

            viewportRenderSurfaceElement = new VisualElement
            {
                name = "ViewportRenderSurface",
                pickingMode = PickingMode.Ignore
            };
            viewportRenderSurfaceElement.style.position = Position.Absolute;
            viewportRenderSurfaceElement.style.left = 0f;
            viewportRenderSurfaceElement.style.top = 0f;
            viewportRenderSurfaceElement.style.right = 0f;
            viewportRenderSurfaceElement.style.bottom = 0f;
            viewportRenderSurfaceElement.style.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);
            viewportHostElement.Insert(0, viewportRenderSurfaceElement);
        }

        private void ReleaseViewportTexture()
        {
            if (viewportCamera != null)
            {
                viewportCamera.targetTexture = null;
            }

            if (viewportTexture != null)
            {
                viewportTexture.Release();
                Destroy(viewportTexture);
                viewportTexture = null;
            }
        }

        private static void SetLayerRecursive(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            root.layer = layer;
            var transform = root.transform;
            for (var index = 0; index < transform.childCount; index++)
            {
                SetLayerRecursive(transform.GetChild(index).gameObject, layer);
            }
        }
    }
}
