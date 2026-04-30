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
    public static partial class RobotControlV3DebugBridge
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
                case "NavStatus":
                case "NavHelp":
                    shell.SetDebugSelection(buttonName, state.ActiveWorkTab, state.ActiveTabletTab);
                    return $"button={buttonName}; found=True; action=nav; {shell.GetDebugSummary()}";
                case "TabEasyMotion":
                case "TabJointJog":
                case "TabTcpJog":
                    shell.SetDebugSelection(state.ActiveNavSection, buttonName, state.ActiveTabletTab);
                    return $"button={buttonName}; found=True; action=work-tab; {shell.GetDebugSummary()}";
                case "BottomTabEasyMotion":
                case "BottomTabJointJog":
                case "BottomTabTcpJog":
                case "BottomTabPointMove":
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
                    var clickMessage = ClickUiButton(buttonName, "desktop", out var found, out var enabled, out var path);
                    return $"button={buttonName}; found={found}; enabled={enabled}; path={path}; action=click; result={clickMessage}";
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

        public static string GetPanelWidthHierarchySummaryForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var document = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            var root = document?.rootVisualElement;
            if (root == null)
            {
                return "UIDocument missing";
            }

            var workPanel = root.Q<VisualElement>("WorkPanel");
            var viewportHost = root.Q<VisualElement>("ViewportHost");
            var contextPanel = root.Q<VisualElement>("ContextPanel");
            var main = workPanel?.worldBound.width ?? 0f;
            var aux = viewportHost?.worldBound.width ?? 0f;
            var context = contextPanel?.worldBound.width ?? 0f;
            var valid = main > aux && aux > context;
            return $"main={main:0.0}; aux={aux:0.0}; context={context:0.0}; hierarchy={(valid ? "main>aux>context" : "invalid")}";
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

        public static string SetShellSpeedPercentForDebug(int speedPercent)
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

            return $"instanceId={shell.GetInstanceID()}; {shell.SetSpeedPercentForDebug(speedPercent)}";
        }

        public static string SetShellJogIncrementForDebug(int jogIncrement)
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

            return $"instanceId={shell.GetInstanceID()}; {shell.SetJogIncrementForDebug(jogIncrement)}";
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

        public static string ConfirmPopupForDebug()
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
            var popup = popupCoordinator.ConfirmActivePopupForDebug();
            return $"{popup}; movement={GetMovementStateSummaryForDebug()}; approval={GetLiveCommandApprovalSummaryForDebug()}";
        }

        public static string CancelPopupForDebug()
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
            var popup = popupCoordinator.CancelActivePopupForDebug();
            return $"{popup}; movement={GetMovementStateSummaryForDebug()}; approval={GetLiveCommandApprovalSummaryForDebug()}";
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

        public static string StartTeachingPathRecordingForDebug()
        {
            return GetRuntimeController().StartTeachingPathRecording();
        }

        public static string StopTeachingPathRecordingForDebug()
        {
            return GetRuntimeController().StopTeachingPathRecording();
        }

        public static string CaptureTeachingPathFrameForDebug()
        {
            return GetRuntimeController().CaptureTeachingPathFrameForDebug();
        }

        public static string PlayRecordedTeachingPathOnceForDebug()
        {
            return GetRuntimeController().PlayRecordedTeachingPathOnce();
        }

        public static string PlayRecordedTeachingPathLoopForDebug()
        {
            return GetRuntimeController().PlayRecordedTeachingPathLoop();
        }

        public static string GetTeachingPathRecordingSummaryForDebug()
        {
            return GetRuntimeController().GetTeachingPathRecordingSummaryForDebug();
        }
    }
}
