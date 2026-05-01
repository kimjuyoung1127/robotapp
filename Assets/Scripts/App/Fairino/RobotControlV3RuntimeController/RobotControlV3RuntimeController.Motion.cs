// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        public void ToggleDryRun()
        {
            snapshot.DryRunEnabled = !snapshot.DryRunEnabled;
            InvalidateLiveApprovalContext();
            PushFeedback(snapshot.DryRunEnabled ? "[DryRun] ON" : "[DryRun] OFF");
            RefreshSnapshot();
        }

        public void SetCoordSystem(string coordSystem)
        {
            snapshot.CoordSystem = coordSystem is "Tool" or "User" ? coordSystem : "Base";
            RefreshSnapshot();
        }

    }
}
