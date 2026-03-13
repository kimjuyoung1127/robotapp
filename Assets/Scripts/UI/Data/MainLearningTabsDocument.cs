// Folder: UI - HUD/view components only; no kinematics logic.
using System;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// Main 학습 쉘의 탭 콘텐츠 루트 문서입니다.
    /// </summary>
    [Serializable]
    public sealed class MainLearningTabsDocument
    {
        public string robotId = string.Empty;
        public string displayTitle = string.Empty;
        public string heroSummary = string.Empty;
        public LegendItemDto[] legend = Array.Empty<LegendItemDto>();
        public MainTabContentDto[] tabs = Array.Empty<MainTabContentDto>();
        public MotionPanelDto motion = new MotionPanelDto();
        public ForwardKinematicsPanelDto forwardKinematics = new ForwardKinematicsPanelDto();

        /// <summary>
        /// 비어 있는 배열/객체를 기본값으로 정리합니다.
        /// </summary>
        public void Sanitize()
        {
            legend ??= Array.Empty<LegendItemDto>();
            tabs ??= Array.Empty<MainTabContentDto>();
            motion ??= new MotionPanelDto();
            forwardKinematics ??= new ForwardKinematicsPanelDto();

            for (var i = 0; i < tabs.Length; i++)
            {
                tabs[i] ??= new MainTabContentDto();
                tabs[i].Sanitize();
            }

            motion.Sanitize();
            forwardKinematics.Sanitize();
        }

        /// <summary>
        /// ID에 맞는 탭 콘텐츠를 반환합니다.
        /// </summary>
        public MainTabContentDto GetTab(string tabId)
        {
            if (tabs == null || string.IsNullOrWhiteSpace(tabId))
            {
                return null;
            }

            for (var i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] != null && string.Equals(tabs[i].id, tabId, StringComparison.Ordinal))
                {
                    return tabs[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 탭 콘텐츠 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class MainTabContentDto
    {
        public string id = string.Empty;
        public string labelKo = string.Empty;
        public string headlineKo = string.Empty;
        public string bodyKo = string.Empty;
        public InfoCardDto[] cards = Array.Empty<InfoCardDto>();
        public CalloutDto[] callouts = Array.Empty<CalloutDto>();

        public void Sanitize()
        {
            cards ??= Array.Empty<InfoCardDto>();
            callouts ??= Array.Empty<CalloutDto>();
        }
    }

    /// <summary>
    /// 모션 탭 전용 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class MotionPanelDto
    {
        public string primaryCtaLabelKo = string.Empty;
        public string resultTitleKo = string.Empty;
        public MetricLabelDto[] metrics = Array.Empty<MetricLabelDto>();
        public string overlaySummaryKo = string.Empty;

        public void Sanitize()
        {
            metrics ??= Array.Empty<MetricLabelDto>();
        }
    }

    /// <summary>
    /// FK 탭 전용 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class ForwardKinematicsPanelDto
    {
        public string compactTitleKo = string.Empty;
        public string[] formulaLinesKo = Array.Empty<string>();
        public string advancedDrawerLabelKo = string.Empty;
        public InfoCardDto[] advancedCards = Array.Empty<InfoCardDto>();

        public void Sanitize()
        {
            formulaLinesKo ??= Array.Empty<string>();
            advancedCards ??= Array.Empty<InfoCardDto>();
        }
    }

    /// <summary>
    /// 색상 범례 항목 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class LegendItemDto
    {
        public string accentKey = string.Empty;
        public string labelKo = string.Empty;
    }

    /// <summary>
    /// 정보 카드 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class InfoCardDto
    {
        public string tagKo = string.Empty;
        public string titleKo = string.Empty;
        public string bodyKo = string.Empty;
    }

    /// <summary>
    /// 보조 콜아웃 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class CalloutDto
    {
        public string tone = string.Empty;
        public string titleKo = string.Empty;
        public string bodyKo = string.Empty;
    }

    /// <summary>
    /// 메트릭 라벨 DTO입니다.
    /// </summary>
    [Serializable]
    public sealed class MetricLabelDto
    {
        public string id = string.Empty;
        public string labelKo = string.Empty;
        public string unitKo = string.Empty;
    }
}
