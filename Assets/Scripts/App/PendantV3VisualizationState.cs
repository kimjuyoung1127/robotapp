// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;

namespace KineTutor3D.App
{
    /// <summary>
    /// Pendant V3가 소비하는 3D 표시 상태 스냅샷입니다.
    /// </summary>
    public readonly struct PendantV3VisualizationState : IEquatable<PendantV3VisualizationState>
    {
        public enum PreviewKind
        {
            None,
            JointGhost,
            TcpTarget,
            MoveJGhost,
            MoveLTarget,
        }

        public PendantV3VisualizationState(
            PreviewKind previewKind,
            bool showBaseFrame,
            bool showToolFrame,
            bool showTrail,
            bool showGhost,
            bool showWorkspaceBoundary,
            bool showCollision,
            bool collisionAuto,
            bool liveArmActive,
            bool actualMoveAllowed,
            int activeJointIndex,
            int activeTcpAxisIndex,
            int activeTcpDirection,
            string coordSystem,
            string previewLabel,
            string riskSummary,
            double[] currentJointAnglesDeg,
            double[] ghostJointAnglesDeg,
            double[] currentTcpPose,
            double[] targetTcpPose)
        {
            CurrentPreviewKind = previewKind;
            ShowBaseFrame = showBaseFrame;
            ShowToolFrame = showToolFrame;
            ShowTrail = showTrail;
            ShowGhost = showGhost;
            ShowWorkspaceBoundary = showWorkspaceBoundary;
            ShowCollision = showCollision;
            CollisionAuto = collisionAuto;
            LiveArmActive = liveArmActive;
            ActualMoveAllowed = actualMoveAllowed;
            ActiveJointIndex = activeJointIndex;
            ActiveTcpAxisIndex = activeTcpAxisIndex;
            ActiveTcpDirection = activeTcpDirection;
            CoordSystem = coordSystem ?? "Base";
            PreviewLabel = previewLabel ?? string.Empty;
            RiskSummary = riskSummary ?? string.Empty;
            CurrentJointAnglesDeg = currentJointAnglesDeg != null ? (double[])currentJointAnglesDeg.Clone() : Array.Empty<double>();
            GhostJointAnglesDeg = ghostJointAnglesDeg != null ? (double[])ghostJointAnglesDeg.Clone() : Array.Empty<double>();
            CurrentTcpPose = currentTcpPose != null ? (double[])currentTcpPose.Clone() : Array.Empty<double>();
            TargetTcpPose = targetTcpPose != null ? (double[])targetTcpPose.Clone() : Array.Empty<double>();
        }

        public PreviewKind CurrentPreviewKind { get; }
        public bool ShowBaseFrame { get; }
        public bool ShowToolFrame { get; }
        public bool ShowTrail { get; }
        public bool ShowGhost { get; }
        public bool ShowWorkspaceBoundary { get; }
        public bool ShowCollision { get; }
        public bool CollisionAuto { get; }
        public bool LiveArmActive { get; }
        public bool ActualMoveAllowed { get; }
        public int ActiveJointIndex { get; }
        public int ActiveTcpAxisIndex { get; }
        public int ActiveTcpDirection { get; }
        public string CoordSystem { get; }
        public string PreviewLabel { get; }
        public string RiskSummary { get; }
        public double[] CurrentJointAnglesDeg { get; }
        public double[] GhostJointAnglesDeg { get; }
        public double[] CurrentTcpPose { get; }
        public double[] TargetTcpPose { get; }

        public string ToDebugSummary()
        {
            return $"kind={CurrentPreviewKind}; base={ShowBaseFrame}; tool={ShowToolFrame}; trail={ShowTrail}; ghost={ShowGhost}; boundary={ShowWorkspaceBoundary}; collision={ShowCollision}; collisionAuto={CollisionAuto}; liveArm={LiveArmActive}; actualMove={ActualMoveAllowed}; joint={ActiveJointIndex}; tcpAxis={ActiveTcpAxisIndex}; tcpDir={ActiveTcpDirection}; coord={CoordSystem}; label={PreviewLabel}; risk={RiskSummary}";
        }

        public bool Equals(PendantV3VisualizationState other)
        {
            return CurrentPreviewKind == other.CurrentPreviewKind
                && ShowBaseFrame == other.ShowBaseFrame
                && ShowToolFrame == other.ShowToolFrame
                && ShowTrail == other.ShowTrail
                && ShowGhost == other.ShowGhost
                && ShowWorkspaceBoundary == other.ShowWorkspaceBoundary
                && ShowCollision == other.ShowCollision
                && CollisionAuto == other.CollisionAuto
                && LiveArmActive == other.LiveArmActive
                && ActualMoveAllowed == other.ActualMoveAllowed
                && ActiveJointIndex == other.ActiveJointIndex
                && ActiveTcpAxisIndex == other.ActiveTcpAxisIndex
                && ActiveTcpDirection == other.ActiveTcpDirection
                && string.Equals(CoordSystem, other.CoordSystem, StringComparison.Ordinal)
                && string.Equals(PreviewLabel, other.PreviewLabel, StringComparison.Ordinal)
                && string.Equals(RiskSummary, other.RiskSummary, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is PendantV3VisualizationState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(CurrentPreviewKind);
            hash.Add(ShowBaseFrame);
            hash.Add(ShowToolFrame);
            hash.Add(ShowTrail);
            hash.Add(ShowGhost);
            hash.Add(ShowWorkspaceBoundary);
            hash.Add(ShowCollision);
            hash.Add(CollisionAuto);
            hash.Add(LiveArmActive);
            hash.Add(ActualMoveAllowed);
            hash.Add(ActiveJointIndex);
            hash.Add(ActiveTcpAxisIndex);
            hash.Add(ActiveTcpDirection);
            hash.Add(CoordSystem);
            hash.Add(PreviewLabel);
            hash.Add(RiskSummary);
            return hash.ToHashCode();
        }

        public static PendantV3VisualizationState Default()
        {
            return new PendantV3VisualizationState(
                PreviewKind.None,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                -1,
                -1,
                0,
                "Base",
                string.Empty,
                "안전",
                Array.Empty<double>(),
                Array.Empty<double>(),
                Array.Empty<double>(),
                Array.Empty<double>());
        }
    }
}
