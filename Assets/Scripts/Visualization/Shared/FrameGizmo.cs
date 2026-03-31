// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.Math;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// Renders a local coordinate frame with three colored axes.
    /// </summary>
    [ExecuteAlways]
    public class FrameGizmo : MonoBehaviour
    {
        [SerializeField] private float axisLength = 0.2f;
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private bool showLabels;

        [SerializeField] private LineRenderer xRenderer;
        [SerializeField] private LineRenderer yRenderer;
        [SerializeField] private LineRenderer zRenderer;

        private void Awake()
        {
            EnsureAxes();
            RefreshVisuals();
        }

        private void OnEnable()
        {
            EnsureAxes();
            RefreshVisuals();
        }

        private void OnValidate()
        {
            if (xRenderer != null && yRenderer != null && zRenderer != null)
            {
                RefreshVisuals();
            }
        }

        /// <summary>
        /// Sets the displayed frame length.
        /// </summary>
        public void SetLength(float newAxisLength)
        {
            axisLength = Mathf.Max(0.01f, newAxisLength);
            RefreshVisuals();
        }

        /// <summary>
        /// Sets the visibility of all axis renderers.
        /// </summary>
        public void SetVisible(bool visible)
        {
            EnsureAxes();
            xRenderer.enabled = visible;
            yRenderer.enabled = visible;
            zRenderer.enabled = visible;
        }

        /// <summary>
        /// Applies a robotics-space pose to this gizmo transform.
        /// </summary>
        public void SetPose(Mat4D roboticsTransform)
        {
            CoordConverter.ApplyLocalTransform(transform, roboticsTransform);
        }

        private void EnsureAxes()
        {
            xRenderer ??= GetOrCreateAxisRenderer("AxisX", Color.red);
            yRenderer ??= GetOrCreateAxisRenderer("AxisY", Color.green);
            zRenderer ??= GetOrCreateAxisRenderer("AxisZ", Color.blue);
        }

        private void RefreshVisuals()
        {
            if (xRenderer == null || yRenderer == null || zRenderer == null)
            {
                return;
            }

            ConfigureAxis(xRenderer, Color.red, Vector3.right * axisLength);
            ConfigureAxis(yRenderer, Color.green, Vector3.up * axisLength);
            ConfigureAxis(zRenderer, Color.blue, Vector3.forward * axisLength);
        }

        private LineRenderer GetOrCreateAxisRenderer(string axisName, Color color)
        {
            var child = transform.Find(axisName);
            GameObject axisObject;
            if (child == null)
            {
                axisObject = new GameObject(axisName);
                axisObject.transform.SetParent(transform, false);
            }
            else
            {
                axisObject = child.gameObject;
            }

            var renderer = axisObject.GetComponent<LineRenderer>();
            if (renderer == null)
            {
                renderer = axisObject.AddComponent<LineRenderer>();
            }

            renderer.useWorldSpace = false;
            renderer.positionCount = 2;
            renderer.numCornerVertices = 2;
            renderer.numCapVertices = 2;
            renderer.loop = false;
            renderer.alignment = LineAlignment.TransformZ;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.generateLightingData = false;
            return renderer;
        }

        private void ConfigureAxis(LineRenderer renderer, Color color, Vector3 endPoint)
        {
            renderer.sharedMaterial = SharedLineMaterial.Get();
            renderer.startWidth = lineWidth;
            renderer.endWidth = lineWidth;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.SetPosition(0, Vector3.zero);
            renderer.SetPosition(1, endPoint);
        }
    }
}

