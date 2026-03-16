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
        }

        private class ResultCollector : ICallbacks
        {
            public int total;
            public int passed;
            public int failed;
            public int skipped;
            public bool finished;
            public readonly List<string> failures = new List<string>();

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
            var collector = new ResultCollector();

            try
            {
                api.RegisterCallbacks(collector);

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
                    $"Tests launched ({testMode}). Results arrive asynchronously via callbacks.",
                    new
                    {
                        mode = testMode.ToString(),
                        status = "launched",
                        note = "Use console-check to monitor progress. Results are collected asynchronously."
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Failed to launch tests: {ex.Message}");
            }
        }
    }
}
