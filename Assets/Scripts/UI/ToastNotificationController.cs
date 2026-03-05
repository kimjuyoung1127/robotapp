using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 하단 토스트 메시지 표시를 담당합니다.
    /// </summary>
    public class ToastNotificationController : MonoBehaviour
    {
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private Color infoColor = new Color(0.16f, 0.16f, 0.31f, 0.95f);
        [SerializeField] private Color successColor = new Color(0.10f, 0.23f, 0.16f, 0.95f);
        [SerializeField] private Color warningColor = new Color(0.23f, 0.16f, 0.10f, 0.95f);

        private Coroutine hideRoutine;

        private void Awake()
        {
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
                toastRoot.SetActive(false);
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
