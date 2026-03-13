// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 0도/90도/180도 방향 기준선을 표시합니다.
    /// MathReadiness에서 학생이 슬라이더를 어디로 옮겨야 하는지 바로 이해하도록 돕습니다.
    /// </summary>
    public class AngleReferenceMarker : MonoBehaviour
    {
        private static readonly float[] ReferenceAnglesDeg = { 0f, 90f, 180f };
        private static readonly string[] ReferenceLabels = { "0°", "90°", "180°" };
        private static Material sharedMaterial;

        [SerializeField] private float radius = 0.72f;
        [SerializeField] private float lineWidth = 0.008f;
        [SerializeField] private float planeHeight = 0.18f;
        [SerializeField] private float labelHeight = 0.08f;
        [SerializeField] private float labelScale = 0.06f;
        [SerializeField] private float labelRadialOffset = 0.14f;

        private LineRenderer[] lines;
        private TextMesh[] labels;
        private bool visible;

        public bool IsVisible => visible;

        public void ShowMarkers(Transform pivot, Color color)
        {
            EnsureVisuals();
            if (pivot == null || lines == null || labels == null)
            {
                return;
            }

            transform.position = pivot.position;
            transform.rotation = Quaternion.identity;

            var faded = new Color(color.r, color.g, color.b, 0.3f);
            for (var i = 0; i < ReferenceAnglesDeg.Length; i++)
            {
                var angleRad = ReferenceAnglesDeg[i] * Mathf.Deg2Rad;
                var radial = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
                var localStart = new Vector3(0f, planeHeight, 0f);
                var localEnd = radial * radius + new Vector3(0f, planeHeight, 0f);
                var line = lines[i];
                line.positionCount = 2;
                line.SetPosition(0, localStart);
                line.SetPosition(1, localEnd);
                line.startColor = faded;
                line.endColor = faded;
                line.enabled = visible;

                var label = labels[i];
                label.text = ReferenceLabels[i];
                label.color = new Color(color.r, color.g, color.b, 0.85f);
                label.transform.localPosition = GetLabelLocalPosition(i, localEnd);
                label.gameObject.SetActive(visible);
            }
        }

        private Vector3 GetLabelLocalPosition(int index, Vector3 lineEnd)
        {
            switch (index)
            {
                case 0:
                    // 0°는 트레일 시작점 오른쪽에 붙여 직관적으로 시작 방향을 읽게 합니다.
                    return new Vector3(radius + labelRadialOffset, planeHeight + (labelHeight * 0.35f), 0f);
                case 1:
                    // 90°는 로봇과 겹치지 않도록 위로 띄워 배치합니다.
                    return new Vector3(0f, planeHeight + radius + labelHeight, radius * 0.08f);
                case 2:
                    // 180°는 트레일 왼쪽 끝 근처에 붙여 반대 방향임을 바로 보이게 합니다.
                    return new Vector3(-(radius + labelRadialOffset), planeHeight + (labelHeight * 0.35f), 0f);
                default:
                    var radial = lineEnd.normalized;
                    return lineEnd + (radial * labelRadialOffset) + new Vector3(0f, labelHeight, 0f);
            }
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            EnsureVisuals();

            if (lines != null)
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i] != null)
                    {
                        lines[i].enabled = visible;
                    }
                }
            }

            if (labels != null)
            {
                for (var i = 0; i < labels.Length; i++)
                {
                    if (labels[i] != null)
                    {
                        labels[i].gameObject.SetActive(visible);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (!visible || labels == null)
            {
                return;
            }

            var targetCamera = Camera.main;
            if (targetCamera == null)
            {
                return;
            }

            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                if (label == null)
                {
                    continue;
                }

                var toCamera = label.transform.position - targetCamera.transform.position;
                if (toCamera.sqrMagnitude > 0.0001f)
                {
                    label.transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                }
            }
        }

        private void EnsureVisuals()
        {
            if (lines != null && labels != null)
            {
                return;
            }

            lines = new LineRenderer[ReferenceAnglesDeg.Length];
            labels = new TextMesh[ReferenceAnglesDeg.Length];

            for (var i = 0; i < ReferenceAnglesDeg.Length; i++)
            {
                lines[i] = GetOrCreateLineRenderer($"RefLine_{i}");
                labels[i] = GetOrCreateLabel($"RefLabel_{i}");
            }
        }

        private LineRenderer GetOrCreateLineRenderer(string childName)
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
            lr.alignment = LineAlignment.View;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.sharedMaterial = GetSharedMaterial();
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            return lr;
        }

        private TextMesh GetOrCreateLabel(string childName)
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

            var textMesh = go.GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = go.AddComponent<TextMesh>();
            }

            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = labelScale;
            textMesh.fontSize = 48;
            textMesh.text = string.Empty;
            textMesh.font = ResolveBuiltinFont();

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null && textMesh.font != null)
            {
                renderer.sharedMaterial = textMesh.font.material;
            }

            go.transform.localScale = Vector3.one;
            return textMesh;
        }

        private static Font ResolveBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            try
            {
                return Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            catch
            {
                return null;
            }
        }

        private static Material GetSharedMaterial()
        {
            if (sharedMaterial != null)
            {
                return sharedMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedMaterial = new Material(shader) { name = "KineTutor3D_AngleReferenceMarker" };
            return sharedMaterial;
        }
    }
}
