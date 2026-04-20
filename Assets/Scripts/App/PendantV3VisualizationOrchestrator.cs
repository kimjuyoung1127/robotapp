// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.App.Fairino;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// Pendant V3 UI intent를 3D 표시 상태로 합성합니다.
    /// </summary>
    [DefaultExecutionOrder(-915)]
    [RequireComponent(typeof(PendantV3ConnectionSessionAdapter))]
    public sealed class PendantV3VisualizationOrchestrator : MonoBehaviour
    {
        [SerializeField] private PendantV3ConnectionSessionAdapter connectionSessionAdapter;

        private readonly double[] currentJointAnglesDeg = new double[6];
        private readonly double[] ghostJointAnglesDeg = new double[6];
        private readonly double[] currentTcpPose = new double[6];
        private readonly double[] targetTcpPose = new double[6];
        private PendantV3VisualizationState.PreviewKind previewKind;
        private bool showBaseFrame = true;
        private bool showToolFrame = true;
        private bool showTrail = true;
        private bool showGhost;
        private bool showWorkspaceBoundary;
        private bool showCollisionManual;
        private int activeJointIndex = -1;
        private int activeTcpAxisIndex = -1;
        private int activeTcpDirection;
        private string coordSystem = "Base";
        private string previewLabel = string.Empty;
        private string riskSummary = "안전";

        public event Action<PendantV3VisualizationState> StateChanged;

        public PendantV3VisualizationState CurrentState { get; private set; } = PendantV3VisualizationState.Default();

        private void OnEnable()
        {
            connectionSessionAdapter ??= GetComponent<PendantV3ConnectionSessionAdapter>();
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.StateChanged -= HandleSessionStateChanged;
                connectionSessionAdapter.StateChanged += HandleSessionStateChanged;
            }

            RefreshFromSession();
        }

        private void OnDisable()
        {
            if (connectionSessionAdapter != null)
            {
                connectionSessionAdapter.StateChanged -= HandleSessionStateChanged;
            }
        }

        public bool ForceInitialize()
        {
            connectionSessionAdapter ??= GetComponent<PendantV3ConnectionSessionAdapter>();
            RefreshFromSession();
            return connectionSessionAdapter != null;
        }

        public string GetDebugSummary()
        {
            return CurrentState.ToDebugSummary();
        }

        public void SetToolbarState(bool baseFrame, bool toolFrame, bool trail, bool ghost, bool boundary, bool collisionManual)
        {
            showBaseFrame = baseFrame;
            showToolFrame = toolFrame;
            showTrail = trail;
            showGhost = ghost && previewKind is PendantV3VisualizationState.PreviewKind.JointGhost or PendantV3VisualizationState.PreviewKind.MoveJGhost;
            showWorkspaceBoundary = boundary;
            showCollisionManual = collisionManual;
            EmitState();
        }

        public void SetRuntimePose(double[] jointAnglesDeg, double[] tcpPose)
        {
            CopyInto(jointAnglesDeg, currentJointAnglesDeg);
            CopyInto(tcpPose, currentTcpPose);
            if (previewKind == PendantV3VisualizationState.PreviewKind.None)
            {
                previewLabel = string.Empty;
            }

            EmitState();
        }

        public void PreviewJointPose(double[] jointAnglesDeg, int activeJoint, string label, bool moveRequest)
        {
            CopyInto(jointAnglesDeg, ghostJointAnglesDeg);
            previewKind = moveRequest ? PendantV3VisualizationState.PreviewKind.MoveJGhost : PendantV3VisualizationState.PreviewKind.JointGhost;
            activeJointIndex = activeJoint;
            activeTcpAxisIndex = -1;
            activeTcpDirection = 0;
            previewLabel = label ?? string.Empty;
            showGhost = true;
            EmitState();
        }

        public void PreviewTcpTarget(double[] tcpPose, int activeAxis, int direction, string coord, string label, bool moveRequest)
        {
            CopyInto(tcpPose, targetTcpPose);
            previewKind = moveRequest ? PendantV3VisualizationState.PreviewKind.MoveLTarget : PendantV3VisualizationState.PreviewKind.TcpTarget;
            activeJointIndex = -1;
            activeTcpAxisIndex = activeAxis;
            activeTcpDirection = direction;
            coordSystem = string.IsNullOrWhiteSpace(coord) ? "Base" : coord;
            previewLabel = label ?? string.Empty;
            showGhost = false;
            EmitState();
        }

        public void ClearPreview()
        {
            previewKind = PendantV3VisualizationState.PreviewKind.None;
            activeJointIndex = -1;
            activeTcpAxisIndex = -1;
            activeTcpDirection = 0;
            previewLabel = string.Empty;
            showGhost = false;
            CopyInto(CurrentState.CurrentJointAnglesDeg, ghostJointAnglesDeg);
            CopyInto(CurrentState.CurrentTcpPose, targetTcpPose);
            EmitState();
        }

        public void RefreshFromSession()
        {
            if (connectionSessionAdapter == null)
            {
                return;
            }

            CopyInto(connectionSessionAdapter.CurrentRobotState.JointPosDeg, currentJointAnglesDeg);
            CopyInto(connectionSessionAdapter.CurrentRobotState.TcpPose, currentTcpPose);
            coordSystem = connectionSessionAdapter.CurrentState.HasSynced ? coordSystem : "Base";
            EmitState();
        }

        private void HandleSessionStateChanged(PendantV3ConnectionSessionState _)
        {
            RefreshFromSession();
        }

        private void EmitState()
        {
            var session = connectionSessionAdapter != null
                ? connectionSessionAdapter.CurrentState
                : PendantV3ConnectionSessionState.DefaultDisconnected();
            var collisionAuto = session.DisplayKind is PendantV3ConnectionDisplayKind.Fault or PendantV3ConnectionDisplayKind.AutoReconnect;
            riskSummary = session.ReconnectFailed
                ? "자동 복구 실패"
                : collisionAuto
                    ? "위험 또는 재연결 상태"
                    : session.DisplayKind == PendantV3ConnectionDisplayKind.ConnectedUnsynced
                        ? "동기화 후 재확인"
                        : "안전";
            var next = new PendantV3VisualizationState(
                previewKind,
                showBaseFrame,
                showToolFrame,
                showTrail,
                showGhost,
                showWorkspaceBoundary,
                showCollisionManual || collisionAuto,
                collisionAuto,
                session.IsLiveArmActive,
                session.ActualMoveAllowed,
                activeJointIndex,
                activeTcpAxisIndex,
                activeTcpDirection,
                coordSystem,
                previewLabel,
                riskSummary,
                currentJointAnglesDeg,
                ghostJointAnglesDeg,
                currentTcpPose,
                targetTcpPose);
            if (CurrentState.Equals(next))
            {
                return;
            }

            CurrentState = next;
            StateChanged?.Invoke(CurrentState);
        }

        private static void CopyInto(double[] source, double[] destination)
        {
            Array.Clear(destination, 0, destination.Length);
            if (source == null)
            {
                return;
            }

            var count = Mathf.Min(source.Length, destination.Length);
            Array.Copy(source, destination, count);
        }
    }
}
