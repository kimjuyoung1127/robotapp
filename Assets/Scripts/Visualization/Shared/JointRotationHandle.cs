// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 관절에 1축 회전 링을 표시하고 마우스 드래그로 회전하는 핸들입니다.
    /// LineRenderer 원호로 링을 그리며, Raycast로 클릭 감지 후 각도를 emit합니다.
    /// </summary>
    public class JointRotationHandle : MonoBehaviour
    {
        private const int ArcSegments = 48;
        private const float DefaultRadius = 0.12f;
        private const float DefaultLineWidth = 0.004f;
        private const float SelectedLineWidth = 0.008f;
        private const float HitRadius = 0.03f;
        private const float DragSensitivity = 0.5f;

        [SerializeField] private int jointIndex;
        [SerializeField] private Color handleColor = new Color(0.29f, 0.56f, 0.85f, 1f);

        private LineRenderer lineRenderer;
        private Vector3 rotationAxis = Vector3.up;
        private float currentAngleDeg;
        private bool selected;
        private bool dragging;
        private Vector3 dragStartScreenPos;
        private float dragStartAngle;

        /// <summary>
        /// 관절 인덱스입니다.
        /// </summary>
        public int JointIndex => jointIndex;

        /// <summary>
        /// 핸들이 드래그되어 각도가 변경되었을 때 발생합니다.
        /// (jointIndex, angleDeg)를 전달합니다.
        /// </summary>
        public event Action<int, float> OnHandleDragged;

        /// <summary>
        /// 드래그 시작/종료 시 발생합니다. (jointIndex, true=시작, false=종료)를 전달합니다.
        /// </summary>
        public event Action<int, bool> OnDragStateChanged;

        /// <summary>
        /// 핸들을 초기화합니다.
        /// </summary>
        public void Initialize(int index, Vector3 axis, Color color)
        {
            jointIndex = index;
            rotationAxis = axis.normalized;
            handleColor = color;
            EnsureRenderer();
            UpdateArc();
        }

        /// <summary>
        /// 현재 각도를 외부에서 설정합니다 (슬라이더 동기화).
        /// </summary>
        public void SetAngle(float angleDeg)
        {
            if (!dragging)
            {
                currentAngleDeg = angleDeg;
            }
        }

        /// <summary>
        /// 선택 상태를 설정합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = selected ? SelectedLineWidth : DefaultLineWidth;
                lineRenderer.endWidth = selected ? SelectedLineWidth : DefaultLineWidth;
                var alpha = selected ? 1f : 0.6f;
                lineRenderer.startColor = new Color(handleColor.r, handleColor.g, handleColor.b, alpha);
                lineRenderer.endColor = new Color(handleColor.r, handleColor.g, handleColor.b, alpha);
            }
        }

        private void Awake()
        {
            EnsureRenderer();
        }

        private void OnEnable()
        {
            EnsureRenderer();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame && !dragging)
            {
                TryStartDrag(mouse);
            }

            if (dragging && mouse.leftButton.isPressed)
            {
                ContinueDrag(mouse);
            }

            if (dragging && mouse.leftButton.wasReleasedThisFrame)
            {
                EndDrag();
            }
        }

        private void TryStartDrag(Mouse mouse)
        {
            // UI 위에서는 핸들 동작 안 함
            if (UnityEngine.EventSystems.EventSystem.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            var screenPos = mouse.position.ReadValue();
            var ray = cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));

            // 링 평면에 투영하여 히트 판정
            var center = transform.position;
            var planeNormal = transform.TransformDirection(rotationAxis);
            var denom = Vector3.Dot(ray.direction, planeNormal);
            if (Mathf.Abs(denom) < 1e-5f)
            {
                return;
            }

            var t = Vector3.Dot(center - ray.origin, planeNormal) / denom;
            if (t < 0f)
            {
                return;
            }

            var hitPoint = ray.origin + ray.direction * t;
            var distToCenter = Vector3.Distance(hitPoint, center);

            if (Mathf.Abs(distToCenter - DefaultRadius) > HitRadius)
            {
                return;
            }

            dragging = true;
            dragStartScreenPos = new Vector3(screenPos.x, screenPos.y, 0f);
            dragStartAngle = currentAngleDeg;
            SetSelected(true);
            OnDragStateChanged?.Invoke(jointIndex, true);
        }

        private void ContinueDrag(Mouse mouse)
        {
            var screenPos = mouse.position.ReadValue();
            var deltaX = screenPos.x - dragStartScreenPos.x;
            var newAngle = dragStartAngle + deltaX * DragSensitivity;
            currentAngleDeg = newAngle;
            OnHandleDragged?.Invoke(jointIndex, currentAngleDeg);
        }

        private void EndDrag()
        {
            dragging = false;
            SetSelected(false);
            OnDragStateChanged?.Invoke(jointIndex, false);
        }

        private void UpdateArc()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = ArcSegments + 1;
            var positions = new Vector3[ArcSegments + 1];

            // 회전 축에 수직한 평면상의 원호
            var perpendicular = GetPerpendicular(rotationAxis);
            var bitangent = Vector3.Cross(rotationAxis, perpendicular).normalized;

            for (var i = 0; i <= ArcSegments; i++)
            {
                var angle = 2f * Mathf.PI * i / ArcSegments;
                var point = perpendicular * (Mathf.Cos(angle) * DefaultRadius)
                    + bitangent * (Mathf.Sin(angle) * DefaultRadius);
                positions[i] = point;
            }

            lineRenderer.SetPositions(positions);
        }

        private static Vector3 GetPerpendicular(Vector3 axis)
        {
            var candidate = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) < 0.9f
                ? Vector3.up
                : Vector3.right;
            return Vector3.Cross(axis, candidate).normalized;
        }

        private void EnsureRenderer()
        {
            if (lineRenderer != null)
            {
                return;
            }

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            var dimColor = new Color(handleColor.r, handleColor.g, handleColor.b, 0.6f);
            SharedLineMaterial.ConfigureLineRenderer(lineRenderer, dimColor, DefaultLineWidth);
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.numCornerVertices = 4;

            UpdateArc();
        }

    }
}
