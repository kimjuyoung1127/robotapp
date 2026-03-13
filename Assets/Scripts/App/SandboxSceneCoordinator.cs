// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI;
using KineTutor3D.Visualization;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// 샌드박스 씬 진입 시 학습 전용 패널과 샌드박스 전용 패널의 가시성을 조율합니다.
    /// </summary>
    internal sealed class SandboxSceneCoordinator
    {
        public bool IsSandboxScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            return activeScene.buildIndex == (int)SceneId.Sandbox ||
                string.Equals(activeScene.name, SceneId.Sandbox.ToString(), System.StringComparison.Ordinal);
        }

        public void ApplySandboxPresentation(
            StepTutorPanel stepTutorPanel,
            StepNavigator stepNavigator,
            FocusZoneHighlighter focusHighlighter,
            JointInputRail jointInputRail,
            WhyItMovedPanel whyItMovedPanel,
            BeginnerLeftPanel beginnerLeftPanel,
            MathReadinessPanel mathReadinessPanel,
            TargetFeedbackPanel targetFeedbackPanel,
            TargetMarkerVisual targetMarkerVisual,
            EndEffectorTrail endEffectorTrail,
            SandboxActionPanel sandboxActionPanel,
            SnapshotLitePanel snapshotLitePanel)
        {
            stepTutorPanel?.SetVisible(false);
            if (stepNavigator != null)
            {
                stepNavigator.gameObject.SetActive(false);
            }

            if (focusHighlighter != null)
            {
                focusHighlighter.gameObject.SetActive(false);
            }

            jointInputRail?.SetRailVisible(true);
            whyItMovedPanel?.SetVisible(false);
            beginnerLeftPanel?.SetVisible(false);
            mathReadinessPanel?.SetVisible(false);
            targetFeedbackPanel?.SetVisible(false);
            targetMarkerVisual?.SetMarkersVisible(false);
            endEffectorTrail?.SetTrailVisible(false);
            sandboxActionPanel?.SetVisible(true);
            snapshotLitePanel?.SetVisible(true);
        }
    }
}
