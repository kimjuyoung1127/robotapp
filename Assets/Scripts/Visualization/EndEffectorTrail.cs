// Folder: Visualization - Unity-side rendering and FK binding.
using System.Collections.Generic;
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 끝점 이동 경로를 라인으로 기록하는 공통 trail 컴포넌트입니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class EndEffectorTrail : MonoBehaviour
    {
        private static Material sharedTrailMaterial;

        [SerializeField] private AppController appController;
        [SerializeField] private Transform trackedTransform;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int maxPoints = 64;
        [SerializeField] private float minDistance = 0.025f;
        [SerializeField] private bool trailVisible;

        private readonly List<Vector3> points = new List<Vector3>();
        private bool hasRecordedMotion;

        public int PointCount => points.Count;
        public bool IsTrailVisible => trailVisible;

        private void Awake()
        {
            EnsureRenderer();
        }

        private void OnEnable()
        {
            EnsureRenderer();
            ApplyVisibility();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;
            trackedTransform ??= transform;

            if (appController != null)
            {
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appController.OnStepChanged += HandleStepChanged;
            }

            EnsureRenderer();
            ApplyVisibility();
        }

        public void SetTrailVisible(bool visible)
        {
            trailVisible = visible;
            if (!visible)
            {
                ClearTrail();
            }

            ApplyVisibility();
        }

        public void ClearTrail()
        {
            points.Clear();
            hasRecordedMotion = false;
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
        }

        private void HandleKinematicsUpdated(Mat4D _a1, Mat4D _a2, Mat4D _t02, TutorPose _pose)
        {
            if (!trailVisible || appController == null || appController.LastUpdateCause != RuntimeUpdateCause.JointAngleChange)
            {
                return;
            }

            if (trackedTransform == null)
            {
                return;
            }

            var currentPoint = trackedTransform.position;
            if (!hasRecordedMotion)
            {
                AppendPoint(currentPoint, force: true);
                hasRecordedMotion = true;
                return;
            }

            AppendPoint(currentPoint, force: false);
        }

        private void HandleStepChanged(int _step, UI.Data.TutorStepConfig _config)
        {
            ClearTrail();
        }

        private void EnsureRenderer()
        {
            lineRenderer ??= GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = 0.025f;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.sharedMaterial = GetSharedTrailMaterial();
            lineRenderer.startColor = new Color(0.29f, 0.56f, 0.85f, 0.95f);
            lineRenderer.endColor = new Color(0.95f, 0.77f, 0.15f, 0.95f);
            lineRenderer.numCapVertices = 4;
            lineRenderer.numCornerVertices = 4;
        }

        private static Material GetSharedTrailMaterial()
        {
            if (sharedTrailMaterial != null)
            {
                return sharedTrailMaterial;
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedTrailMaterial = new Material(shader)
            {
                name = "KineTutor3D_EndEffectorTrail"
            };
            return sharedTrailMaterial;
        }

        private void AppendCurrentPoint(bool force)
        {
            if (!trailVisible || trackedTransform == null || lineRenderer == null)
            {
                return;
            }

            AppendPoint(trackedTransform.position, force);
        }

        private void AppendPoint(Vector3 point, bool force)
        {
            if (lineRenderer == null)
            {
                return;
            }

            if (!force && points.Count > 0 && Vector3.Distance(points[points.Count - 1], point) < minDistance)
            {
                return;
            }

            points.Add(point);
            while (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        private void ApplyVisibility()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = trailVisible;
            }
        }

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnStepChanged -= HandleStepChanged;
            }
        }
    }
}
