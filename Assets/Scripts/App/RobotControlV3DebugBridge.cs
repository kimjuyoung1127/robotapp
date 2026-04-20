// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI.RobotControlV3;
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

        public static string GetConnectionSessionSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var adapter = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                return "PendantV3ConnectionSessionAdapter missing";
            }

            adapter.ForceInitialize();
            return adapter.GetDebugSummary();
        }

        public static string GetVisualizationSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var orchestrator = Object.FindFirstObjectByType<PendantV3VisualizationOrchestrator>(FindObjectsInactive.Include);
            var driver = Object.FindFirstObjectByType<Visualization.PendantV3VisualizationDriver>(FindObjectsInactive.Include);
            if (orchestrator == null || driver == null)
            {
                return "PendantV3 visualization missing";
            }

            orchestrator.ForceInitialize();
            driver.ForceInitialize();
            return $"state=[{orchestrator.GetDebugSummary()}]; driver=[{driver.GetDebugSummary()}]";
        }

        public static string GetViewportVisibilitySummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var camera = Camera.main ?? Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            var actual = GameObject.Find("RobotActual");
            var ghost = GameObject.Find("RobotGhost");
            var driver = Object.FindFirstObjectByType<Visualization.PendantV3VisualizationDriver>(FindObjectsInactive.Include);
            var cameraSummary = camera != null
                ? $"cameraPos={camera.transform.position:F2}; cameraEuler={camera.transform.eulerAngles:F2}; fov={camera.fieldOfView:0.0}; rect={camera.rect}"
                : "camera=missing";
            var actualSummary = actual != null ? $"actual=True; active={actual.activeInHierarchy}" : "actual=False";
            var ghostSummary = ghost != null ? $"ghost=True; active={ghost.activeInHierarchy}" : "ghost=False";
            var driverSummary = driver != null ? driver.GetDebugSummary() : "driver=missing";
            return $"scene={scene.name}; {cameraSummary}; {actualSummary}; {ghostSummary}; {driverSummary}";
        }

        public static string GetViewportProbeSummary()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var camera = Camera.main ?? Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            var orbit = camera != null ? camera.GetComponent<Visualization.OrbitCameraController>() : null;
            var document = Object.FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            var viewportHost = document?.rootVisualElement?.Q<VisualElement>("ViewportHost");
            var runtimeRoot = GameObject.Find("PendantV3RuntimeRoot");
            var actual = GameObject.Find("RobotActual");
            var ghost = GameObject.Find("RobotGhost");
            var target = orbit != null ? orbit.Target : null;
            var hostBounds = viewportHost != null ? viewportHost.worldBound.ToString() : "null";
            var actualPos = actual != null ? actual.transform.position.ToString("F2") : "null";
            var ghostPos = ghost != null ? ghost.transform.position.ToString("F2") : "null";
            var runtimeRootPos = runtimeRoot != null ? runtimeRoot.transform.position.ToString("F2") : "null";
            var targetPos = target != null ? target.position.ToString("F2") : "null";
            var panelName = viewportHost?.panel?.GetType().Name ?? "null";
            var cameraRect = camera != null ? camera.rect.ToString() : "null";
            return $"scene={scene.name}; hostBounds={hostBounds}; panel={panelName}; runtimeRootPos={runtimeRootPos}; actualPos={actualPos}; ghostPos={ghostPos}; targetPos={targetPos}; cameraRect={cameraRect}";
        }

        public static string SetLiveModeForDebug(bool live)
        {
            var adapter = GetConnectionSessionAdapter();
            adapter.SetMockMode(!live);
            return adapter.GetDebugSummary();
        }

        public static string SetLiveArmForDebug(bool armed)
        {
            var adapter = GetConnectionSessionAdapter();
            adapter.SetLiveArmState(armed);
            return adapter.GetDebugSummary();
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
            var status = Object.FindFirstObjectByType<StatusCardController>(FindObjectsInactive.Include);
            var safety = Object.FindFirstObjectByType<SafetyDiagnosticsController>(FindObjectsInactive.Include);
            var visualization = Object.FindFirstObjectByType<PendantV3VisualizationOrchestrator>(FindObjectsInactive.Include);
            var contextTabs = Object.FindFirstObjectByType<ContextPanelTabController>(FindObjectsInactive.Include);
            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            var session = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            var binder = Object.FindFirstObjectByType<PendantV3Binder>(FindObjectsInactive.Include);
            var coordinator = Object.FindFirstObjectByType<PendantV3SceneCoordinator>(FindObjectsInactive.Include);
            var shellCount = Object.FindObjectsByType<PendantV3ShellStateController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var easyCount = Object.FindObjectsByType<EasyMotionController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var jointCount = Object.FindObjectsByType<JointJogController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var tcpCount = Object.FindObjectsByType<TcpJogController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            var pointCount = Object.FindObjectsByType<PointMoveController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            session?.ForceInitialize();
            coordinator?.ForceBootstrap();
            binder?.ForceInitialize();
            home?.ForceInitialize();
            easy?.ForceInitialize();
            jointJog?.ForceInitialize();
            tcpJog?.ForceInitialize();
            pointMove?.ForceInitialize();
            status?.ForceInitialize();
            safety?.ForceInitialize();
            contextTabs?.ForceInitialize();
            var homeSummary = home != null ? home.GetDebugSummary() : "ConnectionHomeController missing";
            var easySummary = easy != null ? easy.GetDebugSummary() : "EasyMotionController missing";
            var jointJogSummary = jointJog != null ? jointJog.GetDebugSummary() : "JointJogController missing";
            var tcpJogSummary = tcpJog != null ? tcpJog.GetDebugSummary() : "TcpJogController missing";
            var pointMoveSummary = pointMove != null ? pointMove.GetDebugSummary() : "PointMoveController missing";
            var statusSummary = status != null ? status.GetDebugSummary() : "StatusCardController missing";
            var safetySummary = safety != null ? safety.GetDebugSummary() : "SafetyDiagnosticsController missing";
            var visualizationSummary = visualization != null ? visualization.GetDebugSummary() : "PendantV3VisualizationOrchestrator missing";
            var contextTabsSummary = contextTabs != null ? contextTabs.GetDebugSummary() : "ContextPanelTabController missing";
            var shellSummary = shell != null ? shell.GetDebugSummary() : "PendantV3ShellStateController missing";
            var sessionSummary = session != null ? session.GetDebugSummary() : "PendantV3ConnectionSessionAdapter missing";
            var binderSummary = binder != null ? binder.GetDebugSummary() : "PendantV3Binder missing";
            var coordinatorSummary = coordinator != null ? coordinator.GetDebugSummary() : "PendantV3SceneCoordinator missing";
            return $"counts=[shell={shellCount}; easy={easyCount}; joint={jointCount}; tcp={tcpCount}; point={pointCount}] | coordinator=[{coordinatorSummary}] | session=[{sessionSummary}] | visualization=[{visualizationSummary}] | binder=[{binderSummary}] | contextTabs=[{contextTabsSummary}] | shell=[{shellSummary}] | home=[{homeSummary}] | status=[{statusSummary}] | safety=[{safetySummary}] | easy=[{easySummary}] | joint=[{jointJogSummary}] | tcp=[{tcpJogSummary}] | point=[{pointMoveSummary}]";
        }

        public static string TriggerConnectionLostForDebug()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var adapter = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                throw new MissingReferenceException("PendantV3ConnectionSessionAdapter not found in RobotControlV3 scene.");
            }

            adapter.TriggerConnectionLostForDebug();
            return adapter.GetDebugSummary();
        }

        public static string AdvanceReconnectTickForDebug(float seconds)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var adapter = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                throw new MissingReferenceException("PendantV3ConnectionSessionAdapter not found in RobotControlV3 scene.");
            }

            adapter.AdvanceReconnectTickForDebug(seconds);
            return adapter.GetDebugSummary();
        }

        public static string CompleteReconnectForDebug(bool success)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var adapter = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                throw new MissingReferenceException("PendantV3ConnectionSessionAdapter not found in RobotControlV3 scene.");
            }

            adapter.CompleteReconnectForDebug(success);
            return adapter.GetDebugSummary();
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
            var contextPanel = root.Q<VisualElement>("ContextPanel") != null;
            var descendantCount = CountDescendants(root);
            var homeHostHidden = homeHost?.ClassListContains("rc-hidden") ?? false;
            var easyHostHidden = easyHost?.ClassListContains("rc-hidden") ?? false;
            var easySheetHidden = easySheet?.ClassListContains("rc-hidden") ?? false;
            var workPanelBodyHidden = workPanelBody?.ClassListContains("rc-hidden") ?? false;
            var bottomSheetBodyHidden = bottomSheetBody?.ClassListContains("rc-hidden") ?? false;
            var whyCardHidden = whyCard?.ClassListContains("rc-hidden") ?? false;
            return $"panel={panelName}; tree={treeName}; rootChildren={childCount}; rootName={(root?.name ?? "null")}; bridge={bridgeName}; robotName={robotName}; easyHome={easyHome}; homeHost={homePanel}; easyHost={easyPanel}; context={contextPanel}; homeHostChildren={homeHost?.childCount ?? -1}; easyHostChildren={easyHost?.childCount ?? -1}; homeHostHidden={homeHostHidden}; easyHostHidden={easyHostHidden}; workPanelBodyHidden={workPanelBodyHidden}; homeSheetChildren={homeSheet?.childCount ?? -1}; easySheetChildren={easySheet?.childCount ?? -1}; easySheetHidden={easySheetHidden}; bottomSheetBodyHidden={bottomSheetBodyHidden}; whyCardHidden={whyCardHidden}; descendants={descendantCount}";
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

        private static Fairino.PendantV3ConnectionSessionAdapter GetConnectionSessionAdapter()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var adapter = Object.FindFirstObjectByType<Fairino.PendantV3ConnectionSessionAdapter>(FindObjectsInactive.Include);
            if (adapter == null)
            {
                throw new MissingReferenceException("PendantV3ConnectionSessionAdapter not found in RobotControlV3 scene.");
            }

            return adapter;
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
