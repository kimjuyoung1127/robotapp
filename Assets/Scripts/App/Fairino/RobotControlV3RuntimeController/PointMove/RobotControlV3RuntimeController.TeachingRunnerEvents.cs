// Folder: PointMove - waypoint runner event binding and runtime visual/snapshot updates for teaching flows.
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles waypoint runner and preset animator event binding plus feedback/current-state updates during teaching playback.
    // Recording, selected-point runtime, and function/block editing live in the other Teaching partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        private void BindWaypointRunnerEvents()
        {
            if (waypointRunner == null || presetAnimator == null || waypointRunnerEventsBound)
            {
                return;
            }

            waypointRunner.OnWaypointReached += OnWaypointRunnerReached;
            waypointRunner.OnSequenceComplete += OnWaypointRunnerComplete;
            waypointRunner.OnError += OnWaypointRunnerError;
            waypointRunner.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            presetAnimator.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            waypointRunnerEventsBound = true;
        }


        private void UnbindWaypointRunnerEvents()
        {
            if (!waypointRunnerEventsBound)
            {
                return;
            }

            if (waypointRunner != null)
            {
                waypointRunner.OnWaypointReached -= OnWaypointRunnerReached;
                waypointRunner.OnSequenceComplete -= OnWaypointRunnerComplete;
                waypointRunner.OnError -= OnWaypointRunnerError;
                waypointRunner.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            waypointRunnerEventsBound = false;
        }


        private void OnWaypointRunnerReached(int index, string pointName)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            PushFeedback($"[Teaching Loop] {index + 1}/{waypointRunner.TotalCount} {pointName} 도달");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerComplete()
        {
            PushFeedback(teachingLoopEnabled ? "[Teaching Loop] 반복 실행 정지" : "[Teaching Run] 시퀀스 완료");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerError(string message)
        {
            PushFeedback($"[Teaching Loop] {message}");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerFrameUpdated(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                return;
            }

            currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
            templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            requestStageRefocus = true;
            ApplyVisualState();
            RefreshSnapshot();
        }
    }
}
