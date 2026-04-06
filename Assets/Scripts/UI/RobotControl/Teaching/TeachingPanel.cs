// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// V2 Teaching 최소 패널의 scene-authored 레이아웃을 우선 바인딩하고,
    /// authored 구조가 없을 때만 fallback 레이아웃을 구성합니다.
    /// </summary>
    public sealed class TeachingPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text summaryText;

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
            if (summaryText != null)
            {
                summaryText.text = state.LastCommandSummary;
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
            EnsureQuickActionRow(root, compact);
            EnsurePointListCard(root, compact);
            EnsureTpdCard(root, compact);
            EnsureSummaryCard(root, compact);
            UiRuntimeStyle.ForceTextHierarchySize(root, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "Hint");
            RemoveDirectChild(root, "SummaryText");
        }

        private void EnsureHeader(RectTransform root, bool compact)
        {
            var header = root.Find("Header") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "Header");
            var element = UiRuntimeStyle.EnsureLayoutElement(header);
            element.preferredHeight = compact ? 42f : 48f;

            var title = UiRuntimeStyle.EnsureText(header, "Title", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 22f), new Vector2(0f, 0f));
            title.text = "티칭";

            var hint = UiRuntimeStyle.EnsureText(header, "Hint", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 20f), new Vector2(0f, -22f));
            hint.text = "Points + Motion Sequence + Preview의 최소 티칭 UX를 제공합니다.";
        }

        private void EnsureQuickActionRow(RectTransform root, bool compact)
        {
            var row = root.Find("QuickActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "QuickActionRow");
            ConfigureRow(row, 8f, compact ? 38f : 42f);
            EnsureActionButton(row, "BtnQuickSave", "빠른 포인트 저장", UIDesignTokens.RobotControlV2.Colors.Accent, compact);
            EnsureActionButton(row, "BtnNamedSave", "이름 저장", UIDesignTokens.RobotControlV2.Colors.Card, compact);
        }

        private void EnsurePointListCard(RectTransform root, bool compact)
        {
            var card = root.Find("PointListCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "PointListCard");
            SetupCard(card, compact ? 162f : 176f);
            EnsureSectionTitle(card, "Title", "포인트 목록", compact);

            var list = card.Find("List") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "List");
            UiRuntimeStyle.Anchor(list, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -34f), new Vector2(-12f, -(compact ? 112f : 122f)));
            var layout = list.GetComponent<VerticalLayoutGroup>() ?? list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = compact ? 4f : 6f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            EnsurePointListRow(list, "PointRow_0", "P_HOME", compact);
            EnsurePointListRow(list, "PointRow_1", "P_READY", compact);
            EnsurePointListRow(list, "PointRow_2", "P_PICK", compact);
            EnsurePointListRow(list, "PointRow_3", "P_PLACE", compact);

            var actionRow = card.Find("ActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "ActionRow");
            UiRuntimeStyle.Anchor(actionRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 40f));
            ConfigureRow(actionRow, 8f, compact ? 34f : 38f);
            EnsureActionButton(actionRow, "BtnLoad", "선택 불러오기", UIDesignTokens.RobotControlV2.Colors.CardAlt, compact);
            EnsureActionButton(actionRow, "BtnDelete", "선택 삭제", UIDesignTokens.RobotControlV2.Colors.Danger, compact);
        }

        private void EnsureTpdCard(RectTransform root, bool compact)
        {
            var card = root.Find("TpdCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "TpdCard");
            SetupCard(card, compact ? 96f : 108f);
            EnsureSectionTitle(card, "Title", "TPD", compact);

            var body = UiRuntimeStyle.EnsureText(card, "Body", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(body.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -34f), new Vector2(-12f, -52f));
            body.text = "기록 / 중지 / 재생 UI만 먼저 제공합니다.";

            var row = card.Find("ActionRow") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(card, "ActionRow");
            UiRuntimeStyle.Anchor(row, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 44f));
            ConfigureRow(row, 8f, compact ? 34f : 38f);
            EnsureActionButton(row, "BtnRecord", "TPD 기록 시작", UIDesignTokens.RobotControlV2.Colors.Accent, compact);
            EnsureActionButton(row, "BtnStop", "TPD 기록 중지", UIDesignTokens.RobotControlV2.Colors.CardAlt, compact);
            EnsureActionButton(row, "BtnPlay", "TPD 재생", UIDesignTokens.RobotControlV2.Colors.Success, compact);
        }

        private void EnsureSummaryCard(RectTransform root, bool compact)
        {
            var card = root.Find("SummaryCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "SummaryCard");
            SetupCard(card, compact ? 52f : 60f);
            summaryText = UiRuntimeStyle.EnsureText(card, "SummaryText", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Stretch(summaryText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));
            summaryText.text = "아직 실행한 명령이 없습니다";
        }

        private void EnsurePointListRow(RectTransform parent, string name, string label, bool compact)
        {
            var row = parent.Find(name) as RectTransform ?? UiRuntimeStyle.EnsureRectChild(parent, name);
            ConfigureRow(row, 8f, compact ? 24f : 26f);
            PaintCard(row, UIDesignTokens.RobotControlV2.Colors.CardAlt);
            var nameText = UiRuntimeStyle.EnsureText(row, "Name", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            var nameElement = UiRuntimeStyle.EnsureLayoutElement(nameText);
            nameElement.flexibleWidth = 1f;
            nameText.text = label;
            var poseText = UiRuntimeStyle.EnsureText(row, "Pose", fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Normal, TextAnchor.MiddleRight, UIDesignTokens.RobotControlV2.Colors.MutedText);
            var poseElement = UiRuntimeStyle.EnsureLayoutElement(poseText);
            poseElement.preferredWidth = compact ? 92f : 108f;
            poseElement.minWidth = compact ? 80f : 92f;
            poseText.text = "Ready 포즈";
        }

        private void EnsureSectionTitle(RectTransform parent, string name, string label, bool compact)
        {
            var title = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, UIDesignTokens.RobotControlV2.Type.UniformText, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Warning);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 18f), new Vector2(12f, -12f));
            title.text = label;
        }

        private void EnsureActionButton(RectTransform parent, string name, string label, Color color, bool compact)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 112f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.flexibleWidth = 1f;
            element.preferredHeight = compact ? 34f : 38f;
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

        private void ConfigureRow(RectTransform row, float spacing, float preferredHeight)
        {
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var element = UiRuntimeStyle.EnsureLayoutElement(row);
            element.preferredHeight = preferredHeight;
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
                && root.Find("QuickActionRow") != null
                && root.Find("QuickActionRow/BtnQuickSave") != null
                && root.Find("QuickActionRow/BtnNamedSave") != null
                && root.Find("PointListCard") != null
                && root.Find("PointListCard/List") != null
                && root.Find("PointListCard/List/PointRow_0") != null
                && root.Find("PointListCard/ActionRow") != null
                && root.Find("TpdCard") != null
                && root.Find("TpdCard/ActionRow") != null
                && root.Find("SummaryCard") != null
                && root.Find("SummaryCard/SummaryText") != null
                && root.GetComponent<VerticalLayoutGroup>() == null;
        }

        private void BindSceneAuthoredReferences(RectTransform root)
        {
            summaryText = root.Find("SummaryCard/SummaryText")?.GetComponent<Text>();
        }
    }
}
