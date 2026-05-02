// Folder: PointMoveController - selected point detail rendering and shared point/detail helper utilities.
using System.Collections.Generic;
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.UIElements;

namespace KineTutor3D.UI.RobotControlV3
{
    // Handles selected point detail rendering and the shared point/panel helper methods used by PointMove UI.
    // Point action modal behavior and bundle picker visibility stay in dedicated modal partials.
    public sealed partial class PointMoveController
    {
        private void ApplyPointDetail(PanelElements panel)
        {
            if (panel?.DetailTitle == null)
            {
                return;
            }

            if (recalledPoint == null)
            {
                panel.DetailTitle.text = "선택된 저장 위치 없음";
                panel.DetailMeta.text = "저장 위치를 선택하면 이동 방식과 저장된 속도/대기 시간이 보인다.";
                panel.DetailJoints.text = "J: -";
                panel.DetailTcp.text = "TCP: -";
                SetHidden(panel.PointEditActions, true);
                return;
            }

            SetHidden(panel.PointEditActions, false);
            panel.DetailTitle.text = recalledPoint.name;
            panel.DetailMeta.text = $"{ToMotionLabel(NormalizeMoveType(recalledPoint.moveType))} · {ToSpeedLabel(selectedSpeedPreset)} · 대기 {selectedDwellSec:0.0}초";
            panel.DetailJoints.text = $"J: {FormatVector(recalledPoint.jointsDeg, "0.0")}";
            panel.DetailTcp.text = $"TCP: {FormatTcp(recalledPoint.tcpMm)}";
            panel.BtnSpeedSlow?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "slow");
            panel.BtnSpeedMedium?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "medium");
            panel.BtnSpeedFast?.EnableInClassList("rc-point-timing-button--active", selectedSpeedPreset == "fast");
            panel.DwellInput?.SetValueWithoutNotify(selectedDwellSec.ToString("0.0", CultureInfo.InvariantCulture));
            panel.DwellInput?.EnableInClassList("rc-point-dwell-input--danger", isDwellInvalid);
        }

        private string BuildPointDetailDebugSummary()
        {
            if (recalledPoint == null)
            {
                return "detail=none";
            }

            return $"detail={recalledPoint.name}; moveType={NormalizeMoveType(recalledPoint.moveType)}; speed={NormalizeSpeedPreset(recalledPoint.speedPreset)}; dwell={recalledPoint.dwellSec:0.0}; joints=[{FormatVector(recalledPoint.jointsDeg, "0.0")}]; tcp=[{FormatTcp(recalledPoint.tcpMm)}]";
        }

        private bool HasNamedPoint(string pointName)
        {
            var sequence = LoadPointSequenceIfExists();
            return FindWaypointIndex(sequence, pointName) >= 0;
        }

        private bool HasAnyPoint()
        {
            var sequence = LoadPointSequenceIfExists();
            return sequence?.waypoints != null && sequence.waypoints.Length > 0;
        }

        private bool CanMoveSelectedPoint(int direction)
        {
            if (recalledPoint == null)
            {
                return false;
            }

            var sequence = LoadPointSequenceIfExists();
            var index = FindWaypointIndex(sequence, recalledPoint.name);
            var targetIndex = index + (direction < 0 ? -1 : 1);
            return index >= 0 && sequence?.waypoints != null && targetIndex >= 0 && targetIndex < sequence.waypoints.Length;
        }

        private static WaypointSequence LoadPointSequenceIfExists()
        {
            return LoadSequenceIfExists(PointSequenceName);
        }

        private static WaypointSequence LoadSequenceIfExists(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
            {
                return null;
            }

            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], sequenceName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return WaypointStore.Load(names[index]);
                }
            }

            return null;
        }

        private static bool HasNamedSequence(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName) != null;
        }

        private static int CountSequenceWaypoints(string sequenceName)
        {
            return LoadSequenceIfExists(sequenceName)?.waypoints?.Length ?? 0;
        }

        private void SetPointName(string pointName)
        {
            var safeName = pointName?.Trim() ?? string.Empty;
            desktopPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
            tabletPanel?.PointNameInput?.SetValueWithoutNotify(safeName);
        }

        private double[] ReadCurrentSnapshotJoints()
        {
            var values = runtimeController?.CurrentSnapshot.JointValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private double[] ReadCurrentSnapshotTcp()
        {
            var values = runtimeController?.CurrentSnapshot.TcpValues;
            var result = new double[6];
            for (var index = 0; index < result.Length; index++)
            {
                if (values == null || index >= values.Length || !double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result[index]))
                {
                    result[index] = 0.0;
                }
            }

            return result;
        }

        private static void ReplaceWaypoint(WaypointSequence sequence, Waypoint waypoint)
        {
            var waypoints = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            for (var index = 0; index < waypoints.Length; index++)
            {
                if (string.Equals(waypoints[index]?.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase))
                {
                    waypoints[index] = CloneWaypoint(waypoint);
                    sequence.waypoints = waypoints;
                    return;
                }
            }

            WaypointStore.AddWaypoint(sequence, CloneWaypoint(waypoint));
        }

        private static Waypoint FindWaypoint(WaypointSequence sequence, string pointName)
        {
            var index = FindWaypointIndex(sequence, pointName);
            return index >= 0 ? sequence.waypoints[index] : null;
        }

        private static int FindWaypointIndex(WaypointSequence sequence, string pointName)
        {
            if (sequence?.waypoints == null || string.IsNullOrWhiteSpace(pointName))
            {
                return -1;
            }

            var waypoints = sequence.waypoints;
            for (var index = 0; index < waypoints.Length; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint != null && string.Equals(waypoint.name, pointName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static Waypoint CloneWaypoint(Waypoint waypoint)
        {
            return new Waypoint
            {
                name = waypoint?.name ?? "Point",
                jointsDeg = waypoint?.jointsDeg != null ? (double[])waypoint.jointsDeg.Clone() : new double[6],
                tcpMm = waypoint?.tcpMm != null ? (double[])waypoint.tcpMm.Clone() : new double[6],
                moveType = waypoint?.moveType ?? "MoveJ",
                speedPreset = waypoint?.speedPreset ?? "medium",
                dwellSec = waypoint?.dwellSec ?? 0.0
            };
        }

        private static void InsertWaypointAfter(WaypointSequence sequence, Waypoint waypoint, int sourceIndex)
        {
            var existing = sequence.waypoints ?? System.Array.Empty<Waypoint>();
            var insertIndex = Mathf.Clamp(sourceIndex + 1, 0, existing.Length);
            var expanded = new Waypoint[existing.Length + 1];
            if (insertIndex > 0)
            {
                System.Array.Copy(existing, 0, expanded, 0, insertIndex);
            }

            expanded[insertIndex] = CloneWaypoint(waypoint);
            if (insertIndex < existing.Length)
            {
                System.Array.Copy(existing, insertIndex, expanded, insertIndex + 1, existing.Length - insertIndex);
            }

            sequence.waypoints = expanded;
        }

        private static string BuildUniqueDuplicateName(WaypointSequence sequence, string sourceName)
        {
            var safeSource = string.IsNullOrWhiteSpace(sourceName) ? "Point" : sourceName.Trim();
            var baseName = $"{safeSource}_COPY";
            var candidate = baseName;
            var suffix = 2;
            while (FindWaypointIndex(sequence, candidate) >= 0)
            {
                candidate = $"{baseName}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string FormatVector(double[] values, string format)
        {
            if (values == null || values.Length == 0)
            {
                return "-";
            }

            var count = System.Math.Min(values.Length, 6);
            var parts = new string[count];
            for (var index = 0; index < count; index++)
            {
                parts[index] = values[index].ToString(format, CultureInfo.InvariantCulture);
            }

            return string.Join(" / ", parts);
        }

        private static string FormatTcp(double[] values)
        {
            if (values == null || values.Length < 6)
            {
                return "-";
            }

            return $"X {values[0]:0.0} / Y {values[1]:0.0} / Z {values[2]:0.0} / RX {values[3]:0.0} / RY {values[4]:0.0} / RZ {values[5]:0.0}";
        }

        private static string NormalizeMoveType(string value)
        {
            return string.Equals(value, "MoveL", System.StringComparison.OrdinalIgnoreCase) ? "MoveL" : "MoveJ";
        }

        private static string ToMotionLabel(string moveType)
        {
            return string.Equals(moveType, "MoveL", System.StringComparison.OrdinalIgnoreCase)
                ? "직선 이동"
                : "관절 이동";
        }

        private static string ToSpeedLabel(string speedPreset)
        {
            return NormalizeSpeedPreset(speedPreset) switch
            {
                "slow" => "느림",
                "fast" => "빠름",
                _ => "중간",
            };
        }

        private static string NormalizeSpeedPreset(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "slow" => "slow",
                "fast" => "fast",
                _ => "medium",
            };
        }

        private static int AxisIndexFromLabel(string axisLabel)
        {
            return (axisLabel ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "X" => 0,
                "Y" => 1,
                "Z" => 2,
                "RX" => 3,
                "RY" => 4,
                _ => 5,
            };
        }

        private static string GetAxisLabel(int index)
        {
            return index switch
            {
                0 => "X",
                1 => "Y",
                2 => "Z",
                3 => "RX",
                4 => "RY",
                _ => "RZ",
            };
        }

        private PendantV3LocalState GetLocalState()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? shellState.GetStateSnapshot()
                : PendantV3LocalState.Normalize(LocalSettingsStore.LoadOrDefault());
        }

        private bool CanPreview()
        {
            if (connectionHomeController.CurrentPreviewState == PendantV3PreviewState.Kind.AutoReconnect)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private bool IsAnyPanelVisible() => isDesktopVisible || isTabletVisible;
        private PanelElements ResolveActivePanel() => isDesktopVisible || !isTabletVisible ? desktopPanel : tabletPanel;
        private bool IsMoveLDispatchMode() => motionKind == "MoveL";

        private bool CanApply()
        {
            if (connectionHomeController.CurrentPreviewState is PendantV3PreviewState.Kind.AutoReconnect or PendantV3PreviewState.Kind.Fault)
            {
                return false;
            }

            return connectionHomeController.CurrentPreviewState != PendantV3PreviewState.Kind.Disconnected
                || connectionHomeController.CurrentPreviewDefinition.DryRunEnabled;
        }

        private static float ParseValue(string rawValue)
        {
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0f;
        }

        private double[] currentValuesToDouble()
        {
            var result = new double[currentValues.Length];
            for (var i = 0; i < currentValues.Length; i++)
            {
                result[i] = currentValues[i];
            }

            return result;
        }
    }
}
