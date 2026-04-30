using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.EditorTools
{
    /// <summary>
    /// Exposes minimal Game view diagnostics for macOS editor troubleshooting.
    /// </summary>
    public static class PendantV3GameViewTool
    {
        public static string GetSummary()
        {
            var gameView = GetGameView();
            if (gameView == null)
            {
                return "GameView missing";
            }

            var selectedSizeIndex = GetValue<int>(gameView, "selectedSizeIndex");
            var targetSize = GetValue<Vector2>(gameView, "targetSize");
            var targetRenderSize = GetValue<Vector2>(gameView, "targetRenderSize");
            var viewMouseScale = GetValue<float>(gameView, "viewMouseScale");
            var zoomAreaScale = GetValue<Vector2>(gameView, "zoomAreaScale");
            var backingScale = GetValue<float>(gameView, "backingScale");
            var minScale = GetValue<float>(gameView, "minScale");
            var maxScale = GetValue<float>(gameView, "maxScale");

            return string.Join("; ",
                $"type={gameView.GetType().FullName}",
                $"selectedSizeIndex={selectedSizeIndex}",
                $"targetSize={targetSize}",
                $"targetRenderSize={targetRenderSize}",
                $"viewMouseScale={viewMouseScale:0.###}",
                $"zoomAreaScale={zoomAreaScale}",
                $"backingScale={backingScale:0.###}",
                $"minScale={minScale:0.###}",
                $"maxScale={maxScale:0.###}");
        }

        public static string SetViewScale(float scale)
        {
            var gameView = GetGameView();
            if (gameView == null)
            {
                return "GameView missing";
            }

            SetValue(gameView, "viewMouseScale", scale);
            Invoke(gameView, "UpdateZoomAreaAndParent");
            Invoke(gameView, "EnforceZoomAreaConstraints");
            gameView.Repaint();
            return GetSummary();
        }

        private static EditorWindow GetGameView()
        {
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            return gameViewType == null ? null : EditorWindow.GetWindow(gameViewType);
        }

        private static T GetValue<T>(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                return default;
            }

            return property.GetValue(target) is T typed ? typed : default;
        }

        private static void SetValue(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(target, value);
        }

        private static void Invoke(object target, string methodName)
        {
            var method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            method?.Invoke(target, null);
        }
    }
}
