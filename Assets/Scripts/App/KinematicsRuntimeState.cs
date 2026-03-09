// Folder: App - application orchestration and runtime state.
using System;
using KineTutor3D.Math;
using KineTutor3D.Types;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.App
{
    internal sealed class KinematicsRuntimeState
    {
        public RobotTemplate CurrentTemplate;
        public DHLink[] CurrentLinks = Array.Empty<DHLink>();
        public double[] CurrentJointValuesRad = Array.Empty<double>();
        public TutorPose CurrentEndEffectorPose;
        public Mat4D CurrentEndEffectorTransform = Mat4D.Identity;
        public Mat4D CurrentA1 = Mat4D.Identity;
        public Mat4D CurrentA2 = Mat4D.Identity;
        public Mat4D CurrentT02 = Mat4D.Identity;
    }
}
