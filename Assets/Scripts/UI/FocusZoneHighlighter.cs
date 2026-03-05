using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 현재 학습 포커스 영역의 펄스 하이라이트를 표시합니다.
    /// </summary>
    public class FocusZoneHighlighter : MonoBehaviour
    {
        [SerializeField] private Graphic leftPanelHighlight;
        [SerializeField] private Graphic rightPanelHighlight;
        [SerializeField] private Graphic bottomBarHighlight;
        [SerializeField] private Graphic viewportHighlight;
        [SerializeField] private float pulsePeriod = 1.2f;

        private Graphic activeGraphic;
        private Color baseColor;
        private bool reducedMotion;

        private void Awake()
        {
            AutoWire();
            reducedMotion = StepProgressSaver.GetReducedMotion();
            DisableAll();
        }

        private void Update()
        {
            if (activeGraphic == null)
            {
                return;
            }

            if (reducedMotion)
            {
                activeGraphic.color = baseColor;
                return;
            }

            var pulse = Mathf.PingPong(Time.unscaledTime, pulsePeriod) / Mathf.Max(0.01f, pulsePeriod);
            var color = baseColor;
            color.a = Mathf.Lerp(0.35f, 0.95f, pulse);
            activeGraphic.color = color;
        }

        public void ApplyFocus(FocusTarget focusTarget, Color color)
        {
            DisableAll();

            activeGraphic = ResolveGraphic(focusTarget);
            if (activeGraphic == null)
            {
                return;
            }

            baseColor = color;
            activeGraphic.gameObject.SetActive(true);
            activeGraphic.color = color;
        }

        private Graphic ResolveGraphic(FocusTarget focusTarget)
        {
            switch (focusTarget)
            {
                case FocusTarget.LeftPanel:
                case FocusTarget.DHTable:
                    return leftPanelHighlight;
                case FocusTarget.RightPanel:
                case FocusTarget.MatrixPanel:
                    return rightPanelHighlight;
                case FocusTarget.BottomBar:
                    return bottomBarHighlight;
                case FocusTarget.Viewport3D:
                case FocusTarget.EndEffectorFrame:
                    return viewportHighlight;
                default:
                    return null;
            }
        }

        private void DisableAll()
        {
            SetActive(leftPanelHighlight, false);
            SetActive(rightPanelHighlight, false);
            SetActive(bottomBarHighlight, false);
            SetActive(viewportHighlight, false);
            activeGraphic = null;
        }

        private void AutoWire()
        {
            if (leftPanelHighlight == null)
            {
                var go = GameObject.Find("FocusLeftHighlight");
                if (go != null) leftPanelHighlight = go.GetComponent<Graphic>();
            }

            if (rightPanelHighlight == null)
            {
                var go = GameObject.Find("FocusRightHighlight");
                if (go != null) rightPanelHighlight = go.GetComponent<Graphic>();
            }

            if (bottomBarHighlight == null)
            {
                var go = GameObject.Find("FocusBottomHighlight");
                if (go != null) bottomBarHighlight = go.GetComponent<Graphic>();
            }

            if (viewportHighlight == null)
            {
                var go = GameObject.Find("FocusViewportHighlight");
                if (go != null) viewportHighlight = go.GetComponent<Graphic>();
            }
        }

        private static void SetActive(Graphic graphic, bool active)
        {
            if (graphic != null)
            {
                graphic.gameObject.SetActive(active);
            }
        }
    }
}
