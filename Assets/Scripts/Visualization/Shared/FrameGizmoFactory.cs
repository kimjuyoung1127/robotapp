// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using KineTutor3D.Math;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// FR5 6관절 좌표 프레임 기즈모를 생성하고 관리합니다.
    /// 기존 FrameGizmo, RobotRigBinder, CoordConverter를 재활용합니다.
    /// </summary>
    public class FrameGizmoFactory : MonoBehaviour
    {
        private const int JointCount = 6;
        private const float DefaultAxisLength = 0.12f;

        private FrameGizmo[] gizmos;
        private bool visible;

        /// <summary>
        /// 기즈모 가시성을 반환합니다.
        /// </summary>
        public bool IsVisible => visible;

        private void Awake()
        {
            EnsureGizmos();
        }

        private void OnEnable()
        {
            EnsureGizmos();
        }

        private void OnDisable()
        {
        }

        /// <summary>
        /// 6관절 + EE의 누적 변환을 기즈모에 적용합니다.
        /// </summary>
        public void ApplyFrames(Mat4D[] cumulativeTransforms)
        {
            EnsureGizmos();

            if (cumulativeTransforms == null)
            {
                return;
            }

            var count = System.Math.Min(cumulativeTransforms.Length, gizmos.Length);
            for (var i = 0; i < count; i++)
            {
                if (gizmos[i] != null)
                {
                    gizmos[i].SetPose(cumulativeTransforms[i]);
                }
            }
        }

        /// <summary>
        /// 모든 기즈모의 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool show)
        {
            visible = show;
            EnsureGizmos();

            for (var i = 0; i < gizmos.Length; i++)
            {
                if (gizmos[i] != null)
                {
                    gizmos[i].SetVisible(show);
                }
            }
        }

        private void EnsureGizmos()
        {
            if (gizmos != null && gizmos.Length == JointCount)
            {
                return;
            }

            gizmos = new FrameGizmo[JointCount];
            for (var i = 0; i < JointCount; i++)
            {
                var gizmoName = $"FrameGizmo_J{i + 1}";
                var existing = transform.Find(gizmoName);
                GameObject gizmoGo;

                if (existing != null)
                {
                    gizmoGo = existing.gameObject;
                }
                else
                {
                    gizmoGo = new GameObject(gizmoName);
                    gizmoGo.transform.SetParent(transform, false);
                }

                gizmos[i] = gizmoGo.GetComponent<FrameGizmo>();
                if (gizmos[i] == null)
                {
                    gizmos[i] = gizmoGo.AddComponent<FrameGizmo>();
                }
                gizmos[i].SetLength(DefaultAxisLength);
                gizmos[i].SetVisible(visible);
            }
        }
    }
}
