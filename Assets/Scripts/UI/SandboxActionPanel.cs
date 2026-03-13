// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 샌드박스 모드 액션 버튼 패널입니다.
    /// </summary>
    public class SandboxActionPanel : MonoBehaviour
    {
        [SerializeField] private AppController appController;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private SandboxRobotPickerOverlay robotPickerOverlay;

        private readonly SandboxActionFlowService flowService = new SandboxActionFlowService();
        private bool viewBuilt;

        private void OnEnable()
        {
            EnsurePresentation();
        }

        public void SetVisible(bool visible)
        {
            EnsurePresentation();
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            appController ??= FindFirstObjectByType<AppController>(FindObjectsInactive.Include);

            if (viewBuilt)
            {
                return;
            }

            panelRoot = SandboxActionPanelViewBuilder.Build(
                this,
                panelRoot,
                fallbackFont,
                OnZeroPose,
                OnHomePose,
                OnDemoPose,
                OnResetPose,
                OnChangeRobot,
                OnBackHome,
                OnOpenRobotLibrary);
            viewBuilt = panelRoot != null;
        }

        private void OnZeroPose()
        {
            flowService.ApplyZeroPose(appController);
        }

        private void OnHomePose()
        {
            flowService.ApplyHomePose(appController);
        }

        private void OnDemoPose()
        {
            flowService.ApplyDemoPose(appController);
        }

        private void OnResetPose()
        {
            flowService.ResetPose(appController);
        }

        private void OnBackHome()
        {
            flowService.BackHome();
        }

        private void OnChangeRobot()
        {
            robotPickerOverlay ??= gameObject.GetComponent<SandboxRobotPickerOverlay>() ?? gameObject.AddComponent<SandboxRobotPickerOverlay>();
            var parent = panelRoot != null ? panelRoot.parent as RectTransform : transform as RectTransform;
            robotPickerOverlay.Show(parent, fallbackFont, OnRobotSelected);
        }

        private void OnRobotSelected(string robotId)
        {
            flowService.ChangeRobot(robotId);
        }

        private void OnOpenRobotLibrary()
        {
            flowService.OpenRobotLibrary();
        }
    }
}
