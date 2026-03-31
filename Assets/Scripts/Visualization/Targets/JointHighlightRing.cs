// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 지정한 관절 앵커를 둘러싸는 간단한 링 하이라이트를 그립니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class JointHighlightRing : MonoBehaviour
    {
        private static Material sharedRingMaterial;

        [SerializeField] private Transform target;
        [SerializeField] private float radius = 0.18f;
        [SerializeField] private int segments = 32;
        [SerializeField] private Color ringColor = new Color(0.95f, 0.77f, 0.15f, 1f);
        [SerializeField] private bool visible;

        private LineRenderer lineRenderer;

        public bool IsVisible => visible;

        private void Awake()
        {
            EnsureRenderer();
            RebuildRing();
            ApplyVisibility();
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position;
            }
        }

        public void Bind(Transform targetAnchor, float radiusValue, Color color)
        {
            target = targetAnchor;
            radius = Mathf.Max(0.01f, radiusValue);
            ringColor = color;
            EnsureRenderer();
            RebuildRing();
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            ApplyVisibility();
        }

        private void EnsureRenderer()
        {
            lineRenderer ??= GetComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.widthMultiplier = 0.02f;
            lineRenderer.positionCount = Mathf.Max(segments, 8);
            lineRenderer.sharedMaterial = GetSharedRingMaterial();
            lineRenderer.startColor = ringColor;
            lineRenderer.endColor = ringColor;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
        }

        private static Material GetSharedRingMaterial()
        {
            if (sharedRingMaterial != null)
            {
                return sharedRingMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedRingMaterial = new Material(shader)
            {
                name = "KineTutor3D_JointHighlightRing"
            };
            return sharedRingMaterial;
        }

        private void RebuildRing()
        {
            if (lineRenderer == null)
            {
                return;
            }

            var pointCount = Mathf.Max(segments, 8);
            lineRenderer.positionCount = pointCount;

            for (var i = 0; i < pointCount; i++)
            {
                var angle = Mathf.PI * 2f * i / pointCount;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                lineRenderer.SetPosition(i, position);
            }

            lineRenderer.startColor = ringColor;
            lineRenderer.endColor = ringColor;
        }

        private void ApplyVisibility()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = visible;
            }
        }
    }
}
