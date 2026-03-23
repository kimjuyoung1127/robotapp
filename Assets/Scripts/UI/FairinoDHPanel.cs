// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using KineTutor3D.Math;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FR5 6축 DH 파라미터 테이블 패널입니다.
    /// θ는 읽기 전용, d/a/α는 편집 가능하며 FK 매트릭스를 표시합니다.
    /// </summary>
    public class FairinoDHPanel : MonoBehaviour, IVisibilityControllable
    {
        private const int Dof = 6;
        private const double Rad2Deg = 180.0 / System.Math.PI;

        [SerializeField] private Font fallbackFont;

        private Text[] thetaLabels;
        private InputField[] dInputs;
        private InputField[] aInputs;
        private InputField[] alphaInputs;
        private Text matrixLabel;
        private Button resetButton;
        private RobotKinematicsFacade kinematicsFacade;
        private bool listenersBound;

        /// <summary>
        /// DH 파라미터가 편집되었을 때 발생합니다.
        /// </summary>
        public event Action OnDHParameterEdited;

        /// <summary>
        /// FK facade를 주입합니다.
        /// </summary>
        public void Inject(RobotKinematicsFacade facade)
        {
            kinematicsFacade = facade;
            if (kinematicsFacade != null)
            {
                kinematicsFacade.OnKinematicsUpdated -= OnKinematicsUpdated;
                kinematicsFacade.OnKinematicsUpdated += OnKinematicsUpdated;
            }

            RefreshTable();
            RefreshMatrix();
        }

        private void Awake()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        private void OnDestroy()
        {
            if (kinematicsFacade != null)
            {
                kinematicsFacade.OnKinematicsUpdated -= OnKinematicsUpdated;
            }
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// theta 라벨을 현재 관절 각도로 갱신합니다.
        /// </summary>
        public void RefreshThetaFromJoints(double[] jointAnglesDeg)
        {
            if (thetaLabels == null || jointAnglesDeg == null)
            {
                return;
            }

            for (var i = 0; i < Dof && i < jointAnglesDeg.Length; i++)
            {
                if (thetaLabels[i] != null)
                {
                    thetaLabels[i].text = DHTableValueFormatter.FormatDouble(jointAnglesDeg[i], 1) + "°";
                }
            }
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null || matrixLabel != null)
            {
                return;
            }

            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 22f), new Vector2(16f, -14f));
            title.text = "DH Parameters (FR5)";

            BuildHeader(root);
            thetaLabels = new Text[Dof];
            dInputs = new InputField[Dof];
            aInputs = new InputField[Dof];
            alphaInputs = new InputField[Dof];

            for (var i = 0; i < Dof; i++)
            {
                BuildRow(root, i);
            }

            matrixLabel = UiRuntimeStyle.EnsureText(root, "MatrixLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(matrixLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(380f, 110f), new Vector2(16f, 50f));
            matrixLabel.text = "FK 매트릭스: 대기 중...";

            resetButton ??= UIComponentFactory.CreateSecondaryButton(root, "BtnReset", "초기값 복원", fallbackFont, 110f);
            UiRuntimeStyle.Anchor((RectTransform)resetButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, UIDesignTokens.Size.ButtonHeightSm), new Vector2(16f, 16f));
        }

        private void BuildHeader(RectTransform root)
        {
            var headerRow = UiRuntimeStyle.EnsureRectChild(root, "HeaderRow");
            UiRuntimeStyle.Anchor(headerRow, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 22f), new Vector2(16f, -46f));

            var headers = new[] { "Link", "θ (deg)", "d (m)", "a (m)", "α (rad)" };
            var offsets = new[] { 0f, 40f, 110f, 190f, 270f };
            var widths = new[] { 36f, 64f, 74f, 74f, 74f };

            for (var h = 0; h < headers.Length; h++)
            {
                var label = UiRuntimeStyle.EnsureText(headerRow, $"H{h}", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextMuted);
                UiRuntimeStyle.Anchor(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(widths[h], 18f), new Vector2(offsets[h], 0f));
                label.text = headers[h];
            }
        }

        private void BuildRow(RectTransform root, int index)
        {
            var row = UiRuntimeStyle.EnsureRectChild(root, $"DHRow_{index}");
            var yOffset = -72f - (index * 30f);
            UiRuntimeStyle.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, 26f), new Vector2(16f, yOffset));
            var rowBg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
            rowBg.color = index % 2 == 0 ? UIDesignTokens.Colors.SurfaceCard : UIDesignTokens.Colors.SurfaceRaisedAlt;

            var linkLabel = UiRuntimeStyle.EnsureText(row, "LinkLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleLeft, ResolveJointColor(index));
            UiRuntimeStyle.Anchor(linkLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 18f), new Vector2(0f, 0f));
            linkLabel.text = $"L{index + 1}";

            thetaLabels[index] = UiRuntimeStyle.EnsureText(row, "Theta", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(thetaLabels[index].rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(64f, 18f), new Vector2(40f, 0f));
            thetaLabels[index].text = "0.0°";

            dInputs[index] = CreateParamInput(row, "DInput", 110f, index, "d");
            aInputs[index] = CreateParamInput(row, "AInput", 190f, index, "a");
            alphaInputs[index] = CreateParamInput(row, "AlphaInput", 270f, index, "alpha");
        }

        private InputField CreateParamInput(RectTransform parent, string inputName, float xOffset, int linkIndex, string paramName)
        {
            var existing = parent.Find(inputName)?.GetComponent<InputField>();
            if (existing != null)
            {
                return existing;
            }

            var input = UIComponentFactory.CreateInputField(parent, inputName, "0.0", fallbackFont);
            UiRuntimeStyle.Anchor((RectTransform)input.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(74f, 22f), new Vector2(xOffset, 0f));
            input.contentType = InputField.ContentType.DecimalNumber;
            return input;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            for (var i = 0; i < Dof; i++)
            {
                var capturedIndex = i;
                if (dInputs != null && dInputs[i] != null)
                    dInputs[i].onEndEdit.AddListener(v => OnParamEdited(capturedIndex, "d", v));
                if (aInputs != null && aInputs[i] != null)
                    aInputs[i].onEndEdit.AddListener(v => OnParamEdited(capturedIndex, "a", v));
                if (alphaInputs != null && alphaInputs[i] != null)
                    alphaInputs[i].onEndEdit.AddListener(v => OnParamEdited(capturedIndex, "alpha", v));
            }

            resetButton?.onClick.AddListener(OnResetClicked);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            for (var i = 0; i < Dof; i++)
            {
                if (dInputs != null && dInputs[i] != null) dInputs[i].onEndEdit.RemoveAllListeners();
                if (aInputs != null && aInputs[i] != null) aInputs[i].onEndEdit.RemoveAllListeners();
                if (alphaInputs != null && alphaInputs[i] != null) alphaInputs[i].onEndEdit.RemoveAllListeners();
            }

            resetButton?.onClick.RemoveListener(OnResetClicked);
            listenersBound = false;
        }

        private void OnParamEdited(int linkIndex, string paramName, string rawValue)
        {
            if (kinematicsFacade == null)
            {
                return;
            }

            if (!DHTableValueFormatter.TryParseFinite(rawValue, out var value))
            {
                return;
            }

            kinematicsFacade.SetDhParameter(linkIndex, paramName, value);
            OnDHParameterEdited?.Invoke();
        }

        private void OnResetClicked()
        {
            kinematicsFacade?.ResetToDefault();
            RefreshTable();
        }

        private void OnKinematicsUpdated(Mat4D[] transforms, Mat4D ee)
        {
            RefreshMatrix();
        }

        private void RefreshTable()
        {
            if (kinematicsFacade == null)
            {
                return;
            }

            var links = kinematicsFacade.Links;
            var jointVals = kinematicsFacade.JointValuesRad;

            for (var i = 0; i < Dof && i < links.Length; i++)
            {
                if (thetaLabels != null && thetaLabels[i] != null)
                {
                    var thetaDeg = jointVals[i] * Rad2Deg;
                    thetaLabels[i].text = DHTableValueFormatter.FormatDouble(thetaDeg, 1) + "°";
                }

                if (dInputs != null && dInputs[i] != null)
                    dInputs[i].text = DHTableValueFormatter.FormatDouble(links[i].D, 4);
                if (aInputs != null && aInputs[i] != null)
                    aInputs[i].text = DHTableValueFormatter.FormatDouble(links[i].A, 4);
                if (alphaInputs != null && alphaInputs[i] != null)
                    alphaInputs[i].text = DHTableValueFormatter.FormatDouble(links[i].Alpha, 4);
            }
        }

        private void RefreshMatrix()
        {
            if (matrixLabel == null || kinematicsFacade == null)
            {
                return;
            }

            var ee = kinematicsFacade.EndEffectorTransform;
            var pos = ee.ExtractPosition();
            matrixLabel.text = $"T₀₆ 위치:\n  X: {pos.X.ToString("F4", CultureInfo.InvariantCulture)} m\n  Y: {pos.Y.ToString("F4", CultureInfo.InvariantCulture)} m\n  Z: {pos.Z.ToString("F4", CultureInfo.InvariantCulture)} m";
        }

        private static Color ResolveJointColor(int index)
        {
            switch (index)
            {
                case 0: return UIDesignTokens.Colors.DiagramLink1;
                case 1: return UIDesignTokens.Colors.DiagramLink2;
                case 2: return UIDesignTokens.Colors.DiagramLink3;
                case 3: return UIDesignTokens.Colors.DiagramLink4;
                case 4: return UIDesignTokens.Colors.DiagramLink5;
                default: return UIDesignTokens.Colors.DiagramLink6;
            }
        }
    }
}
