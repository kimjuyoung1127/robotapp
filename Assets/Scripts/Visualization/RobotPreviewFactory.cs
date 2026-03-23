// Folder: Visualization - 로봇 프리뷰 pod 생성 팩토리.
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// RobotPreviewPod를 생성하고 정리하는 정적 팩토리입니다.
    /// DonorMesh, 2DOF procedural, 6DOF articulated preview를 생성합니다.
    /// </summary>
    public static class RobotPreviewFactory
    {
        private struct DonorPreviewPose
        {
            public Vector3 LocalEuler;
            public Vector3 LocalOffset;
            public float TargetMaxSize;
            public Vector3 RootEuler;
            public string AnchorPath;
            public Vector3 AnchorEuler;
            public Vector3 AnchorOffset;
        }

        private struct SixDofPreviewPreset
        {
            public float BaseWidth;
            public float BaseHeight;
            public float ShoulderOffset;
            public float UpperArmLength;
            public float UpperArmThickness;
            public float ForearmLength;
            public float ForearmThickness;
            public float WristLength;
            public float WristThickness;
            public float ToolLength;
            public Color AccentColor;
            public Color BodyColor;
        }

        private static GameObject cachedScaraDonor;

        public static RobotPreviewPod CreatePod(Transform parent, RobotCatalogEntry entry, bool showLabel)
        {
            if (entry == null || parent == null)
            {
                return null;
            }

            var meta = entry.Metadata;
            var podGo = new GameObject($"Pod_{meta.RobotId}");
            podGo.transform.SetParent(parent, false);

            var pod = podGo.AddComponent<RobotPreviewPod>();
            var meshRoot = BuildMeshForLevel(meta);
            pod.Initialize(meta, meshRoot, showLabel);

            if (meta.DemoPoseDeg != null && meta.DemoPoseDeg.Length > 0)
            {
                pod.SetPose(meta.DemoPoseDeg);
            }

            return pod;
        }

        public static void DisposePod(RobotPreviewPod pod)
        {
            if (pod == null)
            {
                return;
            }

            pod.Dispose();
        }

        private static GameObject BuildMeshForLevel(RobotMetadataInfo meta)
        {
            var previewRobotId = meta.EffectivePreviewRobotId;
            if (string.IsNullOrWhiteSpace(meta.RobotId))
            {
                return BuildUnknownPreview();
            }

            if (meta.VisualizationLevel == "DonorMesh")
            {
                return BuildDonorMeshPreview(meta);
            }

            if (previewRobotId == "2DOF_RR")
            {
                return BuildProcedural2DofArm(meta);
            }

            if (meta.Dof >= 6)
            {
                return BuildProceduralArticulatedArm(meta);
            }

            return BuildUnknownPreview();
        }

        private static GameObject BuildDonorMeshPreview(RobotMetadataInfo meta)
        {
            if (!Application.isPlaying)
            {
                return BuildEditorSafePreview(meta);
            }

            var donorPrefab = LoadDonorPrefab(meta);
            if (donorPrefab == null)
            {
                Debug.LogWarning($"RobotPreviewFactory could not load donor prefab for '{meta.RobotId}'. Falling back to procedural preview.");
                return BuildEditorSafePreview(meta);
            }

            var root = new GameObject($"DonorMesh_{meta.RobotId}");
            CopyDonorHierarchy(donorPrefab.transform, root.transform);
            ApplyDonorPreviewPose(meta, root.transform);
            NormalizePreview(root, GetDonorPreviewPose(meta).TargetMaxSize);
            root.transform.localRotation = Quaternion.Euler(GetDonorPreviewPose(meta).RootEuler);
            return root;
        }

        private static GameObject BuildEditorSafePreview(RobotMetadataInfo meta)
        {
            var previewRobotId = meta.EffectivePreviewRobotId;
            if (previewRobotId == "SCARA_RV")
            {
                return BuildProcedural2DofArm(meta);
            }

            if (meta.Dof >= 6 || previewRobotId == "FAIRINO_FR5" || previewRobotId == "UR5e" || previewRobotId == "DOOSAN_M1013" || previewRobotId == "MECA500")
            {
                return BuildProceduralArticulatedArm(meta);
            }

            return BuildUnknownPreview();
        }

        private static GameObject LoadDonorPrefab(RobotMetadataInfo meta)
        {
            var previewRobotId = meta.EffectivePreviewRobotId;
            if (previewRobotId == "SCARA_RV")
            {
                if (cachedScaraDonor == null)
                {
                    cachedScaraDonor = Resources.Load<GameObject>("Robots/ScaraRobot");
                }

                return cachedScaraDonor;
            }

            if (previewRobotId == "FANUC_CRX10")
            {
                return Resources.Load<GameObject>("Robots/FanucCRX-10iA_L");
            }

            if (previewRobotId == "IGUS_REBEL")
            {
                return Resources.Load<GameObject>("Robots/igusRebel");
            }

            if (previewRobotId == "FAIRINO_FR5")
            {
                return Resources.Load<GameObject>("Robots/FAIRINO_FR5");
            }

            if (previewRobotId == "UR5e")
            {
                return Resources.Load<GameObject>("Robots/UR5e/UR5e");
            }

            if (previewRobotId == "DOOSAN_M1013")
            {
                return Resources.Load<GameObject>("Robots/DoosanM1013/DoosanM1013");
            }

            if (previewRobotId == "MECA500")
            {
                return Resources.Load<GameObject>("Robots/Meca500/Meca500");
            }

            return null;
        }

        private static GameObject BuildProcedural2DofArm(RobotMetadataInfo meta)
        {
            var root = new GameObject($"Procedural2Dof_{meta.RobotId}");
            var presetColor = UIDesignTokens.Colors.AccentPrimary;

            var baseCyl = CreatePrimitive(
                PrimitiveType.Cylinder,
                "Base",
                root.transform,
                new Vector3(0.18f, 0.04f, 0.18f),
                new Vector3(0f, 0.04f, 0f),
                presetColor);

            var link0 = CreatePrimitive(
                PrimitiveType.Cylinder,
                "Link0",
                root.transform,
                new Vector3(0.05f, 0.35f, 0.05f),
                new Vector3(0f, 0.38f, 0f),
                Color.white);

            var joint = CreatePrimitive(
                PrimitiveType.Sphere,
                "Joint0",
                root.transform,
                new Vector3(0.10f, 0.10f, 0.10f),
                new Vector3(0f, 0.72f, 0f),
                presetColor);

            var link1 = CreatePrimitive(
                PrimitiveType.Cylinder,
                "Link1",
                root.transform,
                new Vector3(0.04f, 0.28f, 0.04f),
                new Vector3(0f, 1.02f, 0f),
                Color.white);

            var ee = CreatePrimitive(
                PrimitiveType.Sphere,
                "EndEffector",
                root.transform,
                new Vector3(0.08f, 0.08f, 0.08f),
                new Vector3(0f, 1.32f, 0f),
                presetColor);

            NormalizePreview(root, 1.2f);
            return root;
        }

        private static GameObject BuildProceduralArticulatedArm(RobotMetadataInfo meta)
        {
            var root = new GameObject($"Procedural6Dof_{meta.RobotId}");
            var preset = GetSixDofPreset(meta);

            CreatePrimitive(
                PrimitiveType.Cylinder,
                "Base",
                root.transform,
                new Vector3(preset.BaseWidth, preset.BaseHeight, preset.BaseWidth),
                new Vector3(0f, preset.BaseHeight, 0f),
                preset.BodyColor);

            var joint0 = CreateJointPivot(root.transform, "JointPivot0", new Vector3(0f, preset.ShoulderOffset, 0f));
            CreateBoxLink("BaseColumn", joint0, new Vector3(preset.BaseWidth * 0.7f, preset.ShoulderOffset, preset.BaseWidth * 0.7f), new Vector3(0f, preset.ShoulderOffset * 0.5f, 0f), preset.BodyColor);

            var joint1 = CreateJointPivot(joint0, "JointPivot1", new Vector3(0f, preset.ShoulderOffset, 0f));
            CreateCapsuleLink("UpperArm", joint1, preset.UpperArmLength, preset.UpperArmThickness, new Vector3(0f, preset.UpperArmLength * 0.5f, 0f), preset.AccentColor);

            var joint2 = CreateJointPivot(joint1, "JointPivot2", new Vector3(0f, preset.UpperArmLength, 0f));
            CreateCapsuleLink("Forearm", joint2, preset.ForearmLength, preset.ForearmThickness, new Vector3(0f, preset.ForearmLength * 0.5f, 0f), preset.BodyColor);

            var joint3 = CreateJointPivot(joint2, "JointPivot3", new Vector3(0f, preset.ForearmLength, 0f));
            CreateCapsuleLink("WristA", joint3, preset.WristLength, preset.WristThickness, new Vector3(0f, preset.WristLength * 0.5f, 0f), preset.AccentColor);

            var joint4 = CreateJointPivot(joint3, "JointPivot4", new Vector3(0f, preset.WristLength, 0f));
            CreateCapsuleLink("WristB", joint4, preset.WristLength * 0.75f, preset.WristThickness * 0.9f, new Vector3(0f, preset.WristLength * 0.375f, 0f), preset.BodyColor);

            var joint5 = CreateJointPivot(joint4, "JointPivot5", new Vector3(0f, preset.WristLength * 0.75f, 0f));
            CreateCapsuleLink("ToolMount", joint5, preset.ToolLength, preset.WristThickness * 0.8f, new Vector3(0f, preset.ToolLength * 0.5f, 0f), preset.AccentColor);

            var toolHead = CreatePrimitive(
                PrimitiveType.Cube,
                "ToolHead",
                joint5,
                new Vector3(preset.WristThickness * 2.0f, preset.ToolLength * 0.35f, preset.WristThickness * 1.4f),
                new Vector3(0f, preset.ToolLength, 0f),
                preset.BodyColor);

            NormalizePreview(root, 1.25f);
            return root;
        }

        private static GameObject BuildUnknownPreview()
        {
            var root = new GameObject("UnknownPreview");
            CreatePrimitive(
                PrimitiveType.Capsule,
                "FallbackArm",
                root.transform,
                new Vector3(0.25f, 0.55f, 0.25f),
                new Vector3(0f, 0.55f, 0f),
                UIDesignTokens.Colors.PreviewPlaceholder);
            NormalizePreview(root, 1.0f);
            return root;
        }

        private static DonorPreviewPose GetDonorPreviewPose(RobotMetadataInfo meta)
        {
            if (meta.EffectivePreviewRobotId == "FAIRINO_FR5")
            {
                return new DonorPreviewPose
                {
                    LocalEuler = Vector3.zero,
                    LocalOffset = Vector3.zero,
                    TargetMaxSize = 0.95f,
                    RootEuler = new Vector3(0f, -20f, 0f),
                    AnchorPath = "FAIRINO_FR5/base_link",
                    AnchorEuler = new Vector3(90f, 0f, 0f),
                    AnchorOffset = new Vector3(0f, 0.02f, 0f)
                };
            }

            if (meta.EffectivePreviewRobotId == "UR5e")
            {
                return new DonorPreviewPose
                {
                    LocalEuler = Vector3.zero,
                    LocalOffset = Vector3.zero,
                    TargetMaxSize = 0.95f,
                    RootEuler = new Vector3(0f, -20f, 0f),
                    AnchorPath = string.Empty,
                    AnchorEuler = Vector3.zero,
                    AnchorOffset = Vector3.zero
                };
            }

            if (meta.EffectivePreviewRobotId == "DOOSAN_M1013")
            {
                return new DonorPreviewPose
                {
                    LocalEuler = Vector3.zero,
                    LocalOffset = Vector3.zero,
                    TargetMaxSize = 0.95f,
                    RootEuler = new Vector3(0f, -20f, 0f),
                    AnchorPath = string.Empty,
                    AnchorEuler = Vector3.zero,
                    AnchorOffset = Vector3.zero
                };
            }

            if (meta.EffectivePreviewRobotId == "MECA500")
            {
                return new DonorPreviewPose
                {
                    LocalEuler = Vector3.zero,
                    LocalOffset = Vector3.zero,
                    TargetMaxSize = 0.90f,
                    RootEuler = new Vector3(0f, -20f, 0f),
                    AnchorPath = string.Empty,
                    AnchorEuler = Vector3.zero,
                    AnchorOffset = Vector3.zero
                };
            }

            return new DonorPreviewPose
            {
                LocalEuler = Vector3.zero,
                LocalOffset = Vector3.zero,
                TargetMaxSize = 1.15f,
                RootEuler = Vector3.zero,
                AnchorPath = string.Empty,
                AnchorEuler = Vector3.zero,
                AnchorOffset = Vector3.zero
            };
        }

        private static void ApplyDonorPreviewPose(RobotMetadataInfo meta, Transform donorTransform)
        {
            if (donorTransform == null)
            {
                return;
            }

            var pose = GetDonorPreviewPose(meta);
            donorTransform.localRotation = Quaternion.Euler(pose.LocalEuler);
            donorTransform.localPosition += pose.LocalOffset;

            if (!string.IsNullOrEmpty(pose.AnchorPath))
            {
                var anchor = donorTransform.Find(pose.AnchorPath);
                if (anchor != null)
                {
                    anchor.localRotation = Quaternion.Euler(pose.AnchorEuler);
                    anchor.localPosition += pose.AnchorOffset;
                }
            }
        }

        private static SixDofPreviewPreset GetSixDofPreset(RobotMetadataInfo meta)
        {
            switch (meta.RobotId)
            {
                case "FANUC_CRX10":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.26f,
                        BaseHeight = 0.15f,
                        ShoulderOffset = 0.40f,
                        UpperArmLength = 0.55f,
                        UpperArmThickness = 0.11f,
                        ForearmLength = 0.48f,
                        ForearmThickness = 0.09f,
                        WristLength = 0.20f,
                        WristThickness = 0.065f,
                        ToolLength = 0.16f,
                        AccentColor = new Color(0.27f, 0.58f, 0.92f, 1f),
                        BodyColor = new Color(0.78f, 0.80f, 0.83f, 1f)
                    };
                case "IGUS_REBEL":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.22f,
                        BaseHeight = 0.12f,
                        ShoulderOffset = 0.34f,
                        UpperArmLength = 0.48f,
                        UpperArmThickness = 0.08f,
                        ForearmLength = 0.42f,
                        ForearmThickness = 0.07f,
                        WristLength = 0.17f,
                        WristThickness = 0.05f,
                        ToolLength = 0.13f,
                        AccentColor = new Color(0.92f, 0.50f, 0.12f, 1f),
                        BodyColor = new Color(0.72f, 0.72f, 0.72f, 1f)
                    };
                case "FAIRINO_FR5":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.22f,
                        BaseHeight = 0.13f,
                        ShoulderOffset = 0.36f,
                        UpperArmLength = 0.50f,
                        UpperArmThickness = 0.09f,
                        ForearmLength = 0.46f,
                        ForearmThickness = 0.08f,
                        WristLength = 0.18f,
                        WristThickness = 0.055f,
                        ToolLength = 0.14f,
                        AccentColor = new Color(0.20f, 0.55f, 0.85f, 1f),
                        BodyColor = new Color(0.90f, 0.92f, 0.93f, 1f)
                    };
                case "UR5e":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.20f,
                        BaseHeight = 0.12f,
                        ShoulderOffset = 0.35f,
                        UpperArmLength = 0.52f,
                        UpperArmThickness = 0.08f,
                        ForearmLength = 0.44f,
                        ForearmThickness = 0.07f,
                        WristLength = 0.17f,
                        WristThickness = 0.05f,
                        ToolLength = 0.13f,
                        AccentColor = new Color(0.18f, 0.48f, 0.78f, 1f),
                        BodyColor = new Color(0.85f, 0.87f, 0.90f, 1f)
                    };
                case "DOOSAN_M1013":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.24f,
                        BaseHeight = 0.14f,
                        ShoulderOffset = 0.40f,
                        UpperArmLength = 0.58f,
                        UpperArmThickness = 0.10f,
                        ForearmLength = 0.50f,
                        ForearmThickness = 0.09f,
                        WristLength = 0.20f,
                        WristThickness = 0.06f,
                        ToolLength = 0.15f,
                        AccentColor = new Color(0.12f, 0.42f, 0.72f, 1f),
                        BodyColor = new Color(0.88f, 0.90f, 0.92f, 1f)
                    };
                case "MECA500":
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.14f,
                        BaseHeight = 0.08f,
                        ShoulderOffset = 0.22f,
                        UpperArmLength = 0.32f,
                        UpperArmThickness = 0.055f,
                        ForearmLength = 0.28f,
                        ForearmThickness = 0.045f,
                        WristLength = 0.12f,
                        WristThickness = 0.035f,
                        ToolLength = 0.09f,
                        AccentColor = new Color(0.22f, 0.60f, 0.88f, 1f),
                        BodyColor = new Color(0.86f, 0.88f, 0.90f, 1f)
                    };
                default:
                    return new SixDofPreviewPreset
                    {
                        BaseWidth = 0.24f,
                        BaseHeight = 0.14f,
                        ShoulderOffset = 0.38f,
                        UpperArmLength = 0.52f,
                        UpperArmThickness = 0.10f,
                        ForearmLength = 0.46f,
                        ForearmThickness = 0.08f,
                        WristLength = 0.18f,
                        WristThickness = 0.06f,
                        ToolLength = 0.15f,
                        AccentColor = UIDesignTokens.Colors.AccentPrimary,
                        BodyColor = new Color(0.68f, 0.70f, 0.74f, 1f)
                    };
            }
        }

        private static Transform CreateJointPivot(Transform parent, string name, Vector3 localPosition)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = localPosition;
            pivot.localRotation = Quaternion.identity;
            return pivot;
        }

        private static GameObject CreateCapsuleLink(string name, Transform parent, float length, float thickness, Vector3 localPosition, Color color)
        {
            return CreatePrimitive(
                PrimitiveType.Capsule,
                name,
                parent,
                new Vector3(thickness, length * 0.5f, thickness),
                localPosition,
                color);
        }

        private static GameObject CreateBoxLink(string name, Transform parent, Vector3 scale, Vector3 localPosition, Color color)
        {
            return CreatePrimitive(
                PrimitiveType.Cube,
                name,
                parent,
                scale,
                localPosition,
                color);
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 scale, Vector3 localPosition, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = localPosition;
            DisableCollider(go);
            ApplyMaterial(go, color);
            return go;
        }

        private static void CopyDonorHierarchy(Transform source, Transform parent)
        {
            if (source == null || parent == null)
            {
                return;
            }

            var node = new GameObject(source.name);
            node.transform.SetParent(parent, false);
            node.transform.localPosition = source.localPosition;
            node.transform.localRotation = source.localRotation;
            node.transform.localScale = source.localScale;
            DonorMeshCopier.CopyMeshOnly(node, source);

            for (int i = 0; i < source.childCount; i++)
            {
                CopyDonorHierarchy(source.GetChild(i), node.transform);
            }
        }

        private static void NormalizePreview(GameObject root, float targetMaxSize)
        {
            if (root == null)
            {
                return;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxSize > 0.0001f)
            {
                float scale = targetMaxSize / maxSize;
                root.transform.localScale *= scale;
            }

            renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 offset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            root.transform.position -= offset;
        }

        private static void DisableCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static void ApplyMaterial(GameObject go, Color color)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = color
            };
        }
    }
}
