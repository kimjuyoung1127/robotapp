// Folder: UI - reusable HUD overlays and screen-space feedback components.
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 화면 좌표 기준으로 X/Y/Z 3축 값을 표시하는 재사용 가능한 screen-space 오버레이입니다.
    /// Show/Update/hold/fade 상태를 자체적으로 관리하며, 계산은 외부에서 전달받습니다.
    /// </summary>
    public sealed class AxisTripletOverlay : MonoBehaviour
    {
        private const float PanelWidth = 196f;
        private const float PanelHeight = 122f;
        private const float ScreenPadding = 16f;
        private const float HoldDuration = 0.8f;
        private const float PositionOffsetX = 28f;
        private const float PositionOffsetY = 24f;

        [SerializeField] private Font fallbackFont;
        [SerializeField] private Canvas hostCanvas;

        private RectTransform panelRoot;
        private CanvasGroup canvasGroup;
        private Text titleText;
        private Text xValueText;
        private Text yValueText;
        private Text zValueText;
        private float holdTimer;
        private float fadeTimer;
        private OverlayState state = OverlayState.Hidden;

        private enum OverlayState
        {
            Hidden,
            Visible,
            Holding,
            Fading
        }

        public event System.Action OnHidden;

        public void Initialize(Canvas canvas, Font font)
        {
            hostCanvas = canvas;
            fallbackFont = UiRuntimeStyle.ResolveFont(font);
            if (!EnsurePresentation())
            {
                return;
            }

            HideImmediate();
        }

        public void Show(Vector2 screenPoint, string title, double x, double y, double z, string unitText)
        {
            if (!EnsurePresentation())
            {
                return;
            }

            titleText.text = string.IsNullOrWhiteSpace(title) ? "ΔTCP" : title;
            ApplyAxisValues(x, y, z, unitText);
            ApplyScreenPoint(screenPoint);
            holdTimer = 0f;
            fadeTimer = 0f;
            state = OverlayState.Visible;
            SetAlpha(1f);
        }

        public void UpdateValues(Vector2 screenPoint, double x, double y, double z, string unitText)
        {
            if (!EnsurePresentation())
            {
                return;
            }

            ApplyAxisValues(x, y, z, unitText);
            ApplyScreenPoint(screenPoint);
            holdTimer = 0f;
            fadeTimer = 0f;
            state = OverlayState.Visible;
            SetAlpha(1f);
        }

        public void BeginHoldAndFade()
        {
            if (!EnsurePresentation())
            {
                return;
            }

            if (canvasGroup == null || canvasGroup.alpha <= 0.001f)
            {
                return;
            }

            holdTimer = HoldDuration;
            fadeTimer = 0f;
            state = OverlayState.Holding;
        }

        public void HideImmediate()
        {
            if (!EnsurePresentation())
            {
                return;
            }

            var wasVisible = state != OverlayState.Hidden || (canvasGroup != null && canvasGroup.alpha > 0.001f);
            holdTimer = 0f;
            fadeTimer = 0f;
            state = OverlayState.Hidden;
            SetAlpha(0f);
            if (wasVisible)
            {
                OnHidden?.Invoke();
            }
        }

        private void Update()
        {
            StepAnimation(Time.unscaledDeltaTime);
        }

        private void StepAnimation(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            switch (state)
            {
                case OverlayState.Hidden:
                case OverlayState.Visible:
                    return;
                case OverlayState.Holding:
                    holdTimer -= deltaTime;
                    if (holdTimer <= 0f)
                    {
                        holdTimer = 0f;
                        fadeTimer = 0f;
                        state = OverlayState.Fading;
                    }
                    break;
                case OverlayState.Fading:
                    fadeTimer += deltaTime;
                    var t = Mathf.Clamp01(fadeTimer / UIDesignTokens.Anim.FadeNormal);
                    SetAlpha(1f - t);
                    if (t >= 1f)
                    {
                        HideImmediate();
                    }
                    break;
            }
        }

        private bool EnsurePresentation()
        {
            if (this == null)
            {
                return false;
            }

            panelRoot = transform as RectTransform;
            if (panelRoot == null)
            {
                panelRoot = gameObject.AddComponent<RectTransform>();
            }

            if (hostCanvas != null && panelRoot.parent != hostCanvas.transform)
            {
                panelRoot.SetParent(hostCanvas.transform, false);
            }

            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var background = gameObject.GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }

            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;
            background.raycastTarget = false;

            titleText = UiRuntimeStyle.EnsureText(panelRoot, "Title", fallbackFont, UIDesignTokens.Type.HeadingSm, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(160f, 20f), new Vector2(12f, -12f));

            var divider = UiRuntimeStyle.EnsureImage(panelRoot, "Divider", UIDesignTokens.Colors.BorderSoft);
            UiRuntimeStyle.Anchor((RectTransform)divider.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(PanelWidth - 24f, 1f), new Vector2(0f, -38f));

            xValueText = UiRuntimeStyle.EnsureText(panelRoot, "XAxisValue", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.AxisX);
            UiRuntimeStyle.Anchor(xValueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(172f, 18f), new Vector2(12f, -58f));

            yValueText = UiRuntimeStyle.EnsureText(panelRoot, "YAxisValue", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.AxisY);
            UiRuntimeStyle.Anchor(yValueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(172f, 18f), new Vector2(12f, -80f));

            zValueText = UiRuntimeStyle.EnsureText(panelRoot, "ZAxisValue", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.AxisZ);
            UiRuntimeStyle.Anchor(zValueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(172f, 18f), new Vector2(12f, -102f));
            return true;
        }

        private void ApplyAxisValues(double x, double y, double z, string unitText)
        {
            xValueText.text = $"dX {FormatAxisValue(x, unitText)}";
            yValueText.text = $"dY {FormatAxisValue(y, unitText)}";
            zValueText.text = $"dZ {FormatAxisValue(z, unitText)}";
        }

        private void ApplyScreenPoint(Vector2 screenPoint)
        {
            if (panelRoot == null || hostCanvas == null)
            {
                return;
            }

            var canvasRect = hostCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            var cam = hostCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : hostCanvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out var localPoint))
            {
                localPoint = Vector2.zero;
            }

            var desired = localPoint + new Vector2(PositionOffsetX, PositionOffsetY);
            var panelSize = panelRoot.rect.size.sqrMagnitude > 0f ? panelRoot.rect.size : panelRoot.sizeDelta;
            panelRoot.anchoredPosition = ClampAnchoredPosition(desired, panelSize, canvasRect.rect, ScreenPadding);
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        private static string FormatAxisValue(double value, string unitText)
        {
            var sign = value >= 0d ? "+" : string.Empty;
            var unit = string.IsNullOrWhiteSpace(unitText) ? string.Empty : $" {unitText}";
            return $"{sign}{value.ToString("F1", CultureInfo.InvariantCulture)}{unit}";
        }

        private static Vector2 ClampAnchoredPosition(Vector2 desired, Vector2 panelSize, Rect canvasRect, float padding)
        {
            var halfWidth = panelSize.x * 0.5f;
            var halfHeight = panelSize.y * 0.5f;

            desired.x = Mathf.Clamp(desired.x, canvasRect.xMin + halfWidth + padding, canvasRect.xMax - halfWidth - padding);
            desired.y = Mathf.Clamp(desired.y, canvasRect.yMin + halfHeight + padding, canvasRect.yMax - halfHeight - padding);
            return desired;
        }
    }
}
