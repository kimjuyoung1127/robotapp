// Folder: Tests/EditMode/CliTools - CLI 도구가 의존하는 코어 로직 검증
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode.CliTools
{
    /// <summary>
    /// unity-cli 커스텀 도구들이 의존하는 코어 로직을 검증합니다.
    /// HandleCommand(JObject)는 Unity Editor + connector 런타임에서만 테스트 가능하므로,
    /// 여기서는 도구가 호출하는 핵심 API의 정합성을 확인합니다.
    /// </summary>
    [TestFixture]
    public class CliToolsCoreLogicTests
    {
        private const double Delta = 1e-4;

        // ── RobotCatalogTool 의존 로직 ──

        [Test]
        public void RobotCatalog_GetAll_ReturnsExpectedEntries()
        {
            var entries = RobotCatalog.GetAll();
            Assert.GreaterOrEqual(entries.Length, 3, "최소 3개 로봇(2DOF, SCARA, FR5)이 등록되어야 합니다.");
        }

        [Test]
        public void RobotCatalog_AllEntries_HaveValidMetadata()
        {
            var entries = RobotCatalog.GetAll();
            foreach (var entry in entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Metadata.RobotId), "RobotId가 비어있으면 안 됩니다.");
                Assert.IsFalse(string.IsNullOrEmpty(entry.Metadata.DisplayName), "DisplayName이 비어있으면 안 됩니다.");
                Assert.Greater(entry.Metadata.Dof, 0, $"{entry.Metadata.RobotId}: DOF는 1 이상이어야 합니다.");
            }
        }

        // ── FkComputeTool 의존 로직 ──

        [Test]
        public void FK_2DOF_ZeroAngles_ReturnsExpectedPosition()
        {
            var template = Template2DOF_RR.Create();
            var links = template.GetLinks();
            var joints = new double[links.Length];
            var result = ForwardKinematics.ComputeEndEffectorTransform(links, joints);
            Vec3D pos = result.ExtractPosition();

            Assert.IsNotNull(pos);
            Assert.IsFalse(double.IsNaN(pos.X), "EE X가 NaN이면 안 됩니다.");
            Assert.IsFalse(double.IsNaN(pos.Y), "EE Y가 NaN이면 안 됩니다.");
        }

        [Test]
        public void FK_FR5_ZeroAngles_ReturnsValidPosition()
        {
            var template = TemplateFAIRINO_FR5.Create();
            var links = template.GetLinks();
            var joints = new double[links.Length];
            var result = ForwardKinematics.ComputeEndEffectorTransform(links, joints);
            Vec3D pos = result.ExtractPosition();

            Assert.IsFalse(double.IsNaN(pos.X));
            Assert.IsFalse(double.IsNaN(pos.Y));
            Assert.IsFalse(double.IsNaN(pos.Z));
        }

        [Test]
        public void FK_SCARA_ZeroAngles_ReturnsValidPosition()
        {
            var template = TemplateSCARA_RV.Create();
            var links = template.GetLinks();
            var joints = new double[links.Length];
            var result = ForwardKinematics.ComputeEndEffectorTransform(links, joints);
            Vec3D pos = result.ExtractPosition();

            Assert.IsFalse(double.IsNaN(pos.X));
            Assert.IsFalse(double.IsNaN(pos.Y));
        }

        // ── SceneValidateTool 의존 로직 ──

        [Test]
        public void SceneCatalog_AllKnownScenes_HaveSceneFiles()
        {
            string[] knownScenes = { "Boot", "Onboarding", "Home", "Main",
                "MathReadiness", "RobotLibrary", "Sandbox", "RobotControl" };

            foreach (string sceneName in knownScenes)
            {
                string path = $"Assets/Scenes/{sceneName}.unity";
                bool exists = System.IO.File.Exists(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
                Assert.IsTrue(exists, $"씬 파일이 존재해야 합니다: {path}");
            }
        }

        // ── Template resolution 로직 ──

        [Test]
        public void TemplateResolution_AllAvailable_CreateSuccessfully()
        {
            var ids = RobotCatalog.GetAvailableRobotIds();
            foreach (string id in ids)
            {
                var template = RobotCatalog.CreateTemplate(id);
                Assert.IsNotNull(template, $"템플릿 생성 실패: {id}");
                Assert.Greater(template.Dof, 0, $"{id}: DOF는 1 이상이어야 합니다.");

                var links = template.GetLinks();
                Assert.AreEqual(template.Dof, links.Length, $"{id}: links.Length는 DOF와 같아야 합니다.");
            }
        }

        // ── QaPrepTool 의존 로직 ──

        [Test]
        public void PlayerPrefs_SetAndDelete_DoNotThrow()
        {
            string testKey = "KineTutor3D.Test.CliToolsTest";
            Assert.DoesNotThrow(() =>
            {
                UnityEngine.PlayerPrefs.SetInt(testKey, 42);
                UnityEngine.PlayerPrefs.DeleteKey(testKey);
                UnityEngine.PlayerPrefs.Save();
            });
        }
    }
}
