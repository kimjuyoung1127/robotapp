using System;
using KineTutor3D.App;
using KineTutor3D.Math;
using UnityEngine;
using UnityEngine.Rendering;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// Applies FK results to canonical frame objects and mesh-only donor visuals.
    /// </summary>
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
            }
        }

        private void UnbindController()
        {
            if (appController != null)
            {
                appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
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
            visualRoot ??= EnsureTransformChild("VisualRoot");
            visualRoot.localScale = Vector3.one * donorScale;

            frame0Transform ??= FindSceneTransform(Frame0Name);
            frame1Transform ??= FindSceneTransform(Frame1Name);
            frameEeTransform ??= EnsureTransformChild(FrameEeName);

            frame0Gizmo = EnsureFrameGizmo(frame0Transform);
            frame1Gizmo = EnsureFrameGizmo(frame1Transform);
            frameEeGizmo = EnsureFrameGizmo(frameEeTransform);

            HideLegacyMarker(frame0Transform);
            HideLegacyMarker(frame1Transform);
            DisableLegacyFrame(LegacyWorldFrameName);
            DisableLegacyFrame(LegacyFrame1Name);
            DisableLegacyVisual("BaseJoint");
            DisableLegacyVisual("ElbowJoint");
            DisableLegacyVisual("Link0");
            DisableLegacyVisual("Link1");
            DisableLegacyVisual("EndEffectorVisual");

            donorSourceRoot ??= ResolveDonorSource();
            CacheDonorParts();

            baseVisual = NormalizeVisualReference(baseVisual, "BaseVisual");
            link0Visual = NormalizeVisualReference(link0Visual, "Link0Visual");
            link1Visual = NormalizeVisualReference(link1Visual, "Link1Visual");
            endEffectorVisual = NormalizeVisualReference(endEffectorVisual, "EndEffectorVisualMesh");
            axis3Pivot = NormalizeVisualReference(axis3Pivot, "Axis3Pivot");

            baseVisual ??= EnsureVisualAnchor("BaseVisual", visualRoot, donorBaseSource);
            link0Visual ??= EnsureVisualAnchor("Link0Visual", baseVisual, donorLink0Source);
            link1Visual ??= EnsureVisualAnchor("Link1Visual", link0Visual, donorLink1Source);
            axis3Pivot ??= EnsurePivotChild(link1Visual, "Axis3Pivot");
            endEffectorVisual ??= EnsureVisualAnchor("EndEffectorVisualMesh", axis3Pivot, donorEndEffectorSource);

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
                axis3Pivot.localRotation = donorAxis3Source != null ? donorAxis3Source.localRotation : Quaternion.identity;
                axis3Pivot.localScale = donorAxis3Source != null ? donorAxis3Source.localScale : Vector3.one;
            }

            if (endEffectorVisual != null)
            {
                endEffectorVisual.localPosition = (donorEndEffectorSource != null ? donorEndEffectorSource.localPosition : Vector3.zero) + endEffectorLocalOffset;
                endEffectorVisual.localRotation = (donorEndEffectorSource != null ? donorEndEffectorSource.localRotation : Quaternion.identity) * Quaternion.Euler(endEffectorLocalEuler);
                endEffectorVisual.localScale = ResolveScale(endEffectorLocalScale, ResolveEndEffectorScale());
            }
        }

        private Transform EnsureTransformChild(string childName)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(transform, false);
                child = go.transform;
            }

            return child;
        }

        private static Transform FindSceneTransform(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                return go.transform;
            }

            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.name == objectName)
                {
                    return candidate.transform;
                }
            }

            return null;
        }

        private static FrameGizmo EnsureFrameGizmo(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            var gizmo = target.GetComponent<FrameGizmo>();
            if (gizmo == null)
            {
                gizmo = target.gameObject.AddComponent<FrameGizmo>();
            }

            return gizmo;
        }

        private static void HideLegacyMarker(Transform frameTransform)
        {
            if (frameTransform == null)
            {
                return;
            }

            var renderer = frameTransform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private void DisableLegacyFrame(string childName)
        {
            var legacy = transform.Find(childName);
            if (legacy != null && legacy.gameObject.activeSelf)
            {
                legacy.gameObject.SetActive(false);
            }
        }

        private void DisableLegacyVisual(string childName)
        {
            if (visualRoot == null)
            {
                return;
            }

            var legacy = visualRoot.Find(childName);
            if (legacy != null && legacy.gameObject.activeSelf)
            {
                legacy.gameObject.SetActive(false);
            }
        }

        private Transform ResolveDonorSource()
        {
            var donor = FindSceneTransform(DonorSourceName) ?? FindSceneTransform(DonorFallbackName);
            if (donor == null)
            {
                return null;
            }

            donor.SetParent(visualRoot, false);
            donor.localPosition = Vector3.zero;
            donor.localRotation = Quaternion.identity;
            donor.localScale = Vector3.one;

            DisableRuntimeComponents(donor);
            donor.gameObject.SetActive(false);
            return donor;
        }

        private void CacheDonorParts()
        {
            if (donorSourceRoot == null)
            {
                return;
            }

            donorBaseSource ??= donorSourceRoot.Find("Base");
            donorLink0Source ??= donorSourceRoot.Find("Base/Axis1");
            donorLink1Source ??= donorSourceRoot.Find("Base/Axis1/Axis2");
            donorAxis3Source ??= donorSourceRoot.Find("Base/Axis1/Axis2/Axis3");
            donorEndEffectorSource ??= donorAxis3Source != null
                ? donorAxis3Source.Find("Gripper")
                : donorSourceRoot.Find("Base/Axis1/Axis2/Axis3/Gripper");
            donorPickSource ??= donorSourceRoot.Find("Pick");
        }

        private Transform EnsureVisualAnchor(string childName, Transform parent, Transform source)
        {
            if (parent == null)
            {
                return null;
            }

            var anchor = parent.Find(childName);
            if (anchor == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                anchor = go.transform;
            }

            CopyMeshOnly(anchor.gameObject, source);
            return anchor;
        }

        private static Transform EnsurePivotChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                child = go.transform;
            }

            return child;
        }

        private Transform NormalizeVisualReference(Transform current, string expectedName)
        {
            if (current == null)
            {
                return null;
            }

            return current.name == expectedName ? current : null;
        }

        private static void CopyMeshOnly(GameObject target, Transform source)
        {
            if (target == null || source == null)
            {
                return;
            }

            var sourceFilter = source.GetComponent<MeshFilter>();
            var sourceRenderer = source.GetComponent<MeshRenderer>();

            if (sourceFilter != null)
            {
                var targetFilter = target.GetComponent<MeshFilter>();
                if (targetFilter == null)
                {
                    targetFilter = target.AddComponent<MeshFilter>();
                }

                targetFilter.sharedMesh = sourceFilter.sharedMesh;
            }

            if (sourceRenderer != null)
            {
                var targetRenderer = target.GetComponent<MeshRenderer>();
                if (targetRenderer == null)
                {
                    targetRenderer = target.AddComponent<MeshRenderer>();
                }

                targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
            }
        }

        private static void DisableRuntimeComponents(Transform donorRoot)
        {
            foreach (var behaviour in donorRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }

            foreach (var rigidbody in donorRoot.GetComponentsInChildren<Rigidbody>(true))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.detectCollisions = false;
            }

            foreach (var collider in donorRoot.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        public Bounds GetAggregateVisualBounds()
        {
            EnsureRig();

            var renderers = GetComponentsInChildren<MeshRenderer>(true);
            var hasBounds = false;
            var aggregate = new Bounds(transform.position, Vector3.zero);

            foreach (var renderer in renderers)
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    aggregate = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    aggregate.Encapsulate(renderer.bounds);
                }
            }

            return aggregate;
        }

        public bool IsVisibleFrom(Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            var bounds = GetAggregateVisualBounds();
            if (bounds.size.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
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
            visual.localRotation = (source != null ? source.localRotation : Quaternion.identity)
                * Quaternion.AngleAxis(jointAngleDegrees, Vector3.up)
                * Quaternion.Euler(eulerOffset);
            visual.localScale = localScale;
        }
    }
}
