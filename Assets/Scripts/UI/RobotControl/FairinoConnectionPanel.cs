// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FAIRINO 로봇 연결 설정 패널입니다.
    /// IP 입력, Connect/Disconnect, Enable/Disable, Mock↔Live 전환, 버전 표시를 제공합니다.
    /// </summary>
    public class FairinoConnectionPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private InputField ipInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button enableButton;
        [SerializeField] private Button diagnosticsButton;
        [SerializeField] private Toggle mockToggle;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text versionLabel;
        [SerializeField] private Font fallbackFont;

        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private string connectionTitleText = "Robot Connection";
        private bool listenersBound;

        /// <summary>
        /// 진단 서랍 열기 요청 이벤트입니다.
        /// </summary>
        public event Action OnDiagnosticsRequested;

        /// <summary>
        /// 현재 표시 중인 상태 텍스트입니다.
        /// </summary>
        public string CurrentStatusText => statusLabel != null ? statusLabel.text : string.Empty;

        /// <summary>
        /// 현재 표시 중인 버전 텍스트입니다.
        /// </summary>
        public string CurrentVersionText => versionLabel != null ? versionLabel.text : string.Empty;

        /// <summary>
        /// 현재 IP 입력값입니다.
        /// </summary>
        public string CurrentIp => ipInput != null ? ipInput.text : string.Empty;

        /// <summary>
        /// 연결 서비스를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig, string titleText = null)
        {
            UnsubscribeService();
            connectionService = service;
            config = robotConfig;
            connectionTitleText = !string.IsNullOrWhiteSpace(titleText)
                ? titleText
                : BuildDefaultTitle(robotConfig);
            EnsurePresentation();

            if (ipInput != null && config != null && !string.IsNullOrWhiteSpace(config.defaultIp))
            {
                ipInput.text = config.defaultIp;
            }

            SubscribeService();
            RefreshUI();
        }

        private void Awake()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindListeners();
            SubscribeService();
            RefreshUI();
        }

        private void OnDisable()
        {
            UnbindListeners();
            UnsubscribeService();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

            if (TryBindExistingPresentation(root))
            {
                return;
            }

            // 이전 레이아웃에서 남은 IpLabel 제거
            var staleIpLabel = root.Find("IpLabel");
            if (staleIpLabel != null) UnityEngine.Object.Destroy(staleIpLabel.gameObject);

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200f, 20f), new Vector2(16f, -10f));
            title.text = connectionTitleText;

            // Row 1: IP input + Connect
            ipInput ??= UIComponentFactory.CreateInputField(root, "IpInput", "192.168.58.2", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)ipInput.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(192f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, -34f));

            connectButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnConnect", "Connect", fallbackFont, 100f);
            UiRuntimeStyle.Anchor((RectTransform)connectButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(216f, -34f));

            diagnosticsButton = root.Find("BtnDiagnostics")?.GetComponent<Button>();
            if (diagnosticsButton != null)
            {
                diagnosticsButton.gameObject.SetActive(false);
            }

            // Row 2: Mock toggle + Enable
            mockToggle ??= UIComponentFactory.CreateToggle(root, "MockToggle", "Mock Mode", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)mockToggle.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(140f, 24f), new Vector2(16f, -70f));
            var mockLabel = mockToggle.transform.Find("Label")?.GetComponent<Text>();
            if (mockLabel != null) mockLabel.text = "Mock Mode";

            enableButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnEnable", "Enable", fallbackFont, 100f);
            UiRuntimeStyle.Anchor((RectTransform)enableButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(216f, -68f));

            // Bottom: status + version
            statusLabel = UiRuntimeStyle.EnsureText(root, "StatusLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(statusLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 34f), new Vector2(16f, 34f));

            versionLabel = UiRuntimeStyle.EnsureText(root, "VersionLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(versionLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 28f), new Vector2(16f, 4f));
            versionLabel.text = string.Empty;
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            ipInput = root.Find("IpInput")?.GetComponent<InputField>();
            connectButton = root.Find("BtnConnect")?.GetComponent<Button>();
            enableButton = root.Find("BtnEnable")?.GetComponent<Button>();
            mockToggle = root.Find("MockToggle")?.GetComponent<Toggle>();
            statusLabel = root.Find("StatusLabel")?.GetComponent<Text>();
            versionLabel = root.Find("VersionLabel")?.GetComponent<Text>();
            diagnosticsButton = root.Find("BtnDiagnostics")?.GetComponent<Button>();

            if (ipInput == null
                || connectButton == null
                || enableButton == null
                || mockToggle == null
                || statusLabel == null
                || versionLabel == null)
            {
                return false;
            }

            var title = root.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = connectionTitleText;
            }

            var mockLabel = mockToggle.transform.Find("Label")?.GetComponent<Text>();
            if (mockLabel != null)
            {
                mockLabel.text = "Mock Mode";
            }

            if (diagnosticsButton != null)
            {
                diagnosticsButton.gameObject.SetActive(false);
            }

            return true;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            connectButton?.onClick.AddListener(OnConnectClicked);
            enableButton?.onClick.AddListener(OnEnableClicked);
            mockToggle?.onValueChanged.AddListener(OnMockToggleChanged);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            connectButton?.onClick.RemoveListener(OnConnectClicked);
            enableButton?.onClick.RemoveListener(OnEnableClicked);
            mockToggle?.onValueChanged.RemoveListener(OnMockToggleChanged);
            listenersBound = false;
        }

        private void SubscribeService()
        {
            if (connectionService == null)
            {
                return;
            }

            connectionService.OnConnectionStateChanged -= HandleServiceStateChanged;
            connectionService.OnEnableStateChanged -= HandleServiceStateChanged;
            connectionService.OnModeChanged -= HandleServiceModeChanged;
            connectionService.OnError -= HandleError;

            connectionService.OnConnectionStateChanged += HandleServiceStateChanged;
            connectionService.OnEnableStateChanged += HandleServiceStateChanged;
            connectionService.OnModeChanged += HandleServiceModeChanged;
            connectionService.OnError += HandleError;
        }

        private void UnsubscribeService()
        {
            if (connectionService == null)
            {
                return;
            }

            connectionService.OnConnectionStateChanged -= HandleServiceStateChanged;
            connectionService.OnEnableStateChanged -= HandleServiceStateChanged;
            connectionService.OnModeChanged -= HandleServiceModeChanged;
            connectionService.OnError -= HandleError;
        }

        private void OnConnectClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            if (connectionService.Client.IsConnected)
            {
                connectionService.Disconnect();
                ClearVersionLabel();
                return;
            }

            var ip = ipInput != null ? ipInput.text : (config != null ? config.defaultIp : string.Empty);
            var port = config != null ? config.defaultPort : 8080;
            var result = connectionService.Connect(ip, port);

            if (result.IsSuccess)
            {
                FetchAndDisplayVersion();
            }
        }

        private void OnEnableClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            if (connectionService.Client.IsEnabled)
            {
                connectionService.Disable();
                return;
            }

            connectionService.Enable();
        }

        private void OnMockToggleChanged(bool isMock)
        {
            connectionService?.SetMockMode(isMock);
            ClearVersionLabel();
        }

        private void OnDiagnosticsClicked() => OnDiagnosticsRequested?.Invoke();

        private void HandleServiceStateChanged(bool _)
        {
            RefreshUI();
        }

        private void HandleServiceModeChanged(bool _)
        {
            RefreshUI();
        }

        private void HandleError(FairinoResult result)
        {
            RefreshUI(result.Message);
        }

        private static string BuildDefaultTitle(FairinoRobotConfig robotConfig)
        {
            if (robotConfig != null && !string.IsNullOrWhiteSpace(robotConfig.displayName))
            {
                return $"{robotConfig.displayName} RPC Connection";
            }

            return "Robot RPC Connection";
        }

        private void RefreshUI(string overrideMessage = null)
        {
            if (connectionService == null)
            {
                if (statusLabel != null)
                {
                    statusLabel.text = "서비스 대기 중...";
                }

                return;
            }

            var isConnected = connectionService.Client.IsConnected;
            var isEnabled = connectionService.Client.IsEnabled;
            var mode = connectionService.IsMockMode ? "Mock" : "Live";
            var conn = isConnected ? "연결됨" : "미연결";
            var servo = isEnabled ? "활성" : "비활성";
            var state = connectionService.LastState;
            var fault = connectionService.LastControllerFault;
            var context = connectionService.LastCoordContext;

            if (statusLabel != null)
            {
                statusLabel.text = string.IsNullOrWhiteSpace(overrideMessage)
                    ? $"[{mode}] {conn} | 서보:{servo} | Mode:{state.RobotMode} | Drag:{(state.IsInDragTeach ? "On" : "Off")}\nTool:{context.ToolId} User:{context.UserId} | Safety:{state.SafetyCode} | Err:{fault.MainCode}/{fault.SubCode}"
                    : $"[{mode}] {conn} | {overrideMessage}\nTool:{context.ToolId} User:{context.UserId} | Drag:{(state.IsInDragTeach ? "On" : "Off")} | Safety:{state.SafetyCode}";
                statusLabel.color = isConnected
                    ? UIDesignTokens.Colors.AccentSuccess
                    : UIDesignTokens.Colors.TextMuted;
            }

            if (connectButton != null)
            {
                var label = connectButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = isConnected ? "Disconnect" : "Connect";
                }
            }

            if (enableButton != null)
            {
                enableButton.interactable = isConnected;
                var label = enableButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = isEnabled ? "Disable" : "Enable";
                }
            }

            if (mockToggle != null)
            {
                mockToggle.SetIsOnWithoutNotify(connectionService.IsMockMode);
            }

            if (versionLabel != null
                && isConnected
                && !connectionService.IsMockMode
                && string.IsNullOrWhiteSpace(versionLabel.text))
            {
                versionLabel.text = $"RPC IP: {CurrentIp}\n현재 컨트롤러 기준 TCP/User 문맥 사용 (Tool {context.ToolId}, User {context.UserId})";
            }
        }

        /// <summary>
        /// 연결 끊김 상태를 표시합니다.
        /// </summary>
        public void ShowConnectionLost()
        {
            if (statusLabel != null)
            {
                statusLabel.text = "연결 끊김 \u2014 IP 확인 후 Connect, 자동모드/Drag/Fault를 순서대로 점검하세요.";
                statusLabel.color = UIDesignTokens.Colors.AccentDanger;
            }

            if (connectButton != null)
            {
                var label = connectButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "Connect";
                }
            }

            if (enableButton != null)
            {
                enableButton.interactable = false;
            }

            ClearVersionLabel();
        }

        private void FetchAndDisplayVersion()
        {
            if (connectionService == null || versionLabel == null)
            {
                return;
            }

            var versionResult = connectionService.Client.GetVersion();
            var context = connectionService.LastCoordContext;
            if (versionResult.IsSuccess)
            {
                versionLabel.text =
                    $"FW: {versionResult.Value.FirmwareVersion} | SDK: {versionResult.Value.SdkVersion}\n" +
                    $"RPC IP: {CurrentIp} | Tool {context.ToolId} / User {context.UserId}";
            }
            else
            {
                versionLabel.text = string.Empty;
            }
        }

        private void ClearVersionLabel()
        {
            if (versionLabel != null)
            {
                versionLabel.text = string.Empty;
            }
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
