// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class FR5PosePresetsTests
    {
        [Test]
        public void Home_HasSixJoints()
        {
            Assert.AreEqual(6, FR5PosePresets.Home.JointAnglesDeg.Length);
        }

        [Test]
        public void Home_AllZeros()
        {
            var joints = FR5PosePresets.Home.JointAnglesDeg;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, joints[i], 1e-10, $"Home J{i} should be 0");
            }
        }

        [Test]
        public void Ready_HasSixJoints()
        {
            Assert.AreEqual(6, FR5PosePresets.Ready.JointAnglesDeg.Length);
        }

        [Test]
        public void Ready_HasExpectedAngles()
        {
            var joints = FR5PosePresets.Ready.JointAnglesDeg;
            Assert.AreEqual(0.0, joints[0], 1e-10);
            Assert.AreEqual(-45.0, joints[1], 1e-10);
            Assert.AreEqual(0.0, joints[2], 1e-10);
            Assert.AreEqual(-59.0, joints[3], 1e-10);
            Assert.AreEqual(-92.0, joints[4], 1e-10);
            Assert.AreEqual(-42.0, joints[5], 1e-10);
        }

        [Test]
        public void Folded_HasSixJoints()
        {
            Assert.AreEqual(6, FR5PosePresets.Folded.JointAnglesDeg.Length);
        }

        [Test]
        public void All_ContainsFourPresets()
        {
            Assert.AreEqual(4, FR5PosePresets.All.Length);
        }

        [Test]
        public void UpdateCurrent_UpdatesDynamicPreset()
        {
            var angles = new double[] { 10, 20, 30, 40, 50, 60 };
            FR5PosePresets.UpdateCurrent(angles);
            var current = FR5PosePresets.Current;
            Assert.AreEqual("Current", current.Name);
            Assert.AreEqual(10.0, current.JointAnglesDeg[0], 1e-10);
            Assert.AreEqual(60.0, current.JointAnglesDeg[5], 1e-10);

            // 원래 상태로 복원
            FR5PosePresets.UpdateCurrent(new double[] { 0, 0, 0, 0, 0, 0 });
        }

        [Test]
        public void All_PresetNamesAreNotEmpty()
        {
            foreach (var preset in FR5PosePresets.All)
            {
                Assert.IsFalse(string.IsNullOrEmpty(preset.Name), $"Preset name should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(preset.Description), $"Preset description should not be empty");
            }
        }

        [Test]
        public void Presets_AreImmutable()
        {
            var original = FR5PosePresets.Home.JointAnglesDeg;
            var copy = FR5PosePresets.Home.JointAnglesDeg;

            copy[0] = 999.0;

            Assert.AreEqual(0.0, original[0], 1e-10, "Original should not be mutated");
        }

        [Test]
        public void Ready_FKProducesValidPose()
        {
            var facade = FR5KinematicsFacade.Create();
            facade.SetJointAnglesDegrees(FR5PosePresets.Ready.JointAnglesDeg);

            var ee = facade.EndEffectorTransform;
            var pos = ee.ExtractPosition();

            Assert.That(pos.Magnitude() > 0.01, "Ready pose EE should be away from origin");
        }
    }
}
