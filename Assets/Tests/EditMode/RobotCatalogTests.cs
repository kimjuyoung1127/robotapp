using System;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotCatalog의 등록/조회/생성 기능을 검증합니다.
    /// </summary>
    public class RobotCatalogTests
    {
        [Test]
        public void GetAll_Returns5Entries()
        {
            var entries = RobotCatalog.GetAll();
            Assert.GreaterOrEqual(entries.Length, 5);
        }

        [Test]
        public void GetAllRobotIds_Returns5Ids()
        {
            var ids = RobotCatalog.GetAllRobotIds();
            Assert.GreaterOrEqual(ids.Length, 5);
        }

        [Test]
        public void TryGet_2DOF_RR_Valid()
        {
            var found = RobotCatalog.TryGet("2DOF_RR", out var entry);
            Assert.IsTrue(found);
            Assert.IsNotNull(entry);
            Assert.AreEqual("2DOF_RR", entry.Metadata.RobotId);
            Assert.AreEqual("2DOF RR", entry.Metadata.DisplayName);
            Assert.AreEqual(2, entry.Metadata.Dof);
        }

        [Test]
        public void TryGet_Unknown_False()
        {
            var found = RobotCatalog.TryGet("NONEXISTENT_ROBOT", out var entry);
            Assert.IsFalse(found);
            Assert.IsNull(entry);
        }

        [Test]
        public void TryGet_NullId_False()
        {
            var found = RobotCatalog.TryGet(null, out var entry);
            Assert.IsFalse(found);
            Assert.IsNull(entry);
        }

        [Test]
        public void CreateTemplate_2DOF_Matches()
        {
            var template = RobotCatalog.CreateTemplate("2DOF_RR");
            Assert.IsNotNull(template);
            Assert.AreEqual("2DOF_RR", template.Name);
            Assert.AreEqual(2, template.Dof);
        }

        [Test]
        public void CreateTemplate_SCARA_ReturnsTemplate()
        {
            var template = RobotCatalog.CreateTemplate("SCARA_RV");
            Assert.IsNotNull(template);
            Assert.AreEqual(4, template.Dof);
        }

        [Test]
        public void HasTemplate_2DOF_True()
        {
            Assert.IsTrue(RobotCatalog.HasTemplate("2DOF_RR"));
        }

        [Test]
        public void HasTemplate_SCARA_True()
        {
            Assert.IsTrue(RobotCatalog.HasTemplate("SCARA_RV"));
        }

        [Test]
        public void HasTemplate_Unknown_False()
        {
            Assert.IsFalse(RobotCatalog.HasTemplate("NONEXISTENT"));
        }

        [Test]
        public void GetAll_AllHaveValidMetadata()
        {
            var entries = RobotCatalog.GetAll();
            foreach (var entry in entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Metadata.RobotId), "RobotId should not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Metadata.DisplayName), "DisplayName should not be empty.");
                Assert.Greater(entry.Metadata.Dof, 0, $"DOF for {entry.Metadata.RobotId} should be positive.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Metadata.RobotType), $"RobotType for {entry.Metadata.RobotId} should not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Metadata.Difficulty), $"Difficulty for {entry.Metadata.RobotId} should not be empty.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Metadata.VisualizationLevel), $"VisualizationLevel for {entry.Metadata.RobotId} should not be empty.");
            }
        }
    }
}
