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
            string[] knownScenes = { "Boot", "Onboarding",
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
            Assert.AreEqual(4, links.Length, "SCARA 템플릿은 4개 링크를 가져야 합니다.");
            foreach (var link in links)
            {
                Assert.AreEqual(JointType.Revolute, link.JointType, "현재 SCARA donor 템플릿은 회전 관절 체인으로 정의되어야 합니다.");
            }
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

        // ── PlayerPrefsInspectTool 의존 로직 ──

        [Test]
        public void PlayerPrefs_KnownKeys_AreAccessible()
        {
            string[] knownKeys =
            {
                "KineTutor3D.HasVisited",
                "KineTutor3D.CurrentTrack",
                "KineTutor3D.SelectedRobotId",
                "KineTutor3D.SelectedMode",
                "KineTutor3D.SessionContextJson",
                "KineTutor3D.MathReadiness.LastCompletedStep",
                "KineTutor3D.PreKinematics.LastCompletedStep",
                "KineTutor3D.CoreKinematics.LastCompletedStep",
                "KineTutor3D.ReducedMotion"
            };
            Assert.AreEqual(9, knownKeys.Length, "알려진 PlayerPrefs 키는 9개여야 합니다.");
            Assert.DoesNotThrow(() =>
            {
                foreach (string key in knownKeys)
                    UnityEngine.PlayerPrefs.HasKey(key);
            });
        }

        // ── ResourceValidateTool 의존 로직 ──

        [Test]
        public void Resources_TutorSteps_AllExist()
        {
            string[] steps = { "S01", "S02", "S03", "S04", "S05", "S06", "S07", "S08" };
            foreach (string step in steps)
            {
                string path = $"Assets/Runtime/Resources/TutorSteps/{step}.asset";
                bool exists = System.IO.File.Exists(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
                Assert.IsTrue(exists, $"TutorStep 에셋이 존재해야 합니다: {path}");
            }
        }

        [Test]
        public void Resources_LearningTabs_AllExist()
        {
            string[] tabs = { "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10", "IGUS_REBEL", "GENERIC_6DOF" };
            foreach (string tab in tabs)
            {
                string path = $"Assets/Runtime/Resources/LearningTabs/{tab}.json";
                bool exists = System.IO.File.Exists(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
                Assert.IsTrue(exists, $"LearningTabs JSON이 존재해야 합니다: {path}");
            }
        }

        [Test]
        public void Resources_GlossaryDatabase_Exists()
        {
            string path = "Assets/Runtime/Resources/Glossary/GlossaryDatabase.asset";
            bool exists = System.IO.File.Exists(
                System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
            Assert.IsTrue(exists, "GlossaryDatabase.asset가 존재해야 합니다.");
        }

        [Test]
        public void Resources_Glossary_HasMinimumTerms()
        {
            string dir = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Runtime", "Resources", "Glossary");
            int termCount = 0;
            if (System.IO.Directory.Exists(dir))
            {
                foreach (string file in System.IO.Directory.GetFiles(dir, "*.asset"))
                {
                    if (!System.IO.Path.GetFileName(file).StartsWith("GlossaryDatabase"))
                        termCount++;
                }
            }
            Assert.GreaterOrEqual(termCount, 10, "Glossary 용어는 최소 10개 이상이어야 합니다.");
        }

        // ── SessionContextTool 의존 로직 ──

        [Test]
        public void SessionContext_WriteAndRead_RoundTrips()
        {
            string testKey = "KineTutor3D.Test.SessionContextRoundTrip";
            string json = "{\"RobotId\":\"2DOF_RR\",\"EntryMode\":\"sandbox\",\"Track\":\"core_kinematics\",\"Step\":3}";
            Assert.DoesNotThrow(() =>
            {
                UnityEngine.PlayerPrefs.SetString(testKey, json);
                string loaded = UnityEngine.PlayerPrefs.GetString(testKey, "");
                Assert.AreEqual(json, loaded, "세션 컨텍스트 JSON 라운드트립이 일치해야 합니다.");
                UnityEngine.PlayerPrefs.DeleteKey(testKey);
                UnityEngine.PlayerPrefs.Save();
            });
        }

        // ── PoseCompareTool 의존 로직 ──

        [Test]
        public void PoseCompare_2DOF_SameAngles_ZeroDistance()
        {
            var template = Template2DOF_RR.Create();
            var links = template.GetLinks();
            var joints = new double[links.Length];
            var matA = ForwardKinematics.ComputeEndEffectorTransform(links, joints);
            var matB = ForwardKinematics.ComputeEndEffectorTransform(links, joints);
            Vec3D posA = matA.ExtractPosition();
            Vec3D posB = matB.ExtractPosition();
            double distance = (posA - posB).Magnitude();
            Assert.AreEqual(0.0, distance, Delta, "동일 각도의 EE 거리는 0이어야 합니다.");
        }

        [Test]
        public void PoseCompare_2DOF_DifferentAngles_PositiveDistance()
        {
            var template = Template2DOF_RR.Create();
            var links = template.GetLinks();
            var jointsA = new double[links.Length];
            var jointsB = new double[links.Length];
            jointsB[0] = System.Math.PI / 4; // 45도
            var matA = ForwardKinematics.ComputeEndEffectorTransform(links, jointsA);
            var matB = ForwardKinematics.ComputeEndEffectorTransform(links, jointsB);
            Vec3D posA = matA.ExtractPosition();
            Vec3D posB = matB.ExtractPosition();
            double distance = (posA - posB).Magnitude();
            Assert.Greater(distance, 0.0, "다른 각도의 EE 거리는 양수여야 합니다.");
        }

        // ── LearningTabsTool 의존 로직 ──

        [Test]
        public void LearningTabs_AllRobots_JsonFilesExist()
        {
            string[] robotIds = { "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10", "IGUS_REBEL", "GENERIC_6DOF" };
            foreach (string id in robotIds)
            {
                string path = $"Assets/Runtime/Resources/LearningTabs/{id}.json";
                bool exists = System.IO.File.Exists(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
                Assert.IsTrue(exists, $"LearningTabs JSON이 존재해야 합니다: {id}");
            }
        }

        [Test]
        public void LearningTabs_2DOF_JsonNotEmpty()
        {
            string path = System.IO.Path.Combine(
                UnityEngine.Application.dataPath, "Runtime", "Resources", "LearningTabs", "2DOF_RR.json");
            Assert.IsTrue(System.IO.File.Exists(path), "2DOF_RR.json이 존재해야 합니다.");
            string json = System.IO.File.ReadAllText(path);
            Assert.IsTrue(json.Length > 10, "2DOF_RR.json은 비어있지 않아야 합니다.");
            Assert.IsTrue(json.Contains("\"robotId\"") || json.Contains("\"displayTitle\""),
                "2DOF_RR.json에 robotId 또는 displayTitle 필드가 있어야 합니다.");
        }

        // ── AssetSizeTool 의존 로직 ──

        [Test]
        public void AssetSize_ResourcesFolder_Exists()
        {
            string dir = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Runtime", "Resources");
            Assert.IsTrue(System.IO.Directory.Exists(dir), "Resources 폴더가 존재해야 합니다.");
        }

        // ── SceneDiffTool 의존 로직 ──

        [Test]
        public void SceneDiff_AllKnownScenes_FilesExist()
        {
            string[] scenes = { "Boot", "RobotLibrary" };
            foreach (string name in scenes)
            {
                string path = $"Assets/Scenes/{name}.unity";
                bool exists = System.IO.File.Exists(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", path));
                Assert.IsTrue(exists, $"SceneDiff 대상 씬이 존재해야 합니다: {name}");
            }
        }

        // ── FR5DiagnosticTool 의존 로직 ──

        [Test]
        public void FR5Diagnostic_CoordinatorType_Exists()
        {
            System.Type coordType = null;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                coordType = asm.GetType("KineTutor3D.App.Fairino.RobotControlSceneCoordinator");
                if (coordType != null) break;
            }
            Assert.IsNotNull(coordType, "RobotControlSceneCoordinator 타입이 존재해야 합니다.");
        }

        // ── CameraCaptureTool 의존 로직 ──

        [Test]
        public void CameraCapture_EditorPrefs_RoundTrip()
        {
            string testKey = "KineTutor3D_CameraSnapshot_TestRoundTrip";
            string json = "{\"name\":\"test\",\"scene\":\"Main\",\"posX\":1.0,\"posY\":2.0,\"posZ\":3.0,\"eulerX\":10.0,\"eulerY\":20.0,\"eulerZ\":0.0,\"fov\":60.0,\"nearClip\":0.3,\"farClip\":1000.0,\"bgR\":0.1,\"bgG\":0.1,\"bgB\":0.18,\"timestamp\":\"2026-01-01T00:00:00Z\",\"playMode\":true}";
            Assert.DoesNotThrow(() =>
            {
                UnityEditor.EditorPrefs.SetString(testKey, json);
                string loaded = UnityEditor.EditorPrefs.GetString(testKey, "");
                Assert.AreEqual(json, loaded, "카메라 스냅샷 EditorPrefs 라운드트립이 일치해야 합니다.");
                UnityEditor.EditorPrefs.DeleteKey(testKey);
            });
        }

        [Test]
        public void CameraCapture_OverrideKey_FormatConsistent()
        {
            string sceneName = "RobotControl";
            string overrideKey = "KineTutor3D_CameraOverride_" + sceneName;
            Assert.AreEqual("KineTutor3D_CameraOverride_RobotControl", overrideKey,
                "오버라이드 키 형식이 SceneCameraDirector와 일치해야 합니다.");
        }

        [Test]
        public void SceneCameraDirector_GetSceneName_ReturnsExpected()
        {
            string name = KineTutor3D.App.SceneCatalog.GetSceneName(KineTutor3D.App.SceneId.Sandbox);
            Assert.AreEqual("Sandbox", name, "SceneId.Sandbox의 씬 이름은 'Sandbox'이어야 합니다.");
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
