using KineTutor3D.App;
using KineTutor3D.Math;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class SandboxSnapshotStoreTests
    {
        [SetUp]
        public void SetUp()
        {
            SandboxSnapshotStore.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SandboxSnapshotStore.Clear();
        }

        [Test]
        public void SaveLoad_LatestSnapshot_Roundtrip()
        {
            var pose = new Pose(new Vec3D(1, 2, 3), Mat3D.Identity);
            SandboxSnapshotStore.SaveSnapshot("SCARA_RV", RobotSelectionBridge.SandboxMode, new[] { 1d, 2d, 3d, 4d }, pose, "demo");

            var snapshot = SandboxSnapshotStore.GetLatestSnapshot("SCARA_RV");
            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.robotId, Is.EqualTo("SCARA_RV"));
            Assert.That(snapshot.note, Is.EqualTo("demo"));
            Assert.That(snapshot.jointValuesDeg.Length, Is.EqualTo(4));
        }
    }
}
