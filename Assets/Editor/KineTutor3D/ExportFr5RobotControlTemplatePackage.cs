// Editor-only: export FR5 RobotControl mock-only handoff bundle and unitypackage.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class ExportFr5RobotControlTemplatePackage
    {
        private const string InternalDocsRoot = "Assets/Handoff/FR5_RobotControl_Template";
        private const string ExternalRoot = @"C:\Users\ezen601\Desktop\Jason\robottemplete";
        private const string ExternalBundleName = "FR5_RobotControl_Template";
        private const string UnityPackageName = "FR5_RobotControl_Template.unitypackage";

        private static readonly string[] PackageRoots =
        {
            "Assets/Scenes/RobotControl.unity",
            "Assets/Scripts/App/Fairino",
            "Assets/Scripts/App/SceneId.cs",
            "Assets/Scripts/App/SceneCatalog.cs",
            "Assets/Scripts/App/SceneNavigator.cs",
            "Assets/Scripts/App/RobotSelectionBridge.cs",
            "Assets/Scripts/App/SceneCameraDirector.cs",
            "Assets/Scripts/UI",
            "Assets/Scripts/Visualization/FairinoUrdfJointDriver.cs",
            "Assets/Scripts/Visualization/Shared",
            "Assets/Scripts/Types",
            "Assets/Scripts/Templates",
            "Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab",
            "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab",
            "Assets/Runtime/Resources/LearningTabs/FAIRINO_FR5.json",
            "Assets/Runtime/Resources/UI",
            "Assets/Runtime/Robots/FAIRINO_FR5",
            "Assets/Handoff/FR5_RobotControl_Template/README.md",
            "Assets/Handoff/FR5_RobotControl_Template/IMPORT-CHECKLIST.md",
            "Assets/Handoff/FR5_RobotControl_Template/DEPENDENCIES.md",
            "Assets/Handoff/FR5_RobotControl_Template/PACKAGE-ASSET-ROOTS.txt"
        };

        [MenuItem("KineTutor3D/Export/Build FR5 RobotControl Template Package", priority = 160)]
        public static void BuildPackage()
        {
            ValidateRoots();

            var externalBundleRoot = Path.Combine(ExternalRoot, ExternalBundleName);
            var unityPackagePath = Path.Combine(ExternalRoot, UnityPackageName);

            ResetDirectory(externalBundleRoot);
            Directory.CreateDirectory(ExternalRoot);

            CopyBundleAssets(externalBundleRoot);
            CopyBundleDocs(externalBundleRoot);

            AssetDatabase.ExportPackage(
                PackageRoots,
                unityPackagePath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            Debug.Log($"[Export] FR5 RobotControl template folder created: {externalBundleRoot}");
            Debug.Log($"[Export] FR5 RobotControl unitypackage created: {unityPackagePath}");
        }

        private static void ValidateRoots()
        {
            var missing = new List<string>();
            for (var i = 0; i < PackageRoots.Length; i++)
            {
                var assetPath = PackageRoots[i];
                if (!AssetExists(assetPath))
                {
                    missing.Add(assetPath);
                }
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "[Export] Missing required assets:\n - " + string.Join("\n - ", missing));
            }
        }

        private static bool AssetExists(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return true;
            }

            return File.Exists(GetAbsoluteProjectPath(assetPath));
        }

        private static void CopyBundleAssets(string externalBundleRoot)
        {
            for (var i = 0; i < PackageRoots.Length; i++)
            {
                var assetPath = PackageRoots[i];
                if (assetPath.StartsWith(InternalDocsRoot, StringComparison.Ordinal))
                {
                    continue;
                }

                CopyAssetPath(assetPath, externalBundleRoot);
            }
        }

        private static void CopyBundleDocs(string externalBundleRoot)
        {
            var docNames = new[]
            {
                "README.md",
                "IMPORT-CHECKLIST.md",
                "DEPENDENCIES.md",
                "PACKAGE-ASSET-ROOTS.txt"
            };

            for (var i = 0; i < docNames.Length; i++)
            {
                var source = GetAbsoluteProjectPath($"{InternalDocsRoot}/{docNames[i]}");
                var destination = Path.Combine(externalBundleRoot, docNames[i]);
                EnsureParentDirectory(destination);
                File.Copy(source, destination, true);
            }
        }

        private static void CopyAssetPath(string assetPath, string externalBundleRoot)
        {
            var sourceAbsolute = GetAbsoluteProjectPath(assetPath);
            var destinationAbsolute = Path.Combine(externalBundleRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));

            if (Directory.Exists(sourceAbsolute))
            {
                Directory.CreateDirectory(destinationAbsolute);
                CopyMetaFile(sourceAbsolute, destinationAbsolute);

                foreach (var directory in Directory.GetDirectories(sourceAbsolute, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(sourceAbsolute, directory);
                    var targetDirectory = Path.Combine(destinationAbsolute, relative);
                    Directory.CreateDirectory(targetDirectory);
                    CopyMetaFile(directory, targetDirectory);
                }

                foreach (var file in Directory.GetFiles(sourceAbsolute, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(sourceAbsolute, file);
                    var targetFile = Path.Combine(destinationAbsolute, relative);
                    EnsureParentDirectory(targetFile);
                    File.Copy(file, targetFile, true);
                }

                return;
            }

            EnsureParentDirectory(destinationAbsolute);
            File.Copy(sourceAbsolute, destinationAbsolute, true);
            CopyMetaFile(sourceAbsolute, destinationAbsolute);
        }

        private static void CopyMetaFile(string sourcePath, string destinationPath)
        {
            var sourceMeta = sourcePath + ".meta";
            if (!File.Exists(sourceMeta))
            {
                return;
            }

            var destinationMeta = destinationPath + ".meta";
            EnsureParentDirectory(destinationMeta);
            File.Copy(sourceMeta, destinationMeta, true);
        }

        private static string GetAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static void ResetDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        private static void EnsureParentDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
