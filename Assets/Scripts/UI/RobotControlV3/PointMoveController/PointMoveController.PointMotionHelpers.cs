// Folder: PointMoveController - saved-point live sync, validation, and motion helper logic for the PointMove panel.
// Serves point preview/apply preparation shared by PointMove motion and function run flows.
// Point storage mutation stays in PointCrud.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private bool EnsurePointLiveSyncGate(string actionLabel)
        {
            if (runtimeController == null)
            {
                SetFeedback($"{actionLabel} 전에 runtime을 찾지 못했다.");
                return false;
            }

            if (runtimeController.CurrentSnapshot.DryRunEnabled)
            {
                return true;
            }

            var syncResult = runtimeController.SyncCurrentState();
            if (!syncResult.IsSuccess)
            {
                SetFeedback($"[{actionLabel}] 현재 위치 읽기 실패 · {syncResult.Message}");
                return false;
            }

            var evidenceSummary = runtimeController.RefreshLiveEvidenceForDebug();
            if (!runtimeController.HasStableLiveEvidenceForDebug())
            {
                SetFeedback($"[{actionLabel}] 최신 위치 sync 확인이 아직 안정되지 않았다. {evidenceSummary}");
                return false;
            }

            return true;
        }

        private FairinoResult PreviewSavedMoveJ(string pointName, double[] savedJointTarget)
        {
            runtimeController?.PreviewJointAngles(savedJointTarget, $"저장 위치 {pointName} 저장된 관절 이동 후보");
            return FairinoResult.Ok("저장된 관절값 사용");
        }

        private FairinoResult ApplySavedMoveJ(string pointName, double[] savedJointTarget)
        {
            return runtimeController?.ApplyTeachingMoveJ(savedJointTarget, $"저장 위치 {pointName} 저장된 관절 이동 적용")
                ?? FairinoResult.Fail(-1, "관절 이동 준비 기능을 찾지 못했다.");
        }

        private bool EnsurePopupCoordinatorAvailableForLiveApproval(string actionLabel)
        {
            popupCoordinator ??= GetComponent<PopupCoordinatorV3>();
            popupCoordinator ??= GetComponentInParent<PopupCoordinatorV3>();
            popupCoordinator ??= Object.FindFirstObjectByType<PopupCoordinatorV3>();
            if (popupCoordinator != null)
            {
                return true;
            }

            SetFeedback($"[{actionLabel}] 실행 확인 팝업을 찾지 못했다. V3 화면을 다시 열고 시도해라.");
            return false;
        }

        private bool TryGetSavedJointTarget(double[] targetTcp, string pointName, out double[] savedJointTarget)
        {
            savedJointTarget = null;
            if (recalledPoint == null)
            {
                return false;
            }

            var safePointName = string.IsNullOrWhiteSpace(pointName)
                ? recalledPoint.name
                : pointName.Trim();
            if (!string.Equals(recalledPoint.name, safePointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (recalledPoint.jointsDeg == null || recalledPoint.jointsDeg.Length < 6)
            {
                return false;
            }

            savedJointTarget = (double[])recalledPoint.jointsDeg.Clone();
            return true;
        }

        private bool TryReadActivePanelValues(out double[] target, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            target = new double[currentValues.Length];
            validationMessage = "입력 검증 통과";
            lastInvalidIndex = -1;
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "저장 위치 패널을 찾지 못했다.";
                return false;
            }

            var pointName = panel.PointNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "위치 이름을 먼저 넣어라.";
                return false;
            }

            for (var index = 0; index < panel.ValueInputs.Length && index < target.Length; index++)
            {
                var rawValue = panel.ValueInputs[index].value;
                if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    || double.IsNaN(parsed)
                    || double.IsInfinity(parsed))
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 값 형식을 확인해라.";
                    return false;
                }

                if (index >= 3 && System.Math.Abs(parsed) > 360.0)
                {
                    lastInvalidIndex = index;
                    ApplyPanel(desktopPanel);
                    ApplyPanel(tabletPanel);
                    validationMessage = $"{GetAxisLabel(index)} 는 -360°~360° 범위 안으로 넣어라.";
                    return false;
                }

                target[index] = parsed;
            }

            for (var index = 0; index < currentValues.Length && index < target.Length; index++)
            {
                currentValues[index] = (float)target[index];
            }

            return true;
        }

        private bool TryReadActivePointName(out string pointName, out string validationMessage)
        {
            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            pointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            validationMessage = "위치 이름 검증 통과";
            isPointNameInvalid = false;

            if (panel == null)
            {
                validationMessage = "저장 위치 패널을 찾지 못했다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(pointName))
            {
                isPointNameInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                validationMessage = "위치 이름을 먼저 넣어라.";
                return false;
            }

            return true;
        }

        private string BuildDeltaSummary(string pointName)
        {
            var preview = connectionHomeController.CurrentPreviewDefinition;
            if (preview.TcpValues == null || preview.TcpValues.Length < 3)
            {
                return $"미리보기로 {pointName} 위치를 먼저 확인한 뒤 실제 이동을 보낸다.";
            }

            var dx = currentValues[0] - ParseValue(preview.TcpValues[0]);
            var dy = currentValues[1] - ParseValue(preview.TcpValues[1]);
            var dz = currentValues[2] - ParseValue(preview.TcpValues[2]);
            return $"미리보기 ΔTCP · X {dx:+0.0;-0.0;0.0} / Y {dy:+0.0;-0.0;0.0} / Z {dz:+0.0;-0.0;0.0}";
        }
    }
}
