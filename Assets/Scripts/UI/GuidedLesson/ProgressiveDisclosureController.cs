// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 스텝별 패널 표시/숨김과 전환 애니메이션을 제어합니다.
    /// </summary>
    public class ProgressiveDisclosureController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject leftPanel;
        [SerializeField] private GameObject rightPanel;
        [SerializeField] private GameObject bottomBar;
        [SerializeField] private GameObject frameInfoOverlay;
        [SerializeField] private bool suppressFrameInfoOverlay = true;

        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.4f;
        [SerializeField] private float fadeOutDuration = 0.25f;
        [SerializeField] private bool useReducedMotion;

        private readonly Dictionary<GameObject, Coroutine> runningByPanel = new Dictionary<GameObject, Coroutine>();

        private void Awake()
        {
            AutoWire();
            useReducedMotion = useReducedMotion || StepProgressSaver.GetReducedMotion();
            if (suppressFrameInfoOverlay && frameInfoOverlay != null)
            {
                frameInfoOverlay.SetActive(false);
            }
        }

        public void ApplyStep(TutorStepConfig config)
        {
            if (config == null)
            {
                return;
            }

            ApplyPanel(leftPanel, config.showLeftPanel);
            ApplyPanel(rightPanel, config.showRightPanel);
            ApplyPanel(bottomBar, config.showBottomBar);

            if (frameInfoOverlay != null)
            {
                frameInfoOverlay.SetActive(!suppressFrameInfoOverlay && config.rightContent == RightPanelContent.FrameInfoOverlay);
            }
        }

        private void ApplyPanel(GameObject panel, bool visible)
        {
            if (panel == null)
            {
                return;
            }

            if (useReducedMotion)
            {
                panel.SetActive(visible);
                return;
            }

            if (runningByPanel.TryGetValue(panel, out var running) && running != null)
            {
                StopCoroutine(running);
            }

            runningByPanel[panel] = StartCoroutine(AnimatePanel(panel, visible));
        }

        private IEnumerator AnimatePanel(GameObject panel, bool visible)
        {
            var group = panel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
            }

            if (visible)
            {
                panel.SetActive(true);
                yield return Fade(group, group.alpha, 1f, slideInDuration);
            }
            else
            {
                yield return Fade(group, group.alpha, 0f, fadeOutDuration);
                panel.SetActive(false);
            }

            runningByPanel[panel] = null;
        }

        private void AutoWire()
        {
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null) return;

            if (leftPanel == null)
            {
                var found = FindByName(canvas, "LeftPanel");
                if (found != null) leftPanel = found;
            }

            if (rightPanel == null)
            {
                var found = FindByName(canvas, "RightPanel");
                if (found != null) rightPanel = found;
            }

            if (bottomBar == null)
            {
                var found = FindByName(canvas, "BottomBar");
                if (found != null) bottomBar = found;
            }

            if (frameInfoOverlay == null)
            {
                var found = FindByName(canvas, "FrameInfoOverlay");
                if (found != null) frameInfoOverlay = found;
            }
        }

        private static GameObject FindByName(Canvas canvas, string objectName)
        {
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.name == objectName) return t.gameObject;
            }
            return null;
        }

        private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }
    }
}

