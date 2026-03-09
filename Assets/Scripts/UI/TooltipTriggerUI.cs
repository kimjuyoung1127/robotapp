// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KineTutor3D.UI
{
    /// <summary>
    /// UI 요소의 호버/클릭을 툴팁과 게이트 이벤트로 변환합니다.
    /// </summary>
    public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private string targetId = "ui-target";
        [SerializeField] private string titleKo = "용어";
        [TextArea(2, 6)] [SerializeField] private string bodyKo = "설명";
        [SerializeField] private bool clickOnly;
        [SerializeField] private bool reportHover = true;
        [SerializeField] private bool reportClick = true;
        [SerializeField] private InteractionType hoverInteractionType = InteractionType.Hover;
        [SerializeField] private InteractionType clickInteractionType = InteractionType.Click;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (clickOnly)
            {
                return;
            }

            TooltipSystem.Instance?.ShowScreen(GetMousePositionSafe(), titleKo, bodyKo);
            if (reportHover)
            {
                FindAppController()?.ReportInteraction(hoverInteractionType, targetId);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!clickOnly)
            {
                TooltipSystem.Instance?.Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TooltipSystem.Instance?.ShowScreen(GetMousePositionSafe(), titleKo, bodyKo);
            if (reportClick)
            {
                FindAppController()?.ReportInteraction(clickInteractionType, targetId);
            }
        }

        private static AppController FindAppController()
        {
            return FindAnyObjectByType<AppController>();
        }

        private static Vector2 GetMousePositionSafe()
        {
            try
            {
                return Input.mousePosition;
            }
            catch (InvalidOperationException)
            {
                return Vector2.zero;
            }
        }
    }
}

