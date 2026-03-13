// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 샌드박스 스냅샷 저장/불러오기/비교 UI를 표시합니다.
    /// </summary>
    public class SnapshotLitePanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private AppController appController;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;

        private readonly SnapshotLiteFlowService flowService = new SnapshotLiteFlowService();
        private SandboxSnapshotData lastSnapshot;
        private Text statusText;
        private bool viewBuilt;

        private void OnEnable()
        {
            EnsurePresentation();
            RefreshStatus();
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

            var refs = SnapshotLitePanelViewBuilder.Build(
                this,
                panelRoot,
                fallbackFont,
                OnSaveSnapshot,
                OnLoadSnapshot,
                OnCompareSnapshot);
            panelRoot = refs.PanelRoot;
            statusText = refs.StatusText;
            viewBuilt = panelRoot != null;
        }

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            var status = flowService.GetLatestStatus(appController);
            statusText.text = status.Message;
            lastSnapshot = status.Snapshot;
        }

        private void OnSaveSnapshot()
        {
            var status = flowService.SaveSnapshot(appController);
            statusText.text = status.Message;
            lastSnapshot = status.Snapshot;
        }

        private void OnLoadSnapshot()
        {
            var status = flowService.LoadSnapshot(appController, lastSnapshot);
            statusText.text = status.Message;
            lastSnapshot = status.Snapshot;
        }

        private void OnCompareSnapshot()
        {
            var status = flowService.CompareSnapshot(appController, lastSnapshot);
            statusText.text = status.Message;
            lastSnapshot = status.Snapshot;
        }
    }
}
