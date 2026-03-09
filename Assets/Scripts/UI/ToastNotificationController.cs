using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 하단 토스트 메시지 표시를 담당합니다.
    /// </summary>
    [ExecuteAlways]
    public class ToastNotificationController : MonoBehaviour
    {
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Color infoColor = new Color(0.16f, 0.16f, 0.31f, 0.95f);
        [SerializeField] private Color successColor = new Color(0.10f, 0.23f, 0.16f, 0.95f);
        [SerializeField] private Color warningColor = new Color(0.23f, 0.16f, 0.10f, 0.95f);

        private Coroutine hideRoutine;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (toastRoot == null)
            {
                var go = GameObject.Find("ToastRoot");
                if (go != null) toastRoot = go;
            }

            if (messageText == null)
            {
                var go = GameObject.Find("ToastMessageText");
                if (go != null) messageText = go.GetComponent<Text>();
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
                    UiRuntimeStyle.Anchor(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(460f, 48f), new Vector2(0f, 88f));
                }

                if (background == null)
                {
                    background = toastRoot.GetComponent<Image>() ?? toastRoot.AddComponent<Image>();
                }
            }

            if (messageText == null && toastRoot != null)
            {
                messageText = UiRuntimeStyle.EnsureText(toastRoot.transform, "ToastMessageText", fallbackFont, 14, FontStyle.Bold, TextAnchor.MiddleCenter, UiRuntimeStyle.TextPrimary);
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
                var go = GameObject.Find("ToastRoot");
                if (go != null) toastRoot = go;
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
                    UiRuntimeStyle.Anchor(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(460f, 48f), new Vector2(0f, 88f));
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
    }
}
