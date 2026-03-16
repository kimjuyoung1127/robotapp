// Folder: Editor/CliTools - unity-cli 커스텀 도구: Resources 폴더 무결성 검증
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Resources 폴더의 필수 에셋 존재 여부와 LearningTabs JSON 참조를 검증합니다.
    /// </summary>
    [UnityCliTool(Description = "Validate Resources folder: required assets, LearningTabs, TutorSteps")]
    public static class ResourceValidateTool
    {
        private static readonly string[] expectedTutorSteps =
        {
            "S01", "S02", "S03", "S04", "S05", "S06", "S07", "S08"
        };

        private static readonly string[] expectedLearningTabs =
        {
            "2DOF_RR", "SCARA_RV", "FAIRINO_FR5", "FANUC_CRX10", "IGUS_REBEL", "GENERIC_6DOF"
        };

        private static readonly string[] expectedRobotPrefabs =
        {
            "FAIRINO_FR5", "FAIRINO_FR5_Control", "ScaraRobot", "FanucCRX-10iA_L", "igusRebel"
        };

        public static object HandleCommand(JObject @params)
        {
            string resourcesRoot = Path.Combine(Application.dataPath, "Runtime", "Resources");
            var issues = new List<string>();
            var sections = new List<object>();

            // TutorSteps 검증
            int tutorFound = 0;
            var tutorMissing = new List<string>();
            foreach (string step in expectedTutorSteps)
            {
                string path = Path.Combine(resourcesRoot, "TutorSteps", step + ".asset");
                if (File.Exists(path))
                    tutorFound++;
                else
                {
                    tutorMissing.Add(step);
                    issues.Add($"TutorSteps/{step}.asset 누락");
                }
            }
            sections.Add(new { category = "TutorSteps", expected = expectedTutorSteps.Length, found = tutorFound, missing = tutorMissing.Count > 0 ? tutorMissing : null });

            // LearningTabs 검증
            int tabsFound = 0;
            var tabsMissing = new List<string>();
            foreach (string tab in expectedLearningTabs)
            {
                string path = Path.Combine(resourcesRoot, "LearningTabs", tab + ".json");
                if (File.Exists(path))
                    tabsFound++;
                else
                {
                    tabsMissing.Add(tab);
                    issues.Add($"LearningTabs/{tab}.json 누락");
                }
            }
            sections.Add(new { category = "LearningTabs", expected = expectedLearningTabs.Length, found = tabsFound, missing = tabsMissing.Count > 0 ? tabsMissing : null });

            // Robot Prefabs 검증
            int prefabsFound = 0;
            var prefabsMissing = new List<string>();
            foreach (string prefab in expectedRobotPrefabs)
            {
                string path = Path.Combine(resourcesRoot, "Robots", prefab + ".prefab");
                if (File.Exists(path))
                    prefabsFound++;
                else
                {
                    prefabsMissing.Add(prefab);
                    issues.Add($"Robots/{prefab}.prefab 누락");
                }
            }
            sections.Add(new { category = "RobotPrefabs", expected = expectedRobotPrefabs.Length, found = prefabsFound, missing = prefabsMissing.Count > 0 ? prefabsMissing : null });

            // Glossary 검증
            string glossaryDbPath = Path.Combine(resourcesRoot, "Glossary", "GlossaryDatabase.asset");
            bool glossaryExists = File.Exists(glossaryDbPath);
            int glossaryTermCount = 0;
            if (glossaryExists)
            {
                string glossaryDir = Path.Combine(resourcesRoot, "Glossary");
                foreach (string file in Directory.GetFiles(glossaryDir, "*.asset"))
                {
                    if (!Path.GetFileName(file).StartsWith("GlossaryDatabase"))
                        glossaryTermCount++;
                }
            }
            else
            {
                issues.Add("Glossary/GlossaryDatabase.asset 누락");
            }
            sections.Add(new { category = "Glossary", database_exists = glossaryExists, term_count = glossaryTermCount });

            // Onboarding 검증
            string onboardingPath = Path.Combine(resourcesRoot, "Onboarding", "OnboardingSequenceConfig.asset");
            bool onboardingExists = File.Exists(onboardingPath);
            if (!onboardingExists)
                issues.Add("Onboarding/OnboardingSequenceConfig.asset 누락");
            sections.Add(new { category = "Onboarding", config_exists = onboardingExists });

            bool valid = issues.Count == 0;

            return new SuccessResponse(
                valid
                    ? "Resources validation passed."
                    : $"Resources validation: {issues.Count} issue(s).",
                new
                {
                    valid,
                    issue_count = issues.Count,
                    sections,
                    issues = issues.Count > 0 ? issues : null
                });
        }
    }
}
