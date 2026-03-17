// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// RobotControl에서 사용하는 프리셋 포즈 공급자입니다.
    /// </summary>
    internal sealed class RobotControlPosePresetProvider
    {
        public RobotControlPosePresetProvider(Func<double[]> readyPoseFactory, Action<double[]> updateCurrentPose)
        {
            ReadyPoseFactory = readyPoseFactory ?? throw new ArgumentNullException(nameof(readyPoseFactory));
            UpdateCurrentPose = updateCurrentPose ?? throw new ArgumentNullException(nameof(updateCurrentPose));
        }

        public Func<double[]> ReadyPoseFactory { get; }

        public Action<double[]> UpdateCurrentPose { get; }

        public double[] GetReadyJointAnglesDeg()
        {
            return ReadyPoseFactory();
        }

        public void UpdateCurrent(double[] jointAnglesDeg)
        {
            UpdateCurrentPose(jointAnglesDeg);
        }
    }
}
