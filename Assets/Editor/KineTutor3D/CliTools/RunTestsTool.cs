// Folder: Editor/CliTools - unity-cli 커스텀 도구: 테스트 실행
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// EditMode 또는 PlayMode 테스트를 실행하고 결과를 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Run EditMode or PlayMode tests and return results summary")]
    public static class RunTestsTool
    {
        public class Parameters
        {
            [ToolParameter("Test mode: edit or play")]
            public string Mode { get; set; }

            [ToolParameter("Test name filter (optional)", Required = false)]
            public string Filter { get; set; }

            [ToolParameter("Return last run results instead of launching (optional)", Required = false)]
            public bool Results { get; set; }
        }

        private static ResultCollector lastCollector;

        private class ResultCollector : ICallbacks
        {
            public int total;
            public int passed;
            public int failed;
            public int skipped;
            public bool finished;
            public readonly List<string> failures = new List<string>();
            public DateTime startTime = DateTime.Now;

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                finished = true;
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren)
                {
                    total++;
                    switch (result.TestStatus)
                    {
                        case TestStatus.Passed:
                            passed++;
                            break;
                        case TestStatus.Failed:
                            failed++;
                            failures.Add($"{result.FullName}: {result.Message}");
                            break;
                        case TestStatus.Skipped:
                            skipped++;
                            break;
                    }
                }
            }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            bool checkResults = p.GetBool("results", false);

            if (checkResults)
                return GetLastResults();

            string mode = p.Get("mode", "edit");

            TestMode testMode;
            switch (mode.ToLowerInvariant())
            {
                case "play":
                case "playmode":
                    testMode = TestMode.PlayMode;
                    break;
                default:
                    testMode = TestMode.EditMode;
                    break;
            }

            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            try
            {
                lastCollector = new ResultCollector();
                api.RegisterCallbacks(lastCollector);

                var filter = new Filter
                {
                    testMode = testMode
                };

                string nameFilter = p.Get("filter", null);
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    filter.testNames = new[] { nameFilter };
                }

                api.Execute(new ExecutionSettings(filter));

                return new SuccessResponse(
                    $"Tests launched ({testMode}). Use run-tests --results to check completion.",
                    new
                    {
                        mode = testMode.ToString(),
                        status = "launched"
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Failed to launch tests: {ex.Message}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(api);
            }
        }

        private static object GetLastResults()
        {
            if (lastCollector == null)
                return new ErrorResponse("No test run has been launched yet. Use run-tests --mode edit first.");

            double elapsed = (DateTime.Now - lastCollector.startTime).TotalSeconds;

            return new SuccessResponse(
                lastCollector.finished
                    ? $"Tests finished: {lastCollector.passed}/{lastCollector.total} passed."
                    : $"Tests in progress ({lastCollector.total} completed so far, {elapsed:F1}s elapsed).",
                new
                {
                    finished = lastCollector.finished,
                    total = lastCollector.total,
                    passed = lastCollector.passed,
                    failed = lastCollector.failed,
                    skipped = lastCollector.skipped,
                    elapsed_seconds = System.Math.Round(elapsed, 1),
                    failures = lastCollector.failures.Count > 0 ? lastCollector.failures : null
                });
        }
    }
}
