// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 빈 앵커 사이를 LineRenderer로 연결해 팔 형태를 시각화합니다.
    /// 2DOF_RR처럼 donor mesh가 없는 로봇에서 링크 구조를 보여줍니다.
    /// </summary>
    public class SimpleArmOverlay : MonoBehaviour
    {
        private static Material sharedArmMaterial;

        [SerializeField] private float lineWidth = 0.06f;
        [SerializeField] private float jointMarkerRadius = 0.08f;
        [SerializeField] private int jointMarkerSegments = 24;
        [SerializeField] private Color armColor = Color.white;
        [SerializeField] private bool showArmLine;

        private LineRenderer armLine;
        private LineRenderer[] jointMarkers;
        private Transform[] anchors;
        private bool initialized;

        /// <summary>
        /// 조인트 앵커 배열로 초기화합니다 (Base, Joint1, Joint2, EE 순서).
        /// </summary>
        public void Initialize(Transform[] jointAnchors, Color color)
        {
            anchors = jointAnchors;
            armColor = color;
            EnsureRenderers();
            initialized = true;
            UpdateVisual();
        }

        /// <summary>
        /// 현재 앵커 위치로 시각을 갱신합니다.
        /// </summary>
        public void UpdateVisual()
        {
            if (!initialized || anchors == null || armLine == null)
            {
                return;
            }

            var validCount = 0;
            for (var i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null)
                {
                    validCount++;
                }
            }

            armLine.positionCount = validCount;
            var idx = 0;
            for (var i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null)
                {
                    armLine.SetPosition(idx, anchors[i].position);
                    idx++;
                }
            }

            armLine.enabled = showArmLine;

            if (jointMarkers != null)
            {
                for (var i = 0; i < jointMarkers.Length && i < anchors.Length; i++)
                {
                    if (jointMarkers[i] != null && anchors[i] != null)
                    {
                        jointMarkers[i].transform.position = anchors[i].position;
                    }
                }
            }
        }

        /// <summary>
        /// 표시/숨김을 제어합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            EnsureRenderers();
            if (armLine != null)
            {
                armLine.enabled = visible && showArmLine;
            }

            if (jointMarkers != null)
            {
                for (var i = 0; i < jointMarkers.Length; i++)
                {
                    if (jointMarkers[i] != null)
                    {
                        jointMarkers[i].enabled = visible;
                    }
                }
            }
        }

        /// <summary>
        /// 팔 색상을 변경합니다.
        /// </summary>
        public void SetColor(Color color)
        {
            armColor = color;
            if (armLine != null)
            {
                armLine.startColor = color;
                armLine.endColor = color;
            }

            if (jointMarkers != null)
            {
                for (var i = 0; i < jointMarkers.Length; i++)
                {
                    if (jointMarkers[i] != null)
                    {
                        jointMarkers[i].startColor = color;
                        jointMarkers[i].endColor = color;
                    }
                }
            }
        }

        private void EnsureRenderers()
        {
            if (armLine == null)
            {
                armLine = GetOrCreateLineRenderer("ArmLine");
                armLine.useWorldSpace = true;
                armLine.loop = false;
                armLine.startWidth = lineWidth;
                armLine.endWidth = lineWidth;
                armLine.startColor = armColor;
                armLine.endColor = armColor;
                armLine.numCornerVertices = 4;
                armLine.numCapVertices = 4;
            }

            if (anchors != null && (jointMarkers == null || jointMarkers.Length != anchors.Length))
            {
                jointMarkers = new LineRenderer[anchors.Length];
                for (var i = 0; i < anchors.Length; i++)
                {
                    var marker = GetOrCreateLineRenderer($"JointMarker_{i}");
                    marker.useWorldSpace = false;
                    marker.loop = true;
                    marker.widthMultiplier = 0.025f;
                    marker.startColor = armColor;
                    marker.endColor = armColor;
                    marker.numCornerVertices = 4;
                    marker.numCapVertices = 4;
                    BuildCircle(marker, jointMarkerRadius, jointMarkerSegments);
                    jointMarkers[i] = marker;
                }
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

            lr.sharedMaterial = GetSharedMaterial();
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.alignment = LineAlignment.View;
            return lr;
        }

        private static void BuildCircle(LineRenderer lr, float radius, int segments)
        {
            var count = Mathf.Max(segments, 8);
            lr.positionCount = count;
            for (var i = 0; i < count; i++)
            {
                var angle = Mathf.PI * 2f * i / count;
                lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static Material GetSharedMaterial()
        {
            if (sharedArmMaterial != null)
            {
                return sharedArmMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedArmMaterial = new Material(shader) { name = "KineTutor3D_SimpleArmOverlay" };
            return sharedArmMaterial;
        }
    }
}
