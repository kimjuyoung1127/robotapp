// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 모든 씬에서 공통으로 사용하는 상단 씬 전환 바입니다.
    /// </summary>
    [ExecuteAlways]
    public class SceneNavigationBar : MonoBehaviour
    {
        [SerializeField] private RectTransform topBarRoot;
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Graphic topBarBackground;
        [SerializeField] private bool createTitleIfMissing = true;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            topBarRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "TopBarRect");

            if (topBarRoot == null)
            {
                return;
            }

            UiRuntimeStyle.Stretch(topBarRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -76f), new Vector2(-16f, -16f));

            if (topBarBackground == null)
            {
                topBarBackground = UiRuntimeStyle.EnsureImage(topBarRoot, "TopBarBackground", UiRuntimeStyle.PanelBackgroundAlt);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(topBarBackground, topBarRoot);
            }

            UiRuntimeStyle.Stretch((RectTransform)topBarBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            if (createTitleIfMissing)
            {
                var titleText = GameObject.Find("TitleText")?.GetComponent<Text>();
                if (titleText == null)
                {
                    titleText = UiRuntimeStyle.EnsureText(topBarRoot, "TitleText", fallbackFont, 24, FontStyle.Bold, TextAnchor.MiddleLeft, UiRuntimeStyle.TextPrimary);
                    UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(220f, 40f), new Vector2(26f, 0f));
                    titleText.text = "KineTutor3D";
                }
            }

            buttonContainer ??= topBarRoot.Find("SceneNavButtons") as RectTransform;
            if (buttonContainer == null)
            {
                buttonContainer = UiRuntimeStyle.EnsureRectChild(topBarRoot, "SceneNavButtons");
            }

            buttonContainer.gameObject.SetActive(true);
            RebuildButtons();
        }

        private void RebuildButtons()
        {
            if (buttonContainer == null)
            {
                return;
            }

            var entries = SceneCatalog.GetNavigableEntries();
            var currentScene = SceneCatalog.GetCurrentSceneId();

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var button = ResolveOrCreateButton(i, entry, entry.Id == currentScene);
                var targetScene = entry.Id;
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SceneNavigator.Load(targetScene));
            }

            for (var i = entries.Count; i < buttonContainer.childCount; i++)
            {
                var child = buttonContainer.GetChild(i);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private Button ResolveOrCreateButton(int index, SceneEntry entry, bool isCurrentScene)
        {
            Button button = null;
            if (index < buttonContainer.childCount)
            {
                button = buttonContainer.GetChild(index).GetComponent<Button>();
            }

            if (button == null)
            {
                var go = new GameObject($"Nav{entry.DisplayName}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(buttonContainer, false);
                button = go.GetComponent<Button>();
            }
            else
            {
                button.transform.SetParent(buttonContainer, false);
            }

            button.gameObject.SetActive(true);
            var background = isCurrentScene
                ? new Color(0.23f, 0.27f, 0.40f, 0.95f)
                : UiRuntimeStyle.AccentBlue;
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, entry.DisplayName, background);
            UiRuntimeStyle.Anchor(button.transform as RectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(116f, 36f), new Vector2(380f + index * 126f, 0f));
            var layout = UiRuntimeStyle.EnsureLayoutElement(button);
            layout.preferredWidth = 116f;
            layout.minWidth = 104f;
            return button;
        }
    }
}

