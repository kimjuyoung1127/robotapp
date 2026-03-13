// Folder: Tests - EditMode unit tests; no scene required.
using System;
using NUnit.Framework;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class TemplateFAIRINO_FR5Tests
    {
        private const double PositionDelta = 1e-4;

        [Test]
        public void Create_Returns6DofTemplate()
        {
            var template = TemplateFAIRINO_FR5.Create();

            Assert.IsNotNull(template);
            Assert.AreEqual(6, template.Dof);
            Assert.AreEqual("FAIRINO_FR5", template.Name);
        }

        [Test]
        public void Create_LinksHaveExpectedDHParameters()
        {
            var template = TemplateFAIRINO_FR5.Create();

            var link0 = template.GetLink(0);
            Assert.AreEqual(0.4045, link0.D, PositionDelta, "Link 0 d");
            Assert.AreEqual(0.0, link0.A, PositionDelta, "Link 0 a");

            var link1 = template.GetLink(1);
            Assert.AreEqual(0.3225, link1.A, PositionDelta, "Link 1 a");

            var link3 = template.GetLink(3);
            Assert.AreEqual(0.3685, link3.D, PositionDelta, "Link 3 d");

            var link5 = template.GetLink(5);
            Assert.AreEqual(0.1578, link5.D, PositionDelta, "Link 5 d");
        }

        [Test]
        public void Create_JointLimitsAreValid()
        {
            var template = TemplateFAIRINO_FR5.Create();

            for (int i = 0; i < 6; i++)
            {
                var limit = template.GetJointLimit(i);
                Assert.That(limit.Min < limit.Max, $"Joint {i} min < max");
            }
        }

        [Test]
        public void Create_AllJointsAreRevolute()
        {
            var template = TemplateFAIRINO_FR5.Create();

            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(JointType.Revolute, template.GetLink(i).JointType,
                    $"Joint {i} should be Revolute");
            }
        }

        [Test]
        public void CreateFromConfig_ValidInput_ReturnsTemplate()
        {
            var dh = new[]
            {
                new[] { 0.0, 0.4045, 0.0, -System.Math.PI / 2 },
                new[] { 0.0, 0.0, 0.3225, 0.0 },
                new[] { 0.0, 0.0, 0.0, -System.Math.PI / 2 },
                new[] { 0.0, 0.3685, 0.0, System.Math.PI / 2 },
                new[] { 0.0, 0.0, 0.0, -System.Math.PI / 2 },
                new[] { 0.0, 0.1578, 0.0, 0.0 },
            };

            var limits = new[]
            {
                new[] { -175.0, 175.0 },
                new[] { -265.0, 85.0 },
                new[] { -162.0, 162.0 },
                new[] { -265.0, 85.0 },
                new[] { -175.0, 175.0 },
                new[] { -360.0, 360.0 },
            };

            var template = TemplateFAIRINO_FR5.CreateFromConfig(dh, limits);

            Assert.IsNotNull(template);
            Assert.AreEqual(6, template.Dof);
        }

        [Test]
        public void CreateFromConfig_NullDH_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TemplateFAIRINO_FR5.CreateFromConfig(null, new double[6][]));
        }

        [Test]
        public void CreateFromConfig_MismatchCount_ThrowsArgumentException()
        {
            var dh = new[] { new[] { 0.0, 0.0, 0.0, 0.0 } };
            var limits = new[] { new[] { -180.0, 180.0 }, new[] { -180.0, 180.0 } };

            Assert.Throws<ArgumentException>(() =>
                TemplateFAIRINO_FR5.CreateFromConfig(dh, limits));
        }

        [Test]
        public void CatalogRegistered_FR5EntryExists()
        {
            Assert.IsTrue(RobotCatalog.TryGet("FAIRINO_FR5", out var entry),
                "FAIRINO_FR5 should be registered in RobotCatalog");

            Assert.IsNotNull(entry);
            Assert.AreEqual("FAIRINO FR5", entry.Metadata.DisplayName);
            Assert.AreEqual(6, entry.Metadata.Dof);
            Assert.IsNotNull(entry.TemplateFactory);
        }

        [Test]
        public void CatalogRegistered_FR5TemplateCanBeCreated()
        {
            var template = RobotCatalog.CreateTemplate("FAIRINO_FR5");

            Assert.IsNotNull(template, "CreateTemplate should return non-null");
            Assert.AreEqual(6, template.Dof);
        }
    }
}
