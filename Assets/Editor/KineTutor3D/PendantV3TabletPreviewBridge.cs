// Folder: Editor - Authoring and QA utilities for Unity scenes and tools.
using KineTutor3D.UI.RobotControlV3;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.EditorTools
{
    /// <summary>
    /// Pendant V3 tablet preview 모드를 editor callable로 노출합니다.
    /// </summary>
    public static class PendantV3TabletPreviewBridge
    {
        public static string SetTabletMode()
        {
            var controller = GetController();
            controller.SetPreviewMode(PendantV3LayoutController.PreviewMode.Tablet);
            EditorUtility.SetDirty(controller);
            return controller.GetDebugSummary();
        }

        public static string SetDesktopMode()
        {
            var controller = GetController();
            controller.SetPreviewMode(PendantV3LayoutController.PreviewMode.Desktop);
            EditorUtility.SetDirty(controller);
            return controller.GetDebugSummary();
        }

        public static string SetAutoMode()
        {
            var controller = GetController();
            controller.SetPreviewMode(PendantV3LayoutController.PreviewMode.Auto);
            EditorUtility.SetDirty(controller);
            return controller.GetDebugSummary();
        }

        public static string GetSummary()
        {
            return GetController().GetDebugSummary();
        }

        private static PendantV3LayoutController GetController()
        {
            var controller = Object.FindFirstObjectByType<PendantV3LayoutController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                throw new MissingReferenceException("PendantV3LayoutController not found.");
            }

            return controller;
        }
    }
}
