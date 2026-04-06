// Folder: App - Application controllers and services; single UnityEngine entry point.
using KineTutor3D.UI.RobotControlV3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KineTutor3D.App
{
    /// <summary>
    /// RobotControlV3 입력 계약을 `unityctl exec`로 점검하기 위한 디버그 브리지입니다.
    /// </summary>
    public static class RobotControlV3DebugBridge
    {
        public static string OpenPopupProbe()
        {
            var contract = GetInputContract();
            contract.OpenPopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static string ClosePopupProbe()
        {
            var contract = GetInputContract();
            contract.ClosePopupProbeForDebug();
            return contract.GetDebugStateSummary();
        }

        public static string GetInputContractSummary()
        {
            var contract = GetInputContract();
            return contract.GetDebugStateSummary();
        }

        private static PendantV3InputContract GetInputContract()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var contract = Object.FindFirstObjectByType<PendantV3InputContract>(FindObjectsInactive.Include);
            if (contract == null)
            {
                throw new MissingReferenceException("PendantV3InputContract not found in RobotControlV3 scene.");
            }

            return contract;
        }
    }
}
