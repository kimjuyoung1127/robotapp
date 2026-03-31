// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// SessionContextStore 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class SessionContextStoreTests
    {
        [SetUp]
        public void SetUp()
        {
            SessionContextStore.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SessionContextStore.Clear();
        }

        [Test]
        public void SaveLoad_Roundtrip()
        {
            SessionContextStore.Save(new SessionContextData
            {
                RobotId = "SCARA_RV",
                EntryMode = RobotSelectionBridge.SandboxMode,
                Track = StepProgressSaver.CoreKinematicsTrack,
                Step = 3,
                SceneName = "Sandbox",
                PresetId = "demo"
            });

            Assert.That(SessionContextStore.TryLoad(out var data), Is.True);
            Assert.That(data.RobotId, Is.EqualTo("SCARA_RV"));
            Assert.That(data.EntryMode, Is.EqualTo(RobotSelectionBridge.SandboxMode));
            Assert.That(data.Step, Is.EqualTo(3));
        }
    }
}
