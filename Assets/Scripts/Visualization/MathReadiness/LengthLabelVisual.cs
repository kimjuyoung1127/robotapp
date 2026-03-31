// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 링크 길이를 치수선 스타일로 표시합니다.
    /// MathReadiness M1(길이+각도->위치), M3(두 링크 합성)에서 핵심 시각 힌트입니다.
    /// </summary>
    public class LengthLabelVisual : MonoBehaviour
    {
        private static Material sharedDimMaterial;

        [SerializeField] private float offset = 0.15f;
        [SerializeField] private float lineWidth = 0.015f;
        [SerializeField] private float tickLength = 0.06f;

        private LineRenderer dimLine;
        private LineRenderer startTick;
        private LineRenderer endTick;
        private bool visible;

        /// <summary>
        /// 치수선을 표시합니다.
        /// </summary>
        /// <param name="start">링크 시작 월드 위치</param>
        /// <param name="end">링크 끝 월드 위치</param>
        /// <param name="color">색상</param>
        public void ShowLength(Vector3 start, Vector3 end, Color color)
        {
            EnsureRenderers();
            if (dimLine == null)
            {
                return;
            }

            var dir = (end - start).normalized;
            var perpendicular = Vector3.Cross(dir, Vector3.up).normalized;
            if (perpendicular.sqrMagnitude < 0.01f)
            {
                perpendicular = Vector3.Cross(dir, Vector3.forward).normalized;
            }

            var offsetVec = perpendicular * offset;
            var s = start + offsetVec;
            var e = end + offsetVec;

            dimLine.positionCount = 2;
            dimLine.SetPosition(0, s);
            dimLine.SetPosition(1, e);
            ApplyColor(dimLine, color);

            var tickDir = perpendicular * tickLength * 0.5f;
            startTick.positionCount = 2;
            startTick.SetPosition(0, s - tickDir);
            startTick.SetPosition(1, s + tickDir);
            ApplyColor(startTick, color);

            endTick.positionCount = 2;
            endTick.SetPosition(0, e - tickDir);
            endTick.SetPosition(1, e + tickDir);
            ApplyColor(endTick, color);
        }

        /// <summary>
        /// 표시/숨김을 제어합니다.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            EnsureRenderers();
            SetRendererEnabled(dimLine, visible);
            SetRendererEnabled(startTick, visible);
            SetRendererEnabled(endTick, visible);
        }

        /// <summary>
        /// 현재 표시 상태를 반환합니다.
        /// </summary>
        public bool IsVisible => visible;

        private void EnsureRenderers()
        {
            dimLine ??= CreateLineRenderer("DimLine", lineWidth);
            startTick ??= CreateLineRenderer("StartTick", lineWidth);
            endTick ??= CreateLineRenderer("EndTick", lineWidth);
        }

        private LineRenderer CreateLineRenderer(string childName, float width)
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

            lr.useWorldSpace = true;
            lr.loop = false;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.alignment = LineAlignment.View;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.sharedMaterial = GetSharedMaterial();
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            return lr;
        }

        private static void ApplyColor(LineRenderer lr, Color color)
        {
            if (lr != null)
            {
                lr.startColor = color;
                lr.endColor = color;
            }
        }

        private static void SetRendererEnabled(LineRenderer lr, bool enabled)
        {
            if (lr != null)
            {
                lr.enabled = enabled;
            }
        }

        private static Material GetSharedMaterial()
        {
            if (sharedDimMaterial != null)
            {
                return sharedDimMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedDimMaterial = new Material(shader) { name = "KineTutor3D_LengthLabelVisual" };
            return sharedDimMaterial;
        }
    }
}
