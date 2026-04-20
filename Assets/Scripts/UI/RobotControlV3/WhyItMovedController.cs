// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 우측 WhyItMoved 카드 문구를 전담합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(ConnectionHomeController))]
    public sealed class WhyItMovedController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private VisualElement root;
        private VisualElement whyItMovedCard;
        private Label whyItMovedTitle;
        private Label whyItMovedSummary;
        private ConnectionHomeController connectionHomeController;
        private bool isContextVisible;
        private bool isInitialized;
        private Coroutine initializeCoroutine;

        private void OnEnable()
        {
            TryInitialize();
            initializeCoroutine ??= StartCoroutine(WaitForInitialize());
        }

        private void OnDisable()
        {
            if (initializeCoroutine != null)
            {
                StopCoroutine(initializeCoroutine);
                initializeCoroutine = null;
            }

            isInitialized = false;
        }

        public bool ForceInitialize()
        {
            return TryInitialize();
        }

        public void SetContextVisible(bool visible)
        {
            isContextVisible = visible;
            if (isInitialized)
            {
                ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            }
        }

        internal void RefreshFromBinder(PendantV3PreviewState.Definition data)
        {
            if (!isInitialized && !TryInitialize())
            {
                return;
            }

            ApplyPreview(data);
        }

        public string GetDebugSummary()
        {
            var hidden = whyItMovedCard?.ClassListContains("rc-hidden") ?? true;
            var title = whyItMovedTitle?.text ?? "missing";
            return $"initialized={isInitialized}; hidden={hidden}; title={title}";
        }

        private bool TryInitialize()
        {
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            if (root == null || connectionHomeController == null)
            {
                return false;
            }

            whyItMovedCard = root.Q<VisualElement>("WhyItMoved");
            whyItMovedTitle = root.Q<Label>("WhyItMovedTitle");
            whyItMovedSummary = root.Q<Label>("WhyItMovedSummary");
            if (whyItMovedCard == null || whyItMovedTitle == null || whyItMovedSummary == null)
            {
                return false;
            }

            ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
            isInitialized = true;
            return true;
        }

        private System.Collections.IEnumerator WaitForInitialize()
        {
            for (var frame = 0; frame < 30 && !isInitialized; frame++)
            {
                TryInitialize();
                if (isInitialized)
                {
                    break;
                }

                yield return null;
            }

            initializeCoroutine = null;
        }

        private void ApplyPreview(PendantV3PreviewState.Definition data)
        {
            whyItMovedTitle.text = "최근 조작 메모";
            whyItMovedSummary.text = connectionHomeController.CurrentPreviewState switch
            {
                PendantV3PreviewState.Kind.Fault => "Fault가 감지돼 조작을 잠깐 멈췄고, 지금은 복구 순서를 먼저 읽는 흐름으로 바뀐 상태다.",
                PendantV3PreviewState.Kind.ConnectedUnsynced => "서보는 켜졌지만 아직 동기화가 안 되어 있어서, 현재 자세 읽기가 첫 우선순위다.",
                PendantV3PreviewState.Kind.AutoReconnect => "통신이 흔들리는 동안은 자동 재연결이 먼저라서 조작보다 상태 복귀를 기다리는 흐름이다.",
                _ => data.ActionNow,
            };
            whyItMovedCard.EnableInClassList("rc-hidden", !isContextVisible);
        }
    }
}
