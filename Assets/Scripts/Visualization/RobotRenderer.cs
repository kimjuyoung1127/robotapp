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
        [SerializeField] private Transform donorEndEffectorSource;

        [Header("Visual Anchors")]
        [SerializeField] private Transform baseVisual;
        [SerializeField] private Transform link0Visual;
        [SerializeField] private Transform link1Visual;
        [SerializeField] private Transform endEffectorVisual;

        [Header("Display")]
        [SerializeField] private float frameAxisLength = 0.22f;
        [SerializeField] private float donorScale = 0.22f;
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

        private float link0SourceLength = 1.0f;
        private float link1SourceLength = 1.0f;

        public bool HasAllVisualAnchors => baseVisual != null && link0Visual != null && link1Visual != null && endEffectorVisual != null;
        public bool HasAllDonorSources => donorBaseSource != null && donorLink0Source != null && donorLink1Source != null && donorEndEffectorSource != null;

        private void Awake()
        {
            EnsureRig();
        }

        private void OnEnable()
        {
            BindController();
        }

        private void Start()
        {
            ApplyCurrentState();
        }

        private void OnDisable()
        {
            UnbindController();
        }

        private void OnValidate()
        {
            EnsureRig();
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

            baseVisual ??= EnsureVisualAnchor("BaseVisual", donorBaseSource, false);
            link0Visual ??= EnsureVisualAnchor("Link0Visual", donorLink0Source, true);
            link1Visual ??= EnsureVisualAnchor("Link1Visual", donorLink1Source, true);
            endEffectorVisual ??= EnsureVisualAnchor("EndEffectorVisualMesh", donorEndEffectorSource, false);
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

            var frame1Unity = CoordConverter.ToUnityPosition(frame1TransformValue.ExtractPosition());
            var eeUnity = CoordConverter.ToUnityPosition(endEffectorTransformValue.ExtractPosition());

            if (baseVisual != null)
            {
                baseVisual.localPosition = baseLocalOffset;
                baseVisual.localRotation = Quaternion.Euler(baseLocalEuler);
                baseVisual.localScale = Vector3.one * ResolveBaseScale();
            }

            UpdateSegmentVisual(link0Visual, Vector3.zero, frame1Unity, link0SourceLength, ResolveLink0Thickness(), link0LocalOffset, link0LocalEuler);
            UpdateSegmentVisual(link1Visual, frame1Unity, eeUnity, link1SourceLength, ResolveLink1Thickness(), link1LocalOffset, link1LocalEuler);

            if (endEffectorVisual != null)
            {
                endEffectorVisual.localPosition = eeUnity + endEffectorLocalOffset;
                endEffectorVisual.localRotation = CoordConverter.ToUnityRotation(endEffectorTransformValue.ExtractRotation()) * Quaternion.Euler(endEffectorLocalEuler);
                endEffectorVisual.localScale = Vector3.one * ResolveEndEffectorScale();
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
            donorEndEffectorSource ??= donorSourceRoot.Find("Base/Axis1/Axis2/Axis3/Gripper");

            link0SourceLength = GetSourceLength(donorLink0Source);
            link1SourceLength = GetSourceLength(donorLink1Source);
        }

        private Transform EnsureVisualAnchor(string childName, Transform source, bool alignToSegment)
        {
            var anchor = visualRoot.Find(childName);
            if (anchor == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(visualRoot, false);
                anchor = go.transform;
            }

            CopyMeshOnly(anchor.gameObject, source);
            anchor.localScale = Vector3.one * donorScale;

            if (!alignToSegment)
            {
                anchor.localRotation = Quaternion.identity;
            }

            return anchor;
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

        private static float GetSourceLength(Transform source)
        {
            if (source == null)
            {
                return 1.0f;
            }

            var filter = source.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                return 1.0f;
            }

            var size = filter.sharedMesh.bounds.size;
            var candidate = Mathf.Max(size.x, size.y, size.z);
            return Mathf.Max(0.01f, candidate);
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

        private void UpdateSegmentVisual(Transform visual, Vector3 start, Vector3 end, float sourceLength, float thicknessScale, Vector3 localOffset, Vector3 localEuler)
        {
            if (visual == null)
            {
                return;
            }

            var direction = end - start;
            var length = direction.magnitude;
            if (length < 1e-5f)
            {
                visual.gameObject.SetActive(false);
                return;
            }

            visual.gameObject.SetActive(true);
            visual.localPosition = (start + end) * 0.5f + localOffset;
            visual.localRotation = Quaternion.FromToRotation(Vector3.right, direction.normalized) * Quaternion.Euler(localEuler);
            visual.localScale = new Vector3(length / Mathf.Max(0.01f, sourceLength), thicknessScale, thicknessScale);
        }
    }
}
