// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    [ExecuteAlways]
    public class DHTableEditor : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private RectTransform tableRoot;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private int decimals = 4;
        [SerializeField] private Graphic panelBackground;
        [SerializeField] private Text panelTitleText;
        [SerializeField] private Text panelSubtitleText;

        private readonly List<DHTableRowRefs> rows = new List<DHTableRowRefs>();
        private AppController appController;

        private void OnEnable()
        {
            EnsureRoot();
        }

        public static bool TryParseFinite(string raw, out double value)
        {
            return DHTableValueFormatter.TryParseFinite(raw, out value);
        }

        public static string FormatDouble(double value, int decimals)
        {
            return DHTableValueFormatter.FormatDouble(value, decimals);
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
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

        public bool TryApplyRawValue(int rowIndex, DhEditableField field, string rawValue)
        {
            if (appController == null || rowIndex < 0 || rowIndex >= rows.Count)
            {
                return false;
            }

            if (!DHTableValueFormatter.TryParseFinite(rawValue, out var parsed))
            {
                RefreshRow(rowIndex);
                return false;
            }

            var success = appController.TrySetDhParameter(rowIndex, field, parsed, out _);
            RefreshRow(rowIndex);
            return success;
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
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "LeftPanelRect");
            UiRuntimeStyle.Stretch(panelRoot, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 146f), new Vector2(372f, -92f));

            if (panelBackground == null)
            {
                panelBackground = UiRuntimeStyle.EnsureImage(panelRoot, "LeftPanelBackground", UIDesignTokens.Colors.SurfaceRaised);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(panelBackground, panelRoot);
            }

            UiRuntimeStyle.Stretch((RectTransform)panelBackground.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            panelTitleText = panelTitleText == null
                ? UiRuntimeStyle.EnsureText(panelRoot, "LeftPanelTitleText", fallbackFont, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary)
                : UiRuntimeStyle.ReparentTo(panelTitleText, panelRoot);
            UiRuntimeStyle.Anchor(panelTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 28f), new Vector2(20f, -18f));
            panelTitleText.text = "DH Parameters";

            panelSubtitleText = panelSubtitleText == null
                ? UiRuntimeStyle.EnsureText(panelRoot, "LeftPanelSubtitleText", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary)
                : UiRuntimeStyle.ReparentTo(panelSubtitleText, panelRoot);
            UiRuntimeStyle.Anchor(panelSubtitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 24f), new Vector2(20f, -48f));
            panelSubtitleText.text = "theta is read-only. Edit d / a / alpha only.";

            if (tableRoot == null)
            {
                var existing = panelRoot.Find("DHTableRoot");
                if (existing != null)
                {
                    tableRoot = existing as RectTransform;
                }
            }

            if (tableRoot == null)
            {
                tableRoot = UiRuntimeStyle.EnsureRectChild(panelRoot, "DHTableRoot");
                UiRuntimeStyle.EnsureVerticalLayout(tableRoot.gameObject, 8f, false);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(tableRoot, panelRoot);
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

            DHTableViewBuilder.CreateHeaderRow(tableRoot, fallbackFont);

            var dof = appController.CurrentTemplate.Dof;
            for (var i = 0; i < dof; i++)
            {
                rows.Add(DHTableViewBuilder.CreateDataRow(tableRoot, fallbackFont, i, value => OnEditableEndEdit(i, DhEditableField.D, value), value => OnEditableEndEdit(i, DhEditableField.A, value), value => OnEditableEndEdit(i, DhEditableField.Alpha, value)));
            }
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
            refs.Theta.SetTextWithoutNotify(DHTableValueFormatter.FormatDouble(thetaDeg, 1));
            refs.D.SetTextWithoutNotify(DHTableValueFormatter.FormatDouble(link.D, decimals));
            refs.A.SetTextWithoutNotify(DHTableValueFormatter.FormatDouble(link.A, decimals));
            refs.Alpha.SetTextWithoutNotify(DHTableValueFormatter.FormatDouble(link.Alpha, decimals));
        }

        private void StyleInteractiveCards()
        {
            StyleCard("rz_panel", "Rz(theta)", UIDesignTokens.Colors.AccentPrimary, new Vector2(0f, 0f), new Vector2(164f, 54f));
            StyleCard("tz_panel", "Tz(d)", UIDesignTokens.Colors.AccentPrimary, new Vector2(176f, 0f), new Vector2(164f, 54f));
            StyleCard("tx_panel", "Tx(a)", UIDesignTokens.Colors.AccentSecondary, new Vector2(0f, -62f), new Vector2(164f, 54f));
            StyleCard("rx_panel", "Rx(alpha)", UIDesignTokens.Colors.AccentSecondary, new Vector2(176f, -62f), new Vector2(164f, 54f));
            StyleCard("mul_progress", "Multiply", UIDesignTokens.Colors.TextMuted, new Vector2(0f, -124f), new Vector2(164f, 44f));
            StyleCard("chain_complete", "Chain", UIDesignTokens.Colors.TextMuted, new Vector2(176f, -124f), new Vector2(164f, 44f));

            var tableTargetTransform = panelRoot.Find("DHTableTarget");
            var tableTarget = tableTargetTransform as RectTransform;
            if (tableTarget != null)
            {
                UiRuntimeStyle.Stretch(tableTarget, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -88f), new Vector2(-12f, -40f));
                var image = tableTarget.GetComponent<Image>() ?? tableTarget.gameObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.01f);
            }

            var cellTargetTransform = panelRoot.Find("DHCellTarget");
            var cellTarget = cellTargetTransform as RectTransform;
            if (cellTarget != null)
            {
                UiRuntimeStyle.Anchor(cellTarget, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, 32f), new Vector2(120f, -132f));
                var image = cellTarget.GetComponent<Image>() ?? cellTarget.gameObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.01f);
            }
        }

        private void StyleCard(string objectName, string label, Color accent, Vector2 anchoredPosition, Vector2 size)
        {
            var found = panelRoot.Find(objectName);
            if (found == null)
            {
                return;
            }

            var go = found.gameObject;
            var rect = found as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.SetParent(panelRoot, false);
            UiRuntimeStyle.Anchor(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), size, new Vector2(18f, 164f) + anchoredPosition);

            var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.color = new Color(accent.r * 0.35f, accent.g * 0.35f, accent.b * 0.35f, 0.65f);

            var text = UiRuntimeStyle.EnsureText(go.transform, "CardLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
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
