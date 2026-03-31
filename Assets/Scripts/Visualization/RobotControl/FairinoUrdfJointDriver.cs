// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// FR5 URDF 모델의 6개 관절을 Transform.localRotation으로 직접 제어합니다.
    /// ArticulationBody를 비활성화하고, 캐싱된 초기 회전과 축을 기반으로 회전합니다.
    /// </summary>
    public class FairinoUrdfJointDriver : MonoBehaviour
    {
        private static readonly string[] JointNames =
        {
            "shoulder_link", "upperarm_link", "forearm_link",
            "wrist1_link", "wrist2_link", "wrist3_link"
        };

        private Transform[] joints;
        private Quaternion[] initialLocalRotations;
        private Vector3[] rotationAxes;
        private Transform baseLink;
        private bool initialized;

        /// <summary>
        /// base_link 트랜스폼을 주입하고 관절을 캐싱합니다.
        /// </summary>
        public void Inject(Transform baseLinkTransform)
        {
            baseLink = baseLinkTransform;
            CacheJoints();
            DisableArticulationBodies();
        }

        /// <summary>
        /// 6축 관절 각도를 도 단위로 적용합니다.
        /// </summary>
        public void ApplyJointAngles(double[] jointAnglesDeg)
        {
            if (!initialized || jointAnglesDeg == null || jointAnglesDeg.Length < 6)
            {
                return;
            }

            for (var i = 0; i < 6; i++)
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
            if (!initialized || joints == null || index < 0 || index >= joints.Length)
            {
                return null;
            }

            return joints[index];
        }

        /// <summary>
        /// 지정 관절의 회전 축 벡터를 반환합니다. 핸들 회전 기준용입니다.
        /// </summary>
        public Vector3 GetJointRotationAxis(int index)
        {
            if (!initialized || rotationAxes == null || index < 0 || index >= rotationAxes.Length)
            {
                return Vector3.up;
            }

            return rotationAxes[index];
        }

        private void CacheJoints()
        {
            joints = new Transform[6];
            initialLocalRotations = new Quaternion[6];
            rotationAxes = new Vector3[6];
            initialized = false;

            if (baseLink == null)
            {
                return;
            }

            var current = baseLink;
            for (var i = 0; i < JointNames.Length; i++)
            {
                var child = FindChildRecursive(current, JointNames[i]);
                if (child == null)
                {
                    continue;
                }

                joints[i] = child;
                initialLocalRotations[i] = child.localRotation;

                // AB의 anchorRotation에서 revolute 회전축 계산
                // Unity AB revolute는 anchor 프레임의 X축 기준 회전
                var ab = child.GetComponent<ArticulationBody>();
                if (ab != null)
                {
                    rotationAxes[i] = (ab.anchorRotation * Vector3.right).normalized;
                    Debug.Log($"[JointDriver] J{i} '{JointNames[i]}' axis={rotationAxes[i]} anchorRot={ab.anchorRotation.eulerAngles} initRot={child.localRotation.eulerAngles}");
                }
                else
                {
                    // URDF revolute axis=(0,0,1) 기본 폴백
                    rotationAxes[i] = Vector3.forward;
                    Debug.Log($"[JointDriver] J{i} '{JointNames[i]}' axis=forward(fallback) initRot={child.localRotation.eulerAngles}");
                }

                current = child;
            }

            initialized = true;
        }

        /// <summary>
        /// ArticulationBody가 FixedUpdate에서 Transform을 덮어쓰지 못하도록
        /// non-root ArticulationBody를 비활성화합니다.
        /// </summary>
        private void DisableArticulationBodies()
        {
            if (baseLink == null)
            {
                return;
            }

            var bodies = baseLink.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null || bodies[i].isRoot)
                {
                    continue;
                }

                bodies[i].enabled = false;
            }
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            var direct = parent.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
