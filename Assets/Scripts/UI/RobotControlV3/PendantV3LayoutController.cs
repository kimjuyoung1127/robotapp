// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3 루트에 desktop/tablet 클래스 상태를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PendantV3LayoutController : MonoBehaviour
    {
        public enum PreviewMode
        {
            Auto = 0,
            Desktop = 1,
            Tablet = 2,
        }

        [SerializeField] private PreviewMode previewMode = PreviewMode.Desktop;
        [SerializeField] private float tabletBreakpoint = 1366f;

        private UIDocument document;
        private VisualElement root;

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();
            root = document?.rootVisualElement;
            if (root == null)
            {
                return;
            }

            root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ApplyLayoutClass(root.resolvedStyle.width);
        }

        private void OnDisable()
        {
            root?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void SetPreviewMode(PreviewMode mode)
        {
            previewMode = mode;
            ApplyLayoutClass(root?.resolvedStyle.width ?? 0f);
        }

        public string GetDebugSummary()
        {
            var modeName = previewMode.ToString();
            var width = root?.resolvedStyle.width ?? 0f;
            var isTablet = root != null && root.ClassListContains("rc-root--tablet");
            return $"mode={modeName}; width={width:F1}; tablet={isTablet}";
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyLayoutClass(evt.newRect.width);
        }

        private void ApplyLayoutClass(float width)
        {
            if (root == null)
            {
                return;
            }

            var useTablet = previewMode switch
            {
                PreviewMode.Tablet => true,
                PreviewMode.Desktop => false,
                _ => width > 0f && width <= tabletBreakpoint,
            };

            root.EnableInClassList("rc-root--desktop", !useTablet);
            root.EnableInClassList("rc-root--tablet", useTablet);
        }
    }
}
