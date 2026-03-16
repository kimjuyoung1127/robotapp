// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControl 우측 상단에서 열리는 연결/진단 정보 서랍입니다.
    /// 조작 패널을 과도하게 넓히지 않고, 버전/상태/최근 피드백/오류 힌트를 별도 레이어로 제공합니다.
    /// </summary>
    public class RobotControlDiagnosticsDrawer : MonoBehaviour, IVisibilityControllable
    {
        private const float DrawerWidth = 360f;
        private const float DrawerHeight = 452f;
        private const float OpenAnchorX = -409.2f;
        private const float OpenAnchorY = -86.7f;
        private const float ClosedAnchorX = 380f;

        [SerializeField] private RectTransform drawerPanel;
        [SerializeField] private Image backdrop;
        [SerializeField] private Text connectionSummaryLabel;
        [SerializeField] private Text versionSummaryLabel;
        [SerializeField] private Text recentFeedbackLabel;
        [SerializeField] private Text errorSummaryLabel;
        [SerializeField] private Text retryHintLabel;
        [SerializeField] private Text logPlaceholderLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Font fallbackFont;

        private FairinoConnectionService connectionService;
        private FairinoConnectionPanel connectionPanel;
        private FairinoJointControlPanel jointControlPanel;
        private FairinoTcpControlPanel tcpControlPanel;
        private string cachedVersionText = "FW/SDK: 대기 중";
        private string lastErrorText = "최근 오류 없음";
        private string lastServiceEventText = "최근 이벤트 없음";
        private float slide;
        private float targetSlide;
        private bool listenersBound;

        /// <summary>
        /// 서랍 표시 여부입니다.
        /// </summary>
        public bool IsVisible => targetSlide > 0.5f;

        /// <summary>
        /// 진단 데이터 소스를 주입합니다.
        /// </summary>
        public void Inject(
            FairinoConnectionService service,
            FairinoConnectionPanel connectionView,
            FairinoJointControlPanel jointView,
            FairinoTcpControlPanel tcpView)
        {
            UnsubscribeService();
            connectionService = service;
            connectionPanel = connectionView;
            jointControlPanel = jointView;
            tcpControlPanel = tcpView;
            EnsurePresentation();
            SubscribeService();
            RefreshContent();
        }

        private void Awake()
        {
            EnsurePresentation();
            SetVisible(false);
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindListeners();
            SubscribeService();
            RefreshContent();
        }

        private void OnDisable()
        {
            UnbindListeners();
            UnsubscribeService();
        }

        private void Update()
        {
            if (drawerPanel == null)
            {
                return;
            }

            slide = Mathf.MoveTowards(slide, targetSlide, Time.unscaledDeltaTime * 6f);
            drawerPanel.anchoredPosition = new Vector2(
                Mathf.Lerp(ClosedAnchorX, OpenAnchorX, slide),
                OpenAnchorY);
            if (backdrop != null)
            {
                var color = backdrop.color;
                color.a = 0.08f * slide;
                backdrop.color = color;
                backdrop.raycastTarget = false;
            }

            if (gameObject.activeSelf && slide <= 0.001f && targetSlide <= 0.001f)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 서랍 표시 상태를 토글합니다.
        /// </summary>
        public void Toggle()
        {
            SetVisible(!IsVisible);
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            EnsurePresentation();
            if (visible)
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

                RefreshContent();
                targetSlide = 1f;
                return;
            }

            targetSlide = 0f;
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            UiRuntimeStyle.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            if (TryBindExistingPresentation(root))
            {
                slide = Mathf.Clamp01(slide);
                drawerPanel.anchoredPosition = new Vector2(
                    Mathf.Lerp(ClosedAnchorX, OpenAnchorX, slide),
                    OpenAnchorY);
                return;
            }

            backdrop = UiRuntimeStyle.EnsureImage(root, "Backdrop", new Color(0f, 0f, 0f, 0.08f));
            UiRuntimeStyle.Stretch((RectTransform)backdrop.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            backdrop.raycastTarget = false;

            var backdropButton = backdrop.GetComponent<Button>() ?? backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.enabled = false;

            drawerPanel = UiRuntimeStyle.EnsureRectChild(root, "DrawerPanel");
            UiRuntimeStyle.Anchor(drawerPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(DrawerWidth, DrawerHeight), new Vector2(OpenAnchorX, OpenAnchorY));
            var panelBg = drawerPanel.GetComponent<Image>() ?? drawerPanel.gameObject.AddComponent<Image>();
            panelBg.color = UIDesignTokens.Colors.SurfaceRaised;

            var title = UiRuntimeStyle.EnsureText(drawerPanel, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 20f), new Vector2(16f, -14f));
            title.text = "Diagnostics";

            closeButton ??= UIComponentFactory.CreateSecondaryButton(drawerPanel, "BtnCloseDiagnostics", "닫기", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)closeButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-16f, -10f));

            var sectionY = -48f;
            connectionSummaryLabel = EnsureSection(drawerPanel, "ConnectionSummary", "연결 상태", ref sectionY);
            versionSummaryLabel = EnsureSection(drawerPanel, "VersionSummary", "버전 / 환경", ref sectionY);
            recentFeedbackLabel = EnsureSection(drawerPanel, "RecentFeedback", "최근 피드백", ref sectionY);
            errorSummaryLabel = EnsureSection(drawerPanel, "ErrorSummary", "최근 오류", ref sectionY);
            retryHintLabel = EnsureSection(drawerPanel, "RetryHint", "재시도 힌트", ref sectionY);
            logPlaceholderLabel = EnsureSection(drawerPanel, "LogPlaceholder", "추가 예정", ref sectionY);
            logPlaceholderLabel.text = "로그 수집 / 복사 버튼은 다음 라운드에서 연결합니다.";

            slide = Mathf.Clamp01(slide);
            drawerPanel.anchoredPosition = new Vector2(
                Mathf.Lerp(ClosedAnchorX, OpenAnchorX, slide),
                OpenAnchorY);
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            backdrop = root.Find("Backdrop")?.GetComponent<Image>();
            drawerPanel = root.Find("DrawerPanel") as RectTransform;
            connectionSummaryLabel = drawerPanel?.Find("ConnectionSummaryBody")?.GetComponent<Text>();
            versionSummaryLabel = drawerPanel?.Find("VersionSummaryBody")?.GetComponent<Text>();
            recentFeedbackLabel = drawerPanel?.Find("RecentFeedbackBody")?.GetComponent<Text>();
            errorSummaryLabel = drawerPanel?.Find("ErrorSummaryBody")?.GetComponent<Text>();
            retryHintLabel = drawerPanel?.Find("RetryHintBody")?.GetComponent<Text>();
            logPlaceholderLabel = drawerPanel?.Find("LogPlaceholderBody")?.GetComponent<Text>();
            closeButton = drawerPanel?.Find("BtnCloseDiagnostics")?.GetComponent<Button>();

            if (backdrop == null
                || drawerPanel == null
                || connectionSummaryLabel == null
                || versionSummaryLabel == null
                || recentFeedbackLabel == null
                || errorSummaryLabel == null
                || retryHintLabel == null
                || logPlaceholderLabel == null
                || closeButton == null)
            {
                return false;
            }

            var title = drawerPanel.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = "Diagnostics";
            }

            logPlaceholderLabel.text = "로그 수집 / 복사 버튼은 다음 라운드에서 연결합니다.";

            var backdropButton = backdrop.GetComponent<Button>() ?? backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.enabled = false;
            backdrop.raycastTarget = false;
            return true;
        }

        private static Text EnsureSection(RectTransform parent, string key, string title, ref float currentY)
        {
            var titleText = UiRuntimeStyle.EnsureText(parent, $"{key}Title", UiRuntimeStyle.ResolveFont(null), UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 14f), new Vector2(16f, currentY));
            titleText.text = title;

            var bodyText = UiRuntimeStyle.EnsureText(parent, $"{key}Body", UiRuntimeStyle.ResolveFont(null), UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 40f), new Vector2(16f, currentY - 18f));

            currentY -= 64f;
            return bodyText;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            closeButton?.onClick.AddListener(Close);
            backdrop?.GetComponent<Button>()?.onClick.AddListener(Close);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            closeButton?.onClick.RemoveListener(Close);
            backdrop?.GetComponent<Button>()?.onClick.RemoveListener(Close);
            listenersBound = false;
        }

        private void SubscribeService()
        {
            if (connectionService == null)
            {
                return;
            }

            connectionService.OnConnectionStateChanged -= HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged -= HandleEnableStateChanged;
            connectionService.OnModeChanged -= HandleModeChanged;
            connectionService.OnError -= HandleError;

            connectionService.OnConnectionStateChanged += HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged += HandleEnableStateChanged;
            connectionService.OnModeChanged += HandleModeChanged;
            connectionService.OnError += HandleError;
        }

        private void UnsubscribeService()
        {
            if (connectionService == null)
            {
                return;
            }

            connectionService.OnConnectionStateChanged -= HandleConnectionStateChanged;
            connectionService.OnEnableStateChanged -= HandleEnableStateChanged;
            connectionService.OnModeChanged -= HandleModeChanged;
            connectionService.OnError -= HandleError;
        }

        private void HandleConnectionStateChanged(bool connected)
        {
            lastServiceEventText = connected ? "Connect 성공" : "Disconnect 또는 연결 해제";
            if (connected)
            {
                FetchVersionInfo();
            }
            else
            {
                cachedVersionText = "FW/SDK: 대기 중";
            }

            RefreshContent();
        }

        private void HandleEnableStateChanged(bool enabled)
        {
            lastServiceEventText = enabled ? "Servo Enable" : "Servo Disable";
            RefreshContent();
        }

        private void HandleModeChanged(bool isMock)
        {
            lastServiceEventText = isMock ? "Mock 모드 전환" : "Live 모드 전환";
            RefreshContent();
        }

        private void HandleError(FairinoResult result)
        {
            lastErrorText = $"[{result.ErrorCode}] {result.Message}";
            lastServiceEventText = result.Message;
            RefreshContent();
        }

        private void FetchVersionInfo()
        {
            if (connectionService == null || !connectionService.Client.IsConnected)
            {
                cachedVersionText = "FW/SDK: 대기 중";
                return;
            }

            var versionResult = connectionService.Client.GetVersion();
            cachedVersionText = versionResult.IsSuccess
                ? $"FW: {versionResult.Value.FirmwareVersion}\nSDK: {versionResult.Value.SdkVersion}\nController: {(connectionService.IsMockMode ? "Mock" : "Live")}"
                : versionResult.Message;
        }

        private void RefreshContent()
        {
            if (connectionSummaryLabel == null)
            {
                return;
            }

            var isConnected = connectionService != null && connectionService.Client.IsConnected;
            var isEnabled = connectionService != null && connectionService.Client.IsEnabled;
            var modeText = connectionService == null || connectionService.IsMockMode ? "Mock" : "Live";
            var ipText = connectionPanel != null && !string.IsNullOrWhiteSpace(connectionPanel.CurrentIp)
                ? connectionPanel.CurrentIp
                : "IP 미지정";
            connectionSummaryLabel.text = $"Mode: {modeText}\nConnected: {(isConnected ? "Yes" : "No")}\nEnabled: {(isEnabled ? "Yes" : "No")}\nIP: {ipText}";

            versionSummaryLabel.text = !string.IsNullOrWhiteSpace(connectionPanel != null ? connectionPanel.CurrentVersionText : null)
                ? connectionPanel.CurrentVersionText
                : cachedVersionText;

            var jointFeedback = jointControlPanel != null ? jointControlPanel.CurrentFeedbackText : string.Empty;
            var tcpFeedback = tcpControlPanel != null ? tcpControlPanel.CurrentFeedbackText : string.Empty;
            recentFeedbackLabel.text = !string.IsNullOrWhiteSpace(tcpFeedback)
                ? tcpFeedback
                : !string.IsNullOrWhiteSpace(jointFeedback)
                    ? jointFeedback
                    : lastServiceEventText;

            errorSummaryLabel.text = lastErrorText;
            retryHintLabel.text = ResolveRetryHint(isConnected, isEnabled, modeText);
        }

        private static string ResolveRetryHint(bool isConnected, bool isEnabled, string modeText)
        {
            if (!isConnected)
            {
                return "케이블/IP를 확인하고 Connect를 다시 시도하세요.";
            }

            if (!isEnabled)
            {
                return "실제 동작 전 Enable을 눌러 서보를 활성화하세요.";
            }

            if (modeText == "Mock")
            {
                return "실기 검증 시 Mock을 끄고 DryRun으로 먼저 확인하세요.";
            }

            return "Live 모드에서는 DryRun 확인 후 MoveJ/MoveL을 권장합니다.";
        }

        private void Close()
        {
            SetVisible(false);
        }
    }
}
