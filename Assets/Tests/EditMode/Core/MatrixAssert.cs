// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// 행렬 비교용 테스트 보조 어설션을 제공합니다.
using NUnit.Framework;
using KineTutor3D.Math;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 행렬 비교를 위한 테스트 보조 유틸리티입니다.
    /// </summary>
    public static class MatrixAssert
    {
        /// <summary>
        /// 두 4x4 행렬을 허용 오차 내에서 비교합니다.
        /// </summary>
        public static void AreEqual(Mat4D expected, Mat4D actual, double tolerance, string message = "")
        {
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var diff = System.Math.Abs(expected[row, col] - actual[row, col]);
                    if (diff >= tolerance)
                    {
                        Assert.Fail(
                            $"{message} 원소[{row},{col}] 불일치 " +
                            $"expected={expected[row, col]:G15}, actual={actual[row, col]:G15}, diff={diff:G15}, tol={tolerance:G15}");
                    }
                }
            }
        }

        /// <summary>
        /// 입력 행렬이 단위 행렬인지 검증합니다.
        /// </summary>
        public static void IsIdentity(Mat4D actual, double tolerance, string message = "")
        {
            AreEqual(Mat4D.Identity, actual, tolerance, message);
        }
    }
}
