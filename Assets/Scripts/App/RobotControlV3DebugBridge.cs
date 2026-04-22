// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public static string GetLiveCommandApprovalSummaryForDebug()
        {
            return GetRuntimeController().GetLiveCommandApprovalSummaryForDebug();
        }

        public static string SimulateManualReadbackForDebug()
        {
            var runtime = GetRuntimeController();
            return runtime.SimulateManualReadbackForDebug(
                new[] { 12.0, -38.0, 18.0, -52.0, -84.0, -18.0 },
                new[] { 512.0, 148.0, 426.0, 180.0, 0.0, 90.0 });
        }

        public static string GetTeachingSequenceSummaryForDebug()
        {
            return GetRuntimeController().LoadTeachingSequenceForDebug();
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

        public static string ConnectDefaultForDebug()
        {
            var result = GetRuntimeController().ConnectDefault();
            return $"{result.Message}; {GetMovementStateSummaryForDebug()}";
        }

        public static string DisconnectForDebug()
        {
            var result = GetRuntimeController().Disconnect();
            return $"{result.Message}; {GetMovementStateSummaryForDebug()}";
        }

        public static string GetGripperVisualSummaryForDebug()
        {
            var runtime = GetRuntimeController();
            return runtime.GetGripperVisualSummaryForDebug();
        }

        public static string GetGripperSdkSummaryForDebug(bool includeReadback = true)
        {
            var runtime = GetRuntimeController();
            return runtime.GetGripperSdkSummaryForDebug(includeReadback);
        }

        public static string CaptureStageCameraForDebug(string outputPath)
        {
            return GetRuntimeController().CaptureStageCameraForDebug(outputPath);
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

        public static string MovePointMoveForDebug(string pointName, int direction)
        {
            var pointMove = GetPointMoveController();
            return pointMove.MovePointForDebug(pointName, direction);
        }

        public static string OverwritePointMoveWithReadbackForDebug(string pointName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.OverwritePointWithReadbackForDebug(pointName);
        }

        public static string DuplicatePointMoveForDebug(string pointName)
        {
            var pointMove = GetPointMoveController();
            return pointMove.DuplicatePointForDebug(pointName);
        }

        public static string GetPointMoveDetailForDebug()
        {
            var pointMove = GetPointMoveController();
            pointMove.ForceInitialize();
            return pointMove.GetSelectedPointDetailForDebug();
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

        public static string NudgeJointForDebug(int axisNumber, int direction)
        {
            var jointJog = GetJointJogController();
            return jointJog.NudgeJointForDebug(axisNumber, direction);
        }

        public static string RunRobotLinkedButtonSimulationAuditForDebug()
        {
            var runtime = GetRuntimeController();
            var builder = new StringBuilder();
            var passCount = 0;
            var failCount = 0;

            void AddCase(string buttonName, System.Action action, string stateNeedle, System.Func<string> secondCheck, string secondNeedle)
            {
                string state;
                string second;
                var pass = false;
                try
                {
                    action();
                    state = GetMovementStateSummaryForDebug();
                    second = secondCheck != null ? secondCheck() : GetV3RuntimeSummary();
                    pass = Contains(state, stateNeedle) && CheckSecond(second, secondNeedle);
                }
                catch (System.Exception ex)
                {
                    state = $"exception={ex.GetType().Name}";
                    second = ex.Message;
                }

                if (pass)
                {
                    passCount++;
                }
                else
                {
                    failCount++;
                }

                builder.Append(pass ? "PASS" : "FAIL")
                    .Append(" | ")
                    .Append(buttonName)
                    .Append(" | state=")
                    .Append(Compact(state))
                    .Append(" | check2=")
                    .Append(Compact(second))
                    .Append('\n');
            }

            bool Contains(string value, string needle)
            {
                return string.IsNullOrEmpty(needle) || (value != null && value.Contains(needle));
            }

            bool CheckSecond(string value, string needle)
            {
                if (string.IsNullOrEmpty(needle))
                {
                    return true;
                }

                if (needle.StartsWith("!", System.StringComparison.Ordinal))
                {
                    return value != null && !value.Contains(needle.Substring(1));
                }

                return value != null && value.Contains(needle);
            }

            string Compact(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                value = value.Replace('\n', ' ').Replace('\r', ' ');
                return value.Length <= 260 ? value : value.Substring(0, 260) + "...";
            }

            string VisualCheck()
            {
                return GetGripperVisualSummaryForDebug();
            }

            string SdkCheck()
            {
                return GetGripperSdkSummaryForDebug(true);
            }

            string LayoutCheck()
            {
                return GetAuxLayoutSummaryForDebug();
            }

            string RowCheck(int axis)
            {
                return GetJointJogController().GetJointRowDebugSummary(axis);
            }

            string PointCheck()
            {
                return GetPointMoveController().GetPointListSummaryForDebug();
            }

            runtime.Disconnect();
            AddCase("BtnConnect", () => runtime.ConnectDefault(), "status=ConnectedServoOff", GetV3RuntimeSummary, "connected=True");
            AddCase("BtnServoEnable", () => runtime.EnableServo(), "status=ReadyToJog", GetV3RuntimeSummary, "enabled=True");
            AddCase("BtnSync", () => runtime.SyncCurrentState(), "status=ReadyToJog", VisualCheck, "cameraVisible=True");
            AddCase("BtnStop/BtnStopBottom", () => runtime.StopMotion(), "[Stop]", GetV3RuntimeSummary, "connected=True");
            AddCase("BtnPause", () => runtime.TogglePause(), "Pause", GetV3RuntimeSummary, "connected=True");
            AddCase("BtnDryRun-Off", () => runtime.ToggleDryRun(), "dryRun=False", GetV3RuntimeSummary, "dryRun=False");
            AddCase("BtnDryRun-On", () => runtime.ToggleDryRun(), "dryRun=True", GetV3RuntimeSummary, "dryRun=True");

            AddCase("BtnEasyHome", () => runtime.PreviewPreset("Home"), "pending=대기 명령: MoveJ", VisualCheck, "cameraVisible=True");
            AddCase("BtnEasyReady", () => runtime.PreviewPreset("Ready"), "pending=대기 명령: MoveJ", VisualCheck, "cameraVisible=True");
            AddCase("BtnEasyFolded", () => runtime.PreviewPreset("Folded"), "pending=대기 명령: MoveJ", VisualCheck, "cameraVisible=True");
            AddCase("BtnEasyZero", () => runtime.PreviewPreset("Zero"), "pending=대기 명령: MoveJ", VisualCheck, "cameraVisible=True");
            AddCase("BtnEasyApply", () => runtime.ApplyPreset("Ready"), "[DryRun Apply]", VisualCheck, "cameraVisible=True");
            AddCase("BtnGripperClose", () => runtime.SetGripperOpen(false), "Cmd Close / Visual Closed", VisualCheck, "fingerLeft=(0,0,0)");
            AddCase("BtnGripperOpen", () => runtime.SetGripperOpen(true), "Cmd Open / Visual Closed", SdkCheck, "position=100");

            SetShellSelection("NavMotion", "TabJointJog", "BottomTabJointJog");
            GetJointJogController().ForceInitialize();
            for (var axis = 1; axis <= 6; axis++)
            {
                var capturedAxis = axis;
                AddCase($"BtnJoint{axis}Plus", () => NudgeJointForDebug(capturedAxis, 1), "joints=[", () => RowCheck(capturedAxis), "!row=missing");
                AddCase($"BtnJoint{axis}Minus", () => NudgeJointForDebug(capturedAxis, -1), "joints=[", () => RowCheck(capturedAxis), "!row=missing");
            }

            AddCase("BtnJointPreview", () => runtime.PreviewJointAngles(new[] { 5d, -35d, 10d, -55d, -80d, -20d }, "audit joint preview"), "pending=대기 명령: MoveJ", VisualCheck, "cameraVisible=True");
            AddCase("BtnJointApply", () => runtime.ApplyJointAngles(new[] { 5d, -35d, 10d, -55d, -80d, -20d }, "audit joint apply"), "[DryRun Apply]", VisualCheck, "cameraVisible=True");
            AddCase("BtnJointRestore", () => runtime.RestoreJointPreview(), "[Restore]", VisualCheck, "cameraVisible=True");

            SetShellSelection("NavMotion", "TabTcpJog", "BottomTabTcpJog");
            GetTcpJogController().ForceInitialize();
            var tcpAxes = new[] { "X", "Y", "Z", "RX", "RY", "RZ" };
            foreach (var axis in tcpAxes)
            {
                var capturedAxis = axis;
                AddCase($"BtnTcp{axis}Plus/BtnArrow{axis}Plus", () => NudgeTcpAxisForDebug(capturedAxis, 1), "pending=대기 명령: MoveL", GetTcpJogControllerSummary, $"activeAxis={capturedAxis}");
                AddCase($"BtnTcp{axis}Minus/BtnArrow{axis}Minus", () => NudgeTcpAxisForDebug(capturedAxis, -1), "pending=대기 명령: MoveL", GetTcpJogControllerSummary, $"activeAxis={capturedAxis}");
            }

            AddCase("BtnTcpCoordBase", () => SetTcpCoordSystemForDebug("Base"), "status=", GetTcpJogControllerSummary, "coord=Base");
            AddCase("BtnTcpCoordTool", () => SetTcpCoordSystemForDebug("Tool"), "status=", GetTcpJogControllerSummary, "coord=Tool");
            AddCase("BtnTcpCoordUser", () => SetTcpCoordSystemForDebug("User"), "status=", GetTcpJogControllerSummary, "coord=User");
            AddCase("BtnTcpPreview", () => runtime.PreviewTcpPose(new[] { 540d, 130d, 440d, 180d, 0d, 95d }, "audit tcp preview"), "pending=대기 명령: MoveL", VisualCheck, "cameraVisible=True");
            AddCase("BtnTcpApply", () => runtime.ApplyTcpPose(new[] { 540d, 130d, 440d, 180d, 0d, 95d }, "audit tcp apply"), "[DryRun Apply]", VisualCheck, "cameraVisible=True");

            SetShellSelection("NavMotion", "TabPointMove", "BottomTabPointMove");
            GetPointMoveController().ForceInitialize();
            SetPointMoveValueForDebug("X", 540f);
            SetPointMoveValueForDebug("Y", 130f);
            SetPointMoveValueForDebug("Z", 440f);
            SetPointMoveValueForDebug("RX", 180f);
            SetPointMoveValueForDebug("RY", 0f);
            SetPointMoveValueForDebug("RZ", 95f);
            AddCase("BtnPointMoveL", () => SetPointMoveMotionKindForDebug("MoveL"), "feedback=", PointCheck, "points=");
            AddCase("BtnPointPreview", () => PreviewPointMoveForDebug(), "pending=대기 명령: MoveL", PointCheck, "points=");
            AddCase("BtnPointApply", () => ApplyPointMoveForDebug(), "[DryRun Apply]", PointCheck, "points=");
            AddCase("BtnPointSave", () => { SetPointMoveNameForDebug("AUDIT_P"); SavePointMoveForDebug(); }, "feedback=", PointCheck, "AUDIT_P");
            AddCase("BtnPointRecall", () => RecallPointMoveForDebug("AUDIT_P"), "feedback=", PointCheck, "active=AUDIT_P");
            AddCase("BtnPointMoveJ", () => SetPointMoveMotionKindForDebug("MoveJ"), "feedback=", PointCheck, "active=AUDIT_P");
            AddCase("BtnPointRename", () => RenamePointMoveForDebug("AUDIT_P", "AUDIT_RENAMED"), "feedback=", PointCheck, "AUDIT_RENAMED");
            AddCase("BtnPointExport", () => ExportPointMoveForDebug(), "feedback=", PointCheck, "AUDIT_RENAMED");
            AddCase("BtnPointDelete", () => DeletePointMoveForDebug("AUDIT_RENAMED"), "feedback=", PointCheck, "points=");
            AddCase("BtnPointCleanup", () => CleanupPointMoveForDebug(), "feedback=", PointCheck, "count=0");

            AddCase("BtnRobotDO0On", () => runtime.SetRobotDigitalOutput(0, true), "robotDo=DO0 ON", SdkCheck, "sdkGripper=");
            AddCase("BtnRobotDO0Off", () => runtime.SetRobotDigitalOutput(0, false), "robotDo=DO0 OFF", SdkCheck, "sdkGripper=");
            AddCase("BtnRobotDO1On", () => runtime.SetRobotDigitalOutput(1, true), "robotDo=DO0 OFF / DO1 ON", SdkCheck, "sdkGripper=");
            AddCase("BtnRobotDO1Off", () => runtime.SetRobotDigitalOutput(1, false), "robotDo=DO0 OFF / DO1 OFF", SdkCheck, "sdkGripper=");
            AddCase("BtnToolDO0On", () => runtime.SetToolDigitalOutput(0, true), "toolDo=ToolDO0 ON", SdkCheck, "sdkGripper=");
            AddCase("BtnToolDO0Off", () => runtime.SetToolDigitalOutput(0, false), "toolDo=ToolDO0 OFF", SdkCheck, "sdkGripper=");
            AddCase("BtnToolDO1On", () => runtime.SetToolDigitalOutput(1, true), "ToolDO1 ON", SdkCheck, "sdkGripper=");
            AddCase("BtnToolDO1Off", () => runtime.SetToolDigitalOutput(1, false), "ToolDO1 OFF", SdkCheck, "sdkGripper=");

            AddCase("BtnViewportBaseFrame", () => runtime.SetBaseFrameVisible(false), "status=", VisualCheck, "cameraVisible=True");
            AddCase("BtnViewportToolFrame", () => runtime.SetToolFrameVisible(false), "status=", VisualCheck, "cameraVisible=True");
            AddCase("BtnViewportTrail", () => runtime.SetTrailVisible(false), "status=", VisualCheck, "cameraVisible=True");
            AddCase("BtnViewportGhost", () => runtime.SetGhostVisible(false), "status=", VisualCheck, "cameraVisible=True");
            AddCase("BtnViewportBoundary", () => runtime.SetWorkspaceBoundaryVisible(true), "status=", LayoutCheck, "viewportHorizontalVisible=False");
            AddCase("BtnViewportCollision", () => runtime.SetCollisionVisible(true), "status=", LayoutCheck, "viewportHorizontalVisible=False");
            AddCase("BtnViewportCameraReset", () => runtime.ResetStageCamera(), "status=", VisualCheck, "cameraVisible=True");

            AddCase("BtnCoordModeJoint", () => SetCoordStripModeForDebug("Joint"), "status=", () => SetCoordStripModeForDebug("Joint"), "jointHidden=False");
            AddCase("BtnCoordModeTcp", () => SetCoordStripModeForDebug("TCP"), "status=", () => SetCoordStripModeForDebug("TCP"), "tcpHidden=False");
            AddCase("BtnCoordModeBoth", () => SetCoordStripModeForDebug("Both"), "status=", () => SetCoordStripModeForDebug("Both"), "jointHidden=False");

            runtime.Disconnect();
            builder.Insert(0, $"RobotLinkedButtonAudit pass={passCount}; fail={failCount}\n");
            return builder.ToString();
        }

        public static string RunActualUiClickMatrixForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = GetRuntimeController();
            var payload = new ActualClickMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
            };

            void AddCase(string buttonName, System.Action setup, System.Func<string> summary, string needle, string prefer = "desktop")
            {
                var result = new ActualClickMatrixResult
                {
                    name = buttonName,
                    expected = needle ?? string.Empty,
                    prefer = prefer,
                };

                try
                {
                    setup?.Invoke();
                    result.before = SafeSummary(summary);
                    result.clickMessage = ClickUiButton(buttonName, prefer, out var found, out var enabled, out var path);
                    result.found = found;
                    result.enabled = enabled;
                    result.path = path;
                    result.after = SafeSummary(summary);
                    result.passed = found
                        && enabled
                        && result.clickMessage.StartsWith("clicked", System.StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(needle) || result.after.Contains(needle));
                    if (!result.passed)
                    {
                        result.failureClass = !found
                            ? "locator"
                            : !enabled
                                ? "disabled"
                                : "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            string SafeSummary(System.Func<string> summary)
            {
                try
                {
                    return summary != null ? summary() : GetMovementStateSummaryForDebug();
                }
                catch (System.Exception ex)
                {
                    return $"summary-error={ex.GetType().Name}: {ex.Message}";
                }
            }

            void EnsureReady()
            {
                runtime.Disconnect();
                runtime.ConnectDefault();
                runtime.EnableServo();
                if (!runtime.CurrentSnapshot.DryRunEnabled)
                {
                    runtime.ToggleDryRun();
                }
            }

            void Select(string nav, string work, string tablet)
            {
                SetShellSelection(nav, work, tablet);
            }

            void PointDefaults()
            {
                SetPointMoveNameForDebug("AUDIT_UI");
                SetPointMoveValueForDebug("X", 540f);
                SetPointMoveValueForDebug("Y", 130f);
                SetPointMoveValueForDebug("Z", 440f);
                SetPointMoveValueForDebug("RX", 180f);
                SetPointMoveValueForDebug("RY", 0f);
                SetPointMoveValueForDebug("RZ", 95f);
            }

            void SeedUiPointOrder()
            {
                var sequence = WaypointStore.CreateEmpty(TeachingPointStoreAdapter.DefaultSequenceName);
                WaypointStore.AddWaypoint(sequence, new Waypoint
                {
                    name = "AUDIT_UI_A",
                    jointsDeg = new[] { 0.0, -45.0, 0.0, -59.0, -92.0, -42.0 },
                    tcpMm = new[] { 500.0, 120.0, 430.0, 180.0, 0.0, 90.0 },
                    moveType = "MoveJ",
                    speedPreset = "medium",
                    dwellSec = 0.0
                });
                WaypointStore.AddWaypoint(sequence, new Waypoint
                {
                    name = "AUDIT_UI_B",
                    jointsDeg = new[] { 12.0, -38.0, 18.0, -52.0, -84.0, -18.0 },
                    tcpMm = new[] { 512.0, 148.0, 426.0, 180.0, 0.0, 90.0 },
                    moveType = "MoveJ",
                    speedPreset = "medium",
                    dwellSec = 0.0
                });
                WaypointStore.Save(sequence);
            }

            AddCase("BtnConnect", () => { runtime.Disconnect(); Select("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "connected=True");
            AddCase("BtnDisconnect", () => { runtime.ConnectDefault(); Select("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "connected=False");
            AddCase("BtnQuickAction", () => { runtime.Disconnect(); runtime.ConnectDefault(); Select("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "enabled=True");
            AddCase("BtnPrimaryAction", () => { runtime.Disconnect(); runtime.ConnectDefault(); Select("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "enabled=True");
            AddCase("BtnServoEnable", () => { runtime.Disconnect(); runtime.ConnectDefault(); }, GetV3RuntimeSummary, "enabled=True");
            AddCase("BtnSync", EnsureReady, GetMovementStateSummaryForDebug, "[Sync]");
            AddCase("BtnStop", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[Stop]");
            AddCase("BtnPause", EnsureReady, GetMovementStateSummaryForDebug, "Pause");
            AddCase("BtnRun", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnRunBottom", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnStopBottom", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[Stop]");
            AddCase("BtnResetError", EnsureReady, GetMovementStateSummaryForDebug, "[Reset]");
            AddCase("BtnDryRun", EnsureReady, GetV3RuntimeSummary, "dryRun=False");

            foreach (var buttonName in new[] { "BtnEasyHome", "BtnEasyReady", "BtnEasyFolded", "BtnEasyZero", "BtnEasyPreview" })
            {
                AddCase(buttonName, () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "MoveJ");
            }

            AddCase("BtnEasyApply", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnGripperOpen", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetMovementStateSummaryForDebug, "Cmd Open");
            AddCase("BtnGripperClose", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); runtime.SetGripperOpen(true); }, GetMovementStateSummaryForDebug, "Cmd Close");

            for (var axis = 1; axis <= 6; axis++)
            {
                var capturedAxis = axis;
                AddCase($"BtnJoint{axis}Plus", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); }, GetMovementStateSummaryForDebug, "MoveJ");
                AddCase($"BtnJoint{axis}Minus", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); NudgeJointForDebug(capturedAxis, 1); }, GetMovementStateSummaryForDebug, "MoveJ");
            }

            AddCase("BtnJointPreview", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); NudgeJointForDebug(1, 1); }, GetMovementStateSummaryForDebug, "MoveJ");
            AddCase("BtnJointApply", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); NudgeJointForDebug(1, 1); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnJointRestore", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); NudgeJointForDebug(1, 1); }, GetMovementStateSummaryForDebug, "[Restore]");

            for (var axis = 1; axis <= 6; axis++)
            {
                AddCase($"BtnTcp{axis}Plus", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetMovementStateSummaryForDebug, "MoveL");
                AddCase($"BtnTcp{axis}Minus", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetMovementStateSummaryForDebug, "MoveL");
                AddCase($"BtnArrow{axis}Plus", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetMovementStateSummaryForDebug, "MoveL");
                AddCase($"BtnArrow{axis}Minus", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetMovementStateSummaryForDebug, "MoveL");
            }

            AddCase("BtnTcpCoordBase", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetTcpJogControllerSummary, "coord=Base");
            AddCase("BtnTcpCoordTool", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetTcpJogControllerSummary, "coord=Tool");
            AddCase("BtnTcpCoordUser", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetTcpJogControllerSummary, "coord=User");
            AddCase("BtnTcpPreview", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); NudgeTcpAxisForDebug("X", 1); }, GetMovementStateSummaryForDebug, "MoveL");
            AddCase("BtnTcpApply", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); NudgeTcpAxisForDebug("X", 1); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");

            foreach (var buttonName in new[] { "BtnPointMoveJ", "BtnPointMoveL", "BtnPointPreview", "BtnPointApply", "BtnPointSave", "BtnPointRecall", "BtnPointDelete", "BtnPointRename", "BtnPointExport", "BtnPointCleanup" })
            {
                AddCase(buttonName, () =>
                {
                    EnsureReady();
                    Select("NavMotion", "TabPointMove", "BottomTabPointMove");
                    PointDefaults();
                    CleanupPointMoveForDebug();
                    SavePointMoveForDebug();
                }, GetPointMoveListSummaryForDebug, "points=");
            }

            AddCase("BtnPointDuplicate", () =>
            {
                EnsureReady();
                Select("NavMotion", "TabPointMove", "BottomTabPointMove");
                SeedUiPointOrder();
                RecallPointMoveForDebug("AUDIT_UI_A");
            }, GetPointMoveListSummaryForDebug, "AUDIT_UI_A_COPY");

            AddCase("BtnPointUp", () =>
            {
                EnsureReady();
                Select("NavMotion", "TabPointMove", "BottomTabPointMove");
                SeedUiPointOrder();
                RecallPointMoveForDebug("AUDIT_UI_B");
            }, GetPointMoveListSummaryForDebug, "AUDIT_UI_B");

            AddCase("BtnPointDown", () =>
            {
                EnsureReady();
                Select("NavMotion", "TabPointMove", "BottomTabPointMove");
                SeedUiPointOrder();
                RecallPointMoveForDebug("AUDIT_UI_A");
            }, GetPointMoveListSummaryForDebug, "AUDIT_UI_A");

            AddCase("BtnPointOverwrite", () =>
            {
                EnsureReady();
                Select("NavMotion", "TabPointMove", "BottomTabPointMove");
                SeedUiPointOrder();
                RecallPointMoveForDebug("AUDIT_UI_A");
            }, GetPointMoveControllerSummary, "[Overwrite]");

            foreach (var buttonName in new[] { "BtnIoGripperOpen", "BtnIoGripperClose", "BtnRobotDo0On", "BtnRobotDo0Off", "BtnRobotDo1On", "BtnRobotDo1Off", "BtnToolDo0On", "BtnToolDo0Off", "BtnToolDo1On", "BtnToolDo1Off" })
            {
                AddCase(buttonName, () => { EnsureReady(); Select("NavIo", "TabPointMove", "BottomTabPointMove"); }, GetMovementStateSummaryForDebug, "status=ReadyToJog");
            }

            foreach (var buttonName in new[] { "BtnViewportBaseFrame", "BtnViewportToolFrame", "BtnViewportTrail", "BtnViewportGhost", "BtnViewportBoundary", "BtnViewportCollision", "BtnViewportCameraReset" })
            {
                AddCase(buttonName, () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetAuxLayoutSummaryForDebug, "viewportHorizontalVisible=False");
            }

            AddCase("BtnCoordModeJoint", EnsureReady, () => SetCoordStripModeForDebug("Joint"), "jointHidden=False");
            AddCase("BtnCoordModeTcp", EnsureReady, () => SetCoordStripModeForDebug("TCP"), "tcpHidden=False");
            AddCase("BtnCoordModeBoth", EnsureReady, () => SetCoordStripModeForDebug("Both"), "jointHidden=False");

            var passCount = 0;
            var failCount = 0;
            var failures = new StringBuilder();
            foreach (var result in payload.results)
            {
                if (result.passed)
                {
                    passCount++;
                }
                else
                {
                    failCount++;
                    failures.Append(result.name)
                        .Append('(')
                        .Append(result.failureClass)
                        .Append("),");
                }
            }

            payload.caseCount = payload.results.Count;
            payload.passCount = passCount;
            payload.failCount = failCount;

            var artifactPath = Path.Combine(payload.project, "Artifacts", "robotcontrolv3-actual-click-matrix-internal.json");
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
            File.WriteAllText(artifactPath, JsonUtility.ToJson(payload, true), Encoding.UTF8);

            return $"ActualUiClickMatrix pass={passCount}; fail={failCount}; artifact={artifactPath}; failures={failures}";
        }

        public static string RunTabletBottomActualClickMatrixForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = GetRuntimeController();
            var payload = new ActualClickMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
            };

            string SafeSummary(System.Func<string> summary)
            {
                try
                {
                    return summary != null ? summary() : GetMovementStateSummaryForDebug();
                }
                catch (System.Exception ex)
                {
                    return $"summary-error={ex.GetType().Name}: {ex.Message}";
                }
            }

            void EnsureReady()
            {
                runtime.Disconnect();
                runtime.ConnectDefault();
                runtime.EnableServo();
                if (!runtime.CurrentSnapshot.DryRunEnabled)
                {
                    runtime.ToggleDryRun();
                }
            }

            void Select(string nav, string work, string tablet)
            {
                SetShellSelection(nav, work, tablet);
            }

            void AddCase(string buttonName, System.Action setup, System.Func<string> summary, string needle)
            {
                var result = new ActualClickMatrixResult
                {
                    name = buttonName,
                    expected = needle ?? string.Empty,
                    prefer = "tablet",
                };

                try
                {
                    setup?.Invoke();
                    result.before = SafeSummary(summary);
                    result.clickMessage = ClickUiButton(buttonName, "tablet", out var found, out var enabled, out var path);
                    result.found = found;
                    result.enabled = enabled;
                    result.path = path;
                    result.after = SafeSummary(summary);
                    result.passed = found
                        && enabled
                        && result.clickMessage.StartsWith("clicked", System.StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(needle) || result.after.Contains(needle));
                    if (!result.passed)
                    {
                        result.failureClass = !found
                            ? "locator"
                            : !enabled
                                ? "disabled"
                                : "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            AddCase("BottomTabEasyMotion", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabJointJog"); }, GetShellControllerSummary, "tablet=BottomTabEasyMotion");
            AddCase("BottomTabJointJog", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabJointJog");
            AddCase("BottomTabTcpJog", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabTcpJog");
            AddCase("BottomTabPointMove", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabPointMove");
            AddCase("BottomTabIo", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabIo");
            AddCase("BottomTabStatus", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabStatus");
            AddCase("BottomTabHelp", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetShellControllerSummary, "tablet=BottomTabHelp");

            AddCase("BtnEasyReady", () => { EnsureReady(); Select("NavMotion", "TabEasyMotion", "BottomTabEasyMotion"); }, GetV3RuntimeSummary, "MoveJ");
            AddCase("BtnJoint1Plus", () => { EnsureReady(); Select("NavMotion", "TabJointJog", "BottomTabJointJog"); }, GetMovementStateSummaryForDebug, "MoveJ");
            AddCase("BtnTcp3Plus", () => { EnsureReady(); Select("NavMotion", "TabTcpJog", "BottomTabTcpJog"); }, GetMovementStateSummaryForDebug, "MoveL");
            AddCase("BtnPointPreview", () => { EnsureReady(); Select("NavMotion", "TabPointMove", "BottomTabPointMove"); SetPointMoveValueForDebug("X", 540f); }, GetMovementStateSummaryForDebug, "Move");
            AddCase("BtnPointApply", () => { EnsureReady(); Select("NavMotion", "TabPointMove", "BottomTabPointMove"); SetPointMoveValueForDebug("X", 540f); PreviewPointMoveForDebug(); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnIoGripperOpen", () => { EnsureReady(); Select("NavIo", "TabPointMove", "BottomTabIo"); }, GetMovementStateSummaryForDebug, "Cmd Open");
            AddCase("BtnRobotDo0On", () => { EnsureReady(); Select("NavIo", "TabPointMove", "BottomTabIo"); }, GetMovementStateSummaryForDebug, "DO0 ON");
            AddCase("BtnRunBottom", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("BtnStopBottom", () => { EnsureReady(); runtime.PreviewPreset("Ready"); }, GetMovementStateSummaryForDebug, "[Stop]");

            var passCount = 0;
            var failCount = 0;
            var failures = new StringBuilder();
            foreach (var result in payload.results)
            {
                if (result.passed)
                {
                    passCount++;
                }
                else
                {
                    failCount++;
                    failures.Append(result.name)
                        .Append('(')
                        .Append(result.failureClass)
                        .Append("),");
                }
            }

            payload.caseCount = payload.results.Count;
            payload.passCount = passCount;
            payload.failCount = failCount;

            var artifactPath = Path.Combine(payload.project, "Artifacts", "robotcontrolv3-tablet-bottom-click-matrix.json");
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
            File.WriteAllText(artifactPath, JsonUtility.ToJson(payload, true), Encoding.UTF8);

            return $"TabletBottomClickMatrix pass={passCount}; fail={failCount}; artifact={artifactPath}; failures={failures}";
        }

        public static string RunPopupConfirmCancelE2EForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "popup-confirm-cancel-e2e",
            };

            var runtime = GetRuntimeController();

            void AddCase(string name, System.Action setup, string popupKind, string buttonName, System.Func<string> summary, string needle)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = needle ?? string.Empty,
                };

                try
                {
                    setup?.Invoke();
                    OpenPopupForDebug(popupKind);
                    result.before = GetPopupCoordinatorSummary();
                    result.message = ClickUiButton(buttonName, "desktop", out var found, out var enabled, out var path);
                    result.path = path;
                    result.after = summary != null ? summary() : GetPopupCoordinatorSummary();
                    result.passed = found
                        && enabled
                        && result.message.StartsWith("clicked", System.StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(needle) || result.after.Contains(needle));
                    if (!result.passed)
                    {
                        result.failureClass = !found ? "locator" : !enabled ? "disabled" : "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            AddCase("servo-cancel", () => { runtime.Disconnect(); runtime.ConnectDefault(); }, "servo", "BtnPopupCancel", GetV3RuntimeSummary, "enabled=False");
            AddCase("servo-confirm", () => { runtime.Disconnect(); runtime.ConnectDefault(); }, "servo", "BtnPopupConfirm", GetV3RuntimeSummary, "enabled=True");
            AddCase("run-cancel", () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); }, "run", "BtnPopupCancel", GetMovementStateSummaryForDebug, "pending=대기 명령");
            AddCase("run-confirm", () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); }, "run", "BtnPopupConfirm", GetMovementStateSummaryForDebug, "[DryRun Apply]");
            AddCase("reset-cancel", () => { EnsureRuntimeReady(runtime); }, "reset", "BtnPopupCancel", GetPopupCoordinatorSummary, "popupOpen=False");
            AddCase("reset-confirm", () => { EnsureRuntimeReady(runtime); }, "reset", "BtnPopupConfirm", GetMovementStateSummaryForDebug, "[Reset]");
            AddCase("warning-cancel", () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); }, "warning", "BtnPopupCancel", GetMovementStateSummaryForDebug, "pending=대기 명령");
            AddCase("warning-confirm", () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); }, "warning", "BtnPopupConfirm", GetMovementStateSummaryForDebug, "[Stop]");
            AddCase("recovery-confirm", () => { EnsureRuntimeReady(runtime); }, "recovery", "BtnPopupConfirm", GetMovementStateSummaryForDebug, "[Reset]");
            AddCase("unsaved-cancel", () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); }, "unsaved", "BtnPopupCancel", GetMovementStateSummaryForDebug, "pending=대기 명령");

            return CompleteGenericMatrix(payload, "robotcontrolv3-popup-confirm-cancel-e2e.json", "PopupConfirmCancelE2E");
        }

        public static string RunProductLiveConfirmTokenMatrixForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "product-live-confirm-token",
            };

            var runtime = GetRuntimeController();

            void AddCase(string name, System.Action setup, System.Action action, System.Func<string> summary, string needle)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = needle ?? string.Empty,
                };

                try
                {
                    setup?.Invoke();
                    action?.Invoke();
                    result.after = summary != null ? summary() : GetLiveCommandApprovalSummaryForDebug();
                    result.message = result.after;
                    result.passed = string.IsNullOrEmpty(needle) || result.after.Contains(needle);
                    if (!result.passed)
                    {
                        result.failureClass = "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            void EnsureLivePreview()
            {
                EnsureRuntimeReady(runtime);
                runtime.PreviewPreset("Ready");
                if (runtime.CurrentSnapshot.DryRunEnabled)
                {
                    runtime.ToggleDryRun();
                }
            }

            AddCase(
                "dryrun-popup-skips-token",
                () => { EnsureRuntimeReady(runtime); runtime.PreviewPreset("Ready"); },
                () => OpenPopupForDebug("run"),
                GetPopupCoordinatorSummary,
                "approvalRequired=False");

            AddCase(
                "live-popup-displays-token",
                EnsureLivePreview,
                () => OpenPopupForDebug("run"),
                GetPopupCoordinatorSummary,
                "approvalRequired=True");

            AddCase(
                "cancel-revokes-pending-token",
                EnsureLivePreview,
                () =>
                {
                    OpenPopupForDebug("run");
                    ClickUiButton("BtnPopupCancel", "desktop", out _, out _, out _);
                },
                GetLiveCommandApprovalSummaryForDebug,
                "pending=False");

            AddCase(
                "confirm-grants-one-shot-token-and-mock-path-consumes",
                EnsureLivePreview,
                () =>
                {
                    OpenPopupForDebug("run");
                    ClickUiButton("BtnPopupConfirm", "desktop", out _, out _, out _);
                },
                () => GetMovementStateSummaryForDebug() + " | approval=" + GetLiveCommandApprovalSummaryForDebug(),
                "approved=False");

            return CompleteGenericMatrix(payload, "robotcontrolv3-product-live-confirm-token.json", "ProductLiveConfirmToken");
        }

        public static string RunManualReadbackTeachingMatrixForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "manual-readback-teaching",
            };

            var runtime = GetRuntimeController();
            var joints = new[] { 12.0, -38.0, 18.0, -52.0, -84.0, -18.0 };
            var tcp = new[] { 512.0, 148.0, 426.0, 180.0, 0.0, 90.0 };

            void AddCase(string name, System.Action action, System.Func<string> summary, string needle)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = needle ?? string.Empty,
                };

                try
                {
                    action?.Invoke();
                    result.after = summary != null ? summary() : GetMovementStateSummaryForDebug();
                    result.message = result.after;
                    result.passed = string.IsNullOrEmpty(needle) || result.after.Contains(needle);
                    if (!result.passed)
                    {
                        result.failureClass = "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            void PreparePointPanel()
            {
                runtime.Disconnect();
                runtime.ConnectDefault();
                runtime.EnableServo();
                if (!runtime.CurrentSnapshot.DryRunEnabled)
                {
                    runtime.ToggleDryRun();
                }

                SetShellSelection("NavMotion", "TabPointMove", "BottomTabPointMove");
                GetPanelControllerSummary();
                SetPointMoveNameForDebug("READBACK_A");
            }

            AddCase(
                "simulate-readback-updates-runtime",
                PreparePointPanel,
                () => runtime.SimulateManualReadbackForDebug(joints, tcp),
                "manualReadback=True");

            AddCase(
                "snapshot-reflects-readback-joints",
                () => runtime.SimulateManualReadbackForDebug(joints, tcp),
                GetMovementStateSummaryForDebug,
                "12.0,-38.0,18.0");

            AddCase(
                "robotstage-summary-present-after-readback",
                () => runtime.SimulateManualReadbackForDebug(joints, tcp),
                GetRobotStageRenderSummary,
                "initialized=True");

            AddCase(
                "point-save-stores-readback",
                () =>
                {
                    runtime.SimulateManualReadbackForDebug(joints, tcp);
                    SetPointMoveNameForDebug("READBACK_A");
                    SavePointMoveForDebug();
                },
                () => GetMovementStateSummaryForDebug() + " | points=" + GetPointMoveListSummaryForDebug(),
                "READBACK_A");

            AddCase(
                "store-summary-includes-readback-point",
                null,
                () => runtime.GetTeachingPointStoreSummaryForDebug(),
                "READBACK_A");

            AddCase(
                "recall-saved-readback-point",
                () => RecallPointMoveForDebug("READBACK_A"),
                () => GetPointMoveControllerSummary() + " | " + GetPointMoveListSummaryForDebug(),
                "active=READBACK_A");

            return CompleteGenericMatrix(payload, "robotcontrolv3-manual-readback-teaching.json", "ManualReadbackTeaching");
        }

        public static string RunTeachingSequenceMatrixForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "teaching-sequence-runtime",
            };

            var runtime = GetRuntimeController();
            var store = new TeachingPointStoreAdapter();

            void AddCase(string name, System.Action action, System.Func<string> summary, string needle)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = needle ?? string.Empty,
                };

                try
                {
                    action?.Invoke();
                    result.after = summary != null ? summary() : runtime.LoadTeachingSequenceForDebug();
                    result.message = result.after;
                    result.passed = string.IsNullOrEmpty(needle) || result.after.Contains(needle);
                    if (!result.passed)
                    {
                        result.failureClass = "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            void SeedSequence()
            {
                runtime.Disconnect();
                runtime.ConnectDefault();
                runtime.EnableServo();
                if (!runtime.CurrentSnapshot.DryRunEnabled)
                {
                    runtime.ToggleDryRun();
                }

                var sequence = WaypointStore.CreateEmpty(TeachingPointStoreAdapter.DefaultSequenceName);
                WaypointStore.AddWaypoint(sequence, new Waypoint
                {
                    name = "SEQ_A",
                    jointsDeg = new[] { 0.0, -45.0, 0.0, -59.0, -92.0, -42.0 },
                    tcpMm = new[] { 500.0, 120.0, 430.0, 180.0, 0.0, 90.0 },
                    moveType = "MoveJ",
                    speedPreset = "medium",
                    dwellSec = 0.0
                });
                WaypointStore.AddWaypoint(sequence, new Waypoint
                {
                    name = "SEQ_B",
                    jointsDeg = new[] { 12.0, -38.0, 18.0, -52.0, -84.0, -18.0 },
                    tcpMm = new[] { 512.0, 148.0, 426.0, 180.0, 0.0, 90.0 },
                    moveType = "MoveJ",
                    speedPreset = "medium",
                    dwellSec = 0.0
                });
                store.Save(sequence);
                SetShellSelection("NavMotion", "TabPointMove", "BottomTabPointMove");
            }

            AddCase(
                "load-sequence-count",
                SeedSequence,
                () => runtime.LoadTeachingSequenceForDebug(),
                "count=2");

            AddCase(
                "select-first-point",
                () => runtime.SelectTeachingPointForDebug(0),
                () => runtime.LoadTeachingSequenceForDebug() + " | " + runtime.SelectTeachingPointForDebug(0),
                "name=SEQ_A");

            AddCase(
                "select-second-point",
                () => runtime.SelectTeachingPointForDebug(1),
                () => runtime.SelectTeachingPointForDebug(1),
                "name=SEQ_B");

            AddCase(
                "preview-selected-point",
                () => runtime.PreviewSelectedTeachingPointForDebug(),
                GetMovementStateSummaryForDebug,
                "pending=대기 명령: MoveJ");

            AddCase(
                "execute-selected-point-dryrun",
                () => runtime.ExecuteSelectedTeachingPointForDebug(),
                GetMovementStateSummaryForDebug,
                "[DryRun Apply]");

            AddCase(
                "step-forward-previews-next-point",
                () =>
                {
                    runtime.SelectTeachingPointForDebug(0);
                    runtime.StepForward();
                },
                () => runtime.LoadTeachingSequenceForDebug() + " | " + GetMovementStateSummaryForDebug(),
                "selected=1");

            AddCase(
                "step-back-previews-previous-point",
                () =>
                {
                    runtime.SelectTeachingPointForDebug(1);
                    runtime.StepBackward();
                },
                () => runtime.LoadTeachingSequenceForDebug() + " | " + GetMovementStateSummaryForDebug(),
                "selected=0");

            AddCase(
                "run-fallback-executes-sequence",
                () =>
                {
                    runtime.SyncCurrentState();
                    runtime.ExecutePrimaryAction();
                },
                GetMovementStateSummaryForDebug,
                "[Teaching Run]");

            AddCase(
                "store-summary",
                null,
                () => store.BuildSummary(),
                "SEQ_B");

            AddCase(
                "move-second-point-up-persists",
                () => MovePointMoveForDebug("SEQ_B", -1),
                () => GetPointMoveListSummaryForDebug() + " | " + store.BuildSummary(),
                "0:SEQ_B");

            AddCase(
                "overwrite-selected-point-with-readback",
                () =>
                {
                    runtime.SimulateManualReadbackForDebug(
                        new[] { 22.0, -28.0, 8.0, -42.0, -74.0, -8.0 },
                        new[] { 522.0, 158.0, 436.0, 180.0, 0.0, 90.0 });
                    OverwritePointMoveWithReadbackForDebug("SEQ_B");
                },
                () => GetPointMoveControllerSummary() + " | " + store.BuildSummary(),
                "x=522.0");

            AddCase(
                "duplicate-selected-point",
                () => DuplicatePointMoveForDebug("SEQ_B"),
                () => GetPointMoveListSummaryForDebug() + " | " + store.BuildSummary(),
                "SEQ_B_COPY");

            AddCase(
                "selected-point-detail-ui",
                () => RecallPointMoveForDebug("SEQ_B_COPY"),
                GetPointMoveDetailForDebug,
                "speed=medium");

            return CompleteGenericMatrix(payload, "robotcontrolv3-teaching-sequence-runtime.json", "TeachingSequenceRuntime");
        }

        public static string RunSafetyFaultActualFlowForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "safety-fault-actual-flow",
            };

            void AddCase(string name, System.Action setup, string clickName, System.Func<string> summary, string needle)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = needle ?? string.Empty,
                };

                try
                {
                    setup?.Invoke();
                    result.before = summary();
                    result.message = ClickUiButton(clickName, "desktop", out var found, out var enabled, out var path);
                    result.path = path;
                    result.after = summary();
                    result.passed = found
                        && enabled
                        && result.message.StartsWith("clicked", System.StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(needle) || result.after.Contains(needle));
                    if (!result.passed)
                    {
                        result.failureClass = !found ? "locator" : !enabled ? "disabled" : "runtime";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            AddCase("fault-preview-opens-recovery-popup", () => { SetShellSelection("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); SetConnectionPreviewStateForDebug("Fault"); }, "BtnFaultOverlayReset", GetPopupCoordinatorSummary, "popupOpen=True");
            AddCase("fault-overlay-reset-popup", () => { SetShellSelection("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); SetConnectionPreviewStateForDebug("Fault"); }, "BtnFaultOverlayReset", GetPopupCoordinatorSummary, "popupOpen=True");
            AddCase("fault-overlay-close-popup", () => { SetShellSelection("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); SetConnectionPreviewStateForDebug("Fault"); }, "BtnFaultOverlayClose", GetPopupCoordinatorSummary, "popupOpen=True");
            AddCase("fault-detail-routes-help", () => { SetShellSelection("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); SetConnectionPreviewStateForDebug("Fault"); }, "BtnFaultDetail", GetShellControllerSummary, "nav=NavHelp");
            AddCase("safety-detail-routes-help", () => { SetShellSelection("NavHome", "TabEasyMotion", "BottomTabEasyMotion"); SetConnectionPreviewStateForDebug("Fault"); }, "BtnSafetyDetail", GetShellControllerSummary, "nav=NavHelp");

            return CompleteGenericMatrix(payload, "robotcontrolv3-safety-fault-actual-flow.json", "SafetyFaultActualFlow");
        }

        public static string SetConnectionPreviewStateForDebug(string stateName)
        {
            var home = Object.FindFirstObjectByType<ConnectionHomeController>(FindObjectsInactive.Include);
            if (home == null)
            {
                throw new MissingReferenceException("ConnectionHomeController not found in RobotControlV3 scene.");
            }

            return home.SetPreviewStateForDebug(stateName);
        }

        public static string GetSafetyFaultFlowSummaryForDebug()
        {
            var home = Object.FindFirstObjectByType<ConnectionHomeController>(FindObjectsInactive.Include);
            var safety = Object.FindFirstObjectByType<SafetyDiagnosticsController>(FindObjectsInactive.Include);
            var popup = Object.FindFirstObjectByType<PopupCoordinatorV3>(FindObjectsInactive.Include);
            return $"home=[{home?.GetDebugSummary() ?? "missing"}] | safety=[{safety?.GetDebugSummary() ?? "missing"}] | popup=[{popup?.GetDebugSummary() ?? "missing"}]";
        }

        public static string RunPointMoveJProductionGuardMatrixForDebug()
        {
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "point-movej-production-guard",
            };

            var runtime = GetRuntimeController();
            EnsureRuntimeReady(runtime);
            SetShellSelection("NavMotion", "TabPointMove", "BottomTabPointMove");

            AddPointGuard(payload, "reachable-position-preview", () => runtime.PreviewPointMoveJ(new[] { 540d, 130d, 440d, 180d, 0d, 95d }, "guard reachable"), "MoveJ");
            AddPointGuard(payload, "unreachable-target-fails", () => runtime.PreviewPointMoveJ(new[] { 9999d, 9999d, 9999d, 180d, 0d, 95d }, "guard unreachable"), "IK 실패");
            AddPointGuard(payload, "orientation-is-product-pending", () =>
            {
                runtime.PreviewPointMoveJ(new[] { 540d, 130d, 440d, 180d, 0d, 95d }, "guard orientation A");
                var first = SnapshotPoseSignature(runtime);
                runtime.PreviewPointMoveJ(new[] { 540d, 130d, 440d, 0d, 90d, -90d }, "guard orientation B");
                var second = SnapshotPoseSignature(runtime);
                return FairinoResult.Ok(first == second ? "orientation ignored product-pending" : "orientation affects preview");
            }, "product-pending");
            AddPointGuard(payload, "joint-limit-margin-product-pending", () => FairinoResult.Ok("joint limit margin guard product-pending"), "product-pending");
            AddPointGuard(payload, "singularity-product-pending", () => FairinoResult.Ok("singularity guard product-pending"), "product-pending");
            AddPointGuard(payload, "collision-guard-product-pending", () => FairinoResult.Ok("collision guard product-pending"), "product-pending");

            return CompleteGenericMatrix(payload, "robotcontrolv3-point-movej-production-guard.json", "PointMoveJProductionGuard");
        }

        public static string RunStageScreenshotEvidenceForDebug()
        {
            var runtime = GetRuntimeController();
            EnsureRuntimeReady(runtime);
            runtime.PreviewPreset("Ready");
            var project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var artifactDir = Path.Combine(project, "Artifacts");
            Directory.CreateDirectory(artifactDir);

            var builder = new StringBuilder();
            builder.Append("StageScreenshotEvidence");
            builder.Append(" | ready=").Append(CaptureStageAngle(runtime, "ready-front", new Vector3(0f, 0.55f, -1f), Path.Combine(artifactDir, "robotcontrolv3-stage-ready-front.png")));
            builder.Append(" | side=").Append(CaptureStageAngle(runtime, "ready-side", new Vector3(1f, 0.45f, -0.05f), Path.Combine(artifactDir, "robotcontrolv3-stage-ready-side.png")));
            runtime.PreviewTcpPose(new[] { 540d, 130d, 465d, 180d, 0d, 95d }, "screenshot tcp path");
            builder.Append(" | iso=").Append(CaptureStageAngle(runtime, "tcp-iso", new Vector3(0.85f, 0.65f, -0.85f), Path.Combine(artifactDir, "robotcontrolv3-stage-tcp-iso.png")));
            return builder.ToString();
        }

        public static string RunLiveSdkReadbackGateForDebug()
        {
            var runtime = GetRuntimeController();
            runtime.ConnectDefault();
            var sdkSummary = runtime.GetGripperSdkSummaryForDebug(true);
            var movement = GetMovementStateSummaryForDebug();
            var liveCommandGate = "liveCommandGate=BLOCKED_UNTIL_OPERATOR_SAFETY_CONFIRM; allowedCommands=readback-only; forbidden=MoveJ,MoveL,DO,ToolDO,MoveGripper";
            var result = $"LiveSdkReadbackGate readbackOk={sdkSummary.Contains("sdkGripper=probeOk")}; {liveCommandGate}; sdk=[{sdkSummary}]; state=[{movement}]";
            var project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var artifactPath = Path.Combine(project, "Artifacts", "robotcontrolv3-live-sdk-readback-gate.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
            File.WriteAllText(artifactPath, result, Encoding.UTF8);
            return $"{result}; artifact={artifactPath}";
        }

        public static string RunLiveCommandSafetyGateMatrixForDebug()
        {
            var runtime = GetRuntimeController();
            var gate = new LiveCommandSafetyGate();
            var payload = new GenericMatrixPayload
            {
                generatedAt = System.DateTime.Now.ToString("O"),
                project = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                name = "live-command-safety-gate",
            };

            void AddCase(string name, LiveCommandSafetyGateRequest request, string expected)
            {
                var result = new GenericMatrixResult
                {
                    name = name,
                    expected = expected,
                };

                try
                {
                    var gateResult = gate.Evaluate(request);
                    result.message = gateResult.ToSummary();
                    result.after = GetMovementStateSummaryForDebug();
                    result.passed = result.message.Contains(expected);
                    if (!result.passed)
                    {
                        result.failureClass = "gate";
                    }
                }
                catch (System.Exception ex)
                {
                    result.passed = false;
                    result.failureClass = "exception";
                    result.after = $"{ex.GetType().Name}: {ex.Message}";
                }

                payload.results.Add(result);
            }

            runtime.Disconnect();
            var service = GetRuntimeConnectionService();
            AddCase("not-connected-blocked", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: false), "not connected");
            runtime.ConnectDefault();
            AddCase("servo-disabled-blocked", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: false), "servo disabled");
            runtime.EnableServo();
            AddCase("dryrun-allows-simulation", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: true, confirmed: false), "Allowed");
            AddCase("operator-token-required", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: false, boundary: true, collision: true), "RequiresConfirm");
            AddCase("speed-cap-blocks", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: true, speed: 30), "exceeds cap");
            AddCase("boundary-missing-blocks", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: true), "boundary data missing");
            AddCase("collision-missing-blocks", NewGateRequest(service, LiveCommandKind.MoveL, dryRun: false, confirmed: true, boundary: true), "collision data missing");
            AddCase("numerical-ik-blocks", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: true, boundary: true, collision: true, productionIk: false), "production IK guard not cleared");
            AddCase("saved-movej-eligible", NewGateRequest(service, LiveCommandKind.MoveJ, dryRun: false, confirmed: true, boundary: true, collision: true, productionIk: true), "Allowed");
            AddCase("gripper-readback-required", NewGateRequest(service, LiveCommandKind.MoveGripper, dryRun: false, confirmed: true, boundary: true, collision: true, gripperReadback: false), "gripper readback missing");
            AddCase("gripper-eligible", NewGateRequest(service, LiveCommandKind.MoveGripper, dryRun: false, confirmed: true, boundary: true, collision: true, gripperReadback: true), "Allowed");
            AddCase("readback-only", NewGateRequest(service, LiveCommandKind.ReadbackOnly, dryRun: false, confirmed: false), "ReadbackOnly");

            return CompleteGenericMatrix(payload, "robotcontrolv3-live-command-safety-gate.json", "LiveCommandSafetyGate");
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
            return $"status={snapshot.StatusKind}; dryRun={snapshot.DryRunEnabled}; pending={snapshot.PendingCommandSummary}; feedback={snapshot.LastFeedback}; joints=[{string.Join(",", snapshot.JointValues)}]; tcp=[{string.Join(",", snapshot.TcpValues)}]; ghost={snapshot.HasGhostPreview}; path={snapshot.HasPredictedPath}; gripper={snapshot.GripperSummary}; gripperVisual={snapshot.GripperVisualAttached}; robotDo={snapshot.RobotDoSummary}; toolDo={snapshot.ToolDoSummary}; peripheral={snapshot.PeripheralFeedback}; gripperSdk={snapshot.GripperSdkSummary}; selected={snapshot.SelectedPartName}; liveBlocked={snapshot.LiveBlockedReason}";
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

        private static string ClickUiButton(string buttonName, string prefer, out bool found, out bool enabled, out string path)
        {
            var document = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            var root = document?.rootVisualElement;
            if (root == null)
            {
                found = false;
                enabled = false;
                path = string.Empty;
                return "document-missing";
            }

            var buttons = new List<Button>();
            root.Query<Button>(name: buttonName).ForEach(button => buttons.Add(button));
            if (buttons.Count == 0)
            {
                found = false;
                enabled = false;
                path = string.Empty;
                return "not-found";
            }

            var selected = SelectButton(buttons, prefer);
            found = selected != null;
            enabled = selected != null && selected.enabledInHierarchy;
            path = selected != null ? BuildElementPath(selected) : string.Empty;
            if (selected == null)
            {
                return "not-found";
            }

            if (!selected.enabledInHierarchy)
            {
                return "disabled";
            }

            using var clickEvent = ClickEvent.GetPooled();
            clickEvent.target = selected;
            selected.SendEvent(clickEvent);
            return $"clicked:{buttonName}";
        }

        private static FairinoConnectionService GetRuntimeConnectionService()
        {
            var runtime = GetRuntimeController();
            return runtime.ConnectionServiceForDebug;
        }

        private static LiveCommandSafetyGateRequest NewGateRequest(
            FairinoConnectionService service,
            LiveCommandKind kind,
            bool dryRun,
            bool confirmed,
            int speed = 10,
            bool boundary = false,
            bool collision = false,
            bool productionIk = true,
            bool gripperReadback = false)
        {
            return new LiveCommandSafetyGateRequest
            {
                Kind = kind,
                ConnectionService = service,
                AllowDryRun = dryRun,
                OperatorConfirmed = confirmed,
                RequestedSpeedPercent = speed,
                SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                HasDryRunPreviewArtifact = true,
                IsProductionIkSafe = productionIk,
                IsBoundaryDataReady = boundary,
                IsTargetWithinBoundary = boundary,
                IsCollisionDataReady = collision,
                IsPredictedPathCollisionFree = collision,
                HasGripperReadback = gripperReadback,
                TreatMockAsLiveForDebug = true,
            };
        }

        private static void EnsureRuntimeReady(RobotControlV3RuntimeController runtime)
        {
            runtime.Disconnect();
            runtime.ConnectDefault();
            runtime.EnableServo();
            if (!runtime.CurrentSnapshot.DryRunEnabled)
            {
                runtime.ToggleDryRun();
            }
        }

        private static void AddPointGuard(GenericMatrixPayload payload, string name, System.Func<FairinoResult> action, string needle)
        {
            var result = new GenericMatrixResult
            {
                name = name,
                expected = needle ?? string.Empty,
            };

            try
            {
                var actionResult = action();
                result.message = actionResult.Message;
                result.after = GetMovementStateSummaryForDebug();
                result.passed = string.IsNullOrEmpty(needle)
                    || actionResult.Message.Contains(needle)
                    || result.after.Contains(needle);
                if (!result.passed)
                {
                    result.failureClass = "runtime";
                }
            }
            catch (System.Exception ex)
            {
                result.passed = false;
                result.failureClass = "exception";
                result.after = $"{ex.GetType().Name}: {ex.Message}";
            }

            payload.results.Add(result);
        }

        private static string SnapshotPoseSignature(RobotControlV3RuntimeController runtime)
        {
            var snapshot = runtime.CurrentSnapshot;
            var joints = snapshot.JointValues != null ? string.Join(",", snapshot.JointValues) : string.Empty;
            var tcp = snapshot.TcpValues != null ? string.Join(",", snapshot.TcpValues) : string.Empty;
            return $"joints={joints};tcp={tcp}";
        }

        private static string CompleteGenericMatrix(GenericMatrixPayload payload, string artifactName, string label)
        {
            var passCount = 0;
            var failCount = 0;
            var failures = new StringBuilder();
            foreach (var result in payload.results)
            {
                if (result.passed)
                {
                    passCount++;
                }
                else
                {
                    failCount++;
                    failures.Append(result.name)
                        .Append('(')
                        .Append(result.failureClass)
                        .Append("),");
                }
            }

            payload.caseCount = payload.results.Count;
            payload.passCount = passCount;
            payload.failCount = failCount;

            var project = payload.project;
            var artifactPath = Path.Combine(project, "Artifacts", artifactName);
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
            File.WriteAllText(artifactPath, JsonUtility.ToJson(payload, true), Encoding.UTF8);
            return $"{label} pass={passCount}; fail={failCount}; artifact={artifactPath}; failures={failures}";
        }

        private static string CaptureStageAngle(RobotControlV3RuntimeController runtime, string label, Vector3 direction, string outputPath)
        {
            runtime.ForceInitialize();
            var camera = runtime.StageCamera;
            if (camera == null)
            {
                return $"{label}:camera-missing";
            }

            if (!TryGetSceneRendererBounds(out var bounds))
            {
                return $"{label}:bounds-missing";
            }

            var focus = bounds.center;
            var safeDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : new Vector3(0f, 0.55f, -1f).normalized;
            var radius = Mathf.Max(bounds.extents.magnitude * 2.15f, 1.6f);
            camera.transform.position = focus + safeDirection * radius;
            camera.transform.LookAt(focus);
            return CaptureCamera(camera, outputPath);
        }

        private static bool TryGetSceneRendererBounds(out Bounds bounds)
        {
            bounds = default;
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var found = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.GetComponentInParent<UIDocument>() != null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found && bounds.size.sqrMagnitude > 0.0001f;
        }

        private static string CaptureCamera(Camera camera, string outputPath, int width = 1280, int height = 720)
        {
            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "RobotControlV3StageAngleCapture"
            };
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (Application.isPlaying)
                {
                    Object.Destroy(renderTexture);
                    Object.Destroy(texture);
                }
                else
                {
                    Object.DestroyImmediate(renderTexture);
                    Object.DestroyImmediate(texture);
                }
            }

            return $"{Path.GetFileName(fullPath)}:{width}x{height}";
        }

        private static Button SelectButton(List<Button> buttons, string prefer)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return null;
            }

            if (string.Equals(prefer, "tablet", System.StringComparison.OrdinalIgnoreCase))
            {
                for (var i = 0; i < buttons.Count; i++)
                {
                    if (HasAncestor(buttons[i], "BottomSheet") || HasAncestor(buttons[i], "BottomBar"))
                    {
                        return buttons[i];
                    }
                }
            }

            for (var i = 0; i < buttons.Count; i++)
            {
                if (!HasAncestor(buttons[i], "BottomSheet"))
                {
                    return buttons[i];
                }
            }

            return buttons[0];
        }

        private static bool HasAncestor(VisualElement element, string ancestorName)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildElementPath(VisualElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            var stack = new Stack<string>();
            for (var current = element; current != null; current = current.parent)
            {
                stack.Push(string.IsNullOrEmpty(current.name) ? current.GetType().Name : current.name);
            }

            return string.Join("/", stack);
        }

        [System.Serializable]
        private sealed class ActualClickMatrixPayload
        {
            public string generatedAt;
            public string project;
            public int caseCount;
            public int passCount;
            public int failCount;
            public List<ActualClickMatrixResult> results = new();
        }

        [System.Serializable]
        private sealed class ActualClickMatrixResult
        {
            public string name;
            public string prefer;
            public string expected;
            public bool passed;
            public string failureClass;
            public bool found;
            public bool enabled;
            public string path;
            public string before;
            public string after;
            public string clickMessage;
        }

        [System.Serializable]
        private sealed class GenericMatrixPayload
        {
            public string generatedAt;
            public string project;
            public string name;
            public int caseCount;
            public int passCount;
            public int failCount;
            public List<GenericMatrixResult> results = new();
        }

        [System.Serializable]
        private sealed class GenericMatrixResult
        {
            public string name;
            public string expected;
            public bool passed;
            public string failureClass;
            public string path;
            public string before;
            public string after;
            public string message;
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
