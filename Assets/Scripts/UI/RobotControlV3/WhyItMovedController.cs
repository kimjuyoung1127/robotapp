// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;
using KineTutor3D.App.Fairino;

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
        private bool isInitialized;
        private bool isInitializing;
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

        internal void RefreshFromBinder(RobotControlV3RuntimeSnapshot data)
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
            if (isInitialized)
            {
                return true;
            }

            if (isInitializing)
            {
                return false;
            }

            isInitializing = true;
            document ??= GetComponent<UIDocument>();
            connectionHomeController ??= GetComponent<ConnectionHomeController>();
            root = document?.rootVisualElement;
            try
            {
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

                isInitialized = true;
                ApplyPreview(connectionHomeController.CurrentPreviewDefinition);
                return true;
            }
            finally
            {
                isInitializing = false;
            }
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

        private void ApplyPreview(RobotControlV3RuntimeSnapshot data)
        {
            whyItMovedTitle.text = "최근 조작 메모";
            whyItMovedSummary.text = connectionHomeController.CurrentPreviewState switch
            {
                PendantV3PreviewState.Kind.Fault => "Fault가 감지돼 조작을 잠깐 멈췄고, 지금은 복구 순서를 먼저 읽는 흐름으로 바뀐 상태다.",
                PendantV3PreviewState.Kind.ConnectedUnsynced => "서보는 켜졌지만 아직 동기화가 안 되어 있어서, 현재 자세 읽기가 첫 우선순위다.",
                PendantV3PreviewState.Kind.AutoReconnect => "통신이 흔들리는 동안은 자동 재연결이 먼저라서 조작보다 상태 복귀를 기다리는 흐름이다.",
                _ => data.ActionNow,
            };
        }
    }
}
