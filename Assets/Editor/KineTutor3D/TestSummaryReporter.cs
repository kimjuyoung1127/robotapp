// Editor-only: Parses NUnit XML test results and generates a markdown summary report.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    /// <summary>
    /// Unity Test Runner의 NUnit XML 결과를 파싱하여 마크다운 요약 리포트를 생성합니다.
    /// 메뉴: KineTutor3D > Test Summary Report
    /// </summary>
    public static class TestSummaryReporter
    {
        private const string EditModeResultPath = "Logs/editmode-results.xml";
        private const string PlayModeResultPath = "Logs/playmode-results.xml";
        private const string ReportOutputPath = "Logs/test-summary-report.md";

        [MenuItem("KineTutor3D/Generate Test Summary Report")]
        public static void GenerateReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# KineTutor3D Test Summary Report");
            sb.AppendLine();
            sb.AppendLine($"> Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var editModeResults = ParseResults(EditModeResultPath, "EditMode");
            var playModeResults = ParseResults(PlayModeResultPath, "PlayMode");

            AppendOverview(sb, editModeResults, playModeResults);
            AppendDetailedResults(sb, "EditMode", editModeResults);
            AppendDetailedResults(sb, "PlayMode", playModeResults);
            AppendFailureSummary(sb, editModeResults, playModeResults);

            var report = sb.ToString();

            var dir = Path.GetDirectoryName(ReportOutputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(ReportOutputPath, report, Encoding.UTF8);
            Debug.Log($"[TestSummaryReporter] 리포트 생성 완료: {ReportOutputPath}");
            Debug.Log(report);

            AssetDatabase.Refresh();
        }

        [MenuItem("KineTutor3D/Generate Test Summary Report (Console Only)")]
        public static void GenerateReportConsoleOnly()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== KineTutor3D Test Summary ===");
            sb.AppendLine();

            var editModeResults = ParseResults(EditModeResultPath, "EditMode");
            var playModeResults = ParseResults(PlayModeResultPath, "PlayMode");

            sb.AppendLine($"EditMode: {FormatCounts(editModeResults)}");
            sb.AppendLine($"PlayMode: {FormatCounts(playModeResults)}");
            sb.AppendLine();

            var allFailed = new List<TestCaseResult>();
            allFailed.AddRange(editModeResults.Failed);
            allFailed.AddRange(playModeResults.Failed);

            if (allFailed.Count > 0)
            {
                sb.AppendLine($"--- 실패 {allFailed.Count}건 ---");
                foreach (var f in allFailed)
                {
                    sb.AppendLine($"  FAIL: {f.FullName}");
                    if (!string.IsNullOrEmpty(f.Message))
                    {
                        sb.AppendLine($"        {f.Message}");
                    }
                }
            }
            else
            {
                sb.AppendLine("All tests passed!");
            }

            Debug.Log(sb.ToString());
        }

        private static void AppendOverview(
            StringBuilder sb,
            TestRunResults editMode,
            TestRunResults playMode)
        {
            sb.AppendLine("## Overview");
            sb.AppendLine();
            sb.AppendLine("| Suite | Total | Passed | Failed | Skipped | Duration |");
            sb.AppendLine("|-------|-------|--------|--------|---------|----------|");
            sb.AppendLine($"| EditMode | {editMode.Total} | {editMode.Passed} | {editMode.Failed.Count} | {editMode.Skipped} | {editMode.Duration:F2}s |");
            sb.AppendLine($"| PlayMode | {playMode.Total} | {playMode.Passed} | {playMode.Failed.Count} | {playMode.Skipped} | {playMode.Duration:F2}s |");

            var totalAll = editMode.Total + playMode.Total;
            var passedAll = editMode.Passed + playMode.Passed;
            var failedAll = editMode.Failed.Count + playMode.Failed.Count;
            var skippedAll = editMode.Skipped + playMode.Skipped;
            var durationAll = editMode.Duration + playMode.Duration;

            sb.AppendLine($"| **Total** | **{totalAll}** | **{passedAll}** | **{failedAll}** | **{skippedAll}** | **{durationAll:F2}s** |");
            sb.AppendLine();

            if (failedAll == 0)
            {
                sb.AppendLine("**Status: ALL TESTS PASSED**");
            }
            else
            {
                sb.AppendLine($"**Status: {failedAll} FAILURES**");
            }

            sb.AppendLine();
        }

        private static void AppendDetailedResults(
            StringBuilder sb,
            string suiteName,
            TestRunResults results)
        {
            sb.AppendLine($"## {suiteName} Details");
            sb.AppendLine();

            if (!results.FileFound)
            {
                sb.AppendLine($"*Result file not found: {(suiteName == "EditMode" ? EditModeResultPath : PlayModeResultPath)}*");
                sb.AppendLine();
                return;
            }

            if (results.ByFixture.Count == 0)
            {
                sb.AppendLine("*No test fixtures found.*");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("| Fixture | Passed | Failed | Total |");
            sb.AppendLine("|---------|--------|--------|-------|");

            foreach (var fixture in results.ByFixture)
            {
                var status = fixture.FailedCount > 0 ? "!" : " ";
                sb.AppendLine(
                    $"| {status}{fixture.Name} | {fixture.PassedCount} | {fixture.FailedCount} | {fixture.TotalCount} |");
            }

            sb.AppendLine();
        }

        private static void AppendFailureSummary(
            StringBuilder sb,
            TestRunResults editMode,
            TestRunResults playMode)
        {
            var allFailed = new List<TestCaseResult>();
            allFailed.AddRange(editMode.Failed);
            allFailed.AddRange(playMode.Failed);

            if (allFailed.Count == 0) return;

            sb.AppendLine("## Failures");
            sb.AppendLine();

            foreach (var f in allFailed)
            {
                sb.AppendLine($"### `{f.FullName}`");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(f.Message))
                {
                    sb.AppendLine("```");
                    sb.AppendLine(f.Message);
                    sb.AppendLine("```");
                }

                if (!string.IsNullOrEmpty(f.StackTrace))
                {
                    sb.AppendLine();
                    sb.AppendLine("<details><summary>Stack trace</summary>");
                    sb.AppendLine();
                    sb.AppendLine("```");
                    sb.AppendLine(f.StackTrace);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    sb.AppendLine("</details>");
                }

                sb.AppendLine();
            }
        }

        private static string FormatCounts(TestRunResults results)
        {
            if (!results.FileFound) return "(결과 파일 없음)";
            return $"{results.Passed}/{results.Total} passed, {results.Failed.Count} failed, {results.Skipped} skipped ({results.Duration:F2}s)";
        }

        private static TestRunResults ParseResults(string xmlPath, string suiteName)
        {
            var results = new TestRunResults { SuiteName = suiteName };

            if (!File.Exists(xmlPath))
            {
                results.FileFound = false;
                return results;
            }

            results.FileFound = true;

            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);

                var testRun = doc.SelectSingleNode("//test-run");
                if (testRun != null)
                {
                    results.Total = GetIntAttr(testRun, "testcasecount");
                    results.Passed = GetIntAttr(testRun, "passed");
                    results.Skipped = GetIntAttr(testRun, "skipped") + GetIntAttr(testRun, "inconclusive");
                    results.Duration = GetDoubleAttr(testRun, "duration");
                }

                var testCases = doc.SelectNodes("//test-case[@result='Failed']");
                if (testCases != null)
                {
                    foreach (XmlNode tc in testCases)
                    {
                        var failure = new TestCaseResult
                        {
                            FullName = tc.Attributes?["fullname"]?.Value ?? tc.Attributes?["name"]?.Value ?? "unknown",
                        };

                        var msgNode = tc.SelectSingleNode("failure/message");
                        if (msgNode != null) failure.Message = msgNode.InnerText;

                        var stackNode = tc.SelectSingleNode("failure/stack-trace");
                        if (stackNode != null) failure.StackTrace = stackNode.InnerText;

                        results.Failed.Add(failure);
                    }
                }

                var fixtures = doc.SelectNodes("//test-suite[@type='TestFixture']");
                if (fixtures != null)
                {
                    foreach (XmlNode fix in fixtures)
                    {
                        var fixtureSummary = new FixtureSummary
                        {
                            Name = fix.Attributes?["name"]?.Value ?? "unknown",
                            TotalCount = GetIntAttr(fix, "testcasecount"),
                            PassedCount = GetIntAttr(fix, "passed"),
                            FailedCount = GetIntAttr(fix, "failed"),
                        };
                        results.ByFixture.Add(fixtureSummary);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TestSummaryReporter] {xmlPath} 파싱 실패: {ex.Message}");
            }

            return results;
        }

        private static int GetIntAttr(XmlNode node, string attrName)
        {
            var val = node.Attributes?[attrName]?.Value;
            return int.TryParse(val, out var result) ? result : 0;
        }

        private static double GetDoubleAttr(XmlNode node, string attrName)
        {
            var val = node.Attributes?[attrName]?.Value;
            return double.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        private class TestRunResults
        {
            public string SuiteName = "";
            public bool FileFound;
            public int Total;
            public int Passed;
            public int Skipped;
            public double Duration;
            public List<TestCaseResult> Failed = new();
            public List<FixtureSummary> ByFixture = new();
        }

        private class TestCaseResult
        {
            public string FullName = "";
            public string Message = "";
            public string StackTrace = "";
        }

        private class FixtureSummary
        {
            public string Name = "";
            public int TotalCount;
            public int PassedCount;
            public int FailedCount;
        }
    }
}
#endif
