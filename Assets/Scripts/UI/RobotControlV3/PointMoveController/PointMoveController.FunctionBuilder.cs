// Folder: PointMoveController - function builder source resolution and builder panel copy.
// Serves function creation and builder/source-point selection for the PointMove function subview.
// Function selection, run/loop, and inventory rendering live in sibling partials.
using System.Collections.Generic;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private void CreateFunctionFromSequence()
        {
            if (IsSequenceEditLocked())
            {
                SetFeedback("시퀀스 실행 중에는 묶음 생성을 잠근다. Stop 후 다시 묶어라.");
                return;
            }

            var panel = isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
            var functionName = panel?.PointFunctionNameInput?.value?.Trim();
            if (string.IsNullOrWhiteSpace(functionName))
            {
                functionName = panel?.FunctionNameInput?.value?.Trim();
            }

            if (string.IsNullOrWhiteSpace(functionName))
            {
                SetFeedback("작업 묶음 이름을 먼저 넣어라.");
                return;
            }

            var sourcePointNames = BuildCreateFunctionSourcePointNames();
            var result = runtimeController != null
                ? sourcePointNames.Length > 0
                    ? runtimeController.CreateTeachingFunctionFromPoints(functionName, sourcePointNames)
                    : runtimeController.CreateTeachingFunctionFromSequence(functionName)
                : "runtime missing";
            selectedFunctionName = ExtractCreatedFunctionName(result, functionName);
            SetFunctionName(selectedFunctionName);
            SetFeedback(result);
            ApplyAll();
        }

        private void AddSelectedPointToFunction()
        {
            if (recalledPoint == null)
            {
                SetFeedback("묶음 후보에 넣을 포인트를 먼저 선택해라.");
                return;
            }

            if (!selectedFunctionPointNames.Contains(recalledPoint.name))
            {
                selectedFunctionPointNames.Add(recalledPoint.name);
            }

            SetFeedback($"[Bundle] 후보 추가 · {recalledPoint.name}");
            ApplyAll();
        }

        private void ClearFunctionPointSelection()
        {
            selectedFunctionPointNames.Clear();
            SetFeedback("[Bundle] 묶음 후보 초기화");
            ApplyAll();
        }

        private void AddSelectedPointsToFunction()
        {
            if (selectedPointNames.Count == 0)
            {
                SetFeedback("묶음에 추가할 포인트를 먼저 선택해라.");
                return;
            }

            var added = 0;
            for (var index = 0; index < selectedPointNames.Count; index++)
            {
                var pointName = selectedPointNames[index];
                if (!selectedFunctionPointNames.Contains(pointName) && HasNamedPoint(pointName))
                {
                    selectedFunctionPointNames.Add(pointName);
                    added++;
                }
            }

            SetFeedback($"[Bundle] 선택 {added}개 추가");
            ApplyAll();
        }

        private void ApplyPointFunctionBuilder(PanelElements panel)
        {
            if (panel == null)
            {
                return;
            }

            var sourcePointNames = ResolveFunctionSourcePointNames();
            var sourceLabel = ResolveFunctionSourceLabel(sourcePointNames);
            if (panel.PointFunctionBuildSummary != null)
            {
                panel.PointFunctionBuildSummary.text = sourcePointNames.Length == 0
                    ? "저장 위치가 없다. 먼저 위치를 저장해라."
                    : $"{sourceLabel} {sourcePointNames.Length}개를 바로 작업 묶음에 등록한다.";
            }

            if (panel.PointFunctionSelectionSummary != null)
            {
                panel.PointFunctionSelectionSummary.text = sourcePointNames.Length == 0
                    ? "선택 소스 없음"
                    : $"{sourceLabel}: {string.Join(" / ", sourcePointNames)}";
            }
        }

        private string[] ResolveFunctionSourcePointNames()
        {
            var source = new List<string>();
            AppendExistingPointNames(source, selectedFunctionPointNames);
            if (source.Count > 0)
            {
                return source.ToArray();
            }

            AppendExistingPointNames(source, selectedPointNames);
            if (source.Count > 0)
            {
                return source.ToArray();
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (HasNamedPoint(currentPointName))
            {
                source.Add(currentPointName);
                return source.ToArray();
            }

            if (recalledPoint != null && HasNamedPoint(recalledPoint.name))
            {
                source.Add(recalledPoint.name);
                return source.ToArray();
            }

            var sequence = LoadPointSequenceIfExists();
            var waypoints = sequence?.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                var pointName = waypoints[index]?.name;
                if (!string.IsNullOrWhiteSpace(pointName) && !source.Contains(pointName))
                {
                    source.Add(pointName);
                }
            }

            return source.ToArray();
        }

        private string[] BuildCreateFunctionSourcePointNames()
        {
            if (selectedFunctionPointNames.Count > 0)
            {
                return ResolveFunctionSourcePointNames();
            }

            if (recalledPoint != null && HasNamedPoint(recalledPoint.name))
            {
                return new[] { recalledPoint.name };
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (HasNamedPoint(currentPointName))
            {
                return new[] { currentPointName };
            }

            if (selectedPointNames.Count > 0)
            {
                return ResolveFunctionSourcePointNames();
            }

            return ResolveFunctionSourcePointNames();
        }

        private string ResolveFunctionSourceLabel(string[] sourcePointNames)
        {
            if (sourcePointNames == null || sourcePointNames.Length == 0)
            {
                return "선택 포인트";
            }

            if (selectedFunctionPointNames.Count > 0)
            {
                return "묶음 후보";
            }

            if (selectedPointNames.Count > 0)
            {
                return "선택 포인트";
            }

            var panel = ResolveActivePanel();
            var currentPointName = panel?.PointNameInput?.value?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPointName)
                && sourcePointNames.Length == 1
                && string.Equals(sourcePointNames[0], currentPointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "현재 포인트";
            }

            if (recalledPoint != null && sourcePointNames.Length == 1 && string.Equals(sourcePointNames[0], recalledPoint.name, System.StringComparison.OrdinalIgnoreCase))
            {
                return "현재 선택";
            }

            return "전체 포인트";
        }

        private void AppendExistingPointNames(List<string> target, List<string> names)
        {
            if (target == null || names == null)
            {
                return;
            }

            for (var index = 0; index < names.Count; index++)
            {
                var pointName = names[index];
                if (string.IsNullOrWhiteSpace(pointName) || !HasNamedPoint(pointName) || target.Contains(pointName))
                {
                    continue;
                }

                target.Add(pointName);
            }
        }

        private static string ExtractCreatedFunctionName(string result, string fallbackName)
        {
            const string marker = "[Function] ";
            const string endMarker = " 생성";
            if (string.IsNullOrWhiteSpace(result))
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            var start = result.IndexOf(marker, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return fallbackName?.Trim() ?? string.Empty;
            }

            start += marker.Length;
            var end = result.IndexOf(endMarker, start, System.StringComparison.Ordinal);
            return end > start
                ? result.Substring(start, end - start).Trim()
                : fallbackName?.Trim() ?? string.Empty;
        }
    }
}
