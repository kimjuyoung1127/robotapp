// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 선택한 파츠 위에 월드 XYZ 기즈모를 띄워 현재 기준점을 보여줍니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class RobotPartSelectionGizmo : MonoBehaviour
    {
        [SerializeField] private float axisLength = 0.08f;
        [SerializeField] private Transform selectedTarget;
        [SerializeField] private FrameGizmo frameGizmo;

        public Transform SelectedTarget => selectedTarget;

        private void Awake()
        {
            EnsureGizmo();
            Clear();
        }

        private void OnEnable()
        {
            EnsureGizmo();
            RefreshPose();
        }

        private void LateUpdate()
        {
            RefreshPose();
        }

        public void Select(Transform target)
        {
            selectedTarget = target;
            RefreshPose();
            frameGizmo?.SetVisible(selectedTarget != null);
        }

        public void Clear()
        {
            selectedTarget = null;
            frameGizmo?.SetVisible(false);
        }

        public void SetAxisLength(float length)
        {
            axisLength = Mathf.Max(0.02f, length);
            frameGizmo?.SetLength(axisLength);
        }

        private void EnsureGizmo()
        {
            if (frameGizmo != null)
            {
                return;
            }

            var child = transform.Find("SelectedPartFrame");
            var targetObject = child != null ? child.gameObject : new GameObject("SelectedPartFrame");
            targetObject.transform.SetParent(transform, false);
            frameGizmo = targetObject.GetComponent<FrameGizmo>();
            if (frameGizmo == null)
            {
                frameGizmo = targetObject.AddComponent<FrameGizmo>();
            }

            frameGizmo.SetLength(axisLength);
            frameGizmo.SetVisible(false);
        }

        private void RefreshPose()
        {
            if (frameGizmo == null || selectedTarget == null)
            {
                return;
            }

            frameGizmo.transform.position = selectedTarget.position;
            frameGizmo.transform.rotation = selectedTarget.rotation;
            frameGizmo.transform.localScale = Vector3.one;
        }
    }
}
