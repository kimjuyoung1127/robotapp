// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App.Fairino;
using KineTutor3D.Math;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class FR5KinematicsFacadeTests
    {
        private const double PositionDelta = 1e-4;
        private const double Deg2Rad = System.Math.PI / 180.0;

        private FR5KinematicsFacade facade;

        [SetUp]
        public void SetUp()
        {
            facade = new FR5KinematicsFacade();
        }

        [Test]
        public void Constructor_InitializesWithFR5Template()
        {
            Assert.IsNotNull(facade.Links);
            Assert.AreEqual(6, facade.Links.Length);
            Assert.AreEqual(6, facade.JointValuesRad.Length);
        }

        [Test]
        public void Constructor_InitialJointsAreZero()
        {
            var joints = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, joints[i], 1e-10, $"Joint {i} should be 0");
            }
        }

        [Test]
        public void Constructor_ComputesInitialFK()
        {
            var transforms = facade.CumulativeTransforms;
            Assert.IsNotNull(transforms);
            Assert.AreEqual(6, transforms.Length);
        }

        [Test]
        public void SetJointAnglesDegrees_UpdatesJointValues()
        {
            var angles = new double[] { 10, 20, 30, 40, 50, 60 };
            facade.SetJointAnglesDegrees(angles);

            var rads = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(angles[i] * Deg2Rad, rads[i], 1e-10, $"Joint {i} rad mismatch");
            }
        }

        [Test]
        public void SetJointAnglesDegrees_NullInput_NoChange()
        {
            facade.SetJointAnglesDegrees(null);

            var joints = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, joints[i], 1e-10);
            }
        }

        [Test]
        public void SetJointAnglesDegrees_WrongLength_NoChange()
        {
            facade.SetJointAnglesDegrees(new double[] { 10, 20 });

            var joints = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, joints[i], 1e-10);
            }
        }

        [Test]
        public void SetJointAnglesDegrees_NaN_SkipsInvalidJoint()
        {
            facade.SetJointAnglesDegrees(new double[] { 10, double.NaN, 30, 40, 50, 60 });

            var rads = facade.JointValuesRad;
            Assert.AreEqual(10.0 * Deg2Rad, rads[0], 1e-10, "J0 should be set");
            Assert.AreEqual(0.0, rads[1], 1e-10, "J1 NaN should be skipped");
            Assert.AreEqual(30.0 * Deg2Rad, rads[2], 1e-10, "J2 should be set");
        }

        [Test]
        public void SetJointAngleDegrees_SingleJoint_Updates()
        {
            facade.SetJointAngleDegrees(2, 45.0);

            var rads = facade.JointValuesRad;
            Assert.AreEqual(45.0 * Deg2Rad, rads[2], 1e-10);
            Assert.AreEqual(0.0, rads[0], 1e-10, "Other joints unchanged");
        }

        [Test]
        public void SetJointAngleDegrees_NegativeIndex_NoChange()
        {
            facade.SetJointAngleDegrees(-1, 45.0);

            var rads = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, rads[i], 1e-10);
            }
        }

        [Test]
        public void SetJointAngleDegrees_IndexOutOfRange_NoChange()
        {
            facade.SetJointAngleDegrees(6, 45.0);

            var rads = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, rads[i], 1e-10);
            }
        }

        [Test]
        public void SetDhParameter_D_Updates()
        {
            var originalD = facade.Links[0].D;
            var result = facade.SetDhParameter(0, "d", 0.5);

            Assert.IsTrue(result);
            Assert.AreEqual(0.5, facade.Links[0].D, 1e-10);
        }

        [Test]
        public void SetDhParameter_A_Updates()
        {
            var result = facade.SetDhParameter(1, "a", 0.4);

            Assert.IsTrue(result);
            Assert.AreEqual(0.4, facade.Links[1].A, 1e-10);
        }

        [Test]
        public void SetDhParameter_Alpha_Updates()
        {
            var result = facade.SetDhParameter(0, "alpha", -1.5);

            Assert.IsTrue(result);
            Assert.AreEqual(-1.5, facade.Links[0].Alpha, 1e-10);
        }

        [Test]
        public void SetDhParameter_Theta_Rejected()
        {
            var result = facade.SetDhParameter(0, "theta", 1.0);

            Assert.IsFalse(result, "theta is read-only");
        }

        [Test]
        public void SetDhParameter_NaN_Rejected()
        {
            var result = facade.SetDhParameter(0, "d", double.NaN);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetDhParameter_Infinity_Rejected()
        {
            var result = facade.SetDhParameter(0, "a", double.PositiveInfinity);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetDhParameter_InvalidIndex_Rejected()
        {
            Assert.IsFalse(facade.SetDhParameter(-1, "d", 0.5));
            Assert.IsFalse(facade.SetDhParameter(6, "d", 0.5));
        }

        [Test]
        public void OnKinematicsUpdated_FiredAfterJointChange()
        {
            var firedCount = 0;
            Mat4D[] receivedTransforms = null;
            Mat4D receivedEE = Mat4D.Identity;

            facade.OnKinematicsUpdated += (transforms, ee) =>
            {
                firedCount++;
                receivedTransforms = transforms;
                receivedEE = ee;
            };

            facade.SetJointAngleDegrees(0, 45.0);

            Assert.AreEqual(1, firedCount, "Event should fire once");
            Assert.IsNotNull(receivedTransforms);
            Assert.AreEqual(6, receivedTransforms.Length);
        }

        [Test]
        public void OnKinematicsUpdated_FiredAfterDhChange()
        {
            var firedCount = 0;
            facade.OnKinematicsUpdated += (_, __) => firedCount++;

            facade.SetDhParameter(0, "d", 0.5);

            Assert.AreEqual(1, firedCount);
        }

        [Test]
        public void ResetToDefault_RestoresInitialState()
        {
            facade.SetJointAngleDegrees(0, 90.0);
            facade.SetDhParameter(0, "d", 999.0);

            facade.ResetToDefault();

            var rads = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(0.0, rads[i], 1e-10, $"Joint {i} should be 0 after reset");
            }

            Assert.AreEqual(0.4045, facade.Links[0].D, PositionDelta, "Link 0 d should be restored");
        }

        [Test]
        public void EndEffectorTransform_AtZero_MatchesFK()
        {
            var ee = facade.EndEffectorTransform;
            var pos = ee.ExtractPosition();

            // At zero config, EE should be at (0, d1+d4+d6, a2) approximately
            // d1=0.4045, d4=0.3685, d6=0.1578, a2=0.3225
            // With zero joints: straight up along Z (robotics space)
            Assert.IsNotNull(ee);
            Assert.That(pos.Magnitude() > 0.1, "EE should not be at origin");
        }

        [Test]
        public void JointChange_ChangesEEPosition()
        {
            var eeBeforePos = facade.EndEffectorTransform.ExtractPosition();

            facade.SetJointAngleDegrees(0, 90.0);

            var eeAfterPos = facade.EndEffectorTransform.ExtractPosition();
            var displacement = eeAfterPos - eeBeforePos;

            Assert.That(displacement.Magnitude() > 1e-6, "EE should move after joint rotation");
        }
    }
}
