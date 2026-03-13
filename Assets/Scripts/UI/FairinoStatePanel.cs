// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FAIRINO 로봇 실시간 상태 표시 패널입니다.
    /// 관절 각도, TCP 포즈, 에러 메시지를 표시합니다.
    /// </summary>
    public class FairinoStatePanel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Text jointStateLabel;
        [SerializeField] private Text tcpPoseLabel;
        [SerializeField] private Text errorLabel;
        [SerializeField] private Font fallbackFont;

        private FairinoConnectionService connectionService;
        private FairinoErrorTranslator errorTranslator;
        private bool listenersBound;

        /// <summary>
        /// 연결 서비스와 에러 번역기를 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoErrorTranslator translator)
        {
            UnsubscribeService();
            connectionService = service;
            errorTranslator = translator ?? new FairinoErrorTranslator();
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

            var title = UiRuntimeStyle.EnsureText(root, "Title", fallbackFont, UIDesignTokens.Type.HeadingLg, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 22f), new Vector2(16f, -14f));
            title.text = "Robot State";

            jointStateLabel = UiRuntimeStyle.EnsureText(root, "JointStateLabel", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(jointStateLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 54f), new Vector2(16f, -46f));

            tcpPoseLabel = UiRuntimeStyle.EnsureText(root, "TcpPoseLabel", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(tcpPoseLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 54f), new Vector2(16f, -106f));

            errorLabel = UiRuntimeStyle.EnsureText(root, "ErrorLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentDanger);
            UiRuntimeStyle.Anchor(errorLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(320f, 48f), new Vector2(16f, 16f));

            if (!listenersBound)
            {
                jointStateLabel.text = "관절: 대기 중...";
                tcpPoseLabel.text = "TCP: 대기 중...";
                errorLabel.text = string.Empty;
            }
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
                jointStateLabel.text = $"J1:{j[0]:F1} J2:{j[1]:F1} J3:{j[2]:F1}\nJ4:{j[3]:F1} J5:{j[4]:F1} J6:{j[5]:F1}";
            }

            if (tcpPoseLabel != null)
            {
                var t = state.TcpPose;
                tcpPoseLabel.text = $"X:{t[0]:F2} Y:{t[1]:F2} Z:{t[2]:F2}\nRx:{t[3]:F2} Ry:{t[4]:F2} Rz:{t[5]:F2}";
            }

            if (errorLabel != null)
            {
                errorLabel.text = string.Empty;
            }
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
