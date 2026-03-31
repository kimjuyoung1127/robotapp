// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.Math;
using KineTutor3D.UI.Data;

namespace KineTutor3D.UI
{
    public static class MathReadinessFormatter
    {
        public static string FormatDirection(Vec3D displacement)
        {
            const double threshold = 0.005;
            var horizontal = displacement.X > threshold
                ? "오른쪽"
                : displacement.X < -threshold
                    ? "왼쪽"
                    : string.Empty;
            var vertical = displacement.Y > threshold
                ? "위쪽"
                : displacement.Y < -threshold
                    ? "아래쪽"
                    : string.Empty;

            if (!string.IsNullOrEmpty(horizontal) && !string.IsNullOrEmpty(vertical))
            {
                return $"{horizontal} {vertical}";
            }

            if (!string.IsNullOrEmpty(horizontal))
            {
                return horizontal;
            }

            if (!string.IsNullOrEmpty(vertical))
            {
                return vertical;
            }

            return "조금";
        }

        public static string FormatWhyItMoved(WhyItMovedState state)
        {
            if (state == null || !state.IsMeaningfulChange)
            {
                return "관절을 움직여 방향 변화를 확인해 보세요.";
            }

            return $"좋아요. J{state.ChangedJointIndex + 1}을 움직이니 끝점이 {FormatDirection(state.EEDisplacement)} 쪽으로 바뀌었어요.";
        }

        /// <summary>"Q1/3" 형식 진행 메시지를 반환합니다.</summary>
        public static string FormatProgressMessage(int currentIndex, int total)
        {
            return $"Q{currentIndex + 1}/{total}";
        }

        /// <summary>오답 횟수에 따른 적응형 힌트 메시지를 반환합니다.</summary>
        public static string FormatAdaptiveHint(int attemptCount, string coachHint)
        {
            if (attemptCount >= 3)
            {
                return "정답을 확인해 볼게요. 학습이 더 중요해요!";
            }

            if (attemptCount >= 2)
            {
                if (!string.IsNullOrWhiteSpace(coachHint))
                {
                    return coachHint;
                }

                return "조금만 더 생각해 볼까요? 힌트를 참고해 보세요.";
            }

            return string.Empty;
        }

        /// <summary>방향 화살표 아이콘 이름을 반환합니다.</summary>
        public static string GetDirectionIconName(Vec3D displacement)
        {
            const double threshold = 0.005;
            var hasRight = displacement.X > threshold;
            var hasLeft = displacement.X < -threshold;
            var hasUp = displacement.Y > threshold;
            var hasDown = displacement.Y < -threshold;

            if (hasRight && hasUp) return "icon-arrow-up-right";
            if (hasLeft && hasUp) return "icon-arrow-up-left";
            if (hasRight && hasDown) return "icon-arrow-down-right";
            if (hasLeft && hasDown) return "icon-arrow-down-left";
            if (hasRight) return "icon-arrow-right";
            if (hasLeft) return "icon-arrow-left";
            if (hasUp) return "icon-arrow-up";
            if (hasDown) return "icon-arrow-down";
            return "icon-help";
        }
    }
}
