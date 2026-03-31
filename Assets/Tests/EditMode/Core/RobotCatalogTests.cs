// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotCatalog 동작을 검증하는 EditMode 테스트입니다.
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
        public void TryGet_SCARA_Valid()
        {
            var found = RobotCatalog.TryGet("SCARA_RV", out var entry);
            Assert.IsTrue(found);
            Assert.IsNotNull(entry);
            Assert.AreEqual("SCARA_RV", entry.Metadata.RobotId);
            Assert.AreEqual("SCARA Robot", entry.Metadata.DisplayName);
            Assert.AreEqual(4, entry.Metadata.Dof);
            Assert.AreEqual("SCARA", entry.Metadata.RobotType);
        }

        [Test]
        public void GetAvailableRobotIds_IncludesSCARA()
        {
            var ids = RobotCatalog.GetAvailableRobotIds();
            Assert.GreaterOrEqual(ids.Length, 2, "At least 2DOF_RR and SCARA_RV should have factories.");
            Assert.Contains("SCARA_RV", ids);
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

        [Test]
        public void GetAll_DoesNotContainGeneric6Dof_AndContainsSingleFr5()
        {
            var entries = RobotCatalog.GetAll();
            var fr5Count = 0;
            var templateFr5Count = 0;
            foreach (var entry in entries)
            {
                Assert.AreNotEqual("GENERIC_6DOF", entry.Metadata.RobotId);
                if (entry.Metadata.RobotId == "FAIRINO_FR5")
                {
                    fr5Count++;
                }

                if (entry.Metadata.RobotId == "FAIRINO_FR5_TEMPLATE")
                {
                    templateFr5Count++;
                }
            }

            Assert.AreEqual(1, fr5Count, "FAIRINO_FR5 should appear exactly once in RobotCatalog.");
            Assert.AreEqual(1, templateFr5Count, "FAIRINO_FR5_TEMPLATE should appear exactly once in RobotCatalog.");
            Assert.IsFalse(RobotCatalog.TryGet("GENERIC_6DOF", out _), "GENERIC_6DOF should no longer be registered.");
        }

        [Test]
        public void GetRobotLibraryEntries_PlacesTemplateFr5_BetweenScaraAndFr5()
        {
            var entries = RobotCatalog.GetRobotLibraryEntries();
            var scaraIndex = Array.FindIndex(entries, entry => entry.Metadata.RobotId == "SCARA_RV");
            var templateIndex = Array.FindIndex(entries, entry => entry.Metadata.RobotId == "FAIRINO_FR5_TEMPLATE");
            var fr5Index = Array.FindIndex(entries, entry => entry.Metadata.RobotId == "FAIRINO_FR5");

            Assert.That(scaraIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(templateIndex, Is.GreaterThan(scaraIndex));
            Assert.That(fr5Index, Is.GreaterThan(templateIndex));
        }
    }
}
