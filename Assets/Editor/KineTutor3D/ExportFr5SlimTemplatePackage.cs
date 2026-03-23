// Editor-only: export the FR5 slim template bundle into the robottemplete root.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class ExportFr5SlimTemplatePackage
    {
        private const string InternalDocsRoot = "Assets/Handoff/FR5_Slim_Template";

        [MenuItem("KineTutor3D/Export/Build FR5 Slim Template Package", priority = 163)]
        public static void BuildPackage()
        {
            Fr5TemplateSlimSceneBuilder.BuildScene();
            ValidateRoots();
            ResetGeneratedOutputs();
            CopyBundleAssets();
            CopyBundleDocs();
            ExportUnityPackage();
            ExportZip();
            Debug.Log("[Export] FR5 slim template package created.");
        }

        private static void ValidateRoots()
        {
            var missing = new List<string>();
            var roots = KineTutor3D.App.FR5TemplateSlimManifest.GetPackageRoots();

            for (var i = 0; i < roots.Length; i++)
            {
                var assetPath = roots[i];
                if (!AssetExists(assetPath))
                {
                    missing.Add(assetPath);
                }
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "[Export] Missing required slim template assets:\n - " + string.Join("\n - ", missing));
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

        private static void ResetGeneratedOutputs()
        {
            var externalRoot = KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot;
            Directory.CreateDirectory(externalRoot);

            ResetPath(Path.Combine(externalRoot, "Assets"));
            ResetPath(Path.Combine(externalRoot, KineTutor3D.App.FR5TemplateSlimManifest.EvidenceFolderName));
            DeleteIfExists(Path.Combine(externalRoot, "README.md"));
            DeleteIfExists(Path.Combine(externalRoot, "INSTALL.md"));
            DeleteIfExists(Path.Combine(externalRoot, "DEPENDENCIES.md"));
            DeleteIfExists(Path.Combine(externalRoot, "MATERIALS.md"));
            DeleteIfExists(Path.Combine(externalRoot, "CHECKLIST.md"));
            DeleteIfExists(Path.Combine(externalRoot, KineTutor3D.App.FR5TemplateSlimManifest.UnityPackageName));
            DeleteIfExists(Path.Combine(externalRoot, KineTutor3D.App.FR5TemplateSlimManifest.ZipName));
        }

        private static void CopyBundleAssets()
        {
            var externalRoot = KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot;
            var roots = KineTutor3D.App.FR5TemplateSlimManifest.GetPackageRoots();

            for (var i = 0; i < roots.Length; i++)
            {
                CopyAssetPath(roots[i], externalRoot);
            }
        }

        private static void CopyBundleDocs()
        {
            var externalRoot = KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot;
            var docNames = new[]
            {
                "README.md",
                "INSTALL.md",
                "DEPENDENCIES.md",
                "MATERIALS.md",
                "CHECKLIST.md"
            };

            for (var i = 0; i < docNames.Length; i++)
            {
                var source = GetAbsoluteProjectPath($"{InternalDocsRoot}/{docNames[i]}");
                var destination = Path.Combine(externalRoot, docNames[i]);
                EnsureParentDirectory(destination);
                File.Copy(source, destination, true);
            }
        }

        private static void ExportUnityPackage()
        {
            var unityPackagePath = Path.Combine(
                KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot,
                KineTutor3D.App.FR5TemplateSlimManifest.UnityPackageName);

            AssetDatabase.ExportPackage(
                KineTutor3D.App.FR5TemplateSlimManifest.GetPackageRoots(),
                unityPackagePath,
                ExportPackageOptions.Recurse);
        }

        private static void ExportZip()
        {
            var externalRoot = KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot;
            var zipPath = Path.Combine(externalRoot, KineTutor3D.App.FR5TemplateSlimManifest.ZipName);
            var stagingRoot = Path.Combine(Path.GetTempPath(), "FR5TemplateSlimZipStage");

            ResetPath(stagingRoot);
            Directory.CreateDirectory(stagingRoot);

            CopyPath(Path.Combine(externalRoot, "Assets"), Path.Combine(stagingRoot, "Assets"));
            CopyPath(Path.Combine(externalRoot, "README.md"), Path.Combine(stagingRoot, "README.md"));
            CopyPath(Path.Combine(externalRoot, "INSTALL.md"), Path.Combine(stagingRoot, "INSTALL.md"));
            CopyPath(Path.Combine(externalRoot, "DEPENDENCIES.md"), Path.Combine(stagingRoot, "DEPENDENCIES.md"));
            CopyPath(Path.Combine(externalRoot, "MATERIALS.md"), Path.Combine(stagingRoot, "MATERIALS.md"));
            CopyPath(Path.Combine(externalRoot, "CHECKLIST.md"), Path.Combine(stagingRoot, "CHECKLIST.md"));

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(stagingRoot, zipPath);
            ResetPath(stagingRoot);
        }

        private static void CopyAssetPath(string assetPath, string externalRoot)
        {
            var sourceAbsolute = GetAbsoluteProjectPath(assetPath);
            var destinationAbsolute = Path.Combine(externalRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));

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

        private static void CopyPath(string sourcePath, string destinationPath)
        {
            if (Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(destinationPath);
                foreach (var directory in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(sourcePath, directory);
                    Directory.CreateDirectory(Path.Combine(destinationPath, relative));
                }

                foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(sourcePath, file);
                    var targetFile = Path.Combine(destinationPath, relative);
                    EnsureParentDirectory(targetFile);
                    File.Copy(file, targetFile, true);
                }

                return;
            }

            EnsureParentDirectory(destinationPath);
            File.Copy(sourcePath, destinationPath, true);
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

        private static void ResetPath(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
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
