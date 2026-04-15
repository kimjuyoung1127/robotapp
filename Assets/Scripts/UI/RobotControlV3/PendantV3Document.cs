// Folder: UI/RobotControlV3 - UIDocument bootstrap for Pendant V3 shell
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Owns the minimal Phase 0A UIDocument wiring for the V3 shell.
    /// Visual tree content is attached in later phases.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(UIDocument))]
    public sealed class PendantV3Document : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private PanelSettings panelSettings;
        [SerializeField] private VisualTreeAsset rootVisualTree;

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();

            if (document == null)
            {
                Debug.LogError("[PendantV3Document] UIDocument is required.");
                enabled = false;
                return;
            }

            if (panelSettings != null)
            {
                document.panelSettings = panelSettings;
            }

            if (rootVisualTree != null)
            {
                document.visualTreeAsset = rootVisualTree;
            }
        }

        internal bool IsReadyForSceneBootstrap()
        {
            document ??= GetComponent<UIDocument>();
            return document != null && document.rootVisualElement != null;
        }
    }
}
