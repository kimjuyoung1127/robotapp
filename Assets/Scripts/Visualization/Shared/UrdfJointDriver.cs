// Folder: Visualization/Shared - 로봇 무관 공용 URDF 관절 드라이버.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 임의 URDF 로봇의 revolute 관절을 Transform.localRotation으로 제어합니다.
    /// ArticulationBody 계층을 자동 탐색하여 관절 이름 하드코딩 없이 동작합니다.
    /// </summary>
    public class UrdfJointDriver : MonoBehaviour
    {
        private Transform[] joints;
        private Quaternion[] initialLocalRotations;
        private Vector3[] rotationAxes;
        private int jointCount;
        private bool initialized;

        /// <summary>관절 개수.</summary>
        public int JointCount => jointCount;

        /// <summary>초기화 완료 여부.</summary>
        public bool IsInitialized => initialized;

        /// <summary>
        /// base_link Transform을 주입하고 revolute 관절을 자동 탐색합니다.
        /// </summary>
        public void Inject(Transform baseLinkTransform, int expectedJointCount)
        {
            initialized = false;
            if (baseLinkTransform == null || expectedJointCount <= 0)
            {
                return;
            }

            DiscoverJointChain(baseLinkTransform, expectedJointCount);
            DisableArticulationBodies(baseLinkTransform);
            initialized = jointCount > 0;
        }

        /// <summary>
        /// N축 관절 각도를 도 단위로 적용합니다.
        /// </summary>
        public void ApplyJointAngles(double[] jointAnglesDeg)
        {
            if (!initialized || jointAnglesDeg == null)
            {
                return;
            }

            var count = Mathf.Min(jointAnglesDeg.Length, jointCount);
            for (var i = 0; i < count; i++)
            {
                if (joints[i] == null)
                {
                    continue;
                }

                if (double.IsNaN(jointAnglesDeg[i]) || double.IsInfinity(jointAnglesDeg[i]))
                {
                    continue;
                }

                var angleDeg = (float)jointAnglesDeg[i];
                var rot = initialLocalRotations[i] * Quaternion.AngleAxis(angleDeg, rotationAxes[i]);
                rot.Normalize();
                joints[i].localRotation = rot;
            }
        }

        /// <summary>
        /// 지정 관절의 Transform을 반환합니다. 핸들 부착용입니다.
        /// </summary>
        public Transform GetJointTransform(int index)
        {
            if (!initialized || joints == null || index < 0 || index >= jointCount)
            {
                return null;
            }

            return joints[index];
        }

        /// <summary>
        /// 지정 관절의 회전 축 벡터를 반환합니다.
        /// </summary>
        public Vector3 GetJointRotationAxis(int index)
        {
            if (!initialized || rotationAxes == null || index < 0 || index >= jointCount)
            {
                return Vector3.up;
            }

            return rotationAxes[index];
        }

        private void DiscoverJointChain(Transform root, int expected)
        {
            var allBodies = root.GetComponentsInChildren<ArticulationBody>(true);
            var revolute = new List<ArticulationBody>();

            for (var i = 0; i < allBodies.Length; i++)
            {
                var ab = allBodies[i];
                if (ab == null || ab.isRoot)
                {
                    continue;
                }

                if (ab.jointType == ArticulationJointType.RevoluteJoint)
                {
                    revolute.Add(ab);
                }
            }

            jointCount = Mathf.Min(revolute.Count, expected);
            joints = new Transform[jointCount];
            initialLocalRotations = new Quaternion[jointCount];
            rotationAxes = new Vector3[jointCount];

            for (var i = 0; i < jointCount; i++)
            {
                var ab = revolute[i];
                joints[i] = ab.transform;
                initialLocalRotations[i] = ab.transform.localRotation;
                rotationAxes[i] = (ab.anchorRotation * Vector3.right).normalized;
            }
        }

        private static void DisableArticulationBodies(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var bodies = root.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null || bodies[i].isRoot)
                {
                    continue;
                }

                bodies[i].enabled = false;
            }
        }
    }
}
