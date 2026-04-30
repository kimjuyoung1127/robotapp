// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 NavHelp 전용 도움말 패널 scaffold를 관리합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class HelpPanelController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset helpPanelTemplate;

        private VisualElement root;
        private VisualElement workTabBar;
        private VisualElement workPanelBody;
        private VisualElement bottomTabBar;
        private VisualElement bottomSheetBody;
        private VisualElement helpPanelHost;
        private VisualElement helpSheetHost;
        private Label workPanelTitle;
        private Label workPanelSummary;
        private Label bottomSheetTitle;
        private Label bottomSheetSummary;
        private ConnectionHomeController connectionHomeController;
        private HelpElements desktopPanel;
        private HelpElements tabletPanel;
        private bool isDesktopHelpActive;
        private bool isTabletHelpActive;
        private PendantV3LocalState currentShellState;
        private bool isInitialized;
        private bool isInitializing;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public void SetShellState(string activeNavSection, string activeWorkTab, string activeTabletTab)
        {
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            if (connectionHomeController == null)
            {
                return;
            }

            var shellState = ResolveShellStateSnapshot();
            shellState.ActiveNavSection = activeNavSection;
            shellState.ActiveWorkTab = activeWorkTab;
            shellState.ActiveTabletTab = activeTabletTab;
            RefreshFromBinder(connectionHomeController.CurrentPreviewDefinition, shellState);
        }

        public string GetDebugSummary()
        {
            var panelHidden = helpPanelHost?.ClassListContains("rc-hidden") ?? true;
            var sheetHidden = helpSheetHost?.ClassListContains("rc-hidden") ?? true;
            return $"initialized={isInitialized}; desktopHelp={isDesktopHelpActive}; tabletHelp={isTabletHelpActive}; panelHidden={panelHidden}; sheetHidden={sheetHidden}";
        }

        private bool TryInitialize()
        {
            if (isInitialized)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            isInitializing = true;
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            try
            {
                if (root == null || helpPanelTemplate == null || connectionHomeController == null)
                {
                    return false;
                }

                CacheShellElements();
                if (helpPanelHost == null || helpSheetHost == null)
                {
                    isInitialized = false;
                    return false;
                }

                if (desktopPanel == null || tabletPanel == null || helpPanelHost.childCount == 0 || helpSheetHost.childCount == 0)
                {
                    desktopPanel = CreatePanel(helpPanelHost);
                    tabletPanel = CreatePanel(helpSheetHost);
                }

                currentShellState = ResolveShellStateSnapshot();
                isInitialized = true;
                RefreshFromBinder(connectionHomeController.CurrentPreviewDefinition, currentShellState);
                return true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private System.Collections.IEnumerator WaitForInitialize()
        {
            for (var frame = 0; frame < 30 && !isInitialized; frame++)
            {
                TryInitialize();
                if (isInitialized)
                {
                    break;
                }

                yield return null;
            }

            initializeCoroutine = null;
        }

        private void CacheShellElements()
        {
            workTabBar = root.Q<VisualElement>("WorkTabBar");
            workPanelBody = root.Q<VisualElement>("WorkPanelBody");
            bottomTabBar = root.Q<VisualElement>("BottomTabBar");
            bottomSheetBody = root.Q<VisualElement>("BottomSheetBody");
            helpPanelHost = root.Q<VisualElement>("HelpPanelHost");
            helpSheetHost = root.Q<VisualElement>("HelpSheetHost");
            workPanelTitle = root.Q<Label>("WorkPanelTitle");
            workPanelSummary = root.Q<Label>("WorkPanelSummary");
            bottomSheetTitle = root.Q<Label>("BottomSheetTitle");
            bottomSheetSummary = root.Q<Label>("BottomSheetSummary");
        }

        private HelpElements CreatePanel(VisualElement host)
        {
            host.Clear();
            var tree = helpPanelTemplate.CloneTree();
            host.Add(tree);
            return new HelpElements(tree);
        }

        internal void RefreshFromBinder(RobotControlV3RuntimeSnapshot data, PendantV3LocalState shellState)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            currentShellState = NormalizeShellState(shellState);
            isDesktopHelpActive = currentShellState.ActiveNavSection == "NavHelp";
            isTabletHelpActive = currentShellState.ActiveTabletTab == "BottomTabHelp";
            ApplyPreview(data, currentShellState);
            ApplyVisibility(currentShellState);
        }

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data, PendantV3LocalState shellState)
        {
            ApplyPanel(desktopPanel, data, shellState);
            ApplyPanel(tabletPanel, data, shellState);
        }

        private void ApplyPanel(HelpElements panel, RobotControlV3RuntimeSnapshot data, PendantV3LocalState shellState)
        {
            if (panel == null)
            {
                return;
            }

            panel.HelpPrimaryBody.text = BuildPrimaryHelp(shellState);
            panel.HelpSafetyBody.text = BuildSafetyHelp(data, shellState);
            panel.HelpActionBody.text = BuildActionHelp(data, shellState);
        }

        private void ApplyVisibility(PendantV3LocalState shellState)
        {
            // WorkPanelBody / BottomSheetBody are shared shells for Help and Motion tabs.
            // Help hosts should toggle independently without re-hiding the shared stage body.
            workPanelBody?.EnableInClassList("rc-hidden", false);
            bottomSheetBody?.EnableInClassList("rc-hidden", false);
            helpPanelHost?.EnableInClassList("rc-hidden", !isDesktopHelpActive);
            helpSheetHost?.EnableInClassList("rc-hidden", !isTabletHelpActive);
            workTabBar?.EnableInClassList("rc-hidden", isDesktopHelpActive);
            bottomTabBar?.EnableInClassList("rc-hidden", false);

            if (bottomSheetTitle != null)
            {
                bottomSheetTitle.text = isTabletHelpActive
                    ? "BottomSheet · 도움말"
                    : GetTabletTitle(shellState);
            }

            if (bottomSheetSummary != null)
            {
                bottomSheetSummary.text = isTabletHelpActive
                    ? "태블릿에서는 하단 시트로 현재 단계 도움말을 바로 연다."
                    : GetTabletSummary(shellState);
            }
        }

        private PendantV3LocalState ResolveShellStateSnapshot()
        {
            var shellStateController = GetComponent<PendantV3ShellStateController>();
            return shellStateController != null
                ? PendantV3LocalState.Normalize(shellStateController.GetStateSnapshot())
                : PendantV3LocalState.Normalize(currentShellState);
        }

        private static PendantV3LocalState NormalizeShellState(PendantV3LocalState shellState)
        {
            return PendantV3LocalState.Normalize(shellState);
        }

        private static string GetDesktopTitle(PendantV3LocalState state)
        {
            if (state.ActiveNavSection == "NavPoints")
            {
                return "저장 위치 패널";
            }

            return state.ActiveNavSection == "NavHome"
                ? "연결 홈"
                : state.ActiveWorkTab switch
                {
                    "TabJointJog" => "관절 패널",
                    "TabTcpJog" => "TCP 패널",
                    "TabPointMove" => "저장 위치 패널",
                    _ => "쉬운 조작 패널",
                };
        }

        private static string GetDesktopSummary(PendantV3LocalState state)
        {
            if (state.ActiveNavSection == "NavPoints")
            {
                return "저장 위치 만들기, 수정, 순서 실행, 작업 묶음을 한곳에서 다룬다.";
            }

            return state.ActiveNavSection == "NavHome"
                ? "연결 상태와 다음 행동 추천을 먼저 읽는 홈 패널."
                : state.ActiveWorkTab switch
                {
                    "TabJointJog" => "로봇 표시 목표 패널. 6축 관절값을 슬라이더, 단일축 버튼, 숫자 입력으로 바로 다루는 큰 작업 패널.",
                    "TabTcpJog" => "로봇 표시 목표 패널. Base·Tool·User 좌표계 기준으로 XYZ·RPY 조그를 다루는 큰 작업 패널.",
                    "TabPointMove" => "로봇 표시 목표 패널. 지정 위치를 입력하고 관절 이동·직선 이동 후보를 준비하는 큰 작업 패널.",
                    _ => "로봇 표시 목표 패널. 자주 쓰는 포즈와 작은 이동부터 시작하는 큰 작업 패널.",
                };
        }

        private static string GetTabletTitle(PendantV3LocalState state)
        {
            return state.ActiveTabletTab switch
            {
                "BottomTabJointJog" => "BottomSheet · 관절",
                "BottomTabTcpJog" => "BottomSheet · TCP",
                "BottomTabPointMove" => "BottomSheet · 저장",
                "BottomTabStatus" => "BottomSheet · 상태",
                "BottomTabHelp" => "BottomSheet · 도움말",
                _ => "BottomSheet · 쉬운조작",
            };
        }

        private static string GetTabletSummary(PendantV3LocalState state)
        {
            return state.ActiveTabletTab switch
            {
                "BottomTabJointJog" => "태블릿에서는 관절 조그를 하단 시트에서 열어 3D 뷰를 가리지 않게 유지한다.",
                "BottomTabTcpJog" => "태블릿에서는 TCP 조그와 좌표계 전환을 하단 시트에 모아 한 손 조작 흐름을 유지한다.",
                    "BottomTabPointMove" => "태블릿에서는 저장 위치와 묶음을 하단 시트에서 한 흐름으로 확인한다.",
                "BottomTabStatus" => "태블릿에서는 상태/알람 요약을 하단 시트에서 열어 현재 위험도를 먼저 읽게 한다.",
                "BottomTabHelp" => "태블릿에서는 하단 시트로 현재 단계 도움말을 바로 연다.",
                _ => "태블릿에서는 쉬운 조작 프리셋과 작은 이동을 하단 시트에서 바로 연다.",
            };
        }

        private string BuildPrimaryHelp(PendantV3LocalState state)
        {
            return connectionHomeController.CurrentPreviewState switch
            {
                PendantV3PreviewState.Kind.Fault => "지금은 Fault가 걸린 상태라 조작보다 원인 확인과 복구 순서 읽기가 첫 번째다.",
                PendantV3PreviewState.Kind.ConnectedUnsynced => "서보는 살아 있어도 미동기화면 현재 자세를 먼저 읽고 좌표계부터 다시 맞춰야 한다.",
                PendantV3PreviewState.Kind.AutoReconnect => "통신 복귀가 먼저라서 조작 입력보다 연결 상태와 재시도 흐름을 먼저 봐야 한다.",
                _ => $"현재 기준 좌표계는 {state.CoordSystem}, 증분은 {state.JogIncrement}, 속도는 {state.SpeedPercent}%다. 작은 이동이나 미리보기부터 시작해라.",
            };
        }

        private static string BuildSafetyHelp(RobotControlV3RuntimeSnapshot data, PendantV3LocalState state)
        {
            return state.ActiveNavSection == "NavHelp" || state.ActiveTabletTab == "BottomTabHelp"
                ? $"{data.ActionWhy} 현재 속도 {state.SpeedPercent}% 기준으로는 확인-미리보기-적용 순서를 유지하는 게 안전하다."
                : data.ActionWhy;
        }

        private static string BuildActionHelp(RobotControlV3RuntimeSnapshot data, PendantV3LocalState state)
        {
            var nextStep = state.ActiveNavSection == "NavHelp"
                ? "도움말을 다 읽었으면 원래 작업 탭으로 돌아가 한 단계씩 확인해라."
                : "지금 탭 흐름을 유지한 채 바로 다음 작은 동작을 확인해라.";
            return $"{data.ActionPrimary} {nextStep}";
        }

        private sealed class HelpElements
        {
            public HelpElements(VisualElement root)
            {
                HelpPrimaryBody = root.Q<Label>("HelpPrimaryBody");
                HelpSafetyBody = root.Q<Label>("HelpSafetyBody");
                HelpActionBody = root.Q<Label>("HelpActionBody");
            }

            public Label HelpPrimaryBody { get; }
            public Label HelpSafetyBody { get; }
            public Label HelpActionBody { get; }
        }
    }
}
