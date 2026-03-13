// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Home / Continue Hub 화면을 런타임에 구성합니다.
    /// </summary>
    public class HomeContinueHubController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Font fallbackFont;

        private readonly HomeContinueHubFlowService flowService = new HomeContinueHubFlowService();
        private HomeContinueHubViewRefs view;
        private bool built;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureFallbackCamera();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            RefreshContext();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureFallbackCamera();
            canvas = HomeContinueHubViewBuilder.EnsureCanvas(canvas);
            HomeContinueHubViewBuilder.EnsureEventSystem();

            if (built || canvas == null)
            {
                return;
            }

            if (!HomeContinueHubViewBuilder.TryBindExisting(canvas, out view))
            {
                view = HomeContinueHubViewBuilder.Build(
                    canvas,
                    fallbackFont,
                    ContinueLatestContext,
                    StartGuidedLesson,
                    StartMathReadiness,
                    OpenRobotLibrary,
                    OpenSandbox);
            }

            BindButtons();
            built = true;
        }

        private static void EnsureFallbackCamera()
        {
            if (FindFirstObjectByType<Camera>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UIDesignTokens.Colors.SurfaceBase;
            camera.orthographic = false;
            camera.tag = "MainCamera";
            SceneCameraDirector.ConfigureForCurrentScene(camera);
        }

        private void RefreshContext()
        {
            if (view.ContextText == null || view.ContinueButton == null)
            {
                return;
            }

            var info = flowService.GetContextInfo();
            view.ContextText.text = info.Summary;
            view.ContinueButton.interactable = info.CanContinue;
        }

        private void ContinueLatestContext()
        {
            if (!flowService.ContinueLatestContext())
            {
                RefreshContext();
            }
        }

        private void StartGuidedLesson()
        {
            flowService.StartGuidedLesson();
        }

        private void StartMathReadiness()
        {
            flowService.StartMathReadiness();
        }

        private void OpenRobotLibrary()
        {
            flowService.OpenRobotLibrary();
        }

        private void OpenSandbox()
        {
            flowService.OpenSandbox();
        }

        private void BindButtons()
        {
            BindButton(view.ContinueButton, ContinueLatestContext);
            BindButton(view.StartGuidedLessonButton, StartGuidedLesson);
            BindButton(view.StartMathReadinessButton, StartMathReadiness);
            BindButton(view.OpenRobotLibraryButton, OpenRobotLibrary);
            BindButton(view.OpenSandboxButton, OpenSandbox);
        }

        private static void BindButton(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
