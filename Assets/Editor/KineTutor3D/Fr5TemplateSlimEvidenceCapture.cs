// Editor-only: capture FR5 slim template PNG evidence via the main camera.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class Fr5TemplateSlimEvidenceCapture
    {
        private const int DefaultWidth = 1600;
        private const int DefaultHeight = 900;

        [MenuItem("KineTutor3D/Export/Capture FR5 Slim Evidence", priority = 162)]
        public static void CaptureDefaultEvidenceMenu()
        {
            var outputDirectory = CaptureDefaultEvidenceSafe();
            Debug.Log($"[FR5TemplateSlim] Evidence captured to '{outputDirectory}'.");
        }

        public static string CaptureDefaultEvidence()
        {
            var controller = EnsureDemoRuntime();
            if (controller == null)
            {
                throw new InvalidOperationException("FR5TemplateMinimalController를 찾지 못했습니다.");
            }

            var evidenceRoot = Path.Combine(KineTutor3D.App.FR5TemplateSlimManifest.ExternalRoot, KineTutor3D.App.FR5TemplateSlimManifest.EvidenceFolderName);
            ResetDirectory(evidenceRoot);

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.NeutralName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "fr5-template-neutral.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.ReadyName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "fr5-template-ready.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.ShowcaseName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "fr5-template-showcase.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.NeutralName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "sequence-frame-00-neutral.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.ReadyName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "sequence-frame-01-ready.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.ShowcaseName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "sequence-frame-02-showcase.png"));

            controller.SetPoseByName(KineTutor3D.App.FR5TemplatePoseCatalog.WristTurnName);
            CaptureCurrentCamera(Path.Combine(evidenceRoot, "sequence-frame-03-wristturn.png"));

            return evidenceRoot;
        }

        public static KineTutor3D.App.FR5TemplateMinimalController EnsureDemoRuntime()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<KineTutor3D.App.FR5TemplateMinimalController>(FindObjectsInactive.Include);
            EnsureCamera();
            if (controller != null)
            {
                return controller;
            }

            var root = new GameObject("FR5TemplateDemoRoot_Runtime");
            return root.AddComponent<KineTutor3D.App.FR5TemplateMinimalController>();
        }

        public static int EnsureDemoRuntimeJointCount()
        {
            return EnsureDemoRuntime().GetCurrentJointAnglesDeg().Length;
        }

        public static string CaptureDefaultEvidenceSafe()
        {
            try
            {
                return CaptureDefaultEvidence();
            }
            catch (Exception ex)
            {
                Debug.LogError("[FR5TemplateSlim] Evidence capture failed: " + ex);
                return "ERROR: " + ex.Message;
            }
        }

        public static string CaptureCurrentCamera(string outputPath)
        {
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            if (camera == null)
            {
                throw new InvalidOperationException("Main Camera를 찾지 못했습니다.");
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var renderTexture = RenderTexture.GetTemporary(DefaultWidth, DefaultHeight, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(DefaultWidth, DefaultHeight, TextureFormat.RGB24, false);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0f, 0f, DefaultWidth, DefaultHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }

            return outputPath;
        }

        private static void ResetDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);
            if (camera != null)
            {
                return;
            }

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(-1.39f, 0.63f, -2.35f);
            cameraGo.transform.rotation = Quaternion.Euler(14f, 31f, 0f);

            var mainCamera = cameraGo.GetComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.11f, 0.12f, 0.16f, 1f);
            mainCamera.nearClipPlane = 0.01f;
            mainCamera.farClipPlane = 50f;
        }
    }
}
