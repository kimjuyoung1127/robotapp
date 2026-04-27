// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    /// <summary>
    /// Desktop NavRail 버튼 참조를 캐시하고 활성 클래스를 적용합니다.
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
            AddButton(root, "NavStatus");
            AddButton(root, "NavHelp");
        }

        private void OnDisable()
        {
            navButtons.Clear();
        }

        private void AddButton(VisualElement root, string name)
        {
            var button = root.Q<Button>(name);
            if (button != null)
            {
                navButtons.Add(button);
            }
        }

        public void SetActive(string activeButtonName)
        {
            foreach (var button in navButtons)
            {
                button.EnableInClassList("rc-nav-item--active", button.name == activeButtonName);
            }
        }
    }
}
