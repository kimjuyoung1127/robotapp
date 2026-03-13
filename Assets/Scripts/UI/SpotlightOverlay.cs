// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 온보딩 스포트라이트 오버레이를 제어합니다.
    /// </summary>
    public class SpotlightOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private Image dimImage;
        [SerializeField] private RectTransform spotlightCutout;

        private void Awake()
        {
            if (overlayRoot == null)
            {
                var found = transform.Find("SpotlightOverlayRoot");
                if (found != null) overlayRoot = found.gameObject;
            }

            if (dimImage == null && overlayRoot != null)
            {
                var found = overlayRoot.transform.Find("SpotlightDim");
                if (found != null) dimImage = found.GetComponent<Image>();
            }

            if (spotlightCutout == null && overlayRoot != null)
            {
                var found = overlayRoot.transform.Find("SpotlightCutout");
                if (found != null) spotlightCutout = found as RectTransform;
            }

            Hide();
        }

        private void OnEnable()
        {
            Hide();
        }

        public void Show(float dimAlpha)
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }

            if (dimImage != null)
            {
                var color = dimImage.color;
                color.a = Mathf.Clamp01(dimAlpha);
                dimImage.color = color;
            }
        }

        public void Focus(RectTransform target, float padding = 24f)
        {
            if (target == null || spotlightCutout == null)
            {
                return;
            }

            spotlightCutout.gameObject.SetActive(true);
            spotlightCutout.position = target.position;
            spotlightCutout.sizeDelta = target.rect.size + new Vector2(padding, padding);
        }

        public void ClearFocus()
        {
            if (spotlightCutout != null)
            {
                spotlightCutout.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }

            ClearFocus();
        }
    }
}

