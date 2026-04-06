// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// V2 관절 조그 패널의 scene-authored 레이아웃을 우선 바인딩하고,
    /// authored 구조가 없을 때만 fallback 레이아웃을 구성합니다.
    /// </summary>
    public sealed class JointJogPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text jointSummaryText;

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
            if (jointSummaryText != null && state.CurrentJointValuesDeg.Length >= 3)
            {
                jointSummaryText.text = $"J1 {state.CurrentJointValuesDeg[0]:F1} / J2 {state.CurrentJointValuesDeg[1]:F1} / J3 {state.CurrentJointValuesDeg[2]:F1}";
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

            EnsureHeader(root, compact);
            EnsureSingleAxisCard(root, compact);
            EnsureMultiAxisCard(root, compact);
            EnsureSummaryCard(root, compact);
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "Hint");
            RemoveDirectChild(root, "J1Row");
            RemoveDirectChild(root, "J2Row");
            RemoveDirectChild(root, "J3Row");
            RemoveDirectChild(root, "J4Row");
            RemoveDirectChild(root, "J5Row");
            RemoveDirectChild(root, "J6Row");
            RemoveDirectChild(root, "JointSummary");
        }

        private void EnsureHeader(RectTransform root, bool compact)
        {
            var header = root.Find("Header") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "Header");
            var element = UiRuntimeStyle.EnsureLayoutElement(header);
            element.preferredHeight = compact ? 42f : 48f;

            var titleText = UiRuntimeStyle.EnsureText(header, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 22f), new Vector2(0f, 0f));
            titleText.text = "관절 조그";

            var hintText = UiRuntimeStyle.EnsureText(header, "Hint", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 20f), new Vector2(0f, -22f));
            hintText.text = "상단은 단일축 조그, 하단은 다축 연계와 적용 영역입니다.";
        }

        private void EnsureSingleAxisCard(RectTransform root, bool compact)
        {
            var card = root.Find("SingleAxisCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "SingleAxisCard");
            SetupCard(card, compact ? 188f : 204f);
            EnsureSectionTitle(card, "Title", "단일축 조그", compact);

            var grid = card.Find("Grid") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "Grid");
            UiRuntimeStyle.Anchor(grid, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -36f), new Vector2(-12f, -(compact ? 176f : 192f)));
            var layout = grid.GetComponent<GridLayoutGroup>() ?? grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 1;
            layout.spacing = new Vector2(0f, compact ? 4f : 6f);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.cellSize = new Vector2(compact ? 296f : 312f, compact ? 24f : 26f);

            for (var i = 0; i < 6; i++)
            {
                EnsureSingleAxisRow(grid, i, compact);
            }
        }

        private void EnsureMultiAxisCard(RectTransform root, bool compact)
        {
            var card = root.Find("MultiAxisCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "MultiAxisCard");
            SetupCard(card, compact ? 258f : 286f);
            EnsureSectionTitle(card, "Title", "다축 연계", compact);

            var stack = card.Find("SliderStack") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "SliderStack");
            UiRuntimeStyle.Anchor(stack, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -36f), new Vector2(-12f, -(compact ? 206f : 228f)));
            var layout = stack.GetComponent<VerticalLayoutGroup>() ?? stack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = compact ? 4f : 6f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            for (var i = 0; i < 6; i++)
            {
                EnsureMultiAxisRow(stack, i, compact);
            }

            var actionRow = card.Find("ActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "ActionRow");
            UiRuntimeStyle.Anchor(actionRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 44f));
            ConfigureRow(actionRow, 8f, compact ? 34f : 38f);
            EnsureActionButton(actionRow, "BtnRestore", "복원", UIDesignTokens.RobotControlV2.Colors.CardAlt, compact);
            EnsureActionButton(actionRow, "BtnApply", "적용", UIDesignTokens.RobotControlV2.Colors.Accent, compact);
        }

        private void EnsureSummaryCard(RectTransform root, bool compact)
        {
            var card = root.Find("SummaryCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "SummaryCard");
            SetupCard(card, compact ? 48f : 56f);
            jointSummaryText = UiRuntimeStyle.EnsureText(card, "JointSummary", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Stretch(jointSummaryText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));
            jointSummaryText.text = "J1 0.0 / J2 -32.0 / J3 84.0";
        }

        private void EnsureSingleAxisRow(RectTransform parent, int index, bool compact)
        {
            var row = parent.Find($"SingleAxisRow_{index}") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, $"SingleAxisRow_{index}");
            ConfigureRow(row, 6f, compact ? 24f : 26f);
            PaintCard(row, UIDesignTokens.RobotControlV2.Colors.CardAlt);

            EnsureFixedLabel(row, "Label", $"J{index + 1}", compact, 28f);
            EnsureActionButton(row, "BtnMinus", "-", UIDesignTokens.RobotControlV2.Colors.Card, compact, 36f);
            EnsureFixedValue(row, "Value", "--", compact, compact ? 64f : 72f);
            EnsureActionButton(row, "BtnPlus", "+", UIDesignTokens.RobotControlV2.Colors.Card, compact, 36f);
        }

        private void EnsureMultiAxisRow(RectTransform parent, int index, bool compact)
        {
            var row = parent.Find($"MultiAxisRow_{index}") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, $"MultiAxisRow_{index}");
            ConfigureRow(row, 8f, compact ? 28f : 32f);

            EnsureFixedLabel(row, "Label", $"J{index + 1}", compact, 28f);
            var slider = row.Find("Slider")?.GetComponent<Slider>() ?? UIComponentFactory.CreateSlider(row, "Slider", -180f, 180f);
            var sliderElement = UiRuntimeStyle.EnsureLayoutElement(slider);
            sliderElement.flexibleWidth = 1f;
            sliderElement.preferredHeight = compact ? 24f : 28f;
            EnsureFixedValue(row, "Value", "--", compact, compact ? 64f : 72f);
        }

        private void SetupCard(RectTransform card, float preferredHeight)
        {
            PaintCard(card, UIDesignTokens.RobotControlV2.Colors.Card);
            var element = UiRuntimeStyle.EnsureLayoutElement(card);
            element.preferredHeight = preferredHeight;
        }

        private void PaintCard(RectTransform rect, Color color)
        {
            var bg = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            if (bg.sprite == null)
            {
                bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                bg.type = Image.Type.Sliced;
            }
            bg.color = color;
        }

        private void EnsureSectionTitle(RectTransform parent, string name, string label, bool compact)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Warning);
            UiRuntimeStyle.Anchor(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 18f), new Vector2(12f, -12f));
            text.text = label;
        }

        private void EnsureFixedLabel(RectTransform parent, string name, string label, bool compact, float width)
        {
            var text = parent.Find(name)?.GetComponent<Text>() ?? UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.preferredWidth = width;
            element.minWidth = width;
            text.text = label;
        }

        private void EnsureFixedValue(RectTransform parent, string name, string value, bool compact, float width)
        {
            var text = parent.Find(name)?.GetComponent<Text>() ?? UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.MiddleRight, UIDesignTokens.RobotControlV2.Colors.MutedText);
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.preferredWidth = width;
            element.minWidth = width;
            text.text = value;
        }

        private void EnsureActionButton(RectTransform parent, string name, string label, Color color, bool compact, float width = 96f)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, width);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.preferredWidth = width;
            element.minWidth = width;
            element.preferredHeight = compact ? 24f : 28f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
            var labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            }
        }

        private void ConfigureRow(RectTransform row, float spacing, float preferredHeight)
        {
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(8, 8, 2, 2);
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
                && root.Find("SingleAxisCard") != null
                && root.Find("SingleAxisCard/Grid") != null
                && root.Find("SingleAxisCard/Grid/SingleAxisRow_0") != null
                && root.Find("SingleAxisCard/Grid/SingleAxisRow_5") != null
                && root.Find("MultiAxisCard") != null
                && root.Find("MultiAxisCard/SliderStack") != null
                && root.Find("MultiAxisCard/SliderStack/MultiAxisRow_0") != null
                && root.Find("MultiAxisCard/SliderStack/MultiAxisRow_5") != null
                && root.Find("MultiAxisCard/ActionRow") != null
                && root.Find("SummaryCard") != null
                && root.Find("SummaryCard/JointSummary") != null
                && root.GetComponent<VerticalLayoutGroup>() == null;
        }

        private void BindSceneAuthoredReferences(RectTransform root)
        {
            jointSummaryText = root.Find("SummaryCard/JointSummary")?.GetComponent<Text>();
        }
    }
}
