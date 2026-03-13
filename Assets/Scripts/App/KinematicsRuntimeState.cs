// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Math;
using KineTutor3D.Types;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.App
{
    /// <summary>
    /// 기구학 런타임 상태를 보관하는 내부 가변 상태 객체입니다.
    /// KinematicsRuntimeService만 쓰기 접근해야 합니다.
    /// </summary>
    internal sealed class KinematicsRuntimeState
    {
        public RobotTemplate CurrentTemplate { get; set; }
        public DHLink[] CurrentLinks { get; set; } = Array.Empty<DHLink>();
        public double[] CurrentJointValuesRad { get; set; } = Array.Empty<double>();
        public double[] PreviousJointValuesRad { get; set; } = Array.Empty<double>();
        public TutorPose CurrentEndEffectorPose { get; set; }
        public TutorPose PreviousEndEffectorPose { get; set; }
        public Vec3D PreviousEndEffectorPosition { get; set; } = Vec3D.Zero;
        public Mat4D CurrentEndEffectorTransform { get; set; } = Mat4D.Identity;
        public Mat4D PreviousEndEffectorTransform { get; set; } = Mat4D.Identity;
        public Mat4D CurrentA1 { get; set; } = Mat4D.Identity;
        public Mat4D CurrentA2 { get; set; } = Mat4D.Identity;
        public Mat4D CurrentT02 { get; set; } = Mat4D.Identity;
        public int ChangedJointIndex { get; set; } = -1;
        public RuntimeUpdateCause LastUpdateCause { get; set; } = RuntimeUpdateCause.None;
    }
}
