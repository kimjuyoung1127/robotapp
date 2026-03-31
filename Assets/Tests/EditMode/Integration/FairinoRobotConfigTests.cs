// Folder: Tests - EditMode unit tests; no scene required.
using System.IO;
using NUnit.Framework;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class FairinoRobotConfigTests
    {
        [Test]
        public void JsonFile_ExistsInResources()
        {
            Assert.That(
                File.Exists("Assets/Runtime/Resources/LearningTabs/FAIRINO_FR5.json"),
                Is.True,
                "FAIRINO_FR5.json should exist in Resources/LearningTabs/");
        }

        [Test]
        public void JsonFile_ContainsExpectedFields()
        {
            var json = File.ReadAllText("Assets/Runtime/Resources/LearningTabs/FAIRINO_FR5.json");

            Assert.That(json, Does.Contain("FAIRINO_FR5"), "robotId field");
            Assert.That(json, Does.Contain("defaultIp"), "defaultIp field");
            Assert.That(json, Does.Contain("dhParameters"), "dhParameters field");
            Assert.That(json, Does.Contain("jointLimits"), "jointLimits field");
            Assert.That(json, Does.Contain("speedPresets"), "speedPresets field");
            Assert.That(json, Does.Contain("errorMessages"), "errorMessages field");
            Assert.That(json, Does.Contain("uiTabs"), "uiTabs field");
        }

        [Test]
        public void JsonFile_HasSixDHParameters()
        {
            var json = File.ReadAllText("Assets/Runtime/Resources/LearningTabs/FAIRINO_FR5.json");

            int count = 0;
            int idx = 0;
            while ((idx = json.IndexOf("\"theta\"", idx)) >= 0)
            {
                count++;
                idx++;
            }

            Assert.AreEqual(6, count, "Should have 6 DH parameter entries");
        }

        [Test]
        public void JsonFile_HasSixJointLimits()
        {
            var json = File.ReadAllText("Assets/Runtime/Resources/LearningTabs/FAIRINO_FR5.json");

            int count = 0;
            int idx = 0;
            while ((idx = json.IndexOf("\"minDeg\"", idx)) >= 0)
            {
                count++;
                idx++;
            }

            Assert.AreEqual(6, count, "Should have 6 joint limit entries");
        }

        [Test]
        public void FairinoResult_Ok_IsSuccess()
        {
            var result = FairinoResult.Ok();
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.ErrorCode);
        }

        [Test]
        public void FairinoResult_Fail_IsNotSuccess()
        {
            var result = FairinoResult.Fail(-1, "error");
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(-1, result.ErrorCode);
            Assert.AreEqual("error", result.Message);
        }

        [Test]
        public void FairinoResultGeneric_Ok_HasValue()
        {
            var result = FairinoResult<int>.Ok(42);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void FairinoRobotState_Zero_Has6Elements()
        {
            var state = FairinoRobotState.Zero();
            Assert.AreEqual(6, state.JointPosDeg.Length);
            Assert.AreEqual(6, state.TcpPose.Length);
        }

        [Test]
        public void FairinoRobotState_ClonesArrays()
        {
            var joints = new double[] { 1, 2, 3, 4, 5, 6 };
            var tcp = new double[] { 10, 20, 30, 40, 50, 60 };
            var state = new FairinoRobotState(joints, tcp);

            joints[0] = 999;
            Assert.AreEqual(1, state.JointPosDeg[0], 1e-10,
                "State should clone input arrays");
        }

        [Test]
        public void FairinoVersionInfo_StoresValues()
        {
            var info = new FairinoVersionInfo("1.0", "2.0");
            Assert.AreEqual("1.0", info.FirmwareVersion);
            Assert.AreEqual("2.0", info.SdkVersion);
        }

        [Test]
        public void FairinoVersionInfo_NullSafe()
        {
            var info = new FairinoVersionInfo(null, null);
            Assert.AreEqual(string.Empty, info.FirmwareVersion);
            Assert.AreEqual(string.Empty, info.SdkVersion);
        }
    }
}
