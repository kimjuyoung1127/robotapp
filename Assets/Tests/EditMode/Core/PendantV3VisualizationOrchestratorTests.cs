// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// PendantV3VisualizationOrchestrator 시각 상태 합성을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class PendantV3VisualizationOrchestratorTests
    {
        private GameObject root;
        private PendantV3ConnectionSessionAdapter adapter;
        private PendantV3VisualizationOrchestrator orchestrator;

        [SetUp]
        public void SetUp()
        {
            RobotSelectionBridge.Clear();
            RobotSelectionBridge.SetSelection("FAIRINO_FR5", RobotSelectionBridge.RobotControlMode);
            root = new GameObject("PendantV3VisualizationOrchestratorTests");
            adapter = root.AddComponent<PendantV3ConnectionSessionAdapter>();
            orchestrator = root.AddComponent<PendantV3VisualizationOrchestrator>();
            Assert.That(adapter.ForceInitialize(), Is.True);
            Assert.That(orchestrator.ForceInitialize(), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            RobotSelectionBridge.Clear();
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PreviewJointPose_EnablesGhostPreview()
        {
            orchestrator.PreviewJointPose(new double[] { 10d, -20d, 30d, 0d, 0d, 0d }, 2, "joint ghost", false);

            Assert.That(orchestrator.CurrentState.CurrentPreviewKind, Is.EqualTo(PendantV3VisualizationState.PreviewKind.JointGhost));
            Assert.That(orchestrator.CurrentState.ShowGhost, Is.True);
            Assert.That(orchestrator.CurrentState.ActiveJointIndex, Is.EqualTo(2));
        }

        [Test]
        public void PreviewTcpTarget_UsesTargetPathWithoutGhost()
        {
            orchestrator.PreviewTcpTarget(new double[] { -500d, -130d, 480d, 180d, 0d, 90d }, 0, 1, "Tool", "tcp target", false);

            Assert.That(orchestrator.CurrentState.CurrentPreviewKind, Is.EqualTo(PendantV3VisualizationState.PreviewKind.TcpTarget));
            Assert.That(orchestrator.CurrentState.ShowGhost, Is.False);
            Assert.That(orchestrator.CurrentState.CoordSystem, Is.EqualTo("Tool"));
            Assert.That(orchestrator.CurrentState.ActiveTcpAxisIndex, Is.EqualTo(0));
        }

        [Test]
        public void RefreshFromSession_UsesAdapterConnectionState()
        {
            adapter.ApplyServoEnablePolicy();

            orchestrator.RefreshFromSession();

            Assert.That(orchestrator.CurrentState.CurrentPreviewKind, Is.EqualTo(PendantV3VisualizationState.PreviewKind.None));
            Assert.That(orchestrator.CurrentState.ActualMoveAllowed, Is.False);
            Assert.That(orchestrator.CurrentState.RiskSummary, Is.EqualTo("동기화 후 재확인"));
        }
    }
}
