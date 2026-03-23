// Editor-only: author the FR5 slim template demo scene.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KineTutor3D.Editor
{
    internal static class Fr5TemplateSlimSceneBuilder
    {
        private const string MenuPath = "KineTutor3D/Export/Author FR5 Slim Template Demo Scene";

        [MenuItem(MenuPath, priority = 161)]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("FR5TemplateDemoRoot");
            root.AddComponent<KineTutor3D.App.FR5TemplateMinimalController>();

            var directory = Path.GetDirectoryName(KineTutor3D.App.FR5TemplateSlimManifest.DemoScenePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            EditorSceneManager.SaveScene(scene, KineTutor3D.App.FR5TemplateSlimManifest.DemoScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FR5TemplateSlim] Demo scene authored and saved.");
        }
    }
}
