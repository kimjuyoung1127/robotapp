using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 한국어 툴팁 버블의 표시/숨김을 담당하는 싱글턴입니다.
    /// </summary>
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
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
            if (tooltipRoot == null)
            {
                var go = GameObject.Find("TooltipRoot");
                if (go != null) tooltipRoot = go.GetComponent<RectTransform>();
            }

            if (titleText == null)
            {
                var go = GameObject.Find("TooltipTitleText");
                if (go != null) titleText = go.GetComponent<Text>();
            }

            if (bodyText == null)
            {
                var go = GameObject.Find("TooltipBodyText");
                if (go != null) bodyText = go.GetComponent<Text>();
            }
        }
    }
}
