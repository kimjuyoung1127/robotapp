// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 반응형 레이아웃 보정. 태블릿과 데스크탑 환경에 맞는 치수를 제공합니다.
    /// </summary>
    public static class UILayoutProfile
    {
        private const float TabletAspectThreshold = 1.5f;
        private const float TabletLeftPanelWidth = 300f;
        private const float TabletRightPanelWidth = 340f;

        /// <summary>
        /// 현재 화면 비율이 태블릿 수준인지 반환합니다.
        /// </summary>
        public static bool IsTablet
        {
            get
            {
                if (Screen.height <= 0)
                {
                    return false;
                }

                return (float)Screen.width / Screen.height < TabletAspectThreshold;
            }
        }

        /// <summary>왼쪽 패널 너비 (태블릿 보정 적용).</summary>
        public static float LeftPanelWidth =>
            IsTablet ? TabletLeftPanelWidth : UIDesignTokens.Size.LeftPanelWidth;

        /// <summary>오른쪽 패널 너비 (태블릿 보정 적용).</summary>
        public static float RightPanelWidth =>
            IsTablet ? TabletRightPanelWidth : UIDesignTokens.Size.RightPanelWidth;

        /// <summary>터치 타겟 최소 크기 (태블릿에서 확대).</summary>
        public static float TouchTarget =>
            IsTablet ? UIDesignTokens.Size.TouchTargetMin + 4f : UIDesignTokens.Size.TouchTargetMin;

        /// <summary>기본 간격 (태블릿에서 약간 축소).</summary>
        public static float DefaultSpacing =>
            IsTablet ? UIDesignTokens.Space.Sm : UIDesignTokens.Space.Md;
    }
}
