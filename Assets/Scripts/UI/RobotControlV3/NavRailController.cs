// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Desktop NavRail의 최소 선택 상태를 유지합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class NavRailController : MonoBehaviour
    {
        private UIDocument document;
        private readonly List<Button> navButtons = new();

        private void OnEnable()
        {
            document ??= GetComponent<UIDocument>();
            var root = document?.rootVisualElement;
            if (root == null)
            {
                return;
            }

            navButtons.Clear();
            AddButton(root, "NavHome");
            AddButton(root, "NavMotion");
            AddButton(root, "NavPoints");
            AddButton(root, "NavIo");
            AddButton(root, "NavStatus");
            AddButton(root, "NavHelp");

            for (var index = 0; index < navButtons.Count; index++)
            {
                var button = navButtons[index];
                var capturedIndex = index;
                button.clicked += () => SetActive(capturedIndex);
            }

            SetActive(1);
        }

        private void AddButton(VisualElement root, string name)
        {
            var button = root.Q<Button>(name);
            if (button != null)
            {
                navButtons.Add(button);
            }
        }

        private void SetActive(int activeIndex)
        {
            for (var index = 0; index < navButtons.Count; index++)
            {
                navButtons[index].EnableInClassList("rc-nav-item--active", index == activeIndex);
            }
        }
    }
}
