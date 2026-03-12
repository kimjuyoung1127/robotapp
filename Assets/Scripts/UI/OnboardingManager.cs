// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 온보딩 전용 씬에서 환영 모달과 시작/건너뛰기 동작을 제어합니다.
    /// </summary>
    [ExecuteAlways]
    public class OnboardingManager : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private RectTransform modalRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Button startLearningButton;
        [SerializeField] private Button beginnerButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text headlineText;
        [SerializeField] private Text bodyText;

        private bool listenersBound;

        private void Awake()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        /// <summary>
        /// 학습 시작 후 메인 씬으로 이동합니다.
        /// </summary>
        public void BeginLearning()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 0);
            SceneNavigator.Load(SceneId.Main);
        }

        public void BeginAsBeginner()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.PreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.PreKinematicsTrack, 0);
            SceneNavigator.Load(SceneId.Main);
        }

        /// <summary>
        /// 온보딩을 건너뛰고 메인 씬으로 이동합니다.
        /// </summary>
        public void SkipToMain()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SetCurrentTrack(StepProgressSaver.CoreKinematicsTrack);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 7);
            SceneNavigator.Load(SceneId.Main);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            canvasRoot ??= transform as RectTransform;

            if (canvasRoot == null)
            {
                return;
            }

            modalRoot ??= UiRuntimeStyle.EnsureRectChild(canvasRoot, "WelcomeModal");
            UiRuntimeStyle.Anchor(modalRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 420f), Vector2.zero);

            var modalImage = UiRuntimeStyle.EnsureImage(modalRoot, "ModalSurface", UiRuntimeStyle.PanelBackground);
            UiRuntimeStyle.Stretch((RectTransform)modalImage.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = UiRuntimeStyle.EnsureText(modalRoot, "HeadlineText", fallbackFont, 30, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(620f, 56f), new Vector2(34f, -34f));
            title.text = "KineTutor3D";
            headlineText = title;

            var body = UiRuntimeStyle.EnsureText(modalRoot, "BodyText", fallbackFont, 17, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Stretch(body.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(34f, 34f), new Vector2(-34f, -108f));
            body.text = "로봇 기구학을 직접 이해하세요!\n\n첫 방문에서는 온보딩을 보고 시작하고,\n이후에는 간단 내비게이션으로 언제든 Onboarding과 Main을 오갈 수 있습니다.";
            headlineText = title;
            bodyText = body;

            startLearningButton = EnsureActionButton(modalRoot, "BtnStartLearning", "학습 시작", new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(180f, 44f), new Vector2(-370f, 30f), UiRuntimeStyle.AccentBlue);
            beginnerButton = EnsureActionButton(modalRoot, "BtnBeginner", "초보자 시작", new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(180f, 44f), new Vector2(-200f, 30f), UiRuntimeStyle.AccentYellow);
            skipButton = EnsureActionButton(modalRoot, "BtnOnboardingSkip", "건너뛰기", new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(140f, 44f), new Vector2(-52f, 30f), UiRuntimeStyle.CardBackground);
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.AddListener(BeginLearning);
            }

            if (beginnerButton != null)
            {
                beginnerButton.onClick.AddListener(BeginAsBeginner);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipToMain);
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(BeginLearning);
            }

            if (beginnerButton != null)
            {
                beginnerButton.onClick.RemoveListener(BeginAsBeginner);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipToMain);
            }

            listenersBound = false;
        }

        private Button EnsureActionButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position, Color background)
        {
            var existing = parent.Find(name);
            var button = existing != null ? existing.GetComponent<Button>() : null;
            if (button == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                button = go.GetComponent<Button>();
            }

            var rect = (RectTransform)button.transform;
            UiRuntimeStyle.Anchor(rect, anchor, pivot, size, position);
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, background);
            return button;
        }
    }
}
