// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using KineTutor3D.Math;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// EE 변위 벡터를 화살표(LineRenderer + 원뿔 헤드)로 표시하는 공유 컴포넌트입니다.
    /// 이전 EE 위치에서 현재 EE 위치 방향으로 화살표를 그립니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class DisplacementArrow : MonoBehaviour
    {
        private const float MinDisplacementToShow = 0.01f;
        private const float LineWidth = 0.008f;
        private const float ConeHeight = 0.03f;
        private const float ConeRadius = 0.012f;

        private LineRenderer lineRenderer;
        private GameObject coneHead;
        private Vector3 previousEEWorldPos;
        private bool hasPrevious;
        private bool visible = true;

        /// <summary>
        /// 화살표 가시성 상태입니다.
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
        /// FK 결과로부터 EE 위치를 업데이트하고 화살표를 갱신합니다.
        /// </summary>
        public void UpdateFromFK(Mat4D eeTransform)
        {
            var unityPos = CoordConverter.ToUnityPosition(eeTransform.ExtractPosition());
            UpdateWorldPosition(unityPos);
        }

        /// <summary>
        /// Unity 월드 좌표로 화살표를 갱신합니다.
        /// </summary>
        public void UpdateWorldPosition(Vector3 currentWorldPos)
        {
            EnsureRenderer();

            if (!hasPrevious)
            {
                previousEEWorldPos = currentWorldPos;
                hasPrevious = true;
                HideArrow();
                return;
            }

            var displacement = currentWorldPos - previousEEWorldPos;
            var magnitude = displacement.magnitude;

            if (magnitude < MinDisplacementToShow)
            {
                HideArrow();
                return;
            }

            if (!visible)
            {
                return;
            }

            ShowArrow(previousEEWorldPos, currentWorldPos, displacement.normalized);
            previousEEWorldPos = currentWorldPos;
        }

        /// <summary>
        /// 화살표를 숨기고 이전 위치를 리셋합니다.
        /// </summary>
        public void Clear()
        {
            hasPrevious = false;
            HideArrow();
        }

        /// <summary>
        /// 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool show)
        {
            visible = show;
            if (!show)
            {
                HideArrow();
            }
        }

        private void ShowArrow(Vector3 from, Vector3 to, Vector3 direction)
        {
            if (lineRenderer == null)
            {
                return;
            }

            // 라인: from → to 직전 (cone head 공간 확보)
            var lineEnd = to - direction * ConeHeight;
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, from);
            lineRenderer.SetPosition(1, lineEnd);

            // 원뿔 헤드
            if (coneHead != null)
            {
                coneHead.SetActive(true);
                coneHead.transform.position = to;
                coneHead.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private void HideArrow()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
                lineRenderer.positionCount = 0;
            }

            if (coneHead != null)
            {
                coneHead.SetActive(false);
            }
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

            var arrowColor = UIDesignTokens.Colors.AccentSecondary;
            SharedLineMaterial.ConfigureLineRenderer(lineRenderer, arrowColor, LineWidth);
            lineRenderer.enabled = false;

            EnsureConeHead(arrowColor);
        }

        private void EnsureConeHead(Color color)
        {
            if (coneHead != null)
            {
                return;
            }

            coneHead = new GameObject("ArrowHead");
            coneHead.transform.SetParent(transform, false);

            var meshFilter = coneHead.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateConeMesh();

            var meshRenderer = coneHead.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = SharedLineMaterial.CreateInstance(color);
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            coneHead.SetActive(false);
        }

        private static Mesh CreateConeMesh()
        {
            const int segments = 12;
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 6];

            // 꼭짓점 (위)
            vertices[0] = new Vector3(0f, ConeHeight, 0f);
            // 중심 (아래)
            vertices[segments + 1] = Vector3.zero;

            for (var i = 0; i < segments; i++)
            {
                var angle = 2f * Mathf.PI * i / segments;
                vertices[i + 1] = new Vector3(
                    Mathf.Cos(angle) * ConeRadius,
                    0f,
                    Mathf.Sin(angle) * ConeRadius);

                // 옆면
                var next = (i + 1) % segments;
                triangles[i * 6] = 0;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = next + 1;

                // 바닥면
                triangles[i * 6 + 3] = segments + 1;
                triangles[i * 6 + 4] = next + 1;
                triangles[i * 6 + 5] = i + 1;
            }

            var mesh = new Mesh
            {
                name = "DisplacementArrowCone",
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

    }
}
