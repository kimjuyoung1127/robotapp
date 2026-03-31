// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 조인트 각도를 부채꼴 아크로 시각화합니다.
    /// MathReadiness M0(각도=방향), M2(45°=대각선)에서 핵심 시각 힌트입니다.
    /// </summary>
    public class AngleArcVisual : MonoBehaviour
    {
        private static Material sharedArcMaterial;

        [SerializeField] private float radius = 0.3f;
        [SerializeField] private int segments = 32;
        [SerializeField] private float lineWidth = 0.02f;

        private LineRenderer arcLine;
        private bool visible;

        /// <summary>
        /// 아크를 표시합니다.
        /// </summary>
        /// <param name="pivot">회전 중심 Transform</param>
        /// <param name="startAngleDeg">시작 각도 (0° = Unity +X 방향)</param>
        /// <param name="sweepAngleDeg">아크 범위 각도</param>
        /// <param name="color">아크 색상</param>
        public void ShowArc(Transform pivot, float startAngleDeg, float sweepAngleDeg, Color color)
        {
            EnsureRenderer();
            if (arcLine == null || pivot == null)
            {
                return;
            }

            transform.position = pivot.position;
            transform.rotation = Quaternion.identity;

            var count = Mathf.Max(segments, 8) + 1;
            arcLine.positionCount = count;

            var startRad = startAngleDeg * Mathf.Deg2Rad;
            var sweepRad = sweepAngleDeg * Mathf.Deg2Rad;

            for (var i = 0; i < count; i++)
            {
                var t = (float)i / (count - 1);
                var angle = startRad + sweepRad * t;
                arcLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }

            arcLine.startColor = color;
            arcLine.endColor = color;
            arcLine.startWidth = lineWidth;
            arcLine.endWidth = lineWidth;
        }

        /// <summary>
        /// 표시/숨김을 제어합니다.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            EnsureRenderer();
            if (arcLine != null)
            {
                arcLine.enabled = visible;
            }
        }

        /// <summary>
        /// 현재 표시 상태를 반환합니다.
        /// </summary>
        public bool IsVisible => visible;

        private void EnsureRenderer()
        {
            if (arcLine != null)
            {
                return;
            }

            var child = transform.Find("ArcLine");
            GameObject go;
            if (child == null)
            {
                go = new GameObject("ArcLine");
                go.transform.SetParent(transform, false);
            }
            else
            {
                go = child.gameObject;
            }

            arcLine = go.GetComponent<LineRenderer>();
            if (arcLine == null)
            {
                arcLine = go.AddComponent<LineRenderer>();
            }

            arcLine.useWorldSpace = false;
            arcLine.loop = false;
            arcLine.alignment = LineAlignment.View;
            arcLine.numCornerVertices = 4;
            arcLine.numCapVertices = 4;
            arcLine.sharedMaterial = GetSharedMaterial();
            arcLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            arcLine.receiveShadows = false;
        }

        private static Material GetSharedMaterial()
        {
            if (sharedArcMaterial != null)
            {
                return sharedArcMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedArcMaterial = new Material(shader) { name = "KineTutor3D_AngleArcVisual" };
            return sharedArcMaterial;
        }
    }
}
