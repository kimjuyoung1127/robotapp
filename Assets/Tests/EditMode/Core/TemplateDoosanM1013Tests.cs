// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class TemplateDoosanM1013Tests
    {
        private const double PositionDelta = 1e-4;
        private const double AngleDelta = 1e-6;

        [Test]
        public void Create_Returns6DofTemplate()
        {
            var template = TemplateDoosanM1013.Create();

            Assert.IsNotNull(template);
            Assert.AreEqual(6, template.Dof);
            Assert.AreEqual("DOOSAN_M1013", template.Name);
        }

        [Test]
        public void Create_AllJointsAreRevolute()
        {
            var template = TemplateDoosanM1013.Create();

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(JointType.Revolute, template.GetLink(i).JointType,
                    $"Joint {i} should be Revolute");
            }
        }

        [Test]
        public void Create_DHParametersMatch_Link0()
        {
            var template = TemplateDoosanM1013.Create();
            var link0 = template.GetLink(0);

            Assert.AreEqual(0.1525, link0.D, PositionDelta, "Link 0 d should be 0.1525");
            Assert.AreEqual(0.0, link0.A, PositionDelta, "Link 0 a should be 0");
            Assert.AreEqual(-System.Math.PI / 2, link0.Alpha, AngleDelta, "Link 0 alpha should be -pi/2");
        }

        [Test]
        public void Create_DHParametersMatch_Link1()
        {
            var template = TemplateDoosanM1013.Create();
            var link1 = template.GetLink(1);

            Assert.AreEqual(0.0345, link1.D, PositionDelta, "Link 1 d should be 0.0345");
            Assert.AreEqual(0.62, link1.A, PositionDelta, "Link 1 a should be 0.62");
            Assert.AreEqual(0.0, link1.Alpha, AngleDelta, "Link 1 alpha should be 0");
        }

        [Test]
        public void Create_DHParametersMatch_Link2()
        {
            var template = TemplateDoosanM1013.Create();
            var link2 = template.GetLink(2);

            Assert.AreEqual(0.0, link2.D, PositionDelta, "Link 2 d should be 0");
            Assert.AreEqual(0.0, link2.A, PositionDelta, "Link 2 a should be 0");
            Assert.AreEqual(-System.Math.PI / 2, link2.Alpha, AngleDelta, "Link 2 alpha should be -pi/2");
        }

        [Test]
        public void Create_DHParametersMatch_Link3to5()
        {
            var template = TemplateDoosanM1013.Create();

            var link3 = template.GetLink(3);
            Assert.AreEqual(0.559, link3.D, PositionDelta, "Link 3 d should be 0.559");
            Assert.AreEqual(System.Math.PI / 2, link3.Alpha, AngleDelta, "Link 3 alpha should be pi/2");

            var link4 = template.GetLink(4);
            Assert.AreEqual(0.0, link4.D, PositionDelta, "Link 4 d should be 0");
            Assert.AreEqual(-System.Math.PI / 2, link4.Alpha, AngleDelta, "Link 4 alpha should be -pi/2");

            var link5 = template.GetLink(5);
            Assert.AreEqual(0.121, link5.D, PositionDelta, "Link 5 d should be 0.121");
            Assert.AreEqual(0.0, link5.Alpha, AngleDelta, "Link 5 alpha should be 0");
        }

        [Test]
        public void Create_JointLimitsAreValid()
        {
            var template = TemplateDoosanM1013.Create();

            for (int i = 0; i < 6; i++)
            {
                var limit = template.GetJointLimit(i);
                Assert.That(limit.Min < limit.Max, $"Joint {i} min should be less than max");
            }
        }

        [Test]
        public void Create_AllJointsHave150DegLimits()
        {
            var template = TemplateDoosanM1013.Create();
            var pi = System.Math.PI;
            var expected = 150.0 * pi / 180.0;

            for (int i = 0; i < 6; i++)
            {
                var limit = template.GetJointLimit(i);
                Assert.AreEqual(-expected, limit.Min, AngleDelta, $"Joint {i} min should be -150 deg");
                Assert.AreEqual(expected, limit.Max, AngleDelta, $"Joint {i} max should be +150 deg");
            }
        }

        [Test]
        public void CatalogRegistered_DoosanM1013EntryExists()
        {
            Assert.IsTrue(RobotCatalog.TryGet("DOOSAN_M1013", out var entry),
                "DOOSAN_M1013 should be registered in RobotCatalog");

            Assert.IsNotNull(entry);
            Assert.AreEqual("Doosan M1013", entry.Metadata.DisplayName);
            Assert.AreEqual(6, entry.Metadata.Dof);
            Assert.IsNotNull(entry.TemplateFactory);
        }

        [Test]
        public void CatalogRegistered_DoosanM1013TemplateCanBeCreated()
        {
            var template = RobotCatalog.CreateTemplate("DOOSAN_M1013");

            Assert.IsNotNull(template, "CreateTemplate should return non-null");
            Assert.AreEqual(6, template.Dof);
        }

        [Test]
        public void Create_TemplateNameIsCorrect()
        {
            var template = TemplateDoosanM1013.Create();

            Assert.AreEqual(TemplateDoosanM1013.Name, template.Name);
            Assert.AreEqual("DOOSAN_M1013", TemplateDoosanM1013.Name);
        }

        [Test]
        public void CatalogRegistered_DoosanM1013InLibraryOrder()
        {
            var ids = RobotCatalog.GetRobotLibraryIds();
            bool found = false;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == "DOOSAN_M1013")
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "DOOSAN_M1013 should appear in RobotLibraryIds");
        }
    }
}
