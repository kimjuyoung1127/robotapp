// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;
using UnityEngine.InputSystem;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 대상 오브젝트 중심으로 자유 궤도 카메라를 제공하는 공유 컴포넌트입니다.
    /// 좌클릭 드래그: 회전, 스크롤: 줌, 우클릭 드래그: 팬.
    /// 신규 Input System 전용. 어느 씬에서든 AddComponent로 사용 가능합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class OrbitCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 1.2f;
        [SerializeField] private float minDistance = 0.3f;
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private float rotationSpeed = 0.3f;
        [SerializeField] private float zoomSpeed = 0.15f;
        [SerializeField] private float panSpeed = 0.002f;
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;

        private float yaw;
        private float pitch;
        private Vector3 pivotOffset;
        private bool initialized;
        private bool orbitEnabled = true;

        /// <summary>
        /// 현재 오빗 중심 대상입니다.
        /// </summary>
        public Transform Target => target;

        /// <summary>
        /// 현재 카메라-대상 간 거리입니다.
        /// </summary>
        public float Distance => distance;

        /// <summary>
        /// 현재 적용 중인 pivot 오프셋입니다.
        /// </summary>
        public Vector3 PivotOffset => pivotOffset;

        /// <summary>
        /// 오빗 조작 활성 상태입니다.
        /// </summary>
        public bool OrbitEnabled
        {
            get => orbitEnabled;
            set => orbitEnabled = value;
        }

        /// <summary>
        /// 오빗 중심 대상을 설정합니다.
        /// </summary>
        public void SetTarget(Transform pivotTarget)
        {
            target = pivotTarget;
            if (target != null)
            {
                InitializeFromCurrent();
            }
        }

        /// <summary>
        /// 대상과 초기 거리를 함께 설정합니다.
        /// </summary>
        public void SetTarget(Transform pivotTarget, float initialDistance)
        {
            target = pivotTarget;
            distance = Mathf.Clamp(initialDistance, minDistance, maxDistance);
            if (target != null)
            {
                InitializeFromCurrent();
            }
        }

        /// <summary>
        /// 거리 범위를 설정합니다.
        /// </summary>
        public void SetDistanceLimits(float min, float max)
        {
            minDistance = Mathf.Max(0.01f, min);
            maxDistance = Mathf.Max(minDistance, max);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        /// <summary>
        /// 대상 기준 pivot 오프셋을 설정합니다.
        /// </summary>
        public void SetPivotOffset(Vector3 offset)
        {
            pivotOffset = offset;
            initialized = true;

            if (target != null)
            {
                ApplyOrbit();
            }
        }

        /// <summary>
        /// 팬 오프셋을 초기화하고 대상 중심으로 카메라를 리셋합니다.
        /// </summary>
        public void ResetView()
        {
            pivotOffset = Vector3.zero;
            if (target != null)
            {
                ApplyOrbit();
            }
        }

        /// <summary>
        /// 특정 각도와 거리로 카메라를 설정합니다.
        /// </summary>
        public void SetView(float yawDeg, float pitchDeg, float viewDistance)
        {
            yaw = yawDeg;
            pitch = Mathf.Clamp(pitchDeg, minVerticalAngle, maxVerticalAngle);
            distance = Mathf.Clamp(viewDistance, minDistance, maxDistance);
            pivotOffset = Vector3.zero;
            initialized = true;

            if (target != null)
            {
                ApplyOrbit();
            }
        }

        private void LateUpdate()
        {
            if (target == null || !orbitEnabled)
            {
                return;
            }

            if (!initialized)
            {
                InitializeFromCurrent();
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            // UI 위에서는 카메라 조작 안 함
            if (UnityEngine.EventSystems.EventSystem.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var delta = mouse.delta.ReadValue();
            var scroll = mouse.scroll.ReadValue().y;

            // 좌클릭 드래그: 회전
            if (mouse.leftButton.isPressed)
            {
                yaw += delta.x * rotationSpeed;
                pitch -= delta.y * rotationSpeed;
                pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            }

            // 우클릭 드래그: 팬
            if (mouse.rightButton.isPressed)
            {
                var cam = transform;
                pivotOffset -= cam.right * (delta.x * panSpeed * distance);
                pivotOffset -= cam.up * (delta.y * panSpeed * distance);
            }

            // 스크롤: 줌
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= scroll * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            ApplyOrbit();
        }

        private void InitializeFromCurrent()
        {
            if (target == null)
            {
                return;
            }

            var cam = transform;
            var toCamera = cam.position - target.position;
            distance = Mathf.Clamp(toCamera.magnitude, minDistance, maxDistance);

            if (distance > 0.01f)
            {
                // The orbit offset is computed from a rotated Vector3.back,
                // so we need the camera's look direction here, not the raw
                // target->camera offset, otherwise the first orbit frame flips.
                var lookDirection = (-toCamera).normalized;
                yaw = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
                pitch = -Mathf.Asin(Mathf.Clamp(lookDirection.y, -1f, 1f)) * Mathf.Rad2Deg;
            }

            pivotOffset = Vector3.zero;
            initialized = true;
        }

        private void ApplyOrbit()
        {
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var offset = rotation * new Vector3(0f, 0f, -distance);
            var pivotPoint = target.position + pivotOffset;

            transform.position = pivotPoint + offset;
            transform.LookAt(pivotPoint);
        }
    }
}
