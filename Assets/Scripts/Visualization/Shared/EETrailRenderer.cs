// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using System.Collections.Generic;
using KineTutor3D.Math;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 엔드이펙터 궤적을 LineRenderer로 표시하는 공유 컴포넌트입니다.
    /// MathReadiness, RobotControl, Sandbox 등 모든 로봇 씬에서 재사용합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class EETrailRenderer : MonoBehaviour
    {
        private const int DefaultMaxPoints = 256;
        private const float DefaultMinDistance = 0.02f;
        private const float DefaultLineWidth = 0.015f;

        [SerializeField] private int maxPoints = DefaultMaxPoints;
        [SerializeField] private float minDistance = DefaultMinDistance;
        [SerializeField] private float lineWidth = DefaultLineWidth;

        private LineRenderer lineRenderer;
        private readonly List<Vector3> points = new List<Vector3>();
        private bool visible = true;

        /// <summary>
        /// 현재 기록된 궤적 포인트 수입니다.
        /// </summary>
        public int PointCount => points.Count;

        /// <summary>
        /// 트레일 가시성 상태입니다.
        /// </summary>
        public bool IsVisible => visible;

        private void Awake()
        {
            EnsureRenderer();
        }

        private void OnEnable()
        {
            EnsureRenderer();
        }

        /// <summary>
        /// Mat4D EE 변환 행렬에서 위치를 추출하여 트레일에 추가합니다.
        /// 로보틱스 좌표를 Unity 좌표로 변환합니다.
        /// </summary>
        public void AddPoint(Mat4D eeTransform)
        {
            var unityPos = CoordConverter.ToUnityPosition(eeTransform.ExtractPosition());
            AddWorldPoint(unityPos);
        }

        /// <summary>
        /// Unity 월드 좌표를 직접 트레일에 추가합니다.
        /// </summary>
        public void AddWorldPoint(Vector3 worldPosition)
        {
            EnsureRenderer();
            if (lineRenderer == null)
            {
                return;
            }

            if (!visible)
            {
                return;
            }

            // 거리 게이팅: 이전 포인트와 너무 가까우면 무시
            if (points.Count > 0 && Vector3.Distance(points[points.Count - 1], worldPosition) < minDistance)
            {
                return;
            }

            points.Add(worldPosition);

            // FIFO 오버플로: 오래된 포인트를 1개씩 제거 (궤적 연속성 유지)
            while (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        /// <summary>
        /// 트레일을 초기화합니다.
        /// </summary>
        public void Clear()
        {
            points.Clear();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }

        /// <summary>
        /// 트레일 가시성을 설정합니다. 비활성화 시 궤적도 초기화합니다.
        /// </summary>
        public void SetVisible(bool show)
        {
            visible = show;
            if (!show)
            {
                Clear();
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = show;
            }
        }

        /// <summary>
        /// 최대 포인트 수를 변경합니다.
        /// </summary>
        public void SetMaxPoints(int count)
        {
            maxPoints = Mathf.Max(8, count);
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

            SharedLineMaterial.ConfigureLineRenderer(lineRenderer, new Color(0.29f, 0.56f, 0.85f, 0.95f), lineWidth);
            lineRenderer.endColor = new Color(0.95f, 0.77f, 0.15f, 0.95f);
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
            lineRenderer.enabled = visible;
        }

    }
}
