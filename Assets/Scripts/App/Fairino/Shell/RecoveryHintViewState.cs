// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 우측 레일의 복구 안내 카드가 소비하는 다음 행동 힌트입니다.
    /// </summary>
    public sealed class RecoveryHintViewState
    {
        public string Title { get; set; } = "NEXT ACTION";

        public string Body { get; set; } = "Bind shell state, choose a work tab, then preview before live motion.";

        public string ActionLabel { get; set; } = "Preview first";

        public static RecoveryHintViewState CreateDefault()
        {
            return new RecoveryHintViewState();
        }
    }
}
