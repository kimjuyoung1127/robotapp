// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KineTutor3D.App
{
    /// <summary>
    /// RobotControlV3 입력 계약을 `unityctl exec`로 점검하기 위한 디버그 브리지입니다.
    /// </summary>
    public static class RobotControlV3DebugBridge
    {
        public static string OpenPopupProbe()
        {
            var contract = GetInputContract();
            contract.OpenPopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static string ClosePopupProbe()
        {
            var contract = GetInputContract();
            contract.ClosePopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static string GetInputContractSummary()
        {
            var contract = GetInputContract();
            return contract.GetDebugStateSummary();
        }

        public static string GetLocalSettingsSummary()
        {
            return LocalSettingsStore.LoadOrDefault().ToDebugSummary();
        }

        public static string ClearLocalSettings()
        {
            LocalSettingsStore.Clear();
            return LocalSettingsStore.LoadOrDefault().ToDebugSummary();
        }

        public static string SetLocalNavSection(string navSection)
        {
            var state = LocalSettingsStore.LoadOrDefault();
            state.ActiveNavSection = navSection;
            LocalSettingsStore.Save(state);
            return LocalSettingsStore.LoadOrDefault().ToDebugSummary();
        }

        public static string SetShellSelection(string navSection, string workTab, string tabletTab)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            if (shell == null)
            {
                throw new MissingReferenceException("PendantV3ShellStateController not found in RobotControlV3 scene.");
            }

            var localState = LocalSettingsStore.LoadOrDefault();
            localState.ActiveNavSection = navSection;
            localState.ActiveWorkTab = workTab;
            localState.ActiveTabletTab = tabletTab;
            LocalSettingsStore.Save(localState);
            shell.SetDebugSelection(navSection, workTab, tabletTab);
            return shell.GetDebugSummary();
        }

        public static string SetDesktopSplitRatioForDebug(float ratio)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            if (shell == null)
            {
                throw new MissingReferenceException("PendantV3ShellStateController not found in RobotControlV3 scene.");
            }

            var state = shell.GetStateSnapshot();
            state.DesktopSplitRatio = ratio;
            state = PendantV3LocalState.Normalize(state);
            LocalSettingsStore.Save(state);
            shell.SetDebugSelection(state.ActiveNavSection, state.ActiveWorkTab, state.ActiveTabletTab);
            return shell.GetDebugSummary();
        }

        public static string ClickVisualButtonForDebug(string buttonName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var document = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            if (document == null || shell == null)
            {
                return "UIDocument or shell missing";
            }

            var button = document.rootVisualElement?.Q<Button>(buttonName);
            if (button == null)
            {
                return $"button={buttonName}; found=False";
            }

            var state = shell.GetStateSnapshot();
            switch (buttonName)
            {
                case "NavHome":
                case "NavMotion":
                case "NavPoints":
                case "NavIo":
                case "NavStatus":
                case "NavHelp":
                    shell.SetDebugSelection(buttonName, state.ActiveWorkTab, state.ActiveTabletTab);
                    return $"button={buttonName}; found=True; action=nav; {shell.GetDebugSummary()}";
                case "TabEasyMotion":
                case "TabJointJog":
                case "TabTcpJog":
                case "TabPointMove":
                    shell.SetDebugSelection(state.ActiveNavSection, buttonName, state.ActiveTabletTab);
                    return $"button={buttonName}; found=True; action=work-tab; {shell.GetDebugSummary()}";
                case "BottomTabEasyMotion":
                case "BottomTabJointJog":
                case "BottomTabTcpJog":
                case "BottomTabPointMove":
                case "BottomTabIo":
                case "BottomTabStatus":
                    shell.SetDebugSelection(state.ActiveNavSection, state.ActiveWorkTab, buttonName);
                    return $"button={buttonName}; found=True; action=bottom-tab; {shell.GetDebugSummary()}";
                case "BtnTcpCoordBase":
                    return $"button={buttonName}; found=True; action=coord; {SetTcpCoordSystemForDebug("Base")}";
                case "BtnTcpCoordTool":
                    return $"button={buttonName}; found=True; action=coord; {SetTcpCoordSystemForDebug("Tool")}";
                case "BtnTcpCoordUser":
                    return $"button={buttonName}; found=True; action=coord; {SetTcpCoordSystemForDebug("User")}";
                default:
                    return $"button={buttonName}; found=True; action=unmapped";
            }
        }

        public static string GetSceneRouteSummary()
        {
            return RobotControlScenePreference.GetDebugSummary();
        }

        public static string GetShellControllerSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            return shell == null
                ? "PendantV3ShellStateController missing"
                : $"instanceId={shell.GetInstanceID()}; {shell.GetDebugSummary()}";
        }

        public static string GetSceneCoordinatorSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var coordinator = Object.FindFirstObjectByType<PendantV3SceneCoordinator>(FindObjectsInactive.Include);
            if (coordinator == null)
            {
                return "PendantV3SceneCoordinator missing";
            }

            coordinator.ForceBootstrap();
            return $"instanceId={coordinator.GetInstanceID()}; {coordinator.GetDebugSummary()}";
        }

        public static string GetBinderSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var binder = Object.FindFirstObjectByType<PendantV3Binder>(FindObjectsInactive.Include);
            if (binder == null)
            {
                return "PendantV3Binder missing";
            }

            return $"instanceId={binder.GetInstanceID()}; {binder.RefreshFromSourcesForDebug()}";
        }

        public static string GetJointJogControllerSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var jointJog = Object.FindFirstObjectByType<JointJogController>(FindObjectsInactive.Include);
            if (jointJog == null)
            {
                return "JointJogController missing";
            }

            jointJog.ForceInitialize();
            return $"instanceId={jointJog.GetInstanceID()}; {jointJog.GetDebugSummary()}";
        }

        public static string GetTcpJogControllerSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var tcpJog = Object.FindFirstObjectByType<TcpJogController>(FindObjectsInactive.Include);
            if (tcpJog == null)
            {
                return "TcpJogController missing";
            }

            tcpJog.ForceInitialize();
            return $"instanceId={tcpJog.GetInstanceID()}; {tcpJog.GetDebugSummary()}";
        }

        public static string GetPointMoveControllerSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var pointMove = Object.FindFirstObjectByType<PointMoveController>(FindObjectsInactive.Include);
            if (pointMove == null)
            {
                return "PointMoveController missing";
            }

            pointMove.ForceInitialize();
            return $"instanceId={pointMove.GetInstanceID()}; {pointMove.GetDebugSummary()}";
        }

        public static string SetCoordStripModeForDebug(string mode)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var status = Object.FindFirstObjectByType<StatusCardController>(FindObjectsInactive.Include);
            if (status == null)
            {
                throw new MissingReferenceException("StatusCardController not found in RobotControlV3 scene.");
            }

            status.ForceInitialize();
            return $"instanceId={status.GetInstanceID()}; {status.SetCoordStripModeForDebug(mode)}";
        }

        public static string GetPopupCoordinatorSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var popupCoordinator = Object.FindFirstObjectByType<PopupCoordinatorV3>(FindObjectsInactive.Include);
            if (popupCoordinator == null)
            {
                return "PopupCoordinatorV3 missing";
            }

            popupCoordinator.ForceInitialize();
            return $"instanceId={popupCoordinator.GetInstanceID()}; {popupCoordinator.GetDebugSummary()}";
        }

        public static string OpenPopupForDebug(string popupKind)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var popupCoordinator = Object.FindFirstObjectByType<PopupCoordinatorV3>(FindObjectsInactive.Include);
            if (popupCoordinator == null)
            {
                throw new MissingReferenceException("PopupCoordinatorV3 not found in RobotControlV3 scene.");
            }

            popupCoordinator.ForceInitialize();
            return popupCoordinator.OpenPopupForDebug(popupKind);
        }

        public static string SetPointMoveMotionKindForDebug(string motionKind)
        {
            var pointMove = GetPointMoveController();
            return pointMove.SetMotionKindForDebug(motionKind);
        }

        public static string PreviewPointMoveForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.PreviewForDebug();
        }

        public static string ApplyPointMoveForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.ApplyForDebug();
        }

        public static string SetGripperOpenForDebug(bool open)
        {
            var result = GetRuntimeController().SetGripperOpen(open);
            return $"{result.Message}; {GetMovementStateSummaryForDebug()}";
        }

        public static string SetRobotDoForDebug(int channel, bool value)
        {
            var result = GetRuntimeController().SetRobotDigitalOutput(channel, value);
            return $"{result.Message}; {GetMovementStateSummaryForDebug()}";
        }

        public static string SetToolDoForDebug(int channel, bool value)
        {
            var result = GetRuntimeController().SetToolDigitalOutput(channel, value);
            return $"{result.Message}; {GetMovementStateSummaryForDebug()}";
        }

        public static string SavePointMoveForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.SavePointForDebug();
        }

        public static string RecallPointMoveForDebug(string pointName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.RecallPointForDebug(pointName);
        }

        public static string DeletePointMoveForDebug(string pointName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.DeletePointForDebug(pointName);
        }

        public static string GetPointMoveListSummaryForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.GetPointListSummaryForDebug();
        }

        public static string RenamePointMoveForDebug(string oldName, string newName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.RenamePointForDebug(oldName, newName);
        }

        public static string ExportPointMoveForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.ExportPointsForDebug();
        }

        public static string CleanupPointMoveForDebug()
        {
            var pointMove = GetPointMoveController();
            return pointMove.CleanupPointsForDebug();
        }

        public static string SetPointMoveNameForDebug(string pointName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.SetPointNameForDebug(pointName);
        }

        public static string SetPointMoveValueForDebug(string axisLabel, float value)
        {
            var pointMove = GetPointMoveController();
            return pointMove.SetPointValueForDebug(axisLabel, value);
        }

        public static string SetJointJogShellState(string navSection, string workTab, string tabletTab)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var jointJog = Object.FindFirstObjectByType<JointJogController>(FindObjectsInactive.Include);
            if (jointJog == null)
            {
                throw new MissingReferenceException("JointJogController not found in RobotControlV3 scene.");
            }

            jointJog.SetShellState(navSection, workTab, tabletTab);
            return jointJog.GetDebugSummary();
        }

        public static string NudgeTcpAxisForDebug(string axisLabel, int direction)
        {
            var tcpJog = GetTcpJogController();
            return tcpJog.NudgeAxisForDebug(axisLabel, direction);
        }

        public static string SetTcpCoordSystemForDebug(string coordSystem)
        {
            var tcpJog = GetTcpJogController();
            return tcpJog.SetCoordSystemForDebug(coordSystem);
        }

        public static string GetJointRowSummary(int axisNumber)
        {
            var jointJog = GetJointJogController();
            return jointJog.GetJointRowDebugSummary(axisNumber);
        }

        public static string FocusJointInputForDebug(int axisNumber)
        {
            var jointJog = GetJointJogController();
            return jointJog.FocusJointInputForDebug(axisNumber);
        }

        public static string SetJointSliderForDebug(int axisNumber, float value)
        {
            var jointJog = GetJointJogController();
            return jointJog.SetJointSliderForDebug(axisNumber, value);
        }

        public static string SetJointInputForDebug(int axisNumber, string rawValue)
        {
            var jointJog = GetJointJogController();
            return jointJog.SetJointInputForDebug(axisNumber, rawValue);
        }

        public static string GetPanelControllerSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var home = Object.FindFirstObjectByType<ConnectionHomeController>(FindObjectsInactive.Include);
            var easy = Object.FindFirstObjectByType<EasyMotionController>(FindObjectsInactive.Include);
            var jointJog = Object.FindFirstObjectByType<JointJogController>(FindObjectsInactive.Include);
            var tcpJog = Object.FindFirstObjectByType<TcpJogController>(FindObjectsInactive.Include);
            var pointMove = Object.FindFirstObjectByType<PointMoveController>(FindObjectsInactive.Include);
            var ioPanel = Object.FindFirstObjectByType<IoPanelController>(FindObjectsInactive.Include);
            var status = Object.FindFirstObjectByType<StatusCardController>(FindObjectsInactive.Include);
            var safety = Object.FindFirstObjectByType<SafetyDiagnosticsController>(FindObjectsInactive.Include);
            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            var renderSurface = Object.FindFirstObjectByType<RobotStageRenderSurface>(FindObjectsInactive.Include);
            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            var binder = Object.FindFirstObjectByType<PendantV3Binder>(FindObjectsInactive.Include);
            var coordinator = Object.FindFirstObjectByType<PendantV3SceneCoordinator>(FindObjectsInactive.Include);
            var shellCount = Object.FindObjectsByType<PendantV3ShellStateController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var easyCount = Object.FindObjectsByType<EasyMotionController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var jointCount = Object.FindObjectsByType<JointJogController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var tcpCount = Object.FindObjectsByType<TcpJogController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var pointCount = Object.FindObjectsByType<PointMoveController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            coordinator?.ForceBootstrap();
            ioPanel ??= Object.FindFirstObjectByType<IoPanelController>(FindObjectsInactive.Include);
            var ioCount = Object.FindObjectsByType<IoPanelController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            binder?.ForceInitialize();
            home?.ForceInitialize();
            easy?.ForceInitialize();
            jointJog?.ForceInitialize();
            tcpJog?.ForceInitialize();
            pointMove?.ForceInitialize();
            ioPanel?.ForceInitialize();
            status?.ForceInitialize();
            safety?.ForceInitialize();
            var homeSummary = home != null ? home.GetDebugSummary() : "ConnectionHomeController missing";
            var easySummary = easy != null ? easy.GetDebugSummary() : "EasyMotionController missing";
            var jointJogSummary = jointJog != null ? jointJog.GetDebugSummary() : "JointJogController missing";
            var tcpJogSummary = tcpJog != null ? tcpJog.GetDebugSummary() : "TcpJogController missing";
            var pointMoveSummary = pointMove != null ? pointMove.GetDebugSummary() : "PointMoveController missing";
            var ioSummary = ioPanel != null ? ioPanel.GetDebugSummary() : "IoPanelController missing";
            var statusSummary = status != null ? $"instanceId={status.GetInstanceID()}" : "StatusCardController missing";
            var safetySummary = safety != null ? $"instanceId={safety.GetInstanceID()}" : "SafetyDiagnosticsController missing";
            var runtimeSummary = runtime != null ? runtime.GetDebugSummary() : "RobotControlV3RuntimeController missing";
            var renderSummary = renderSurface != null ? renderSurface.GetDebugSummary() : "RobotStageRenderSurface missing";
            var shellSummary = shell != null ? shell.GetDebugSummary() : "PendantV3ShellStateController missing";
            var binderSummary = binder != null ? binder.GetDebugSummary() : "PendantV3Binder missing";
            var coordinatorSummary = coordinator != null ? coordinator.GetDebugSummary() : "PendantV3SceneCoordinator missing";
            return $"counts=[shell={shellCount}; easy={easyCount}; joint={jointCount}; tcp={tcpCount}; point={pointCount}; io={ioCount}] | coordinator=[{coordinatorSummary}] | runtime=[{runtimeSummary}] | render=[{renderSummary}] | binder=[{binderSummary}] | shell=[{shellSummary}] | home=[{homeSummary}] | status=[{statusSummary}] | safety=[{safetySummary}] | easy=[{easySummary}] | joint=[{jointJogSummary}] | tcp=[{tcpJogSummary}] | point=[{pointMoveSummary}] | io=[{ioSummary}]";
        }

        public static string GetV3RuntimeSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            return runtime == null
                ? "RobotControlV3RuntimeController missing"
                : $"instanceId={runtime.GetInstanceID()}; {runtime.GetDebugSummary()}";
        }

        public static string GetMovementStateSummaryForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = GetRuntimeController();
            var snapshot = runtime.CurrentSnapshot;
            return $"status={snapshot.StatusKind}; dryRun={snapshot.DryRunEnabled}; pending={snapshot.PendingCommandSummary}; feedback={snapshot.LastFeedback}; joints=[{string.Join(",", snapshot.JointValues)}]; tcp=[{string.Join(",", snapshot.TcpValues)}]; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; gripper={snapshot.GripperSummary}; robotDo={snapshot.RobotDoSummary}; toolDo={snapshot.ToolDoSummary}; peripheral={snapshot.PeripheralFeedback}; selected={snapshot.SelectedPartName}; liveBlocked={snapshot.LiveBlockedReason}";
        }

        public static string GetRobotStageRenderSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var surface = Object.FindFirstObjectByType<RobotStageRenderSurface>(FindObjectsInactive.Include);
            surface?.ForceInitialize();
            return surface == null
                ? "RobotStageRenderSurface missing"
                : $"instanceId={surface.GetInstanceID()}; {surface.GetDebugSummary()}";
        }

        public static string SelectRobotPartAtViewportForDebug(float normalizedX, float normalizedY)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            if (runtime == null)
            {
                throw new MissingReferenceException("RobotControlV3RuntimeController not found in RobotControlV3 scene.");
            }

            var selected = runtime.SelectRobotPartAtViewport(new Vector2(normalizedX, normalizedY));
            return $"selected={selected}; {runtime.GetDebugSummary()}";
        }

        public static string SelectRobotPartCenterForDebug()
        {
            return SelectRobotPartAtViewportForDebug(0.5f, 0.5f);
        }

        public static string ToggleViewportDescriptionForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var controller = Object.FindFirstObjectByType<ViewportAuxInfoController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                throw new MissingReferenceException("ViewportAuxInfoController not found in RobotControlV3 scene.");
            }

            controller.ForceInitialize();
            return controller.ToggleDescriptionForDebug();
        }

        public static string ToggleViewportSelectionForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var controller = Object.FindFirstObjectByType<ViewportAuxInfoController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                throw new MissingReferenceException("ViewportAuxInfoController not found in RobotControlV3 scene.");
            }

            controller.ForceInitialize();
            return controller.ToggleSelectionForDebug();
        }

        public static string PreviewEasyMotionForDebug(string presetName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            if (runtime == null)
            {
                throw new MissingReferenceException("RobotControlV3RuntimeController not found in RobotControlV3 scene.");
            }

            runtime.PreviewPreset(presetName);
            return runtime.GetDebugSummary();
        }

        public static string SetPreferV3Route(bool value)
        {
            RobotControlScenePreference.SetPreferV3(value);
            return RobotControlScenePreference.GetDebugSummary();
        }

        public static string GetDocumentDebugSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var document = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            var bridge = Object.FindFirstObjectByType<PendantV3Document>(FindObjectsInactive.Include);
            if (document == null)
            {
                return "UIDocument missing";
            }

            var root = document.rootVisualElement;
            var childCount = root != null ? root.childCount : -1;
            var panelName = document.panelSettings != null ? document.panelSettings.name : "null";
            var treeName = document.visualTreeAsset != null ? document.visualTreeAsset.name : "null";
            var bridgeName = bridge != null ? bridge.GetType().Name : "null";
            var robotName = root.Q<Label>("RobotNameLabel")?.text ?? "missing";
            var easyHome = root.Q<Button>("BtnEasyHome") != null;
            var homePanel = root.Q<VisualElement>("HomePanelHost") != null;
            var easyPanel = root.Q<VisualElement>("EasyMotionPanelHost") != null;
            var homeSheet = root.Q<VisualElement>("HomeSheetHost");
            var easySheet = root.Q<VisualElement>("EasyMotionSheetHost");
            var homeHost = root.Q<VisualElement>("HomePanelHost");
            var easyHost = root.Q<VisualElement>("EasyMotionPanelHost");
            var workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            var bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            var whyCard = root.Q<VisualElement>("WhyItMoved");
            var workPanelHeader = root.Q<VisualElement>("WorkPanelHeader");
            var workPanelTitle = root.Q<Label>("WorkPanelTitle");
            var workPanelSummary = root.Q<Label>("WorkPanelSummary");
            var workPanelChipPrimary = root.Q<Label>("WorkPanelChipPrimary");
            var workPanelChipSecondary = root.Q<Label>("WorkPanelChipSecondary");
            var robotStageHost = root.Q<VisualElement>("RobotStageHost");
            var robotStageSurface = root.Q<VisualElement>("RobotStageRenderSurface");
            var robotStageDiagnostic = root.Q<Label>("RobotStageDiagnosticLabel");
            var viewportHost = root.Q<VisualElement>("ViewportHost");
            var viewportToolbarHost = root.Q<VisualElement>("ViewportToolbarHost");
            var cartesianOverlayHost = root.Q<VisualElement>("CartesianArrowsOverlayHost");
            var contextPanel = root.Q<VisualElement>("ContextPanel") != null;
            var descendantCount = CountDescendants(root);
            var homeHostHidden = homeHost?.ClassListContains("rc-hidden") ?? false;
            var easyHostHidden = easyHost?.ClassListContains("rc-hidden") ?? false;
            var easySheetHidden = easySheet?.ClassListContains("rc-hidden") ?? false;
            var workPanelBodyHidden = workPanelBody?.ClassListContains("rc-hidden") ?? false;
            var workPanelSummaryHidden = workPanelSummary?.ClassListContains("rc-hidden") ?? false;
            var bottomSheetBodyHidden = bottomSheetBody?.ClassListContains("rc-hidden") ?? false;
            var whyCardHidden = whyCard?.ClassListContains("rc-hidden") ?? false;
            var workPanelHeaderBounds = workPanelHeader != null ? $"{workPanelHeader.worldBound.x:0.#},{workPanelHeader.worldBound.y:0.#},{workPanelHeader.worldBound.width:0.#}x{workPanelHeader.worldBound.height:0.#}" : "missing";
            var robotStageHostDisplay = robotStageHost != null ? robotStageHost.resolvedStyle.display.ToString() : "missing";
            var robotStageSurfaceDisplay = robotStageSurface != null ? robotStageSurface.resolvedStyle.display.ToString() : "missing";
            var robotStageDiagnosticDisplay = robotStageDiagnostic != null ? robotStageDiagnostic.resolvedStyle.display.ToString() : "missing";
            var robotStageDiagnosticText = robotStageDiagnostic?.text ?? "missing";
            var viewportHostDisplay = viewportHost != null ? viewportHost.resolvedStyle.display.ToString() : "missing";
            var viewportToolbarDisplay = viewportToolbarHost != null ? viewportToolbarHost.resolvedStyle.display.ToString() : "missing";
            var viewportToolbarParent = viewportToolbarHost?.hierarchy.parent?.name ?? "missing";
            var cartesianOverlayParent = cartesianOverlayHost?.hierarchy.parent?.name ?? "missing";
            var robotStageHostBounds = robotStageHost != null ? $"{robotStageHost.worldBound.x:0.#},{robotStageHost.worldBound.y:0.#},{robotStageHost.worldBound.width:0.#}x{robotStageHost.worldBound.height:0.#}" : "missing";
            var viewportHostBounds = viewportHost != null ? $"{viewportHost.worldBound.x:0.#},{viewportHost.worldBound.y:0.#},{viewportHost.worldBound.width:0.#}x{viewportHost.worldBound.height:0.#}" : "missing";
            var viewportToolbarBounds = viewportToolbarHost != null ? $"{viewportToolbarHost.worldBound.x:0.#},{viewportToolbarHost.worldBound.y:0.#},{viewportToolbarHost.worldBound.width:0.#}x{viewportToolbarHost.worldBound.height:0.#}" : "missing";
            return $"panel={panelName}; tree={treeName}; rootChildren={childCount}; rootName={(root?.name ?? "null")}; bridge={bridgeName}; robotName={robotName}; easyHome={easyHome}; homeHost={homePanel}; easyHost={easyPanel}; context={contextPanel}; homeHostChildren={homeHost?.childCount ?? -1}; easyHostChildren={easyHost?.childCount ?? -1}; homeHostHidden={homeHostHidden}; easyHostHidden={easyHostHidden}; workPanelTitle={(workPanelTitle?.text ?? "missing")}; workPanelChipPrimary={(workPanelChipPrimary?.text ?? "missing")}; workPanelChipSecondary={(workPanelChipSecondary?.text ?? "missing")}; workPanelSummaryHidden={workPanelSummaryHidden}; workPanelHeaderBounds={workPanelHeaderBounds}; workPanelBodyHidden={workPanelBodyHidden}; homeSheetChildren={homeSheet?.childCount ?? -1}; easySheetChildren={easySheet?.childCount ?? -1}; easySheetHidden={easySheetHidden}; bottomSheetBodyHidden={bottomSheetBodyHidden}; robotStageHostDisplay={robotStageHostDisplay}; robotStageHostBounds={robotStageHostBounds}; robotStageSurfaceDisplay={robotStageSurfaceDisplay}; robotStageDiagnosticDisplay={robotStageDiagnosticDisplay}; robotStageDiagnostic={robotStageDiagnosticText}; viewportHostDisplay={viewportHostDisplay}; viewportHostBounds={viewportHostBounds}; viewportToolbarDisplay={viewportToolbarDisplay}; viewportToolbarParent={viewportToolbarParent}; viewportToolbarBounds={viewportToolbarBounds}; cartesianOverlayParent={cartesianOverlayParent}; whyCardHidden={whyCardHidden}; descendants={descendantCount}";
        }

        public static string ScrollContextPanelToTopForDebug()
        {
            var scrollView = GetContextPanelScrollView();
            scrollView.scrollOffset = Vector2.zero;
            return GetContextPanelScrollSummary();
        }

        public static string ScrollContextPanelToBottomForDebug()
        {
            var scrollView = GetContextPanelScrollView();
            scrollView.scrollOffset = new Vector2(0f, 100000f);
            return GetContextPanelScrollSummary();
        }

        public static string GetContextPanelScrollSummary()
        {
            var scrollView = GetContextPanelScrollView();
            var viewportHeight = scrollView.contentViewport.layout.height;
            var contentHeight = scrollView.contentContainer.layout.height;
            return $"offsetY={scrollView.scrollOffset.y:F1}; viewportHeight={viewportHeight:F1}; contentHeight={contentHeight:F1}";
        }

        public static string GetAuxLayoutSummaryForDebug()
        {
            var contract = GetInputContract();
            var root = contract.GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null)
            {
                throw new MissingReferenceException("RobotControlV3 UIDocument root not found.");
            }

            var viewportHost = root.Q<VisualElement>("ViewportHost");
            var viewportScroll = root.Q<ScrollView>("ViewportPanelScroll");
            var contextScroll = root.Q<ScrollView>("ContextPanelScroll");
            var viewportSummary = GetScrollLayoutSummary("viewport", viewportHost, viewportScroll);
            var contextSummary = GetScrollLayoutSummary("context", root.Q<VisualElement>("ContextPanel"), contextScroll);
            return $"{viewportSummary}; {contextSummary}";
        }

        private static PendantV3InputContract GetInputContract()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var contract = Object.FindFirstObjectByType<PendantV3InputContract>(FindObjectsInactive.Include);
            if (contract == null)
            {
                throw new MissingReferenceException("PendantV3InputContract not found in RobotControlV3 scene.");
            }

            return contract;
        }

        private static JointJogController GetJointJogController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var jointJog = Object.FindFirstObjectByType<JointJogController>(FindObjectsInactive.Include);
            if (jointJog == null)
            {
                throw new MissingReferenceException("JointJogController not found in RobotControlV3 scene.");
            }

            return jointJog;
        }

        private static TcpJogController GetTcpJogController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var tcpJog = Object.FindFirstObjectByType<TcpJogController>(FindObjectsInactive.Include);
            if (tcpJog == null)
            {
                throw new MissingReferenceException("TcpJogController not found in RobotControlV3 scene.");
            }

            return tcpJog;
        }

        private static PointMoveController GetPointMoveController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var pointMove = Object.FindFirstObjectByType<PointMoveController>(FindObjectsInactive.Include);
            if (pointMove == null)
            {
                throw new MissingReferenceException("PointMoveController not found in RobotControlV3 scene.");
            }

            return pointMove;
        }

        private static RobotControlV3RuntimeController GetRuntimeController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            if (runtime == null)
            {
                throw new MissingReferenceException("RobotControlV3RuntimeController not found in RobotControlV3 scene.");
            }

            runtime.ForceInitialize();
            return runtime;
        }

        private static int CountDescendants(VisualElement root)
        {
            var total = 0;
            using var iterator = root.Children().GetEnumerator();
            while (iterator.MoveNext())
            {
                var child = iterator.Current;
                total++;
                total += CountDescendants(child);
            }

            return total;
        }

        private static string GetScrollLayoutSummary(string label, VisualElement host, ScrollView scrollView)
        {
            if (scrollView == null)
            {
                return $"{label}=missing";
            }

            var viewportWidth = scrollView.contentViewport?.worldBound.width ?? 0f;
            var viewportHeight = scrollView.contentViewport?.worldBound.height ?? 0f;
            var contentWidth = scrollView.contentContainer?.worldBound.width ?? 0f;
            var contentHeight = scrollView.contentContainer?.worldBound.height ?? 0f;
            var hostHeight = host?.worldBound.height ?? 0f;
            var scrollShare = hostHeight > 0.1f ? viewportHeight / hostHeight : 0f;
            var horizontalVisible = scrollView.horizontalScroller != null
                && scrollView.horizontalScroller.resolvedStyle.display != DisplayStyle.None;
            var clipped = CountHorizontallyClippedDescendants(
                scrollView.contentContainer,
                scrollView.contentViewport?.worldBound ?? Rect.zero);
            return $"{label}Mode={scrollView.mode}; {label}Viewport={viewportWidth:F1}x{viewportHeight:F1}; {label}Content={contentWidth:F1}x{contentHeight:F1}; {label}ScrollShare={scrollShare:F2}; {label}HorizontalVisible={horizontalVisible}; {label}Clipped={clipped}";
        }

        private static int CountHorizontallyClippedDescendants(VisualElement element, Rect clipBounds)
        {
            if (element == null || clipBounds.width <= 0.1f)
            {
                return 0;
            }

            var total = 0;
            using var iterator = element.Children().GetEnumerator();
            while (iterator.MoveNext())
            {
                var child = iterator.Current;
                if (IsVisibleForLayout(child))
                {
                    var bounds = child.worldBound;
                    if (bounds.width > 0.5f
                        && (bounds.xMin < clipBounds.xMin - 0.5f || bounds.xMax > clipBounds.xMax + 0.5f))
                    {
                        total++;
                    }
                }

                total += CountHorizontallyClippedDescendants(child, clipBounds);
            }

            return total;
        }

        private static bool IsVisibleForLayout(VisualElement element)
        {
            return element.resolvedStyle.display != DisplayStyle.None
                && element.resolvedStyle.visibility != Visibility.Hidden
                && element.worldBound.width > 0.5f
                && element.worldBound.height > 0.5f;
        }

        private static ScrollView GetContextPanelScrollView()
        {
            var contract = GetInputContract();
            var scrollView = contract.GetComponent<UIDocument>()?.rootVisualElement?.Q<ScrollView>("ContextPanelScroll");
            if (scrollView == null)
            {
                throw new MissingReferenceException("ContextPanelScroll not found in RobotControlV3 document.");
            }

            return scrollView;
        }
    }
}
