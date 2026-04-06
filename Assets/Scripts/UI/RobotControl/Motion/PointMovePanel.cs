// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// V2 포인트 이동 패널의 scene-authored 레이아웃을 우선 바인딩하고,
    /// authored 구조가 없을 때만 fallback 레이아웃을 구성합니다.
    /// </summary>
    public sealed class PointMovePanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text targetText;
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
            if (targetText != null)
            {
                targetText.text = $"미리보기 목표: {state.PreviewTarget}";
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
            background.color = UIDesignTokens.RobotControlV2.Colors.Card;
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
            EnsureTargetCard(root, compact);
            EnsurePoseGrid(root, compact);
            EnsureActionRow(root, compact);
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "Hint");
            RemoveDirectChild(root, "TargetText");
            RemoveDirectChild(root, "BtnCalculate");
            RemoveDirectChild(root, "BtnMove");
            RemoveDirectChild(root, "BtnRestore");
        }

        private void EnsureHeader(RectTransform root, bool compact)
        {
            var header = root.Find("Header") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "Header");
            var element = UiRuntimeStyle.EnsureLayoutElement(header);
            element.preferredHeight = compact ? 42f : 48f;

            var title = UiRuntimeStyle.EnsureText(header, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 22f), new Vector2(0f, 0f));
            title.text = "포인트 이동";

            var hint = UiRuntimeStyle.EnsureText(header, "Hint", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 20f), new Vector2(0f, -22f));
            hint.text = "Cartesian 입력을 기준으로 관절 계산과 이동 CTA를 제공합니다.";
        }

        private void EnsureTargetCard(RectTransform root, bool compact)
        {
            var card = root.Find("TargetCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "TargetCard");
            SetupCard(card, compact ? 68f : 76f);

            currentPoseText = UiRuntimeStyle.EnsureText(card, "CurrentPose", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(currentPoseText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -8f), new Vector2(-12f, -28f));
            currentPoseText.text = "현재 TCP · X -497 / Y -130 / Z 477 / RX 180 / RY 0 / RZ 90";

            targetText = UiRuntimeStyle.EnsureText(card, "TargetText", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(targetText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 8f), new Vector2(-12f, 28f));
            targetText.text = "미리보기 목표: Ready 포즈";
        }

        private void EnsurePoseGrid(RectTransform root, bool compact)
        {
            var gridRoot = root.Find("PoseGrid") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "PoseGrid");
            var grid = gridRoot.GetComponent<GridLayoutGroup>() ?? gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.spacing = new Vector2(8f, compact ? 6f : 8f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.cellSize = new Vector2(compact ? 146f : 154f, compact ? 38f : 42f);
            var element = UiRuntimeStyle.EnsureLayoutElement(gridRoot);
            element.preferredHeight = compact ? 126f : 142f;

            EnsurePoseInput(gridRoot, "X", compact);
            EnsurePoseInput(gridRoot, "Y", compact);
            EnsurePoseInput(gridRoot, "Z", compact);
            EnsurePoseInput(gridRoot, "RX", compact);
            EnsurePoseInput(gridRoot, "RY", compact);
            EnsurePoseInput(gridRoot, "RZ", compact);
        }

        private void EnsureActionRow(RectTransform root, bool compact)
        {
            var row = root.Find("ActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "ActionRow");
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var element = UiRuntimeStyle.EnsureLayoutElement(row);
            element.preferredHeight = compact ? 38f : 42f;

            EnsureButton(row, "BtnCalculate", "관절 위치 계산", compact, UIDesignTokens.RobotControlV2.Colors.CardAlt);
            EnsureButton(row, "BtnMove", "이 포인트로 이동", compact, UIDesignTokens.RobotControlV2.Colors.Accent);
            EnsureButton(row, "BtnRestore", "복원", compact, UIDesignTokens.RobotControlV2.Colors.CardAlt);
        }

        private void EnsurePoseInput(RectTransform parent, string label, bool compact)
        {
            var row = parent.Find($"{label}Card") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, $"{label}Card");
            SetupCard(row, compact ? 38f : 42f);

            var labelText = UiRuntimeStyle.EnsureText(row, "Label", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Anchor(labelText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(28f, 18f), new Vector2(10f, 0f));
            labelText.text = label;

            var input = UIComponentFactory.CreateInputField(row, "ValueInput", "0.0", fallbackFont);
            UiRuntimeStyle.Anchor(input.transform as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(compact ? 80f : 92f, compact ? 28f : 30f), new Vector2(-10f, 0f));
        }

        private void EnsureButton(RectTransform parent, string name, string label, bool compact, Color color)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 112f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.flexibleWidth = 1f;
            element.preferredHeight = compact ? 38f : 42f;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
            var text = button.transform.Find("Label")?.GetComponent<Text>();
            if (text != null)
            {
                text.fontSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = UIDesignTokens.RobotControlV2.Type.UniformText;
                text.resizeTextMaxSize = UIDesignTokens.RobotControlV2.Type.UniformText;
            }
        }

        private void SetupCard(RectTransform card, float preferredHeight)
        {
            var bg = card.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
            if (bg.sprite == null)
            {
                bg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                bg.type = Image.Type.Sliced;
            }
            bg.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;
            var element = UiRuntimeStyle.EnsureLayoutElement(card);
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
                && root.Find("TargetCard") != null
                && root.Find("TargetCard/CurrentPose") != null
                && root.Find("TargetCard/TargetText") != null
                && root.Find("PoseGrid") != null
                && root.Find("PoseGrid/XCard") != null
                && root.Find("PoseGrid/RZCard") != null
                && root.Find("ActionRow") != null
                && root.Find("ActionRow/BtnCalculate") != null
                && root.Find("ActionRow/BtnMove") != null
                && root.Find("ActionRow/BtnRestore") != null
                && root.GetComponent<VerticalLayoutGroup>() == null;
        }

        private void BindSceneAuthoredReferences(RectTransform root)
        {
            currentPoseText = root.Find("TargetCard/CurrentPose")?.GetComponent<Text>();
            targetText = root.Find("TargetCard/TargetText")?.GetComponent<Text>();
        }
    }
}
