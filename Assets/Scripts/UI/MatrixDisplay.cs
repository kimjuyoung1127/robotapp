using System.Globalization;
using System.Text;
using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.UI
{
    /// <summary>
    /// A1/A2/T02 행렬을 실시간으로 표시합니다.
    /// </summary>
    public class MatrixDisplay : MonoBehaviour
    {
        [SerializeField] private RectTransform matrixRoot;
        [SerializeField] private Text a1Text;
        [SerializeField] private Text a2Text;
        [SerializeField] private Text t02Text;
        [SerializeField] private Font fallbackFont;
        [SerializeField] private int decimals = 4;

        private AppController appController;

        public string A1RenderedText => a1Text != null ? a1Text.text : string.Empty;
        public string A2RenderedText => a2Text != null ? a2Text.text : string.Empty;
        public string T02RenderedText => t02Text != null ? t02Text.text : string.Empty;

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
            if (fallbackFont == null)
            {
                fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (matrixRoot == null)
            {
                var existing = transform.Find("MatrixDisplayRuntime");
                if (existing != null)
                {
                    matrixRoot = existing as RectTransform;
                }
            }

            if (matrixRoot == null)
            {
                var root = new GameObject("MatrixDisplayRuntime", typeof(RectTransform), typeof(VerticalLayoutGroup));
                root.transform.SetParent(transform, false);
                matrixRoot = root.GetComponent<RectTransform>();
                matrixRoot.anchorMin = new Vector2(0f, 0f);
                matrixRoot.anchorMax = new Vector2(1f, 0.55f);
                matrixRoot.offsetMin = new Vector2(8f, 8f);
                matrixRoot.offsetMax = new Vector2(-8f, -8f);

                var layout = root.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            a1Text ??= FindOrCreateText("MatrixA1Text");
            a2Text ??= FindOrCreateText("MatrixA2Text");
            t02Text ??= FindOrCreateText("MatrixT02Text");
        }

        private Text FindOrCreateText(string name)
        {
            var existing = matrixRoot.Find(name);
            if (existing != null)
            {
                var found = existing.GetComponent<Text>();
                if (found != null)
                {
                    return found;
                }
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(matrixRoot, false);

            var text = go.GetComponent<Text>();
            text.font = fallbackFont;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
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
