using KineTutor3D.App;
using KineTutor3D.UI.Data;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// MainLearningTabsLoader의 JSON 파싱과 코드 폴백 동작을 검증합니다.
    /// </summary>
    public class MainLearningTabsLoaderTests
    {
        [Test]
        public void ParseOrFallback_ValidJson_ParsesDocument()
        {
            const string rawJson = @"
{
  ""robotId"": ""2DOF_RR"",
  ""displayTitle"": ""RR Stage"",
  ""heroSummary"": ""hero"",
  ""legend"": [ { ""accentKey"": ""joint1"", ""labelKo"": ""Joint 1"" } ],
  ""tabs"": [
    { ""id"": ""overview"", ""labelKo"": ""개요"", ""headlineKo"": ""h1"", ""bodyKo"": ""b1"" },
    { ""id"": ""motion"", ""labelKo"": ""움직임"", ""headlineKo"": ""h2"", ""bodyKo"": ""b2"" },
    { ""id"": ""fk"", ""labelKo"": ""FK"", ""headlineKo"": ""h3"", ""bodyKo"": ""b3"" }
  ],
  ""motion"": {
    ""primaryCtaLabelKo"": ""Go"",
    ""resultTitleKo"": ""실시간 결과"",
    ""metrics"": [ { ""id"": ""ee_x"", ""labelKo"": ""EE X"", ""unitKo"": ""m"" } ],
    ""overlaySummaryKo"": ""overlay""
  },
  ""forwardKinematics"": {
    ""compactTitleKo"": ""현재 누적 변환"",
    ""formulaLinesKo"": [ ""0Tn = A1 × A2"" ],
    ""advancedDrawerLabelKo"": ""자세히 보기"",
    ""advancedCards"": [ { ""tagKo"": ""READ"", ""titleKo"": ""읽기"", ""bodyKo"": ""설명"" } ]
  }
}";

            var metadata = new KineTutor3D.Types.RobotMetadataInfo("2DOF_RR", "2DOF RR", 2, "RR", "Easy");
            var document = MainLearningTabsLoader.ParseOrFallback(rawJson, "2DOF_RR", metadata);

            Assert.That(document, Is.Not.Null);
            Assert.That(document.robotId, Is.EqualTo("2DOF_RR"));
            Assert.That(document.tabs.Length, Is.EqualTo(3));
            Assert.That(document.motion.metrics.Length, Is.EqualTo(1));
            Assert.That(document.forwardKinematics.formulaLinesKo.Length, Is.EqualTo(1));
        }

        [Test]
        public void ParseOrFallback_InvalidJson_ReturnsFallbackDocument()
        {
            var metadata = new KineTutor3D.Types.RobotMetadataInfo("SCARA_RV", "SCARA Robot", 4, "SCARA", "Medium");
            var document = MainLearningTabsLoader.ParseOrFallback("not-json", "SCARA_RV", metadata);

            Assert.That(document, Is.Not.Null);
            Assert.That(document.robotId, Is.EqualTo("SCARA_RV"));
            Assert.That(document.displayTitle, Is.EqualTo("SCARA Robot"));
            Assert.That(document.tabs.Length, Is.EqualTo(3));
            Assert.That(document.motion.metrics.Length, Is.GreaterThan(0));
        }

        [Test]
        public void BuildFallbackDocument_UsesRobotMetadata()
        {
            var metadata = new KineTutor3D.Types.RobotMetadataInfo("IGUS_REBEL", "igus REBEL", 6, "Educational", "Medium");
            var document = MainLearningTabsLoader.BuildFallbackDocument("IGUS_REBEL", metadata);

            Assert.That(document.displayTitle, Is.EqualTo("igus REBEL"));
            Assert.That(document.legend.Length, Is.EqualTo(7));
            Assert.That(document.GetTab("overview"), Is.Not.Null);
            Assert.That(document.GetTab("motion"), Is.Not.Null);
            Assert.That(document.GetTab("fk"), Is.Not.Null);
        }
    }
}
