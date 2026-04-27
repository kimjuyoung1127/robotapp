// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// RobotStage 우상단에 카메라 방향만 보여주는 orientation widget을 표시합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(RobotControlV3RuntimeController))]
    public sealed class RobotStageOrientationGizmoController : MonoBehaviour
    {
        private const float WidgetSize = 88f;
        private const float BadgeSize = 18f;
        private const float Radius = 24f;
        private const float HiddenOpacity = 0.32f;

        [SerializeField] private UIDocument document;
        [SerializeField] private RobotControlV3RuntimeController runtimeController;

        private VisualElement root;
        private VisualElement orientationHost;
        private VisualElement widgetRoot;
        private VisualElement centerDot;
        private Label axisX;
        private Label axisY;
        private Label axisZ;
        private bool callbacksRegistered;
        private bool initialized;

        private void OnEnable()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!initialized && !TryInitialize())
            {
                return;
            }

            RefreshWidget();
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public string GetDebugSummary()
        {
            return $"initialized={initialized}; host={(orientationHost != null)}; widget={(widgetRoot != null)}; camera={(runtimeController?.StageCamera != null)}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            root = document?.rootVisualElement;
            if (root == null || runtimeController == null)
            {
                initialized = false;
                return false;
            }

            orientationHost = root.Q<VisualElement>("RobotStageOrientationHost");
            if (orientationHost == null)
            {
                initialized = false;
                return false;
            }

            EnsureWidget();
            initialized = widgetRoot != null;
            RefreshWidget();
            return initialized;
        }

        private void EnsureWidget()
        {
            if (widgetRoot != null && axisX != null && axisY != null && axisZ != null)
            {
                return;
            }

            orientationHost.Clear();
            widgetRoot = new VisualElement
            {
                name = "RobotStageOrientationWidget"
            };
            widgetRoot.AddToClassList("rc-robot-stage-orientation-widget");

            centerDot = new VisualElement
            {
                name = "RobotStageOrientationCenter"
            };
            centerDot.AddToClassList("rc-robot-stage-orientation-center");
            widgetRoot.Add(centerDot);

            axisX = CreateAxisBadge("RobotStageOrientationAxisX", "X", "rc-robot-stage-orientation-axis--x");
            axisY = CreateAxisBadge("RobotStageOrientationAxisY", "Y", "rc-robot-stage-orientation-axis--y");
            axisZ = CreateAxisBadge("RobotStageOrientationAxisZ", "Z", "rc-robot-stage-orientation-axis--z");

            widgetRoot.Add(axisX);
            widgetRoot.Add(axisY);
            widgetRoot.Add(axisZ);
            orientationHost.Add(widgetRoot);
            RegisterClicks();
        }

        private static Label CreateAxisBadge(string name, string text, string colorClass)
        {
            var badge = new Label(text)
            {
                name = name
            };
            badge.AddToClassList("rc-robot-stage-orientation-axis");
            badge.AddToClassList(colorClass);
            return badge;
        }

        private void RegisterClicks()
        {
            if (callbacksRegistered)
            {
                return;
            }

            axisX?.RegisterCallback<ClickEvent>(_ => runtimeController?.SetStageCameraPreset("RIGHT"));
            axisY?.RegisterCallback<ClickEvent>(_ => runtimeController?.SetStageCameraPreset("TOP"));
            axisZ?.RegisterCallback<ClickEvent>(_ => runtimeController?.SetStageCameraPreset("FRONT"));
            centerDot?.RegisterCallback<ClickEvent>(_ => runtimeController?.SetStageCameraPreset("ISO"));
            callbacksRegistered = true;
        }

        private void RefreshWidget()
        {
            if (widgetRoot == null || runtimeController == null)
            {
                return;
            }

            var stageCamera = runtimeController.StageCamera;
            var visible = stageCamera != null;
            widgetRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible)
            {
                return;
            }

            PositionAxis(axisX, stageCamera.transform.InverseTransformDirection(Vector3.right));
            PositionAxis(axisY, stageCamera.transform.InverseTransformDirection(Vector3.up));
            PositionAxis(axisZ, stageCamera.transform.InverseTransformDirection(Vector3.forward));
        }

        private void PositionAxis(Label badge, Vector3 localDirection)
        {
            if (badge == null)
            {
                return;
            }

            var planar = new Vector2(localDirection.x, -localDirection.y);
            if (planar.sqrMagnitude < 0.0001f)
            {
                planar = new Vector2(0f, -1f);
            }

            planar.Normalize();
            var center = WidgetSize * 0.5f;
            var offset = planar * Radius;
            badge.style.left = center + offset.x - (BadgeSize * 0.5f);
            badge.style.top = center + offset.y - (BadgeSize * 0.5f);
            badge.style.opacity = Mathf.Clamp(localDirection.z * 0.5f + 0.5f, HiddenOpacity, 1f);
        }
    }
}
