using System.Collections.Generic;
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 2DOF DH 테이블을 렌더하고 d/a/alpha 편집 입력을 AppController에 전달합니다.
    /// </summary>
    public class DHTableEditor : MonoBehaviour
    {
        [SerializeField] private RectTransform tableRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private int decimals = 4;
        [SerializeField] private Graphic panelBackground;
        [SerializeField] private Text panelTitleText;
        [SerializeField] private Text panelSubtitleText;

        private readonly List<RowRefs> rows = new List<RowRefs>();
        private AppController appController;

        private sealed class RowRefs
        {
            public int Index;
            public InputField Theta;
            public InputField D;
            public InputField A;
            public InputField Alpha;
            public Text JointType;
        }

        public void Bind(AppController owner)
        {
            UnbindCurrent();
            appController = owner;

            EnsureRoot();
            RebuildRows();
            RefreshAllRows();

            if (appController != null)
            {
                appController.OnTemplateChanged += HandleTemplateChanged;
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
            }
        }

        private void OnDestroy()
        {
            UnbindCurrent();
        }

        /// <summary>
        /// 테스트/외부 호출용 원시 값 반영 진입점입니다.
        /// </summary>
        public bool TryApplyRawValue(int rowIndex, DhEditableField field, string rawValue)
        {
            if (appController == null || rowIndex < 0 || rowIndex >= rows.Count)
            {
                return false;
            }

            if (!TryParseFinite(rawValue, out var parsed))
            {
                RefreshRow(rowIndex);
                return false;
            }

            var success = appController.TrySetDhParameter(rowIndex, field, parsed, out _);
            RefreshRow(rowIndex);
            return success;
        }

        public static bool TryParseFinite(string raw, out double value)
        {
            value = 0.0;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }

            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static string FormatDouble(double value, int digits = 4)
        {
            var safeDigits = Mathf.Clamp(digits, 0, 10);
            return value.ToString($"F{safeDigits}", CultureInfo.InvariantCulture);
        }

        private void HandleTemplateChanged(RobotTemplate _)
        {
            RebuildRows();
            RefreshAllRows();
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            RefreshAllRows();
        }

        private void OnEditableEndEdit(int rowIndex, DhEditableField field, string raw)
        {
            TryApplyRawValue(rowIndex, field, raw);
        }

        private void EnsureRoot()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            var rect = transform as RectTransform;
            if (rect != null)
            {
                UiRuntimeStyle.Stretch(rect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 146f), new Vector2(372f, -92f));
            }

            if (panelBackground == null)
            {
                panelBackground = UiRuntimeStyle.EnsureImage(transform, "LeftPanelBackground", UiRuntimeStyle.PanelBackground);
            }

            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            panelTitleText ??= UiRuntimeStyle.EnsureText(transform, "LeftPanelTitleText", fallbackFont, 22, FontStyle.Bold, TextAnchor.UpperLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(panelTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 28f), new Vector2(20f, -18f));
            panelTitleText.text = "DH Parameters";

            panelSubtitleText ??= UiRuntimeStyle.EnsureText(transform, "LeftPanelSubtitleText", fallbackFont, 13, FontStyle.Normal, TextAnchor.UpperLeft, UiRuntimeStyle.TextSecondary);
            UiRuntimeStyle.Anchor(panelSubtitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 24f), new Vector2(20f, -48f));
            panelSubtitleText.text = "theta is read-only. Edit d / a / alpha only.";

            if (tableRoot == null)
            {
                var existing = transform.Find("DHTableRoot");
                if (existing != null)
                {
                    tableRoot = existing as RectTransform;
                }
            }

            if (tableRoot == null)
            {
                tableRoot = UiRuntimeStyle.EnsureRectChild(transform, "DHTableRoot");
                UiRuntimeStyle.EnsureVerticalLayout(tableRoot.gameObject, 8f, false);
            }

            UiRuntimeStyle.Stretch(tableRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 16f), new Vector2(-16f, -88f));
            StyleInteractiveCards();
        }

        private void RebuildRows()
        {
            rows.Clear();

            if (tableRoot == null || appController == null || appController.CurrentTemplate == null)
            {
                return;
            }

            for (var i = tableRoot.childCount - 1; i >= 0; i--)
            {
                var child = tableRoot.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            CreateHeaderRow();

            var dof = appController.CurrentTemplate.Dof;
            for (var i = 0; i < dof; i++)
            {
                rows.Add(CreateDataRow(i));
            }
        }

        private void CreateHeaderRow()
        {
            var header = new GameObject("DHHeaderRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(tableRoot, false);
            header.GetComponent<Image>().color = UiRuntimeStyle.CardBackground;
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0f, 34f);

            var layout = UiRuntimeStyle.EnsureHorizontalLayout(header, 6f);
            layout.padding = new RectOffset(10, 10, 6, 6);

            CreateHeaderLabel(header.transform, "Joint", 64f);
            CreateHeaderLabel(header.transform, "theta", 70f);
            CreateHeaderLabel(header.transform, "d", 58f);
            CreateHeaderLabel(header.transform, "a", 58f);
            CreateHeaderLabel(header.transform, "alpha", 70f);
        }

        private RowRefs CreateDataRow(int rowIndex)
        {
            var row = new GameObject($"DHRow_{rowIndex}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(tableRoot, false);
            row.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.88f);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);

            var layout = UiRuntimeStyle.EnsureHorizontalLayout(row, 6f);
            layout.padding = new RectOffset(10, 10, 6, 6);

            var refs = new RowRefs { Index = rowIndex };
            refs.JointType = CreateReadOnlyText(row.transform, $"JointType_{rowIndex}", 64f);
            refs.Theta = CreateInput(row.transform, $"ThetaInput_{rowIndex}", false, 70f);
            refs.D = CreateInput(row.transform, $"DInput_{rowIndex}", true, 58f);
            refs.A = CreateInput(row.transform, $"AInput_{rowIndex}", true, 58f);
            refs.Alpha = CreateInput(row.transform, $"AlphaInput_{rowIndex}", true, 70f);

            refs.D.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.D, value));
            refs.A.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.A, value));
            refs.Alpha.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.Alpha, value));

            return refs;
        }

        private Text CreateHeaderLabel(Transform parent, string text, float width)
        {
            var label = UiRuntimeStyle.EnsureText(parent, $"Header_{text}", fallbackFont, 13, FontStyle.Bold, TextAnchor.MiddleCenter, UiRuntimeStyle.TextSecondary);
            var element = UiRuntimeStyle.EnsureLayoutElement(label);
            element.minWidth = width;
            element.preferredWidth = width;
            label.text = text;
            return label;
        }

        private Text CreateReadOnlyText(Transform parent, string name, float width)
        {
            var text = UiRuntimeStyle.EnsureText(parent, name, fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter, UiRuntimeStyle.TextPrimary);
            var element = UiRuntimeStyle.EnsureLayoutElement(text);
            element.minWidth = width;
            element.preferredWidth = width;
            text.text = "-";
            return text;
        }

        private InputField CreateInput(Transform parent, string name, bool interactable, float width)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);

            var element = UiRuntimeStyle.EnsureLayoutElement(root.GetComponent<InputField>());
            element.minWidth = width;
            element.preferredWidth = width;

            var image = root.GetComponent<Image>();
            image.color = interactable ? UiRuntimeStyle.PanelBackgroundAlt : new Color(0.09f, 0.10f, 0.15f, 0.75f);

            var input = root.GetComponent<InputField>();
            input.interactable = interactable;
            input.contentType = InputField.ContentType.DecimalNumber;
            input.lineType = InputField.LineType.SingleLine;

            var text = UiRuntimeStyle.EnsureText(root.transform, "Text", fallbackFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 1f), new Vector2(-4f, -1f));
            input.textComponent = text;

            input.SetTextWithoutNotify("0");
            return input;
        }

        private void RefreshAllRows()
        {
            if (appController == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                RefreshRow(i);
            }
        }

        private void RefreshRow(int index)
        {
            if (appController == null || index < 0 || index >= rows.Count)
            {
                return;
            }

            var refs = rows[index];
            var link = appController.CurrentLinks[index];
            var jointValues = appController.CurrentJointValuesRad;
            var thetaDeg = index < jointValues.Length ? jointValues[index] * Mathf.Rad2Deg : 0.0;

            refs.JointType.text = link.JointType.ToString();
            refs.Theta.SetTextWithoutNotify(FormatDouble(thetaDeg, 1));
            refs.D.SetTextWithoutNotify(FormatDouble(link.D, decimals));
            refs.A.SetTextWithoutNotify(FormatDouble(link.A, decimals));
            refs.Alpha.SetTextWithoutNotify(FormatDouble(link.Alpha, decimals));
        }

        private void StyleInteractiveCards()
        {
            StyleCard("rz_panel", "Rz(theta)", UiRuntimeStyle.AccentBlue, new Vector2(0f, 0f), new Vector2(164f, 54f));
            StyleCard("tz_panel", "Tz(d)", UiRuntimeStyle.AccentBlue, new Vector2(176f, 0f), new Vector2(164f, 54f));
            StyleCard("tx_panel", "Tx(a)", UiRuntimeStyle.AccentYellow, new Vector2(0f, -62f), new Vector2(164f, 54f));
            StyleCard("rx_panel", "Rx(alpha)", UiRuntimeStyle.AccentYellow, new Vector2(176f, -62f), new Vector2(164f, 54f));
            StyleCard("mul_progress", "Multiply", UiRuntimeStyle.TextMuted, new Vector2(0f, -124f), new Vector2(164f, 44f));
            StyleCard("chain_complete", "Chain", UiRuntimeStyle.TextMuted, new Vector2(176f, -124f), new Vector2(164f, 44f));

            var tableTarget = GameObject.Find("DHTableTarget")?.transform as RectTransform;
            if (tableTarget != null)
            {
                UiRuntimeStyle.Stretch(tableTarget, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -88f), new Vector2(-12f, -40f));
                var image = tableTarget.GetComponent<Image>();
                if (image == null)
                {
                    image = tableTarget.gameObject.AddComponent<Image>();
                }

                image.color = new Color(1f, 1f, 1f, 0.01f);
            }

            var cellTarget = GameObject.Find("DHCellTarget")?.transform as RectTransform;
            if (cellTarget != null)
            {
                UiRuntimeStyle.Anchor(cellTarget, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, 32f), new Vector2(120f, -132f));
                var image = cellTarget.GetComponent<Image>();
                if (image == null)
                {
                    image = cellTarget.gameObject.AddComponent<Image>();
                }

                image.color = new Color(1f, 1f, 1f, 0.01f);
            }
        }

        private void StyleCard(string objectName, string label, Color accent, Vector2 anchoredPosition, Vector2 size)
        {
            var go = GameObject.Find(objectName);
            if (go == null)
            {
                return;
            }

            var rect = go.transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            UiRuntimeStyle.Anchor(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), size, new Vector2(18f, 164f) + anchoredPosition);

            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = new Color(accent.r * 0.35f, accent.g * 0.35f, accent.b * 0.35f, 0.65f);

            var text = UiRuntimeStyle.EnsureText(go.transform, "CardLabel", fallbackFont, 12, FontStyle.Bold, TextAnchor.MiddleCenter, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            text.text = label;
        }

        private void UnbindCurrent()
        {
            if (appController != null)
            {
                appController.OnTemplateChanged -= HandleTemplateChanged;
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController = null;
            }
        }
    }
}
