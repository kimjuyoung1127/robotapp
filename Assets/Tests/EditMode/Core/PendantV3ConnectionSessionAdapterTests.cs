// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// PendantV3ConnectionSessionAdapter 상태 전이를 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class PendantV3ConnectionSessionAdapterTests
    {
        private GameObject root;
        private PendantV3ConnectionSessionAdapter adapter;

        [SetUp]
        public void SetUp()
        {
            RobotSelectionBridge.Clear();
            RobotSelectionBridge.SetSelection("FAIRINO_FR5", RobotSelectionBridge.RobotControlMode);
            root = new GameObject("PendantV3ConnectionSessionAdapterTests");
            adapter = root.AddComponent<PendantV3ConnectionSessionAdapter>();
            Assert.That(adapter.ForceInitialize(), Is.True, "adapter should initialize with FR5 selection");
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
        public void ForceInitialize_DefaultsToConnectedServoOff()
        {
            var state = adapter.CurrentState;

            Assert.That(state.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.ConnectedServoOff));
            Assert.That(state.IsConnected, Is.True);
            Assert.That(state.IsEnabled, Is.False);
            Assert.That(state.ReconnectActive, Is.False);
        }

        [Test]
        public void ApplyServoEnableAndSync_TransitionsToReadyToJog()
        {
            adapter.ApplyServoEnablePolicy();
            Assert.That(adapter.CurrentState.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.ConnectedUnsynced));
            Assert.That(adapter.CurrentState.IsEnabled, Is.True);

            adapter.ApplySyncPolicy();

            Assert.That(adapter.CurrentState.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.ReadyToJog));
            Assert.That(adapter.CurrentState.HasSynced, Is.True);
        }

        [Test]
        public void TriggerConnectionLostForDebug_TransitionsThroughReconnectAndFailure()
        {
            adapter.TriggerConnectionLostForDebug();

            Assert.That(adapter.CurrentState.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.AutoReconnect));
            Assert.That(adapter.CurrentState.ReconnectActive, Is.True);

            adapter.AdvanceReconnectTickForDebug(3f);
            Assert.That(adapter.CurrentState.ReconnectActive, Is.False);
            Assert.That(adapter.CurrentState.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.ConnectedServoOff));

            adapter.TriggerConnectionLostForDebug();
            adapter.CompleteReconnectForDebug(false);

            Assert.That(adapter.CurrentState.DisplayKind, Is.EqualTo(PendantV3ConnectionDisplayKind.Disconnected));
            Assert.That(adapter.CurrentState.ReconnectFailed, Is.True);
            Assert.That(adapter.CurrentState.ReconnectFailureSummary, Does.Contain("수동 연결"));
        }

        [Test]
        public void LiveArm_RequiresLiveModeAndServoEnabled()
        {
            Assert.That(adapter.SetLiveArmState(true), Is.False);
            Assert.That(adapter.CurrentState.IsLiveArmActive, Is.False);
            Assert.That(adapter.CurrentState.ActualMoveAllowed, Is.False);

            adapter.SetMockMode(false);
            Assert.That(adapter.CurrentState.IsMockMode, Is.False);
            Assert.That(adapter.SetLiveArmState(true), Is.False);
            Assert.That(adapter.CurrentState.ActualMoveAllowed, Is.False);
            Assert.That(adapter.CurrentState.ActualMoveBlockReason, Is.Not.Empty);
        }
    }
}
