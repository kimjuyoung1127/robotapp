// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControl 씬의 초기화를 담당하는 코디네이터입니다.
    /// 연결 서비스, 패널, 설정 로드, FR5 control prefab 복원을 조율합니다.
    /// </summary>
    public class RobotControlSceneCoordinator : MonoBehaviour
    {
        private const string FairinoRobotId = "FAIRINO_FR5";
        private const string RuntimeRootName = "FR5_RuntimeRoot";
        private const string ControlRobotInstanceName = "FR5_UrdfInstance";
        private const string ControlPrefabResourcePath = "Robots/FAIRINO_FR5_Control";

        [SerializeField] private FairinoConnectionPanel connectionPanel;
        [SerializeField] private FairinoJointControlPanel jointControlPanel;
        [SerializeField] private FairinoStatePanel statePanel;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Transform runtimeRoot;
        [SerializeField] private GameObject controlRobotInstance;

        private FairinoConnectionService connectionService;
        private FairinoErrorTranslator errorTranslator;
        private FairinoRobotConfig config;

        private void Awake()
        {
            FairinoRobotControlViewBuilder.EnsureEventSystem();
            canvas = FairinoRobotControlViewBuilder.EnsureCanvas(canvas, fallbackFont);
            FairinoRobotControlViewBuilder.EnsureCamera();
            FairinoRobotControlViewBuilder.EnsureLight();
            FairinoRobotControlViewBuilder.EnsureLayout(canvas, fallbackFont, out connectionPanel, out jointControlPanel, out statePanel);

            errorTranslator = new FairinoErrorTranslator();
            connectionService = new FairinoConnectionService(errorTranslator);
            connectionService.SetMockMode(true);
            config = FairinoRobotConfig.Load() ?? BuildFallbackConfig();

            EnsureRobotSelection();
            EnsureRuntimeRoot();
            EnsureControlRobot();
            InjectDependencies();
        }

        private void Update()
        {
            connectionService?.Tick(Time.deltaTime);
        }

        private void InjectDependencies()
        {
            connectionPanel?.Inject(connectionService, config);
            jointControlPanel?.Inject(connectionService, config);
            statePanel?.Inject(connectionService, errorTranslator);
        }

        /// <summary>
        /// Mock↔Live 모드를 전환합니다.
        /// </summary>
        public void SetMockMode(bool mock)
        {
            connectionService?.SetMockMode(mock);
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
            if (runtimeRoot == null || controlRobotInstance != null)
            {
                return;
            }

            var existing = runtimeRoot.Find(ControlRobotInstanceName);
            if (existing != null)
            {
                controlRobotInstance = existing.gameObject;
                return;
            }

            if (!TryLoadControlPrefab(out var prefab, out var diagnostic, out var meshFilterCount, out var meshRendererCount))
            {
                Debug.LogWarning($"[RobotControlSceneCoordinator] {diagnostic}");
                controlRobotInstance = CreatePlaceholderControlRobot(runtimeRoot);
                return;
            }

            controlRobotInstance = Instantiate(prefab, runtimeRoot);
            controlRobotInstance.name = ControlRobotInstanceName;
            controlRobotInstance.transform.localPosition = Vector3.zero;
            controlRobotInstance.transform.localRotation = Quaternion.identity;
            RepairVisualMeshes(controlRobotInstance);
            StabilizeControlRobot(controlRobotInstance);
            Debug.Log($"[RobotControlSceneCoordinator] Loaded FR5 control prefab with {meshFilterCount} MeshFilter(s) and {meshRendererCount} MeshRenderer(s).");
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

            meshFilterCount = prefab.GetComponentsInChildren<MeshFilter>(true).Length;
            meshRendererCount = prefab.GetComponentsInChildren<MeshRenderer>(true).Length;
            if (meshFilterCount <= 0 || meshRendererCount <= 0)
            {
                diagnostic = $"Control prefab at Resources/{ControlPrefabResourcePath} has no visible meshes. Re-run FR5 import/repair.";
                return false;
            }

            diagnostic = $"Loaded control prefab '{prefab.name}'.";
            return true;
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
            }

            var baseLink = controlRoot.transform.Find("base_link");
            var baseBody = baseLink != null ? baseLink.GetComponent<ArticulationBody>() : null;
            if (baseBody != null)
            {
                baseBody.immovable = true;
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
