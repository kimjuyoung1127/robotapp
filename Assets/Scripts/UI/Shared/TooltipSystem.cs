// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 한국어 툴팁 버블의 표시/숨김을 담당하는 싱글턴입니다.
    /// </summary>
    [ExecuteAlways]
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Vector2 offset = new Vector2(16f, -16f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            AutoWire();
            Hide();
        }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            AutoWire();

            if (!Application.isPlaying)
            {
                Hide();
            }
        }

        public void ShowScreen(Vector2 screenPosition, string titleKo, string bodyKo)
        {
            if (tooltipRoot == null)
            {
                return;
            }

            tooltipRoot.gameObject.SetActive(true);
            tooltipRoot.position = screenPosition + offset;

            if (titleText != null)
            {
                titleText.text = titleKo;
            }

            if (bodyText != null)
            {
                bodyText.text = bodyKo;
            }
        }

        public void ShowWorld(Vector3 worldPosition, Camera targetCamera, string titleKo, string bodyKo)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            var screenPos = targetCamera.WorldToScreenPoint(worldPosition);
            ShowScreen(screenPos, titleKo, bodyKo);
        }

        public void Hide()
        {
            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(false);
            }
        }

        private void AutoWire()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (tooltipRoot == null)
            {
                var found = transform.Find("TooltipRoot");
                if (found != null) tooltipRoot = found as RectTransform;
            }

            if (titleText == null && tooltipRoot != null)
            {
                var found = tooltipRoot.Find("TooltipTitleText");
                if (found != null) titleText = found.GetComponent<Text>();
            }

            if (bodyText == null && tooltipRoot != null)
            {
                var found = tooltipRoot.Find("TooltipBodyText");
                if (found != null) bodyText = found.GetComponent<Text>();
            }

            if (tooltipRoot == null)
            {
                tooltipRoot = UiRuntimeStyle.EnsureRectChild(transform, "TooltipRoot");
            }

            var image = tooltipRoot.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                image = tooltipRoot.gameObject.AddComponent<UnityEngine.UI.Image>();
            }

            image.color = UIDesignTokens.Colors.SurfaceRaisedAlt;
            UiRuntimeStyle.Anchor(tooltipRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 120f), new Vector2(0f, 0f));

            if (titleText == null)
            {
                titleText = UiRuntimeStyle.EnsureText(tooltipRoot, "TooltipTitleText", fallbackFont, UIDesignTokens.Type.HeadingSm, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            }

            if (bodyText == null)
            {
                bodyText = UiRuntimeStyle.EnsureText(tooltipRoot, "TooltipBodyText", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            }

            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(244f, 22f), new Vector2(12f, -10f));
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(244f, 82f), new Vector2(12f, -34f));
        }
    }
}

