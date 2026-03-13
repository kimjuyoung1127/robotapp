// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FAIRINO FR5 6축 관절 제어 패널입니다.
    /// 슬라이더, MoveJ/ServoJ 버튼, DryRun 토글, 비상정지를 제공합니다.
    /// </summary>
    public class FairinoJointControlPanel : MonoBehaviour
    {
        [SerializeField] private Slider[] jointSliders = new Slider[6];
        [SerializeField] private Text[] jointLabels = new Text[6];
        [SerializeField] private Button moveJButton;
        [SerializeField] private Button servoJButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Toggle dryRunToggle;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private Font fallbackFont;

        private readonly UnityAction<float>[] sliderListeners = new UnityAction<float>[6];
        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private bool listenersBound;
        private bool dryRun = true;

        /// <summary>
        /// 연결 서비스와 설정을 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig)
        {
            connectionService = service;
            config = robotConfig;
            EnsurePresentation();
            InitSliders();
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
            title.text = "Joint Control";

            for (var i = 0; i < 6; i++)
            {
                var row = UiRuntimeStyle.EnsureRectChild(root, $"JointRow_{i + 1}");
                UiRuntimeStyle.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 44f), new Vector2(16f, -50f - (i * 46f)));
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

            moveJButton ??= UIComponentFactory.CreatePrimaryButton(root, "BtnMoveJ", "MoveJ", fallbackFont, 110f);
            UiRuntimeStyle.Anchor((RectTransform)moveJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, 16f));

            servoJButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnServoJ", "ServoJ", fallbackFont, 110f);
            UiRuntimeStyle.Anchor((RectTransform)servoJButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(136f, 16f));

            stopButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnStop", "Stop", fallbackFont, 92f);
            UiRuntimeStyle.Anchor((RectTransform)stopButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(92f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(256f, 16f));

            dryRunToggle ??= UIComponentFactory.CreateToggle(root, "DryRunToggle", "DryRun", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)dryRunToggle.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(180f, 24f), new Vector2(16f, -344f));
            var dryRunLabel = dryRunToggle.transform.Find("Label")?.GetComponent<Text>();
            if (dryRunLabel != null) dryRunLabel.text = "DryRun";
            feedbackLabel = UiRuntimeStyle.EnsureText(root, "FeedbackLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(feedbackLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 36f), new Vector2(16f, 58f));

            if (dryRunToggle != null)
            {
                dryRunToggle.SetIsOnWithoutNotify(dryRun);
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
            dryRunToggle?.onValueChanged.RemoveListener(OnDryRunChanged);

            for (var i = 0; i < jointSliders.Length; i++)
            {
                if (jointSliders[i] != null && sliderListeners[i] != null)
                {
                    jointSliders[i].onValueChanged.RemoveListener(sliderListeners[i]);
                }

                sliderListeners[i] = null;
            }

            listenersBound = false;
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
            var speed = 30;
            var acc = 50;
            if (config?.speedPresets?.medium != null)
            {
                speed = config.speedPresets.medium.jointSpeedPercent;
                acc = config.speedPresets.medium.accPercent;
            }

            if (dryRun)
            {
                ShowFeedback($"[DryRun] MoveJ → [{target[0]:F1}, {target[1]:F1}, {target[2]:F1}, {target[3]:F1}, {target[4]:F1}, {target[5]:F1}]");
                return;
            }

            var result = connectionService.Client.MoveJ(target, speed, acc);
            ShowFeedback(result.Message);
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
                ShowFeedback($"[DryRun] ServoJ → [{target[0]:F1}, {target[1]:F1}, {target[2]:F1}, {target[3]:F1}, {target[4]:F1}, {target[5]:F1}]");
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

        private void OnDryRunChanged(bool value)
        {
            dryRun = value;
            ShowFeedback(dryRun ? "DryRun 모드 활성" : "DryRun 모드 해제 — 실제 명령이 전송됩니다!");
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
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
