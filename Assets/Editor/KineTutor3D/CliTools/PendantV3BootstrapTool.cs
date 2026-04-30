// Folder: Editor/CliTools - Pendant V3 Phase 0A asset bootstrap
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.U2D;
using UnityCliConnector;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.U2D;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Ensures the minimum Phase 0A assets for Pendant V3 exist and carry the locked defaults.
    /// </summary>
    [UnityCliTool(Description = "Ensure Pendant V3 Phase 0A UI Toolkit assets")]
    public static class PendantV3BootstrapTool
    {
        private const string MenuPath = "KineTutor3D/RobotControl/Ensure V3 Phase 0A Assets";
        private const string RootFolder = "Assets/UI/PendantV3";
        private const string PanelSettingsFolder = RootFolder + "/PanelSettings";
        private const string IconsFolder = RootFolder + "/icons";

        private const string PanelSettingsPath = PanelSettingsFolder + "/PendantV3PanelSettings.asset";
        private const string TextSettingsPath = PanelSettingsFolder + "/PendantV3TextSettings.asset";
        private const string SpriteAtlasPath = IconsFolder + "/PendantV3.spriteatlas";

        public static object HandleCommand(JObject @params)
        {
            return new JObject
            {
                ["message"] = EnsurePhase0Assets()
            };
        }

        [MenuItem(MenuPath, priority = 171)]
        public static void EnsurePhase0AssetsMenu()
        {
            var message = EnsurePhase0Assets();
            Debug.Log("[PendantV3BootstrapTool] " + message);
        }

        public static string EnsurePhase0Assets()
        {
            EnsureFolder("Assets/UI", "PendantV3");
            EnsureFolder(RootFolder, "PanelSettings");
            EnsureFolder(RootFolder, "icons");
            EnsureFolder(RootFolder, "popups");

            var panelSettings = LoadOrCreatePanelSettings();
            var textSettings = LoadOrCreateTextSettings();
            var spriteAtlas = LoadOrCreateSpriteAtlas();

            ApplyLockedPanelSettings(panelSettings, textSettings);
            ApplyLockedTextSettings(textSettings);
            ApplyLockedSpriteAtlas(spriteAtlas);

            EditorUtility.SetDirty(panelSettings);
            EditorUtility.SetDirty(textSettings);
            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return "PendantV3 Phase 0A assets ensured.";
        }

        private static void EnsureFolder(string parent, string name)
        {
            var target = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(target))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static PanelSettings LoadOrCreatePanelSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(asset, PanelSettingsPath);
            return asset;
        }

        private static TMP_Settings LoadOrCreateTextSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TextSettingsPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<TMP_Settings>();
            AssetDatabase.CreateAsset(asset, TextSettingsPath);
            return asset;
        }

        private static SpriteAtlas LoadOrCreateSpriteAtlas()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(SpriteAtlasPath);
            if (asset != null)
            {
                return asset;
            }

            asset = new SpriteAtlas();
            AssetDatabase.CreateAsset(asset, SpriteAtlasPath);
            return asset;
        }

        private static void ApplyLockedPanelSettings(PanelSettings panelSettings, TMP_Settings textSettings)
        {
            var serialized = new SerializedObject(panelSettings);
            serialized.FindProperty("m_ScaleMode").intValue = 2;
            serialized.FindProperty("m_ReferenceResolution").vector2IntValue = new Vector2Int(1920, 1080);
            serialized.FindProperty("m_Match").floatValue = 0.5f;
            serialized.FindProperty("m_SortingOrder").intValue = 100;
            serialized.FindProperty("m_ClearDepthStencil").boolValue = true;
            serialized.FindProperty("m_ClearColor").boolValue = false;
            serialized.FindProperty("textSettings").objectReferenceValue = textSettings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyLockedTextSettings(TMP_Settings textSettings)
        {
            var serialized = new SerializedObject(textSettings);
            var displayWarnings = serialized.FindProperty("m_DisplayWarnings");
            if (displayWarnings != null)
            {
                displayWarnings.boolValue = true;
            }

            var lineBreaking = serialized.FindProperty("m_UnicodeLineBreakingRules");
            if (lineBreaking != null)
            {
                var modernHangul = lineBreaking.FindPropertyRelative("m_UseModernHangulLineBreakingRules");
                if (modernHangul != null)
                {
                    modernHangul.boolValue = true;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyLockedSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            var packing = spriteAtlas.GetPackingSettings();
            packing.enableRotation = false;
            packing.enableTightPacking = false;
            packing.padding = 4;
            spriteAtlas.SetPackingSettings(packing);

            var texture = spriteAtlas.GetTextureSettings();
            texture.readable = false;
            texture.generateMipMaps = false;
            texture.sRGB = true;
            texture.filterMode = FilterMode.Bilinear;
            spriteAtlas.SetTextureSettings(texture);

            SpriteAtlasExtensions.SetIncludeInBuild(spriteAtlas, true);
        }
    }
}
