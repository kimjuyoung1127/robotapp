// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 부침개 조리 데모용 포인트, 묶음, 작업 시퀀스 프리셋을 생성합니다.
    /// </summary>
    public static class BuchimgaeCookingSequencePreset
    {
        public const string SetupBundleName = "BUCHIMGAE_01_COOK_SETUP";
        public const string BatterBundleName = "BUCHIMGAE_02_BATTER_SPREAD";
        public const string FirstCookBundleName = "BUCHIMGAE_03_FIRST_COOK";
        public const string FlipBundleName = "BUCHIMGAE_04_FLIP";
        public const string FinishBundleName = "BUCHIMGAE_05_FINISH";
        private static readonly string[] ObsoletePointNames = {"BCH_COOK_EDGE_CHECK"};

        public static BuchimgaeCookingSequenceManifest BuildManifest()
        {
            var points = new[]
            {
                Point("BCH_SETUP_SAFE_HOME", new[] {0.0, -45.0, 0.0, -60.0, -90.0, 0.0}, new[] {420.0, 0.0, 480.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.0),
                Point("BCH_SETUP_FETCH_INGREDIENTS", new[] {-28.0, -44.0, 18.0, -62.0, -88.0, 38.0}, new[] {360.0, 260.0, 255.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.5),
                Point("BCH_SETUP_RETURN_WITH_INGREDIENTS", new[] {-8.0, -42.0, 16.0, -61.0, -87.0, 18.0}, new[] {500.0, -120.0, 300.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.2),
                Point("BCH_SETUP_PAN_APPROACH", new[] {8.0, -42.0, 15.0, -64.0, -86.0, 18.0}, new[] {500.0, -120.0, 300.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.2),
                Point("BCH_SETUP_OIL_SPREAD_LEFT", new[] {12.0, -38.0, 18.0, -58.0, -84.0, 24.0}, new[] {465.0, -160.0, 155.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.4),
                Point("BCH_SETUP_OIL_SPREAD_RIGHT", new[] {4.0, -39.0, 16.0, -57.0, -85.0, -18.0}, new[] {545.0, -160.0, 155.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.4),
                Point("BCH_SETUP_LADLE_READY", new[] {-8.0, -44.0, 20.0, -62.0, -88.0, 32.0}, new[] {430.0, 130.0, 260.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.2),

                Point("BCH_BATTER_POUR_CENTER", new[] {-2.0, -40.0, 24.0, -60.0, -86.0, 0.0}, new[] {505.0, -150.0, 135.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 1.2),
                Point("BCH_BATTER_SPREAD_NORTH", new[] {-4.0, -38.0, 26.0, -61.0, -84.0, 8.0}, new[] {505.0, -205.0, 125.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.3),
                Point("BCH_BATTER_SPREAD_EAST", new[] {6.0, -38.0, 23.0, -60.0, -85.0, -12.0}, new[] {565.0, -150.0, 125.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.3),
                Point("BCH_BATTER_SPREAD_SOUTH", new[] {4.0, -39.0, 22.0, -59.0, -85.0, -2.0}, new[] {505.0, -95.0, 125.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.3),
                Point("BCH_BATTER_SPREAD_WEST", new[] {-9.0, -38.0, 23.0, -60.0, -85.0, 14.0}, new[] {445.0, -150.0, 125.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.3),
                Point("BCH_BATTER_LIFT_CLEAR", new[] {-4.0, -43.0, 18.0, -61.0, -88.0, 4.0}, new[] {505.0, -150.0, 230.0, 180.0, 0.0, 90.0}, "MoveL", "medium", 0.0),

                Point("BCH_COOK_WAIT_SIDE_A", new[] {-4.0, -43.0, 18.0, -61.0, -88.0, 4.0}, new[] {505.0, -150.0, 260.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 8.0),
                Point("BCH_COOK_FETCH_SPATULA", new[] {-35.0, -42.0, 18.0, -62.0, -88.0, 45.0}, new[] {345.0, -260.0, 235.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.5),
                Point("BCH_COOK_RETURN_WITH_SPATULA", new[] {-15.0, -42.0, 20.0, -62.0, -87.0, 28.0}, new[] {410.0, -210.0, 210.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.2),
                Point("BCH_COOK_SPATULA_READY", new[] {-15.0, -42.0, 20.0, -62.0, -87.0, 28.0}, new[] {410.0, -210.0, 210.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.2),

                Point("BCH_FLIP_INSERT", new[] {-11.0, -39.0, 27.0, -58.0, -84.0, 18.0}, new[] {435.0, -188.0, 118.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.4),
                Point("BCH_FLIP_LIFT", new[] {-6.0, -42.0, 25.0, -63.0, -86.0, 32.0}, new[] {500.0, -150.0, 215.0, 180.0, 0.0, 135.0}, "MoveL", "slow", 0.3),
                Point("BCH_FLIP_TURN_OVER", new[] {2.0, -40.0, 24.0, -64.0, -83.0, 178.0}, new[] {515.0, -150.0, 210.0, 180.0, 0.0, 270.0}, "MoveJ", "slow", 0.5),
                Point("BCH_FLIP_RELEASE_SIDE_B", new[] {4.0, -38.0, 24.0, -58.0, -84.0, 172.0}, new[] {510.0, -150.0, 128.0, 180.0, 0.0, 270.0}, "MoveL", "slow", 0.4),
                Point("BCH_FLIP_LIFT_CLEAR", new[] {2.0, -43.0, 18.0, -62.0, -88.0, 120.0}, new[] {510.0, -150.0, 245.0, 180.0, 0.0, 180.0}, "MoveL", "medium", 0.1),

                Point("BCH_FINISH_WAIT_SIDE_B", new[] {0.0, -43.0, 18.0, -61.0, -88.0, 0.0}, new[] {510.0, -150.0, 265.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 6.0),
                Point("BCH_FINISH_REINSERT_UNDER", new[] {-8.0, -39.0, 27.0, -58.0, -84.0, 18.0}, new[] {435.0, -188.0, 118.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.4),
                Point("BCH_FINISH_LIFT_FROM_PAN", new[] {2.0, -42.0, 25.0, -63.0, -86.0, 28.0}, new[] {510.0, -150.0, 220.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.3),
                Point("BCH_FINISH_PLATE_APPROACH", new[] {20.0, -40.0, 18.0, -64.0, -86.0, -35.0}, new[] {585.0, 95.0, 220.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.2),
                Point("BCH_FINISH_PLATE_PLACE", new[] {23.0, -38.0, 21.0, -58.0, -84.0, -30.0}, new[] {585.0, 95.0, 120.0, 180.0, 0.0, 90.0}, "MoveL", "slow", 0.8),
                Point("BCH_FINISH_SAFE_HOME", new[] {0.0, -45.0, 0.0, -60.0, -90.0, 0.0}, new[] {420.0, 0.0, 480.0, 180.0, 0.0, 90.0}, "MoveJ", "medium", 0.0),
            };

            var bundles = new[]
            {
                Bundle(SetupBundleName, "Cooking setup: safe home, fetch ingredients, pan check, oil spread, ladle ready.", "BCH_SETUP_SAFE_HOME", "BCH_SETUP_FETCH_INGREDIENTS", "BCH_SETUP_RETURN_WITH_INGREDIENTS", "BCH_SETUP_PAN_APPROACH", "BCH_SETUP_OIL_SPREAD_LEFT", "BCH_SETUP_OIL_SPREAD_RIGHT", "BCH_SETUP_LADLE_READY"),
                Bundle(BatterBundleName, "Pour batter and spread it into a round surface path.", "BCH_BATTER_POUR_CENTER", "BCH_BATTER_SPREAD_NORTH", "BCH_BATTER_SPREAD_EAST", "BCH_BATTER_SPREAD_SOUTH", "BCH_BATTER_SPREAD_WEST", "BCH_BATTER_LIFT_CLEAR"),
                Bundle(FirstCookBundleName, "Cook the first side, fetch the spatula, and return to the pan.", "BCH_COOK_WAIT_SIDE_A", "BCH_COOK_FETCH_SPATULA", "BCH_COOK_RETURN_WITH_SPATULA", "BCH_COOK_SPATULA_READY"),
                Bundle(FlipBundleName, "Insert, lift, rotate, release, and clear after the flip motion.", "BCH_FLIP_INSERT", "BCH_FLIP_LIFT", "BCH_FLIP_TURN_OVER", "BCH_FLIP_RELEASE_SIDE_B", "BCH_FLIP_LIFT_CLEAR"),
                Bundle(FinishBundleName, "Finish the second side, reinsert under the pancake, lift, transfer slowly to the plate, and return home.", "BCH_FINISH_WAIT_SIDE_B", "BCH_FINISH_REINSERT_UNDER", "BCH_FINISH_LIFT_FROM_PAN", "BCH_FINISH_PLATE_APPROACH", "BCH_FINISH_PLATE_PLACE", "BCH_FINISH_SAFE_HOME"),
            };

            return new BuchimgaeCookingSequenceManifest(points, bundles, BuildBundleBlocks(bundles));
        }

        public static string Apply()
        {
            var manifest = BuildManifest();
            var pointStore = new TeachingPointStoreAdapter();
            var pointSequence = pointStore.LoadOrCreate();
            RemoveObsoletePresetWaypoints(pointSequence);
            UpsertWaypoints(pointSequence, manifest.Points);
            pointStore.Save(pointSequence);

            var functionStore = new TeachingFunctionStore();
            for (var index = 0; index < manifest.Bundles.Length; index++)
            {
                functionStore.Save(manifest.Bundles[index]);
            }

            var blockStore = new TeachingBlockSequenceStore();
            var blockSequence = blockStore.LoadOrCreate();
            blockSequence.blocks = AppendIdempotentBlocks(blockSequence.blocks, manifest.Blocks);
            blockStore.Save(blockSequence);

            return $"buchimgaePreset=applied; points={manifest.Points.Length}; bundles={manifest.Bundles.Length}; blockSequence={blockSequence.name}; blocks={blockSequence.blocks.Length}; expanded={manifest.Points.Length}";
        }

        private static Waypoint Point(string name, double[] jointsDeg, double[] tcpMm, string moveType, string speedPreset, double dwellSec)
        {
            return new Waypoint
            {
                name = name,
                jointsDeg = jointsDeg,
                tcpMm = tcpMm,
                moveType = moveType,
                speedPreset = speedPreset,
                dwellSec = dwellSec
            };
        }

        private static TeachingFunction Bundle(string name, string description, params string[] pointNames)
        {
            var steps = new TeachingFunctionStep[pointNames.Length];
            for (var index = 0; index < pointNames.Length; index++)
            {
                steps[index] = new TeachingFunctionStep
                {
                    kind = "PointRef",
                    refName = pointNames[index],
                    enabled = true,
                    note = string.Empty
                };
            }

            var now = DateTime.Now.ToString("O");
            return new TeachingFunction
            {
                name = name,
                description = description,
                steps = steps,
                created = now,
                updated = now
            };
        }

        private static TeachingSequenceBlock[] BuildBundleBlocks(TeachingFunction[] bundles)
        {
            var blocks = new TeachingSequenceBlock[bundles.Length];
            for (var index = 0; index < bundles.Length; index++)
            {
                blocks[index] = new TeachingSequenceBlock
                {
                    kind = TeachingSequenceBlock.BundleRefKind,
                    refName = bundles[index].name,
                    enabled = true
                };
            }

            return blocks;
        }

        private static void UpsertWaypoints(WaypointSequence sequence, Waypoint[] points)
        {
            var merged = new List<Waypoint>(sequence.waypoints ?? Array.Empty<Waypoint>());
            for (var index = 0; index < points.Length; index++)
            {
                var point = points[index];
                var existingIndex = FindWaypointIndex(merged, point.name);
                if (existingIndex >= 0)
                {
                    merged[existingIndex] = CloneWaypoint(point);
                    continue;
                }

                merged.Add(CloneWaypoint(point));
            }

            sequence.waypoints = merged.ToArray();
        }

        private static void RemoveObsoletePresetWaypoints(WaypointSequence sequence)
        {
            var existing = sequence.waypoints ?? Array.Empty<Waypoint>();
            if (existing.Length == 0)
            {
                return;
            }

            var obsolete = new HashSet<string>(ObsoletePointNames, StringComparer.OrdinalIgnoreCase);
            var filtered = new List<Waypoint>();
            for (var index = 0; index < existing.Length; index++)
            {
                var waypoint = existing[index];
                if (waypoint == null || obsolete.Contains(waypoint.name ?? string.Empty))
                {
                    continue;
                }

                filtered.Add(waypoint);
            }

            sequence.waypoints = filtered.ToArray();
        }

        private static TeachingSequenceBlock[] AppendIdempotentBlocks(TeachingSequenceBlock[] existing, TeachingSequenceBlock[] blocksToAppend)
        {
            var merged = new List<TeachingSequenceBlock>();
            var presetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < blocksToAppend.Length; index++)
            {
                presetNames.Add(blocksToAppend[index].refName);
            }

            var safeExisting = existing ?? Array.Empty<TeachingSequenceBlock>();
            for (var index = 0; index < safeExisting.Length; index++)
            {
                var block = safeExisting[index];
                if (block == null
                    || string.Equals(block.kind, TeachingSequenceBlock.BundleRefKind, StringComparison.OrdinalIgnoreCase)
                    && presetNames.Contains(block.refName ?? string.Empty))
                {
                    continue;
                }

                merged.Add(CloneBlock(block));
            }

            for (var index = 0; index < blocksToAppend.Length; index++)
            {
                merged.Add(CloneBlock(blocksToAppend[index]));
            }

            return merged.ToArray();
        }

        private static int FindWaypointIndex(List<Waypoint> waypoints, string pointName)
        {
            for (var index = 0; index < waypoints.Count; index++)
            {
                if (string.Equals(waypoints[index]?.name, pointName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
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

        private static TeachingSequenceBlock CloneBlock(TeachingSequenceBlock block)
        {
            return new TeachingSequenceBlock
            {
                kind = string.Equals(block?.kind, TeachingSequenceBlock.BundleRefKind, StringComparison.OrdinalIgnoreCase)
                    ? TeachingSequenceBlock.BundleRefKind
                    : TeachingSequenceBlock.PointRefKind,
                refName = block?.refName ?? string.Empty,
                enabled = block == null || block.enabled
            };
        }
    }

    public sealed class BuchimgaeCookingSequenceManifest
    {
        public BuchimgaeCookingSequenceManifest(
            Waypoint[] points,
            TeachingFunction[] bundles,
            TeachingSequenceBlock[] blocks)
        {
            Points = points ?? Array.Empty<Waypoint>();
            Bundles = bundles ?? Array.Empty<TeachingFunction>();
            Blocks = blocks ?? Array.Empty<TeachingSequenceBlock>();
        }

        public Waypoint[] Points { get; }
        public TeachingFunction[] Bundles { get; }
        public TeachingSequenceBlock[] Blocks { get; }
    }
}
