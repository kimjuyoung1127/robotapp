using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControl UX 레이아웃과 진단 서랍 배선을 검증합니다.
    /// </summary>
    public class FairinoRobotControlUxTests
    {
        [Test]
        public void EnsureLayout_CreatesDiagnosticsDrawer_HiddenByDefault()
        {
            var canvasGo = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            try
            {
                var canvas = canvasGo.GetComponent<Canvas>();

                FairinoRobotControlViewBuilder.EnsureLayout(
                    canvas,
                    null,
                    out _,
                    out _,
                    out _,
                    out _,
                    out var diagnosticsDrawer,
                    out _,
                    out _,
                    out var diagnosticsButton,
                    out _,
                    out _);

                Assert.That(diagnosticsDrawer, Is.Not.Null);
                Assert.That(diagnosticsDrawer.gameObject.activeSelf, Is.False);
                Assert.That(diagnosticsButton, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void EnsureLayout_CreatesDiagnosticsButton_InTopBar()
        {
            var canvasGo = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            try
            {
                var canvas = canvasGo.GetComponent<Canvas>();
                FairinoRobotControlViewBuilder.EnsureLayout(
                    canvas,
                    null,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _,
                    out var diagnosticsButton,
                    out _,
                    out _);

                Assert.That(diagnosticsButton, Is.Not.Null);
                Assert.That(diagnosticsButton.transform.parent.name, Is.EqualTo("TopBar"));
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void TryBindExistingLayout_ReusesSceneAuthoredShell()
        {
            var canvasGo = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            try
            {
                var canvas = canvasGo.GetComponent<Canvas>();
                FairinoRobotControlViewBuilder.EnsureLayout(
                    canvas,
                    null,
                    out var connectionPanel,
                    out var jointPanel,
                    out var statePanel,
                    out var tcpPanel,
                    out var diagnosticsDrawer,
                    out var whyLabel,
                    out var moveDialog,
                    out var diagnosticsButton,
                    out var gizmoToggle,
                    out var clearTrailButton);

                var bound = FairinoRobotControlViewBuilder.TryBindExistingLayout(
                    canvas,
                    out var reboundConnectionPanel,
                    out var reboundJointPanel,
                    out var reboundStatePanel,
                    out var reboundTcpPanel,
                    out var reboundDiagnosticsDrawer,
                    out var reboundWhyLabel,
                    out var reboundMoveDialog,
                    out var reboundDiagnosticsButton,
                    out var reboundGizmoToggle,
                    out var reboundClearTrailButton);

                Assert.That(bound, Is.True);
                Assert.That(reboundConnectionPanel, Is.SameAs(connectionPanel));
                Assert.That(reboundJointPanel, Is.SameAs(jointPanel));
                Assert.That(reboundStatePanel, Is.SameAs(statePanel));
                Assert.That(reboundTcpPanel, Is.SameAs(tcpPanel));
                Assert.That(reboundDiagnosticsDrawer, Is.SameAs(diagnosticsDrawer));
                Assert.That(reboundWhyLabel, Is.SameAs(whyLabel));
                Assert.That(reboundMoveDialog, Is.SameAs(moveDialog));
                Assert.That(reboundDiagnosticsButton, Is.SameAs(diagnosticsButton));
                Assert.That(reboundGizmoToggle, Is.SameAs(gizmoToggle));
                Assert.That(reboundClearTrailButton, Is.SameAs(clearTrailButton));
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }
    }
}
