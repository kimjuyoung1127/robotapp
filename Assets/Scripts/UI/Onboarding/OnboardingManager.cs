// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 온보딩 전용 씬에서 환영 화면과 학습 트랙 선택 카드를 제공합니다.
    /// UI 빌드는 OnboardingViewBuilder에 위임합니다.
    /// </summary>
    [ExecuteAlways]
    public class OnboardingManager : MonoBehaviour
    {
        private const string DefaultRobotId = "2DOF_RR";
        private const string DefaultRobotControlV3RobotId = "FAIRINO_FR5";

        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private SceneNavigationBar sceneNavigationBar;

        private Button startLearningButton;
        private Button beginnerButton;
        private Button skipButton;
        private Button v3RouteButton;
        private bool listenersBound;
        private bool viewBuilt;

        private static bool IsBatchEditTime => Application.isBatchMode && !Application.isPlaying;

        private void Awake()
        {
            if (IsBatchEditTime)
            {
                return;
            }

            EnsurePresentation();
            if (Application.isPlaying)
            {
                BindListeners();
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                EnsurePresentation();
            }
        }

        private void OnEnable()
        {
            if (IsBatchEditTime)
            {
                return;
            }

            EnsurePresentation();
            if (Application.isPlaying)
            {
                UnbindListeners();
                BindListeners();
            }
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void OnValidate()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                viewBuilt = false;
                EnsurePresentation();
            }
        }

        public void BeginLearning()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SessionContextStore.Clear();
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        public void BeginAsBeginner()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.MathReadinessTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.MathReadinessTrack, 0);
            SessionContextStore.Clear();
            RobotSelectionBridge.SetSelection(DefaultRobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.MathReadiness);
        }

        public void SkipToSandbox()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SessionContextStore.Clear();
            SceneNavigator.Load(SceneId.Sandbox);
        }

        public void OpenRobotControlV3Path()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SessionContextStore.Clear();
            RobotControlEntryPolicy.Apply(SceneId.RobotControlV3, RobotControlEntryPolicy.Intent.FreshStart);
            RobotSelectionBridge.SetSelection(DefaultRobotControlV3RobotId, RobotSelectionBridge.RobotControlMode);
            RobotControlScenePreference.SetPreferV3(true);
            SceneNavigator.Load(SceneId.RobotControlV3);
        }

        private void EnsurePresentation()
        {
            canvasRoot ??= transform as RectTransform;
            if (canvasRoot == null) return;

            canvasRoot.gameObject.SetActive(true);
            sceneNavigationBar ??= GetComponentInChildren<SceneNavigationBar>(true);
            EnsureGlobalNavigation();

            if (!viewBuilt)
            {
                var refs = OnboardingViewBuilder.TryBindExisting(canvasRoot, fallbackFont, out var existingRefs)
                    ? existingRefs
                    : OnboardingViewBuilder.Build(canvasRoot, fallbackFont);
                beginnerButton = refs.BeginnerButton;
                startLearningButton = refs.StartLearningButton;
                skipButton = refs.SkipButton;
                v3RouteButton = refs.V3RouteButton;
                viewBuilt = true;
            }

            BringGlobalNavigationToFront();
        }

        private void EnsureGlobalNavigation()
        {
            if (sceneNavigationBar == null) return;
            sceneNavigationBar.enabled = true;
            sceneNavigationBar.SetHideOnOnboarding(false);
        }

        private void BringGlobalNavigationToFront()
        {
            if (canvasRoot == null) return;
            var topBarRect = canvasRoot.Find("TopBarRect") as RectTransform;
            if (topBarRect != null) topBarRect.SetAsLastSibling();
        }

        private void BindListeners()
        {
            if (listenersBound) return;

            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(BeginLearning);
                startLearningButton.onClick.AddListener(BeginLearning);
            }

            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(BeginAsBeginner);
                beginnerButton.onClick.AddListener(BeginAsBeginner);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipToSandbox);
                skipButton.onClick.AddListener(SkipToSandbox);
            }

            if (v3RouteButton != null)
            {
                v3RouteButton.onClick.RemoveListener(OpenRobotControlV3Path);
                v3RouteButton.onClick.AddListener(OpenRobotControlV3Path);
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound) return;

            if (startLearningButton != null) startLearningButton.onClick.RemoveListener(BeginLearning);
            if (beginnerButton != null) beginnerButton.onClick.RemoveListener(BeginAsBeginner);
            if (skipButton != null) skipButton.onClick.RemoveListener(SkipToSandbox);
            if (v3RouteButton != null) v3RouteButton.onClick.RemoveListener(OpenRobotControlV3Path);

            listenersBound = false;
        }
    }
}
