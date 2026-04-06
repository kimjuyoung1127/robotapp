// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using KineTutor3D.App.HandTracking;
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
        private const string DrawerTitleText = "진단";
        private const string HandInputSectionTitle = "손 입력";
        private const string HandInputUnconfiguredText = "손 입력: 미구성\n현재 씬에 hand input source가 연결되지 않았습니다.";
        private const string HandInputSuccessHintText = "Step 1 성공 기준: 여기 상태가 Fresh로 바뀌면 최소 연결 확인입니다.";
        private const string HandInputNoPacketText = "패킷이 아직 들어오지 않았습니다.";
        private const string HandInputStaleText = "최근 샘플은 있었지만 현재는 fresh 상태가 아닙니다.";

        [SerializeField] private RectTransform drawerPanel;
        [SerializeField] private Image backdrop;
        [SerializeField] private Text connectionSummaryLabel;
        [SerializeField] private Text versionSummaryLabel;
        [SerializeField] private Text recentFeedbackLabel;
        [SerializeField] private Text errorSummaryLabel;
        [SerializeField] private Text retryHintLabel;
        [SerializeField] private Text handInputLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetErrorButton;
        [SerializeField] private Font fallbackFont;

        private FairinoConnectionService connectionService;
        private FairinoConnectionPanel connectionPanel;
        private FairinoJointControlPanel jointControlPanel;
        private FairinoTcpControlPanel tcpControlPanel;
        private IHandPoseSource handPoseSource;
        private readonly HandInputDiagnosticsState handInputState = new HandInputDiagnosticsState();
        private string cachedVersionText = "FW/SDK: 대기 중";
        private string lastErrorText = "최근 오류 없음";
        private string lastServiceEventText = "최근 이벤트 없음";
        private float slide;
        private float targetSlide;
        private float nextStatusRefreshTime;
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
            FairinoTcpControlPanel tcpView,
            IHandPoseSource handSource = null)
        {
            UnsubscribeService();
            UnsubscribeHandSource();
            connectionService = service;
            connectionPanel = connectionView;
            jointControlPanel = jointView;
            tcpControlPanel = tcpView;
            handPoseSource = handSource;
            EnsurePresentation();
            SubscribeService();
            SubscribeHandSource();
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
            SubscribeHandSource();
            RefreshContent();
        }

        private void OnDisable()
        {
            UnbindListeners();
            UnsubscribeService();
            UnsubscribeHandSource();
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

            if (targetSlide > 0.001f && Time.unscaledTime >= nextStatusRefreshTime)
            {
                nextStatusRefreshTime = Time.unscaledTime + 0.25f;
                RefreshContent();
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
            title.text = DrawerTitleText;

            closeButton ??= UIComponentFactory.CreateSecondaryButton(drawerPanel, "BtnCloseDiagnostics", "닫기", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)closeButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-16f, -10f));

            resetErrorButton ??= UIComponentFactory.CreateSecondaryButton(drawerPanel, "BtnResetError", "오류 초기화", fallbackFont, 104f);
            UiRuntimeStyle.Anchor((RectTransform)resetErrorButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(104f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(-96f, -10f));

            var sectionY = -48f;
            connectionSummaryLabel = EnsureSection(drawerPanel, "ConnectionSummary", "연결 상태", ref sectionY);
            versionSummaryLabel = EnsureSection(drawerPanel, "VersionSummary", "버전 / 환경", ref sectionY);
            recentFeedbackLabel = EnsureSection(drawerPanel, "RecentFeedback", "최근 피드백", ref sectionY);
            errorSummaryLabel = EnsureSection(drawerPanel, "ErrorSummary", "최근 오류", ref sectionY);
            retryHintLabel = EnsureSection(drawerPanel, "RetryHint", "재시도 힌트", ref sectionY);
            handInputLabel = EnsureSection(drawerPanel, "LogPlaceholder", HandInputSectionTitle, ref sectionY);
            handInputLabel.text = HandInputNoPacketText;

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
            handInputLabel = drawerPanel?.Find("LogPlaceholderBody")?.GetComponent<Text>();
            closeButton = drawerPanel?.Find("BtnCloseDiagnostics")?.GetComponent<Button>();
            resetErrorButton = drawerPanel?.Find("BtnResetError")?.GetComponent<Button>();

            if (backdrop == null
                || drawerPanel == null
                || connectionSummaryLabel == null
                || versionSummaryLabel == null
                || recentFeedbackLabel == null
                || errorSummaryLabel == null
                || retryHintLabel == null
                || handInputLabel == null
                || closeButton == null
                || resetErrorButton == null)
            {
                return false;
            }

            var title = drawerPanel.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = DrawerTitleText;
            }

            var handInputTitle = drawerPanel.Find("LogPlaceholderTitle")?.GetComponent<Text>();
            if (handInputTitle != null)
            {
                handInputTitle.text = HandInputSectionTitle;
            }

            handInputLabel.text = HandInputNoPacketText;

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
            UiRuntimeStyle.Anchor(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 50f), new Vector2(16f, currentY - 18f));

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
            resetErrorButton?.onClick.AddListener(OnResetErrorClicked);
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
            resetErrorButton?.onClick.RemoveListener(OnResetErrorClicked);
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

        private void SubscribeHandSource()
        {
            if (handPoseSource == null)
            {
                return;
            }

            handPoseSource.OnSampleReceived -= HandleHandSampleReceived;
            handPoseSource.OnSampleReceived += HandleHandSampleReceived;
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

        private void UnsubscribeHandSource()
        {
            if (handPoseSource == null)
            {
                return;
            }

            handPoseSource.OnSampleReceived -= HandleHandSampleReceived;
        }

        private void HandleConnectionStateChanged(bool connected)
        {
            lastServiceEventText = connected ? "연결 성공" : "연결 해제 또는 종료";
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
            lastServiceEventText = enabled ? "서보 활성화" : "서보 비활성화";
            RefreshContent();
        }

        private void HandleModeChanged(bool isMock)
        {
            lastServiceEventText = isMock ? "모의 연결 모드 전환" : "실기 연결 모드 전환";
            RefreshContent();
        }

        private void HandleError(FairinoResult result)
        {
            lastErrorText = $"[{result.ErrorCode}] {result.Message}";
            lastServiceEventText = result.Message;
            RefreshContent();
        }

        private void HandleHandSampleReceived(HandPoseSample sample)
        {
            if (sample == null)
            {
                return;
            }
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
                ? $"FW: {versionResult.Value.FirmwareVersion}\nSDK: {versionResult.Value.SdkVersion}\nSW: {versionResult.Value.SoftwareVersion}\nCTRL: {versionResult.Value.ControllerVersion}\n컨트롤러: {(connectionService.IsMockMode ? "모의 연결" : "실기 연결")}"
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
            var modeText = connectionService == null || connectionService.IsMockMode ? "모의 연결" : "실기 연결";
            var ipText = connectionPanel != null && !string.IsNullOrWhiteSpace(connectionPanel.CurrentIp)
                ? connectionPanel.CurrentIp
                : "IP 미지정";
            var state = connectionService != null ? connectionService.LastState : default;
            var fault = connectionService != null ? connectionService.LastControllerFault : FairinoControllerFault.None();
            var context = connectionService != null ? connectionService.LastCoordContext : FairinoCoordContext.Default();
            var samplePeriod = connectionService != null ? connectionService.LastRealtimeStateSamplePeriodMs : 0;
            var safetyCode = connectionService != null ? connectionService.LastSafetyCode : 0;
            connectionSummaryLabel.text =
                $"모드: {modeText}\n" +
                $"연결: {(isConnected ? "예" : "아니오")}\n" +
                $"활성화: {(isEnabled ? "예" : "아니오")}\n" +
                $"IP: {ipText}\n" +
                $"로봇 모드: {state.RobotMode} | 드래그: {(state.IsInDragTeach ? "켜짐" : "꺼짐")}\n" +
                $"Tool/User: {context.ToolId}/{context.UserId}\n" +
                $"큐: {state.MotionQueueLength} | 안전 코드: {safetyCode}\n" +
                $"오류: {fault.MainCode}/{fault.SubCode} | 안전 정지: {(fault.IsSafetyStop ? "켜짐" : "꺼짐")}\n" +
                $"샘플 주기: {samplePeriod} ms";

            versionSummaryLabel.text = !string.IsNullOrWhiteSpace(connectionPanel != null ? connectionPanel.CurrentVersionText : null)
                ? connectionPanel.CurrentVersionText
                : $"{cachedVersionText}\n재연결: {(modeText == "실기 연결" ? "사용 (30s / 500ms)" : "모의 연결")}";

            var jointFeedback = jointControlPanel != null ? jointControlPanel.CurrentFeedbackText : string.Empty;
            var tcpFeedback = tcpControlPanel != null ? tcpControlPanel.CurrentFeedbackText : string.Empty;
            recentFeedbackLabel.text = !string.IsNullOrWhiteSpace(tcpFeedback)
                ? tcpFeedback
                : !string.IsNullOrWhiteSpace(jointFeedback)
                    ? jointFeedback
                    : $"이벤트: {lastServiceEventText}\n비상 정지: {(state.IsEmergencyStop ? "예" : "아니오")}\n충돌 감지: {(state.IsCollisionDetected ? "예" : "아니오")}";

            errorSummaryLabel.text = $"최근 오류: {lastErrorText}\n차단 오류: {(fault.HasBlockingFault ? "예" : "아니오")}";
            retryHintLabel.text = ResolveRetryHint(isConnected, isEnabled, modeText, state.IsInDragTeach, fault.HasBlockingFault);
            handInputLabel.text = BuildHandInputSummary();
        }

        private static string ResolveRetryHint(bool isConnected, bool isEnabled, string modeText, bool isInDragTeach, bool hasFault)
        {
            if (!isConnected)
            {
                return "1) IP/케이블 확인 2) Connect 재시도";
            }

            if (isInDragTeach)
            {
                return "1) Drag teach 종료 2) 자동 모드 확인 3) Enable";
            }

            if (hasFault)
            {
                return "1) 오류 초기화 2) 자동 모드 확인 3) Enable 후 재시도";
            }

            if (!isEnabled)
            {
                return "1) 자동 모드 확인 2) Enable 3) Sync";
            }

            if (modeText == "모의 연결")
            {
                return "실기 검증 시 모의 연결을 끄고 DryRun으로 먼저 확인하세요.";
            }

            return "실기 연결 모드에서는 DryRun 확인 후 MoveJ/MoveL을 권장합니다.";
        }

        private string BuildHandInputSummary()
        {
            if (handPoseSource == null)
            {
                handInputState.SetUnconfigured();
                return HandInputDiagnosticsFormatter.FormatBody(handInputState);
            }

            if (handPoseSource is UdpHandPoseReceiver udpReceiver)
            {
                var allowedSenderIp = udpReceiver.RestrictSenderIp && !string.IsNullOrWhiteSpace(udpReceiver.AllowedSenderIp)
                    ? udpReceiver.AllowedSenderIp
                    : "Any";

                if (handPoseSource.TryGetLatestSample(out var freshSample) && freshSample != null)
                {
                    handInputState.SetFresh(
                        udpReceiver.ListenPort,
                        allowedSenderIp,
                        handPoseSource.SampleTimeoutSeconds,
                        udpReceiver.LastSenderIp,
                        freshSample.seq,
                        freshSample.tracked,
                        freshSample.handX,
                        freshSample.handY,
                        freshSample.pinch,
                        freshSample.sourceId);
                    return HandInputDiagnosticsFormatter.FormatBody(handInputState);
                }

                if (handPoseSource.LatestSample != null)
                {
                    var latestSample = handPoseSource.LatestSample;
                    handInputState.SetStale(
                        udpReceiver.ListenPort,
                        allowedSenderIp,
                        handPoseSource.SampleTimeoutSeconds,
                        udpReceiver.LastSenderIp,
                        latestSample.seq,
                        latestSample.tracked,
                        latestSample.handX,
                        latestSample.handY,
                        latestSample.pinch,
                        latestSample.sourceId);
                    return HandInputDiagnosticsFormatter.FormatBody(handInputState);
                }

                handInputState.SetListening(udpReceiver.ListenPort, allowedSenderIp, udpReceiver.IsListening);
                return HandInputDiagnosticsFormatter.FormatBody(handInputState);
            }

            handInputState.SetListening(0, "Any", false);
            return HandInputDiagnosticsFormatter.FormatBody(handInputState);
        }

        private void Close()
        {
            SetVisible(false);
        }

        private void OnResetErrorClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            var result = connectionService.ResetErrors();
            lastServiceEventText = result.Message;
            if (!result.IsSuccess)
            {
                lastErrorText = $"[{result.ErrorCode}] {result.Message}";
            }

            RefreshContent();
        }
    }
}
