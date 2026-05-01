// Folder: PointMoveController - function panel summaries, detail text, list rebuild, and loop/timing view state.
// Serves inventory/detail rendering for the function subview and related textual summaries.
// Builder, selection, and mutation flows live in sibling partials.
using System.Globalization;

namespace KineTutor3D.UI.RobotControlV3
{
    public sealed partial class PointMoveController
    {
        private void ApplyFunctionPanel(PanelElements panel)
        {
            if (panel?.FunctionSummary == null)
            {
                return;
            }

            SelectFirstExistingFunctionIfNeeded();
            var functionNames = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            panel.FunctionSummary.text = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? $"작업 묶음 {functionNames.Length}개"
                : $"작업 묶음 {functionNames.Length}개 · 선택 {ShortDisplayName(selectedFunctionName)}";
            if (panel.FunctionInventorySummary != null)
            {
                panel.FunctionInventorySummary.text = BuildFunctionInventorySummary(functionNames);
            }

            panel.FunctionDetail.text = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? FormatFunctionDetailForUi(runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName))
                : "묶음을 선택하면 포함된 저장 위치가 보인다.";
            RebuildFunctionList(panel);
        }

        private static string FormatFunctionDetailForUi(string rawDetail)
        {
            if (string.IsNullOrWhiteSpace(rawDetail) || rawDetail.Contains("function=none"))
            {
                return "묶음을 선택하면 포함된 저장 위치가 보인다.";
            }

            var name = ExtractDebugValue(rawDetail, "function=");
            var steps = ExtractDebugValue(rawDetail, "steps=");
            var missingCount = ExtractDebugValue(rawDetail, "missingCount=");
            var missing = ExtractDebugBracketValue(rawDetail, "missing=[");
            return missingCount == "0" || string.IsNullOrWhiteSpace(missingCount)
                ? $"{name} · {steps}개 위치 · 누락 없음"
                : $"{name} · {steps}개 위치 · 누락 {missingCount}: {missing}";
        }

        private string BuildFunctionInventorySummary(string[] functionNames)
        {
            functionNames ??= System.Array.Empty<string>();
            var selectedDetail = !string.IsNullOrWhiteSpace(selectedFunctionName) && runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(selectedFunctionName)
                : string.Empty;
            var selectedSteps = ExtractDebugValue(selectedDetail, "steps=");
            var selectedMissing = ExtractDebugValue(selectedDetail, "missingCount=");
            var detail = string.IsNullOrWhiteSpace(selectedFunctionName)
                ? "선택 없음"
                : $"{ShortDisplayName(selectedFunctionName)} 포함 {selectedSteps}개 · 누락 {selectedMissing}";
            return $"작업 묶음 {functionNames.Length}개 · 선택 {selectedFunctionNames.Count}개 · {detail}";
        }

        private static string FormatPathRecordSummary(string rawSummary)
        {
            if (string.IsNullOrWhiteSpace(rawSummary))
            {
                return "기록: 대기 / 샘플 0개";
            }

            var recording = ExtractDebugValue(rawSummary, "recording=");
            var samples = ExtractDebugValue(rawSummary, "samples=");
            var saved = ExtractDebugValue(rawSummary, "saved=");
            var runner = ExtractDebugValue(rawSummary, "runner=");
            var state = string.Equals(recording, "True", System.StringComparison.OrdinalIgnoreCase)
                ? "기록 중"
                : "대기";
            return $"기록: {state} / 샘플 {samples}개 / 저장 {saved}개 / 재생 {runner}";
        }

        private static string ExtractDebugValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(';', start);
            return end > start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private static string ExtractDebugBracketValue(string raw, string key)
        {
            var start = raw.IndexOf(key, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            start += key.Length;
            var end = raw.IndexOf(']', start);
            return end >= start
                ? raw.Substring(start, end - start).Trim()
                : raw.Substring(start).Trim();
        }

        private void RebuildFunctionList(PanelElements panel)
        {
            if (panel?.FunctionListView == null)
            {
                return;
            }

            functionListItems.Clear();
            var names = runtimeController != null
                ? runtimeController.GetTeachingFunctionNames()
                : System.Array.Empty<string>();
            for (var index = 0; index < names.Length; index++)
            {
                var functionName = names[index];
                if (ShouldShowFunctionRow(functionName))
                {
                    functionListItems.Add(functionName);
                }
            }

            RefreshListView(panel.FunctionListView);
        }

        private string BuildFunctionRowSummary(string functionName)
        {
            var detail = runtimeController != null
                ? runtimeController.GetTeachingFunctionDetailForDebug(functionName)
                : string.Empty;
            var steps = ExtractDebugValue(detail, "steps=");
            var missingCount = ExtractDebugValue(detail, "missingCount=");
            var missing = string.IsNullOrWhiteSpace(missingCount) || missingCount == "0"
                ? "누락 없음"
                : $"누락 {missingCount}";
            return $"{ShortDisplayName(functionName)} · {steps}개 · {missing}";
        }

        private void HandleDwellChanged(string rawValue)
        {
            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed)
                || parsed < 0.0
                || parsed > 600.0)
            {
                isDwellInvalid = true;
                ApplyPanel(desktopPanel);
                ApplyPanel(tabletPanel);
                return;
            }

            selectedDwellSec = parsed;
            isDwellInvalid = false;
            ClearPendingConfirmation();
            ApplyPanel(desktopPanel);
            ApplyPanel(tabletPanel);
        }
    }
}
