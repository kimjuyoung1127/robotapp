// Folder: Visualization - Unity-side rendering and FK binding.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// XZ 평면(로보틱스 XY) 좌표 그리드를 표시합니다.
    /// MathReadiness M2(45°=대각선)에서 대각선 방향 이해를 돕습니다.
    /// </summary>
    public class CoordinateGridVisual : MonoBehaviour
    {
        private static Material sharedGridMaterial;

        [SerializeField] private float gridSpacing = 0.5f;
        [SerializeField] private int gridExtent = 2;
        [SerializeField] private float lineWidth = 0.005f;
        [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        [SerializeField] private Color xAxisColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color zAxisColor = new Color(0.3f, 1f, 0.3f, 0.5f);
        [SerializeField] private float axisLineWidth = 0.01f;

        private readonly List<LineRenderer> gridLines = new List<LineRenderer>();
        private LineRenderer xAxisLine;
        private LineRenderer zAxisLine;
        private LineRenderer diagonalLine;
        private bool diagonalVisible;
        private bool diagonalNegativeSlope;

        /// <summary>
        /// 그리드를 생성합니다.
        /// </summary>
        public void Initialize()
        {
            BuildGrid();
            BuildAxes();
            BuildDiagonal();
        }

        /// <summary>
        /// 45도 대각선 가이드라인 표시를 제어합니다.
        /// </summary>
        public void ShowDiagonalGuide(bool show)
        {
            diagonalVisible = show;
            diagonalNegativeSlope = false;
            UpdateDiagonalGuide();
        }

        /// <summary>
        /// 대각선 가이드라인 표시와 방향을 제어합니다.
        /// </summary>
        public void ShowDiagonalGuide(bool show, bool negativeSlope)
        {
            diagonalVisible = show;
            diagonalNegativeSlope = negativeSlope;
            UpdateDiagonalGuide();
        }

        /// <summary>
        /// 대각선 가이드 색상을 설정합니다.
        /// </summary>
        public void SetDiagonalColor(Color color)
        {
            if (diagonalLine != null)
            {
                diagonalLine.startColor = color;
                diagonalLine.endColor = new Color(color.r, color.g, color.b, color.a * 0.4f);
            }
        }

        private void UpdateDiagonalGuide()
        {
            if (diagonalLine == null)
            {
                return;
            }

            var extent = gridExtent * gridSpacing;
            diagonalLine.positionCount = 2;
            diagonalLine.SetPosition(0, Vector3.zero);
            diagonalLine.SetPosition(1, diagonalNegativeSlope
                ? new Vector3(extent, 0f, -extent)
                : new Vector3(extent, 0f, extent));
            diagonalLine.enabled = diagonalVisible;
        }

        /// <summary>
        /// 표시/숨김을 제어합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            for (var i = 0; i < gridLines.Count; i++)
            {
                if (gridLines[i] != null)
                {
                    gridLines[i].enabled = visible;
                }
            }

            if (xAxisLine != null)
            {
                xAxisLine.enabled = visible;
            }

            if (zAxisLine != null)
            {
                zAxisLine.enabled = visible;
            }

            if (diagonalLine != null)
            {
                diagonalLine.enabled = visible && diagonalVisible;
            }
        }

        private void BuildGrid()
        {
            var extent = gridExtent * gridSpacing;

            for (var i = -gridExtent; i <= gridExtent; i++)
            {
                var pos = i * gridSpacing;
                if (Mathf.Abs(i) == 0)
                {
                    continue;
                }

                var hLine = CreateLineRenderer($"GridH_{i}", lineWidth, gridColor);
                hLine.positionCount = 2;
                hLine.SetPosition(0, new Vector3(-extent, 0f, pos));
                hLine.SetPosition(1, new Vector3(extent, 0f, pos));
                gridLines.Add(hLine);

                var vLine = CreateLineRenderer($"GridV_{i}", lineWidth, gridColor);
                vLine.positionCount = 2;
                vLine.SetPosition(0, new Vector3(pos, 0f, -extent));
                vLine.SetPosition(1, new Vector3(pos, 0f, extent));
                gridLines.Add(vLine);
            }
        }

        private void BuildAxes()
        {
            var extent = gridExtent * gridSpacing;

            xAxisLine = CreateLineRenderer("XAxis", axisLineWidth, xAxisColor);
            xAxisLine.positionCount = 2;
            xAxisLine.SetPosition(0, new Vector3(-extent, 0f, 0f));
            xAxisLine.SetPosition(1, new Vector3(extent, 0f, 0f));

            zAxisLine = CreateLineRenderer("ZAxis", axisLineWidth, zAxisColor);
            zAxisLine.positionCount = 2;
            zAxisLine.SetPosition(0, new Vector3(0f, 0f, -extent));
            zAxisLine.SetPosition(1, new Vector3(0f, 0f, extent));
        }

        private void BuildDiagonal()
        {
            var extent = gridExtent * gridSpacing;
            var diagColor = new Color(0.62f, 0.38f, 0.85f, 0.5f);

            diagonalLine = CreateLineRenderer("Diagonal45", axisLineWidth, diagColor);
            diagonalLine.positionCount = 2;
            diagonalLine.SetPosition(0, Vector3.zero);
            diagonalLine.SetPosition(1, new Vector3(extent, 0f, extent));
            diagonalLine.enabled = false;
        }

        private LineRenderer CreateLineRenderer(string childName, float width, Color color)
        {
            var child = transform.Find(childName);
            GameObject go;
            if (child == null)
            {
                go = new GameObject(childName);
                go.transform.SetParent(transform, false);
            }
            else
            {
                go = child.gameObject;
            }

            var lr = go.GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = go.AddComponent<LineRenderer>();
            }

            lr.useWorldSpace = false;
            lr.loop = false;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.alignment = LineAlignment.View;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.sharedMaterial = GetSharedMaterial();
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            return lr;
        }

        private static Material GetSharedMaterial()
        {
            if (sharedGridMaterial != null)
            {
                return sharedGridMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedGridMaterial = new Material(shader) { name = "KineTutor3D_CoordinateGridVisual" };
            return sharedGridMaterial;
        }
    }
}
