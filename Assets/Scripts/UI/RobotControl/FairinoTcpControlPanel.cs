// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FR5 TCP 직교 좌표 제어 패널입니다.
    /// X/Y/Z (mm) + Rx/Ry/Rz (°) 입력과 MoveL/ServoCart/Stop 버튼을 제공합니다.
    /// FK 계산 결과로부터 현재 TCP 위치를 읽기 전용으로 표시합니다.
    /// Fill Current 버튼으로 FK 결과를 입력 필드에 자동 채울 수 있습니다.
    /// </summary>
    public class FairinoTcpControlPanel : MonoBehaviour, IVisibilityControllable
    {
        private static readonly string[] Labels = { "X (mm)", "Y (mm)", "Z (mm)", "Rx (\u00b0)", "Ry (\u00b0)", "Rz (\u00b0)" };

        [SerializeField] private InputField[] tcpInputs = new InputField[6];
        [SerializeField] private Text currentTcpLabel;
        [SerializeField] private Button moveLButton;
        [SerializeField] private Button servoCartButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button fillCurrentButton;
        [SerializeField] private Toggle dryRunToggle;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Text validationSummaryLabel;
        [SerializeField] private Text deltaSummaryLabel;
        [SerializeField] private Font fallbackFont;

        private static readonly string[] SpeedPresetNames = { "slow", "medium", "fast" };
        private static readonly string[] SpeedPresetLabels = { "Slow 10%", "Medium 30%", "Fast 60%" };

        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private RobotKinematicsFacade kinematicsFacade;
        private FairinoMoveConfirmDialog moveConfirmDialog;
        private Button[] speedButtons;
        private string selectedSpeedPreset = "medium";
        private readonly UnityAction<string>[] inputChangedListeners = new UnityAction<string>[6];
        private bool listenersBound;
        private bool dryRun = true;
        private bool controlsEnabled = true;
        private bool serviceSubscribed;

        /// <summary>
        /// 현재 패널 피드백 텍스트입니다.
        /// </summary>
        public string CurrentFeedbackText => feedbackLabel != null ? feedbackLabel.text : string.Empty;

        /// <summary>
        /// 현재 TCP 요약 텍스트입니다.
        /// </summary>
        public string CurrentTcpSummaryText => currentTcpLabel != null ? currentTcpLabel.text : string.Empty;

        /// <summary>
        /// TCP 이동 요청 이벤트입니다. (tcpPose[6])를 전달합니다.
        /// </summary>
        public event Action<double[]> OnTcpMoveRequested;

        /// <summary>
        /// 연결 서비스, 설정, FK facade를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig, RobotKinematicsFacade facade)
        {
            connectionService = service;
            config = robotConfig;
            kinematicsFacade = facade;
            EnsurePresentation();

            if (kinematicsFacade != null)
            {
                kinematicsFacade.OnKinematicsUpdated -= OnKinematicsUpdated;
                kinematicsFacade.OnKinematicsUpdated += OnKinematicsUpdated;
                RefreshCurrentTcp();
            }

            SubscribeService();
            RefreshActionAvailability();
        }

        /// <summary>
        /// MoveL 확인 대화상자를 주입합니다.
        /// </summary>
        public void InjectMoveConfirmDialog(FairinoMoveConfirmDialog dialog)
        {
            moveConfirmDialog = dialog;
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
            RefreshActionAvailability();
        }

        private void OnDisable()
        {
            UnbindListeners();
            UnsubscribeService();
        }

        private void OnDestroy()
        {
            if (kinematicsFacade != null)
            {
                kinematicsFacade.OnKinematicsUpdated -= OnKinematicsUpdated;
            }
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
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
                if (dryRunToggle != null)
                {
                    dryRunToggle.SetIsOnWithoutNotify(dryRun);
                }

                RefreshPredictionSummary();
                return;
            }

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 22f), new Vector2(16f, -14f));
            title.text = "TCP Control";

            for (var i = 0; i < 6; i++)
            {
                var row = UiRuntimeStyle.EnsureRectChild(root, $"TcpRow_{i}");
                UiRuntimeStyle.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 34f), new Vector2(16f, -46f - (i * 36f)));
                var rowBg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
                rowBg.color = UIDesignTokens.Colors.SurfaceCard;

                var label = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextSecondary);
                UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(72f, 20f), new Vector2(8f, 0f));
                label.text = Labels[i];

                tcpInputs[i] = row.Find("TcpInput")?.GetComponent<InputField>();
                if (tcpInputs[i] == null)
                {
                    tcpInputs[i] = UIComponentFactory.CreateInputField(row, "TcpInput", "0.0", fallbackFont);
                }

                UiRuntimeStyle.Anchor((RectTransform)tcpInputs[i].transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 26f), new Vector2(86f, 0f));
                tcpInputs[i].contentType = InputField.ContentType.DecimalNumber;
            }

            // 현재 TCP 위치 (읽기 전용) + Fill Current 버튼
            currentTcpLabel = UiRuntimeStyle.EnsureText(root, "CurrentTcpLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(currentTcpLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(280f, 36f), new Vector2(16f, 152f));
            currentTcpLabel.text = "현재 TCP: 대기 중...";

            fillCurrentButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnFillCurrent", "Fill Current", fallbackFont, 96f);
            UiRuntimeStyle.Anchor((RectTransform)fillCurrentButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(96f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(306f, 156f));

            validationSummaryLabel = UiRuntimeStyle.EnsureText(root, "ValidationSummary", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(validationSummaryLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 18f), new Vector2(16f, 128f));

            deltaSummaryLabel = UiRuntimeStyle.EnsureText(root, "DeltaSummary", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(deltaSummaryLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 18f), new Vector2(16f, 110f));

            // Speed 버튼
            EnsureSpeedButtons(root);

            // Action 버튼: MoveL / ServoCart / Stop
            moveLButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnMoveL", "MoveL", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)moveLButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 68f));

            servoCartButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnServoCart", "ServoCart", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)servoCartButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(116f, 68f));

            stopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnStop", "Stop", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)stopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(216f, 68f));

            dryRunToggle ??= UIComponentFactory.CreateToggle(root, "TcpDryRunToggle", "DryRun", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)dryRunToggle.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(120f, 24f), new Vector2(296f, 72f));

            feedbackLabel = UiRuntimeStyle.EnsureText(root, "TcpFeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(feedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 24f), new Vector2(16f, 8f));

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
            }

            RefreshPredictionSummary();
        }

        private void SubscribeService()
        {
            if (serviceSubscribed || connectionService == null)
            {
                return;
            }

            connectionService.OnModeChanged += HandleModeChanged;
            serviceSubscribed = true;
        }

        private void UnsubscribeService()
        {
            if (!serviceSubscribed || connectionService == null)
            {
                return;
            }

            connectionService.OnModeChanged -= HandleModeChanged;
            serviceSubscribed = false;
        }

        private void HandleModeChanged(bool _)
        {
            RefreshActionAvailability();
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            currentTcpLabel = root.Find("CurrentTcpLabel")?.GetComponent<Text>();
            fillCurrentButton = root.Find("BtnFillCurrent")?.GetComponent<Button>();
            validationSummaryLabel = root.Find("ValidationSummary")?.GetComponent<Text>();
            deltaSummaryLabel = root.Find("DeltaSummary")?.GetComponent<Text>();
            moveLButton = root.Find("BtnMoveL")?.GetComponent<Button>();
            servoCartButton = root.Find("BtnServoCart")?.GetComponent<Button>();
            stopButton = root.Find("BtnStop")?.GetComponent<Button>();
            dryRunToggle = root.Find("TcpDryRunToggle")?.GetComponent<Toggle>();
            feedbackLabel = root.Find("TcpFeedbackLabel")?.GetComponent<Text>();

            for (var i = 0; i < 6; i++)
            {
                tcpInputs[i] = root.Find($"TcpRow_{i}/TcpInput")?.GetComponent<InputField>();
            }

            speedButtons = new Button[SpeedPresetNames.Length];
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                speedButtons[i] = root.Find($"BtnTcpSpeed_{SpeedPresetNames[i]}")?.GetComponent<Button>();
            }

            if (currentTcpLabel == null
                || fillCurrentButton == null
                || validationSummaryLabel == null
                || deltaSummaryLabel == null
                || moveLButton == null
                || servoCartButton == null
                || stopButton == null
                || dryRunToggle == null
                || feedbackLabel == null)
            {
                return false;
            }

            for (var i = 0; i < tcpInputs.Length; i++)
            {
                if (tcpInputs[i] == null)
                {
                    return false;
                }

                tcpInputs[i].contentType = InputField.ContentType.DecimalNumber;
            }

            for (var i = 0; i < speedButtons.Length; i++)
            {
                if (speedButtons[i] == null)
                {
                    return false;
                }
            }

            var title = root.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = "TCP Control";
            }

            RefreshSpeedButtonColors();
            return true;
        }

        private void EnsureSpeedButtons(RectTransform root)
        {
            speedButtons = new Button[SpeedPresetNames.Length];
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                var btnName = $"BtnTcpSpeed_{SpeedPresetNames[i]}";
                var existing = root.Find(btnName)?.GetComponent<Button>();
                speedButtons[i] = existing ?? UIComponentFactory.CreateSecondaryButton(root, btnName, SpeedPresetLabels[i], fallbackFont, 100f);
                UiRuntimeStyle.Anchor((RectTransform)speedButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 108f), 104f));
            }

            RefreshSpeedButtonColors();
        }

        /// <summary>
        /// 선택된 속도 프리셋에 해당하는 속도/가속을 반환합니다.
        /// </summary>
        public (int speed, int acc) GetSelectedSpeedAcc()
        {
            return config != null ? config.GetSpeedAcc(selectedSpeedPreset) : (30, 50);
        }

        private void OnSpeedSelected(int index)
        {
            if (index < 0 || index >= SpeedPresetNames.Length)
            {
                return;
            }

            selectedSpeedPreset = SpeedPresetNames[index];
            RefreshSpeedButtonColors();
            ShowFeedback($"속도: {SpeedPresetLabels[index]}");
        }

        private void RefreshSpeedButtonColors()
        {
            if (speedButtons == null)
            {
                return;
            }

            for (var i = 0; i < speedButtons.Length; i++)
            {
                if (speedButtons[i] == null)
                {
                    continue;
                }

                var isSelected = SpeedPresetNames[i] == selectedSpeedPreset;
                speedButtons[i].colors = isSelected
                    ? UIDesignTokens.ButtonColors(UIDesignTokens.Colors.AccentPrimary)
                    : UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);

                var label = speedButtons[i].GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.color = isSelected ? UIDesignTokens.Colors.TextOnAccent : UIDesignTokens.Colors.TextSecondary;
                }
            }
        }

        private void RefreshActionAvailability()
        {
            var liveUnsupported = connectionService != null && !connectionService.IsMockMode;

            if (moveLButton != null)
            {
                moveLButton.interactable = controlsEnabled;
            }

            if (servoCartButton != null)
            {
                servoCartButton.interactable = controlsEnabled && !liveUnsupported;
            }

            if (stopButton != null)
            {
                stopButton.interactable = controlsEnabled;
            }

            if (fillCurrentButton != null)
            {
                fillCurrentButton.interactable = controlsEnabled;
            }
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            moveLButton?.onClick.AddListener(OnMoveLClicked);
            servoCartButton?.onClick.AddListener(OnServoCartClicked);
            stopButton?.onClick.AddListener(OnStopClicked);
            fillCurrentButton?.onClick.AddListener(OnFillCurrentClicked);
            dryRunToggle?.onValueChanged.AddListener(OnDryRunChanged);

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    if (speedButtons[i] == null) continue;
                    var capturedIndex = i;
                    speedButtons[i].onClick.AddListener(() => OnSpeedSelected(capturedIndex));
                }
            }

            for (var i = 0; i < tcpInputs.Length; i++)
            {
                if (tcpInputs[i] == null)
                {
                    continue;
                }

                inputChangedListeners[i] = _ => RefreshPredictionSummary();
                tcpInputs[i].onValueChanged.AddListener(inputChangedListeners[i]);
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            moveLButton?.onClick.RemoveListener(OnMoveLClicked);
            servoCartButton?.onClick.RemoveListener(OnServoCartClicked);
            stopButton?.onClick.RemoveListener(OnStopClicked);
            fillCurrentButton?.onClick.RemoveListener(OnFillCurrentClicked);
            dryRunToggle?.onValueChanged.RemoveListener(OnDryRunChanged);

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    speedButtons[i]?.onClick.RemoveAllListeners();
                }
            }

            for (var i = 0; i < tcpInputs.Length; i++)
            {
                if (tcpInputs[i] != null && inputChangedListeners[i] != null)
                {
                    tcpInputs[i].onValueChanged.RemoveListener(inputChangedListeners[i]);
                }

                inputChangedListeners[i] = null;
            }

            listenersBound = false;
        }

        private double[] GetInputValues()
        {
            var values = new double[6];
            for (var i = 0; i < 6; i++)
            {
                if (tcpInputs[i] != null && double.TryParse(tcpInputs[i].text, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    values[i] = val;
                }
            }

            return values;
        }

        private bool TryGetValidatedInputValues(out double[] values, out string validationMessage)
        {
            values = new double[6];
            validationMessage = "입력 검증 통과";

            for (var i = 0; i < 6; i++)
            {
                if (tcpInputs[i] == null || string.IsNullOrWhiteSpace(tcpInputs[i].text))
                {
                    validationMessage = $"{Labels[i]} 값을 입력하세요.";
                    return false;
                }

                if (!double.TryParse(tcpInputs[i].text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    || double.IsNaN(parsed)
                    || double.IsInfinity(parsed))
                {
                    validationMessage = $"{Labels[i]} 값 형식을 확인하세요.";
                    return false;
                }

                if (i >= 3 && System.Math.Abs(parsed) > 360.0)
                {
                    validationMessage = $"{Labels[i]} 는 -360°~360° 범위를 권장합니다.";
                    return false;
                }

                values[i] = parsed;
            }

            return true;
        }

        private void OnMoveLClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            if (!TryGetValidatedInputValues(out var target, out var validationMessage))
            {
                ShowFeedback(validationMessage);
                RefreshPredictionSummary(validationMessage);
                return;
            }

            var (speed, acc) = GetSelectedSpeedAcc();

            if (dryRun)
            {
                ShowFeedback($"[DryRun] MoveL ({selectedSpeedPreset}) \u2192 X:{target[0]:F1} Y:{target[1]:F1} Z:{target[2]:F1}");
                RefreshPredictionSummary();
                OnTcpMoveRequested?.Invoke(target);
                return;
            }

            if (!connectionService.IsMockMode && moveConfirmDialog != null)
            {
                var msg = $"Live 모드에서 MoveL을 실행합니다.\n속도: {selectedSpeedPreset}\n목표: X:{target[0]:F1} Y:{target[1]:F1} Z:{target[2]:F1}";
                var capturedSpeed = speed;
                var capturedAcc = acc;
                var capturedTarget = target;
                moveConfirmDialog.Show(msg, () =>
                {
                    var result = connectionService.Client.MoveL(capturedTarget, capturedSpeed, capturedAcc);
                    ShowFeedback(result.Message);
                });
                return;
            }

            var moveResult = connectionService.Client.MoveL(target, speed, acc);
            ShowFeedback(moveResult.Message);
            RefreshPredictionSummary();
            OnTcpMoveRequested?.Invoke(target);
        }

        private void OnServoCartClicked()
        {
            if (!TryGetValidatedInputValues(out var target, out var validationMessage))
            {
                ShowFeedback(validationMessage);
                RefreshPredictionSummary(validationMessage);
                return;
            }

            if (dryRun)
            {
                ShowFeedback($"[DryRun] ServoCart \u2192 X:{target[0]:F1} Y:{target[1]:F1} Z:{target[2]:F1}");
                RefreshPredictionSummary();
                return;
            }

            ShowFeedback("ServoCart는 v1 하드웨어 bring-up 범위에서 비활성화되어 있습니다. Live에서는 MoveL만 사용하세요.");
        }

        private void OnStopClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            var result = connectionService.StopMotion();
            ShowFeedback(result.Message);
        }

        private void OnFillCurrentClicked()
        {
            if (kinematicsFacade == null)
            {
                ShowFeedback("FK 데이터 없음");
                return;
            }

            var ee = kinematicsFacade.EndEffectorTransform;
            var pos = ee.ExtractPosition();

            // FK 위치를 mm로 변환 (DH 파라미터는 미터 단위)
            SetInputValue(0, pos.X * 1000.0);
            SetInputValue(1, pos.Y * 1000.0);
            SetInputValue(2, pos.Z * 1000.0);

            // 회전 행렬에서 ZYX 오일러각 추출 (degrees)
            var rot = ee.ExtractRotation();
            var (rx, ry, rz) = ExtractEulerZYX(rot);
            SetInputValue(3, rx * (180.0 / System.Math.PI));
            SetInputValue(4, ry * (180.0 / System.Math.PI));
            SetInputValue(5, rz * (180.0 / System.Math.PI));

            ShowFeedback("현재 FK 위치로 채움 완료");
            RefreshPredictionSummary();
        }

        private void SetInputValue(int index, double value)
        {
            if (index >= 0 && index < tcpInputs.Length && tcpInputs[index] != null)
            {
                tcpInputs[index].text = value.ToString("F1", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// 회전 행렬에서 ZYX 오일러각(라디안)을 추출합니다.
        /// </summary>
        private static (double rx, double ry, double rz) ExtractEulerZYX(KineTutor3D.Math.Mat3D rot)
        {
            var r20 = rot[2, 0];
            var cosB = System.Math.Sqrt(rot[0, 0] * rot[0, 0] + rot[1, 0] * rot[1, 0]);

            if (cosB < 1e-6)
            {
                // 김벌락 (Gimbal lock)
                var ryLock = r20 < 0 ? System.Math.PI / 2.0 : -System.Math.PI / 2.0;
                return (0.0, ryLock, System.Math.Atan2(rot[0, 1], rot[1, 1]));
            }

            var ry = System.Math.Atan2(-r20, cosB);
            var rx = System.Math.Atan2(rot[2, 1], rot[2, 2]);
            var rz = System.Math.Atan2(rot[1, 0], rot[0, 0]);
            return (rx, ry, rz);
        }

        private void OnDryRunChanged(bool value)
        {
            dryRun = value;
            ShowFeedback(dryRun ? "DryRun 모드 활성" : "DryRun 모드 해제");
            RefreshPredictionSummary();
        }

        private void OnKinematicsUpdated(KineTutor3D.Math.Mat4D[] transforms, KineTutor3D.Math.Mat4D ee)
        {
            RefreshCurrentTcp();
            RefreshPredictionSummary();
        }

        private void RefreshCurrentTcp()
        {
            if (currentTcpLabel == null || kinematicsFacade == null)
            {
                return;
            }

            var pos = kinematicsFacade.EndEffectorTransform.ExtractPosition();
            currentTcpLabel.text = $"현재 TCP (FK): X:{pos.X.ToString("F3", CultureInfo.InvariantCulture)}m "
                + $"Y:{pos.Y.ToString("F3", CultureInfo.InvariantCulture)}m "
                + $"Z:{pos.Z.ToString("F3", CultureInfo.InvariantCulture)}m";
        }

        private void RefreshPredictionSummary(string overrideValidation = null)
        {
            if (validationSummaryLabel == null || deltaSummaryLabel == null)
            {
                return;
            }

            if (kinematicsFacade == null)
            {
                validationSummaryLabel.text = "검증: FK 데이터 대기 중";
                validationSummaryLabel.color = UIDesignTokens.Colors.TextMuted;
                deltaSummaryLabel.text = "예측 Δ: 계산 불가";
                return;
            }

            if (!TryGetValidatedInputValues(out var values, out var validationMessage))
            {
                validationSummaryLabel.text = $"검증: {overrideValidation ?? validationMessage}";
                validationSummaryLabel.color = UIDesignTokens.Colors.AccentWarning;
                deltaSummaryLabel.text = "예측 Δ: 입력 보정 후 계산";
                return;
            }

            validationSummaryLabel.text = $"검증: {overrideValidation ?? validationMessage}";
            validationSummaryLabel.color = UIDesignTokens.Colors.AccentSuccess;

            var currentPos = kinematicsFacade.EndEffectorTransform.ExtractPosition();
            var dx = values[0] / 1000.0 - currentPos.X;
            var dy = values[1] / 1000.0 - currentPos.Y;
            var dz = values[2] / 1000.0 - currentPos.Z;
            deltaSummaryLabel.text =
                $"예측 ΔTCP: X {dx.ToString("+0.000;-0.000", CultureInfo.InvariantCulture)}m  " +
                $"Y {dy.ToString("+0.000;-0.000", CultureInfo.InvariantCulture)}m  " +
                $"Z {dz.ToString("+0.000;-0.000", CultureInfo.InvariantCulture)}m";
        }

        /// <summary>
        /// 입력 필드와 버튼의 일괄 활성/비활성을 설정합니다.
        /// </summary>
        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            for (var i = 0; i < tcpInputs.Length; i++)
            {
                if (tcpInputs[i] != null)
                {
                    tcpInputs[i].interactable = enabled;
                }
            }

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    if (speedButtons[i] != null) speedButtons[i].interactable = enabled;
                }
            }

            RefreshActionAvailability();
        }

        private void ShowFeedback(string text)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = text;
            }
        }
    }
}
