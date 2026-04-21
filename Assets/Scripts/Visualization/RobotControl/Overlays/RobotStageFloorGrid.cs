// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// RobotStage 바닥에 얕은 기준 격자를 그려 포즈 감각을 돕습니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class RobotStageFloorGrid : MonoBehaviour
    {
        [SerializeField] private float gridSpacing = 0.25f;
        [SerializeField] private int gridExtent = 8;
        [SerializeField] private float lineWidth = 0.004f;
        [SerializeField] private Color gridColor = new(0.45f, 0.53f, 0.65f, 0.26f);
        [SerializeField] private bool visible = true;

        private readonly List<LineRenderer> gridLines = new();

        private void Awake()
        {
            EnsureGrid();
            ApplyVisibility();
        }

        private void OnEnable()
        {
            EnsureGrid();
            ApplyVisibility();
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            ApplyVisibility();
        }

        private void EnsureGrid()
        {
            if (gridLines.Count > 0)
            {
                return;
            }

            var extent = gridExtent * gridSpacing;
            for (var index = -gridExtent; index <= gridExtent; index++)
            {
                var offset = index * gridSpacing;
                gridLines.Add(CreateLineRenderer($"GridH_{index}", new Vector3(-extent, 0f, offset), new Vector3(extent, 0f, offset)));
                gridLines.Add(CreateLineRenderer($"GridV_{index}", new Vector3(offset, 0f, -extent), new Vector3(offset, 0f, extent)));
            }
        }

        private LineRenderer CreateLineRenderer(string name, Vector3 start, Vector3 end)
        {
            var child = transform.Find(name);
            var lineObject = child != null ? child.gameObject : new GameObject(name);
            lineObject.transform.SetParent(transform, false);

            var lineRenderer = lineObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = lineObject.AddComponent<LineRenderer>();
            }

            SharedLineMaterial.ConfigureLineRenderer(lineRenderer, gridColor, lineWidth);
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.startColor = gridColor;
            lineRenderer.endColor = gridColor;
            return lineRenderer;
        }

        private void ApplyVisibility()
        {
            for (var index = 0; index < gridLines.Count; index++)
            {
                if (gridLines[index] != null)
                {
                    gridLines[index].enabled = visible;
                }
            }
        }
    }
}
