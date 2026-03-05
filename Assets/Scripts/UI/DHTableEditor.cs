using System;
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
            if (fallbackFont == null)
            {
                fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

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
                var go = new GameObject("DHTableRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
                go.transform.SetParent(transform, false);
                tableRoot = go.GetComponent<RectTransform>();
                tableRoot.anchorMin = new Vector2(0f, 0f);
                tableRoot.anchorMax = new Vector2(1f, 1f);
                tableRoot.offsetMin = new Vector2(8f, 8f);
                tableRoot.offsetMax = new Vector2(-8f, -8f);

                var layout = go.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
            }
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
            var header = new GameObject("DHHeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(tableRoot, false);
            var layout = header.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateHeaderLabel(header.transform, "Joint");
            CreateHeaderLabel(header.transform, "theta(deg)");
            CreateHeaderLabel(header.transform, "d");
            CreateHeaderLabel(header.transform, "a");
            CreateHeaderLabel(header.transform, "alpha");
        }

        private RowRefs CreateDataRow(int rowIndex)
        {
            var row = new GameObject($"DHRow_{rowIndex}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(tableRoot, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var refs = new RowRefs { Index = rowIndex };
            refs.JointType = CreateReadOnlyText(row.transform, $"JointType_{rowIndex}");
            refs.Theta = CreateInput(row.transform, $"ThetaInput_{rowIndex}", false);
            refs.D = CreateInput(row.transform, $"DInput_{rowIndex}", true);
            refs.A = CreateInput(row.transform, $"AInput_{rowIndex}", true);
            refs.Alpha = CreateInput(row.transform, $"AlphaInput_{rowIndex}", true);

            refs.D.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.D, value));
            refs.A.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.A, value));
            refs.Alpha.onEndEdit.AddListener(value => OnEditableEndEdit(rowIndex, DhEditableField.Alpha, value));

            return refs;
        }

        private Text CreateHeaderLabel(Transform parent, string text)
        {
            var go = new GameObject($"Header_{text}", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = fallbackFont;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private Text CreateReadOnlyText(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = fallbackFont;
            text.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "-";
            return text;
        }

        private InputField CreateInput(Transform parent, string name, bool interactable)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);

            var image = root.GetComponent<Image>();
            image.color = interactable
                ? new Color(0.20f, 0.20f, 0.30f, 0.95f)
                : new Color(0.15f, 0.15f, 0.20f, 0.85f);

            var input = root.GetComponent<InputField>();
            input.interactable = interactable;
            input.contentType = InputField.ContentType.DecimalNumber;
            input.lineType = InputField.LineType.SingleLine;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(root.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = fallbackFont;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 1f);
            rect.offsetMax = new Vector2(-4f, -1f);

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

            var links = appController.CurrentLinks;
            var joints = appController.CurrentJointValuesRad;

            if (index >= links.Length || index >= joints.Length)
            {
                return;
            }

            var link = links[index];
            var thetaRad = link.JointType == JointType.Revolute
                ? link.Theta + joints[index]
                : link.Theta;
            var thetaDeg = thetaRad * Mathf.Rad2Deg;

            var row = rows[index];
            row.JointType.text = link.JointType.ToString();
            row.Theta.SetTextWithoutNotify(FormatDouble(thetaDeg, decimals));
            row.D.SetTextWithoutNotify(FormatDouble(link.D, decimals));
            row.A.SetTextWithoutNotify(FormatDouble(link.A, decimals));
            row.Alpha.SetTextWithoutNotify(FormatDouble(link.Alpha, decimals));
        }

        private void UnbindCurrent()
        {
            if (appController == null)
            {
                return;
            }

            appController.OnTemplateChanged -= HandleTemplateChanged;
            appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
            appController = null;
        }
    }
}
