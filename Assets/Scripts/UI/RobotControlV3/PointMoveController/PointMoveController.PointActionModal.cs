// Folder: PointMoveController - point action modal visibility, copy, and primary edit/run actions.
using System.Globalization;
using UnityEngine;

namespace KineTutor3D.UI.RobotControlV3
{
    // Handles point action modal open/close state, summary text, and edit/run/apply flows.
    // Selected point detail rendering and generic point helpers remain in PointDetail; bundle picker stays separate.
    public sealed partial class PointMoveController
    {
        private void MovePointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalRunMode);
        }

        private void PreviewPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalPreviewMode);
        }

        private void EditPointRow(string pointName)
        {
            OpenPointActionModal(pointName, PointModalEditMode);
        }

        private void AddPointRowToFunction(string pointName)
        {
            OpenPointActionModal(pointName, PointModalFunctionMode);
        }

        private void ApplyPointActionModal(PanelElements panel)
        {
            if (panel?.PointActionModal == null)
            {
                return;
            }

            var show = pointActionModalOpen && activeNavSection == "NavPoints" && recalledPoint != null;
            SetHidden(panel.PointActionModal, !show);
            if (!show)
            {
                return;
            }

            var isEditMode = pointActionModalMode == PointModalEditMode;
            panel.PointActionModalTitle.text = BuildPointModalTitle();
            panel.PointActionModalSummary.text = BuildPointModalSummary();
            panel.PointActionModalPose.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")} / TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.PointActionModalNameInput?.SetValueWithoutNotify(recalledPoint.name ?? string.Empty);
            panel.PointActionModalDwellInput?.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.PointActionModalNameInput?.SetEnabled(isEditMode);
            panel.PointActionModalDwellInput?.SetEnabled(isEditMode);
            panel.PointActionModalNameInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.PointActionModalDwellInput?.EnableInClassList("rc-point-modal-readonly", !isEditMode);
            panel.BtnPointModalSpeedSlow?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedMedium?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedFast?.SetEnabled(isEditMode);
            panel.BtnPointModalSpeedSlow?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnPointModalSpeedMedium?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnPointModalSpeedFast?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.BtnPointModalPrimary.text = BuildPointModalPrimaryText();
            SetHidden(panel.BtnPointModalOverwrite, !isEditMode);
            SetHidden(panel.BtnPointModalDuplicate, !isEditMode);
            SetHidden(panel.BtnPointModalDelete, !isEditMode);
        }

        private string BuildPointModalTitle()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 확인",
                PointModalRunMode => "저장 위치 실행",
                PointModalEditMode => "저장 위치 편집",
                PointModalFunctionMode => "묶음에 추가",
                _ => "저장 위치 작업",
            };
        }

        private string BuildPointModalSummary()
        {
            var name = recalledPoint?.name ?? "-";
            return pointActionModalMode switch
            {
                PointModalPreviewMode => $"{name} 위치를 화면에서 먼저 확인한다.",
                PointModalRunMode => $"{name} 위치로 이동한다. 미리보기에서 먼저 움직임을 확인한다.",
                PointModalEditMode => $"{name} 이름, 속도, 대기 시간을 여기서 바로 수정한다.",
                PointModalFunctionMode => $"{name} 위치를 묶음 만들기 후보에 추가한다.",
                _ => $"{name} 저장 위치 작업을 선택한다.",
            };
        }

        private string BuildPointModalPrimaryText()
        {
            return pointActionModalMode switch
            {
                PointModalPreviewMode => "미리보기 실행",
                PointModalRunMode => "실행",
                PointModalEditMode => "저장",
                PointModalFunctionMode => "묶음에 추가",
                _ => "확인",
            };
        }

        private string BuildPointActionModalDebugSummary()
        {
            return $"modalOpen={pointActionModalOpen}; mode={pointActionModalMode}; point={recalledPoint?.name ?? "none"}; speed={selectedSpeedPreset}; dwell={selectedDwellSec:0.0}; feedback={lastFeedback}";
        }

        private void OpenPointActionModal(string pointName, string mode)
        {
            RecallPoint(pointName);
            if (recalledPoint == null)
            {
                return;
            }

            pointActionModalMode = NormalizePointModalMode(mode);
            pointActionModalOpen = true;
            SetFeedback($"[{BuildPointModalTitle()}] {recalledPoint.name}");
            ApplyAll();
        }

        private void ClosePointActionModal()
        {
            pointActionModalOpen = false;
            pointActionModalMode = string.Empty;
            ApplyAll();
        }

        private static string NormalizePointModalMode(string mode)
        {
            return mode switch
            {
                PointModalRunMode => PointModalRunMode,
                PointModalEditMode => PointModalEditMode,
                PointModalFunctionMode => PointModalFunctionMode,
                _ => PointModalPreviewMode,
            };
        }

        private void ApplyPointActionModalPrimary()
        {
            if (!pointActionModalOpen || recalledPoint == null)
            {
                SetFeedback("작업할 포인트를 먼저 선택해라.");
                return;
            }

            switch (pointActionModalMode)
            {
                case PointModalRunMode:
                    ApplyMotionCandidate();
                    break;
                case PointModalEditMode:
                    ApplyPointModalEdits();
                    break;
                case PointModalFunctionMode:
                    AddSelectedPointToFunction();
                    break;
                default:
                    PreviewMotionCandidate();
                    break;
            }
        }

        private void ApplyPointModalEdits()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 포인트 편집을 잠근다. Stop 후 다시 수정해라.");
                return;
            }

            var panel = ResolveActivePanel();
            var nextName = panel?.PointActionModalNameInput?.value?.Trim() ?? string.Empty;
            var nextDwellRaw = panel?.PointActionModalDwellInput?.value ?? "0";
            if (string.IsNullOrWhiteSpace(nextName))
            {
                SetFeedback("위치 이름을 먼저 넣어라.");
                return;
            }

            if (!double.TryParse(nextDwellRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var nextDwell)
                || double.IsNaN(nextDwell)
                || double.IsInfinity(nextDwell)
                || nextDwell < 0.0
                || nextDwell > 600.0)
            {
                SetFeedback("대기 시간은 0~600초 사이 숫자로 넣어라.");
                return;
            }

            var originalName = recalledPoint.name;
            selectedDwellSec = nextDwell;
            isDwellInvalid = false;
            if (!string.Equals(originalName, nextName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (HasNamedPoint(nextName))
                {
                    SetFeedback($"{nextName} 이름이 이미 있다.");
                    return;
                }

                RenamePoint(originalName, nextName);
            }

            ApplySelectedPointTiming();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = PointModalEditMode;
            ApplyAll();
        }

        private void SetPointModalSpeedPreset(string speedPreset)
        {
            if (pointActionModalMode != PointModalEditMode)
            {
                return;
            }

            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private void ExecutePointModalEditAction(System.Action action)
        {
            if (pointActionModalMode != PointModalEditMode || recalledPoint == null)
            {
                SetFeedback("편집할 포인트를 먼저 선택해라.");
                return;
            }

            action?.Invoke();
            pointActionModalOpen = recalledPoint != null;
            pointActionModalMode = pointActionModalOpen ? PointModalEditMode : string.Empty;
            ApplyAll();
        }

        private void SetSelectedSpeedPreset(string speedPreset)
        {
            selectedSpeedPreset = NormalizeSpeedPreset(speedPreset);
            ClearPendingConfirmation();
            ApplyAll();
        }

        private bool IsSequenceEditLocked()
        {
            return debugSequenceEditLocked || (runtimeController != null && runtimeController.IsTeachingSequenceRunning);
        }

        private void SetPendingConfirmation(string kind, string pointName)
        {
            pendingConfirmKind = kind ?? string.Empty;
            pendingConfirmName = pointName?.Trim() ?? string.Empty;
        }

        private bool IsPendingConfirmation(string kind, string pointName)
        {
            return string.Equals(pendingConfirmKind, kind, System.StringComparison.Ordinal)
                && string.Equals(pendingConfirmName, pointName?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
        }

        private void ClearPendingConfirmation()
        {
            pendingConfirmKind = string.Empty;
            pendingConfirmName = string.Empty;
        }
    }
}
