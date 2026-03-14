// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App.Fairino;
using KineTutor3D.Math;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// FR5KinematicsFacade와 MockFairinoClient의 통합 시나리오 테스트입니다.
    /// Connect→Enable→MoveJ→FK→Delta 전체 흐름을 검증합니다.
    /// </summary>
    [TestFixture]
    public class FR5KinematicsFacadeIntegrationTests
    {
        private const double Deg2Rad = System.Math.PI / 180.0;
        private const double PositionDelta = 1e-4;

        private MockFairinoClient client;
        private FR5KinematicsFacade facade;

        [SetUp]
        public void SetUp()
        {
            client = new MockFairinoClient();
            facade = new FR5KinematicsFacade();
        }

        [Test]
        public void MockConnect_MoveJ_FKReflectsState()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var target = new double[] { 10, -20, 30, -40, 50, -60 };
            client.MoveJ(target, 30, 50);

            var stateResult = client.ReadState();
            Assert.IsTrue(stateResult.IsSuccess);

            facade.SetJointAnglesDegrees(stateResult.Value.JointPosDeg);

            var joints = facade.JointValuesRad;
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(target[i] * Deg2Rad, joints[i], 1e-10, $"Joint {i} should match MoveJ target");
            }
        }

        [Test]
        public void MockConnect_MoveJ_EEMoves()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var eeAtZero = facade.EndEffectorTransform.ExtractPosition();

            var target = new double[] { 45, -30, 60, 0, 0, 0 };
            client.MoveJ(target, 30, 50);
            var stateResult = client.ReadState();
            facade.SetJointAnglesDegrees(stateResult.Value.JointPosDeg);

            var eeAfterMove = facade.EndEffectorTransform.ExtractPosition();
            var displacement = eeAfterMove - eeAtZero;

            Assert.That(displacement.Magnitude() > 0.01, "EE should move significantly after MoveJ");
        }

        [Test]
        public void DhEdit_ChangesFK()
        {
            var eeOriginal = facade.EndEffectorTransform.ExtractPosition();

            facade.SetDhParameter(0, "d", 1.0);

            var eeAfterEdit = facade.EndEffectorTransform.ExtractPosition();
            var delta = eeAfterEdit - eeOriginal;

            Assert.That(delta.Magnitude() > 0.1, "DH change should shift EE significantly");
        }

        [Test]
        public void PresetApply_ThenReset_RestoresOriginal()
        {
            facade.SetJointAnglesDegrees(FR5PosePresets.Ready.JointAnglesDeg);
            var eeReady = facade.EndEffectorTransform.ExtractPosition();

            facade.ResetToDefault();
            var eeReset = facade.EndEffectorTransform.ExtractPosition();

            var eeZero = new FR5KinematicsFacade().EndEffectorTransform.ExtractPosition();
            Assert.AreEqual(eeZero.X, eeReset.X, PositionDelta);
            Assert.AreEqual(eeZero.Y, eeReset.Y, PositionDelta);
            Assert.AreEqual(eeZero.Z, eeReset.Z, PositionDelta);
        }

        [Test]
        public void CumulativeTransforms_AllValid()
        {
            facade.SetJointAnglesDegrees(new double[] { 10, 20, 30, 40, 50, 60 });

            var transforms = facade.CumulativeTransforms;
            Assert.AreEqual(6, transforms.Length);

            for (var i = 0; i < transforms.Length; i++)
            {
                var pos = transforms[i].ExtractPosition();
                Assert.IsFalse(double.IsNaN(pos.X), $"T{i} X should not be NaN");
                Assert.IsFalse(double.IsNaN(pos.Y), $"T{i} Y should not be NaN");
                Assert.IsFalse(double.IsNaN(pos.Z), $"T{i} Z should not be NaN");
            }
        }

        [Test]
        public void SequentialMoveJ_ProducesConsistentFK()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var targets = new[]
            {
                new double[] { 0, 0, 0, 0, 0, 0 },
                new double[] { 10, 0, 0, 0, 0, 0 },
                new double[] { 10, 20, 0, 0, 0, 0 },
                new double[] { 10, 20, 30, 0, 0, 0 },
            };

            Vec3D previousEE = facade.EndEffectorTransform.ExtractPosition();

            for (var step = 1; step < targets.Length; step++)
            {
                client.MoveJ(targets[step], 30, 50);
                var state = client.ReadState();
                facade.SetJointAnglesDegrees(state.Value.JointPosDeg);

                var currentEE = facade.EndEffectorTransform.ExtractPosition();
                var delta = currentEE - previousEE;

                Assert.IsFalse(double.IsNaN(delta.Magnitude()),
                    $"Step {step} displacement should not be NaN");

                previousEE = currentEE;
            }
        }
    }
}
