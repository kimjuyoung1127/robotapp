// Folder: PointMoveController - bundle picker modal visibility and confirm flow for PointMove function insertion.
namespace KineTutor3D.UI.RobotControlV3
{
    // Handles bundle picker open/close state and selected function confirmation.
    // Point action modal behavior stays in PointActionModal, and detail/shared helpers remain in PointDetail.
    public sealed partial class PointMoveController
    {
        private void OpenBundlePickerModal()
        {
            if (runtimeController == null || runtimeController.GetTeachingFunctionNames().Length == 0)
            {
                SetFeedback("시퀀스에 넣을 묶음이 없다. 포인트 탭에서 함수부터 등록해라.");
                return;
            }

            bundlePickerSelectedName = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? runtimeController.GetTeachingFunctionNames()[0]
                : selectedFunctionName;
            bundlePickerModalOpen = true;
            ApplyAll();
        }

        private void CloseBundlePickerModal()
        {
            bundlePickerModalOpen = false;
            ApplyAll();
        }

        private void ConfirmBundlePickerSelection()
        {
            if (string.IsNullOrWhiteSpace(bundlePickerSelectedName))
            {
                SetFeedback("작업 시퀀스에 넣을 묶음을 먼저 골라라.");
                return;
            }

            selectedFunctionName = bundlePickerSelectedName;
            AddSelectedBundleToBlockSequence();
            bundlePickerModalOpen = false;
            ApplyAll();
        }

        private void ApplyBundlePickerModal(PanelElements panel)
        {
            if (panel?.BundlePickerModal == null)
            {
                return;
            }

            SetHidden(panel.BundlePickerModal, !bundlePickerModalOpen);
            if (!bundlePickerModalOpen)
            {
                return;
            }

            var functionNames = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            if (panel.BundlePickerSummary != null)
            {
                panel.BundlePickerSummary.text = functionNames.Length == 0
                    ? "등록된 묶음이 없다."
                    : string.IsNullOrWhiteSpace(bundlePickerSelectedName)
                        ? $"묶음 {functionNames.Length}개 · 선택 없음"
                        : $"묶음 {functionNames.Length}개 · 선택 {ShortDisplayName(bundlePickerSelectedName)}";
            }

            if (panel.BundlePickerListView != null)
            {
                bundlePickerListItems.Clear();
                for (var index = 0; index < functionNames.Length; index++)
                {
                    bundlePickerListItems.Add(functionNames[index]);
                }

                RefreshListView(panel.BundlePickerListView);
            }

            panel.BtnBundlePickerConfirm?.SetEnabled(!string.IsNullOrWhiteSpace(bundlePickerSelectedName));
        }
    }
}
