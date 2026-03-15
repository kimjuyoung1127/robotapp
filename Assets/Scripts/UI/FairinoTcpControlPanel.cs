// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Globalization;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FR5 TCP 직교 좌표 제어 패널입니다.
    /// X/Y/Z (mm) + Rx/Ry/Rz (°) 입력과 MoveL/ServoCart 버튼을 제공합니다.
    /// FK 계산 결과로부터 현재 TCP 위치를 읽기 전용으로 표시합니다.
    /// </summary>
    public class FairinoTcpControlPanel : MonoBehaviour, IVisibilityControllable
    {
        private static readonly string[] Labels = { "X (mm)", "Y (mm)", "Z (mm)", "Rx (°)", "Ry (°)", "Rz (°)" };

        [SerializeField] private InputField[] tcpInputs = new InputField[6];
        [SerializeField] private Text currentTcpLabel;
        [SerializeField] private Button moveLButton;
        [SerializeField] private Button servoCartButton;
        [SerializeField] private Toggle dryRunToggle;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Font fallbackFont;

        private static readonly string[] SpeedPresetNames = { "slow", "medium", "fast" };
        private static readonly string[] SpeedPresetLabels = { "Slow 10%", "Medium 30%", "Fast 60%" };

        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private FR5KinematicsFacade kinematicsFacade;
        private FairinoMoveConfirmDialog moveConfirmDialog;
        private Button[] speedButtons;
        private string selectedSpeedPreset = "medium";
        private bool listenersBound;
        private bool dryRun = true;

        /// <summary>
        /// TCP 이동 요청 이벤트입니다. (tcpPose[6])를 전달합니다.
        /// </summary>
        public event Action<double[]> OnTcpMoveRequested;

        /// <summary>
        /// 연결 서비스, 설정, FK facade를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig, FR5KinematicsFacade facade)
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
        }

        private void OnDisable()
        {
            UnbindListeners();
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
            if (root == null || currentTcpLabel != null)
            {
                return;
            }

            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

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

            currentTcpLabel = UiRuntimeStyle.EnsureText(root, "CurrentTcpLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(currentTcpLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(380f, 40f), new Vector2(16f, 120f));
            currentTcpLabel.text = "현재 TCP: 대기 중...";

            moveLButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnMoveL", "MoveL", fallbackFont, 110f);
            UiRuntimeStyle.Anchor((RectTransform)moveLButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, 70f));

            servoCartButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnServoCart", "ServoCart", fallbackFont, 110f);
            UiRuntimeStyle.Anchor((RectTransform)servoCartButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(136f, 70f));

            dryRunToggle ??= UIComponentFactory.CreateToggle(root, "TcpDryRunToggle", "DryRun", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)dryRunToggle.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 24f), new Vector2(256f, 76f));

            feedbackLabel = UiRuntimeStyle.EnsureText(root, "TcpFeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(feedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(380f, 36f), new Vector2(16f, 16f));

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
            }

            EnsureSpeedButtons(root);
        }

        private void EnsureSpeedButtons(RectTransform root)
        {
            speedButtons = new Button[SpeedPresetNames.Length];
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                var btnName = $"BtnTcpSpeed_{SpeedPresetNames[i]}";
                var existing = root.Find(btnName)?.GetComponent<Button>();
                speedButtons[i] = existing ?? UIComponentFactory.CreateSecondaryButton(root, btnName, SpeedPresetLabels[i], fallbackFont, 100f);
                UiRuntimeStyle.Anchor((RectTransform)speedButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 108f), 48f));
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

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            moveLButton?.onClick.AddListener(OnMoveLClicked);
            servoCartButton?.onClick.AddListener(OnServoCartClicked);
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
            dryRunToggle?.onValueChanged.RemoveListener(OnDryRunChanged);

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    speedButtons[i]?.onClick.RemoveAllListeners();
                }
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

        private void OnMoveLClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            var target = GetInputValues();
            var (speed, acc) = GetSelectedSpeedAcc();

            if (dryRun)
            {
                ShowFeedback($"[DryRun] MoveL ({selectedSpeedPreset}) \u2192 X:{target[0]:F1} Y:{target[1]:F1} Z:{target[2]:F1}");
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
            OnTcpMoveRequested?.Invoke(target);
        }

        private void OnServoCartClicked()
        {
            var target = GetInputValues();
            if (dryRun)
            {
                ShowFeedback($"[DryRun] ServoCart \u2192 X:{target[0]:F1} Y:{target[1]:F1} Z:{target[2]:F1}");
                return;
            }

            ShowFeedback("ServoCart는 Live 모드에서만 사용할 수 있습니다.");
        }

        private void OnDryRunChanged(bool value)
        {
            dryRun = value;
            ShowFeedback(dryRun ? "DryRun 모드 활성" : "DryRun 모드 해제");
        }

        private void OnKinematicsUpdated(KineTutor3D.Math.Mat4D[] transforms, KineTutor3D.Math.Mat4D ee)
        {
            RefreshCurrentTcp();
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

        /// <summary>
        /// 입력 필드와 버튼의 일괄 활성/비활성을 설정합니다.
        /// </summary>
        public void SetControlsEnabled(bool enabled)
        {
            for (var i = 0; i < tcpInputs.Length; i++)
            {
                if (tcpInputs[i] != null)
                {
                    tcpInputs[i].interactable = enabled;
                }
            }

            if (moveLButton != null) moveLButton.interactable = enabled;
            if (servoCartButton != null) servoCartButton.interactable = enabled;

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    if (speedButtons[i] != null) speedButtons[i].interactable = enabled;
                }
            }
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
