// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Templates;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace KineTutor3D.App
{
    /// <summary>
    /// FR5 슬림 템플릿 데모 씬에서 관절 링 조작과 3D 포즈 반영만 담당하는 최소 호스트입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FR5TemplateMinimalController : MonoBehaviour
    {
        private static readonly Color[] JointColors =
        {
            new Color(0.29f, 0.56f, 0.85f, 1f),
            new Color(0.95f, 0.77f, 0.15f, 1f),
            new Color(0.30f, 0.85f, 0.45f, 1f),
            new Color(0.90f, 0.35f, 0.30f, 1f),
            new Color(0.70f, 0.40f, 0.90f, 1f),
            new Color(0.95f, 0.55f, 0.25f, 1f)
        };

        [SerializeField] private string initialPoseName = FR5TemplatePoseCatalog.ReadyName;
        [SerializeField] private bool spawnPreviewReference;
        [SerializeField] private Vector3 previewOffset = new Vector3(1.45f, 0f, -0.35f);

        private Transform runtimeRoot;
        private GameObject controlRobotInstance;
        private GameObject previewRobotInstance;
        private FairinoUrdfJointDriver jointDriver;
        private OrbitCameraController orbitCamera;
        private JointRotationHandle[] jointHandles;
        private RobotKinematicsFacade kinematicsFacade;
        private double[] currentAnglesDeg = new double[6];
        private bool listenersBound;

        /// <summary>
        /// 현재 관절 각도 복사본을 반환합니다.
        /// </summary>
        public double[] GetCurrentJointAnglesDeg()
        {
            return (double[])currentAnglesDeg.Clone();
        }

        /// <summary>
        /// 포즈 이름으로 현재 관절값을 설정합니다.
        /// </summary>
        public bool SetPoseByName(string poseName)
        {
            if (!FR5TemplatePoseCatalog.TryGetPose(poseName, out var jointAnglesDeg))
            {
                return false;
            }

            SetJointAnglesDeg(jointAnglesDeg);
            return true;
        }

        /// <summary>
        /// 관절 각도 배열을 직접 적용합니다.
        /// </summary>
        public void SetJointAnglesDeg(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null || jointAnglesDeg.Length != currentAnglesDeg.Length)
            {
                return;
            }

            Array.Copy(jointAnglesDeg, currentAnglesDeg, currentAnglesDeg.Length);
            jointDriver?.ApplyJointAngles(currentAnglesDeg);
            kinematicsFacade?.SetJointAnglesDegrees(currentAnglesDeg);
            SyncHandleAngles();
        }

        private void Awake()
        {
            EnsurePresentation();
            EnsureRuntime();
            BindListeners();

            if (!SetPoseByName(initialPoseName))
            {
                SetPoseByName(FR5TemplatePoseCatalog.ReadyName);
            }
        }

        private void OnEnable()
        {
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void EnsurePresentation()
        {
            EnsureEventSystem();
            EnsureCamera();
            EnsureLight();
        }

        private void EnsureRuntime()
        {
            kinematicsFacade ??= new RobotKinematicsFacade(TemplateFAIRINO_FR5.Create());
            EnsureRuntimeRoot();
            EnsureFloor();
            EnsureControlRobot();
            EnsureJointDriver();
            EnsurePreviewReference();
            EnsureOrbitCamera();
            EnsureJointHandles();
        }

        private void EnsureRuntimeRoot()
        {
            if (runtimeRoot != null)
            {
                return;
            }

            var existing = transform.Find("FR5TemplateRuntimeRoot");
            if (existing != null)
            {
                runtimeRoot = existing;
                return;
            }

            var go = new GameObject("FR5TemplateRuntimeRoot");
            go.transform.SetParent(transform, false);
            runtimeRoot = go.transform;
        }

        private void EnsureFloor()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            if (runtimeRoot.Find("Floor") != null)
            {
                return;
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(runtimeRoot, false);
            floor.transform.localScale = new Vector3(2.6f, 0.02f, 2.6f);
            floor.transform.localPosition = new Vector3(0f, -0.01f, 0f);

            var renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0.16f, 0.18f, 0.22f, 1f)
                };
                renderer.sharedMaterial = material;
            }
        }

        private void EnsureControlRobot()
        {
            if (controlRobotInstance != null)
            {
                return;
            }

            var existing = runtimeRoot.Find("FR5_ControlInstance");
            if (existing != null)
            {
                controlRobotInstance = existing.gameObject;
                StabilizeControlRobot(controlRobotInstance);
                TeleportRootUpright(controlRobotInstance);
                return;
            }

            var prefab = Resources.Load<GameObject>(FR5TemplateSlimManifest.ControlPrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError("[FR5TemplateMinimalController] Control prefab missing at Resources/Robots/FAIRINO_FR5_Control.");
                return;
            }

            controlRobotInstance = Instantiate(prefab, runtimeRoot);
            controlRobotInstance.name = "FR5_ControlInstance";
            controlRobotInstance.transform.localPosition = Vector3.zero;
            controlRobotInstance.transform.localRotation = Quaternion.identity;
            RepairVisualMeshes(controlRobotInstance);
            StabilizeControlRobot(controlRobotInstance);
            TeleportRootUpright(controlRobotInstance);
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
            if (baseLink == null)
            {
                Debug.LogError("[FR5TemplateMinimalController] base_link를 찾지 못했습니다.");
                return;
            }

            jointDriver.Inject(baseLink);
        }

        private void EnsurePreviewReference()
        {
            if (!spawnPreviewReference || previewRobotInstance != null || runtimeRoot == null)
            {
                return;
            }

            var prefab = Resources.Load<GameObject>(FR5TemplateSlimManifest.PreviewPrefabResourcePath);
            if (prefab == null)
            {
                return;
            }

            previewRobotInstance = Instantiate(prefab, runtimeRoot);
            previewRobotInstance.name = "FR5_PreviewReference";
            previewRobotInstance.transform.localPosition = previewOffset;
            previewRobotInstance.transform.localRotation = Quaternion.identity;
        }

        private void EnsureOrbitCamera()
        {
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            if (camera == null)
            {
                return;
            }

            orbitCamera = camera.GetComponent<OrbitCameraController>()
                ?? camera.gameObject.AddComponent<OrbitCameraController>();

            var target = controlRobotInstance != null
                ? controlRobotInstance.transform.Find("base_link") ?? controlRobotInstance.transform
                : runtimeRoot;

            orbitCamera.SetDistanceLimits(0.6f, 4.5f);
            orbitCamera.SetTarget(target, 2.4f);
        }

        private void EnsureJointHandles()
        {
            if (jointDriver == null || runtimeRoot == null)
            {
                return;
            }

            var host = runtimeRoot.Find("JointHandles");
            if (host == null)
            {
                var go = new GameObject("JointHandles");
                go.transform.SetParent(runtimeRoot, false);
                host = go.transform;
            }

            jointHandles = new JointRotationHandle[6];

            for (var i = 0; i < jointHandles.Length; i++)
            {
                var jointTransform = jointDriver.GetJointTransform(i);
                if (jointTransform == null)
                {
                    continue;
                }

                var handleName = $"Handle_J{i + 1}";
                var existing = host.Find(handleName);
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
                jointHandles[i].Initialize(i, jointDriver.GetJointRotationAxis(i), JointColors[i]);
            }
        }

        private void BindListeners()
        {
            if (listenersBound || jointHandles == null)
            {
                return;
            }

            for (var i = 0; i < jointHandles.Length; i++)
            {
                if (jointHandles[i] == null)
                {
                    continue;
                }

                jointHandles[i].OnHandleDragged += OnHandleDragged;
                jointHandles[i].OnDragStateChanged += OnHandleDragStateChanged;
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound || jointHandles == null)
            {
                return;
            }

            for (var i = 0; i < jointHandles.Length; i++)
            {
                if (jointHandles[i] == null)
                {
                    continue;
                }

                jointHandles[i].OnHandleDragged -= OnHandleDragged;
                jointHandles[i].OnDragStateChanged -= OnHandleDragStateChanged;
            }

            listenersBound = false;
        }

        private void OnHandleDragged(int jointIndex, float angleDeg)
        {
            currentAnglesDeg[jointIndex] = angleDeg;
            SetJointAnglesDeg(currentAnglesDeg);
        }

        private void OnHandleDragStateChanged(int jointIndex, bool isDragging)
        {
            if (orbitCamera != null)
            {
                orbitCamera.OrbitEnabled = !isDragging;
            }
        }

        private void SyncHandleAngles()
        {
            if (jointHandles == null)
            {
                return;
            }

            for (var i = 0; i < jointHandles.Length && i < currentAnglesDeg.Length; i++)
            {
                jointHandles[i]?.SetAngle((float)currentAnglesDeg[i]);
            }
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void EnsureCamera()
        {
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            if (camera != null)
            {
                return;
            }

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(-1.39f, 0.63f, -2.35f);
            cameraGo.transform.rotation = Quaternion.Euler(14f, 31f, 0f);

            var mainCamera = cameraGo.GetComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.11f, 0.12f, 0.16f, 1f);
            mainCamera.nearClipPlane = 0.01f;
            mainCamera.farClipPlane = 50f;
        }

        private void EnsureLight()
        {
            if (UnityEngine.Object.FindFirstObjectByType<Light>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var lightGo = new GameObject("Directional Light", typeof(Light));
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
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

        private static void TeleportRootUpright(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var baseLink = root.transform.Find("base_link");
            var baseBody = baseLink != null ? baseLink.GetComponent<ArticulationBody>() : null;
            if (baseBody != null)
            {
                baseBody.TeleportRoot(baseLink.position, baseLink.rotation);
            }
        }
    }
}
