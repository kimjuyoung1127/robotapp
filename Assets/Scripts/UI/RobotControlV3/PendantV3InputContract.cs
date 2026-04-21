// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Pendant V3의 기본 입력 경계와 포커스 순서를 고정합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PendantV3InputContract : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement root;

        private VisualElement topStatusBar;
        private VisualElement navRail;
        private VisualElement workTabBar;
        private VisualElement workPanel;
        private VisualElement viewportHost;
        private VisualElement contextPanel;
        private VisualElement bottomBar;
        private VisualElement popupLayer;
        private VisualElement popupCard;
        private Button popupProbeButton;
        private Button popupCancelButton;
        private Button popupConfirmButton;
        private VisualElement lastFocusedElement;

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();
            if (document == null)
            {
                return;
            }

            root = document.rootVisualElement;
            if (root == null)
            {
                return;
            }

            topStatusBar = root.Q<VisualElement>("TopStatusBar");
            navRail = root.Q<VisualElement>("NavRail");
            workTabBar = root.Q<VisualElement>("WorkTabBar");
            workPanel = root.Q<VisualElement>("WorkPanel");
            viewportHost = root.Q<VisualElement>("RobotStageHost");
            contextPanel = root.Q<VisualElement>("ContextPanel");
            bottomBar = root.Q<VisualElement>("BottomBar");
            popupLayer = root.Q<VisualElement>("PopupLayer");
            popupCard = root.Q<VisualElement>("PopupCard");
            popupProbeButton = root.Q<Button>("BtnPopupProbe");
            popupCancelButton = root.Q<Button>("BtnPopupCancel");
            popupConfirmButton = root.Q<Button>("BtnPopupConfirm");

            ConfigureRegion(topStatusBar, 0);
            ConfigureRegion(navRail, 1);
            ConfigureRegion(workTabBar, 2);
            ConfigureRegion(workPanel, 3);
            ConfigureViewport(viewportHost);
            ConfigureRegion(contextPanel, 4);
            ConfigureRegion(bottomBar, 5);
            ConfigurePopupLayer(popupLayer, 6);
            ConfigurePopupCard(popupCard);
            BindPopupButtons();
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            root.RegisterCallback<FocusInEvent>(OnFocusIn);

            root.schedule.Execute(() =>
            {
                if (popupProbeButton != null)
                {
                    popupProbeButton.Focus();
                }
            }).StartingIn(0);
        }

        private void OnDisable()
        {
            UnconfigureRegion(topStatusBar);
            UnconfigureRegion(navRail);
            UnconfigureRegion(workTabBar);
            UnconfigureRegion(workPanel);
            UnconfigureViewport(viewportHost);
            UnconfigureRegion(contextPanel);
            UnconfigureRegion(bottomBar);
            UnconfigurePopupLayer(popupLayer);
            UnbindPopupButtons();

            if (root != null)
            {
                root.UnregisterCallback<KeyDownEvent>(OnKeyDown);
                root.UnregisterCallback<FocusInEvent>(OnFocusIn);
            }
        }

        private static void ConfigureRegion(VisualElement element, int tabIndex)
        {
            if (element == null)
            {
                return;
            }

            element.focusable = true;
            element.tabIndex = tabIndex;
            element.pickingMode = PickingMode.Position;
            element.RegisterCallback<PointerDownEvent>(OnUiPointerDown);
        }

        private static void UnconfigureRegion(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.UnregisterCallback<PointerDownEvent>(OnUiPointerDown);
        }

        private static void ConfigureViewport(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.focusable = false;
            element.tabIndex = -1;
            element.pickingMode = PickingMode.Position;
        }

        private static void UnconfigureViewport(VisualElement element)
        {
            if (element == null)
            {
                return;
            }
        }

        private static void ConfigurePopupLayer(VisualElement element, int tabIndex)
        {
            if (element == null)
            {
                return;
            }

            element.focusable = true;
            element.tabIndex = tabIndex;
            element.pickingMode = PickingMode.Ignore;
            element.RegisterCallback<PointerDownEvent>(OnUiPointerDown);
        }

        private static void UnconfigurePopupLayer(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.UnregisterCallback<PointerDownEvent>(OnUiPointerDown);
        }

        private static void ConfigurePopupCard(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.focusable = true;
            element.tabIndex = 0;
            element.RegisterCallback<PointerDownEvent>(OnUiPointerDown);
        }

        private void BindPopupButtons()
        {
            if (popupProbeButton != null)
            {
                popupProbeButton.clicked += OpenPopupProbe;
            }

            if (popupCancelButton != null)
            {
                popupCancelButton.clicked += ClosePopupProbe;
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.clicked += ClosePopupProbe;
            }
        }

        private void UnbindPopupButtons()
        {
            if (popupProbeButton != null)
            {
                popupProbeButton.clicked -= OpenPopupProbe;
            }

            if (popupCancelButton != null)
            {
                popupCancelButton.clicked -= ClosePopupProbe;
            }

            if (popupConfirmButton != null)
            {
                popupConfirmButton.clicked -= ClosePopupProbe;
            }
        }

        private void OpenPopupProbe()
        {
            if (popupLayer == null || popupCard == null)
            {
                return;
            }

            lastFocusedElement = root?.panel?.focusController?.focusedElement as VisualElement;
            popupLayer.EnableInClassList("rc-popup-layer--active", true);
            popupCard.EnableInClassList("rc-hidden", false);
            popupLayer.pickingMode = PickingMode.Position;
            popupConfirmButton?.Focus();
        }

        private void ClosePopupProbe()
        {
            if (popupLayer == null || popupCard == null)
            {
                return;
            }

            popupLayer.EnableInClassList("rc-popup-layer--active", false);
            popupCard.EnableInClassList("rc-hidden", true);
            popupLayer.pickingMode = PickingMode.Ignore;

            if (lastFocusedElement != null && lastFocusedElement.panel != null)
            {
                lastFocusedElement.Focus();
                return;
            }

            popupProbeButton?.Focus();
        }

        public void OpenPopupProbeForDebug()
        {
            OpenPopupProbe();
        }

        public void ClosePopupProbeForDebug()
        {
            ClosePopupProbe();
        }

        public string GetDebugStateSummary()
        {
            var focused = root?.panel?.focusController?.focusedElement as VisualElement;
            var focusedName = focused?.name ?? "null";
            var lastFocusedName = lastFocusedElement?.name ?? "null";
            return $"popupActive={IsPopupActive()}; focused={focusedName}; lastFocused={lastFocusedName}";
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!IsPopupActive())
            {
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopImmediatePropagation();
                ClosePopupProbe();
                return;
            }

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopImmediatePropagation();
                ClosePopupProbe();
                return;
            }

            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopImmediatePropagation();
                FocusNextPopupButton((evt.modifiers & EventModifiers.Shift) != 0);
            }
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (!IsPopupActive() || popupCard == null)
            {
                return;
            }

            if (evt.target is not VisualElement target)
            {
                return;
            }

            if (target == popupCard || IsDescendantOf(target, popupCard))
            {
                return;
            }

            evt.StopImmediatePropagation();
            FocusNextPopupButton(reverse: false);
        }

        private void FocusNextPopupButton(bool reverse)
        {
            if (popupCancelButton == null || popupConfirmButton == null)
            {
                popupCard?.Focus();
                return;
            }

            var focused = root?.panel?.focusController?.focusedElement as VisualElement;
            if (reverse)
            {
                if (focused == popupConfirmButton)
                {
                    popupCancelButton.Focus();
                    return;
                }

                popupConfirmButton.Focus();
                return;
            }

            if (focused == popupCancelButton)
            {
                popupConfirmButton.Focus();
                return;
            }

            popupCancelButton.Focus();
        }

        private bool IsPopupActive()
        {
            return popupCard != null && !popupCard.ClassListContains("rc-hidden");
        }

        private static bool IsDescendantOf(VisualElement child, VisualElement parent)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void OnUiPointerDown(PointerDownEvent evt)
        {
            evt.StopPropagation();
        }
    }
}
