// Folder: PointMove - teaching-point stores, function/block editing, and teaching runtime helpers for point move.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles teaching sequence load/select/preview/execute plus function/block editing helpers used by the point panel.
    // Live sequence execution stays in PointMove and broad diagnostics/evidence stay in StatusSafety.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string StartTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 시작"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Start(Time.timeAsDouble);
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            recordedPathSequence = null;
            PushFeedback("[Path Record] 기록 시작 · 현재 자세부터 샘플링");
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string StopTeachingPathRecording()
        {
            if (!EnsureReadyForCommand("경로 기록 중지"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            teachingPathRecorder.Stop();
            recordedPathSequence = teachingPathRecorder.BuildSequence(RecordedPathSequenceName);
            var count = recordedPathSequence.waypoints?.Length ?? 0;
            if (count >= 2)
            {
                WaypointStore.Save(recordedPathSequence);
                PushFeedback($"[Path Record] 기록 저장 · {count}개 샘플 → {RecordedPathSequenceName}");
            }
            else
            {
                PushFeedback("[Path Record] 저장할 움직임이 부족하다. 최소 2개 자세가 필요함.");
            }

            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string CaptureTeachingPathFrameForDebug()
        {
            if (!EnsureReadyForCommand("경로 샘플 캡처"))
            {
                return GetTeachingPathRecordingSummaryForDebug();
            }

            teachingPathRecorder ??= new TeachingPathRecorder();
            teachingPathRecorder.Capture(currentState, Time.timeAsDouble, force: true);
            RefreshSnapshot();
            return GetTeachingPathRecordingSummaryForDebug();
        }


        public string PlayRecordedTeachingPathOnce()
        {
            return PlayRecordedTeachingPath(loop: false);
        }


        public string PlayRecordedTeachingPathLoop()
        {
            return PlayRecordedTeachingPath(loop: true);
        }


        public string GetTeachingPathRecordingSummaryForDebug()
        {
            var recorder = teachingPathRecorder?.ToDebugSummary() ?? "recording=False; samples=0";
            var saved = ResolveRecordedPathSequence();
            var savedCount = saved?.waypoints?.Length ?? 0;
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            return $"{recorder}; saved={savedCount}; runner={runnerState}; sequence={RecordedPathSequenceName}; feedback={snapshot.LastFeedback}";
        }


        public string SimulateManualReadbackForDebug(double[] jointsDeg, double[] tcpMm)
        {
            ForceInitialize();
            manualReadbackTeachingProbe ??= new ManualReadbackTeachingProbe(connectionService);
            var result = manualReadbackTeachingProbe.SimulateManualMove(jointsDeg, tcpMm);
            RefreshSnapshot();
            return result.IsSuccess
                ? $"manualReadback=True; {FormatRobotStateForDebug(result.Value)}; {GetDebugSummary()}"
                : $"manualReadback=False; error={result.Message}; {GetDebugSummary()}";
        }


        public string GetTeachingPointStoreSummaryForDebug()
        {
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            return teachingPointStoreAdapter.BuildSummary();
        }


        public string LoadTeachingSequenceForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            return $"{teachingSequenceRuntime.ToDebugSummary()}; {GetTeachingLoopSummaryForDebug()}";
        }


        public string GetTeachingLoopSummaryForDebug()
        {
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            var runnerIndex = waypointRunner != null ? waypointRunner.CurrentIndex : -1;
            var runnerTotal = waypointRunner != null ? waypointRunner.TotalCount : 0;
            return $"loopEnabled={teachingLoopEnabled}; runnerState={runnerState}; runnerIndex={runnerIndex}; runnerTotal={runnerTotal}; isTeachingRunning={IsTeachingSequenceRunning}";
        }


        public bool SetTeachingLoopEnabled(bool enabled)
        {
            teachingLoopEnabled = enabled;
            PushFeedback(enabled ? "[Teaching Loop] 반복 실행 ON" : "[Teaching Loop] 반복 실행 OFF");
            RefreshSnapshot();
            return teachingLoopEnabled;
        }


        public bool ToggleTeachingLoopEnabled()
        {
            return SetTeachingLoopEnabled(!teachingLoopEnabled);
        }


        public string SelectTeachingPointForDebug(int index)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            return teachingSequenceRuntime.ToDebugSummary();
        }


        public string PreviewSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }


        public string ExecuteSelectedTeachingPointForDebug()
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
            RefreshSnapshot();
            return $"{result.Message}; {teachingSequenceRuntime.ToDebugSummary()}; {GetDebugSummary()}";
        }


        public string ExecuteTeachingSequenceFromPoint(string pointName)
        {
            ForceInitialize();
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Teaching From] 실행할 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var startIndex = FindWaypointIndex(sequence, pointName);
            if (startIndex < 0)
            {
                PushFeedback($"[Teaching From] {pointName} 포인트를 찾지 못했다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (waypointRunner != null && waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Teaching From] 실행 중인 반복이 있다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var executed = 0;
            for (var index = startIndex; index < sequence.waypoints.Length; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = ExecuteTeachingWaypoint(sequence.waypoints[index]);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching From] {index + 1}/{sequence.waypoints.Length} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                executed++;
            }

            PushFeedback($"[Teaching From] {startIndex + 1}/{sequence.waypoints.Length}부터 {executed}개 포인트 실행 완료");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        public string ExecuteTeachingSequenceFromPointForDebug(string pointName)
        {
            return ExecuteTeachingSequenceFromPoint(pointName);
        }


        public string CreateTeachingFunctionFromSequence(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function] 묶을 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var function = teachingFunctionStore.CreateFromSequence(teachingFunctionStore.BuildUniqueName(functionName), sequence);
            if (function == null || !teachingFunctionStore.Save(function))
            {
                PushFeedback("[Function] 함수 저장 실패");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            PushFeedback($"[Function] {function.name} 생성 · {function.steps.Length}개 포인트");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildDetail(function.name)}";
        }


        public string CreateTeachingFunctionFromPoints(string functionName, string[] pointNames)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function] 묶을 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (pointNames == null || pointNames.Length == 0)
            {
                return CreateTeachingFunctionFromSequence(functionName);
            }

            var filtered = new System.Collections.Generic.List<string>();
            for (var index = 0; index < pointNames.Length; index++)
            {
                var pointName = pointNames[index]?.Trim();
                if (string.IsNullOrWhiteSpace(pointName))
                {
                    continue;
                }

                if (FindWaypoint(sequence, pointName) == null)
                {
                    PushFeedback($"[Function] {pointName} 포인트를 찾지 못했다.");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                if (!filtered.Contains(pointName))
                {
                    filtered.Add(pointName);
                }
            }

            if (filtered.Count == 0)
            {
                return CreateTeachingFunctionFromSequence(functionName);
            }

            var function = teachingFunctionStore.CreateFromPointRefs(teachingFunctionStore.BuildUniqueName(functionName), filtered.ToArray(), TeachingPointStoreAdapter.DefaultSequenceName);
            if (function == null || !teachingFunctionStore.Save(function))
            {
                PushFeedback("[Function] 함수 저장 실패");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            PushFeedback($"[Function] {function.name} 생성 · 선택 {function.steps.Length}개 포인트");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {GetTeachingFunctionDetailForDebug(function.name)}";
        }


        public string GetTeachingFunctionSummaryForDebug()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            return teachingFunctionStore.BuildSummary();
        }


        public string[] GetTeachingFunctionNames()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            return teachingFunctionStore.LoadAllNames();
        }


        public string GetTeachingFunctionDetailForDebug(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var detail = teachingFunctionStore.BuildDetail(functionName);
            var function = teachingFunctionStore.Load(functionName);
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (function?.steps == null)
            {
                return detail;
            }

            var missing = new System.Collections.Generic.List<string>();
            for (var index = 0; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step == null || !step.enabled || !string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (FindWaypoint(sequence, step.refName) == null)
                {
                    missing.Add(step.refName);
                }
            }

            return $"{detail}; missingCount={missing.Count}; missing=[{string.Join(",", missing)}]";
        }


        public string RenameTeachingFunctionForDebug(string oldName, string newName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var ok = teachingFunctionStore.Rename(oldName, newName);
            PushFeedback(ok ? $"[Function] {oldName} -> {newName}" : "[Function] 이름 변경 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }


        public string DuplicateTeachingFunctionForDebug(string sourceName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var copy = teachingFunctionStore.Duplicate(sourceName);
            PushFeedback(copy != null ? $"[Function] {sourceName} 복사 -> {copy.name}" : "[Function] 복사 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }


        public string DeleteTeachingFunctionForDebug(string functionName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var ok = teachingFunctionStore.Delete(functionName);
            PushFeedback(ok ? $"[Function] {functionName} 삭제" : "[Function] 삭제 실패");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }


        public string DeleteAllTeachingFunctionsForDebug()
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            var deleted = teachingFunctionStore.DeleteAll();
            PushFeedback($"[Bundle] 전체 묶음 {deleted}개 삭제");
            RefreshSnapshot();
            return $"{snapshot.LastFeedback}; {teachingFunctionStore.BuildSummary()}";
        }


        public string AddTeachingBlockPoint(string pointName)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (FindWaypoint(sequence, pointName) == null)
            {
                PushFeedback($"[Block Sequence] {pointName} 포인트를 찾지 못했다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var ok = teachingBlockSequenceStore.AddBlock(TeachingSequenceBlock.PointRefKind, pointName);
            PushFeedback(ok ? $"[Block Sequence] 포인트 {pointName} 추가" : "[Block Sequence] 포인트 추가 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string AddTeachingBlockBundle(string bundleName)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingFunctionStore ??= new TeachingFunctionStore();
            if (teachingFunctionStore.Load(bundleName) == null)
            {
                PushFeedback($"[Block Sequence] {bundleName} 묶음을 찾지 못했다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var ok = teachingBlockSequenceStore.AddBlock(TeachingSequenceBlock.BundleRefKind, bundleName);
            PushFeedback(ok ? $"[Block Sequence] 묶음 {bundleName} 추가" : "[Block Sequence] 묶음 추가 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string MoveTeachingBlock(int index, int direction)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var ok = teachingBlockSequenceStore.MoveBlock(index, direction);
            PushFeedback(ok ? $"[Block Sequence] {index}번 블록 이동" : "[Block Sequence] 블록 이동 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string DeleteTeachingBlock(int index)
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var ok = teachingBlockSequenceStore.DeleteBlock(index);
            PushFeedback(ok ? $"[Block Sequence] {index}번 블록 삭제" : "[Block Sequence] 블록 삭제 실패");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string ClearTeachingBlockSequenceForDebug()
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingBlockSequenceStore.Clear();
            PushFeedback("[Block Sequence] 작업 시퀀스 초기화");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string PreviewTeachingBlockSequence()
        {
            ForceInitialize();
            var sequence = ExpandTeachingBlockSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Block Preview] 미리보기할 작업 시퀀스가 없다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var result = PreviewTeachingWaypoint(sequence.waypoints[0]);
            PushFeedback(result.IsSuccess
                ? $"[Block Preview] 1/{sequence.waypoints.Length} {sequence.waypoints[0].name}"
                : result.Message);
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string ExecuteTeachingBlockSequenceDryRun()
        {
            ForceInitialize();
            var sequence = ExpandTeachingBlockSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Block Run] 실행할 작업 시퀀스가 없다.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            var restoreDryRun = !snapshot.DryRunEnabled;
            if (restoreDryRun)
            {
                ToggleDryRun();
            }

            if (waypointRunner == null)
            {
                EnsureRuntimeHelpers();
            }

            if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
            {
                PushFeedback("[Block Run] 이미 실행 중이다. Stop 후 다시 실행해라.");
                RefreshSnapshot();
                return GetTeachingBlockSequenceSummaryForDebug();
            }

            waypointRunner.PlayOnce(sequence, dryRun: true);
            PushFeedback($"[Block Run] {sequence.waypoints.Length}개 포인트 DryRun 시작");
            RefreshSnapshot();
            return GetTeachingBlockSequenceSummaryForDebug();
        }


        public string GetTeachingBlockSequenceSummaryForDebug()
        {
            ForceInitialize();
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            var expanded = ExpandTeachingBlockSequence();
            var expandedCount = expanded?.waypoints?.Length ?? 0;
            var runnerState = waypointRunner != null ? waypointRunner.State.ToString() : "missing";
            return $"{teachingBlockSequenceStore.BuildSummary()}; expanded={expandedCount}; runner={runnerState}; feedback={snapshot.LastFeedback}";
        }


        public string ExecuteTeachingFunctionOnceDryRun(string functionName)
        {
            return ExecuteTeachingFunctionDryRun(functionName, null);
        }


        public string ExecuteTeachingFunctionFromPointDryRun(string functionName, string pointName)
        {
            return ExecuteTeachingFunctionDryRun(functionName, pointName);
        }


        private string ExecuteTeachingFunctionDryRun(string functionName, string startPointName)
        {
            ForceInitialize();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var function = teachingFunctionStore.Load(functionName);
            var sequence = teachingPointStoreAdapter.LoadIfExists();
            if (function?.steps == null || function.steps.Length == 0)
            {
                PushFeedback($"[Function Run] {functionName} 함수가 비어 있다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
            {
                PushFeedback("[Function Run] 참조할 저장 포인트가 없다.");
                RefreshSnapshot();
                return snapshot.LastFeedback;
            }

            var startIndex = 0;
            if (!string.IsNullOrWhiteSpace(startPointName))
            {
                startIndex = FindFunctionStepIndex(function, startPointName);
                if (startIndex < 0)
                {
                    PushFeedback($"[Function Run] {function.name} 안에서 {startPointName} 참조를 찾지 못했다.");
                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }
            }

            var restoreDryRun = !snapshot.DryRunEnabled;
            if (restoreDryRun)
            {
                ToggleDryRun();
            }

            var executed = 0;
            for (var index = startIndex; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step == null || !step.enabled)
                {
                    continue;
                }

                if (!string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    PushFeedback($"[Function Run] {step.kind} step은 v1에서 제외다.");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                var point = FindWaypoint(sequence, step.refName);
                if (point == null)
                {
                    PushFeedback($"[Function Run] {step.refName} 포인트를 찾지 못했다.");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                var result = ExecuteTeachingWaypoint(point);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Function Run] {function.name} {index + 1}/{function.steps.Length} 실패 · {result.Message}");
                    if (restoreDryRun && snapshot.DryRunEnabled)
                    {
                        ToggleDryRun();
                    }

                    RefreshSnapshot();
                    return snapshot.LastFeedback;
                }

                executed++;
            }

            if (restoreDryRun && snapshot.DryRunEnabled)
            {
                ToggleDryRun();
            }

            var prefix = string.IsNullOrWhiteSpace(startPointName)
                ? "[Function Run]"
                : "[Function From]";
            PushFeedback($"{prefix} {function.name} DryRun {executed}개 포인트 실행 완료");
            RefreshSnapshot();
            return snapshot.LastFeedback;
        }


        private WaypointSequence ExpandTeachingBlockSequence()
        {
            teachingBlockSequenceStore ??= new TeachingBlockSequenceStore();
            teachingFunctionStore ??= new TeachingFunctionStore();
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var blocks = teachingBlockSequenceStore.LoadOrCreate().blocks ?? Array.Empty<TeachingSequenceBlock>();
            var pointSequence = teachingPointStoreAdapter.LoadIfExists();
            var expanded = WaypointStore.CreateEmpty("PendantV3BlocksExpanded");
            for (var index = 0; index < blocks.Length; index++)
            {
                var block = blocks[index];
                if (block == null || !block.enabled || string.IsNullOrWhiteSpace(block.refName))
                {
                    continue;
                }

                if (string.Equals(block.kind, TeachingSequenceBlock.BundleRefKind, StringComparison.OrdinalIgnoreCase))
                {
                    ExpandBundleBlock(teachingFunctionStore.Load(block.refName), pointSequence, expanded);
                    continue;
                }

                var point = FindWaypoint(pointSequence, block.refName);
                if (point != null)
                {
                    WaypointStore.AddWaypoint(expanded, CloneWaypoint(point));
                }
            }

            return expanded;
        }


        private static void ExpandBundleBlock(TeachingFunction function, WaypointSequence pointSequence, WaypointSequence expanded)
        {
            var steps = function?.steps ?? Array.Empty<TeachingFunctionStep>();
            for (var index = 0; index < steps.Length; index++)
            {
                var step = steps[index];
                if (step == null
                    || !step.enabled
                    || !string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var point = FindWaypoint(pointSequence, step.refName);
                if (point != null)
                {
                    WaypointStore.AddWaypoint(expanded, CloneWaypoint(point));
                }
            }
        }


        private static Waypoint CloneWaypoint(Waypoint point)
        {
            return new Waypoint
            {
                name = point?.name ?? string.Empty,
                jointsDeg = point?.jointsDeg != null ? (double[])point.jointsDeg.Clone() : new double[6],
                tcpMm = point?.tcpMm != null ? (double[])point.tcpMm.Clone() : new double[6],
                moveType = point?.moveType ?? "MoveJ",
                speedPreset = point?.speedPreset ?? "medium",
                dwellSec = point?.dwellSec ?? 0.0
            };
        }


        private bool TryRunTeachingSequenceOnce()
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (teachingLoopEnabled)
            {
                var sequence = teachingPointStoreAdapter.LoadIfExists();
                if (sequence?.waypoints == null || sequence.waypoints.Length == 0)
                {
                    return false;
                }

                if (waypointRunner == null)
                {
                    EnsureRuntimeHelpers();
                }

                if (waypointRunner.State != WaypointCycleRunner.RunState.Idle)
                {
                    PushFeedback("[Teaching Loop] 이미 반복 실행 중이다. Stop 후 다시 실행해라.");
                    RefreshSnapshot();
                    return true;
                }

                waypointRunner.PlayLoop(sequence, snapshot.DryRunEnabled || connectionService == null || connectionService.IsMockMode);
                PushFeedback($"[Teaching Loop] {teachingSequenceRuntime.Count}개 포인트 반복 실행 시작");
                RefreshSnapshot();
                return true;
            }

            for (var index = 0; index < teachingSequenceRuntime.Count; index++)
            {
                teachingSequenceRuntime.Select(index);
                var result = teachingSequenceRuntime.ExecuteSelected(ExecuteTeachingWaypoint);
                if (!result.IsSuccess)
                {
                    PushFeedback($"[Teaching Run] {index + 1}/{teachingSequenceRuntime.Count} 실패 · {result.Message}");
                    RefreshSnapshot();
                    return true;
                }
            }

            PushFeedback($"[Teaching Run] {teachingSequenceRuntime.Count}개 포인트 실행 완료");
            RefreshSnapshot();
            return true;
        }


        private bool PreviewTeachingStep(int delta)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Load();
            if (teachingSequenceRuntime.Count <= 0)
            {
                return false;
            }

            if (delta >= 0)
            {
                teachingSequenceRuntime.SelectNext();
            }
            else
            {
                teachingSequenceRuntime.SelectPrevious();
            }

            var result = teachingSequenceRuntime.PreviewSelected(PreviewTeachingWaypoint);
            PushFeedback(result.IsSuccess
                ? $"[Teaching Step] {teachingSequenceRuntime.State.SelectedIndex + 1}/{teachingSequenceRuntime.Count} 미리보기"
                : result.Message);
            RefreshSnapshot();
            return true;
        }


        private FairinoResult PreviewTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-94, "teaching point missing");
            }

            if (string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase))
            {
                PreviewTcpPose(point.tcpMm, $"Teaching {point.name} MoveL preview");
                return FairinoResult.Ok($"preview MoveL {point.name}");
            }

            PreviewJointAngles(point.jointsDeg, $"Teaching {point.name} MoveJ preview");
            return FairinoResult.Ok($"preview MoveJ {point.name}");
        }


        private FairinoResult ExecuteTeachingWaypoint(Waypoint point)
        {
            if (point == null)
            {
                return FairinoResult.Fail(-95, "teaching point missing");
            }

            return string.Equals(point.moveType, "MoveL", StringComparison.OrdinalIgnoreCase)
                ? ApplyTcpPose(point.tcpMm, $"Teaching {point.name} MoveL")
                : ApplyTeachingMoveJ(point.jointsDeg, $"Teaching {point.name} MoveJ");
        }


        private static int FindWaypointIndex(WaypointSequence sequence, string pointName)
        {
            if (sequence?.waypoints == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            for (var index = 0; index < sequence.waypoints.Length; index++)
            {
                var waypoint = sequence.waypoints[index];
                if (waypoint != null && string.Equals(waypoint.name, pointName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }


        private static Waypoint FindWaypoint(WaypointSequence sequence, string pointName)
        {
            var index = FindWaypointIndex(sequence, pointName);
            return index >= 0 ? sequence.waypoints[index] : null;
        }


        private static int FindFunctionStepIndex(TeachingFunction function, string pointName)
        {
            if (function?.steps == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            for (var index = 0; index < function.steps.Length; index++)
            {
                var step = function.steps[index];
                if (step != null
                    && step.enabled
                    && string.Equals(step.kind, "PointRef", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(step.refName, pointName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }


        private void BindWaypointRunnerEvents()
        {
            if (waypointRunner == null || presetAnimator == null || waypointRunnerEventsBound)
            {
                return;
            }

            waypointRunner.OnWaypointReached += OnWaypointRunnerReached;
            waypointRunner.OnSequenceComplete += OnWaypointRunnerComplete;
            waypointRunner.OnError += OnWaypointRunnerError;
            waypointRunner.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            presetAnimator.OnFrameUpdated += OnWaypointRunnerFrameUpdated;
            waypointRunnerEventsBound = true;
        }


        private void UnbindWaypointRunnerEvents()
        {
            if (!waypointRunnerEventsBound)
            {
                return;
            }

            if (waypointRunner != null)
            {
                waypointRunner.OnWaypointReached -= OnWaypointRunnerReached;
                waypointRunner.OnSequenceComplete -= OnWaypointRunnerComplete;
                waypointRunner.OnError -= OnWaypointRunnerError;
                waypointRunner.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            if (presetAnimator != null)
            {
                presetAnimator.OnFrameUpdated -= OnWaypointRunnerFrameUpdated;
            }

            waypointRunnerEventsBound = false;
        }


        private void OnWaypointRunnerReached(int index, string pointName)
        {
            teachingSequenceRuntime ??= new TeachingSequenceRuntime(teachingPointStoreAdapter);
            teachingSequenceRuntime.Select(index);
            PushFeedback($"[Teaching Loop] {index + 1}/{waypointRunner.TotalCount} {pointName} 도달");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerComplete()
        {
            PushFeedback(teachingLoopEnabled ? "[Teaching Loop] 반복 실행 정지" : "[Teaching Run] 시퀀스 완료");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerError(string message)
        {
            PushFeedback($"[Teaching Loop] {message}");
            RefreshSnapshot();
        }


        private void OnWaypointRunnerFrameUpdated(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null || jointAnglesDeg.Length < templateDefinition.JointCount)
            {
                return;
            }

            currentState = new FairinoRobotState(jointAnglesDeg, ComputeTcpPoseFromJoints(jointAnglesDeg), isRobotEnabled: connectionService.Client.IsEnabled);
            templateDefinition.PosePresetProvider?.UpdateCurrent(jointAnglesDeg);
            previewJointAnglesDeg = null;
            previewTcpPose = null;
            previewTcpVisualJointAnglesDeg = null;
            previewUsesJointPose = false;
            requestStageRefocus = true;
            ApplyVisualState();
            RefreshSnapshot();
        }

    }
}
