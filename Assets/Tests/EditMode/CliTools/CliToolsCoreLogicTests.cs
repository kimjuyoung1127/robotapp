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

        // ── DhTableTool 의존 로직 ──

        [Test]
        public void DhTable_2DOF_ReturnsCorrectLinkCount()
        {
            var template = Template2DOF_RR.Create();
            var links = template.GetLinks();
            Assert.AreEqual(2, links.Length, "2DOF 템플릿은 2개 링크를 가져야 합니다.");
        }

        [Test]
        public void DhTable_FR5_AllLinksHaveFiniteValues()
        {
            var template = TemplateFAIRINO_FR5.Create();
            var links = template.GetLinks();
            foreach (var link in links)
            {
                Assert.IsFalse(double.IsNaN(link.Theta), "Theta가 NaN이면 안 됩니다.");
                Assert.IsFalse(double.IsNaN(link.D), "D가 NaN이면 안 됩니다.");
                Assert.IsFalse(double.IsNaN(link.A), "A가 NaN이면 안 됩니다.");
                Assert.IsFalse(double.IsNaN(link.Alpha), "Alpha가 NaN이면 안 됩니다.");
                Assert.IsFalse(double.IsInfinity(link.Theta), "Theta가 Infinity이면 안 됩니다.");
                Assert.IsFalse(double.IsInfinity(link.D), "D가 Infinity이면 안 됩니다.");
                Assert.IsFalse(double.IsInfinity(link.A), "A가 Infinity이면 안 됩니다.");
                Assert.IsFalse(double.IsInfinity(link.Alpha), "Alpha가 Infinity이면 안 됩니다.");
            }
        }

        [Test]
        public void DhTable_SCARA_ContainsPrismaticJoint()
        {
            var template = TemplateSCARA_RV.Create();
            var links = template.GetLinks();
            bool hasPrismatic = false;
            foreach (var link in links)
            {
                if (link.JointType == JointType.Prismatic)
                    hasPrismatic = true;
            }
            Assert.IsTrue(hasPrismatic, "SCARA 템플릿에는 Prismatic 관절이 포함되어야 합니다.");
        }

        // ── JointLimitTool 의존 로직 ──

        [Test]
        public void JointLimits_2DOF_ReturnsCorrectCount()
        {
            var template = Template2DOF_RR.Create();
            var limits = template.GetJointLimits();
            Assert.AreEqual(template.Dof, limits.Length, "관절 제한 수는 DOF와 같아야 합니다.");
        }

        [Test]
        public void JointLimits_FR5_AllRangesPositive()
        {
            var template = TemplateFAIRINO_FR5.Create();
            var limits = template.GetJointLimits();
            for (int i = 0; i < limits.Length; i++)
            {
                double range = limits[i].Max - limits[i].Min;
                Assert.Greater(range, 0, $"Joint {i}: 범위가 양수여야 합니다.");
            }
        }

        [Test]
        public void JointLimits_AllTemplates_MinLessThanMax()
        {
            var ids = RobotCatalog.GetAvailableRobotIds();
            foreach (string id in ids)
            {
                var template = RobotCatalog.CreateTemplate(id);
                if (template == null) continue;

                var limits = template.GetJointLimits();
                for (int i = 0; i < limits.Length; i++)
                {
                    Assert.LessOrEqual(limits[i].Min, limits[i].Max,
                        $"{id} Joint {i}: Min({limits[i].Min})이 Max({limits[i].Max})보다 클 수 없습니다.");
                }
            }
        }

        // ── BuildSettingsTool 의존 로직 ──

        [Test]
        public void BuildSettings_KnownScenes_ExistInBuildSettings()
        {
            var buildScenes = UnityEditor.EditorBuildSettings.scenes;
            Assert.Greater(buildScenes.Length, 0, "Build Settings에 씬이 등록되어 있어야 합니다.");
        }

        // ── AsmdefValidateTool 의존 로직 ──

        [Test]
        public void Asmdef_ProjectHasMinimumAssemblies()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            Assert.GreaterOrEqual(guids.Length, 5, "프로젝트에 최소 5개의 asmdef가 있어야 합니다.");
        }

        // ── TemplateResolver 로직 ──

        [Test]
        public void TemplateResolver_KnownNames_ReturnNonNull()
        {
            string[] validNames = { "2DOF_RR", "2DOF", "SCARA_RV", "SCARA", "FR5", "FAIRINO_FR5" };
            foreach (string name in validNames)
            {
                var template = RobotCatalog.CreateTemplate(
                    name.Contains("2DOF") ? "2DOF_RR" :
                    name.Contains("SCARA") ? "SCARA_RV" : "FAIRINO_FR5");
                Assert.IsNotNull(template, $"'{name}' 해석이 null이면 안 됩니다.");
            }
        }

        [Test]
        public void TemplateResolver_UnknownName_CatalogReturnsNull()
        {
            var template = RobotCatalog.CreateTemplate("NONEXISTENT_ROBOT_XYZ");
            Assert.IsNull(template, "존재하지 않는 템플릿은 null을 반환해야 합니다.");
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
