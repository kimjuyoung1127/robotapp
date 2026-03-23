// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class TemplateMeca500Tests
    {
        private const double PositionDelta = 1e-4;
        private const double AngleDelta = 1e-6;

        [Test]
        public void Create_Returns6DofTemplate()
        {
            var template = TemplateMeca500.Create();

            Assert.IsNotNull(template);
            Assert.AreEqual(6, template.Dof);
            Assert.AreEqual("MECA500", template.Name);
        }

        [Test]
        public void Create_AllJointsAreRevolute()
        {
            var template = TemplateMeca500.Create();

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(JointType.Revolute, template.GetLink(i).JointType,
                    $"Joint {i} should be Revolute");
            }
        }

        [Test]
        public void Create_DHParametersMatch_Link0()
        {
            var template = TemplateMeca500.Create();
            var link0 = template.GetLink(0);

            Assert.AreEqual(0.091, link0.D, PositionDelta, "Link 0 d should be 0.091");
            Assert.AreEqual(0.0,   link0.A, PositionDelta, "Link 0 a should be 0");
            Assert.AreEqual(-System.Math.PI / 2, link0.Alpha, AngleDelta, "Link 0 alpha should be -pi/2");
        }

        [Test]
        public void Create_DHParametersMatch_Link1()
        {
            var template = TemplateMeca500.Create();
            var link1 = template.GetLink(1);

            Assert.AreEqual(0.044, link1.D, PositionDelta, "Link 1 d should be 0.044");
            Assert.AreEqual(0.135, link1.A, PositionDelta, "Link 1 a should be 0.135");
            Assert.AreEqual(0.0,   link1.Alpha, AngleDelta, "Link 1 alpha should be 0");
        }

        [Test]
        public void Create_DHParametersMatch_Link2()
        {
            var template = TemplateMeca500.Create();
            var link2 = template.GetLink(2);

            Assert.AreEqual(0.0, link2.D, PositionDelta, "Link 2 d should be 0");
            Assert.AreEqual(0.0, link2.A, PositionDelta, "Link 2 a should be 0");
            Assert.AreEqual(-System.Math.PI / 2, link2.Alpha, AngleDelta, "Link 2 alpha should be -pi/2");
        }

        [Test]
        public void Create_DHParametersMatch_Link3to5()
        {
            var template = TemplateMeca500.Create();

            var link3 = template.GetLink(3);
            Assert.AreEqual(0.038, link3.D, PositionDelta, "Link 3 d should be 0.038");
            Assert.AreEqual(System.Math.PI / 2, link3.Alpha, AngleDelta, "Link 3 alpha should be pi/2");

            var link4 = template.GetLink(4);
            Assert.AreEqual(0.0,   link4.D, PositionDelta, "Link 4 d should be 0");
            Assert.AreEqual(0.120, link4.A, PositionDelta, "Link 4 a should be 0.120");
            Assert.AreEqual(-System.Math.PI / 2, link4.Alpha, AngleDelta, "Link 4 alpha should be -pi/2");

            var link5 = template.GetLink(5);
            Assert.AreEqual(0.070, link5.D, PositionDelta, "Link 5 d should be 0.070");
            Assert.AreEqual(0.0,   link5.A, PositionDelta, "Link 5 a should be 0");
            Assert.AreEqual(0.0,   link5.Alpha, AngleDelta, "Link 5 alpha should be 0");
        }

        [Test]
        public void Create_JointLimitsAreValid()
        {
            var template = TemplateMeca500.Create();

            for (int i = 0; i < 6; i++)
            {
                var limit = template.GetJointLimit(i);
                Assert.That(limit.Min < limit.Max, $"Joint {i} min should be less than max");
            }
        }

        [Test]
        public void Create_Joint0HasSymmetricLimits()
        {
            var template = TemplateMeca500.Create();
            var limit0 = template.GetJointLimit(0);
            var deg2Rad = System.Math.PI / 180.0;

            Assert.AreEqual(-175.0 * deg2Rad, limit0.Min, AngleDelta, "J1 min should be -175 deg");
            Assert.AreEqual( 175.0 * deg2Rad, limit0.Max, AngleDelta, "J1 max should be +175 deg");
        }

        [Test]
        public void Create_Joint1HasAsymmetricLimits()
        {
            var template = TemplateMeca500.Create();
            var limit1 = template.GetJointLimit(1);
            var deg2Rad = System.Math.PI / 180.0;

            Assert.AreEqual(-70.0 * deg2Rad, limit1.Min, AngleDelta, "J2 min should be -70 deg");
            Assert.AreEqual( 90.0 * deg2Rad, limit1.Max, AngleDelta, "J2 max should be +90 deg");
        }

        [Test]
        public void Create_Joint2HasAsymmetricLimits()
        {
            var template = TemplateMeca500.Create();
            var limit2 = template.GetJointLimit(2);
            var deg2Rad = System.Math.PI / 180.0;

            Assert.AreEqual(-135.0 * deg2Rad, limit2.Min, AngleDelta, "J3 min should be -135 deg");
            Assert.AreEqual(  70.0 * deg2Rad, limit2.Max, AngleDelta, "J3 max should be +70 deg");
        }

        [Test]
        public void CatalogRegistered_Meca500EntryExists()
        {
            Assert.IsTrue(RobotCatalog.TryGet("MECA500", out var entry),
                "MECA500 should be registered in RobotCatalog");

            Assert.IsNotNull(entry);
            Assert.AreEqual("Mecademic Meca500", entry.Metadata.DisplayName);
            Assert.AreEqual(6, entry.Metadata.Dof);
            Assert.IsNotNull(entry.TemplateFactory);
        }

        [Test]
        public void CatalogRegistered_Meca500TemplateCanBeCreated()
        {
            var template = RobotCatalog.CreateTemplate("MECA500");

            Assert.IsNotNull(template, "CreateTemplate should return non-null");
            Assert.AreEqual(6, template.Dof);
        }

        [Test]
        public void Create_TemplateNameIsCorrect()
        {
            var template = TemplateMeca500.Create();

            Assert.AreEqual(TemplateMeca500.Name, template.Name);
            Assert.AreEqual("MECA500", TemplateMeca500.Name);
        }
    }
}
