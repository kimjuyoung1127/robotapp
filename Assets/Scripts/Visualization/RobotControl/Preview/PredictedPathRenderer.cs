// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 미리보기 경로를 별도 라인으로 표시합니다.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [DisallowMultipleComponent]
    public sealed class PredictedPathRenderer : MonoBehaviour
    {
        [SerializeField] private Color pathColor = new(0.55f, 0.80f, 1.00f, 0.85f);
        [SerializeField] private float pathWidth = 0.012f;
        [SerializeField] private LineRenderer lineRenderer;

        public bool HasPath => lineRenderer != null && lineRenderer.enabled && lineRenderer.positionCount > 1;

        private void Awake()
        {
            EnsureRenderer();
            ClearPath();
        }

        private void OnEnable()
        {
            EnsureRenderer();
        }

        public void RenderPath(IReadOnlyList<Vector3> positions)
        {
            EnsureRenderer();
            if (positions == null || positions.Count < 2)
            {
                ClearPath();
                return;
            }

            lineRenderer.positionCount = positions.Count;
            for (var index = 0; index < positions.Count; index++)
            {
                lineRenderer.SetPosition(index, positions[index]);
            }

            lineRenderer.enabled = true;
        }

        public void ClearPath()
        {
            EnsureRenderer();
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        private void EnsureRenderer()
        {
            lineRenderer ??= GetComponent<LineRenderer>();
            SharedLineMaterial.ConfigureLineRenderer(lineRenderer, pathColor, pathWidth);
            lineRenderer.useWorldSpace = true;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 2;
        }
    }
}
