// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 하단 토스트 메시지 표시를 담당합니다.
    /// </summary>
    public class ToastNotificationController : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Color infoColor = default;
        [SerializeField] private Color successColor = default;
        [SerializeField] private Color warningColor = default;

        private void InitColors()
        {
            if (infoColor == default) infoColor = UIDesignTokens.Colors.ToastInfo;
            if (successColor == default) successColor = UIDesignTokens.Colors.ToastSuccess;
            if (warningColor == default) warningColor = UIDesignTokens.Colors.ToastWarning;
        }

        private Coroutine hideRoutine;

        private void Awake()
        {
            InitColors();
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (toastRoot == null)
            {
                toastRoot = transform.Find("ToastRoot")?.gameObject;
            }

            if (messageText == null && toastRoot != null)
            {
                var t = toastRoot.transform.Find("ToastMessageText");
                if (t != null) messageText = t.GetComponent<Text>();
            }

            if (background == null && toastRoot != null)
            {
                background = toastRoot.GetComponent<Image>();
            }

            if (toastRoot != null)
            {
                var rect = toastRoot.transform as RectTransform;
                if (rect != null)
                {
                    UiRuntimeStyle.Anchor(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(460f, UIDesignTokens.Space.Xxl), new Vector2(0f, 88f));
                }

                if (background == null)
                {
                    background = toastRoot.GetComponent<Image>() ?? toastRoot.AddComponent<Image>();
                }
            }

            if (messageText == null && toastRoot != null)
            {
                messageText = UiRuntimeStyle.EnsureText(toastRoot.transform, "ToastMessageText", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            }

            if (messageText != null)
            {
                UiRuntimeStyle.Stretch(messageText.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 6f), new Vector2(-16f, -6f));
            }

            if (toastRoot != null)
            {
                toastRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (toastRoot == null)
            {
                toastRoot = transform.Find("ToastRoot")?.gameObject;
            }

            if (messageText == null && toastRoot != null)
            {
                messageText = toastRoot.GetComponentInChildren<Text>(true);
            }

            if (background == null && toastRoot != null)
            {
                background = toastRoot.GetComponent<Image>();
            }

            if (toastRoot != null)
            {
                var rect = toastRoot.transform as RectTransform;
                if (rect != null)
                {
                    UiRuntimeStyle.Anchor(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(460f, UIDesignTokens.Space.Xxl), new Vector2(0f, 88f));
                }
            }
        }

        public void ShowInfo(string message, float duration = 3f) => Show(message, infoColor, duration);
        public void ShowSuccess(string message, float duration = 3f) => Show(message, successColor, duration);
        public void ShowWarning(string message, float duration = 3f) => Show(message, warningColor, duration);

        private void Show(string message, Color color, float duration)
        {
            if (toastRoot == null)
            {
                return;
            }

            if (background != null) background.color = color;
            if (messageText != null) messageText.text = message;

            toastRoot.SetActive(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, duration));
            if (toastRoot != null)
            {
                toastRoot.SetActive(false);
            }

            hideRoutine = null;
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (toastRoot != null) toastRoot.SetActive(visible);
        }
    }
}

