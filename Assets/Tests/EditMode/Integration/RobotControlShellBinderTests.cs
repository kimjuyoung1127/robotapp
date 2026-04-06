// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
using KineTutor3D.App.Fairino;
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControlShellBinder가 기존 authored RectTransform을 덮어쓰지 않는지 검증합니다.
    /// </summary>
    public class RobotControlShellBinderTests
    {
        [Test]
        public void EnsureShellStructure_KeepsExistingAuthoredRects()
        {
            var shellRoot = new GameObject("RobotControlShell", typeof(RectTransform));

            try
            {
                var shellRect = shellRoot.GetComponent<RectTransform>();
                var safeArea = CreateChild(shellRect, "SafeArea");
                var leftRail = CreateChild(safeArea, "LeftRail");
                var workPanelHost = CreateChild(leftRail, "WorkPanelHost");
                var jointJogPanel = CreateChild(workPanelHost, "JointJogPanel");

                SetRect(leftRail, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(18f, 20f), new Vector2(352f, -120f));
                SetRect(workPanelHost, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 24f), new Vector2(-18f, -182f));
                SetRect(jointJogPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(321f, 577f), new Vector2(14f, -91f));

                var leftRailSnapshot = Snapshot(leftRail);
                var workPanelHostSnapshot = Snapshot(workPanelHost);
                var jointJogPanelSnapshot = Snapshot(jointJogPanel);

                var binder = shellRoot.AddComponent<RobotControlShellBinder>();
                LogAssert.ignoreFailingMessages = true;
                binder.EnsureShellStructure();
                LogAssert.ignoreFailingMessages = false;

                AssertRectEquals(leftRailSnapshot, leftRail, "LeftRail authored rect should remain unchanged.");
                AssertRectEquals(workPanelHostSnapshot, workPanelHost, "WorkPanelHost authored rect should remain unchanged.");
                AssertRectEquals(jointJogPanelSnapshot, jointJogPanel, "JointJogPanel authored rect should remain unchanged.");
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
                Object.DestroyImmediate(shellRoot);
            }
        }

        [Test]
        public void Bind_NormalizesAllV2TextsToUniformSize()
        {
            var canvasRoot = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            try
            {
                var canvas = canvasRoot.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var shell = RobotControlShell.EnsureV2Shell(canvas, null, "로봇 제어 V2", "모의 연결");
                Assert.That(shell, Is.Not.Null);

                shell.Bind(RobotControlViewState.CreateDefault());

                var texts = shell.GetComponentsInChildren<Text>(true);
                Assert.That(texts.Length, Is.GreaterThan(0), "RobotControl V2 shell should create text nodes.");

                for (var i = 0; i < texts.Length; i++)
                {
                    var text = texts[i];
                    var path = GetPath(shell.transform, text.transform);
                    Assert.AreEqual(UIDesignTokens.RobotControlV2.Type.UniformText, text.fontSize, $"Expected uniform font size on {path}.");

                    if (text.resizeTextForBestFit)
                    {
                        Assert.AreEqual(UIDesignTokens.RobotControlV2.Type.UniformText, text.resizeTextMinSize, $"Expected uniform min size on {path}.");
                        Assert.AreEqual(UIDesignTokens.RobotControlV2.Type.UniformText, text.resizeTextMaxSize, $"Expected uniform max size on {path}.");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(canvasRoot);
            }
        }

        private static RectTransform CreateChild(RectTransform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            child.SetParent(parent, false);
            return child;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static RectSnapshot Snapshot(RectTransform rect)
        {
            return new RectSnapshot(
                rect.anchorMin,
                rect.anchorMax,
                rect.pivot,
                rect.anchoredPosition,
                rect.sizeDelta,
                rect.offsetMin,
                rect.offsetMax);
        }

        private static void AssertRectEquals(RectSnapshot expected, RectTransform actual, string message)
        {
            Assert.That(actual.anchorMin, Is.EqualTo(expected.AnchorMin), message + " anchorMin");
            Assert.That(actual.anchorMax, Is.EqualTo(expected.AnchorMax), message + " anchorMax");
            Assert.That(actual.pivot, Is.EqualTo(expected.Pivot), message + " pivot");
            Assert.That(actual.anchoredPosition, Is.EqualTo(expected.AnchoredPosition), message + " anchoredPosition");
            Assert.That(actual.sizeDelta, Is.EqualTo(expected.SizeDelta), message + " sizeDelta");
            Assert.That(actual.offsetMin, Is.EqualTo(expected.OffsetMin), message + " offsetMin");
            Assert.That(actual.offsetMax, Is.EqualTo(expected.OffsetMax), message + " offsetMax");
        }

        private static string GetPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            if (root == target)
            {
                return root.name;
            }

            var names = new System.Collections.Generic.List<string>();
            var current = target;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Add(root.name);
            names.Reverse();
            return string.Join("/", names);
        }

        private readonly struct RectSnapshot
        {
            public RectSnapshot(
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 pivot,
                Vector2 anchoredPosition,
                Vector2 sizeDelta,
                Vector2 offsetMin,
                Vector2 offsetMax)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                Pivot = pivot;
                AnchoredPosition = anchoredPosition;
                SizeDelta = sizeDelta;
                OffsetMin = offsetMin;
                OffsetMax = offsetMax;
            }

            public Vector2 AnchorMin { get; }

            public Vector2 AnchorMax { get; }

            public Vector2 Pivot { get; }

            public Vector2 AnchoredPosition { get; }

            public Vector2 SizeDelta { get; }

            public Vector2 OffsetMin { get; }

            public Vector2 OffsetMax { get; }
        }
    }
}
