// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Threading.Tasks;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private Task<AsyncReadbackOperationResult> activeReadbackOperationTask;
        private string activeReadbackOperationLabel = string.Empty;
        private bool hasPendingReadbackStartUiUpdate;
        private string pendingReadbackStartUiMessage = string.Empty;
        private bool awaitingPolledReadbackCompletion;
        private AsyncReadbackOperationKind awaitingPolledReadbackKind;
        private string awaitingPolledReadbackLabel = string.Empty;
        private string awaitingPolledReadbackCompletedMessage = string.Empty;

        private enum AsyncReadbackOperationKind
        {
            None,
            ConnectAndSync,
            SyncCurrentState,
        }

        private readonly struct AsyncReadbackOperationResult
        {
            public AsyncReadbackOperationResult(
                AsyncReadbackOperationKind kind,
                FairinoResult result,
                FairinoRobotState state,
                bool hasState,
                bool resetCurrentPositionReadComplete,
                bool awaitPolledState = false,
                string awaitPolledStateMessage = "",
                string completedMessage = "")
            {
                Kind = kind;
                Result = result;
                State = state;
                HasState = hasState;
                ResetCurrentPositionReadComplete = resetCurrentPositionReadComplete;
                AwaitPolledState = awaitPolledState;
                AwaitPolledStateMessage = awaitPolledStateMessage ?? string.Empty;
                CompletedMessage = completedMessage ?? result.Message;
            }

            public AsyncReadbackOperationKind Kind { get; }
            public FairinoResult Result { get; }
            public FairinoRobotState State { get; }
            public bool HasState { get; }
            public bool ResetCurrentPositionReadComplete { get; }
            public bool AwaitPolledState { get; }
            public string AwaitPolledStateMessage { get; }
            public string CompletedMessage { get; }
        }

        public bool ConnectAndSyncDefaultAsync()
        {
            return TryStartAsyncReadbackOperation(
                AsyncReadbackOperationKind.ConnectAndSync,
                "연결 + 위치 읽기",
                "[Connect] 연결과 현재 위치 읽기 요청 중...",
                RunConnectAndSyncDefaultAsync);
        }

        public bool SyncCurrentStateAsync()
        {
            return TryStartAsyncReadbackOperation(
                AsyncReadbackOperationKind.SyncCurrentState,
                "현재 위치 읽기",
                "[Sync] 현재 자세 읽기 요청 중...",
                RunSyncCurrentStateAsync);
        }

        private bool HasPendingAsyncReadbackOperation()
        {
            var task = activeReadbackOperationTask;
            return (task != null && !task.IsCompleted) || awaitingPolledReadbackCompletion;
        }

        private bool HasPendingAsyncReadbackBackgroundTask()
        {
            var task = activeReadbackOperationTask;
            return task != null && !task.IsCompleted;
        }

        private bool TryStartAsyncReadbackOperation(
            AsyncReadbackOperationKind kind,
            string label,
            string startMessage,
            Func<AsyncReadbackOperationResult> work)
        {
            if (connectionService == null || config == null || templateDefinition == null)
            {
                PushFeedback($"[{label}] 런타임 초기화가 아직 끝나지 않았다.");
                RefreshSnapshot();
                return false;
            }

            if (HasPendingAsyncReadbackOperation())
            {
                var pendingLabel = !string.IsNullOrWhiteSpace(activeReadbackOperationLabel)
                    ? activeReadbackOperationLabel
                    : awaitingPolledReadbackLabel;
                PushFeedback($"[{label}] 이미 {pendingLabel} 작업이 진행 중이다.");
                RefreshSnapshot();
                return false;
            }

            activeReadbackOperationLabel = label;
            activeReadbackOperationTask = Task.Run(work);
            hasPendingReadbackStartUiUpdate = true;
            pendingReadbackStartUiMessage = startMessage;
            return true;
        }

        private void ApplyPendingReadbackStartUiIfNeeded()
        {
            if (!hasPendingReadbackStartUiUpdate)
            {
                return;
            }

            hasPendingReadbackStartUiUpdate = false;
            PushFeedback(pendingReadbackStartUiMessage);
            pendingReadbackStartUiMessage = string.Empty;
            RefreshSnapshot();
        }

        private void PollAsyncReadbackOperationCompletion()
        {
            var task = activeReadbackOperationTask;
            if (task == null || !task.IsCompleted)
            {
                return;
            }

            activeReadbackOperationTask = null;
            activeReadbackOperationLabel = string.Empty;

            if (task.IsFaulted)
            {
                var message = task.Exception?.GetBaseException().Message ?? "알 수 없는 오류";
                PushFeedback($"[Readback] 백그라운드 작업 실패: {message}");
                RefreshSnapshot();
                return;
            }

            ApplyAsyncReadbackOperationResult(task.Result);
        }

        private AsyncReadbackOperationResult RunConnectAndSyncDefaultAsync()
        {
            var connectResult = connectionService.Connect(
                config.defaultIp,
                config.defaultPort,
                applyLiveBringupPolicies: false,
                emitConnectionStateChanged: false,
                emitEnableStateChanged: false,
                emitInitialState: false,
                emitError: false);
            if (!connectResult.IsSuccess)
            {
                return new AsyncReadbackOperationResult(
                    AsyncReadbackOperationKind.ConnectAndSync,
                    connectResult,
                    FairinoRobotState.Zero(),
                    hasState: false,
                    resetCurrentPositionReadComplete: true);
            }

            var syncResult = connectionService.SyncCurrentState(
                emitStateUpdated: false,
                emitError: false);
            if (!syncResult.IsSuccess)
            {
                return new AsyncReadbackOperationResult(
                    AsyncReadbackOperationKind.ConnectAndSync,
                    FairinoResult.Fail(syncResult.ErrorCode, syncResult.Message),
                    FairinoRobotState.Zero(),
                    hasState: false,
                    resetCurrentPositionReadComplete: true);
            }

            return new AsyncReadbackOperationResult(
                AsyncReadbackOperationKind.ConnectAndSync,
                FairinoResult.Ok("[Connect] 연결과 현재 위치 읽기 완료"),
                syncResult.Value,
                hasState: true,
                resetCurrentPositionReadComplete: true);
        }

        private AsyncReadbackOperationResult RunSyncCurrentStateAsync()
        {
            if (!connectionService.Client.IsConnected)
            {
                return new AsyncReadbackOperationResult(
                    AsyncReadbackOperationKind.SyncCurrentState,
                    FairinoResult.Fail(-1, "연결되지 않은 상태입니다."),
                    FairinoRobotState.Zero(),
                    hasState: false,
                    resetCurrentPositionReadComplete: false);
            }

            var syncResult = connectionService.SyncCurrentState(
                emitStateUpdated: false,
                emitError: false);
            if (!syncResult.IsSuccess)
            {
                return new AsyncReadbackOperationResult(
                    AsyncReadbackOperationKind.SyncCurrentState,
                    FairinoResult.Fail(syncResult.ErrorCode, syncResult.Message),
                    FairinoRobotState.Zero(),
                    hasState: false,
                    resetCurrentPositionReadComplete: false);
            }

            return new AsyncReadbackOperationResult(
                AsyncReadbackOperationKind.SyncCurrentState,
                FairinoResult.Ok("[Sync] 현재 자세 동기화 완료"),
                syncResult.Value,
                hasState: true,
                resetCurrentPositionReadComplete: false);
        }

        private void ApplyAsyncReadbackOperationResult(AsyncReadbackOperationResult operationResult)
        {
            if (operationResult.ResetCurrentPositionReadComplete)
            {
                hasCurrentPositionReadComplete = false;
                liveGripperWarmupAttemptedThisConnection = false;
            }

            if (operationResult.Result.IsSuccess && operationResult.AwaitPolledState)
            {
                awaitingPolledReadbackCompletion = true;
                awaitingPolledReadbackKind = operationResult.Kind;
                awaitingPolledReadbackLabel = activeReadbackOperationLabel;
                awaitingPolledReadbackCompletedMessage = operationResult.CompletedMessage;
                connectionService.RequestImmediatePoll();
                PushFeedback(operationResult.AwaitPolledStateMessage);
                RefreshSnapshot();
                return;
            }

            if (operationResult.Result.IsSuccess && operationResult.HasState)
            {
                ClearRememberedOperatorBlockedReason();
                hasCurrentPositionReadComplete = true;
                currentState = operationResult.State;
                templateDefinition.PosePresetProvider?.UpdateCurrent(operationResult.State.JointPosDeg);
                previewJointAnglesDeg = null;
                previewTcpPose = null;
                previewTcpVisualJointAnglesDeg = null;
                previewUsesJointPose = false;
                ClearPreparedMotionContext();
                InvalidateLiveApprovalContext();
                requestStageRefocus = true;
                ApplyVisualState();
                PushFeedback(operationResult.Result.Message);
            }
            else
            {
                RememberOperatorBlockedReason(operationResult.Result.Message);
                PushFeedback(operationResult.Result.Message);
            }

            RefreshSnapshot();
        }

        private void CompleteAwaitingPolledReadbackIfNeeded()
        {
            if (!awaitingPolledReadbackCompletion)
            {
                return;
            }

            awaitingPolledReadbackCompletion = false;
            awaitingPolledReadbackKind = AsyncReadbackOperationKind.None;
            awaitingPolledReadbackLabel = string.Empty;
            var message = awaitingPolledReadbackCompletedMessage;
            awaitingPolledReadbackCompletedMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(message))
            {
                PushFeedback(message);
            }
        }
    }
}
