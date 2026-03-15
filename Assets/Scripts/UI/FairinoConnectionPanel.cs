// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Toggle mockToggle;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text versionLabel;
        [SerializeField] private Font fallbackFont;

        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private bool listenersBound;

        /// <summary>
        /// 연결 서비스를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig)
        {
            UnsubscribeService();
            connectionService = service;
            config = robotConfig;
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

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 22f), new Vector2(16f, -14f));
            title.text = "FR5 Connection";

            var ipLabel = UiRuntimeStyle.EnsureText(root, "IpLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(ipLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, 18f), new Vector2(16f, -46f));
            ipLabel.text = "IP Address";

            ipInput ??= UIComponentFactory.CreateInputField(root, "IpInput", "192.168.58.2", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)ipInput.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, -72f));

            connectButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnConnect", "Connect", fallbackFont, 124f);
            UiRuntimeStyle.Anchor((RectTransform)connectButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(124f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(212f, -72f));

            enableButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnEnable", "Enable", fallbackFont, 124f);
            UiRuntimeStyle.Anchor((RectTransform)enableButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(124f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(212f, -116f));

            mockToggle ??= UIComponentFactory.CreateToggle(root, "MockToggle", "Mock Mode", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)mockToggle.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 24f), new Vector2(16f, -122f));
            var mockLabel = mockToggle.transform.Find("Label")?.GetComponent<Text>();
            if (mockLabel != null) mockLabel.text = "Mock Mode";

            statusLabel = UiRuntimeStyle.EnsureText(root, "StatusLabel", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(statusLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 24f), new Vector2(16f, 36f));

            versionLabel = UiRuntimeStyle.EnsureText(root, "VersionLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(versionLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 20f), new Vector2(16f, 16f));
            versionLabel.text = string.Empty;
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

            var ip = ipInput != null ? ipInput.text : "192.168.58.2";
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

            if (statusLabel != null)
            {
                statusLabel.text = string.IsNullOrWhiteSpace(overrideMessage)
                    ? $"[{mode}] {conn} | 서보: {servo}"
                    : $"[{mode}] {conn} | {overrideMessage}";
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
        }

        /// <summary>
        /// 연결 끊김 상태를 표시합니다.
        /// </summary>
        public void ShowConnectionLost()
        {
            if (statusLabel != null)
            {
                statusLabel.text = "연결 끊김 \u2014 재연결하세요";
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
            if (versionResult.IsSuccess)
            {
                versionLabel.text = $"FW: {versionResult.Value.FirmwareVersion} | SDK: {versionResult.Value.SdkVersion}";
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
