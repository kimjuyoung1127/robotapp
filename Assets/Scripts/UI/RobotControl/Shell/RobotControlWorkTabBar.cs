// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControlV2의 작업 패널 탭 전환을 담당합니다.
    /// </summary>
    public sealed class RobotControlWorkTabBar : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Button easyMotionButton;
        [SerializeField] private Button tcpButton;
        [SerializeField] private Button jointButton;
        [SerializeField] private Button pointButton;
        [SerializeField] private Button teachingButton;
        [SerializeField] private RectTransform workPanelHost;

        private readonly Dictionary<RobotControlWorkTab, Button> buttons = new();
        private RobotControlWorkTab activeTab;
        private bool listenersBound;

        private void Awake()
        {
            EnsurePresentation();
            BindListeners();
            SelectTab(RobotControlWorkTab.EasyMotion);
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

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetFallbackFont(Font font)
        {
            fallbackFont = font;
            EnsurePresentation();
        }

        public void Bind(RectTransform panelHost)
        {
            workPanelHost = panelHost;
            EnsurePresentation();
            SelectTab(activeTab);
        }

        public void SelectTab(RobotControlWorkTab tab)
        {
            activeTab = tab;
            ApplyPanelVisibility();
            RefreshButtonStyles();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (transform is not RectTransform root)
            {
                return;
            }

            var compact = root.rect.width < 360f;
            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            if (background.sprite == null)
            {
                background.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                background.type = Image.Type.Sliced;
            }
            background.color = UIDesignTokens.RobotControlV2.Colors.Card;
            UiRuntimeStyle.Stretch(root, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            ConfigureGridLayout(root, compact);

            easyMotionButton = EnsureTabButton(root, easyMotionButton, "BtnEasyMotion", "쉬운 조작", compact, 0);
            tcpButton = EnsureTabButton(root, tcpButton, "BtnTcp", "TCP 조그", compact, 1);
            jointButton = EnsureTabButton(root, jointButton, "BtnJoint", "관절 조그", compact, 2);
            pointButton = EnsureTabButton(root, pointButton, "BtnPoint", "포인트 이동", compact, 3);
            teachingButton = EnsureTabButton(root, teachingButton, "BtnTeaching", "티칭", compact, 4);

            buttons.Clear();
            buttons[RobotControlWorkTab.EasyMotion] = easyMotionButton;
            buttons[RobotControlWorkTab.TcpJog] = tcpButton;
            buttons[RobotControlWorkTab.JointJog] = jointButton;
            buttons[RobotControlWorkTab.PointMove] = pointButton;
            buttons[RobotControlWorkTab.Teaching] = teachingButton;

            RefreshButtonStyles();
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void ConfigureGridLayout(RectTransform root, bool compact)
        {
            var horizontal = root.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(horizontal);
                }
                else
                {
                    Object.DestroyImmediate(horizontal);
                }
            }

            var grid = root.GetComponent<GridLayoutGroup>() ?? root.gameObject.AddComponent<GridLayoutGroup>();
            grid.enabled = true;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.spacing = new Vector2(8f, 8f);

            var contentWidth = Mathf.Max(120f, root.rect.width - grid.padding.left - grid.padding.right - (grid.spacing.x * 2f));
            var cellWidth = Mathf.Floor(contentWidth / 3f);
            grid.cellSize = new Vector2(cellWidth, compact ? 28f : 30f);
        }

        private Button EnsureTabButton(RectTransform root, Button current, string name, string label, bool compact, int index)
        {
            var button = current ?? root.Find(name)?.GetComponent<Button>();
            button ??= UIComponentFactory.CreateSecondaryButton(root, name, label, fallbackFont, 100f);
            UiRuntimeStyle.EnsureButtonLabel(
                button,
                fallbackFont,
                label,
                button.GetComponent<Image>() != null
                    ? button.GetComponent<Image>().color
                    : UIDesignTokens.RobotControlV2.Colors.CardAlt);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.minWidth = -1f;
            element.preferredWidth = -1f;
            element.flexibleWidth = -1f;
            element.preferredHeight = compact ? 28f : 30f;

            var labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                labelText.resizeTextForBestFit = true;
                labelText.resizeTextMinSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                labelText.resizeTextMaxSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            }
            return button;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            easyMotionButton?.onClick.AddListener(OnEasyMotionClicked);
            tcpButton?.onClick.AddListener(OnTcpClicked);
            jointButton?.onClick.AddListener(OnJointClicked);
            pointButton?.onClick.AddListener(OnPointClicked);
            teachingButton?.onClick.AddListener(OnTeachingClicked);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            easyMotionButton?.onClick.RemoveListener(OnEasyMotionClicked);
            tcpButton?.onClick.RemoveListener(OnTcpClicked);
            jointButton?.onClick.RemoveListener(OnJointClicked);
            pointButton?.onClick.RemoveListener(OnPointClicked);
            teachingButton?.onClick.RemoveListener(OnTeachingClicked);
            listenersBound = false;
        }

        private void ApplyPanelVisibility()
        {
            if (workPanelHost == null)
            {
                return;
            }

            SetPanelVisible("EasyMotionPanel", activeTab == RobotControlWorkTab.EasyMotion);
            SetPanelVisible("TcpJogPanel", activeTab == RobotControlWorkTab.TcpJog);
            SetPanelVisible("JointJogPanel", activeTab == RobotControlWorkTab.JointJog);
            SetPanelVisible("PointMovePanel", activeTab == RobotControlWorkTab.PointMove);
            SetPanelVisible("TeachingPanel", activeTab == RobotControlWorkTab.Teaching);
        }

        private void SetPanelVisible(string panelName, bool visible)
        {
            var target = workPanelHost.Find(panelName);
            if (target == null)
            {
                return;
            }

            if (target.TryGetComponent<IVisibilityControllable>(out var controllable))
            {
                controllable.SetVisible(visible);
                return;
            }

            target.gameObject.SetActive(visible);
        }

        private void RefreshButtonStyles()
        {
            foreach (var pair in buttons)
            {
                StyleButton(pair.Value, pair.Key == activeTab);
            }
        }

        private static void StyleButton(Button button, bool isActive)
        {
            if (button == null)
            {
                return;
            }

            var background = button.GetComponent<Image>();
            if (background != null)
            {
                background.color = isActive
                    ? UIDesignTokens.RobotControlV2.Colors.Accent
                    : UIDesignTokens.RobotControlV2.Colors.CardAlt;
            }

            var label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                label.color = UIDesignTokens.RobotControlV2.Colors.TitleText;
                label.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
            }
        }

        private void OnEasyMotionClicked() => SelectTab(RobotControlWorkTab.EasyMotion);

        private void OnTcpClicked() => SelectTab(RobotControlWorkTab.TcpJog);

        private void OnJointClicked() => SelectTab(RobotControlWorkTab.JointJog);

        private void OnPointClicked() => SelectTab(RobotControlWorkTab.PointMove);

        private void OnTeachingClicked() => SelectTab(RobotControlWorkTab.Teaching);
    }
}
