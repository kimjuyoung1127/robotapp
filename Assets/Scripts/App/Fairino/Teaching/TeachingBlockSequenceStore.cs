// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.IO;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    [Serializable]
    public sealed class TeachingSequenceBlock
    {
        public string kind = PointRefKind;
        public string refName;
        public bool enabled = true;

        public const string PointRefKind = "PointRef";
        public const string BundleRefKind = "BundleRef";
    }

    [Serializable]
    public sealed class TeachingBlockSequence
    {
        public string name;
        public string updated;
        public TeachingSequenceBlock[] blocks = Array.Empty<TeachingSequenceBlock>();
    }

    public sealed class TeachingBlockSequenceStore
    {
        public const string DefaultSequenceName = "PendantV3Blocks";
        private const string SubFolder = "teaching-block-sequences";

        public TeachingBlockSequence LoadOrCreate(string sequenceName = DefaultSequenceName)
        {
            return Load(sequenceName) ?? new TeachingBlockSequence
            {
                name = NormalizeName(sequenceName),
                updated = DateTime.Now.ToString("O"),
                blocks = Array.Empty<TeachingSequenceBlock>()
            };
        }

        public TeachingBlockSequence Load(string sequenceName = DefaultSequenceName)
        {
            var path = BuildPath(sequenceName);
            if (!File.Exists(path))
            {
                return null;
            }

            return JsonUtility.FromJson<TeachingBlockSequence>(File.ReadAllText(path));
        }

        public bool Save(TeachingBlockSequence sequence)
        {
            if (sequence == null || string.IsNullOrWhiteSpace(sequence.name))
            {
                return false;
            }

            var dir = GetStorageDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            sequence.name = NormalizeName(sequence.name);
            sequence.updated = DateTime.Now.ToString("O");
            File.WriteAllText(BuildPath(sequence.name), JsonUtility.ToJson(sequence, true));
            return true;
        }

        public bool AddBlock(string kind, string refName)
        {
            if (string.IsNullOrWhiteSpace(refName))
            {
                return false;
            }

            var sequence = LoadOrCreate();
            var existing = sequence.blocks ?? Array.Empty<TeachingSequenceBlock>();
            var next = new TeachingSequenceBlock[existing.Length + 1];
            Array.Copy(existing, next, existing.Length);
            next[existing.Length] = new TeachingSequenceBlock
            {
                kind = NormalizeKind(kind),
                refName = refName.Trim(),
                enabled = true
            };
            sequence.blocks = next;
            return Save(sequence);
        }

        public bool MoveBlock(int index, int direction)
        {
            var sequence = LoadOrCreate();
            var blocks = sequence.blocks ?? Array.Empty<TeachingSequenceBlock>();
            var target = index + (direction < 0 ? -1 : 1);
            if (index < 0 || index >= blocks.Length || target < 0 || target >= blocks.Length)
            {
                return false;
            }

            (blocks[index], blocks[target]) = (blocks[target], blocks[index]);
            sequence.blocks = blocks;
            return Save(sequence);
        }

        public bool DeleteBlock(int index)
        {
            var sequence = LoadOrCreate();
            var blocks = sequence.blocks ?? Array.Empty<TeachingSequenceBlock>();
            if (index < 0 || index >= blocks.Length)
            {
                return false;
            }

            var next = new TeachingSequenceBlock[blocks.Length - 1];
            if (index > 0)
            {
                Array.Copy(blocks, 0, next, 0, index);
            }

            if (index < blocks.Length - 1)
            {
                Array.Copy(blocks, index + 1, next, index, blocks.Length - index - 1);
            }

            sequence.blocks = next;
            return Save(sequence);
        }

        public bool Clear()
        {
            var sequence = LoadOrCreate();
            sequence.blocks = Array.Empty<TeachingSequenceBlock>();
            return Save(sequence);
        }

        public string BuildSummary()
        {
            var sequence = LoadOrCreate();
            var blocks = sequence.blocks ?? Array.Empty<TeachingSequenceBlock>();
            var parts = new string[blocks.Length];
            for (var index = 0; index < blocks.Length; index++)
            {
                var block = blocks[index];
                parts[index] = $"{index}:{NormalizeKind(block?.kind)}:{block?.refName}:{block?.enabled}";
            }

            return $"blockSequence={sequence.name}; blocks={blocks.Length}; list=[{string.Join(",", parts)}]";
        }

        private static string NormalizeKind(string kind)
        {
            return string.Equals(kind, TeachingSequenceBlock.BundleRefKind, StringComparison.OrdinalIgnoreCase)
                ? TeachingSequenceBlock.BundleRefKind
                : TeachingSequenceBlock.PointRefKind;
        }

        private static string NormalizeName(string sequenceName)
        {
            return string.IsNullOrWhiteSpace(sequenceName)
                ? DefaultSequenceName
                : sequenceName.Trim();
        }

        private static string BuildPath(string sequenceName)
        {
            return Path.Combine(GetStorageDirectory(), NormalizeName(sequenceName) + ".json");
        }

        private static string GetStorageDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SubFolder);
        }
    }
}
