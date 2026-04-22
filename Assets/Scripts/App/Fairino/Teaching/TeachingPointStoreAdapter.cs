// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3 포인트 저장소 이름과 로드/저장 정책을 한 곳에 모읍니다.
    /// </summary>
    public sealed class TeachingPointStoreAdapter
    {
        public const string DefaultSequenceName = "PendantV3Points";

        public WaypointSequence LoadOrCreate(string sequenceName = DefaultSequenceName)
        {
            return LoadIfExists(sequenceName) ?? WaypointStore.CreateEmpty(sequenceName);
        }

        public WaypointSequence LoadIfExists(string sequenceName = DefaultSequenceName)
        {
            var safeName = NormalizeSequenceName(sequenceName);
            var names = WaypointStore.LoadAllNames();
            for (var index = 0; index < names.Length; index++)
            {
                if (string.Equals(names[index], safeName, StringComparison.OrdinalIgnoreCase))
                {
                    return WaypointStore.Load(safeName);
                }
            }

            return null;
        }

        public bool Save(WaypointSequence sequence)
        {
            if (sequence == null)
            {
                return false;
            }

            sequence.name = NormalizeSequenceName(sequence.name);
            return WaypointStore.Save(sequence);
        }

        public string BuildSummary(string sequenceName = DefaultSequenceName)
        {
            var sequence = LoadIfExists(sequenceName);
            var waypoints = sequence?.waypoints ?? Array.Empty<Waypoint>();
            var names = new string[waypoints.Length];
            for (var index = 0; index < waypoints.Length; index++)
            {
                var point = waypoints[index];
                names[index] = point != null
                    ? $"{index}:{point.name}:{point.moveType}"
                    : $"{index}:null";
            }

            return $"sequence={NormalizeSequenceName(sequenceName)}; count={waypoints.Length}; points=[{string.Join(",", names)}]";
        }

        private static string NormalizeSequenceName(string sequenceName)
        {
            return string.IsNullOrWhiteSpace(sequenceName)
                ? DefaultSequenceName
                : sequenceName.Trim();
        }
    }
}
