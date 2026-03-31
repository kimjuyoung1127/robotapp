// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.UI.Data;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// L3 타깃 도달 피드백 — EE와 타깃 거리를 측정하고 도달 시 gate에 보고합니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class TargetFeedbackPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private AppController appController;
        [SerializeField] private TargetMarkerVisual targetMarkerVisual;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private float reachThreshold = 0.15f;

        private Text distanceText;
        private Text feedbackText;
        private Image panelBackground;
        private bool panelVisible;
        private int targetsReached;

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
            SetVisible(false);
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// AppController에 이벤트를 바인딩합니다.
        /// </summary>
        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;
            targetMarkerVisual ??= Object.FindFirstObjectByType<TargetMarkerVisual>(FindObjectsInactive.Include);

            if (appController != null)
            {
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureLayout();
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            panelVisible = visible;
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            if (!panelVisible || appController == null || targetMarkerVisual == null)
            {
                return;
            }

            if (!targetMarkerVisual.IsMarkersVisible || targetMarkerVisual.TargetMarker == null)
            {
                return;
            }

            var eePos = appController.CurrentEndEffectorTransform.ExtractPosition();
            var targetPos = targetMarkerVisual.TargetMarker.transform.position;
            var targetVec = new Vec3D(targetPos.x, targetPos.y, targetPos.z);
            var displacement = eePos - targetVec;
            var distance = displacement.Magnitude();

            UpdateDisplay(distance);

            if (distance <= reachThreshold)
            {
                OnTargetReached();
            }
        }

        private void HandleStepChanged(int _step, TutorStepConfig _config)
        {
            targetsReached = 0;
            UpdateDisplay(-1f);
        }

        private void OnTargetReached()
        {
            targetsReached++;
            targetMarkerVisual?.ShowSuccess();
            appController?.ReportInteraction(InteractionType.StepAction, "target_reached");

            if (feedbackText != null)
            {
                feedbackText.text = $"타깃 도달! ({targetsReached}회)";
                feedbackText.color = UIDesignTokens.Colors.AccentSuccess;
            }
        }

        private void UpdateDisplay(double distance)
        {
            if (distanceText == null)
            {
                return;
            }

            if (distance < 0)
            {
                distanceText.text = "타깃까지 거리: -";
                if (feedbackText != null)
                {
                    feedbackText.text = "관절을 조정해 타깃에 끝점을 맞추세요.";
                    feedbackText.color = UIDesignTokens.Colors.TextSecondary;
                }
                return;
            }

            distanceText.text = $"타깃까지 거리: {distance:F3} m";
            distanceText.color = distance <= reachThreshold ? UIDesignTokens.Colors.AccentSuccess : UIDesignTokens.Colors.TextPrimary;
        }

        private void EnsureLayout()
        {
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "TargetFeedbackRect");
            // Position below WhyItMovedPanel in RightPanel area
            UiRuntimeStyle.Stretch(panelRoot,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-404f, 146f), new Vector2(-16f, 220f));

            if (panelBackground == null)
            {
                panelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "TargetFeedbackBackground", UIDesignTokens.Colors.SurfaceCard);
            }
            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            distanceText = UiRuntimeStyle.EnsureText(panelRoot, "TFP_Distance", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(distanceText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(352f, 22f), new Vector2(16f, -12f));

            feedbackText = UiRuntimeStyle.EnsureText(panelRoot, "TFP_Feedback", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Italic, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(feedbackText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(352f, 28f), new Vector2(16f, -38f));
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnStepChanged -= HandleStepChanged;
            }
        }
    }
}
