// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// 우측 레일의 복구 안내 카드가 소비하는 다음 행동 힌트입니다.
    /// </summary>
    public sealed class RecoveryHintViewState
    {
        public string Title { get; set; } = "다음 안내";

        public string Body { get; set; } = "셸 상태를 먼저 바인딩하고, 작업 탭을 고른 뒤, 실제 이동 전에 미리보기를 확인하세요.";

        public string ActionLabel { get; set; } = "미리보기 먼저";

        public static RecoveryHintViewState CreateDefault()
        {
            return new RecoveryHintViewState();
        }
    }
}
