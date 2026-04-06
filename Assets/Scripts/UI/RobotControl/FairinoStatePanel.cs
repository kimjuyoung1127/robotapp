// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FAIRINO 로봇 실시간 상태 표시 패널입니다.
    /// 관절 각도, TCP 포즈, 에러 메시지, 관절 델타를 표시합니다.
    /// </summary>
    public class FairinoStatePanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Text jointStateLabel;
        [SerializeField] private Text tcpPoseLabel;
        [SerializeField] private Text errorLabel;
        [SerializeField] private Font fallbackFont;

        private static readonly Color ColorAxisX = UIDesignTokens.Colors.AxisX;
        private static readonly Color ColorAxisY = UIDesignTokens.Colors.AxisY;
        private static readonly Color ColorAxisZ = UIDesignTokens.Colors.AxisZ;

        private FairinoConnectionService connectionService;
        private FairinoErrorTranslator errorTranslator;
        private RobotKinematicsFacade kinematicsFacade;
        private double[] previousJointsDeg;
        private bool hasPrevious;
        private bool listenersBound;

        /// <summary>
        /// 연결 서비스, 에러 번역기, FK facade를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoErrorTranslator translator, RobotKinematicsFacade facade = null)
        {
            UnsubscribeService();
            connectionService = service;
            errorTranslator = translator ?? new FairinoErrorTranslator();
            kinematicsFacade = facade;
            EnsurePresentation();
            SubscribeService();
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            SubscribeService();
        }

        private void OnDisable()
        {
            UnsubscribeService();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

            if (TryBindExistingPresentation(root))
            {
                if (!listenersBound)
                {
                    jointStateLabel.text = "관절: 대기 중...";
                    tcpPoseLabel.text = "TCP: 대기 중...";
                    errorLabel.text = string.Empty;
                }

                return;
            }

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 22f), new Vector2(16f, -14f));
            title.text = "Robot State";

            var subtitle = UiRuntimeStyle.EnsureText(root, "Subtitle", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextMuted);
            UiRuntimeStyle.Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 16f), new Vector2(16f, -38f));
            subtitle.text = "실시간 관절/TCP 상태";

            jointStateLabel = UiRuntimeStyle.EnsureText(root, "JointStateLabel", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(jointStateLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 80f), new Vector2(16f, -58f));

            tcpPoseLabel = UiRuntimeStyle.EnsureText(root, "TcpPoseLabel", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(tcpPoseLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 54f), new Vector2(16f, -144f));
            tcpPoseLabel.supportRichText = true;

            errorLabel = UiRuntimeStyle.EnsureText(root, "ErrorLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentDanger);
            UiRuntimeStyle.Anchor(errorLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 48f), new Vector2(16f, 16f));

            if (!listenersBound)
            {
                jointStateLabel.text = "관절: 대기 중...";
                tcpPoseLabel.text = "TCP: 대기 중...";
                errorLabel.text = string.Empty;
            }
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            jointStateLabel = root.Find("JointStateLabel")?.GetComponent<Text>();
            tcpPoseLabel = root.Find("TcpPoseLabel")?.GetComponent<Text>();
            errorLabel = root.Find("ErrorLabel")?.GetComponent<Text>();

            if (jointStateLabel == null || tcpPoseLabel == null || errorLabel == null)
            {
                return false;
            }

            var title = root.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = "Robot State";
            }

            var subtitle = root.Find("Subtitle")?.GetComponent<Text>();
            if (subtitle != null)
            {
                subtitle.text = "실시간 관절/TCP 상태";
            }

            tcpPoseLabel.supportRichText = true;
            return true;
        }

        private void SubscribeService()
        {
            if (listenersBound || connectionService == null)
            {
                return;
            }

            connectionService.OnStateUpdated += OnStateUpdated;
            connectionService.OnError += OnErrorReceived;
            listenersBound = true;

            if (connectionService.LastState.JointPosDeg != null)
            {
                OnStateUpdated(connectionService.LastState);
            }
        }

        private void UnsubscribeService()
        {
            if (!listenersBound || connectionService == null)
            {
                return;
            }

            connectionService.OnStateUpdated -= OnStateUpdated;
            connectionService.OnError -= OnErrorReceived;
            listenersBound = false;
        }

        private void OnStateUpdated(FairinoRobotState state)
        {
            if (jointStateLabel != null)
            {
                var j = state.JointPosDeg;
                if (j != null && hasPrevious && previousJointsDeg != null)
                {
                    jointStateLabel.text = FormatJointsWithDelta(j, previousJointsDeg);
                }
                else
                {
                    jointStateLabel.text = $"J1:{j[0]:F1} J2:{j[1]:F1} J3:{j[2]:F1}\nJ4:{j[3]:F1} J5:{j[4]:F1} J6:{j[5]:F1}";
                }

                previousJointsDeg = (double[])j.Clone();
                hasPrevious = true;
            }

            if (tcpPoseLabel != null)
            {
                var t = state.TcpPose;
                tcpPoseLabel.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ColorAxisX)}>X:{t[0]:F2}</color> "
                    + $"<color=#{ColorUtility.ToHtmlStringRGB(ColorAxisY)}>Y:{t[1]:F2}</color> "
                    + $"<color=#{ColorUtility.ToHtmlStringRGB(ColorAxisZ)}>Z:{t[2]:F2}</color>\n"
                    + $"Rx:{t[3]:F2} Ry:{t[4]:F2} Rz:{t[5]:F2}\n"
                    + $"Mode:{state.RobotMode} Tool:{state.ToolId} User:{state.UserId} Drag:{(state.IsInDragTeach ? "On" : "Off")}";
            }

            if (errorLabel != null)
            {
                errorLabel.text = state.MainErrorCode != 0 || state.SubErrorCode != 0 || state.IsSafetyStop
                    ? $"Fault {state.MainErrorCode}/{state.SubErrorCode} | SafetyStop:{(state.IsSafetyStop ? "On" : "Off")}"
                    : string.Empty;
            }
        }

        private static string FormatJointsWithDelta(double[] current, double[] previous)
        {
            var lines = new System.Text.StringBuilder();
            for (var i = 0; i < 6; i++)
            {
                var delta = current[i] - previous[i];
                var sign = delta >= 0 ? "+" : "";
                var deltaText = System.Math.Abs(delta) > 0.05
                    ? $" ({sign}{delta.ToString("F1", CultureInfo.InvariantCulture)}°)"
                    : "";
                lines.Append($"J{i + 1}:{current[i].ToString("F1", CultureInfo.InvariantCulture)}°{deltaText}");
                if (i == 2) lines.Append("\n");
                else if (i < 5) lines.Append(" ");
            }

            return lines.ToString();
        }

        private void OnErrorReceived(FairinoResult result)
        {
            if (errorLabel != null)
            {
                errorLabel.text = result.Message;
            }
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
