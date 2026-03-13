// Folder: UI - 3D 로봇 쇼룸 설정 컨텍스트.
using System;

namespace KineTutor3D.UI
{
    public enum RobotShowroomCtaKind
    {
        GuidedLesson,
        Sandbox,
        SelectOnly,
        None
    }

    /// <summary>
    /// 3D 로봇 쇼룸의 페이지별 설정을 담는 불변 구조체입니다.
    /// </summary>
    public readonly struct RobotShowroomContext
    {
        /// <summary>표시할 로봇 ID 목록.</summary>
        public string[] RobotIds { get; }

        /// <summary>동시에 활성화할 최대 3D pod 수.</summary>
        public int MaxVisiblePods { get; }

        /// <summary>이름 라벨 표시 여부.</summary>
        public bool ShowLabels { get; }

        /// <summary>CTA 버튼 표시 여부.</summary>
        public bool ShowCtaButtons { get; }

        /// <summary>사용자 orbit 허용 여부.</summary>
        public bool AllowOrbit { get; }

        /// <summary>pod 간 X축 간격.</summary>
        public float PodSpacing { get; }

        /// <summary>초기 hero 로봇 ID.</summary>
        public string HeroRobotId { get; }

        /// <summary>페이지 이동 허용 여부.</summary>
        public bool EnablePaging { get; }

        /// <summary>주 CTA 종류.</summary>
        public RobotShowroomCtaKind PrimaryCtaKind { get; }

        /// <summary>보조 CTA 종류.</summary>
        public RobotShowroomCtaKind SecondaryCtaKind { get; }

        public RobotShowroomContext(
            string[] robotIds,
            int maxVisiblePods = 3,
            bool showLabels = true,
            bool showCtaButtons = true,
            bool allowOrbit = false,
            float podSpacing = 1.2f,
            string heroRobotId = "",
            bool enablePaging = true,
            RobotShowroomCtaKind primaryCtaKind = RobotShowroomCtaKind.GuidedLesson,
            RobotShowroomCtaKind secondaryCtaKind = RobotShowroomCtaKind.Sandbox)
        {
            RobotIds = robotIds ?? Array.Empty<string>();
            MaxVisiblePods = maxVisiblePods > 0 ? maxVisiblePods : 3;
            ShowLabels = showLabels;
            ShowCtaButtons = showCtaButtons;
            AllowOrbit = allowOrbit;
            PodSpacing = podSpacing > 0f ? podSpacing : 1.2f;
            HeroRobotId = heroRobotId ?? string.Empty;
            EnablePaging = enablePaging;
            PrimaryCtaKind = primaryCtaKind;
            SecondaryCtaKind = secondaryCtaKind;
        }
    }
}
