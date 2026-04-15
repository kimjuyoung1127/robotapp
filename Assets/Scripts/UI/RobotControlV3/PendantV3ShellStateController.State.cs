// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using System.Collections.Generic;
using KineTutor3D.App;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 셸 상태 적용과 저장 헬퍼를 분리합니다.
    /// </summary>
    public sealed partial class PendantV3ShellStateController
    {
        private void ApplyState()
        {
            state = PendantV3LocalState.Normalize(state);
            ApplyNavState();
            ApplyWorkTabState();
            ApplyBottomTabState();
            ApplyCoordSystemState();
            ApplyIncrementState();
            ApplySpeedState();
            ApplySplitRatio();
            ApplyBottomSheetState();
            NotifyPanelControllers();
        }

        private void ApplyNavState()
        {
            SetActiveButton(navButtons, state.ActiveNavSection, "rc-nav-item--active");
            NotifyPanelControllers();
        }

        private void ApplyWorkTabState()
        {
            SetActiveButton(workTabButtons, state.ActiveWorkTab, "rc-tab--active");
            var label = GetWorkTabLabel(state.ActiveWorkTab);
            if (workPanelTitle != null)
            {
                workPanelTitle.text = $"{label} 패널";
            }

            if (workPanelSummary != null)
            {
                workPanelSummary.text = GetWorkTabSummary(state.ActiveWorkTab);
            }

            NotifyPanelControllers();
        }

        private void ApplyBottomTabState()
        {
            SetActiveButton(bottomTabButtons, state.ActiveTabletTab, "rc-bottom-tab--active");
            var label = GetBottomTabLabel(state.ActiveTabletTab);
            if (bottomSheetTitle != null)
            {
                bottomSheetTitle.text = $"BottomSheet · {label}";
            }

            if (bottomSheetSummary != null)
            {
                bottomSheetSummary.text = GetBottomTabSummary(state.ActiveTabletTab);
            }

            NotifyPanelControllers();
        }

        private void NotifyPanelControllers()
        {
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            easyMotionController ??= GetComponent<EasyMotionController>();
            jointJogController ??= GetComponent<JointJogController>();
            tcpJogController ??= GetComponent<TcpJogController>();
            pointMoveController ??= GetComponent<PointMoveController>();
            connectionHomeController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            easyMotionController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            jointJogController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            tcpJogController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            pointMoveController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
        }

        private void ApplyCoordSystemState()
        {
            if (coordSystemLabel != null)
            {
                coordSystemLabel.text = $"좌표계: {state.CoordSystem}";
            }

            if (coordSystemButton != null)
            {
                coordSystemButton.text = $"좌표 {state.CoordSystem}";
            }

            NotifyPanelControllers();
        }

        private void ApplyIncrementState()
        {
            if (incrementButton != null)
            {
                incrementButton.text = $"증분 {state.JogIncrement}";
            }

            NotifyPanelControllers();
        }

        private void ApplySpeedState()
        {
            speedSlider?.SetValueWithoutNotify(state.SpeedPercent);
            if (speedLabel != null)
            {
                speedLabel.text = $"속도: {state.SpeedPercent}%";
            }

            if (speedValueLabel != null)
            {
                speedValueLabel.text = $"{state.SpeedPercent}%";
            }

            NotifyPanelControllers();
        }

        private void ApplySplitRatio()
        {
            if (workPanel == null || viewportHost == null)
            {
                return;
            }

            workPanel.style.flexGrow = state.DesktopSplitRatio;
            viewportHost.style.flexGrow = 1f - state.DesktopSplitRatio;
        }

        private void ApplyBottomSheetState()
        {
            bottomSheet?.EnableInClassList("rc-bottom-sheet--collapsed", !state.IsTabletSheetExpanded);
            bottomSheetContent?.EnableInClassList("rc-hidden", !state.IsTabletSheetExpanded);
            if (sheetToggleButton != null)
            {
                sheetToggleButton.text = state.IsTabletSheetExpanded ? "시트 접기" : "시트 펼치기";
            }
        }

        private void QueueSave()
        {
            hasPendingSave = true;
            if (saveCoroutine != null)
            {
                StopCoroutine(saveCoroutine);
            }

            saveCoroutine = StartCoroutine(SaveAfterDelay());
        }

        private IEnumerator SaveAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            LocalSettingsStore.Save(state);
            hasPendingSave = false;
            saveCoroutine = null;
        }

        private static void SetActiveButton(IEnumerable<Button> buttons, string activeName, string className)
        {
            foreach (var button in buttons)
            {
                button.EnableInClassList(className, button.name == activeName);
            }
        }

        private static int ResolveIndex(IReadOnlyList<string> values, string current)
        {
            for (var index = 0; index < values.Count; index++)
            {
                if (values[index] == current)
                {
                    return index;
                }
            }

            return 0;
        }

        private static int ResolveIndex(IReadOnlyList<int> values, int current)
        {
            for (var index = 0; index < values.Count; index++)
            {
                if (values[index] == current)
                {
                    return index;
                }
            }

            return 0;
        }

        private static string GetWorkTabLabel(string buttonName)
        {
            return buttonName switch
            {
                "TabJointJog" => "관절",
                "TabTcpJog" => "TCP",
                "TabPointMove" => "포인트 이동",
                "NavHelp" => "도움말",
                _ => "쉬운 조작",
            };
        }

        private static string GetBottomTabLabel(string buttonName)
        {
            return buttonName switch
            {
                "BottomTabJointJog" => "관절",
                "BottomTabTcpJog" => "TCP",
                "BottomTabPointMove" => "포인트",
                "BottomTabIo" => "I/O",
                "BottomTabStatus" => "상태",
                "BottomTabHelp" => "도움말",
                _ => "쉬운조작",
            };
        }

        private static string GetWorkTabSummary(string buttonName)
        {
            return buttonName switch
            {
                "TabJointJog" => "6축 관절값을 슬라이더, 단일축 버튼, 숫자 입력으로 바로 다루는 데스크탑 메인 패널.",
                "TabTcpJog" => "Base·Tool·User 좌표계 기준으로 XYZ·RPY 조그와 뷰포트 오버레이를 같이 쓰는 데스크탑 메인 패널.",
                "TabPointMove" => "지정 좌표를 입력하고 MoveJ·MoveL 후보를 준비하는 포인트 이동 패널.",
                _ => "자주 쓰는 포즈와 작은 이동부터 시작하는 데스크탑 메인 패널.",
            };
        }

        private static string GetBottomTabSummary(string buttonName)
        {
            return buttonName switch
            {
                "BottomTabJointJog" => "태블릿에서는 관절 조그를 하단 시트에서 열어 3D 뷰를 가리지 않게 유지한다.",
                "BottomTabTcpJog" => "태블릿에서는 TCP 조그와 좌표계 전환을 하단 시트에 모아 한 손 조작 흐름을 유지한다.",
                "BottomTabPointMove" => "태블릿에서는 포인트 이동 입력을 하단 시트에서 열어 확인과 취소 흐름을 좁게 묶는다.",
                "BottomTabIo" => "태블릿에서는 I/O 상태와 출력 토글을 하단 시트에서 빠르게 확인한다.",
                "BottomTabStatus" => "태블릿에서는 상태/알람 요약을 하단 시트에서 열어 현재 위험도를 먼저 읽게 한다.",
                "BottomTabHelp" => "태블릿에서는 현재 단계 도움말을 하단 시트에서 바로 열어 작업 흐름을 끊지 않게 유지한다.",
                _ => "태블릿에서는 쉬운 조작 프리셋과 작은 이동을 하단 시트에서 바로 연다.",
            };
        }
    }
}
