// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// ?⑤낫???꾩슜 ?ъ뿉???섏쁺 ?⑤꼸怨??쒖옉/嫄대꼫?곌린 ?숈옉???쒖뼱?⑸땲??
    /// </summary>
    [ExecuteAlways]
    public class OnboardingManager : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private RectTransform modalRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Button startLearningButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text headlineText;
        [SerializeField] private Text bodyText;

        private bool buttonsBound;

        private void Awake()
        {
            EnsurePresentation();
            BindButtons();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        /// <summary>
        /// ?숈뒿 ?쒖옉 ??硫붿씤 ?ъ쑝濡??대룞?⑸땲??
        /// </summary>
        public void BeginLearning()
        {
            StepProgressSaver.MarkVisited();
            StepProgressSaver.SaveLastCompletedStep(0);
            SceneNavigator.Load(SceneId.Main);
        }

        /// <summary>
        /// ?⑤낫?⑹쓣 嫄대꼫?곌퀬 硫붿씤 ?ъ쑝濡??대룞?⑸땲??
        /// </summary>
        public void SkipToMain()
        {
            StepProgressSaver.MarkVisited();
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
            body.text = "濡쒕큸 湲곌뎄?숈쓣 ?덉쑝濡??댄빐?섏꽭??\n\n泥?諛⑸Ц?먯꽌???⑤낫?⑹쓣 蹂닿퀬 ?쒖옉?섍퀬,\n?댄썑?먮뒗 ?곷떒 ?ㅻ퉬寃뚯씠?섏쑝濡??몄젣?좎? Onboarding怨?Main???ㅺ컝 ???덉뒿?덈떎.";
            headlineText = title;
            bodyText = body;

            startLearningButton = EnsureActionButton(modalRoot, "BtnStartLearning", "?숈뒿 ?쒖옉", new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(200f, 44f), new Vector2(-246f, 30f), UiRuntimeStyle.AccentBlue);
            skipButton = EnsureActionButton(modalRoot, "BtnOnboardingSkip", "嫄대꼫?곌린", new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(160f, 44f), new Vector2(-52f, 30f), UiRuntimeStyle.CardBackground);
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.AddListener(BeginLearning);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipToMain);
            }

            buttonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!buttonsBound)
            {
                return;
            }

            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(BeginLearning);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(SkipToMain);
            }

            buttonsBound = false;
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

