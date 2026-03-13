// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// L2 비교 모드 — J1만/J2만/둘다 버튼을 표시하고 상호작용을 보고합니다.
    /// </summary>
    [ExecuteAlways]
    public class CompareModePanelHelper : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;

        private Button btnJ1Only;
        private Button btnJ2Only;
        private Button btnBoth;
        private RectTransform rootRect;
        private bool isVisible;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            isVisible = visible;
            if (rootRect != null)
            {
                rootRect.gameObject.SetActive(visible);
            }
        }

        private void EnsureLayout()
        {
            rootRect = transform as RectTransform;
            if (rootRect == null)
            {
                return;
            }

            btnJ1Only = EnsureCompareButton("BtnCompareJ1", "J1만", new Vector2(0f, -4f), UIDesignTokens.Colors.AccentSecondary);
            btnJ2Only = EnsureCompareButton("BtnCompareJ2", "J2만", new Vector2(0f, -40f), UIDesignTokens.Colors.AccentPrimary);
            btnBoth = EnsureCompareButton("BtnCompareBoth", "둘 다", new Vector2(0f, -76f), UIDesignTokens.Colors.SurfaceCard);

            BindButton(btnJ1Only, "compare_j1_only");
            BindButton(btnJ2Only, "compare_j2_only");
            BindButton(btnBoth, "compare_both");
        }

        private Button EnsureCompareButton(string name, string label, Vector2 position, Color bgColor)
        {
            var existing = rootRect.Find(name);
            Button button;
            if (existing != null)
            {
                button = existing.GetComponent<Button>();
                if (button != null) return button;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(rootRect, false);
            button = go.GetComponent<Button>();

            var rect = (RectTransform)go.transform;
            UiRuntimeStyle.Anchor(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, 32f), position);
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, bgColor);
            return button;
        }

        private static void BindButton(Button button, string actionId)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                var controller = Object.FindFirstObjectByType<AppController>();
                controller?.ReportInteraction(UI.Data.InteractionType.StepAction, actionId);
            });
        }
    }
}
