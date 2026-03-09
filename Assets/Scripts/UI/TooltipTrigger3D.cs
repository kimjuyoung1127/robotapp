// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 3D 오브젝트의 호버/클릭을 툴팁과 게이트 이벤트로 변환합니다.
    /// </summary>
    public class TooltipTrigger3D : MonoBehaviour
    {
        [SerializeField] private string targetId = "3d-target";
        [SerializeField] private string titleKo = "용어";
        [TextArea(2, 6)] [SerializeField] private string bodyKo = "설명";

        public string TargetId => targetId;

        private void OnMouseEnter()
        {
            TooltipSystem.Instance?.ShowWorld(transform.position, Camera.main, titleKo, bodyKo);
            FindAppController()?.ReportInteraction(InteractionType.Hover, targetId);
        }

        private void OnMouseExit()
        {
            TooltipSystem.Instance?.Hide();
        }

        private void OnMouseDown()
        {
            FindAppController()?.ReportInteraction(InteractionType.Click, targetId);
        }

        private static AppController FindAppController()
        {
            return FindAnyObjectByType<AppController>();
        }
    }
}

