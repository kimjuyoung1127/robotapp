// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class TemplateUR5eTests
    {
        private const double PositionDelta = 1e-4;
        private const double AngleDelta = 1e-6;

        [Test]
        public void Create_Returns6DofTemplate()
        {
            var template = TemplateUR5e.Create();

            Assert.IsNotNull(template);
            Assert.AreEqual(6, template.Dof);
            Assert.AreEqual("UR5e", template.Name);
        }

        [Test]
        public void Create_AllJointsAreRevolute()
        {
            var template = TemplateUR5e.Create();

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(JointType.Revolute, template.GetLink(i).JointType,
                    $"Joint {i} should be Revolute");
            }
        }

        [Test]
        public void Create_DHParametersMatch_Link0()
        {
            var template = TemplateUR5e.Create();
            var link0 = template.GetLink(0);

            Assert.AreEqual(0.1625, link0.D, PositionDelta, "Link 0 d should be 0.1625");
            Assert.AreEqual(0.0, link0.A, PositionDelta, "Link 0 a should be 0");
            Assert.AreEqual(System.Math.PI / 2, link0.Alpha, AngleDelta, "Link 0 alpha should be pi/2");
        }

        [Test]
        public void Create_DHParametersMatch_Link1()
        {
            var template = TemplateUR5e.Create();
            var link1 = template.GetLink(1);

            Assert.AreEqual(0.0, link1.D, PositionDelta, "Link 1 d should be 0");
            Assert.AreEqual(-0.425, link1.A, PositionDelta, "Link 1 a should be -0.425");
            Assert.AreEqual(0.0, link1.Alpha, AngleDelta, "Link 1 alpha should be 0");
        }

        [Test]
        public void Create_DHParametersMatch_Link2()
        {
            var template = TemplateUR5e.Create();
            var link2 = template.GetLink(2);

            Assert.AreEqual(0.0, link2.D, PositionDelta, "Link 2 d should be 0");
            Assert.AreEqual(-0.3922, link2.A, PositionDelta, "Link 2 a should be -0.3922");
        }

        [Test]
        public void Create_DHParametersMatch_Link3to5()
        {
            var template = TemplateUR5e.Create();

            var link3 = template.GetLink(3);
            Assert.AreEqual(0.1333, link3.D, PositionDelta, "Link 3 d should be 0.1333");

            var link4 = template.GetLink(4);
            Assert.AreEqual(0.0997, link4.D, PositionDelta, "Link 4 d should be 0.0997");

            var link5 = template.GetLink(5);
            Assert.AreEqual(0.0996, link5.D, PositionDelta, "Link 5 d should be 0.0996");
        }

        [Test]
        public void Create_JointLimitsAreValid()
        {
            var template = TemplateUR5e.Create();

            for (int i = 0; i < 6; i++)
            {
                var limit = template.GetJointLimit(i);
                Assert.That(limit.Min < limit.Max, $"Joint {i} min should be less than max");
            }
        }

        [Test]
        public void Create_Joint2HasRestrictedLimits()
        {
            var template = TemplateUR5e.Create();
            var limit2 = template.GetJointLimit(2);
            var pi = System.Math.PI;

            Assert.AreEqual(-pi, limit2.Min, AngleDelta, "J3 min should be -pi (180 deg)");
            Assert.AreEqual(pi, limit2.Max, AngleDelta, "J3 max should be +pi (180 deg)");
        }

        [Test]
        public void CatalogRegistered_UR5eEntryExists()
        {
            Assert.IsTrue(RobotCatalog.TryGet("UR5e", out var entry),
                "UR5e should be registered in RobotCatalog");

            Assert.IsNotNull(entry);
            Assert.AreEqual("Universal Robots UR5e", entry.Metadata.DisplayName);
            Assert.AreEqual(6, entry.Metadata.Dof);
            Assert.IsNotNull(entry.TemplateFactory);
        }

        [Test]
        public void CatalogRegistered_UR5eTemplateCanBeCreated()
        {
            var template = RobotCatalog.CreateTemplate("UR5e");

            Assert.IsNotNull(template, "CreateTemplate should return non-null");
            Assert.AreEqual(6, template.Dof);
        }

        [Test]
        public void Create_TemplateNameIsCorrect()
        {
            var template = TemplateUR5e.Create();

            Assert.AreEqual(TemplateUR5e.Name, template.Name);
            Assert.AreEqual("UR5e", TemplateUR5e.Name);
        }
    }
}
