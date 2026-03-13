// Folder: App - application orchestration and runtime state.
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.App
{
    internal sealed class SandboxActionFlowService
    {
        public void ApplyZeroPose(AppController appController)
        {
            ApplyPoseArray(appController, GetMetadata(appController).ZeroPoseDeg);
        }

        public void ApplyHomePose(AppController appController)
        {
            ApplyPoseArray(appController, GetMetadata(appController).HomePoseDeg);
        }

        public void ApplyDemoPose(AppController appController)
        {
            ApplyPoseArray(appController, GetMetadata(appController).DemoPoseDeg);
        }

        public void ResetPose(AppController appController)
        {
            ApplyZeroPose(appController);
        }

        public void BackHome()
        {
            SceneNavigator.Load(SceneId.Home);
        }

        public void OpenRobotLibrary()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        public void ChangeRobot(string robotId)
        {
            if (string.IsNullOrWhiteSpace(robotId))
            {
                return;
            }

            RobotSelectionBridge.SetSelection(robotId, RobotSelectionBridge.SandboxMode);
            SceneNavigator.Load(SceneId.Sandbox);
        }

        private static void ApplyPoseArray(AppController appController, double[] pose)
        {
            if (appController == null || pose == null)
            {
                return;
            }

            for (var i = 0; i < pose.Length; i++)
            {
                appController.SetJointAngleDegrees(i, (float)pose[i]);
            }
        }

        private static RobotMetadataInfo GetMetadata(AppController appController)
        {
            if (appController == null || appController.CurrentTemplate == null)
            {
                return new RobotMetadataInfo("2DOF_RR", "2DOF RR", 2, "RR", "Easy");
            }

            return RobotCatalog.TryGet(appController.CurrentTemplate.Name, out var entry)
                ? entry.Metadata
                : new RobotMetadataInfo("2DOF_RR", "2DOF RR", 2, "RR", "Easy");
        }
    }
}
