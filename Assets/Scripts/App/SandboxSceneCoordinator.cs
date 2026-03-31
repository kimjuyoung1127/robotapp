// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.App.Fairino;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KineTutor3D.App
{
    /// <summary>
    /// Sandbox 씬의 초기화와 상호작용을 담당하는 독립 코디네이터입니다.
    /// AppController 의존 없이 로봇 선택, 프리팹 로딩, FK, 슬라이더, 프리셋을 조율합니다.
    /// </summary>
    public class SandboxSceneCoordinator : MonoBehaviour
    {
        private const string DefaultRobotId = "FAIRINO_FR5";
        private const string RuntimeRootName = "SandboxRuntimeRoot";
        private const string ControlRobotInstanceName = "SandboxRobot";

        [SerializeField] private Font fallbackFont;

        private string robotId;
        private RobotControlTemplateDefinition templateDefinition;
        private RobotCatalogEntry catalogEntry;
        private RobotTemplate robotTemplate;
        private RobotKinematicsFacade kinematicsFacade;
        private UrdfJointDriver jointDriver;
        private FrameGizmoFactory frameGizmoFactory;
        private EETrailRenderer eeTrailRenderer;
        private OrbitCameraController orbitCamera;
        private PresetTransitionAnimator presetAnimator;
        private Transform runtimeRoot;
        private GameObject controlRobotInstance;
        private Canvas canvas;
        private SandboxViewRefs viewRefs;
        private double[] currentAnglesDeg;
        private bool listenersBound;

        private void Awake()
        {
            EnsureRobotSelection();
            EnsureRuntimeRoot();
            EnsureControlRobot();
            EnsureJointDriver();
            EnsureKinematics();
            EnsureVisualizationHelpers();
            EnsureOrbitCamera();
            EnsurePresetAnimator();
            EnsureUI();
            BindListeners();

            var readyPose = GetReadyPose();
            ApplyJointSnapshot(readyPose);
        }

        private void OnEnable()
        {
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void EnsureRobotSelection()
        {
            robotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrEmpty(robotId))
            {
                robotId = DefaultRobotId;
                RobotSelectionBridge.SetSelection(robotId, RobotSelectionBridge.SandboxMode);
            }

            templateDefinition = RobotControlFactory.Create(robotId);
            RobotCatalog.TryGet(robotId, out catalogEntry);

            if (catalogEntry != null && catalogEntry.TemplateFactory != null)
            {
                robotTemplate = catalogEntry.TemplateFactory();
            }
        }

        private void EnsureRuntimeRoot()
        {
            if (runtimeRoot != null)
            {
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                var roots = scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    if (roots[i] != null && roots[i].name == RuntimeRootName)
                    {
                        runtimeRoot = roots[i].transform;
                        return;
                    }
                }
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

            var prefabPath = templateDefinition.ControlPrefabResourcePath;
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning("[SandboxSceneCoordinator] ControlPrefabResourcePath is empty.");
                controlRobotInstance = CreatePlaceholder();
                return;
            }

            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[SandboxSceneCoordinator] Failed to load prefab at '{prefabPath}'.");
                controlRobotInstance = CreatePlaceholder();
                return;
            }

            controlRobotInstance = Instantiate(prefab, runtimeRoot);
            controlRobotInstance.name = ControlRobotInstanceName;
            StabilizeControlRobot(controlRobotInstance);
            TeleportRootUpright(controlRobotInstance);

            Debug.Log($"[SandboxSceneCoordinator] Loaded robot prefab '{prefabPath}'.");
        }

        private void EnsureJointDriver()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            jointDriver = controlRobotInstance.GetComponent<UrdfJointDriver>()
                ?? controlRobotInstance.AddComponent<UrdfJointDriver>();

            var driverRoot = ResolveRobotAnchor(controlRobotInstance.transform);
            if (driverRoot == null)
            {
                Debug.LogWarning("[SandboxSceneCoordinator] Failed to resolve a joint-driver root.");
                return;
            }

            jointDriver.Inject(driverRoot, templateDefinition.JointCount);
        }

        private void EnsureKinematics()
        {
            if (templateDefinition.KinematicsFactory == null)
            {
                return;
            }

            kinematicsFacade = templateDefinition.KinematicsFactory();
            currentAnglesDeg = new double[templateDefinition.JointCount];
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
        }

        private void EnsureOrbitCamera()
        {
            FairinoRobotControlViewBuilder.EnsureEventSystem();
            var camera = FairinoRobotControlViewBuilder.EnsureCamera();
            FairinoRobotControlViewBuilder.EnsureLight();

            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null || controlRobotInstance == null)
            {
                return;
            }

            orbitCamera = camera.GetComponent<OrbitCameraController>();
            if (orbitCamera == null)
            {
                orbitCamera = camera.gameObject.AddComponent<OrbitCameraController>();
            }

            orbitCamera.SetTarget(ResolveRobotAnchor(controlRobotInstance.transform) ?? controlRobotInstance.transform);
        }

        private void EnsurePresetAnimator()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            var animHost = runtimeRoot.Find("PresetAnimator");
            if (animHost == null)
            {
                var go = new GameObject("PresetAnimator");
                go.transform.SetParent(runtimeRoot, false);
                animHost = go.transform;
            }

            presetAnimator = animHost.GetComponent<PresetTransitionAnimator>()
                ?? animHost.gameObject.AddComponent<PresetTransitionAnimator>();
        }

        private void EnsureUI()
        {
            canvas = FairinoRobotControlViewBuilder.EnsureCanvas(null, fallbackFont);

            var displayName = catalogEntry != null ? catalogEntry.Metadata.DisplayName : robotId;
            var dof = templateDefinition.JointCount;
            var jointLimits = robotTemplate != null ? robotTemplate.GetJointLimits() : CreateDefaultJointLimits(dof);

            viewRefs = SandboxViewBuilder.Build(
                canvas.transform,
                UiRuntimeStyle.ResolveFont(fallbackFont),
                dof,
                displayName,
                dof,
                jointLimits);
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            if (viewRefs.jointSliders != null)
            {
                for (var i = 0; i < viewRefs.jointSliders.Length; i++)
                {
                    if (viewRefs.jointSliders[i] != null)
                    {
                        var index = i;
                        viewRefs.jointSliders[i].onValueChanged.AddListener(value => OnSliderChanged(index, value));
                    }
                }
            }

            if (viewRefs.gizmoToggle != null)
            {
                viewRefs.gizmoToggle.onValueChanged.AddListener(OnGizmoToggleChanged);
            }

            if (viewRefs.clearTrailButton != null)
            {
                viewRefs.clearTrailButton.onClick.AddListener(OnClearTrailClicked);
            }

            if (viewRefs.backToLibraryButton != null)
            {
                viewRefs.backToLibraryButton.onClick.AddListener(OnBackToLibrary);
            }

            if (viewRefs.zeroPoseButton != null)
            {
                viewRefs.zeroPoseButton.onClick.AddListener(() => OnPresetClicked("Zero"));
            }

            if (viewRefs.homePoseButton != null)
            {
                viewRefs.homePoseButton.onClick.AddListener(() => OnPresetClicked("Home"));
            }

            if (viewRefs.demoPoseButton != null)
            {
                viewRefs.demoPoseButton.onClick.AddListener(() => OnPresetClicked("Demo"));
            }

            if (viewRefs.readyPoseButton != null)
            {
                viewRefs.readyPoseButton.onClick.AddListener(() => OnPresetClicked("Ready"));
            }

            if (viewRefs.changeRobotButton != null)
            {
                viewRefs.changeRobotButton.onClick.AddListener(OnChangeRobot);
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

            if (viewRefs.jointSliders != null)
            {
                for (var i = 0; i < viewRefs.jointSliders.Length; i++)
                {
                    if (viewRefs.jointSliders[i] != null)
                    {
                        viewRefs.jointSliders[i].onValueChanged.RemoveAllListeners();
                    }
                }
            }

            if (viewRefs.gizmoToggle != null)
            {
                viewRefs.gizmoToggle.onValueChanged.RemoveListener(OnGizmoToggleChanged);
            }

            if (viewRefs.clearTrailButton != null)
            {
                viewRefs.clearTrailButton.onClick.RemoveListener(OnClearTrailClicked);
            }

            if (viewRefs.backToLibraryButton != null)
            {
                viewRefs.backToLibraryButton.onClick.RemoveListener(OnBackToLibrary);
            }

            if (viewRefs.zeroPoseButton != null)
            {
                viewRefs.zeroPoseButton.onClick.RemoveAllListeners();
            }

            if (viewRefs.homePoseButton != null)
            {
                viewRefs.homePoseButton.onClick.RemoveAllListeners();
            }

            if (viewRefs.demoPoseButton != null)
            {
                viewRefs.demoPoseButton.onClick.RemoveAllListeners();
            }

            if (viewRefs.readyPoseButton != null)
            {
                viewRefs.readyPoseButton.onClick.RemoveAllListeners();
            }

            if (viewRefs.changeRobotButton != null)
            {
                viewRefs.changeRobotButton.onClick.RemoveAllListeners();
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated -= OnAnimationFrameUpdated;
                presetAnimator.OnTransitionComplete -= OnAnimationComplete;
            }

            listenersBound = false;
        }

        private void OnSliderChanged(int jointIndex, float degrees)
        {
            if (presetAnimator != null && presetAnimator.IsAnimating)
            {
                presetAnimator.Cancel();
            }

            if (currentAnglesDeg == null || jointIndex < 0 || jointIndex >= currentAnglesDeg.Length)
            {
                return;
            }

            currentAnglesDeg[jointIndex] = degrees;
            ApplyJointSnapshot(currentAnglesDeg);
        }

        private void ApplyJointSnapshot(double[] anglesDeg)
        {
            if (anglesDeg == null)
            {
                return;
            }

            if (currentAnglesDeg == null || currentAnglesDeg.Length != anglesDeg.Length)
            {
                currentAnglesDeg = new double[anglesDeg.Length];
            }

            Array.Copy(anglesDeg, currentAnglesDeg, anglesDeg.Length);

            if (jointDriver != null && jointDriver.IsInitialized)
            {
                jointDriver.ApplyJointAngles(anglesDeg);
            }

            kinematicsFacade?.SetJointAnglesDegrees(anglesDeg);

            if (eeTrailRenderer != null && kinematicsFacade != null)
            {
                eeTrailRenderer.AddPoint(kinematicsFacade.EndEffectorTransform);
            }

            if (frameGizmoFactory != null && frameGizmoFactory.IsVisible && kinematicsFacade != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }

            UpdateInfoLabel();
            UpdateSliderValues(anglesDeg);
        }

        private void OnPresetClicked(string presetName)
        {
            var targetPose = ResolvePresetPose(presetName);
            if (targetPose == null)
            {
                return;
            }

            eeTrailRenderer?.Clear();

            if (presetAnimator != null && currentAnglesDeg != null)
            {
                presetAnimator.StartTransition(currentAnglesDeg, targetPose, UIDesignTokens.Anim.PresetTransition);
                return;
            }

            ApplyJointSnapshot(targetPose);
        }

        private void OnAnimationFrameUpdated(double[] interpolatedAngles)
        {
            ApplyJointSnapshot(interpolatedAngles);
        }

        private void OnAnimationComplete(double[] finalAngles)
        {
            ApplyJointSnapshot(finalAngles);
        }

        private void OnBackToLibrary()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        private void OnChangeRobot()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        private void OnGizmoToggleChanged(bool on)
        {
            frameGizmoFactory?.SetVisible(on);
            if (on && kinematicsFacade != null && frameGizmoFactory != null)
            {
                frameGizmoFactory.ApplyFrames(kinematicsFacade.CumulativeTransforms);
            }
        }

        private void OnClearTrailClicked()
        {
            eeTrailRenderer?.Clear();
        }

        private double[] ResolvePresetPose(string presetName)
        {
            var dof = templateDefinition.JointCount;

            switch (presetName)
            {
                case "Zero":
                    if (catalogEntry != null && catalogEntry.Metadata.ZeroPoseDeg != null && catalogEntry.Metadata.ZeroPoseDeg.Length > 0)
                    {
                        return (double[])catalogEntry.Metadata.ZeroPoseDeg.Clone();
                    }
                    return new double[dof];

                case "Home":
                    if (catalogEntry != null && catalogEntry.Metadata.HomePoseDeg != null && catalogEntry.Metadata.HomePoseDeg.Length > 0)
                    {
                        return (double[])catalogEntry.Metadata.HomePoseDeg.Clone();
                    }
                    return new double[dof];

                case "Demo":
                    if (catalogEntry != null && catalogEntry.Metadata.DemoPoseDeg != null && catalogEntry.Metadata.DemoPoseDeg.Length > 0)
                    {
                        return (double[])catalogEntry.Metadata.DemoPoseDeg.Clone();
                    }
                    return new double[dof];

                case "Ready":
                    return GetReadyPose();

                default:
                    return new double[dof];
            }
        }

        private double[] GetReadyPose()
        {
            if (templateDefinition.PosePresetProvider != null)
            {
                return templateDefinition.PosePresetProvider.GetReadyJointAnglesDeg();
            }

            if (catalogEntry != null && catalogEntry.Metadata.HomePoseDeg != null && catalogEntry.Metadata.HomePoseDeg.Length > 0)
            {
                return (double[])catalogEntry.Metadata.HomePoseDeg.Clone();
            }

            return new double[templateDefinition.JointCount];
        }

        private void UpdateInfoLabel()
        {
            if (viewRefs.infoLabel == null || kinematicsFacade == null)
            {
                return;
            }

            var eePos = kinematicsFacade.EndEffectorTransform.ExtractPosition();
            viewRefs.infoLabel.text = $"X: {eePos.X * 1000.0:F1}  Y: {eePos.Y * 1000.0:F1}  Z: {eePos.Z * 1000.0:F1} mm";
        }

        private void UpdateSliderValues(double[] anglesDeg)
        {
            if (viewRefs.jointSliders == null || anglesDeg == null)
            {
                return;
            }

            for (var i = 0; i < viewRefs.jointSliders.Length && i < anglesDeg.Length; i++)
            {
                if (viewRefs.jointSliders[i] != null)
                {
                    viewRefs.jointSliders[i].SetValueWithoutNotify((float)anglesDeg[i]);
                }
            }
        }

        private static void StabilizeControlRobot(GameObject robot)
        {
            if (robot == null)
            {
                return;
            }

            var controllers = robot.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] != null && controllers[i].GetType().FullName == "Unity.Robotics.UrdfImporter.Control.Controller")
                {
                    controllers[i].enabled = false;
                }
            }

            var bodies = robot.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                var body = bodies[i];
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

            var rigidbodies = robot.GetComponentsInChildren<Rigidbody>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                if (rigidbodies[i] != null)
                {
                    rigidbodies[i].isKinematic = true;
                    rigidbodies[i].useGravity = false;
                    rigidbodies[i].linearVelocity = Vector3.zero;
                    rigidbodies[i].angularVelocity = Vector3.zero;
                }
            }

            var rootBody = FindRootArticulationBody(robot);
            if (rootBody != null)
            {
                rootBody.immovable = true;
            }
        }

        private static void TeleportRootUpright(GameObject robot)
        {
            var rootBody = FindRootArticulationBody(robot);
            if (rootBody != null)
            {
                rootBody.TeleportRoot(rootBody.transform.position, rootBody.transform.rotation);
            }
        }

        private static ArticulationBody FindRootArticulationBody(GameObject robot)
        {
            if (robot == null)
            {
                return null;
            }

            var bodies = robot.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null && bodies[i].isRoot)
                {
                    return bodies[i];
                }
            }

            return null;
        }

        private static Transform ResolveRobotAnchor(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            return FindChildRecursive(root, "base_link") ?? root;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            var direct = parent.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private GameObject CreatePlaceholder()
        {
            var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = ControlRobotInstanceName;
            placeholder.transform.SetParent(runtimeRoot, false);
            placeholder.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            placeholder.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var renderer = placeholder.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = UIDesignTokens.Colors.AccentDanger;
            }

            return placeholder;
        }

        private static JointLimit[] CreateDefaultJointLimits(int dof)
        {
            var limits = new JointLimit[dof];
            for (var i = 0; i < dof; i++)
            {
                limits[i] = new JointLimit(-System.Math.PI, System.Math.PI);
            }

            return limits;
        }
    }
}
