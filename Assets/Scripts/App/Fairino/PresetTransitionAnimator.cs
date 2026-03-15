// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections;
using KineTutor3D.UI;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 관절 각도 보간 전환을 담당하는 Coroutine 기반 애니메이터입니다.
    /// EaseInOutCubic 보간으로 프리셋/연결 포즈 전환을 부드럽게 처리합니다.
    /// </summary>
    public class PresetTransitionAnimator : MonoBehaviour
    {
        private Coroutine activeCoroutine;

        /// <summary>
        /// 현재 보간 중인지 여부입니다.
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        /// 매 프레임 보간된 관절 각도를 전달합니다.
        /// </summary>
        public event Action<double[]> OnFrameUpdated;

        /// <summary>
        /// 보간이 완료되었을 때 최종 관절 각도를 전달합니다.
        /// </summary>
        public event Action<double[]> OnTransitionComplete;

        /// <summary>
        /// 보간 전환을 시작합니다.
        /// </summary>
        public void StartTransition(double[] from, double[] to, float duration)
        {
            if (from == null || to == null || from.Length != to.Length)
            {
                return;
            }

            if (duration <= 0f)
            {
                OnFrameUpdated?.Invoke((double[])to.Clone());
                OnTransitionComplete?.Invoke((double[])to.Clone());
                return;
            }

            Cancel();
            activeCoroutine = StartCoroutine(AnimateTransition(from, to, duration));
        }

        /// <summary>
        /// 진행 중인 전환을 즉시 취소합니다.
        /// </summary>
        public void Cancel()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            IsAnimating = false;
        }

        /// <summary>
        /// EaseInOutCubic 보간 함수입니다. t=[0,1] 범위의 입력을 부드럽게 변환합니다.
        /// </summary>
        public static double EaseInOutCubic(double t)
        {
            if (t < 0.0) return 0.0;
            if (t > 1.0) return 1.0;
            return t < 0.5
                ? 4.0 * t * t * t
                : 1.0 - System.Math.Pow(-2.0 * t + 2.0, 3.0) / 2.0;
        }

        /// <summary>
        /// 두 값을 선형 보간합니다.
        /// </summary>
        public static double LerpDouble(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        private IEnumerator AnimateTransition(double[] from, double[] to, float duration)
        {
            IsAnimating = true;
            var count = from.Length;
            var interpolated = new double[count];
            var elapsed = 0f;

            while (elapsed < duration)
            {
                var t = elapsed / duration;
                var eased = EaseInOutCubic(t);

                for (var i = 0; i < count; i++)
                {
                    interpolated[i] = LerpDouble(from[i], to[i], eased);
                }

                OnFrameUpdated?.Invoke(interpolated);
                yield return null;
                elapsed += Time.deltaTime;
            }

            // 최종 프레임: 정확한 목표값 전달
            for (var i = 0; i < count; i++)
            {
                interpolated[i] = to[i];
            }

            OnFrameUpdated?.Invoke(interpolated);
            IsAnimating = false;
            activeCoroutine = null;
            OnTransitionComplete?.Invoke(interpolated);
        }
    }
}
