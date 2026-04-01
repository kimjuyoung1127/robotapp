// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// V2 쉬운 조작 패널의 플레이스홀더 레이아웃과 상태 뱃지를 구성합니다.
    /// </summary>
    public sealed class EasyMotionPanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text stateText;

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
                stateText.text = $"{(state.IsMockMode ? "Mock" : "Live")} · Tool {state.ToolId:00} · User {state.UserId:00}";
            }
        }

        public void SetVisible(bool visible)
        {
            UiRuntimeStyle.SetCanvasVisible(gameObject, visible);
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
            UiRuntimeStyle.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var layout = UiRuntimeStyle.EnsureVerticalLayout(root.gameObject, compact ? UIDesignTokens.Space.Xs : UIDesignTokens.Space.Sm);
            layout.padding = new RectOffset(
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md,
                compact ? (int)UIDesignTokens.Space.Sm : (int)UIDesignTokens.Space.Md);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            EnsureHeader(root, "쉬운 조작", "초보자용 큰 버튼 프리셋", compact);

            var presetGrid = EnsureRow(root, "PresetGridTop", compact);
            var presetGridBottom = EnsureRow(root, "PresetGridBottom", compact);
            var actionRow = EnsureRow(root, "ActionRow", compact);
            var infoCard = root.Find("InfoCard") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "InfoCard");
            var infoBg = infoCard.GetComponent<Image>() ?? infoCard.gameObject.AddComponent<Image>();
            infoBg.color = UIDesignTokens.RobotControlV2.Colors.CardAlt;
            var infoElement = UiRuntimeStyle.EnsureLayoutElement(infoCard);
            infoElement.preferredHeight = compact ? 52f : 64f;

            EnsureActionButton(presetGrid, "BtnHome", "HOME", compact);
            EnsureActionButton(presetGrid, "BtnReady", "READY", compact);
            EnsureActionButton(presetGridBottom, "BtnFolded", "FOLDED", compact);
            EnsureActionButton(presetGridBottom, "BtnZero", "ZERO", compact);
            EnsureActionButton(actionRow, "BtnPreview", "미리보기", compact, UIDesignTokens.RobotControlV2.Colors.Success);
            EnsureActionButton(actionRow, "BtnApply", "실제 이동", compact, UIDesignTokens.RobotControlV2.Colors.Danger);

            stateText = UiRuntimeStyle.EnsureText(infoCard, "StateText", fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.RobotControlV2.Colors.TitleText);
            UiRuntimeStyle.Stretch(stateText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 6f), new Vector2(-12f, -6f));
            stateText.text = "Mock · Tool 00 · User 00";
            stateText.fontSize = compact ? 11 : 12;
        }

        private void NormalizeLegacyRootChildren(RectTransform root)
        {
            RemoveDirectChild(root, "Title");
            RemoveDirectChild(root, "Hint");
            RemoveDirectChild(root, "BtnHome");
            RemoveDirectChild(root, "BtnReady");
            RemoveDirectChild(root, "BtnFolded");
            RemoveDirectChild(root, "BtnZero");
            RemoveDirectChild(root, "BtnPreview");
            RemoveDirectChild(root, "BtnApply");
            RemoveDirectChild(root, "StateText");
        }

        private void EnsureHeader(RectTransform root, string title, string hint, bool compact)
        {
            var header = root.Find("Header") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "Header");
            var headerElement = UiRuntimeStyle.EnsureLayoutElement(header);
            headerElement.preferredHeight = compact ? 40f : 48f;

            var titleText = UiRuntimeStyle.EnsureText(header, "Title", fallbackFont, compact ? 13 : 14, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.Accent);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 22f), new Vector2(0f, 0f));
            titleText.text = title;

            var hintText = UiRuntimeStyle.EnsureText(header, "Hint", fallbackFont, compact ? 11 : 12, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.RobotControlV2.Colors.MutedText);
            UiRuntimeStyle.Anchor(hintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 20f), new Vector2(0f, -22f));
            hintText.text = hint;
        }

        private RectTransform EnsureRow(RectTransform root, string name, bool compact)
        {
            var row = root.Find(name) as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, name);
            var layout = UiRuntimeStyle.EnsureHorizontalLayout(row.gameObject, compact ? UIDesignTokens.Space.Xs : UIDesignTokens.Space.Sm);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            var element = UiRuntimeStyle.EnsureLayoutElement(row);
            element.preferredHeight = compact ? 36f : 44f;
            return row;
        }

        private void EnsureActionButton(RectTransform parent, string name, string label, bool compact, Color? colorOverride = null)
        {
            var button = parent.Find(name)?.GetComponent<Button>() ?? UIComponentFactory.CreateSecondaryButton(parent, name, label, fallbackFont, 120f);
            var element = UiRuntimeStyle.EnsureLayoutElement(button);
            element.minWidth = compact ? 96f : 112f;
            element.preferredWidth = compact ? 112f : 132f;
            element.flexibleWidth = 1f;
            element.preferredHeight = compact ? 36f : 44f;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = colorOverride ?? UIDesignTokens.RobotControlV2.Colors.CardAlt;
            }

            var labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.fontSize = compact ? 11 : 12;
                labelText.resizeTextForBestFit = true;
                labelText.resizeTextMinSize = 9;
                labelText.resizeTextMaxSize = compact ? 11 : 12;
            }
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
    }
}
