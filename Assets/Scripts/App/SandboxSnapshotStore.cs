// Folder: App - application orchestration and runtime state.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KineTutor3D.Types;
using KineTutor3D.Visualization;
using UnityEngine;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.App
{
    [Serializable]
    public sealed class SandboxSnapshotData
    {
        public string snapshotId;
        public string robotId;
        public string contextMode;
        public double[] jointValuesDeg = Array.Empty<double>();
        public Vector3 eePosition;
        public Vector3 eeEuler;
        public string note;
        public string createdAtUtc;
    }

    [Serializable]
    internal sealed class SandboxSnapshotFile
    {
        public List<SandboxSnapshotData> items = new List<SandboxSnapshotData>();
    }

    /// <summary>
    /// Sandbox snapshot lite를 JSON 파일로 저장합니다.
    /// </summary>
    public static class SandboxSnapshotStore
    {
        private const int MaxSnapshotsPerRobot = 10;
        private const string FileName = "sandbox-snapshots.json";

        public static SandboxSnapshotData SaveSnapshot(string robotId, string contextMode, double[] jointValuesDeg, TutorPose eePose, string note)
        {
            var file = LoadFile();
            var item = new SandboxSnapshotData
            {
                snapshotId = Guid.NewGuid().ToString("N"),
                robotId = robotId ?? string.Empty,
                contextMode = contextMode ?? string.Empty,
                jointValuesDeg = jointValuesDeg != null ? (double[])jointValuesDeg.Clone() : Array.Empty<double>(),
                eePosition = ToVector3(eePose.Position),
                eeEuler = CoordConverter.ToUnityRotation(eePose.Rotation).eulerAngles,
                note = string.IsNullOrWhiteSpace(note) ? $"{robotId} snapshot" : note.Trim(),
                createdAtUtc = DateTime.UtcNow.ToString("O")
            };

            file.items.Add(item);
            TrimSnapshots(file, robotId);
            SaveFile(file);
            return item;
        }

        public static SandboxSnapshotData GetLatestSnapshot(string robotId)
        {
            var file = LoadFile();
            return file.items
                .Where(x => string.Equals(x.robotId, robotId, StringComparison.Ordinal))
                .OrderByDescending(x => x.createdAtUtc)
                .FirstOrDefault();
        }

        public static SandboxSnapshotData[] GetSnapshots(string robotId)
        {
            var file = LoadFile();
            return file.items
                .Where(x => string.Equals(x.robotId, robotId, StringComparison.Ordinal))
                .OrderByDescending(x => x.createdAtUtc)
                .ToArray();
        }

        public static void Clear()
        {
            var path = GetFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void TrimSnapshots(SandboxSnapshotFile file, string robotId)
        {
            var keep = file.items
                .Where(x => string.Equals(x.robotId, robotId, StringComparison.Ordinal))
                .OrderByDescending(x => x.createdAtUtc)
                .Take(MaxSnapshotsPerRobot)
                .Select(x => x.snapshotId)
                .ToHashSet(StringComparer.Ordinal);

            file.items = file.items
                .Where(x => !string.Equals(x.robotId, robotId, StringComparison.Ordinal) || keep.Contains(x.snapshotId))
                .ToList();
        }

        private static SandboxSnapshotFile LoadFile()
        {
            var path = GetFilePath();
            if (!File.Exists(path))
            {
                return new SandboxSnapshotFile();
            }

            try
            {
                return JsonUtility.FromJson<SandboxSnapshotFile>(File.ReadAllText(path)) ?? new SandboxSnapshotFile();
            }
            catch
            {
                return new SandboxSnapshotFile();
            }
        }

        private static void SaveFile(SandboxSnapshotFile file)
        {
            var path = GetFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Application.persistentDataPath);
            File.WriteAllText(path, JsonUtility.ToJson(file, true));
        }

        private static string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        private static Vector3 ToVector3(Math.Vec3D value)
        {
            return new Vector3((float)value.X, (float)value.Y, (float)value.Z);
        }
    }
}
