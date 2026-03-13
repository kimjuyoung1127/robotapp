// Folder: Math - Pure C# double-precision math; no UnityEngine references.
using System;

namespace KineTutor3D.Math
{
    /// <summary>
    /// double 기반 3x3 회전 행렬을 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct Mat3D : IEquatable<Mat3D>
    {
        /// <summary>
        /// Equals 비교 기본 허용 오차입니다.
        /// </summary>
        public const double DefaultTolerance = 1e-10;

        private readonly double m00;
        private readonly double m01;
        private readonly double m02;
        private readonly double m10;
        private readonly double m11;
        private readonly double m12;
        private readonly double m20;
        private readonly double m21;
        private readonly double m22;

        /// <summary>
        /// 단위 행렬입니다.
        /// </summary>
        public static Mat3D Identity => new(
            1.0, 0.0, 0.0,
            0.0, 1.0, 0.0,
            0.0, 0.0, 1.0);

        /// <summary>
        /// 3x3 행렬을 생성합니다.
        /// </summary>
        public Mat3D(
            double m00, double m01, double m02,
            double m10, double m11, double m12,
            double m20, double m21, double m22)
        {
            GuardFinite(m00, nameof(m00));
            GuardFinite(m01, nameof(m01));
            GuardFinite(m02, nameof(m02));
            GuardFinite(m10, nameof(m10));
            GuardFinite(m11, nameof(m11));
            GuardFinite(m12, nameof(m12));
            GuardFinite(m20, nameof(m20));
            GuardFinite(m21, nameof(m21));
            GuardFinite(m22, nameof(m22));

            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
        }

        /// <summary>
        /// row-major 배열로 행렬을 생성합니다.
        /// </summary>
        public static Mat3D FromRowMajor(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length != 9)
            {
                throw new ArgumentException("3x3 행렬은 9개 원소가 필요합니다.", nameof(values));
            }

            return new Mat3D(
                values[0], values[1], values[2],
                values[3], values[4], values[5],
                values[6], values[7], values[8]);
        }

        /// <summary>
        /// 인덱서로 원소를 조회합니다.
        /// </summary>
        public double this[int row, int col]
        {
            get
            {
                return (row, col) switch
                {
                    (0, 0) => m00,
                    (0, 1) => m01,
                    (0, 2) => m02,
                    (1, 0) => m10,
                    (1, 1) => m11,
                    (1, 2) => m12,
                    (2, 0) => m20,
                    (2, 1) => m21,
                    (2, 2) => m22,
                    _ => throw new IndexOutOfRangeException("Mat3D 인덱스 범위를 벗어났습니다.")
                };
            }
        }

        /// <summary>
        /// 전치 행렬을 반환합니다.
        /// </summary>
        public Mat3D Transpose()
        {
            return new Mat3D(
                m00, m10, m20,
                m01, m11, m21,
                m02, m12, m22);
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Mat3D other)
        {
            return Equals(other, DefaultTolerance);
        }

        /// <summary>
        /// 지정 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Mat3D other, double tolerance)
        {
            if (tolerance < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance));
            }

            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    if (System.Math.Abs(this[row, col] - other[row, col]) >= tolerance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Mat3D other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    hash.Add(System.Math.Round(this[row, col], 8));
                }
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(Mat3D left, Mat3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 기본 허용 오차로 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(Mat3D left, Mat3D right)
        {
            return !left.Equals(right);
        }

        private static void GuardFinite(double value, string paramName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException($"유효하지 않은 값입니다: {value}", paramName);
            }
        }
    }
}
