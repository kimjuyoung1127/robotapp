// Folder: Visualization - 단일 로봇 3D 프리뷰 유닛.
using System;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 단일 로봇의 3D 프리뷰를 관리하는 컴포넌트입니다.
    /// 페데스탈, 이름 라벨, 턴테이블 회전과 라이브러리용 포즈 제어를 담당합니다.
    /// </summary>
    public class RobotPreviewPod : MonoBehaviour
    {
        private const float TurntableSpeed = 15f;
        private const float SelectedScale = 1.24f;
        private const float PreviewScale = 0.82f;
        private const float MillimeterToUnity = 0.001f;

        private enum PoseControlMode
        {
            None,
            TransformTargets,
            FairinoUrdf
        }

        private readonly struct JointControlSpec
        {
            public readonly float Min;
            public readonly float Max;
            public readonly bool IsRotational;

            public JointControlSpec(float min, float max, bool isRotational)
            {
                Min = min;
                Max = max;
                IsRotational = isRotational;
            }
        }

        private RobotMetadataInfo metadata;
        private GameObject meshRoot;
        private GameObject pedestal;
        private TextMesh nameLabel;
        private BoxCollider hitCollider;
        private Transform[] poseTargets = Array.Empty<Transform>();
        private Quaternion[] initialPoseRotations = Array.Empty<Quaternion>();
        private Vector3[] initialLocalPositions = Array.Empty<Vector3>();
        private Vector3[] poseAxes = Array.Empty<Vector3>();
        private JointControlSpec[] controlSpecs = Array.Empty<JointControlSpec>();
        private FairinoUrdfJointDriver fairinoPoseDriver;
        private PoseControlMode poseControlMode;
        private int poseDof;
        private bool selected;
        private bool initialized;

        /// <summary>로봇 ID를 반환합니다.</summary>
        public string RobotId => initialized ? metadata.RobotId : string.Empty;

        /// <summary>선택 상태를 반환합니다.</summary>
        public bool IsSelected => selected;

        /// <summary>라이브러리 안에서 포즈 제어가 가능한지 반환합니다.</summary>
        public bool SupportsPoseControl => poseControlMode != PoseControlMode.None;

        /// <summary>현재 preview pod가 지원하는 포즈 자유도입니다.</summary>
        public int PoseDof => poseDof;

        public void Initialize(RobotMetadataInfo meta, GameObject createdMeshRoot, bool showLabel)
        {
            metadata = meta;
            meshRoot = createdMeshRoot;
            initialized = true;

            if (meshRoot != null)
            {
                meshRoot.transform.SetParent(transform, false);
            }

            BuildPedestal();
            if (showLabel)
            {
                BuildNameLabel();
            }

            CachePoseControl();
            EnsureHitCollider();
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            float scale = isSelected ? SelectedScale : PreviewScale;
            transform.localScale = new Vector3(scale, scale, scale);
            EnsureHitCollider();
        }

        public void SetPose(double[] jointsDeg)
        {
            if (meshRoot == null || jointsDeg == null)
            {
                return;
            }

            if (poseControlMode == PoseControlMode.FairinoUrdf && fairinoPoseDriver != null)
            {
                fairinoPoseDriver.ApplyJointAngles(jointsDeg);
                EnsureHitCollider();
                return;
            }

            var count = Mathf.Min(jointsDeg.Length, poseTargets.Length);
            for (var i = 0; i < count; i++)
            {
                var target = poseTargets[i];
                if (target == null)
                {
                    continue;
                }

                var rawValue = (!double.IsNaN(jointsDeg[i]) && !double.IsInfinity(jointsDeg[i])) ? (float)jointsDeg[i] : 0f;
                if (i < controlSpecs.Length && !controlSpecs[i].IsRotational)
                {
                    target.localPosition = initialLocalPositions[i] + poseAxes[i] * (rawValue * MillimeterToUnity);
                }
                else
                {
                    target.localRotation = initialPoseRotations[i] * Quaternion.AngleAxis(rawValue, poseAxes[i]);
                }
            }

            EnsureHitCollider();
        }

        public void GetControlSpecs(out float[] minLimits, out float[] maxLimits, out bool[] isRotational)
        {
            minLimits = new float[controlSpecs.Length];
            maxLimits = new float[controlSpecs.Length];
            isRotational = new bool[controlSpecs.Length];

            for (var i = 0; i < controlSpecs.Length; i++)
            {
                minLimits[i] = controlSpecs[i].Min;
                maxLimits[i] = controlSpecs[i].Max;
                isRotational[i] = controlSpecs[i].IsRotational;
            }
        }

        public void SetMeshVisible(bool visible)
        {
            if (meshRoot == null)
            {
                return;
            }

            foreach (var renderer in meshRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = visible;
            }
        }

        public void Dispose()
        {
            if (meshRoot != null)
            {
                SafeDestroy(meshRoot);
                meshRoot = null;
            }

            if (pedestal != null)
            {
                SafeDestroy(pedestal);
                pedestal = null;
            }

            initialized = false;
            SafeDestroy(gameObject);
        }

        private void Update()
        {
            if (!selected || meshRoot == null)
            {
                return;
            }

            meshRoot.transform.Rotate(Vector3.up, TurntableSpeed * Time.deltaTime, Space.Self);
        }

        private void BuildPedestal()
        {
            pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(transform, false);

            float radius = UIDesignTokens.Size.PedestalRadius;
            float height = UIDesignTokens.Size.PedestalHeight;
            pedestal.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            pedestal.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f);

            var renderer = pedestal.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = UIDesignTokens.Colors.PedestalSurface
                };
            }

            var collider = pedestal.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private void BuildNameLabel()
        {
            var labelGo = new GameObject("NameLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, -0.15f, 0f);

            nameLabel = labelGo.AddComponent<TextMesh>();
            nameLabel.text = metadata.DisplayName;
            nameLabel.fontSize = 32;
            nameLabel.characterSize = 0.02f;
            nameLabel.anchor = TextAnchor.UpperCenter;
            nameLabel.alignment = TextAlignment.Center;
            nameLabel.color = UIDesignTokens.Colors.TextPrimary;
        }

        private void CachePoseControl()
        {
            poseTargets = Array.Empty<Transform>();
            initialPoseRotations = Array.Empty<Quaternion>();
            initialLocalPositions = Array.Empty<Vector3>();
            poseAxes = Array.Empty<Vector3>();
            controlSpecs = Array.Empty<JointControlSpec>();
            fairinoPoseDriver = null;
            poseControlMode = PoseControlMode.None;
            poseDof = 0;

            if (meshRoot == null)
            {
                return;
            }

            var fairinoBaseLink = FindChildRecursive(meshRoot.transform, "base_link");
            if (fairinoBaseLink != null && metadata.EffectivePreviewRobotId == "FAIRINO_FR5")
            {
                fairinoPoseDriver = meshRoot.GetComponent<FairinoUrdfJointDriver>() ?? meshRoot.AddComponent<FairinoUrdfJointDriver>();
                fairinoPoseDriver.Inject(fairinoBaseLink);

                var templateSpecs = BuildTemplateSpecs(metadata);
                controlSpecs = templateSpecs;
                poseDof = templateSpecs.Length;
                poseControlMode = PoseControlMode.FairinoUrdf;
                return;
            }

            if (TryCacheJointPivotChain())
            {
                poseControlMode = PoseControlMode.TransformTargets;
                return;
            }

            if (TryCacheNamedAxisChain())
            {
                poseControlMode = PoseControlMode.TransformTargets;
            }
        }

        private bool TryCacheJointPivotChain()
        {
            var dof = Mathf.Max(1, metadata.Dof);
            var targets = new Transform[dof];
            for (var i = 0; i < dof; i++)
            {
                var pivot = meshRoot.transform.Find($"JointPivot{i}");
                if (pivot == null)
                {
                    return false;
                }

                targets[i] = pivot;
            }

            var templateSpecs = BuildTemplateSpecs(metadata);
            var axes = new Vector3[dof];
            for (var i = 0; i < dof; i++)
            {
                axes[i] = Vector3.forward;
            }

            CacheTargets(targets, axes, templateSpecs);
            return true;
        }

        private bool TryCacheNamedAxisChain()
        {
            var targetPaths = BuildTargetPaths();
            if (targetPaths.Length == 0)
            {
                return false;
            }

            var templateSpecs = BuildTemplateSpecs(metadata);
            var targets = new Transform[targetPaths.Length];
            var axes = new Vector3[targetPaths.Length];
            var specs = new JointControlSpec[targetPaths.Length];

            for (var i = 0; i < targetPaths.Length; i++)
            {
                var target = FindPathRecursive(meshRoot.transform, targetPaths[i]);
                if (target == null)
                {
                    return false;
                }

                targets[i] = target;
                axes[i] = ResolveFallbackAxis(metadata.EffectivePreviewRobotId, i);
                specs[i] = i < templateSpecs.Length
                    ? templateSpecs[i]
                    : new JointControlSpec(-180f, 180f, true);
            }

            CacheTargets(targets, axes, specs);
            return true;
        }

        private void CacheTargets(Transform[] targets, Vector3[] axes, JointControlSpec[] specs)
        {
            poseTargets = targets;
            poseAxes = axes;
            controlSpecs = specs;
            poseDof = targets.Length;
            initialPoseRotations = new Quaternion[targets.Length];
            initialLocalPositions = new Vector3[targets.Length];

            for (var i = 0; i < targets.Length; i++)
            {
                initialPoseRotations[i] = targets[i] != null ? targets[i].localRotation : Quaternion.identity;
                initialLocalPositions[i] = targets[i] != null ? targets[i].localPosition : Vector3.zero;
            }
        }

        private void EnsureHitCollider()
        {
            hitCollider = GetComponent<BoxCollider>();
            if (hitCollider == null)
            {
                hitCollider = gameObject.AddComponent<BoxCollider>();
            }

            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                hitCollider.center = Vector3.zero;
                hitCollider.size = Vector3.one;
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var localCenter = transform.InverseTransformPoint(bounds.center);
            var scaledSize = new Vector3(
                bounds.size.x / Mathf.Max(0.0001f, transform.lossyScale.x),
                bounds.size.y / Mathf.Max(0.0001f, transform.lossyScale.y),
                bounds.size.z / Mathf.Max(0.0001f, transform.lossyScale.z));

            hitCollider.center = localCenter;
            hitCollider.size = scaledSize + new Vector3(0.2f, 0.2f, 0.2f);
        }

        private string[] BuildTargetPaths()
        {
            var previewRobotId = metadata.EffectivePreviewRobotId;
            if (previewRobotId == "SCARA_RV")
            {
                return new[] { "Axis1", "Axis1/Axis2", "Axis1/Axis2/Axis3", "Axis1/Axis2/Axis3/Gripper" };
            }

            var dof = Mathf.Max(1, metadata.Dof);
            var prefix = previewRobotId == "FANUC_CRX10" ? "A" : "Axis";
            var paths = new string[dof];
            for (var i = 0; i < dof; i++)
            {
                paths[i] = BuildChainedPath(prefix, i + 1);
            }

            return paths;
        }

        private static string BuildChainedPath(string prefix, int index)
        {
            var parts = new string[index];
            for (var i = 0; i < index; i++)
            {
                parts[i] = $"{prefix}{i + 1}";
            }

            return string.Join("/", parts);
        }

        private static JointControlSpec[] BuildTemplateSpecs(RobotMetadataInfo metadata)
        {
            var template = RobotCatalog.CreateTemplate(metadata.RobotId)
                ?? RobotCatalog.CreateTemplate(metadata.EffectivePreviewRobotId);
            if (template == null)
            {
                var fallback = new JointControlSpec[Mathf.Max(1, metadata.Dof)];
                for (var i = 0; i < fallback.Length; i++)
                {
                    fallback[i] = new JointControlSpec(-180f, 180f, true);
                }

                return fallback;
            }

            var specs = new JointControlSpec[template.Dof];
            for (var i = 0; i < template.Dof; i++)
            {
                var limit = template.GetJointLimit(i);
                var link = template.GetLink(i);
                var isRotational = link.JointType != JointType.Prismatic;
                specs[i] = new JointControlSpec((float)limit.Min * Mathf.Rad2Deg, (float)limit.Max * Mathf.Rad2Deg, isRotational);
            }

            return specs;
        }

        private static Vector3 ResolveFallbackAxis(string robotId, int jointIndex)
        {
            if (robotId == "SCARA_RV")
            {
                return jointIndex == 2 ? Vector3.up : Vector3.up;
            }

            return Vector3.forward;
        }

        private static Transform FindPathRecursive(Transform root, string path)
        {
            if (root == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var direct = root.Find(path);
            if (direct != null)
            {
                return direct;
            }

            var segments = path.Split('/');
            return FindSegmentRecursive(root, segments, 0);
        }

        private static Transform FindSegmentRecursive(Transform current, string[] segments, int index)
        {
            if (current == null || segments == null || index >= segments.Length)
            {
                return current;
            }

            for (var i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                if (!string.Equals(child.name, segments[index], StringComparison.Ordinal))
                {
                    continue;
                }

                if (index == segments.Length - 1)
                {
                    return child;
                }

                var found = FindSegmentRecursive(child, segments, index + 1);
                if (found != null)
                {
                    return found;
                }
            }

            for (var i = 0; i < current.childCount; i++)
            {
                var found = FindSegmentRecursive(current.GetChild(i), segments, index);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            var direct = parent.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SafeDestroy(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
