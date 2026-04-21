// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// WorkPanel 안 RobotStageHost를 RenderTexture 표시면으로 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(RobotControlV3RuntimeController))]
    public sealed class RobotStageRenderSurface : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private RobotControlV3RuntimeController runtimeController;

        private VisualElement root;
        private VisualElement robotStageHost;
        private Image renderSurfaceElement;
        private Label diagnosticLabel;
        private RenderTexture renderTexture;
        private int initializeAttemptCount;
        private int ensureAttemptCount;
        private float lastHostWidth;
        private float lastHostHeight;
        private string lastCameraName = "null";
        private string lastError = string.Empty;
        private bool initialized;

        private void OnEnable()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            if (robotStageHost != null)
            {
                robotStageHost.UnregisterCallback<GeometryChangedEvent>(OnStageGeometryChanged);
            }

            if (renderSurfaceElement != null)
            {
                renderSurfaceElement.UnregisterCallback<PointerDownEvent>(OnRenderSurfacePointerDown);
            }

            ReleaseRenderTexture();
            initialized = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            var camera = ResolveStageCamera();
            var cameraName = camera != null ? camera.name : "null";
            return $"initialized={initialized}; stage={(robotStageHost != null)}; surface={(renderSurfaceElement != null)}; diag={(diagnosticLabel != null)}; initAttempts={initializeAttemptCount}; ensureAttempts={ensureAttemptCount}; host={lastHostWidth:0.#}x{lastHostHeight:0.#}; camera={cameraName}; lastCamera={lastCameraName}; rt={(renderTexture != null ? $"{renderTexture.width}x{renderTexture.height}" : "null")}; error={lastError}";
        }

        private bool TryInitialize()
        {
            initializeAttemptCount++;
            document ??= GetComponent<UIDocument>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            if (document == null || runtimeController == null)
            {
                lastError = "document-or-runtime-missing";
                return false;
            }

            root = document.rootVisualElement;
            robotStageHost = root?.Q<VisualElement>("RobotStageHost");
            if (robotStageHost == null)
            {
                lastError = "robot-stage-host-missing";
                initialized = false;
                return false;
            }

            EnsureRenderSurfaceElement();
            EnsureDiagnosticLabel();
            robotStageHost.UnregisterCallback<GeometryChangedEvent>(OnStageGeometryChanged);
            robotStageHost.RegisterCallback<GeometryChangedEvent>(OnStageGeometryChanged);
            renderSurfaceElement?.UnregisterCallback<PointerDownEvent>(OnRenderSurfacePointerDown);
            renderSurfaceElement?.RegisterCallback<PointerDownEvent>(OnRenderSurfacePointerDown);
            var camera = ResolveStageCamera();
            if (camera != null)
            {
                EnsureRenderTexture(camera);
                initialized = renderTexture != null;
                UpdateDiagnosticLabel();
                return true;
            }

            lastCameraName = "null";
            lastError = "camera-missing";
            ReleaseRenderTexture();
            initialized = false;
            UpdateDiagnosticLabel();
            return false;
        }

        private void OnStageGeometryChanged(GeometryChangedEvent _)
        {
            var camera = ResolveStageCamera();
            if (camera != null)
            {
                EnsureRenderTexture(camera);
            }
        }

        private void EnsureRenderTexture(Camera camera)
        {
            ensureAttemptCount++;
            if (robotStageHost == null || camera == null)
            {
                lastError = "host-or-camera-null";
                UpdateDiagnosticLabel();
                return;
            }

            lastCameraName = camera.name;
            lastHostWidth = robotStageHost.resolvedStyle.width;
            lastHostHeight = robotStageHost.resolvedStyle.height;
            var width = Mathf.Max(512, Mathf.RoundToInt(lastHostWidth));
            var height = Mathf.Max(320, Mathf.RoundToInt(lastHostHeight));
            if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
            {
                renderSurfaceElement.image = renderTexture;
                lastError = string.Empty;
                UpdateDiagnosticLabel();
                return;
            }

            ReleaseRenderTexture();
            camera.aspect = (float)width / height;
            runtimeController?.ResetStageCamera();
            renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                name = "RobotControlV3StageRT"
            };
            renderTexture.Create();
            runtimeController?.SetStageTargetTexture(renderTexture);
            if (camera.targetTexture != renderTexture)
            {
                camera.targetTexture = renderTexture;
            }
            renderSurfaceElement.image = renderTexture;
            renderSurfaceElement.scaleMode = ScaleMode.StretchToFill;
            lastError = renderTexture.IsCreated() ? string.Empty : "rendertexture-create-failed";
            UpdateDiagnosticLabel();
        }

        private void ReleaseRenderTexture()
        {
            var camera = ResolveStageCamera();
            if (camera != null && camera.targetTexture == renderTexture)
            {
                camera.targetTexture = null;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                }

                renderTexture = null;
            }

            if (renderSurfaceElement != null)
            {
                renderSurfaceElement.image = null;
            }

            UpdateDiagnosticLabel();
        }

        private void EnsureRenderSurfaceElement()
        {
            if (robotStageHost == null)
            {
                return;
            }

            renderSurfaceElement = robotStageHost.Q<Image>("RobotStageRenderSurface");
            if (renderSurfaceElement != null)
            {
                renderSurfaceElement.pickingMode = PickingMode.Ignore;
            }
        }

        private void EnsureDiagnosticLabel()
        {
            if (robotStageHost == null)
            {
                return;
            }

            diagnosticLabel = robotStageHost.Q<Label>("RobotStageDiagnosticLabel");
            if (diagnosticLabel != null)
            {
                diagnosticLabel.pickingMode = PickingMode.Ignore;
            }
        }

        private void OnRenderSurfacePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || renderSurfaceElement == null || runtimeController == null)
            {
                return;
            }

            var bounds = renderSurfaceElement.worldBound;
            if (bounds.width <= 1f || bounds.height <= 1f)
            {
                return;
            }

            var normalizedX = Mathf.Clamp01((evt.position.x - bounds.xMin) / bounds.width);
            var normalizedY = Mathf.Clamp01(1f - ((evt.position.y - bounds.yMin) / bounds.height));
            runtimeController.SelectRobotPartAtViewport(new Vector2(normalizedX, normalizedY));
            evt.StopPropagation();
        }

        private void UpdateDiagnosticLabel()
        {
            if (diagnosticLabel == null)
            {
                return;
            }

            diagnosticLabel.text = $"Stage init={initialized}\nHost={lastHostWidth:0.#} x {lastHostHeight:0.#}\nCamera={lastCameraName}\nRT={(renderTexture != null ? $"{renderTexture.width}x{renderTexture.height}" : "null")}\nInitAttempts={initializeAttemptCount} / EnsureAttempts={ensureAttemptCount}\nError={lastError}";
        }

        private static Camera ResolveStageCamera()
        {
            var stageCameraObject = GameObject.Find("V3StageCamera");
            var stageCamera = stageCameraObject != null ? stageCameraObject.GetComponent<Camera>() : null;
            if (stageCamera != null)
            {
                return stageCamera;
            }

            var mainCameraObject = GameObject.Find("Main Camera");
            var mainCamera = mainCameraObject != null ? mainCameraObject.GetComponent<Camera>() : null;
            if (mainCamera != null)
            {
                return mainCamera;
            }

            return Camera.main ?? Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
        }

    }
}
