// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 프리뷰 단계의 위험 요약을 UI가 단일 객체로 소비하도록 묶습니다.
    /// </summary>
    public sealed class PreviewRiskSummary
    {
        public bool HasBlockingRisk { get; set; }

        public string SeverityLabel { get; set; } = "SAFE";

        public string Summary { get; set; } = "Preview gate enabled";

        public string Detail { get; set; } = "Run preview before live motion.";

        public static PreviewRiskSummary CreateDefault()
        {
            return new PreviewRiskSummary();
        }
    }
}
