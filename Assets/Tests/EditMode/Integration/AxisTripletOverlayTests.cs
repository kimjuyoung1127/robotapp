// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// AxisTripletOverlay 동작을 검증하는 EditMode 테스트입니다.
using System.Reflection;
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class AxisTripletOverlayTests
    {
        [Test]
        public void Show_RendersSignedAxisValuesWithUnit()
        {
            var canvasGo = CreateCanvasRoot(new Vector2(640f, 480f));
            var overlayGo = new GameObject("AxisTripletOverlay", typeof(RectTransform));

            try
            {
                var overlay = overlayGo.AddComponent<AxisTripletOverlay>();
                overlay.Initialize(canvasGo.GetComponent<Canvas>(), null);

                overlay.Show(new Vector2(200f, 120f), "ΔTCP", 12.4d, -3.1d, 0d, "mm");

                Assert.That(overlayGo.transform.Find("Title")?.GetComponent<Text>()?.text, Is.EqualTo("ΔTCP"));
                Assert.That(overlayGo.transform.Find("XAxisValue")?.GetComponent<Text>()?.text, Is.EqualTo("dX +12.4 mm"));
                Assert.That(overlayGo.transform.Find("YAxisValue")?.GetComponent<Text>()?.text, Is.EqualTo("dY -3.1 mm"));
                Assert.That(overlayGo.transform.Find("ZAxisValue")?.GetComponent<Text>()?.text, Is.EqualTo("dZ +0.0 mm"));
                Assert.That(overlayGo.GetComponent<CanvasGroup>()?.alpha, Is.EqualTo(1f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(overlayGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void BeginHoldAndFade_HidesOverlayAfterConfiguredDurations()
        {
            var canvasGo = CreateCanvasRoot(new Vector2(640f, 480f));
            var overlayGo = new GameObject("AxisTripletOverlay", typeof(RectTransform));

            try
            {
                var overlay = overlayGo.AddComponent<AxisTripletOverlay>();
                overlay.Initialize(canvasGo.GetComponent<Canvas>(), null);
                overlay.Show(new Vector2(200f, 120f), "ΔTCP", 0d, 0d, 0d, "mm");
                overlay.BeginHoldAndFade();

                InvokeStepAnimation(overlay, 0.81f);
                Assert.That(overlayGo.GetComponent<CanvasGroup>()?.alpha, Is.EqualTo(1f).Within(0.001f));

                InvokeStepAnimation(overlay, UIDesignTokens.Anim.FadeNormal);
                Assert.That(overlayGo.GetComponent<CanvasGroup>()?.alpha, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(overlayGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void Show_ClampsOverlayInsideCanvasBounds()
        {
            var canvasGo = CreateCanvasRoot(new Vector2(400f, 300f));
            var overlayGo = new GameObject("AxisTripletOverlay", typeof(RectTransform));

            try
            {
                var overlay = overlayGo.AddComponent<AxisTripletOverlay>();
                overlay.Initialize(canvasGo.GetComponent<Canvas>(), null);
                overlay.Show(new Vector2(9999f, 9999f), "ΔTCP", 0d, 0d, 0d, "mm");

                var rect = overlayGo.GetComponent<RectTransform>();
                var canvasRect = canvasGo.GetComponent<RectTransform>();
                Assert.That(rect, Is.Not.Null);
                Assert.That(canvasRect, Is.Not.Null);

                var maxX = canvasRect.rect.xMax - (rect.rect.width * 0.5f) - 16f;
                var minX = canvasRect.rect.xMin + (rect.rect.width * 0.5f) + 16f;
                var maxY = canvasRect.rect.yMax - (rect.rect.height * 0.5f) - 16f;
                var minY = canvasRect.rect.yMin + (rect.rect.height * 0.5f) + 16f;

                Assert.That(rect.anchoredPosition.x, Is.InRange(minX, maxX));
                Assert.That(rect.anchoredPosition.y, Is.InRange(minY, maxY));
            }
            finally
            {
                Object.DestroyImmediate(overlayGo);
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void Initialize_WithDifferentCanvas_ReparentsOverlay()
        {
            var firstCanvasGo = CreateCanvasRoot(new Vector2(640f, 480f));
            var secondCanvasGo = CreateCanvasRoot(new Vector2(320f, 240f));
            var overlayGo = new GameObject("AxisTripletOverlay", typeof(RectTransform));

            try
            {
                var overlay = overlayGo.AddComponent<AxisTripletOverlay>();
                overlay.Initialize(firstCanvasGo.GetComponent<Canvas>(), null);
                Assert.That(overlayGo.transform.parent, Is.EqualTo(firstCanvasGo.transform));

                overlay.Initialize(secondCanvasGo.GetComponent<Canvas>(), null);
                Assert.That(overlayGo.transform.parent, Is.EqualTo(secondCanvasGo.transform));
            }
            finally
            {
                Object.DestroyImmediate(overlayGo);
                Object.DestroyImmediate(firstCanvasGo);
                Object.DestroyImmediate(secondCanvasGo);
            }
        }

        [Test]
        public void HideImmediate_OnDestroyedOverlay_DoesNotThrow()
        {
            var canvasGo = CreateCanvasRoot(new Vector2(640f, 480f));
            var overlayGo = new GameObject("AxisTripletOverlay", typeof(RectTransform));
            var overlay = overlayGo.AddComponent<AxisTripletOverlay>();

            try
            {
                overlay.Initialize(canvasGo.GetComponent<Canvas>(), null);
                Object.DestroyImmediate(overlayGo);

                Assert.DoesNotThrow(() => overlay.HideImmediate());
            }
            finally
            {
                if (canvasGo != null)
                {
                    Object.DestroyImmediate(canvasGo);
                }
            }
        }

        private static GameObject CreateCanvasRoot(Vector2 size)
        {
            var canvasGo = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var rect = canvasGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return canvasGo;
        }

        private static void InvokeStepAnimation(AxisTripletOverlay overlay, float deltaTime)
        {
            var method = typeof(AxisTripletOverlay).GetMethod("StepAnimation", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(overlay, new object[] { deltaTime });
        }
    }
}
