// Folder: App - application orchestration and runtime state.
using System.Linq;
using UnityEngine;

namespace KineTutor3D.App
{
    internal readonly struct SnapshotLiteStatus
    {
        public SnapshotLiteStatus(string message, SandboxSnapshotData snapshot = null)
        {
            Message = message;
            Snapshot = snapshot;
        }

        public string Message { get; }
        public SandboxSnapshotData Snapshot { get; }
    }

    internal sealed class SnapshotLiteFlowService
    {
        public SnapshotLiteStatus SaveSnapshot(AppController appController)
        {
            if (appController == null || appController.CurrentTemplate == null)
            {
                return new SnapshotLiteStatus(string.Empty);
            }

            var snapshot = SandboxSnapshotStore.SaveSnapshot(
                appController.CurrentTemplate.Name,
                appController.IsSandboxMode ? RobotSelectionBridge.SandboxMode : RobotSelectionBridge.GuidedLessonMode,
                appController.CurrentJointValuesRad.Select(x => x * Mathf.Rad2Deg).Cast<double>().ToArray(),
                appController.CurrentEndEffectorPose,
                $"{appController.CurrentTemplate.Name} snapshot");

            return new SnapshotLiteStatus($"Saved: {snapshot.note}", snapshot);
        }

        public SnapshotLiteStatus LoadSnapshot(AppController appController, SandboxSnapshotData lastSnapshot)
        {
            if (appController == null || appController.CurrentTemplate == null)
            {
                return new SnapshotLiteStatus(string.Empty, lastSnapshot);
            }

            var snapshot = lastSnapshot ?? SandboxSnapshotStore.GetLatestSnapshot(appController.CurrentTemplate.Name);
            if (snapshot == null)
            {
                return new SnapshotLiteStatus("저장된 snapshot이 없습니다.");
            }

            for (var i = 0; i < snapshot.jointValuesDeg.Length; i++)
            {
                appController.SetJointAngleDegrees(i, (float)snapshot.jointValuesDeg[i]);
            }

            return new SnapshotLiteStatus($"Loaded: {snapshot.note}", snapshot);
        }

        public SnapshotLiteStatus CompareSnapshot(AppController appController, SandboxSnapshotData lastSnapshot)
        {
            if (appController == null || appController.CurrentTemplate == null)
            {
                return new SnapshotLiteStatus(string.Empty, lastSnapshot);
            }

            var snapshot = lastSnapshot ?? SandboxSnapshotStore.GetLatestSnapshot(appController.CurrentTemplate.Name);
            if (snapshot == null)
            {
                return new SnapshotLiteStatus("비교할 snapshot이 없습니다.");
            }

            var current = appController.CurrentJointValuesRad.Select(x => x * Mathf.Rad2Deg).ToArray();
            var delta = 0d;
            var count = System.Math.Min(current.Length, snapshot.jointValuesDeg.Length);
            for (var i = 0; i < count; i++)
            {
                delta += System.Math.Abs(current[i] - snapshot.jointValuesDeg[i]);
            }

            return new SnapshotLiteStatus($"Compare: total joint delta {delta:F1} deg", snapshot);
        }

        public SnapshotLiteStatus GetLatestStatus(AppController appController)
        {
            if (appController == null || appController.CurrentTemplate == null)
            {
                return new SnapshotLiteStatus(string.Empty);
            }

            var snapshot = SandboxSnapshotStore.GetLatestSnapshot(appController.CurrentTemplate.Name);
            return snapshot == null
                ? new SnapshotLiteStatus("최근 snapshot 없음")
                : new SnapshotLiteStatus($"Latest: {snapshot.note}", snapshot);
        }
    }
}
