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
    /// 슬라이더, MoveJ/ServoJ 버튼, DryRun 토글, 프리셋, 안전 모드 배너,
    /// 그리고 Waypoint Teaching 섹션(Save/Play/Loop/Stop/Export/Import)을 포함합니다.
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
        [SerializeField] private Text summaryTitleLabel;
        [SerializeField] private Text summaryBodyLabel;

        // Teaching 섹션
        [SerializeField] private Button savePointButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button loopButton;
        [SerializeField] private Button teachStopButton;
        [SerializeField] private Button exportButton;
        [SerializeField] private Button importButton;
        [SerializeField] private Button undoLastButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Text waypointListLabel;
        [SerializeField] private Text teachFeedbackLabel;

        [SerializeField] private Font fallbackFont;

        private static readonly string[] SpeedPresetNames = { "slow", "medium", "fast" };
        private static readonly string[] SpeedPresetLabels = { "Slow 10%", "Medium 30%", "Fast 60%" };

        private readonly UnityAction<float>[] sliderListeners = new UnityAction<float>[6];
        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private Func<RobotControlPosePresetOption[]> presetOptionsFactory;
        private FairinoMoveConfirmDialog moveConfirmDialog;
        private Button[] presetButtons;
        private Button[] speedButtons;
        private string selectedSpeedPreset = "medium";
        private string currentPoseSummary = "Ready";
        private bool listenersBound;
        private bool serviceSubscribed;
        private bool dryRun = true;
        private bool controlsEnabled = true;

        /// <summary>
        /// 슬라이더 프리뷰 이벤트입니다.
        /// </summary>
        public event Action<double[]> OnJointSliderPreview;

        /// <summary>
        /// 프리셋 적용 이벤트입니다.
        /// </summary>
        public event Action<double[]> OnPresetApplied;

        /// <summary>
        /// Sync 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnSyncRequested;

        // Teaching 이벤트
        /// <summary>
        /// Save Point 버튼 클릭 이벤트입니다. 현재 관절 각도를 웨이포인트로 저장합니다.
        /// </summary>
        public event Action OnSaveWaypointRequested;

        /// <summary>
        /// Play 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnPlayRequested;

        /// <summary>
        /// Loop 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnLoopRequested;

        /// <summary>
        /// Teaching Stop 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnTeachStopRequested;

        /// <summary>
        /// Export 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnExportRequested;

        /// <summary>
        /// Import 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnImportRequested;

        /// <summary>
        /// Undo Last 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnUndoLastRequested;

        /// <summary>
        /// Clear All 버튼 클릭 이벤트입니다.
        /// </summary>
        public event Action OnClearAllRequested;

        /// <summary>
        /// 현재 패널 피드백 텍스트입니다.
        /// </summary>
        public string CurrentFeedbackText => feedbackLabel != null ? feedbackLabel.text : string.Empty;

        /// <summary>
        /// 연결 서비스와 설정을 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig, Func<RobotControlPosePresetOption[]> posePresetOptionsFactory = null)
        {
            UnsubscribeService();
            connectionService = service;
            config = robotConfig;
            presetOptionsFactory = posePresetOptionsFactory;
            EnsurePresentation();
            InitSliders();
            SubscribeService();
            RefreshModeIndicator();
            RefreshActionAvailability();
        }

        /// <summary>
        /// MoveJ 확인 대화상자를 주입합니다.
        /// </summary>
        public void InjectMoveConfirmDialog(FairinoMoveConfirmDialog dialog)
        {
            moveConfirmDialog = dialog;
        }

        /// <summary>
        /// 슬라이더 값을 외부에서 동기화합니다.
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

            RefreshCompactSummary();
        }

        /// <summary>
        /// 웨이포인트 리스트 표시를 갱신합니다.
        /// </summary>
        public void UpdateWaypointList(WaypointSequence sequence)
        {
            if (waypointListLabel == null)
            {
                return;
            }

            if (sequence == null || sequence.waypoints == null || sequence.waypoints.Length == 0)
            {
                waypointListLabel.text = "웨이포인트 없음";
                return;
            }

            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < sequence.waypoints.Length; i++)
            {
                var wp = sequence.waypoints[i];
                sb.Append($"W{i + 1}: {wp.name} ({wp.moveType}, {wp.speedPreset})");
                if (i < sequence.waypoints.Length - 1)
                {
                    sb.Append("\n");
                }
            }

            waypointListLabel.text = sb.ToString();
        }

        /// <summary>
        /// Teaching 피드백 텍스트를 표시합니다.
        /// </summary>
        public void ShowTeachFeedback(string text)
        {
            if (teachFeedbackLabel != null)
            {
                teachFeedbackLabel.text = text;
            }
        }

        /// <summary>
        /// Teaching 실행 상태에 따라 버튼을 활성/비활성합니다.
        /// </summary>
        public void SetTeachingState(bool isRunning)
        {
            if (savePointButton != null) savePointButton.interactable = !isRunning;
            if (playButton != null) playButton.interactable = !isRunning;
            if (loopButton != null) loopButton.interactable = !isRunning;
            if (teachStopButton != null) teachStopButton.interactable = isRunning;
            if (undoLastButton != null) undoLastButton.interactable = !isRunning;
            if (clearAllButton != null) clearAllButton.interactable = !isRunning;
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
            RefreshActionAvailability();
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
                RefreshModeIndicator();
                RefreshCompactSummary();
                return;
            }

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

            var summaryCard = UiRuntimeStyle.EnsureRectChild(root, "ControlSummaryCard");
            UiRuntimeStyle.Anchor(summaryCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 60f), new Vector2(16f, 376f));
            var summaryBg = summaryCard.GetComponent<Image>() ?? summaryCard.gameObject.AddComponent<Image>();
            summaryBg.color = UIDesignTokens.Colors.SurfaceCard;

            summaryTitleLabel = UiRuntimeStyle.EnsureText(summaryCard, "SummaryTitle", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(summaryTitleLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, 14f), new Vector2(10f, -8f));
            summaryTitleLabel.text = "Control Summary";

            summaryBodyLabel = UiRuntimeStyle.EnsureText(summaryCard, "SummaryBody", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(summaryBodyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(370f, 34f), new Vector2(10f, -24f));

            // ═══ 하단 섹션 (bottom-anchored, 패딩 8px 간격) ═══

            // ── Presets 섹션 ──
            EnsureSectionHeader(root, "Presets", "Presets", 348f, 364f);
            EnsurePresetButtons(root);

            // ── Speed 섹션 ──
            EnsureSectionHeader(root, "Speed", "Speed", 292f, 308f);
            EnsureSpeedButtons(root);

            // ── Actions 섹션 ──
            EnsureSectionHeader(root, "Actions", "Actions", 236f, 252f);

            moveJButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnMoveJ", "MoveJ", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)moveJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 204f));

            servoJButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnServoJ", "ServoJ", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)servoJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(116f, 204f));

            stopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnStop", "Stop", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)stopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(216f, 204f));

            syncButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnSync", "Sync", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)syncButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(296f, 204f));

            dryRunToggle ??= UIComponentFactory.CreateToggle(root, "DryRunToggle", "DryRun", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)dryRunToggle.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 24f), new Vector2(16f, 174f));
            var dryRunLabel = dryRunToggle.transform.Find("Label")?.GetComponent<Text>();
            if (dryRunLabel != null) dryRunLabel.text = "DryRun";

            feedbackLabel = UiRuntimeStyle.EnsureText(root, "FeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(feedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 16f), new Vector2(210f, 178f));

            // ── Teaching 섹션 ──
            EnsureSectionHeader(root, "Teaching", "Teaching", 142f, 158f);
            EnsureTeachingSection(root);

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
            }

            RefreshModeIndicator();
            RefreshCompactSummary();
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            modeBannerBg = root.Find("ModeBanner")?.GetComponent<Image>();
            modeBannerText = root.Find("ModeBanner/BannerText")?.GetComponent<Text>();
            summaryTitleLabel = root.Find("ControlSummaryCard/SummaryTitle")?.GetComponent<Text>();
            summaryBodyLabel = root.Find("ControlSummaryCard/SummaryBody")?.GetComponent<Text>();
            moveJButton = root.Find("BtnMoveJ")?.GetComponent<Button>();
            servoJButton = root.Find("BtnServoJ")?.GetComponent<Button>();
            stopButton = root.Find("BtnStop")?.GetComponent<Button>();
            syncButton = root.Find("BtnSync")?.GetComponent<Button>();
            dryRunToggle = root.Find("DryRunToggle")?.GetComponent<Toggle>();
            feedbackLabel = root.Find("FeedbackLabel")?.GetComponent<Text>();
            savePointButton = root.Find("BtnSavePoint")?.GetComponent<Button>();
            playButton = root.Find("BtnTeachPlay")?.GetComponent<Button>();
            loopButton = root.Find("BtnTeachLoop")?.GetComponent<Button>();
            teachStopButton = root.Find("BtnTeachStop")?.GetComponent<Button>();
            exportButton = root.Find("BtnExport")?.GetComponent<Button>();
            importButton = root.Find("BtnImport")?.GetComponent<Button>();
            undoLastButton = root.Find("BtnUndoLast")?.GetComponent<Button>();
            clearAllButton = root.Find("BtnClearAll")?.GetComponent<Button>();
            waypointListLabel = root.Find("WaypointListLabel")?.GetComponent<Text>();
            teachFeedbackLabel = root.Find("TeachFeedbackLabel")?.GetComponent<Text>();

            for (var i = 0; i < 6; i++)
            {
                jointLabels[i] = root.Find($"JointRow_{i + 1}/Label")?.GetComponent<Text>();
                jointSliders[i] = root.Find($"JointRow_{i + 1}/Slider")?.GetComponent<Slider>();
            }

            var presets = GetPosePresetOptions();
            presetButtons = new Button[presets.Length];
            for (var i = 0; i < presets.Length; i++)
            {
                presetButtons[i] = root.Find($"BtnPreset_{presets[i].Name}")?.GetComponent<Button>();
                var label = presetButtons[i] != null ? presetButtons[i].GetComponentInChildren<Text>() : null;
                if (label != null)
                {
                    label.text = presets[i].Name;
                }
            }

            speedButtons = new Button[SpeedPresetNames.Length];
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                speedButtons[i] = root.Find($"BtnSpeed_{SpeedPresetNames[i]}")?.GetComponent<Button>();
            }

            if (modeBannerBg == null
                || modeBannerText == null
                || summaryBodyLabel == null
                || moveJButton == null
                || servoJButton == null
                || stopButton == null
                || syncButton == null
                || dryRunToggle == null
                || feedbackLabel == null
                || waypointListLabel == null
                || teachFeedbackLabel == null)
            {
                return false;
            }

            for (var i = 0; i < 6; i++)
            {
                if (jointLabels[i] == null || jointSliders[i] == null)
                {
                    return false;
                }

                UpdateJointLabel(i);
            }

            for (var i = 0; i < presetButtons.Length; i++)
            {
                if (presetButtons[i] == null)
                {
                    return false;
                }
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
                title.text = "Joint Control";
            }

            var dryRunLabel = dryRunToggle.transform.Find("Label")?.GetComponent<Text>();
            if (dryRunLabel != null)
            {
                dryRunLabel.text = "DryRun";
            }

            if (summaryTitleLabel != null)
            {
                summaryTitleLabel.text = "Control Summary";
            }

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
            }

            RefreshSpeedButtonColors();
            return true;
        }

        private void EnsureTeachingSection(RectTransform root)
        {
            // Row 1: Save Point / Play / Loop / Stop
            savePointButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnSavePoint", "Save Point", fallbackFont, 96f);
            UiRuntimeStyle.Anchor((RectTransform)savePointButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(96f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 112f));

            playButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnTeachPlay", "\u25b6 Play", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)playButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(120f, 112f));

            loopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnTeachLoop", "\u27f3 Loop", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)loopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(200f, 112f));

            teachStopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnTeachStop", "\u25a0 Stop", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)teachStopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(280f, 112f));
            if (teachStopButton != null) teachStopButton.interactable = false;

            // Waypoint 리스트 (텍스트 표시)
            waypointListLabel = UiRuntimeStyle.EnsureText(root, "WaypointListLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(waypointListLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 56f), new Vector2(16f, 50f));
            waypointListLabel.text = "웨이포인트 없음";

            // Row 2: Undo Last / Export / Import / Clear All
            undoLastButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnUndoLast", "Undo", fallbackFont, 60f);
            UiRuntimeStyle.Anchor((RectTransform)undoLastButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 20f));

            exportButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnExport", "Export", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)exportButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(84f, 20f));

            importButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnImport", "Import", fallbackFont, 72f);
            UiRuntimeStyle.Anchor((RectTransform)importButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(72f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(164f, 20f));

            clearAllButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnClearAll", "Clear All", fallbackFont, 80f);
            UiRuntimeStyle.Anchor((RectTransform)clearAllButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(80f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(244f, 20f));

            // Teaching 피드백
            teachFeedbackLabel = UiRuntimeStyle.EnsureText(root, "TeachFeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(teachFeedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(390f, 14f), new Vector2(16f, 4f));
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
            var presets = GetPosePresetOptions();
            presetButtons = new Button[presets.Length];

            for (var i = 0; i < presets.Length; i++)
            {
                var btnName = $"BtnPreset_{presets[i].Name}";
                var existing = root.Find(btnName)?.GetComponent<Button>();
                presetButtons[i] = existing ?? UIComponentFactory.CreateSecondaryButton(root, btnName, presets[i].Name, fallbackFont, 90f);
                UiRuntimeStyle.Anchor((RectTransform)presetButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(90f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 100f), 316f));
                var label = presetButtons[i].GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = presets[i].Name;
                }
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
                UiRuntimeStyle.Anchor((RectTransform)speedButtons[i].transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f + (i * 108f), 260f));
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

        /// <summary>
        /// 현재 선택된 속도 프리셋 이름을 반환합니다.
        /// </summary>
        public string GetSelectedSpeedPreset()
        {
            return selectedSpeedPreset;
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
            RefreshCompactSummary();
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
                for (var i = 0; i < presetButtons.Length; i++)
                {
                    if (presetButtons[i] == null) continue;
                    var capturedIndex = i;
                    presetButtons[i].onClick.AddListener(() => OnPresetClicked(capturedIndex));
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

            // Teaching 버튼
            savePointButton?.onClick.AddListener(() => OnSaveWaypointRequested?.Invoke());
            playButton?.onClick.AddListener(() => OnPlayRequested?.Invoke());
            loopButton?.onClick.AddListener(() => OnLoopRequested?.Invoke());
            teachStopButton?.onClick.AddListener(() => OnTeachStopRequested?.Invoke());
            exportButton?.onClick.AddListener(() => OnExportRequested?.Invoke());
            importButton?.onClick.AddListener(() => OnImportRequested?.Invoke());
            undoLastButton?.onClick.AddListener(() => OnUndoLastRequested?.Invoke());
            clearAllButton?.onClick.AddListener(() => OnClearAllRequested?.Invoke());

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

            // Teaching 버튼
            savePointButton?.onClick.RemoveAllListeners();
            playButton?.onClick.RemoveAllListeners();
            loopButton?.onClick.RemoveAllListeners();
            teachStopButton?.onClick.RemoveAllListeners();
            exportButton?.onClick.RemoveAllListeners();
            importButton?.onClick.RemoveAllListeners();
            undoLastButton?.onClick.RemoveAllListeners();
            clearAllButton?.onClick.RemoveAllListeners();

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
            RefreshActionAvailability();
        }

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
                modeBannerText.text = "Live 연결, DryRun ON — MoveJ/MoveL만 허용";
            }
            else
            {
                modeBannerBg.color = UIDesignTokens.Colors.AccentDanger;
                modeBannerText.text = "LIVE — 실제 로봇 동작 (ServoJ 비활성)";
            }

            RefreshCompactSummary();
        }

        private void RefreshActionAvailability()
        {
            var liveUnsupported = connectionService != null && !connectionService.IsMockMode;

            if (moveJButton != null)
            {
                moveJButton.interactable = controlsEnabled;
            }

            if (servoJButton != null)
            {
                servoJButton.interactable = controlsEnabled && !liveUnsupported;
            }

            if (stopButton != null)
            {
                stopButton.interactable = controlsEnabled;
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
            currentPoseSummary = "Custom";
            RefreshCompactSummary();
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

            if (!connectionService.IsMockMode)
            {
                ShowFeedback("ServoJ는 Live 모드에서 비활성화되어 있습니다. v1 bring-up에서는 MoveJ만 사용하세요.");
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
            RefreshCompactSummary();
        }

        private void OnPresetClicked(int presetIndex)
        {
            var presets = GetPosePresetOptions();
            if (presetIndex < 0 || presetIndex >= presets.Length)
            {
                return;
            }

            var preset = presets[presetIndex];
            currentPoseSummary = preset.Name;
            RefreshCompactSummary();
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

        private void RefreshCompactSummary()
        {
            if (summaryBodyLabel == null)
            {
                return;
            }

            var modeText = dryRun ? "DryRun ON" : "DryRun OFF";
            var syncText = connectionService != null && !connectionService.IsMockMode ? "Sync Ready" : "Sync Mock";
            summaryBodyLabel.text =
                $"Pose: {currentPoseSummary}  |  Speed: {GetSelectedSpeedPresetLabel()}\n" +
                $"Mode: {modeText}  |  {syncText}";
        }

        private string GetSelectedSpeedPresetLabel()
        {
            for (var i = 0; i < SpeedPresetNames.Length; i++)
            {
                if (SpeedPresetNames[i] == selectedSpeedPreset)
                {
                    return SpeedPresetLabels[i];
                }
            }

            return selectedSpeedPreset;
        }

        private RobotControlPosePresetOption[] GetPosePresetOptions()
        {
            var options = presetOptionsFactory != null
                ? presetOptionsFactory()
                : Array.Empty<RobotControlPosePresetOption>();

            if (options != null && options.Length > 0)
            {
                return options;
            }

            var fallback = FR5PosePresets.All;
            var fallbackOptions = new RobotControlPosePresetOption[fallback.Length];
            for (var i = 0; i < fallback.Length; i++)
            {
                fallbackOptions[i] = new RobotControlPosePresetOption(
                    fallback[i].Name,
                    fallback[i].Description,
                    fallback[i].JointAnglesDeg);
            }

            return fallbackOptions;
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
            controlsEnabled = enabled;
            for (var i = 0; i < jointSliders.Length; i++)
            {
                if (jointSliders[i] != null)
                {
                    jointSliders[i].interactable = enabled;
                }
            }

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

            RefreshActionAvailability();
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
