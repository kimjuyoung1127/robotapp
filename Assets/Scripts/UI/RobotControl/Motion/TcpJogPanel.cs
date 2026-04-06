// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// V2 TCP 조그 패널의 scene-authored 레이아웃을 우선 바인딩하고,
    /// authored 구조가 없을 때만 fallback 레이아웃을 구성합니다.
    /// </summary>
    public sealed class TcpJogPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text stateText;
        [SerializeField] private Text currentPoseText;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        public void Bind(RobotControlViewState state)
        {
            ApplyState(state);
        }

        public void ApplyState(RobotControlViewState state)
        {
            EnsurePresentation();
            if (stateText != null)
            {
                stateText.text = $"미리보기 목표: {state.PreviewTarget}";
            }

            if (currentPoseText != null)
            {
                currentPoseText.text = $"현재 TCP · {state.CurrentTcpPose}";
            }
        }

        public void SetVisible(bool visible)
        {
            UiRuntimeStyle.SetCanvasVisible(gameObject, visible);
        }

        public void RefreshAuthoring()
        {
            EnsurePresentation();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            if (transform is not RectTransform root)
            {
                return;
            }

            var compact = root.rect.width < 340f;
            NormalizeLegacyRootChildren(root);
            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            if (background.sprite == null)
            {
                background.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                background.type = Image.Type.Sliced;
            }
            background.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;
            if (HasSceneAuthoredLayout(root))
            {
                BindSceneAuthoredReferences(root);
                UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
                return;
            }

            var layout = UiRuntimeStyle.EnsureVerticalLayout(root.gameObject, compact ? UIDesignTokens.Space.Xs : UIDesignTokens.Space.Sm);
            layout.padding = new RectOffset(
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            EnsureHeader(root, compact);
            EnsureCoordinateRow(root, compact);
            EnsureIncrementCard(root, compact);
            EnsureAxisGrid(root, compact);
            EnsureActionRow(root, compact);
            EnsureInfoCard(root, compact);
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "Hint");
            RemoveDirectChild(root, "ChipBase");
            RemoveDirectChild(root, "ChipTool");
            RemoveDirectChild(root, "ChipWobj");
            RemoveDirectChild(root, "XRow");
            RemoveDirectChild(root, "YRow");
            RemoveDirectChild(root, "ZRow");
            RemoveDirectChild(root, "RXRow");
            RemoveDirectChild(root, "RYRow");
            RemoveDirectChild(root, "RZRow");
            RemoveDirectChild(root, "StateText");
        }

        private void EnsureHeader(RectTransform root, bool compact)
        {
            var header = root.Find("Header") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "Header");
            var element = UiRuntimeStyle.EnsureLayoutElement(header);
            element.preferredHeight = compact ? 42f : 48f;

            var titleText = UiRuntimeStyle.EnsureText(header, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 22f), new Vector2(0f, 0f));
            titleText.text = "TCP 조그";

            var hintText = UiRuntimeStyle.EnsureText(header, "Hint", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 20f), new Vector2(0f, -22f));
            hintText.text = "Base / Tool / Wobj 좌표계와 증분 이동량을 먼저 고릅니다.";
        }

        private void EnsureCoordinateRow(RectTransform root, bool compact)
        {
            var row = root.Find("CoordinateRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "CoordinateRow");
            ConfigureRow(row, compact ? 6f : 8f, compact ? 30f : 32f);
            EnsureChip(row, "ChipBase", "BASE", true, compact);
            EnsureChip(row, "ChipTool", "TOOL", false, compact);
            EnsureChip(row, "ChipWobj", "WOBJ", false, compact);
        }

        private void EnsureIncrementCard(RectTransform root, bool compact)
        {
            var card = root.Find("IncrementCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "IncrementCard");
            var bg = card.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
            if (bg.sprite == null)
            {
                bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                bg.type = Image.Type.Sliced;
            }
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;
            var element = UiRuntimeStyle.EnsureLayoutElement(card);
            element.preferredHeight = compact ? 42f : 48f;

            var labelText = UiRuntimeStyle.EnsureText(card, "Label", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(labelText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 20f), new Vector2(12f, 0f));
            labelText.text = "증분 이동량";

            var input = UIComponentFactory.CreateInputField(card, "IncrementInput", compact ? "10" : "10 mm", fallbackFont);
            var rect = input.transform as RectTransform;
            UiRuntimeStyle.Anchor(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(compact ? 72f : 88f, 30f), new Vector2(-12f, 0f));
        }

        private void EnsureAxisGrid(RectTransform root, bool compact)
        {
            var gridRoot = root.Find("AxisGrid") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "AxisGrid");
            var grid = gridRoot.GetComponent<GridLayoutGroup>() ?? gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 1;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.spacing = new Vector2(0f, compact ? 6f : 8f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.cellSize = new Vector2(compact ? 300f : 316f, compact ? 34f : 38f);
            var element = UiRuntimeStyle.EnsureLayoutElement(gridRoot);
            element.preferredHeight = (compact ? 34f : 38f) * 6f + (compact ? 6f : 8f) * 5f;

            EnsureValueRow(gridRoot, "X", compact);
            EnsureValueRow(gridRoot, "Y", compact);
            EnsureValueRow(gridRoot, "Z", compact);
            EnsureValueRow(gridRoot, "RX", compact);
            EnsureValueRow(gridRoot, "RY", compact);
            EnsureValueRow(gridRoot, "RZ", compact);
        }

        private void EnsureActionRow(RectTransform root, bool compact)
        {
            var row = root.Find("ActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "ActionRow");
            ConfigureRow(row, 8f, compact ? 36f : 40f);
            EnsureActionButton(row, "BtnPreview", "미리보기", UIDesignTokens.RobotControlV2.Colors.Success, compact);
            EnsureActionButton(row, "BtnMove", "실제 이동", UIDesignTokens.RobotControlV2.Colors.Danger, compact);
        }

        private void EnsureInfoCard(RectTransform root, bool compact)
        {
            var card = root.Find("InfoCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "InfoCard");
            var bg = card.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
            if (bg.sprite == null)
            {
                bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                bg.type = Image.Type.Sliced;
            }
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;
            var element = UiRuntimeStyle.EnsureLayoutElement(card);
            element.preferredHeight = compact ? 52f : 58f;

            currentPoseText = UiRuntimeStyle.EnsureText(card, "CurrentPose", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(currentPoseText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -8f), new Vector2(-12f, -26f));
            currentPoseText.text = "현재 TCP · X -497 / Y -130 / Z 477 / RX 180 / RY 0 / RZ 90";

            stateText = UiRuntimeStyle.EnsureText(card, "StateText", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(stateText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 8f), new Vector2(-12f, 28f));
            stateText.text = "미리보기 목표: Ready 포즈";
        }

        private void EnsureChip(RectTransform parent, string name, string label, bool active, bool compact)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, compact ? 78f : 86f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = compact ? 78f : 86f;
            element.minWidth = compact ? 70f : 78f;
            element.preferredHeight = compact ? 28f : 30f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = active ? UIDesignTokens.RobotControlV2.Colors.Accent : UIDesignTokens.RobotControlV2.Colors.Card;
            }
        }

        private void EnsureValueRow(RectTransform parent, string label, bool compact)
        {
            var row = parent.Find($"{label}Row") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, $"{label}Row");
            ConfigureRow(row, 8f, compact ? 34f : 38f);

            var bg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
            if (bg.sprite == null)
            {
                bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                bg.type = Image.Type.Sliced;
            }
            bg.color = UIDesignTokens.RobotControlV2.Colors.Card;

            var labelText = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            var labelElement = UiRuntimeStyle.EnsureLayoutElement(labelText);
            labelElement.preferredWidth = 28f;
            labelElement.minWidth = 28f;
            labelText.text = label;

            var minusButton = row.Find("BtnMinus")?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(row, "BtnMinus", $"{label}-", fallbackFont, compact ? 42f : 48f);
            var minusElement = UiRuntimeStyle.EnsureLayoutElement(minusButton);
            minusElement.preferredWidth = compact ? 42f : 48f;
            minusElement.minWidth = compact ? 42f : 48f;
            minusElement.preferredHeight = compact ? 28f : 30f;

            var input = UIComponentFactory.CreateInputField(row, "ValueInput", "0.0", fallbackFont);
            var inputElement = UiRuntimeStyle.EnsureLayoutElement(input);
            inputElement.preferredWidth = compact ? 88f : 104f;
            inputElement.minWidth = compact ? 80f : 92f;
            inputElement.preferredHeight = compact ? 28f : 30f;

            var plusButton = row.Find("BtnPlus")?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(row, "BtnPlus", $"{label}+", fallbackFont, compact ? 42f : 48f);
            var plusElement = UiRuntimeStyle.EnsureLayoutElement(plusButton);
            plusElement.preferredWidth = compact ? 42f : 48f;
            plusElement.minWidth = compact ? 42f : 48f;
            plusElement.preferredHeight = compact ? 28f : 30f;

            var valueHint = UiRuntimeStyle.EnsureText(row, "ValueHint", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.MiddleRight, UIDesignTokens.RobotControlV2.Colors.MutedText);
            var hintElement = UiRuntimeStyle.EnsureLayoutElement(valueHint);
            hintElement.preferredWidth = compact ? 62f : 72f;
            hintElement.minWidth = compact ? 56f : 64f;
            valueHint.text = label.StartsWith("R") ? "deg" : "mm";
        }

        private void EnsureActionButton(RectTransform parent, string name, string label, Color color, bool compact)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 120f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredHeight = compact ? 36f : 40f;
            element.minHeight = compact ? 34f : 38f;
            element.flexibleWidth = 1f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private void ConfigureRow(RectTransform row, float spacing, float preferredHeight)
        {
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var element = UiRuntimeStyle.EnsureLayoutElement(row);
            element.preferredHeight = preferredHeight;
        }

        private static void RemoveDirectChild(RectTransform parent, string childName)
        {
            RectTransform child = null;
            for (var i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) is RectTransform rect && rect.name == childName)
                {
                    child = rect;
                    break;
                }
            }

            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static bool HasSceneAuthoredLayout(RectTransform root)
        {
            return root.Find("Header") != null
                && root.Find("Header/Title") != null
                && root.Find("Header/Hint") != null
                && root.Find("CoordinateRow") != null
                && root.Find("CoordinateRow/ChipBase") != null
                && root.Find("CoordinateRow/ChipTool") != null
                && root.Find("CoordinateRow/ChipWobj") != null
                && root.Find("IncrementCard") != null
                && root.Find("IncrementCard/IncrementInput") != null
                && root.Find("AxisGrid") != null
                && root.Find("AxisGrid/XRow") != null
                && root.Find("AxisGrid/RZRow") != null
                && root.Find("ActionRow") != null
                && root.Find("ActionRow/BtnPreview") != null
                && root.Find("ActionRow/BtnMove") != null
                && root.Find("InfoCard") != null
                && root.Find("InfoCard/CurrentPose") != null
                && root.Find("InfoCard/StateText") != null
                && root.GetComponent<VerticalLayoutGroup>() == null;
        }

        private void BindSceneAuthoredReferences(RectTransform root)
        {
            currentPoseText = root.Find("InfoCard/CurrentPose")?.GetComponent<Text>();
            stateText = root.Find("InfoCard/StateText")?.GetComponent<Text>();
        }
    }
}
