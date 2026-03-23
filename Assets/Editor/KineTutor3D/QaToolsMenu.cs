// Editor-only: QA helper tools for testing the full user flow.
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Robotics.UrdfImporter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace KineTutor3D.Editor
{
    public static class QaToolsMenu
    {
        private const string HasVisitedKey = "KineTutor3D.HasVisited";
        private const string TrackKey = "KineTutor3D.CurrentTrack";
        private const string MathReadinessLastCompletedStepKey = "KineTutor3D.MathReadiness.LastCompletedStep";
        private const string PreKinematicsLastCompletedStepKey = "KineTutor3D.PreKinematics.LastCompletedStep";
        private const string CoreKinematicsLastCompletedStepKey = "KineTutor3D.CoreKinematics.LastCompletedStep";
        private const string SessionContextKey = "KineTutor3D.SessionContextJson";
        private const string ReducedMotionKey = "KineTutor3D.ReducedMotion";
        private const string SelectedRobotIdKey = "KineTutor3D.SelectedRobotId";
        private const string SelectedModeKey = "KineTutor3D.SelectedMode";

        private const string CoreTrack = "core_kinematics";
        private const string MathReadinessTrack = "math_readiness";
        private const string GuidedLessonMode = "guided_lesson";
        private const string SandboxMode = "sandbox";
        private const string DefaultRobotId = "2DOF_RR";
        private const string FairinoUrdfAssetPath = "Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf";
        private const string FairinoMeshesFolder = "Assets/Runtime/Robots/FAIRINO_FR5/meshes";
        private const string FairinoPrefabAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab";
        private const string FairinoControlPrefabAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab";
        private const string FairinoMaterialAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat";
        private const string SharedUiPrefabFolder = "Assets/Runtime/UI/Prefabs";
        private const string SceneNavigationBarPrefabPath = "Assets/Runtime/UI/Prefabs/SceneNavigationBar.prefab";

        private const string Ur5eUrdfAssetPath = "Assets/Runtime/Robots/UR5e/ur5e.urdf";
        private const string Ur5ePrefabAssetPath = "Assets/Runtime/Resources/Robots/UR5e/UR5e.prefab";
        private const string Ur5eControlPrefabAssetPath = "Assets/Runtime/Resources/Robots/UR5e/UR5e_Control.prefab";

        private const string DoosanUrdfAssetPath = "Assets/Runtime/Robots/DOOSAN_M1013/m1013.urdf";
        private const string DoosanPrefabAssetPath = "Assets/Runtime/Resources/Robots/DoosanM1013/DoosanM1013.prefab";
        private const string DoosanControlPrefabAssetPath = "Assets/Runtime/Resources/Robots/DoosanM1013/DoosanM1013_Control.prefab";

        [MenuItem("KineTutor3D/QA: Reset to First-Time User", priority = 100)]
        private static void ResetToFirstTimeUser()
        {
            ClearQaState();
            PlayerPrefs.Save();
            Debug.Log("[QA] PlayerPrefs cleared — next Play will start from Onboarding.");
        }

        [MenuItem("KineTutor3D/QA: Reset to Returning User (skip onboarding)", priority = 101)]
        private static void ResetToReturningUser()
        {
            PlayerPrefs.SetInt("KineTutor3D.HasVisited", 1);
            PlayerPrefs.DeleteKey("KineTutor3D.CurrentTrack");
            PlayerPrefs.DeleteKey("KineTutor3D.MathReadiness.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.PreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.CoreKinematics.LastCompletedStep");
            PlayerPrefs.DeleteKey("KineTutor3D.SessionContextJson");
            PlayerPrefs.Save();
            Debug.Log("[QA] PlayerPrefs set to returning user — next Play will start from RobotLibrary.");
        }

        [MenuItem("KineTutor3D/QA: Prep Guided Lesson (Core Step 1)", priority = 111)]
        private static void PrepGuidedLessonCore()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, CoreTrack);
            PlayerPrefs.SetInt(CoreKinematicsLastCompletedStepKey, 0);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Guided Lesson Core 준비 완료 — Play 후 Home에서 '학습 시작'을 눌러 Main으로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Math Readiness", priority = 112)]
        private static void PrepMathReadiness()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, MathReadinessTrack);
            PlayerPrefs.SetInt(MathReadinessLastCompletedStepKey, 0);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Math Readiness 준비 완료 — Play 후 Home에서 '수학 기초 워밍업'을 눌러 MathReadiness로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Robot Library", priority = 113)]
        private static void PrepRobotLibrary()
        {
            ResetToReturningUser();
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, GuidedLessonMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Robot Library 준비 완료 — Play 후 Home에서 '로봇 선택'을 눌러 Robot Library로 진입하세요.");
        }

        [MenuItem("KineTutor3D/QA: Prep Sandbox", priority = 114)]
        private static void PrepSandbox()
        {
            ClearQaState();
            PlayerPrefs.SetInt(HasVisitedKey, 1);
            PlayerPrefs.SetString(TrackKey, CoreTrack);
            PlayerPrefs.SetString(SelectedRobotIdKey, DefaultRobotId);
            PlayerPrefs.SetString(SelectedModeKey, SandboxMode);
            PlayerPrefs.Save();
            Debug.Log("[QA] Sandbox 준비 완료 — Play 후 Home에서 '샌드박스'를 눌러 Sandbox로 진입하세요.");
        }

        [MenuItem("KineTutor3D/Robots/Import FAIRINO FR5 URDF", priority = 140)]
        public static void ImportFairinoFr5Urdf()
        {
            var fullUrdfPath = Path.GetFullPath(FairinoUrdfAssetPath);
            if (!File.Exists(fullUrdfPath))
            {
                Debug.LogError($"[QA] FAIRINO FR5 URDF not found at '{fullUrdfPath}'.");
                return;
            }

            EnsureFolder("Assets/Runtime/Resources");
            EnsureFolder("Assets/Runtime/Resources/Robots");
            PreprocessFairinoStlMeshes();

            var settings = ImportSettings.DefaultSettings();
            settings.OverwriteExistingPrefabs = true;
            settings.chosenAxis = ImportSettings.axisType.yAxis;
            settings.convexMethod = ImportSettings.convexDecomposer.unity;

            var importRoutine = UrdfRobotExtensions.Create(fullUrdfPath, settings, loadStatus: false, forceRuntimeMode: false);
            GameObject importedRobot = null;
            while (importRoutine.MoveNext())
            {
                importedRobot = importRoutine.Current;
            }

            if (importedRobot == null)
            {
                Debug.LogError("[QA] FAIRINO FR5 import failed. UrdfRobotExtensions.Create returned null.");
                return;
            }

            var unpackedPrefabRoots = UnpackFairinoControlPrefabInstances(importedRobot);
            var reboundMeshCount = RebindFairinoControlMeshes(importedRobot.transform);
            var controlPrefab = PrefabUtility.SaveAsPrefabAsset(importedRobot, FairinoControlPrefabAssetPath, out var controlSuccess);
            var previewRoot = BuildFairinoPreviewHierarchy(importedRobot.transform);
            previewRoot.name = "FAIRINO_FR5";
            var prefab = PrefabUtility.SaveAsPrefabAsset(previewRoot, FairinoPrefabAssetPath, out var success);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Object.DestroyImmediate(previewRoot);
            Object.DestroyImmediate(importedRobot);

            if (!controlSuccess || controlPrefab == null)
            {
                Debug.LogError($"[QA] FAIRINO FR5 control prefab save failed at '{FairinoControlPrefabAssetPath}'.");
                return;
            }

            if (!ValidateFairinoControlPrefab(controlPrefab, out var controlDiagnostic))
            {
                Debug.LogError($"[QA] FAIRINO FR5 control prefab validation failed: {controlDiagnostic}");
                return;
            }

            if (!success || prefab == null)
            {
                Debug.LogError($"[QA] FAIRINO FR5 prefab save failed at '{FairinoPrefabAssetPath}'.");
                return;
            }

            Debug.Log($"[QA] FAIRINO FR5 control prefab imported successfully: {FairinoControlPrefabAssetPath} (unpacked {unpackedPrefabRoots} prefab roots, rebound {reboundMeshCount} meshes)");
            Debug.Log($"[QA] FAIRINO FR5 preview prefab imported successfully: {FairinoPrefabAssetPath}");
        }

        [MenuItem("KineTutor3D/Robots/Import UR5e URDF", priority = 141)]
        public static void ImportUr5eUrdf()
        {
            EnsureFolder("Assets/Runtime/Resources/Robots");
            EnsureFolder("Assets/Runtime/Resources/Robots/UR5e");
            ImportGenericRobotUrdf(
                urdfAssetPath: Ur5eUrdfAssetPath,
                robotId: "UR5e",
                controlPrefabPath: Ur5eControlPrefabAssetPath,
                previewPrefabPath: Ur5ePrefabAssetPath);
        }

        [MenuItem("KineTutor3D/Robots/Import Doosan M1013 URDF", priority = 142)]
        public static void ImportDoosanM1013Urdf()
        {
            EnsureFolder("Assets/Runtime/Resources/Robots");
            EnsureFolder("Assets/Runtime/Resources/Robots/DoosanM1013");
            ImportGenericRobotUrdf(
                urdfAssetPath: DoosanUrdfAssetPath,
                robotId: "DoosanM1013",
                controlPrefabPath: DoosanControlPrefabAssetPath,
                previewPrefabPath: DoosanPrefabAssetPath);
        }

        [MenuItem("KineTutor3D/RobotControl/Author Scene UI", priority = 145)]
        public static void AuthorRobotControlSceneUi()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("RobotControl.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[QA] 먼저 Assets/Scenes/RobotControl.unity 씬을 열어주세요.");
                return;
            }

            var canvas = KineTutor3D.UI.FairinoRobotControlViewBuilder.EnsureCanvas(null, null);
            KineTutor3D.UI.FairinoRobotControlViewBuilder.EnsureEventSystem();
            KineTutor3D.UI.FairinoRobotControlViewBuilder.EnsureCamera();
            KineTutor3D.UI.FairinoRobotControlViewBuilder.EnsureLight();
            KineTutor3D.UI.FairinoRobotControlViewBuilder.EnsureLayout(
                canvas,
                null,
                out var connectionPanel,
                out var jointControlPanel,
                out var statePanel,
                out var tcpPanel,
                out var diagnosticsDrawer,
                out var whyItMovedLabel,
                out var moveConfirmDialog,
                out _,
                out _,
                out _);

            InvokePrivate(connectionPanel, "EnsurePresentation");
            InvokePrivate(jointControlPanel, "EnsurePresentation");
            InvokePrivate(statePanel, "EnsurePresentation");
            InvokePrivate(tcpPanel, "EnsurePresentation");
            InvokePrivate(diagnosticsDrawer, "EnsurePresentation");
            InvokePrivate(whyItMovedLabel, "EnsurePresentation");
            InvokePrivate(moveConfirmDialog, "EnsurePresentation");

            diagnosticsDrawer.SetVisible(false);
            moveConfirmDialog.SetVisible(false);

            var coordinator = Object.FindFirstObjectByType<KineTutor3D.App.Fairino.RobotControlSceneCoordinator>(FindObjectsInactive.Include);
            if (coordinator != null)
            {
                EditorUtility.SetDirty(coordinator);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QA] RobotControl scene-authored UI를 씬에 생성하고 저장했습니다.");
        }

        [MenuItem("KineTutor3D/Onboarding/Author Scene UI", priority = 146)]
        public static void AuthorOnboardingSceneUi()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("Onboarding.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[QA] 먼저 Assets/Scenes/Onboarding.unity 씬을 열어주세요.");
                return;
            }

            var onboardingManager = Object.FindFirstObjectByType<KineTutor3D.UI.OnboardingManager>(FindObjectsInactive.Include);
            if (onboardingManager == null)
            {
                Debug.LogError("[QA] OnboardingManager를 찾지 못했습니다.");
                return;
            }

            var builderType = typeof(KineTutor3D.UI.OnboardingManager).Assembly.GetType("KineTutor3D.UI.OnboardingViewBuilder");
            if (builderType == null)
            {
                Debug.LogError("[QA] OnboardingViewBuilder 타입을 찾지 못했습니다.");
                return;
            }

            var build = builderType.GetMethod("Build", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (build == null)
            {
                Debug.LogError("[QA] OnboardingViewBuilder.Build 메서드를 찾지 못했습니다.");
                return;
            }

            build.Invoke(null, new object[] { onboardingManager.transform as RectTransform, null });
            EditorUtility.SetDirty(onboardingManager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QA] Onboarding scene-authored UI를 씬에 정리하고 저장했습니다.");
        }

        [MenuItem("KineTutor3D/RobotLibrary/Author Scene UI", priority = 148)]
        public static void AuthorRobotLibrarySceneUi()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("RobotLibrary.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[QA] 먼저 Assets/Scenes/RobotLibrary.unity 씬을 열어주세요.");
                return;
            }

            var manager = Object.FindFirstObjectByType<KineTutor3D.UI.RobotLibraryManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                Debug.LogError("[QA] RobotLibraryManager를 찾지 못했습니다.");
                return;
            }

            InvokePrivate(manager, "EnsurePresentation");
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QA] RobotLibrary scene-authored UI를 씬에 생성하고 저장했습니다.");
        }

        [MenuItem("KineTutor3D/UI/Author Shared SceneNavigationBar Prefab", priority = 149)]
        public static void AuthorSharedSceneNavigationBarPrefab()
        {
            EnsureFolder("Assets/Runtime");
            EnsureFolder("Assets/Runtime/UI");
            EnsureFolder(SharedUiPrefabFolder);

            var tempRoot = new GameObject("SceneNavigationBar", typeof(RectTransform));
            try
            {
                var navBar = tempRoot.AddComponent<KineTutor3D.UI.SceneNavigationBar>();
                InvokePrivate(navBar, "EnsurePresentation");

                var prefab = PrefabUtility.SaveAsPrefabAsset(tempRoot, SceneNavigationBarPrefabPath, out var success);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!success || prefab == null)
                {
                    Debug.LogError($"[QA] SceneNavigationBar prefab save failed at '{SceneNavigationBarPrefabPath}'.");
                    return;
                }

                Debug.Log($"[QA] Shared SceneNavigationBar prefab authored at '{SceneNavigationBarPrefabPath}'.");
            }
            finally
            {
                Object.DestroyImmediate(tempRoot);
            }
        }

        // Generic URDF import helper for robots that do not need FR5-specific post-processing.
        // The URDF Importer creates the joint hierarchy automatically from the URDF.
        // Both control and preview prefabs are saved from the same imported root.
        private static void ImportGenericRobotUrdf(
            string urdfAssetPath,
            string robotId,
            string controlPrefabPath,
            string previewPrefabPath)
        {
            var fullUrdfPath = Path.GetFullPath(urdfAssetPath);
            if (!File.Exists(fullUrdfPath))
            {
                Debug.LogError($"[QA] {robotId} URDF not found at '{fullUrdfPath}'.");
                return;
            }

            AssetDatabase.Refresh();

            var settings = ImportSettings.DefaultSettings();
            settings.OverwriteExistingPrefabs = true;
            settings.chosenAxis = ImportSettings.axisType.yAxis;
            settings.convexMethod = ImportSettings.convexDecomposer.unity;

            var importRoutine = UrdfRobotExtensions.Create(fullUrdfPath, settings, loadStatus: false, forceRuntimeMode: false);
            GameObject importedRobot = null;
            while (importRoutine.MoveNext())
            {
                importedRobot = importRoutine.Current;
            }

            if (importedRobot == null)
            {
                Debug.LogError($"[QA] {robotId} import failed. UrdfRobotExtensions.Create returned null.");
                return;
            }

            // Unpack any nested prefab instances before saving
            var unpackedPrefabRoots = new HashSet<GameObject>();
            var transforms = importedRobot.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(transforms[i].gameObject);
                if (instanceRoot != null && instanceRoot != importedRobot && instanceRoot.transform.IsChildOf(importedRobot.transform))
                {
                    unpackedPrefabRoots.Add(instanceRoot);
                }
            }

            foreach (var root in unpackedPrefabRoots)
            {
                if (root != null && PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }

            // Save control prefab (full ArticulationBody hierarchy for runtime control)
            importedRobot.name = robotId + "_Control";
            var controlPrefab = PrefabUtility.SaveAsPrefabAsset(importedRobot, controlPrefabPath, out var controlSuccess);

            // Save preview prefab (same hierarchy, renamed for showroom use)
            importedRobot.name = robotId;
            var previewPrefab = PrefabUtility.SaveAsPrefabAsset(importedRobot, previewPrefabPath, out var previewSuccess);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Object.DestroyImmediate(importedRobot);

            if (!controlSuccess || controlPrefab == null)
            {
                Debug.LogError($"[QA] {robotId} control prefab save failed at '{controlPrefabPath}'.");
                return;
            }

            if (!previewSuccess || previewPrefab == null)
            {
                Debug.LogError($"[QA] {robotId} preview prefab save failed at '{previewPrefabPath}'.");
                return;
            }

            Debug.Log($"[QA] {robotId} control prefab imported successfully: {controlPrefabPath}");
            Debug.Log($"[QA] {robotId} preview prefab imported successfully: {previewPrefabPath}");
        }

        private static void ClearQaState()
        {
            PlayerPrefs.DeleteKey(HasVisitedKey);
            PlayerPrefs.DeleteKey(TrackKey);
            PlayerPrefs.DeleteKey(MathReadinessLastCompletedStepKey);
            PlayerPrefs.DeleteKey(PreKinematicsLastCompletedStepKey);
            PlayerPrefs.DeleteKey(CoreKinematicsLastCompletedStepKey);
            PlayerPrefs.DeleteKey(SessionContextKey);
            PlayerPrefs.DeleteKey(ReducedMotionKey);
            PlayerPrefs.DeleteKey(SelectedRobotIdKey);
            PlayerPrefs.DeleteKey(SelectedModeKey);
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var folderName = Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void PreprocessFairinoStlMeshes()
        {
            if (!AssetDatabase.IsValidFolder(FairinoMeshesFolder))
            {
                Debug.LogWarning($"[QA] FAIRINO mesh folder not found: {FairinoMeshesFolder}");
                return;
            }

            var meshDirectory = Path.GetFullPath(FairinoMeshesFolder);
            if (!Directory.Exists(meshDirectory))
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(meshDirectory, "*.*", SearchOption.TopDirectoryOnly))
            {
                var extension = Path.GetExtension(filePath);
                if (!string.Equals(extension, ".stl", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var assetPath = filePath.Replace("\\", "/");
                var assetsIndex = assetPath.IndexOf("/Assets/", System.StringComparison.OrdinalIgnoreCase);
                if (assetsIndex >= 0)
                {
                    assetPath = assetPath.Substring(assetsIndex + 1);
                }

                StlAssetPostProcessor.PostprocessStlFile(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject BuildFairinoPreviewHierarchy(Transform sourceRoot)
        {
            var previewRoot = new GameObject(sourceRoot != null ? sourceRoot.name : "FAIRINO_FR5");
            if (sourceRoot == null)
            {
                return previewRoot;
            }

            var material = LoadOrCreateFairinoPreviewMaterial();
            CopyFairinoPreviewNode(sourceRoot, previewRoot.transform, material);
            return previewRoot;
        }

        private static int UnpackFairinoControlPrefabInstances(GameObject root)
        {
            if (root == null)
            {
                return 0;
            }

            var unpackedCount = 0;
            var pendingRoots = new HashSet<GameObject>();
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(transforms[i].gameObject);
                if (instanceRoot == null)
                {
                    continue;
                }

                if (instanceRoot != root && !instanceRoot.transform.IsChildOf(root.transform))
                {
                    continue;
                }

                pendingRoots.Add(instanceRoot);
            }

            foreach (var pendingRoot in pendingRoots)
            {
                if (pendingRoot == null || !PrefabUtility.IsPartOfPrefabInstance(pendingRoot))
                {
                    continue;
                }

                PrefabUtility.UnpackPrefabInstance(
                    pendingRoot,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
                unpackedCount++;
            }

            return unpackedCount;
        }

        private static int RebindFairinoControlMeshes(Transform root)
        {
            if (root == null)
            {
                return 0;
            }

            var reboundCount = 0;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var meshAsset = LoadFairinoMeshAsset(transforms[i].name);
                if (meshAsset == null)
                {
                    continue;
                }

                var meshFilter = transforms[i].GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    continue;
                }

                if (meshFilter.sharedMesh != meshAsset)
                {
                    meshFilter.sharedMesh = meshAsset;
                    reboundCount++;
                }

                var meshCollider = transforms[i].GetComponent<MeshCollider>();
                if (meshCollider != null && meshCollider.sharedMesh != meshAsset)
                {
                    meshCollider.sharedMesh = meshAsset;
                }
            }

            return reboundCount;
        }

        private static void InvokePrivate(object target, string methodName)
        {
            if (target == null)
            {
                return;
            }

            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(target, null);
        }

        private static void CopyFairinoPreviewNode(Transform source, Transform targetParent, Material material)
        {
            for (var i = 0; i < source.childCount; i++)
            {
                var child = source.GetChild(i);
                var target = new GameObject(child.name).transform;
                target.SetParent(targetParent, false);
                target.localPosition = child.localPosition;
                target.localRotation = child.localRotation;
                target.localScale = child.localScale;

                var meshAssetPath = $"{FairinoMeshesFolder}/{child.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
                if (mesh != null)
                {
                    var meshFilter = target.gameObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = mesh;

                    var renderer = target.gameObject.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }

                CopyFairinoPreviewNode(child, target, material);
            }
        }

        private static Mesh LoadFairinoMeshAsset(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                return null;
            }

            var meshAssetPath = $"{FairinoMeshesFolder}/{nodeName}.asset";
            return AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);
        }

        private static bool ValidateFairinoControlPrefab(GameObject controlPrefab, out string diagnostic)
        {
            diagnostic = "Control prefab is null.";
            if (controlPrefab == null)
            {
                return false;
            }

            var meshFilters = controlPrefab.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters.Length == 0)
            {
                diagnostic = "Control prefab has no MeshFilter components.";
                return false;
            }

            var validMeshCount = 0;
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var mesh = meshFilters[i] != null ? meshFilters[i].sharedMesh : null;
                if (mesh != null && mesh.vertexCount > 0)
                {
                    validMeshCount++;
                }
            }

            if (validMeshCount != meshFilters.Length)
            {
                diagnostic = $"Only {validMeshCount}/{meshFilters.Length} MeshFilter components have valid sharedMesh references.";
                return false;
            }

            diagnostic = $"All {validMeshCount} MeshFilter components have valid sharedMesh references.";
            return true;
        }

        private static Material LoadOrCreateFairinoPreviewMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(FairinoMaterialAssetPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.27f, 0.56f, 0.87f, 1f)
            };

            AssetDatabase.CreateAsset(material, FairinoMaterialAssetPath);
            return material;
        }
    }
}
