// Folder: Visualization - Unity-side rendering and FK binding.
using System;
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.Visualization
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class RobotRenderer : MonoBehaviour
    {
        private const string Frame0Name = "frame_0";
        private const string Frame1Name = "frame_1";
        private const string FrameEeName = "Frame_EE";
        private const string LegacyWorldFrameName = "WorldFrame";
        private const string LegacyFrame1Name = "Frame_1";
        private const string DonorSourceName = "ScaraDonorProbe";
        private const string DonorFallbackName = "ScaraRobot";

        [Header("References")]
        [SerializeField] private AppController appController;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform frame0Transform;
        [SerializeField] private Transform frame1Transform;
        [SerializeField] private Transform frameEeTransform;
        [SerializeField] private FrameGizmo frame0Gizmo;
        [SerializeField] private FrameGizmo frame1Gizmo;
        [SerializeField] private FrameGizmo frameEeGizmo;

        [Header("Donor Source")]
        [SerializeField] private Transform donorSourceRoot;
        [SerializeField] private Transform donorBaseSource;
        [SerializeField] private Transform donorLink0Source;
        [SerializeField] private Transform donorLink1Source;
        [SerializeField] private Transform donorAxis3Source;
        [SerializeField] private Transform donorEndEffectorSource;
        [SerializeField] private Transform donorPickSource;

        [Header("Visual Anchors")]
        [SerializeField] private Transform baseVisual;
        [SerializeField] private Transform link0Visual;
        [SerializeField] private Transform link1Visual;
        [SerializeField] private Transform axis3Pivot;
        [SerializeField] private Transform endEffectorVisual;
        [SerializeField] private LinkHighlighter linkHighlighter;

        [Header("Display")]
        [SerializeField] private float frameAxisLength = 0.22f;
        [SerializeField] private float donorScale = 2.85f;
        [SerializeField] private float baseScale = 0.22f;
        [SerializeField] private float endEffectorScale = 0.22f;
        [SerializeField] private float segmentThicknessScale = 0.22f;
        [SerializeField] private float link0ThicknessScale = 0.22f;
        [SerializeField] private float link1ThicknessScale = 0.22f;
        [SerializeField] private Vector3 baseLocalOffset = new Vector3(0f, -0.04f, 0f);
        [SerializeField] private Vector3 baseLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 link0LocalOffset = Vector3.zero;
        [SerializeField] private Vector3 link0LocalEuler = Vector3.zero;
        [SerializeField] private Vector3 link1LocalOffset = Vector3.zero;
        [SerializeField] private Vector3 link1LocalEuler = Vector3.zero;
        [SerializeField] private Vector3 endEffectorLocalOffset = Vector3.zero;
        [SerializeField] private Vector3 endEffectorLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 baseLocalScale = Vector3.one;
        [SerializeField] private Vector3 link0LocalScale = Vector3.one;
        [SerializeField] private Vector3 link1LocalScale = Vector3.one;
        [SerializeField] private Vector3 endEffectorLocalScale = Vector3.one;

        public bool HasAllVisualAnchors => baseVisual != null && link0Visual != null && link1Visual != null && endEffectorVisual != null;
        public bool HasAllDonorSources => donorBaseSource != null && donorLink0Source != null && donorLink1Source != null && donorEndEffectorSource != null;
        public Transform DonorBaseSource => donorBaseSource;
        public Transform DonorLink0Source => donorLink0Source;
        public Transform DonorLink1Source => donorLink1Source;
        public Transform DonorEndEffectorSource => donorEndEffectorSource;
        public Transform DonorPickSource => donorPickSource;
        public bool UsesPickAsEndEffectorSource => donorEndEffectorSource != null && string.Equals(donorEndEffectorSource.name, "Pick", StringComparison.Ordinal);

        private void Awake()
        {
            EnsureRig();
        }

        private void OnEnable()
        {
            EnsureRig();
            BindController();

            if (!Application.isPlaying)
            {
                ApplyCurrentState();
            }
        }

        private void Start()
        {
            ApplyCurrentState();
        }

        private void OnDisable()
        {
            UnbindController();
        }

        private void BindController()
        {
            if (appController == null)
            {
                appController = FindFirstObjectByType<AppController>();
            }

            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appController.OnJointFocusRequested -= HandleJointFocusRequested;
                appController.OnJointFocusRequested += HandleJointFocusRequested;
                appController.OnJointFocusCleared -= HandleJointFocusCleared;
                appController.OnJointFocusCleared += HandleJointFocusCleared;
            }
        }

        private void UnbindController()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
                appController.OnJointFocusRequested -= HandleJointFocusRequested;
                appController.OnJointFocusCleared -= HandleJointFocusCleared;
            }
        }

        private void HandleKinematicsUpdated(Mat4D a1, Mat4D a2, Mat4D t02, TutorPose _pose)
        {
            ApplyTransforms(a1, a2, t02);
        }

        private void ApplyCurrentState()
        {
            EnsureRig();

            if (appController == null)
            {
                appController = FindFirstObjectByType<AppController>();
            }

            if (appController == null)
            {
                ApplyTransforms(Mat4D.Identity, Mat4D.Identity, Mat4D.Identity);
                return;
            }

            ApplyTransforms(appController.CurrentA1, appController.CurrentA2, appController.CurrentEndEffectorTransform);
        }

        private void EnsureRig()
        {
            visualRoot ??= RobotRigBinder.EnsureTransformChild(transform, "VisualRoot");
            visualRoot.localScale = Vector3.one * donorScale;

            frame0Transform ??= RobotRigBinder.FindSceneTransform(Frame0Name);
            frame1Transform ??= RobotRigBinder.FindSceneTransform(Frame1Name);
            frameEeTransform ??= RobotRigBinder.EnsureTransformChild(transform, FrameEeName);

            frame0Gizmo = RobotRigBinder.EnsureFrameGizmo(frame0Transform);
            frame1Gizmo = RobotRigBinder.EnsureFrameGizmo(frame1Transform);
            frameEeGizmo = RobotRigBinder.EnsureFrameGizmo(frameEeTransform);

            RobotRigBinder.HideLegacyMarker(frame0Transform);
            RobotRigBinder.HideLegacyMarker(frame1Transform);
            RobotRigBinder.DisableLegacyFrame(transform, LegacyWorldFrameName);
            RobotRigBinder.DisableLegacyFrame(transform, LegacyFrame1Name);
            RobotRigBinder.DisableLegacyVisual(visualRoot, "BaseJoint");
            RobotRigBinder.DisableLegacyVisual(visualRoot, "ElbowJoint");
            RobotRigBinder.DisableLegacyVisual(visualRoot, "Link0");
            RobotRigBinder.DisableLegacyVisual(visualRoot, "Link1");
            RobotRigBinder.DisableLegacyVisual(visualRoot, "EndEffectorVisual");

            donorSourceRoot ??= ScaraDonorMapper.ResolveDonorSource(visualRoot, DonorSourceName, DonorFallbackName);
            ScaraDonorMapper.CacheDonorParts(donorSourceRoot, ref donorBaseSource, ref donorLink0Source, ref donorLink1Source, ref donorAxis3Source, ref donorEndEffectorSource, ref donorPickSource);

            baseVisual = RobotRigBinder.NormalizeVisualReference(baseVisual, "BaseVisual");
            link0Visual = RobotRigBinder.NormalizeVisualReference(link0Visual, "Link0Visual");
            link1Visual = RobotRigBinder.NormalizeVisualReference(link1Visual, "Link1Visual");
            endEffectorVisual = RobotRigBinder.NormalizeVisualReference(endEffectorVisual, "EndEffectorVisualMesh");
            axis3Pivot = RobotRigBinder.NormalizeVisualReference(axis3Pivot, "Axis3Pivot");

            baseVisual ??= RobotRigBinder.EnsureVisualAnchor("BaseVisual", visualRoot, donorBaseSource);
            link0Visual ??= RobotRigBinder.EnsureVisualAnchor("Link0Visual", baseVisual, donorLink0Source);
            link1Visual ??= RobotRigBinder.EnsureVisualAnchor("Link1Visual", link0Visual, donorLink1Source);
            axis3Pivot ??= RobotRigBinder.EnsurePivotChild(link1Visual, "Axis3Pivot");
            endEffectorVisual ??= RobotRigBinder.EnsureVisualAnchor("EndEffectorVisualMesh", axis3Pivot, donorEndEffectorSource);

            if (link0Visual != null && link0Visual.parent != baseVisual)
            {
                link0Visual.SetParent(baseVisual, false);
            }

            if (link1Visual != null && link1Visual.parent != link0Visual)
            {
                link1Visual.SetParent(link0Visual, false);
            }

            if (axis3Pivot != null && axis3Pivot.parent != link1Visual)
            {
                axis3Pivot.SetParent(link1Visual, false);
            }

            if (endEffectorVisual != null && endEffectorVisual.parent != axis3Pivot)
            {
                endEffectorVisual.SetParent(axis3Pivot, false);
            }

            linkHighlighter ??= GetComponent<LinkHighlighter>() ?? gameObject.AddComponent<LinkHighlighter>();
            linkHighlighter.Configure(baseVisual, link0Visual, axis3Pivot, endEffectorVisual);
        }

        private void ApplyTransforms(Mat4D frame1TransformValue, Mat4D a2TransformValue, Mat4D endEffectorTransformValue)
        {
            EnsureRig();

            frame0Gizmo?.SetPose(Mat4D.Identity);
            frame0Gizmo?.SetLength(frameAxisLength);
            frame1Gizmo?.SetPose(frame1TransformValue);
            frame1Gizmo?.SetLength(frameAxisLength);
            frameEeGizmo?.SetPose(endEffectorTransformValue);
            frameEeGizmo?.SetLength(frameAxisLength * 1.15f);
            visualRoot.localScale = Vector3.one * donorScale;

            var jointValues = appController != null ? appController.CurrentJointValuesRad : Array.Empty<double>();
            var joint0Degrees = jointValues.Length > 0 ? (float)(jointValues[0] * Mathf.Rad2Deg) : 0f;
            var joint1Degrees = jointValues.Length > 1 ? (float)(jointValues[1] * Mathf.Rad2Deg) : 0f;
            var joint2Degrees = jointValues.Length > 2 ? (float)(jointValues[2] * Mathf.Rad2Deg) : 0f;
            var joint3Degrees = jointValues.Length > 3 ? (float)(jointValues[3] * Mathf.Rad2Deg) : 0f;

            if (baseVisual != null)
            {
                baseVisual.localPosition = baseLocalOffset;
                baseVisual.localRotation = Quaternion.Euler(baseLocalEuler);
                baseVisual.localScale = ResolveScale(baseLocalScale, ResolveBaseScale());
            }

            ApplyJointVisual(link0Visual, donorLink0Source, joint0Degrees, link0LocalOffset, link0LocalEuler, ResolveScale(link0LocalScale, ResolveLink0Thickness()));
            ApplyJointVisual(link1Visual, donorLink1Source, joint1Degrees, link1LocalOffset, link1LocalEuler, ResolveScale(link1LocalScale, ResolveLink1Thickness()));

            if (axis3Pivot != null)
            {
                axis3Pivot.localPosition = donorAxis3Source != null ? donorAxis3Source.localPosition : Vector3.zero;
                axis3Pivot.localRotation = (donorAxis3Source != null ? donorAxis3Source.localRotation : Quaternion.identity) *
                    Quaternion.AngleAxis(-joint2Degrees, Vector3.up);
                axis3Pivot.localScale = donorAxis3Source != null ? donorAxis3Source.localScale : Vector3.one;
            }

            if (endEffectorVisual != null)
            {
                endEffectorVisual.localPosition = (donorEndEffectorSource != null ? donorEndEffectorSource.localPosition : Vector3.zero) + endEffectorLocalOffset;
                endEffectorVisual.localRotation = (donorEndEffectorSource != null ? donorEndEffectorSource.localRotation : Quaternion.identity) *
                    Quaternion.AngleAxis(-joint3Degrees, Vector3.up) *
                    Quaternion.Euler(endEffectorLocalEuler);
                endEffectorVisual.localScale = ResolveScale(endEffectorLocalScale, ResolveEndEffectorScale());
            }
        }

        public Bounds GetAggregateVisualBounds()
        {
            EnsureRig();
            return RobotVisibilityProbe.GetAggregateVisualBounds(this);
        }

        public bool IsVisibleFrom(Camera camera)
        {
            return RobotVisibilityProbe.IsVisibleFrom(this, camera);
        }

        public void HighlightJoint(int jointIndex)
        {
            EnsureRig();
            linkHighlighter?.HighlightJoint(jointIndex);
        }

        public void ClearJointHighlight()
        {
            linkHighlighter?.ClearHighlight();
        }

        private float ResolveBaseScale()
        {
            return baseScale > 0f ? baseScale : donorScale;
        }

        private float ResolveEndEffectorScale()
        {
            return endEffectorScale > 0f ? endEffectorScale : donorScale;
        }

        private float ResolveLink0Thickness()
        {
            return link0ThicknessScale > 0f ? link0ThicknessScale : segmentThicknessScale;
        }

        private float ResolveLink1Thickness()
        {
            return link1ThicknessScale > 0f ? link1ThicknessScale : segmentThicknessScale;
        }

        private static Vector3 ResolveScale(Vector3 serializedScale, float fallbackScalar)
        {
            if (serializedScale.sqrMagnitude < 1e-6f)
            {
                return Vector3.one * fallbackScalar;
            }

            return serializedScale;
        }

        private static void ApplyJointVisual(Transform visual, Transform source, float jointAngleDegrees, Vector3 offset, Vector3 eulerOffset, Vector3 localScale)
        {
            if (visual == null)
            {
                return;
            }

            visual.gameObject.SetActive(true);
            visual.localPosition = (source != null ? source.localPosition : Vector3.zero) + offset;
            visual.localRotation = (source != null ? source.localRotation : Quaternion.identity) * Quaternion.AngleAxis(-jointAngleDegrees, Vector3.up) * Quaternion.Euler(eulerOffset);
            visual.localScale = localScale;
        }

        private void HandleJointFocusRequested(int jointIndex)
        {
            HighlightJoint(jointIndex);
        }

        private void HandleJointFocusCleared()
        {
            ClearJointHighlight();
        }
    }
}
