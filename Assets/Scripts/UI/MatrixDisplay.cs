// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using System.Text;
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// A1/A2/T02 행렬을 실시간으로 표시합니다.
    /// </summary>
    [ExecuteAlways]
    public class MatrixDisplay : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private RectTransform matrixRoot;
        [SerializeField] private Text a1Text;
        [SerializeField] private Text a2Text;
        [SerializeField] private Text t02Text;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private int decimals = 4;

        private AppController appController;

        private void OnEnable()
        {
            EnsureUi();
        }

        public string A1RenderedText => a1Text != null ? a1Text.text : string.Empty;
        public string A2RenderedText => a2Text != null ? a2Text.text : string.Empty;
        public string T02RenderedText => t02Text != null ? t02Text.text : string.Empty;

        /// <summary>패널 가시성을 설정합니다.</summary>
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

            EnsureUi();
            Render(appController != null ? appController.CurrentA1 : Mat4D.Identity,
                appController != null ? appController.CurrentA2 : Mat4D.Identity,
                appController != null ? appController.CurrentT02 : Mat4D.Identity);

            if (appController != null)
            {
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
            }
        }

        private void OnDestroy()
        {
            UnbindCurrent();
        }

        private void HandleKinematicsUpdated(Mat4D a1, Mat4D a2, Mat4D t02, TutorPose _)
        {
            Render(a1, a2, t02);
        }

        private void Render(Mat4D a1, Mat4D a2, Mat4D t02)
        {
            if (a1Text != null)
            {
                a1Text.text = "A1\n" + FormatMatrix(a1);
            }

            if (a2Text != null)
            {
                a2Text.text = "A2\n" + FormatMatrix(a2);
            }

            if (t02Text != null)
            {
                t02Text.text = "T02\n" + FormatMatrix(t02);
            }
        }

        private void EnsureUi()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "RightPanelRect");

            if (matrixRoot == null)
            {
                var existing = panelRoot.Find("MatrixDisplayRuntime");
                if (existing != null)
                {
                    matrixRoot = existing as RectTransform;
                }
            }

            if (matrixRoot == null)
            {
                matrixRoot = UiRuntimeStyle.EnsureRectChild(panelRoot, "MatrixDisplayRuntime");
                UiRuntimeStyle.EnsureVerticalLayout(matrixRoot.gameObject, 8f, false);
            }
            else
            {
                UiRuntimeStyle.ReparentTo(matrixRoot, panelRoot);
            }

            UiRuntimeStyle.Stretch(matrixRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 20f), new Vector2(-16f, 240f));

            var matrixBlue = new Color(UIDesignTokens.Colors.AccentPrimary.r * 0.45f, UIDesignTokens.Colors.AccentPrimary.g * 0.36f, UIDesignTokens.Colors.AccentPrimary.b * 0.36f, 0.96f);
            var matrixWarm = new Color(UIDesignTokens.Colors.AccentSecondary.r * 0.25f, UIDesignTokens.Colors.AccentSecondary.g * 0.23f, UIDesignTokens.Colors.AccentSecondary.b * 0.67f, 0.96f);
            a1Text = EnsureMatrixCard("MatrixA1Card", "MatrixA1Text", ref a1Text, matrixBlue);
            a2Text = EnsureMatrixCard("MatrixA2Card", "MatrixA2Text", ref a2Text, matrixBlue);
            t02Text = EnsureMatrixCard("MatrixT02Card", "MatrixT02Text", ref t02Text, matrixWarm);
            StyleMatrixHotspots();
        }

        private Text EnsureMatrixCard(string cardName, string textName, ref Text field, Color backgroundColor)
        {
            var card = matrixRoot.Find(cardName) as RectTransform;
            if (card == null)
            {
                card = UiRuntimeStyle.EnsureRectChild(matrixRoot, cardName);
                card.gameObject.AddComponent<Image>().color = backgroundColor;
                UiRuntimeStyle.EnsureLayoutElement(card).preferredHeight = 98f;
                UiRuntimeStyle.EnsureLayoutElement(card).minHeight = 98f;
            }

            var image = card.GetComponent<Image>();
            if (image != null)
            {
                image.color = backgroundColor;
            }

            var text = card.Find(textName)?.GetComponent<Text>();
            if (text == null)
            {
                text = UiRuntimeStyle.EnsureText(card, textName, fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            }

            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
            text.font = fallbackFont;
            text.fontSize = UIDesignTokens.Type.Body;
            text.color = UIDesignTokens.Colors.TextPrimary;

            field = text;
            return text;
        }

        private void StyleMatrixHotspots()
        {
            StyleHotspot("matrix_r", "R block", new Vector2(0f, 0f));
            StyleHotspot("matrix_p", "p column", new Vector2(150f, 0f));
            StyleHotspot("pose_position_col", "position", new Vector2(0f, -50f));
            StyleHotspot("pose_rotation_col", "rotation", new Vector2(150f, -50f));
        }

        private void StyleHotspot(string name, string label, Vector2 anchoredPosition)
        {
            var found = panelRoot.Find(name);
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
            UiRuntimeStyle.Anchor(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(132f, 40f), new Vector2(20f, 272f) + anchoredPosition);

            var image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.color = UIDesignTokens.Colors.NavCurrentScene;

            var text = UiRuntimeStyle.EnsureText(go.transform, "HotspotLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            text.text = label;
        }

        private string FormatMatrix(Mat4D matrix)
        {
            var sb = new StringBuilder();
            var fmt = "F" + Mathf.Clamp(decimals, 0, 10);

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    sb.Append(matrix[row, col].ToString(fmt, CultureInfo.InvariantCulture));
                    if (col < 3)
                    {
                        sb.Append("  ");
                    }
                }

                if (row < 3)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private void UnbindCurrent()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController = null;
            }
        }
    }
}

