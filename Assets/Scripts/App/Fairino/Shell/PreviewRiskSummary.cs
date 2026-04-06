// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 프리뷰 단계의 위험 요약을 UI가 단일 객체로 소비하도록 묶습니다.
    /// </summary>
    public sealed class PreviewRiskSummary
    {
        public bool HasBlockingRisk { get; set; }

        public string SeverityLabel { get; set; } = "안전";

        public string Summary { get; set; } = "미리보기 안전 확인 사용 중";

        public string Detail { get; set; } = "실제 이동 전에 미리보기를 먼저 확인하세요.";

        public static PreviewRiskSummary CreateDefault()
        {
            return new PreviewRiskSummary();
        }
    }
}
