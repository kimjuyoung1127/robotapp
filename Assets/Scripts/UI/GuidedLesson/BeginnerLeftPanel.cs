// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Beginner track 전용 왼쪽 패널 — 레슨별 안내 텍스트와 비교 모드 버튼을 표시합니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BeginnerLeftPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private AppController appController;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;

        private Text guideTitleText;
        private Text guideBodyText;
        private Image panelBackground;
        private CompareModePanelHelper compareModeHelper;
        private bool panelVisible;
        private BeginnerLeftContent currentContent = BeginnerLeftContent.None;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
            SetVisible(false);
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// AppController에 이벤트를 바인딩합니다.
        /// </summary>
        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;

            if (appController != null)
            {
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureLayout();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            panelVisible = visible;
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 레슨별 왼쪽 패널 콘텐츠를 적용합니다.
        /// </summary>
        public void ApplyContent(BeginnerLeftContent content)
        {
            currentContent = content;
            SetVisible(content != BeginnerLeftContent.None);

            switch (content)
            {
                case BeginnerLeftContent.ObserveGuide:
                    SetGuide("관찰 가이드", "슬라이더를 움직여 관절이 로봇 팔에 어떤 영향을 주는지 관찰하세요.\n\n오른쪽 패널에서 무엇이 왜 움직였는지 확인할 수 있습니다.");
                    SetCompareModeVisible(false);
                    break;
                case BeginnerLeftContent.ArcCompareGuide:
                    SetGuide("호 궤적 가이드", "관절1만 천천히 움직여 보세요.\n\n끝점(EE)이 원호를 그리며 이동하는 것을 관찰할 수 있습니다.\n파란 트레일이 궤적을 보여줍니다.");
                    SetCompareModeVisible(false);
                    break;
                case BeginnerLeftContent.CombinationGuide:
                    SetGuide("조합 비교 가이드", "아래 버튼으로 관절을 개별/동시에 움직여 궤적 차이를 비교하세요.");
                    SetCompareModeVisible(true);
                    break;
                case BeginnerLeftContent.TargetHintGuide:
                    SetGuide("타깃 맞추기 가이드", "노란색 타깃 마커를 보고 관절 각도를 조정해 끝점을 맞춰 보세요.\n\n어느 관절을 먼저 움직여야 할지 생각해 보세요.");
                    SetCompareModeVisible(false);
                    break;
                default:
                    SetGuide(string.Empty, string.Empty);
                    SetCompareModeVisible(false);
                    break;
            }
        }

        private void HandleStepChanged(int _step, TutorStepConfig config)
        {
            if (config != null && config.beginnerMode)
            {
                ApplyContent(config.beginnerLeftContent);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void SetGuide(string title, string body)
        {
            if (guideTitleText != null) guideTitleText.text = title;
            if (guideBodyText != null) guideBodyText.text = body;
        }

        private void SetCompareModeVisible(bool visible)
        {
            if (compareModeHelper != null)
            {
                compareModeHelper.SetVisible(visible);
            }
        }

        private void EnsureLayout()
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "BeginnerLeftRect");
            UiRuntimeStyle.Stretch(panelRoot,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(16f, 146f), new Vector2(372f, -92f));

            if (panelBackground == null)
            {
                panelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "BeginnerLeftBackground", UIDesignTokens.Colors.SurfaceRaised);
            }
            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            guideTitleText = UiRuntimeStyle.EnsureText(panelRoot, "BLP_GuideTitle", fallbackFont, 16, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(guideTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 24f), new Vector2(16f, -16f));

            guideBodyText = UiRuntimeStyle.EnsureText(panelRoot, "BLP_GuideBody", fallbackFont, 14, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Stretch(guideBodyText.rectTransform,
                new Vector2(0f, 0.3f), new Vector2(1f, 1f),
                new Vector2(16f, 16f), new Vector2(-16f, -48f));

            EnsureCompareHelper();
        }

        private void EnsureCompareHelper()
        {
            if (compareModeHelper != null)
            {
                return;
            }

            var helperGo = panelRoot.Find("CompareModePanelHelper");
            if (helperGo != null)
            {
                compareModeHelper = helperGo.GetComponent<CompareModePanelHelper>();
            }

            if (compareModeHelper == null)
            {
                var go = new GameObject("CompareModePanelHelper", typeof(RectTransform));
                go.transform.SetParent(panelRoot, false);
                compareModeHelper = go.AddComponent<CompareModePanelHelper>();
            }

            UiRuntimeStyle.Stretch((RectTransform)compareModeHelper.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0.3f),
                new Vector2(8f, 8f), new Vector2(-8f, -8f));
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnStepChanged -= HandleStepChanged;
            }
        }
    }
}
