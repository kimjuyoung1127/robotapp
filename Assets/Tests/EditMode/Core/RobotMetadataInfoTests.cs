// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotMetadataInfo 동작을 검증하는 EditMode 테스트입니다.
using System;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotMetadataInfo 구조체 생성 및 검증을 테스트합니다.
    /// </summary>
    public class RobotMetadataInfoTests
    {
        [Test]
        public void Constructor_ValidInputs_SetsAllFields()
        {
            var info = new RobotMetadataInfo("TEST_01", "Test Robot", 3, "RR", "Easy",
                guidedLessonSupported: true, sandboxSupported: true, instructorRecommended: true,
                description: "A test robot.",
                supportedLessons: new[] { "L0", "Sandbox" },
                inputModes: new[] { "slider", "numeric" },
                visualizationLevel: "DonorMesh",
                zeroPoseDeg: new[] { 0d, 0d, 0d },
                homePoseDeg: new[] { 5d, 10d, 15d },
                demoPoseDeg: new[] { 15d, 10d, 5d },
                importSource: "Assets/Test.prefab");

            Assert.AreEqual("TEST_01", info.RobotId);
            Assert.AreEqual("Test Robot", info.DisplayName);
            Assert.AreEqual(3, info.Dof);
            Assert.AreEqual("RR", info.RobotType);
            Assert.AreEqual("Easy", info.Difficulty);
            Assert.AreEqual("DH-Standard", info.Convention);
            Assert.IsTrue(info.GuidedLessonSupported);
            Assert.IsTrue(info.SandboxSupported);
            Assert.IsTrue(info.InstructorRecommended);
            Assert.AreEqual("A test robot.", info.Description);
            CollectionAssert.AreEqual(new[] { "L0", "Sandbox" }, info.SupportedLessons);
            CollectionAssert.AreEqual(new[] { "slider", "numeric" }, info.InputModes);
            Assert.AreEqual("DonorMesh", info.VisualizationLevel);
            CollectionAssert.AreEqual(new[] { 0d, 0d, 0d }, info.ZeroPoseDeg);
            CollectionAssert.AreEqual(new[] { 5d, 10d, 15d }, info.HomePoseDeg);
            CollectionAssert.AreEqual(new[] { 15d, 10d, 5d }, info.DemoPoseDeg);
            Assert.AreEqual("Assets/Test.prefab", info.ImportSource);
        }

        [Test]
        public void Constructor_NullRobotId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new RobotMetadataInfo(null, "Name", 2, "RR", "Easy"));
        }

        [Test]
        public void Constructor_EmptyRobotId_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new RobotMetadataInfo("", "Name", 2, "RR", "Easy"));
        }

        [Test]
        public void Constructor_NullDisplayName_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new RobotMetadataInfo("ID", null, 2, "RR", "Easy"));
        }

        [Test]
        public void Constructor_DefaultOptionalFields()
        {
            var info = new RobotMetadataInfo("ID", "Name", 2, "RR", "Easy");

            Assert.AreEqual("DH-Standard", info.Convention);
            Assert.IsFalse(info.GuidedLessonSupported);
            Assert.IsFalse(info.SandboxSupported);
            Assert.IsFalse(info.InstructorRecommended);
            Assert.AreEqual(string.Empty, info.Description);
            Assert.That(info.SupportedLessons, Is.Empty);
            Assert.That(info.InputModes, Is.Empty);
            Assert.AreEqual("Lesson", info.VisualizationLevel);
            Assert.AreEqual(2, info.ZeroPoseDeg.Length);
            Assert.AreEqual(2, info.HomePoseDeg.Length);
            Assert.AreEqual(2, info.DemoPoseDeg.Length);
            Assert.AreEqual(string.Empty, info.ImportSource);
        }
    }
}
