// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 웨이포인트 시퀀스를 순차 실행하는 재생 엔진입니다.
    /// PlayOnce(단일 실행), PlayLoop(반복 실행), Stop(정지)을 지원합니다.
    /// DryRun 모드에서는 3D 애니메이션만 실행하고 로봇에 명령을 전송하지 않습니다.
    /// </summary>
    public class WaypointCycleRunner : MonoBehaviour
    {
        /// <summary>
        /// 실행 상태입니다.
        /// </summary>
        public enum RunState { Idle, Running, Stopping }

        private const double ArrivalThresholdDeg = 1.0;
        private const float ArrivalPollInterval = 0.1f;
        private const float ArrivalTimeoutSec = 30f;

        private FairinoConnectionService connectionService;
        private FairinoRobotConfig config;
        private PresetTransitionAnimator presetAnimator;
        private Coroutine activeCoroutine;
        private double[] lastCompletedAngles;
        private Action<double[]> activeCompleteHandler;

        /// <summary>
        /// 현재 실행 상태입니다.
        /// </summary>
        public RunState State { get; private set; } = RunState.Idle;

        /// <summary>
        /// 현재 실행 중인 웨이포인트 인덱스입니다.
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// 전체 웨이포인트 수입니다.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// 웨이포인트 도달 시 호출됩니다.
        /// </summary>
        public event Action<int, string> OnWaypointReached;

        /// <summary>
        /// 시퀀스 완료 시 호출됩니다.
        /// </summary>
        public event Action OnSequenceComplete;

        /// <summary>
        /// 에러 발생 시 호출됩니다.
        /// </summary>
        public event Action<string> OnError;

        /// <summary>
        /// 각 웨이포인트 전환 시 관절 각도 프레임을 전달합니다.
        /// </summary>
        public event Action<double[]> OnFrameUpdated;

        /// <summary>
        /// 의존성을 주입합니다.
        /// </summary>
        public void Inject(FairinoConnectionService service, FairinoRobotConfig robotConfig, PresetTransitionAnimator animator)
        {
            connectionService = service;
            config = robotConfig;
            presetAnimator = animator;
        }

        /// <summary>
        /// 시퀀스를 한 번 실행합니다.
        /// </summary>
        public void PlayOnce(WaypointSequence sequence, bool dryRun)
        {
            if (State != RunState.Idle)
            {
                OnError?.Invoke("이미 실행 중입니다. Stop 후 다시 시도하세요.");
                return;
            }

            if (sequence == null || sequence.waypoints == null || sequence.waypoints.Length == 0)
            {
                OnError?.Invoke("실행할 웨이포인트가 없습니다.");
                return;
            }

            activeCoroutine = StartCoroutine(RunSequence(sequence, dryRun, false));
        }

        /// <summary>
        /// 시퀀스를 반복 실행합니다.
        /// </summary>
        public void PlayLoop(WaypointSequence sequence, bool dryRun)
        {
            if (State != RunState.Idle)
            {
                OnError?.Invoke("이미 실행 중입니다. Stop 후 다시 시도하세요.");
                return;
            }

            if (sequence == null || sequence.waypoints == null || sequence.waypoints.Length == 0)
            {
                OnError?.Invoke("실행할 웨이포인트가 없습니다.");
                return;
            }

            activeCoroutine = StartCoroutine(RunSequence(sequence, dryRun, true));
        }

        /// <summary>
        /// 실행을 정지합니다.
        /// </summary>
        public void Stop()
        {
            if (State == RunState.Idle)
            {
                return;
            }

            State = RunState.Stopping;

            CleanupCompleteHandler();

            if (presetAnimator != null)
            {
                presetAnimator.Cancel();
            }

            if (connectionService != null && !connectionService.IsMockMode)
            {
                connectionService.StopMotion();
            }

            // StopAllCoroutines로 중첩 코루틴(AnimateToWaypoint 등)까지 확실히 정지
            StopAllCoroutines();
            activeCoroutine = null;

            State = RunState.Idle;
            CurrentIndex = 0;
            TotalCount = 0;
            lastCompletedAngles = null;
            Debug.Log("[WaypointCycleRunner] 정지");
            OnSequenceComplete?.Invoke();
        }

        private IEnumerator RunSequence(WaypointSequence sequence, bool dryRun, bool loop)
        {
            State = RunState.Running;
            TotalCount = sequence.waypoints.Length;
            lastCompletedAngles = null;

            do
            {
                // 빈 시퀀스 감지 (Clear All 중 호출 방지)
                if (sequence.waypoints == null || sequence.waypoints.Length == 0)
                {
                    OnError?.Invoke("웨이포인트가 비어있습니다.");
                    break;
                }

                TotalCount = sequence.waypoints.Length;

                for (var i = 0; i < sequence.waypoints.Length; i++)
                {
                    if (State != RunState.Running)
                    {
                        yield break;
                    }

                    CurrentIndex = i;
                    var wp = sequence.waypoints[i];

                    // 3D 미리보기 애니메이션
                    yield return StartCoroutine(AnimateToWaypoint(wp));

                    if (State != RunState.Running) yield break;

                    // DryRun이 아니면 실제 로봇에 명령 전송
                    if (!dryRun && connectionService != null && connectionService.Client.IsConnected)
                    {
                        yield return StartCoroutine(ExecuteRobotMove(wp));

                        if (State != RunState.Running) yield break;
                    }

                    // 대기 시간
                    if (wp.dwellSec > 0)
                    {
                        yield return new WaitForSeconds((float)wp.dwellSec);

                        if (State != RunState.Running) yield break;
                    }

                    OnWaypointReached?.Invoke(i, wp.name);
                }

                // 루프 사이 최소 1프레임 대기 (UI 응답성 보장)
                yield return null;

            } while (loop && State == RunState.Running);

            State = RunState.Idle;
            CurrentIndex = 0;
            activeCoroutine = null;
            OnSequenceComplete?.Invoke();
        }

        private IEnumerator AnimateToWaypoint(Waypoint wp)
        {
            if (presetAnimator == null)
            {
                lastCompletedAngles = (double[])wp.jointsDeg.Clone();
                OnFrameUpdated?.Invoke(wp.jointsDeg);
                yield break;
            }

            // 이전 완료 포즈에서 목표 포즈로 보간 (텔레포트 방지)
            var fromAngles = lastCompletedAngles ?? GetCurrentAnglesFromAnimator();
            var speedDuration = ResolveDuration(wp.speedPreset);
            var completed = false;

            // 이벤트 핸들러를 필드에 저장 → Stop 시 확실한 해제
            CleanupCompleteHandler();
            activeCompleteHandler = _ => { completed = true; };
            presetAnimator.OnTransitionComplete += activeCompleteHandler;

            presetAnimator.StartTransition(fromAngles, wp.jointsDeg, speedDuration);

            while (!completed && State == RunState.Running)
            {
                yield return null;
            }

            CleanupCompleteHandler();

            // 완료된 포즈를 기억 → 다음 웨이포인트의 시작점으로 사용
            lastCompletedAngles = (double[])wp.jointsDeg.Clone();
        }

        private IEnumerator ExecuteRobotMove(Waypoint wp)
        {
            var speedAcc = config != null ? config.GetSpeedAcc(wp.speedPreset) : (speed: 30, acc: 50);

            FairinoResult result;
            if (wp.moveType == "MoveL" && wp.tcpMm != null && wp.tcpMm.Length >= 6)
            {
                result = connectionService.Client.MoveL(wp.tcpMm, speedAcc.speed, speedAcc.acc);
            }
            else
            {
                result = connectionService.Client.MoveJ(wp.jointsDeg, speedAcc.speed, speedAcc.acc);
            }

            if (!result.IsSuccess)
            {
                OnError?.Invoke($"이동 실패: {result.Message}");
                yield break;
            }

            // 목표 도달 대기
            yield return StartCoroutine(WaitForArrival(wp.jointsDeg));
        }

        private IEnumerator WaitForArrival(double[] targetDeg)
        {
            var elapsed = 0f;

            while (elapsed < ArrivalTimeoutSec)
            {
                if (State == RunState.Stopping)
                {
                    yield break;
                }

                if (connectionService != null && connectionService.LastState.JointPosDeg != null)
                {
                    var current = connectionService.LastState.JointPosDeg;
                    if (IsArrived(current, targetDeg))
                    {
                        yield break;
                    }
                }

                yield return new WaitForSeconds(ArrivalPollInterval);
                elapsed += ArrivalPollInterval;
            }

            OnError?.Invoke("목표 도달 타임아웃 (30초)");
        }

        private static bool IsArrived(double[] current, double[] target)
        {
            if (current == null || target == null || current.Length < 6 || target.Length < 6)
            {
                return false;
            }

            for (var i = 0; i < 6; i++)
            {
                if (System.Math.Abs(current[i] - target[i]) > ArrivalThresholdDeg)
                {
                    return false;
                }
            }

            return true;
        }

        private void CleanupCompleteHandler()
        {
            if (activeCompleteHandler != null && presetAnimator != null)
            {
                presetAnimator.OnTransitionComplete -= activeCompleteHandler;
            }

            activeCompleteHandler = null;
        }

        private double[] GetCurrentAnglesFromAnimator()
        {
            // PresetTransitionAnimator는 마지막 프레임 각도를 직접 노출하지 않으므로
            // connectionService의 마지막 상태를 사용합니다.
            if (connectionService != null && connectionService.LastState.JointPosDeg != null)
            {
                return (double[])connectionService.LastState.JointPosDeg.Clone();
            }

            return new double[6];
        }

        private static float ResolveDuration(string speedPreset)
        {
            switch (speedPreset)
            {
                case "slow": return UIDesignTokens.Anim.PresetTransition * 2f;
                case "fast": return UIDesignTokens.Anim.PresetTransition * 0.6f;
                default: return UIDesignTokens.Anim.PresetTransition;
            }
        }
    }
}
