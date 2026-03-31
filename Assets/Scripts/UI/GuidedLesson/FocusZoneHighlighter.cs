// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 현재 학습 단계의 영역별 포커스 하이라이트를 표시합니다.
    /// </summary>
    [ExecuteAlways]
    public class FocusZoneHighlighter : MonoBehaviour
    {
        [SerializeField] private Graphic leftPanelHighlight;
        [SerializeField] private Graphic rightPanelHighlight;
        [SerializeField] private Graphic bottomBarHighlight;
        [SerializeField] private Graphic viewportHighlight;
        [SerializeField] private RectTransform leftPanelTarget;
        [SerializeField] private RectTransform rightPanelTarget;
        [SerializeField] private RectTransform bottomBarTarget;
        [SerializeField] private RectTransform viewportTarget;
        [SerializeField] private float pulsePeriod = 1.2f;

        private Graphic activeGraphic;
        private Color baseColor;
        private bool reducedMotion;

        private void Awake()
        {
            reducedMotion = StepProgressSaver.GetReducedMotion();
        }

        private void OnEnable()
        {
            AutoWire();
            DisableAll();
        }

        private void OnDisable()
        {
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
            color.a = Mathf.Lerp(baseColor.a * 0.6f, baseColor.a, pulse);
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
            if (baseColor.a <= 0f)
            {
                baseColor.a = ResolveDefaultAlpha(focusTarget);
            }

            AlignToTarget(activeGraphic.rectTransform, ResolveTargetRect(focusTarget));
            activeGraphic.gameObject.SetActive(true);
            activeGraphic.color = baseColor;
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
                    return null;
                default:
                    return null;
            }
        }

        private RectTransform ResolveTargetRect(FocusTarget focusTarget)
        {
            switch (focusTarget)
            {
                case FocusTarget.LeftPanel:
                case FocusTarget.DHTable:
                    return leftPanelTarget;
                case FocusTarget.RightPanel:
                case FocusTarget.MatrixPanel:
                    return rightPanelTarget;
                case FocusTarget.BottomBar:
                    return bottomBarTarget;
                case FocusTarget.Viewport3D:
                case FocusTarget.EndEffectorFrame:
                    return null;
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
            leftPanelHighlight ??= FindByName<Graphic>("FocusLeftHighlight");
            rightPanelHighlight ??= FindByName<Graphic>("FocusRightHighlight");
            bottomBarHighlight ??= FindByName<Graphic>("FocusBottomHighlight");
            viewportHighlight ??= FindByName<Graphic>("FocusViewportHighlight");
            leftPanelTarget ??= FindByName<RectTransform>("LeftPanelRect");
            rightPanelTarget ??= FindByName<RectTransform>("RightPanelRect");
            bottomBarTarget ??= FindByName<RectTransform>("BottomBarRect");
            leftPanelTarget ??= FindByName<RectTransform>("LeftPanel");
            rightPanelTarget ??= FindByName<RectTransform>("RightPanel");
            bottomBarTarget ??= FindByName<RectTransform>("BottomBar");
            viewportTarget ??= FindByName<RectTransform>("ViewportTarget");
        }

        private static void SetActive(Graphic graphic, bool active)
        {
            if (graphic != null)
            {
                graphic.gameObject.SetActive(active);
            }
        }

        private static T FindByName<T>(string objectName) where T : Component
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
            {
                if (candidate == null || candidate.gameObject.hideFlags != HideFlags.None)
                {
                    continue;
                }

                if (candidate.gameObject.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static float ResolveDefaultAlpha(FocusTarget focusTarget)
        {
            return focusTarget == FocusTarget.Viewport3D || focusTarget == FocusTarget.EndEffectorFrame
                ? 0.12f
                : 0.18f;
        }

        private static void AlignToTarget(RectTransform highlight, RectTransform target)
        {
            if (highlight == null || target == null)
            {
                return;
            }

            highlight.anchorMin = target.anchorMin;
            highlight.anchorMax = target.anchorMax;
            highlight.anchoredPosition = target.anchoredPosition;
            highlight.sizeDelta = target.sizeDelta;
            highlight.pivot = target.pivot;
        }
    }
}

