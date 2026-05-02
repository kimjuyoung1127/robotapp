// Folder: PointMove - Home↔Point1 loop sequence building and waypoint equivalence helpers.
using System;

namespace KineTutor3D.App.Fairino
{
    // Builds and refreshes reusable Home/Point1 loop sequences from teaching points.
    // Named sequence execution and mixed-live runtime stay in separate PointMove partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string EnsureHomePoint1LoopSequenceForProduct()
        {
            var sequence = BuildOrRefreshHomePoint1LoopSequence();
            return sequence?.waypoints != null && sequence.waypoints.Length >= 2
                ? $"{sequence.name}:{sequence.waypoints.Length}"
                : string.Empty;
        }


        private bool CanBuildHomePoint1LoopSequence(out WaypointSequence sequence, out string resolvedSequenceName, out string message)
        {
            resolvedSequenceName = HomePoint1LoopSequenceName;
            sequence = BuildOrRefreshHomePoint1LoopSequence();
            if (sequence?.waypoints == null || sequence.waypoints.Length < 2)
            {
                message = $"[Sequence] {HomePoint1LoopSequenceName} 준비 실패 · Home와 {DefaultPoint1Name} 저장 포인트를 확인해라.";
                return false;
            }

            message = string.Empty;
            return true;
        }


        private WaypointSequence BuildOrRefreshHomePoint1LoopSequence()
        {
            teachingPointStoreAdapter ??= new TeachingPointStoreAdapter();
            var teachingSequence = teachingPointStoreAdapter.LoadOrCreate();
            if (teachingSequence == null)
            {
                return null;
            }

            var homePoint = FindWaypoint(teachingSequence, DefaultHomePointName);
            if (homePoint == null)
            {
                homePoint = TryCaptureHomeWaypoint(teachingSequence);
            }

            var point1 = FindWaypoint(teachingSequence, DefaultPoint1Name);
            if (homePoint == null || point1 == null)
            {
                return null;
            }

            var existingSequence = WaypointStore.Load(HomePoint1LoopSequenceName);
            var sequence = new WaypointSequence
            {
                name = HomePoint1LoopSequenceName,
                created = string.IsNullOrWhiteSpace(existingSequence?.created)
                    ? DateTime.UtcNow.ToString("O")
                    : existingSequence.created,
                waypoints = new[]
                {
                    CloneWaypointForSequence(homePoint, DefaultHomePointName),
                    CloneWaypointForSequence(point1, DefaultPoint1Name),
                },
            };
            if (AreWaypointSequencesEquivalent(existingSequence, sequence))
            {
                return existingSequence;
            }

            WaypointStore.Save(sequence);
            return sequence;
        }


        private Waypoint TryCaptureHomeWaypoint(WaypointSequence teachingSequence)
        {
            if (!hasCurrentPositionReadComplete
                || currentState.JointPosDeg == null
                || currentState.JointPosDeg.Length < 6
                || currentState.TcpPose == null
                || currentState.TcpPose.Length < 6)
            {
                return null;
            }

            var capturedHome = new Waypoint
            {
                name = DefaultHomePointName,
                jointsDeg = CopyJointArray(currentState.JointPosDeg),
                tcpMm = CopyPoseArray(currentState.TcpPose),
                moveType = "MoveJ",
                speedPreset = "medium",
                dwellSec = 0d,
            };

            var existing = teachingSequence.waypoints ?? Array.Empty<Waypoint>();
            var expanded = new Waypoint[existing.Length + 1];
            Array.Copy(existing, expanded, existing.Length);
            expanded[existing.Length] = capturedHome;
            teachingSequence.waypoints = expanded;
            teachingPointStoreAdapter?.Save(teachingSequence);
            return capturedHome;
        }


        private static Waypoint CloneWaypointForSequence(Waypoint waypoint, string fallbackName)
        {
            return new Waypoint
            {
                name = string.IsNullOrWhiteSpace(waypoint?.name) ? fallbackName : waypoint.name.Trim(),
                jointsDeg = waypoint?.jointsDeg != null ? (double[])waypoint.jointsDeg.Clone() : new double[6],
                tcpMm = waypoint?.tcpMm != null ? (double[])waypoint.tcpMm.Clone() : new double[6],
                moveType = string.Equals(waypoint?.moveType, "MoveL", StringComparison.OrdinalIgnoreCase) ? "MoveL" : "MoveJ",
                speedPreset = string.IsNullOrWhiteSpace(waypoint?.speedPreset) ? "medium" : waypoint.speedPreset,
                dwellSec = waypoint?.dwellSec ?? 0d,
            };
        }


        private static bool AreWaypointSequencesEquivalent(WaypointSequence left, WaypointSequence right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            if (!string.Equals(left.name, right.name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var leftWaypoints = left.waypoints ?? Array.Empty<Waypoint>();
            var rightWaypoints = right.waypoints ?? Array.Empty<Waypoint>();
            if (leftWaypoints.Length != rightWaypoints.Length)
            {
                return false;
            }

            for (var index = 0; index < leftWaypoints.Length; index++)
            {
                if (!AreWaypointsEquivalent(leftWaypoints[index], rightWaypoints[index]))
                {
                    return false;
                }
            }

            return true;
        }


        private static bool AreWaypointsEquivalent(Waypoint left, Waypoint right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return string.Equals(left.name, right.name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.moveType, right.moveType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.speedPreset, right.speedPreset, StringComparison.OrdinalIgnoreCase)
                && System.Math.Abs(left.dwellSec - right.dwellSec) < 0.0001d
                && AreDoubleArraysEquivalent(left.jointsDeg, right.jointsDeg)
                && AreDoubleArraysEquivalent(left.tcpMm, right.tcpMm);
        }


        private static bool AreDoubleArraysEquivalent(double[] left, double[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (System.Math.Abs(left[index] - right[index]) >= 0.0001d)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
