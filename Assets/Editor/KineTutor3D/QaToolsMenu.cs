// Editor-only: QA helper tools for testing the full user flow.
using System.IO;
using Unity.Robotics.UrdfImporter;
using UnityEditor;
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
            Debug.Log("[QA] PlayerPrefs set to returning user — next Play will start from Home.");
        }

        [MenuItem("KineTutor3D/QA: Prep Home / Continue Hub", priority = 110)]
        private static void PrepHomeContinueHub()
        {
            ResetToReturningUser();
            Debug.Log("[QA] Home / Continue Hub 준비 완료 — Play 후 Home에서 시작합니다.");
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

            if (!success || prefab == null)
            {
                Debug.LogError($"[QA] FAIRINO FR5 prefab save failed at '{FairinoPrefabAssetPath}'.");
                return;
            }

            Debug.Log($"[QA] FAIRINO FR5 control prefab imported successfully: {FairinoControlPrefabAssetPath}");
            Debug.Log($"[QA] FAIRINO FR5 preview prefab imported successfully: {FairinoPrefabAssetPath}");
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
