// Folder: Editor/CliTools - unity-cli 커스텀 도구: 테스트 실행
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEditor;
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
        private const string StateKey = "KineTutor3D.RunTestsTool.State";
        private const string ActiveKey = "KineTutor3D.RunTestsTool.Active";

        private static TestRunnerApi activeApi;
        private static bool callbacksRegistered;
        private static readonly PersistentResultCollector persistentCollector = new PersistentResultCollector();

        public class Parameters
        {
            [ToolParameter("Test mode: edit or play")]
            public string Mode { get; set; }

            [ToolParameter("Test name filter (optional)", Required = false)]
            public string Filter { get; set; }

            [ToolParameter("Return last run results instead of launching (optional)", Required = false)]
            public bool Results { get; set; }
        }

        [Serializable]
        private sealed class StoredResults
        {
            public int total;
            public int passed;
            public int failed;
            public int skipped;
            public bool finished;
            public bool launched;
            public string mode = string.Empty;
            public long startedAtUtcTicks;
            public List<string> failures = new List<string>();
            public List<string> allTestNames = new List<string>();
        }

        private sealed class PersistentResultCollector : ICallbacks, IErrorCallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                var state = LoadState() ?? new StoredResults();
                state.launched = true;
                state.finished = false;
                state.total = 0;
                state.passed = 0;
                state.failed = 0;
                state.skipped = 0;
                state.failures = new List<string>();
                state.allTestNames = new List<string>();
                if (state.startedAtUtcTicks == 0)
                {
                    state.startedAtUtcTicks = DateTime.UtcNow.Ticks;
                }

                SaveState(state);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var state = LoadState() ?? new StoredResults();
                state.finished = true;
                SaveState(state);
                SessionState.SetBool(ActiveKey, false);
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren)
                {
                    var state = LoadState() ?? new StoredResults();
                    state.total++;
                    state.allTestNames ??= new List<string>();
                    state.failures ??= new List<string>();
                    state.allTestNames.Add(result.FullName);

                    switch (result.TestStatus)
                    {
                        case TestStatus.Passed:
                            state.passed++;
                            break;
                        case TestStatus.Failed:
                            state.failed++;
                            state.failures.Add($"{result.FullName}: {result.Message}");
                            break;
                        case TestStatus.Skipped:
                            state.skipped++;
                            break;
                    }

                    SaveState(state);
                }
            }

            public void OnError(string message)
            {
                var state = LoadState() ?? new StoredResults();
                state.finished = true;
                state.failures ??= new List<string>();
                state.failed = System.Math.Max(state.failed, 1);
                state.failures.Add($"RUN_ERROR: {message}");
                SaveState(state);
                SessionState.SetBool(ActiveKey, false);
            }
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            if (SessionState.GetBool(ActiveKey, false))
            {
                EnsurePersistentRegistration();
            }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            bool checkResults = p.GetBool("results", false);

            bool verbose = p.GetBool("verbose", false);
            if (checkResults)
                return GetLastResults(verbose);

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

            try
            {
                ResetState(testMode.ToString());
                SessionState.SetBool(ActiveKey, true);
                if (activeApi != null)
                {
                    UnityEngine.Object.DestroyImmediate(activeApi);
                    activeApi = null;
                }

                EnsurePersistentRegistration();

                var filter = new Filter
                {
                    testMode = testMode
                };

                string nameFilter = p.Get("filter", null);
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    filter.testNames = new[] { nameFilter };
                }

                activeApi.Execute(new ExecutionSettings(filter));

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
        }

        private static object GetLastResults(bool verbose = false)
        {
            var state = LoadState();
            if (state == null || !state.launched)
                return new ErrorResponse("No test run has been launched yet. Use run-tests --mode edit first.");

            double elapsed = state.startedAtUtcTicks > 0
                ? (DateTime.UtcNow - new DateTime(state.startedAtUtcTicks, DateTimeKind.Utc)).TotalSeconds
                : 0d;

            if (state.finished && activeApi != null)
            {
                UnityEngine.Object.DestroyImmediate(activeApi);
                activeApi = null;
                callbacksRegistered = false;
            }

            return new SuccessResponse(
                state.finished
                    ? $"Tests finished: {state.passed}/{state.total} passed."
                    : $"Tests in progress ({state.total} completed so far, {elapsed:F1}s elapsed).",
                new
                {
                    finished = state.finished,
                    total = state.total,
                    passed = state.passed,
                    failed = state.failed,
                    skipped = state.skipped,
                    elapsed_seconds = System.Math.Round(elapsed, 1),
                    failures = state.failures != null && state.failures.Count > 0 ? state.failures : null,
                    all_test_names = verbose ? state.allTestNames : null
                });
        }

        private static void EnsurePersistentRegistration()
        {
            if (callbacksRegistered && activeApi != null)
            {
                return;
            }

            if (activeApi == null)
            {
                activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            }

            activeApi.RegisterCallbacks(persistentCollector);
            callbacksRegistered = true;
        }

        private static void ResetState(string mode)
        {
            var state = new StoredResults
            {
                mode = mode,
                launched = true,
                finished = false,
                startedAtUtcTicks = DateTime.UtcNow.Ticks,
                failures = new List<string>(),
                allTestNames = new List<string>()
            };

            SaveState(state);
        }

        private static StoredResults LoadState()
        {
            var json = SessionState.GetString(StateKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<StoredResults>(json);
            }
            catch
            {
                return null;
            }
        }

        private static void SaveState(StoredResults state)
        {
            SessionState.SetString(StateKey, JsonUtility.ToJson(state));
        }
    }
}
