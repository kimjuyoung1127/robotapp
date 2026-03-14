// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.Math;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// Converts robotics-space math types to Unity-space runtime types.
    /// </summary>
    public static class CoordConverter
    {
        /// <summary>
        /// Converts a robotics-space position to a Unity-space position.
        /// </summary>
        public static Vector3 ToUnityPosition(Vec3D roboticsPosition)
        {
            return new Vector3(
                (float)roboticsPosition.X,
                (float)roboticsPosition.Z,
                (float)roboticsPosition.Y);
        }

        /// <summary>
        /// Converts a robotics-space direction to a Unity-space direction.
        /// </summary>
        public static Vector3 ToUnityDirection(Vec3D roboticsDirection)
        {
            return new Vector3(
                (float)roboticsDirection.X,
                (float)roboticsDirection.Z,
                (float)roboticsDirection.Y);
        }

        /// <summary>
        /// Converts a Unity-space position to a robotics-space position.
        /// </summary>
        public static Vec3D FromUnityPosition(Vector3 unityPosition)
        {
            return new Vec3D(unityPosition.x, unityPosition.z, unityPosition.y);
        }

        /// <summary>
        /// Converts a robotics-space rotation matrix to a Unity quaternion.
        /// </summary>
        public static Quaternion ToUnityRotation(Mat3D roboticsRotation)
        {
            var roboticsXAxis = new Vec3D(roboticsRotation[0, 0], roboticsRotation[1, 0], roboticsRotation[2, 0]);
            var roboticsYAxis = new Vec3D(roboticsRotation[0, 1], roboticsRotation[1, 1], roboticsRotation[2, 1]);
            var roboticsZAxis = new Vec3D(roboticsRotation[0, 2], roboticsRotation[1, 2], roboticsRotation[2, 2]);

            var unityRight = ToUnityDirection(roboticsXAxis).normalized;
            var unityForward = ToUnityDirection(roboticsYAxis).normalized;
            var unityUp = ToUnityDirection(roboticsZAxis).normalized;

            if (unityForward.sqrMagnitude < 1e-8f || unityUp.sqrMagnitude < 1e-8f)
            {
                return Quaternion.identity;
            }

            var rotation = Quaternion.LookRotation(unityForward, unityUp);
            var rotatedRight = rotation * Vector3.right;
            if (Vector3.Dot(rotatedRight, unityRight) < 0.999f)
            {
                rotation = Quaternion.LookRotation(unityForward, unityUp) * Quaternion.AngleAxis(180f, Vector3.forward);
            }

            return rotation;
        }

        /// <summary>
        /// Converts a robotics transform matrix to a Unity matrix.
        /// </summary>
        public static Matrix4x4 ToUnityMatrix(Mat4D roboticsTransform)
        {
            return Matrix4x4.TRS(
                ToUnityPosition(roboticsTransform.ExtractPosition()),
                ToUnityRotation(roboticsTransform.ExtractRotation()),
                Vector3.one);
        }

        /// <summary>
        /// Applies a robotics-space transform to a Unity transform in local space.
        /// </summary>
        public static void ApplyLocalTransform(Transform target, Mat4D roboticsTransform)
        {
            if (target == null)
            {
                return;
            }

            target.localPosition = ToUnityPosition(roboticsTransform.ExtractPosition());
            target.localRotation = ToUnityRotation(roboticsTransform.ExtractRotation());
            target.localScale = Vector3.one;
        }
    }
}

