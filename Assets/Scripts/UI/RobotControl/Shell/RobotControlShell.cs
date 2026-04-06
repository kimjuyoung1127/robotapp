// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// RobotControlV2 authored shell의 루트 구조를 보장하고 binder를 통해 하위 계층을 연결합니다.
    /// </summary>
    public sealed class RobotControlShell : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Font fallbackFont;
        [SerializeField] private TopStatusBar topStatusBar;
        [SerializeField] private RobotControlShellBinder binder;
        [SerializeField] private RobotControlAuthoredLayoutLock authoredLayoutLock;

        public TopStatusBar TopStatusBar => topStatusBar;

        public RobotControlShellBinder Binder => binder;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetFallbackFont(Font font)
        {
            fallbackFont = font;
            EnsurePresentation();
        }

        public void ApplyModeText(string modeText)
        {
            EnsurePresentation();
            topStatusBar?.SetModeText(modeText);
        }

        public void ApplyTitleText(string titleText)
        {
            EnsurePresentation();
            topStatusBar?.SetTitleText(titleText);
        }

        public void Bind(RobotControlViewState state)
        {
            EnsurePresentation();
            binder?.Bind(state);
            topStatusBar = binder != null ? binder.TopStatusBar : topStatusBar;
            ApplyUniformTextSize();
        }

        public void ApplyLayoutMode(bool tabletLayout)
        {
            EnsurePresentation();
            binder?.ApplyLayoutMode(tabletLayout);
            ApplyUniformTextSize();
        }

        public void SetPopupCopy(string moveConfirmBody, string warningBody, string recoveryBody)
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            SetPopupBody(root, "MoveConfirmDialog", moveConfirmBody);
            SetPopupBody(root, "WarningDialog", warningBody);
            SetPopupBody(root, "RecoveryDialog", recoveryBody);
        }

        public void SetPopupBody(string popupName, string body)
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            SetPopupBody(root, popupName, body);
        }

        public void ShowPopup(string popupName)
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            HideAllPopups();
            SetPopupVisible(root, popupName, true);
        }

        public void HidePopup(string popupName)
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            SetPopupVisible(root, popupName, false);
        }

        public void HideAllPopups()
        {
            if (transform is not RectTransform root)
            {
                return;
            }

            SetPopupVisible(root, "MoveConfirmDialog", false);
            SetPopupVisible(root, "WarningDialog", false);
            SetPopupVisible(root, "RecoveryDialog", false);
            SetPopupVisible(root, "FirstRunGuideDialog", false);
        }

        public Button FindButton(string relativePath)
        {
            return transform.Find(relativePath)?.GetComponent<Button>();
        }

        public static RobotControlShell EnsureV2Shell(Canvas canvas, Font fallbackFont, string titleText, string modeText)
        {
            if (canvas == null)
            {
                return null;
            }

            var root = canvas.transform as RectTransform;
            if (root == null)
            {
                return null;
            }

            var shellRoot = root.Find("RobotControlShell") as RectTransform ?? UiRuntimeStyle.EnsureRectChild(root, "RobotControlShell");
            RemoveLegacyChildren(shellRoot);

            var shell = shellRoot.GetComponent<RobotControlShell>() ?? shellRoot.gameObject.AddComponent<RobotControlShell>();
            shell.SetFallbackFont(fallbackFont);
            shell.ApplyTitleText(titleText);
            shell.ApplyModeText(modeText);
            shell.TopStatusBar?.SetConnectionStateText("V2 구조 준비 중");
            shell.TopStatusBar?.SetSpeedText("속도 --");
            return shell;
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);

            if (transform is not RectTransform root)
            {
                return;
            }

            UiRuntimeStyle.Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = UiRuntimeStyle.EnsureImage(root, "Backdrop", UIDesignTokens.RobotControlV2.Colors.Backdrop);
            UiRuntimeStyle.Stretch((RectTransform)backdrop.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            authoredLayoutLock = GetComponent<RobotControlAuthoredLayoutLock>() ?? gameObject.AddComponent<RobotControlAuthoredLayoutLock>();
            authoredLayoutLock.CaptureIfNeeded(root);

            binder = GetComponent<RobotControlShellBinder>() ?? gameObject.AddComponent<RobotControlShellBinder>();
            binder.EnsureShellStructure();
            authoredLayoutLock.RestoreCapturedLayout(root);
            topStatusBar = binder.TopStatusBar;
            if (topStatusBar != null)
            {
                topStatusBar.SetFallbackFont(fallbackFont);
            }

            ApplyUniformTextSize();
        }

        private void ApplyUniformTextSize()
        {
            UiRuntimeStyle.ForceTextHierarchySize(transform, UIDesignTokens.RobotControlV2.Type.UniformText);
        }

        private static void SetPopupBody(RectTransform root, string popupName, string body)
        {
            var popup = root.Find($"SafeArea/Popups/{popupName}/Body");
            var text = popup?.GetComponent<Text>();
            if (text != null)
            {
                text.text = body;
            }
        }

        private static void SetPopupVisible(RectTransform root, string popupName, bool visible)
        {
            var popup = root.Find($"SafeArea/Popups/{popupName}");
            if (popup != null)
            {
                popup.gameObject.SetActive(visible);
            }
        }

        private static void RemoveLegacyChildren(RectTransform shellRoot)
        {
            RemoveLegacyChild(shellRoot, "ConnectionPanel");
            RemoveLegacyChild(shellRoot, "JointControlPanel");
            RemoveLegacyChild(shellRoot, "StatePanel");
            RemoveLegacyChild(shellRoot, "TcpControlPanel");
            RemoveLegacyChild(shellRoot, "DiagnosticsDrawer");
            RemoveLegacyChild(shellRoot, "TopBar");
            RemoveLegacyChild(shellRoot, "TabBar");
            RemoveLegacyChild(shellRoot, "RobotControlOverlay");
            RemoveLegacyChild(shellRoot, "WhyItMovedLabel");
            RemoveLegacyChild(shellRoot, "MoveConfirmDialog");
        }

        private static void RemoveLegacyChild(RectTransform shellRoot, string childName)
        {
            var child = shellRoot.Find(childName);
            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
