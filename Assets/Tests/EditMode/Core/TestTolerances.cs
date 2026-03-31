// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// 테스트 수치 비교에 사용하는 허용 오차 상수를 제공합니다.
namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// EditMode 테스트 공용 허용 오차 상수입니다.
    /// </summary>
    public static class TestTolerances
    {
        /// <summary>
        /// 순수 수학 연산 허용 오차입니다.
        /// </summary>
        public const double Math = 1e-10;

        /// <summary>
        /// 회전 행렬 요소 비교 허용 오차입니다.
        /// </summary>
        public const double Rotation = 1e-6;

        /// <summary>
        /// 위치 벡터 비교 허용 오차(미터)입니다.
        /// </summary>
        public const double Position = 1e-4;

        /// <summary>
        /// FK 회전 비교 허용 오차(라디안)입니다.
        /// </summary>
        public const double FKRotation = 1e-3;
    }
}
