// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FAIRINO FR5 6축 관절 제어 패널입니다.
    /// 슬라이더, MoveJ/ServoJ 버튼, DryRun 토글, 비상정지, 프리셋 포즈를 제공합니다.
    /// 안전 모드 배너와 섹션 구분 라벨을 포함합니다.
    /// </summary>
    public class FairinoJointControlPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Slider[] jointSliders = new Slider[6];
        [SerializeField] private Text[] jointLabels = new Text[6];
        [SerializeField] private Button moveJButton;
        [SerializeField] private Button servoJButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button syncButton;
        [SerializeField] private Toggle dryRunToggle;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Image modeBannerBg;
        [SerializeField] private Text modeBannerText;
        [SerializeField] private Font fallbackFont;

        private static readonly string[] SpeedPresetNames = { "slow", "medium", "fast" };
        private static readonly string[] SpeedPresetLabels = { "Slow 10%", "Medium 30%", "Fast 60%" };

        private readonly UnityAction<float>[] sliderListeners = new UnityAction<float>[6];
        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private FairinoMoveConfirmDialog moveConfirmDialog;
        private Button[] presetButtons;
        private Button[] speedButtons;
        private string selectedSpeedPreset = "medium";
        private bool listenersBound;
        private bool serviceSubscribed;
        private bool dryRun = true;

        /// <summary>
        /// 슬라이더 프리뷰 이벤트입니다. 슬라이더 드래그 시 6축 각도를 emit합니다.
        /// </summary>
        public event Action<double[]> OnJointSliderPreview;

        /// <summary>
        /// 프리셋 적용 이벤트입니다. 프리셋 버튼 클릭 시 6축 각도를 emit합니다.
        /// 슬라이더 프리뷰와 분리하여 EE 궤적 생성을 방지합니다.
        /// </summary>
        public event Action<double[]> OnPresetApplied;

        /// <summary>
        /// Sync 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnSyncRequested;

        /// <summary>
        /// 연결 서비스와 설정을 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig)
        {
            UnsubscribeService();
            connectionService = service;
            config = robotConfig;
            EnsurePresentation();
            InitSliders();
            SubscribeService();
            RefreshModeIndicator();
        }

        /// <summary>
        /// MoveJ 확인 대화상자를 주입합니다.
        /// </summary>
        public void InjectMoveConfirmDialog(FairinoMoveConfirmDialog dialog)
        {
            moveConfirmDialog = dialog;
        }

        /// <summary>
        /// 슬라이더 값을 외부에서 동기화합니다 (상태 폴링 시 사용).
        /// </summary>
        public void SetSliderValues(double[] values)
        {
            if (values == null || values.Length < 6)
            {
                return;
            }

            for (var i = 0; i < 6 && i < jointSliders.Length; i++)
            {
                if (jointSliders[i] != null)
                {
                    jointSliders[i].SetValueWithoutNotify((float)values[i]);
                    UpdateJointLabel(i);
                }
            }
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
            RefreshModeIndicator();
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
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 22f), new Vector2(16f, -12f));
            title.text = "Joint Control";

            // 안전 모드 배너
            var bannerRect = UiRuntimeStyle.EnsureRectChild(root, "ModeBanner");
            UiRuntimeStyle.Anchor(bannerRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 24f), new Vector2(16f, -38f));
            modeBannerBg = bannerRect.GetComponent<Image>() ?? bannerRect.gameObject.AddComponent<Image>();
            modeBannerBg.color = UIDesignTokens.Colors.AccentSuccess;

            modeBannerText = UiRuntimeStyle.EnsureText(bannerRect, "BannerText", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextOnAccent);
            UiRuntimeStyle.Stretch(modeBannerText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 6축 슬라이더 (배너 아래)
            for (var i = 0; i < 6; i++)
            {
                var row = UiRuntimeStyle.EnsureRectChild(root, $"JointRow_{i + 1}");
                UiRuntimeStyle.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 44f), new Vector2(16f, -68f - (i * 46f)));
                var rowBg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
                rowBg.color = UIDesignTokens.Colors.SurfaceCard;

                jointLabels[i] = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Bold, TextAnchor.MiddleLeft, ResolveJointColor(i));
                UiRuntimeStyle.Anchor(jointLabels[i].rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 20f), new Vector2(12f, 0f));

                jointSliders[i] = row.Find("Slider")?.GetComponent<Slider>();
                if (jointSliders[i] == null)
                {
                    jointSliders[i] = UIComponentFactory.CreateSlider(row, "Slider", -180f, 180f);
                }

                UiRuntimeStyle.Anchor((RectTransform)jointSliders[i].transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(210f, UIDesignTokens.Size.SliderHeight), new Vector2(102f, 0f));
                UpdateJointLabel(i);
            }

            // ── Presets 섹션 ──
            EnsureSectionHeader(root, "Presets", "Presets", 202f, 217f);
            EnsurePresetButtons(root);

            // ── Speed 섹션 ──
            EnsureSectionHeader(root, "Speed", "Speed", 148f, 163f);
            EnsureSpeedButtons(root);

            // ── Actions 섹션 ──
            EnsureSectionHeader(root, "Actions", "Actions", 94f, 109f);

            moveJButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnMoveJ", "MoveJ", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)moveJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 62f));

            servoJButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnServoJ", "ServoJ", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)servoJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(116f, 62f));

            stopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnStop", "Stop", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)stopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(216f, 62f));

            syncButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnSync", "Sync", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)syncButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(296f, 62f));

            dryRunToggle ??= UIComponentFactory.CreateToggle(root, "DryRunToggle", "DryRun", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)dryRunToggle.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 24f), new Vector2(16f, 32f));
            var dryRunLabel = dryRunToggle.transform.Find("Label")?.GetComponent<Text>();
            if (dryRunLabel != null) dryRunLabel.text = "DryRun";

            feedbackLabel = UiRuntimeStyle.EnsureText(root, "FeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(feedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 20f), new Vector2(16f, 8f));

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
            }

            RefreshModeIndicator();
        }

        private void EnsureSectionHeader(RectTransform root, string name, string label, float labelY, float dividerY)
        {
            var dividerColor = new Color(
                UIDesignTokens.Colors.TextMuted.r,
                UIDesignTokens.Colors.TextMuted.g,
                UIDesignTokens.Colors.TextMuted.b, 0.25f);
            var divider = UiRuntimeStyle.EnsureImage(root, $"Divider_{name}", dividerColor);
            divider.raycastTarget = false;
            UiRuntimeStyle.Anchor((RectTransform)divider.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 1f), new Vector2(16f, dividerY));

            var sectionLabel = UiRuntimeStyle.EnsureText(root, $"Section_{name}", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(sectionLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 14f), new Vector2(16f, labelY));
            sectionLabel.text = label;
        }

        private void EnsurePresetButtons(RectTransform root)
        {
            var presets = FR5PosePresets.All;
            presetButtons = new Button[presets.Length];

            for (var i = 0; i < presets.Length; i++)
            {
                var btnName = $"BtnPreset_{presets[i].Name}";
                var existing = root.Find(btnName)?.GetComponent<Button>();
                presetButtons[i] = existing ?? UIComponentFactory.CreateSecondaryButton(root, btnName, presets[i].Name, fallbackFont, 90f);
                UiRuntimeStyle.Anchor((RectTransform)presetButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(90f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 100f), 170f));
            }
        }

        private void EnsureSpeedButtons(RectTransform root)
        {
            speedButtons = new Button[SpeedPresetNames.Length];
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                var btnName = $"BtnSpeed_{SpeedPresetNames[i]}";
                var existing = root.Find(btnName)?.GetComponent<Button>();
                speedButtons[i] = existing ?? UIComponentFactory.CreateSecondaryButton(root, btnName, SpeedPresetLabels[i], fallbackFont, 100f);
                UiRuntimeStyle.Anchor((RectTransform)speedButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 108f), 116f));
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

            moveJButton?.onClick.AddListener(OnMoveJClicked);
            servoJButton?.onClick.AddListener(OnServoJClicked);
            stopButton?.onClick.AddListener(OnStopClicked);
            syncButton?.onClick.AddListener(OnSyncClicked);
            dryRunToggle?.onValueChanged.AddListener(OnDryRunChanged);

            for (var i = 0; i < jointSliders.Length; i++)
            {
                if (jointSliders[i] == null)
                {
                    continue;
                }

                var capturedIndex = i;
                sliderListeners[i] = value => OnSliderChanged(capturedIndex, value);
                jointSliders[i].onValueChanged.AddListener(sliderListeners[i]);
            }

            if (presetButtons != null)
            {
                var presets = FR5PosePresets.All;
                for (var i = 0; i < presetButtons.Length && i < presets.Length; i++)
                {
                    if (presetButtons[i] == null) continue;
                    var capturedPreset = presets[i];
                    presetButtons[i].onClick.AddListener(() => OnPresetClicked(capturedPreset));
                }
            }

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

            moveJButton?.onClick.RemoveListener(OnMoveJClicked);
            servoJButton?.onClick.RemoveListener(OnServoJClicked);
            stopButton?.onClick.RemoveListener(OnStopClicked);
            syncButton?.onClick.RemoveListener(OnSyncClicked);
            dryRunToggle?.onValueChanged.RemoveListener(OnDryRunChanged);

            for (var i = 0; i < jointSliders.Length; i++)
            {
                if (jointSliders[i] != null && sliderListeners[i] != null)
                {
                    jointSliders[i].onValueChanged.RemoveListener(sliderListeners[i]);
                }

                sliderListeners[i] = null;
            }

            if (presetButtons != null)
            {
                for (var i = 0; i < presetButtons.Length; i++)
                {
                    presetButtons[i]?.onClick.RemoveAllListeners();
                }
            }

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    speedButtons[i]?.onClick.RemoveAllListeners();
                }
            }

            listenersBound = false;
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

        private void HandleModeChanged(bool isMock)
        {
            RefreshModeIndicator();
        }

        /// <summary>
        /// 안전 모드 배너를 현재 상태에 맞게 갱신합니다.
        /// </summary>
        private void RefreshModeIndicator()
        {
            if (modeBannerBg == null || modeBannerText == null)
            {
                return;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                modeBannerBg.color = UIDesignTokens.Colors.AccentSuccess;
                modeBannerText.text = "시뮬레이션 — 슬라이더 안전";
            }
            else if (dryRun)
            {
                modeBannerBg.color = UIDesignTokens.Colors.AccentWarning;
                modeBannerText.text = "Live 연결, DryRun ON";
            }
            else
            {
                modeBannerBg.color = UIDesignTokens.Colors.AccentDanger;
                modeBannerText.text = "LIVE — 실제 로봇 동작";
            }
        }

        private void InitSliders()
        {
            if (config == null || config.jointLimits == null)
            {
                return;
            }

            for (var i = 0; i < 6 && i < jointSliders.Length; i++)
            {
                if (jointSliders[i] == null)
                {
                    continue;
                }

                if (i < config.jointLimits.Length)
                {
                    jointSliders[i].minValue = (float)config.jointLimits[i].minDeg;
                    jointSliders[i].maxValue = (float)config.jointLimits[i].maxDeg;
                }

                UpdateJointLabel(i);
            }
        }

        private void OnSliderChanged(int index, float value)
        {
            UpdateJointLabel(index);
            OnJointSliderPreview?.Invoke(GetSliderValues());
        }

        private void UpdateJointLabel(int index)
        {
            if (index < 0 || index >= jointLabels.Length || jointLabels[index] == null)
            {
                return;
            }

            var value = (index < jointSliders.Length && jointSliders[index] != null) ? jointSliders[index].value : 0f;
            jointLabels[index].text = $"J{index + 1}: {value:F1}\u00b0";
        }

        private double[] GetSliderValues()
        {
            var values = new double[6];
            for (var i = 0; i < 6; i++)
            {
                values[i] = (i < jointSliders.Length && jointSliders[i] != null) ? jointSliders[i].value : 0.0;
            }

            return values;
        }

        private void OnMoveJClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            var target = GetSliderValues();
            var (speed, acc) = GetSelectedSpeedAcc();

            if (dryRun)
            {
                ShowFeedback($"[DryRun] MoveJ ({selectedSpeedPreset}) \u2192 [{target[0]:F1}, {target[1]:F1}, {target[2]:F1}, {target[3]:F1}, {target[4]:F1}, {target[5]:F1}]");
                return;
            }

            if (!connectionService.IsMockMode && moveConfirmDialog != null)
            {
                var msg = $"Live 모드에서 MoveJ를 실행합니다.\n속도: {selectedSpeedPreset}\n목표: [{target[0]:F1}, {target[1]:F1}, {target[2]:F1}, {target[3]:F1}, {target[4]:F1}, {target[5]:F1}]";
                var capturedSpeed = speed;
                var capturedAcc = acc;
                var capturedTarget = target;
                moveConfirmDialog.Show(msg, () =>
                {
                    var result = connectionService.Client.MoveJ(capturedTarget, capturedSpeed, capturedAcc);
                    ShowFeedback(result.Message);
                });
                return;
            }

            var moveResult = connectionService.Client.MoveJ(target, speed, acc);
            ShowFeedback(moveResult.Message);
        }

        private void OnServoJClicked()
        {
            if (connectionService == null)
            {
                return;
            }

            var target = GetSliderValues();
            if (dryRun)
            {
                ShowFeedback($"[DryRun] ServoJ \u2192 [{target[0]:F1}, {target[1]:F1}, {target[2]:F1}, {target[3]:F1}, {target[4]:F1}, {target[5]:F1}]");
                return;
            }

            var result = connectionService.Client.ServoJ(target);
            ShowFeedback(result.Message);
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

        private void OnSyncClicked()
        {
            if (connectionService != null && connectionService.IsMockMode)
            {
                ShowFeedback("Sync는 Live 모드에서만 사용할 수 있습니다.");
                return;
            }

            OnSyncRequested?.Invoke();
            ShowFeedback("상태 동기화 요청...");
        }

        /// <summary>
        /// Sync 버튼의 활성 상태를 설정합니다.
        /// </summary>
        public void SetSyncEnabled(bool enabled)
        {
            if (syncButton != null)
            {
                syncButton.interactable = enabled;
            }
        }

        private void OnDryRunChanged(bool value)
        {
            dryRun = value;
            ShowFeedback(dryRun ? "DryRun 모드 활성" : "DryRun 모드 해제 \u2014 실제 명령이 전송됩니다!");
            RefreshModeIndicator();
        }

        private void OnPresetClicked(FR5PosePresets.Preset preset)
        {
            OnPresetApplied?.Invoke(preset.JointAnglesDeg);
            ShowFeedback($"프리셋 '{preset.Name}' 적용: {preset.Description}");
        }

        private void ShowFeedback(string text)
        {
            if (feedbackLabel != null)
            {
                feedbackLabel.text = text;
            }
        }

        private static Color ResolveJointColor(int jointIndex)
        {
            switch (jointIndex)
            {
                case 0: return UIDesignTokens.Colors.DiagramLink1;
                case 1: return UIDesignTokens.Colors.DiagramLink2;
                case 2: return UIDesignTokens.Colors.DiagramLink3;
                case 3: return UIDesignTokens.Colors.DiagramLink4;
                case 4: return UIDesignTokens.Colors.DiagramLink5;
                default: return UIDesignTokens.Colors.DiagramLink6;
            }
        }

        /// <summary>
        /// 슬라이더와 버튼의 일괄 활성/비활성을 설정합니다.
        /// </summary>
        public void SetControlsEnabled(bool enabled)
        {
            for (var i = 0; i < jointSliders.Length; i++)
            {
                if (jointSliders[i] != null)
                {
                    jointSliders[i].interactable = enabled;
                }
            }

            if (moveJButton != null) moveJButton.interactable = enabled;
            if (servoJButton != null) servoJButton.interactable = enabled;
            if (stopButton != null) stopButton.interactable = enabled;

            if (speedButtons != null)
            {
                for (var i = 0; i < speedButtons.Length; i++)
                {
                    if (speedButtons[i] != null) speedButtons[i].interactable = enabled;
                }
            }

            if (presetButtons != null)
            {
                for (var i = 0; i < presetButtons.Length; i++)
                {
                    if (presetButtons[i] != null) presetButtons[i].interactable = enabled;
                }
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
