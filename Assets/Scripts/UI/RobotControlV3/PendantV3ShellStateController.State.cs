// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
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
            ApplyPanelRoleMarkers();
            ApplyBottomSheetState();
            ApplyBottomBarState();
            ApplyWorkTabBarVisibility();
            NotifyPanelControllers();
        }

        private void ApplyNavState()
        {
            SetActiveButton(navButtons, state.ActiveNavSection, "rc-nav-item--active");
            ApplyWorkTabBarVisibility();
            ApplyWorkPanelHeaderState();
            NotifyPanelControllers();
        }

        private void ApplyWorkTabBarVisibility()
        {
            if (workTabBar == null)
            {
                return;
            }

            var visible = state.ActiveNavSection == "NavMotion";
            workTabBar.EnableInClassList("rc-hidden", !visible);
            workTabBar.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplyWorkTabState()
        {
            SetActiveButton(workTabButtons, state.ActiveWorkTab, "rc-tab--active");
            ApplyWorkPanelHeaderState();

            if (viewportPanelScroll != null)
            {
                viewportPanelScroll.scrollOffset = Vector2.zero;
            }
            runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            runtimeController?.ResetStageCamera();
            NotifyPanelControllers();
        }

        private void ApplyWorkPanelHeaderState()
        {
            if (workPanelTitle != null)
            {
                workPanelTitle.text = GetWorkPanelTitle(state);
            }

            if (workPanelSummary != null)
            {
                var summary = GetWorkPanelSummary(state);
                workPanelSummary.text = summary;
                workPanelSummary.EnableInClassList("rc-hidden", string.IsNullOrEmpty(summary));
            }
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
            helpPanelController ??= GetComponent<HelpPanelController>();
            easyMotionController ??= GetComponent<EasyMotionController>();
            jointJogController ??= GetComponent<JointJogController>();
            tcpJogController ??= GetComponent<TcpJogController>();
            pointMoveController ??= GetComponent<PointMoveController>();
            ioPanelController ??= GetComponent<IoPanelController>();
            connectionHomeController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            helpPanelController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            easyMotionController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            jointJogController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            tcpJogController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            pointMoveController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            ioPanelController?.SetShellState(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
        }

        private void ApplyCoordSystemState()
        {
            if (coordSystemLabel != null)
            {
                coordSystemLabel.text = $"좌표 기준: {FormatCoordSystemDisplay(state.CoordSystem)}";
            }

            if (coordSystemButton != null)
            {
                coordSystemButton.text = $"기준 {FormatCoordSystemDisplay(state.CoordSystem)}";
            }

            NotifyPanelControllers();
        }

        private static string FormatCoordSystemDisplay(string coordSystem)
        {
            return coordSystem switch
            {
                "Tool" => "툴",
                "User" => "작업",
                "Base" => "로봇",
                _ => string.IsNullOrWhiteSpace(coordSystem) ? "--" : coordSystem,
            };
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

            var desktopSplitRatio = Mathf.Clamp(
                state.DesktopSplitRatio,
                PendantV3LocalState.MinSplitRatio,
                PendantV3LocalState.MaxSplitRatio);
            workPanel.style.flexGrow = 1f - desktopSplitRatio;
            viewportHost.style.flexGrow = desktopSplitRatio;
        }

        private void ApplyPanelRoleMarkers()
        {
            workPanel?.EnableInClassList("rc-work-panel--debug-highlight", false);
            workPanelDebugBadge?.EnableInClassList("rc-hidden", true);
            viewportHost?.EnableInClassList("rc-viewport-host--debug-highlight", false);
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

        private void ApplyBottomBarState()
        {
            var runtime = runtimeController ??= GetComponent<RobotControlV3RuntimeController>();
            var dryRunEnabled = runtime?.CurrentSnapshot.DryRunEnabled ?? false;
            if (dryRunButton != null)
            {
                dryRunButton.text = dryRunEnabled ? "미리보기 ON" : "미리보기 OFF";
                dryRunButton.EnableInClassList("rc-bottom-tab--active", dryRunEnabled);
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

        private static string GetWorkPanelTitle(PendantV3LocalState state)
        {
            if (state.ActiveNavSection == "NavPoints")
            {
                return "저장 위치";
            }

            if (state.ActiveNavSection == "NavMotion")
            {
                return "조작";
            }

            return state.ActiveWorkTab switch
            {
                "TabJointJog" => "관절",
                "TabTcpJog" => "TCP",
                "TabPointMove" => "저장 위치",
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
                "BottomTabPointMove" => "저장",
                "BottomTabStatus" => "상태",
                "BottomTabHelp" => "도움말",
                _ => "쉬운조작",
            };
        }

        private static string GetWorkPanelSummary(PendantV3LocalState state)
        {
            if (state.ActiveNavSection == "NavPoints")
            {
                return "저장 위치, 실행 순서, 작업 묶음을 한곳에서 관리한다.";
            }

            if (state.ActiveNavSection == "NavMotion")
            {
                return "메인 로봇 뷰 · 조작 방식은 보조패널에서 고른다.";
            }

            return state.ActiveWorkTab switch
            {
                "TabJointJog" => "메인 로봇 뷰 · 관절",
                "TabTcpJog" => "메인 로봇 뷰 · TCP",
                "TabPointMove" => "메인 로봇 뷰 · 저장 위치",
                _ => "메인 로봇 뷰 · 쉬운 조작",
            };
        }

        private static string GetBottomTabSummary(string buttonName)
        {
            return buttonName switch
            {
                "BottomTabJointJog" => "태블릿에서는 관절 조그를 하단 시트에서 열어 3D 뷰를 가리지 않게 유지한다.",
                "BottomTabTcpJog" => "태블릿에서는 TCP 조그와 좌표계 전환을 하단 시트에 모아 한 손 조작 흐름을 유지한다.",
                "BottomTabPointMove" => "태블릿에서는 저장 위치와 묶음을 하단 시트에서 한 흐름으로 확인한다.",
                "BottomTabStatus" => "태블릿에서는 상태/알람 요약을 하단 시트에서 열어 현재 위험도를 먼저 읽게 한다.",
                "BottomTabHelp" => "태블릿에서는 현재 단계 도움말을 하단 시트에서 바로 열어 작업 흐름을 끊지 않게 유지한다.",
                _ => "태블릿에서는 쉬운 조작 프리셋, 그리퍼, I/O를 하단 시트에서 바로 연다.",
            };
        }
    }
}
