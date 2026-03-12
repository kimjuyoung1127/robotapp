// Folder: Math - Pure C# double-precision math; no UnityEngine references.
using System;

namespace KineTutor3D.Math
{
    /// <summary>
    /// double 기반 4x4 동차 변환 행렬을 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct Mat4D : IEquatable<Mat4D>
    {
        /// <summary>
        /// Equals 비교 기본 허용 오차입니다.
        /// </summary>
        public const double DefaultTolerance = 1e-10;

        private readonly double m00;
        private readonly double m01;
        private readonly double m02;
        private readonly double m03;
        private readonly double m10;
        private readonly double m11;
        private readonly double m12;
        private readonly double m13;
        private readonly double m20;
        private readonly double m21;
        private readonly double m22;
        private readonly double m23;
        private readonly double m30;
        private readonly double m31;
        private readonly double m32;
        private readonly double m33;

        /// <summary>
        /// 단위 행렬입니다.
        /// </summary>
        public static Mat4D Identity => new(
            1.0, 0.0, 0.0, 0.0,
            0.0, 1.0, 0.0, 0.0,
            0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0);

        /// <summary>
        /// 4x4 행렬을 생성합니다.
        /// </summary>
        public Mat4D(
            double m00, double m01, double m02, double m03,
            double m10, double m11, double m12, double m13,
            double m20, double m21, double m22, double m23,
            double m30, double m31, double m32, double m33)
        {
            GuardFinite(m00, nameof(m00));
            GuardFinite(m01, nameof(m01));
            GuardFinite(m02, nameof(m02));
            GuardFinite(m03, nameof(m03));
            GuardFinite(m10, nameof(m10));
            GuardFinite(m11, nameof(m11));
            GuardFinite(m12, nameof(m12));
            GuardFinite(m13, nameof(m13));
            GuardFinite(m20, nameof(m20));
            GuardFinite(m21, nameof(m21));
            GuardFinite(m22, nameof(m22));
            GuardFinite(m23, nameof(m23));
            GuardFinite(m30, nameof(m30));
            GuardFinite(m31, nameof(m31));
            GuardFinite(m32, nameof(m32));
            GuardFinite(m33, nameof(m33));

            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
        }

        /// <summary>
        /// row-major 배열로 행렬을 생성합니다.
        /// </summary>
        public static Mat4D FromRowMajor(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length != 16)
            {
                throw new ArgumentException("4x4 행렬은 16개 원소가 필요합니다.", nameof(values));
            }

            return new Mat4D(
                values[0], values[1], values[2], values[3],
                values[4], values[5], values[6], values[7],
                values[8], values[9], values[10], values[11],
                values[12], values[13], values[14], values[15]);
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
                    (0, 3) => m03,
                    (1, 0) => m10,
                    (1, 1) => m11,
                    (1, 2) => m12,
                    (1, 3) => m13,
                    (2, 0) => m20,
                    (2, 1) => m21,
                    (2, 2) => m22,
                    (2, 3) => m23,
                    (3, 0) => m30,
                    (3, 1) => m31,
                    (3, 2) => m32,
                    (3, 3) => m33,
                    _ => throw new IndexOutOfRangeException("Mat4D 인덱스 범위를 벗어났습니다.")
                };
            }
        }

        /// <summary>
        /// 행렬 곱을 수행합니다.
        /// </summary>
        public static Mat4D operator *(Mat4D a, Mat4D b)
        {
            return new Mat4D(
                DotRowCol(a, 0, b, 0), DotRowCol(a, 0, b, 1), DotRowCol(a, 0, b, 2), DotRowCol(a, 0, b, 3),
                DotRowCol(a, 1, b, 0), DotRowCol(a, 1, b, 1), DotRowCol(a, 1, b, 2), DotRowCol(a, 1, b, 3),
                DotRowCol(a, 2, b, 0), DotRowCol(a, 2, b, 1), DotRowCol(a, 2, b, 2), DotRowCol(a, 2, b, 3),
                DotRowCol(a, 3, b, 0), DotRowCol(a, 3, b, 1), DotRowCol(a, 3, b, 2), DotRowCol(a, 3, b, 3));
        }

        /// <summary>
        /// 점(w=1)을 변환합니다.
        /// </summary>
        public Vec3D TransformPoint(Vec3D point)
        {
            var x = (m00 * point.X) + (m01 * point.Y) + (m02 * point.Z) + m03;
            var y = (m10 * point.X) + (m11 * point.Y) + (m12 * point.Z) + m13;
            var z = (m20 * point.X) + (m21 * point.Y) + (m22 * point.Z) + m23;
            return new Vec3D(x, y, z);
        }

        /// <summary>
        /// 방향 벡터(w=0)를 변환합니다.
        /// </summary>
        public Vec3D TransformDirection(Vec3D direction)
        {
            var x = (m00 * direction.X) + (m01 * direction.Y) + (m02 * direction.Z);
            var y = (m10 * direction.X) + (m11 * direction.Y) + (m12 * direction.Z);
            var z = (m20 * direction.X) + (m21 * direction.Y) + (m22 * direction.Z);
            return new Vec3D(x, y, z);
        }

        /// <summary>
        /// 위치 벡터를 추출합니다.
        /// </summary>
        public Vec3D ExtractPosition()
        {
            return new Vec3D(m03, m13, m23);
        }

        /// <summary>
        /// 회전 행렬(상단 3x3)을 추출합니다.
        /// </summary>
        public Mat3D ExtractRotation()
        {
            return new Mat3D(
                m00, m01, m02,
                m10, m11, m12,
                m20, m21, m22);
        }

        /// <summary>
        /// 강체 동차 행렬의 역행렬을 계산합니다.
        /// </summary>
        public Mat4D InverseHomogeneous()
        {
            var rt = ExtractRotation().Transpose();
            var p = ExtractPosition();

            var tx = -((rt[0, 0] * p.X) + (rt[0, 1] * p.Y) + (rt[0, 2] * p.Z));
            var ty = -((rt[1, 0] * p.X) + (rt[1, 1] * p.Y) + (rt[1, 2] * p.Z));
            var tz = -((rt[2, 0] * p.X) + (rt[2, 1] * p.Y) + (rt[2, 2] * p.Z));

            return new Mat4D(
                rt[0, 0], rt[0, 1], rt[0, 2], tx,
                rt[1, 0], rt[1, 1], rt[1, 2], ty,
                rt[2, 0], rt[2, 1], rt[2, 2], tz,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// X축 회전 행렬을 생성합니다.
        /// </summary>
        public static Mat4D RotateX(double radians)
        {
            GuardFinite(radians, nameof(radians));
            var c = System.Math.Cos(radians);
            var s = System.Math.Sin(radians);

            return new Mat4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, c, -s, 0.0,
                0.0, s, c, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Z축 회전 행렬을 생성합니다.
        /// </summary>
        public static Mat4D RotateZ(double radians)
        {
            GuardFinite(radians, nameof(radians));
            var c = System.Math.Cos(radians);
            var s = System.Math.Sin(radians);

            return new Mat4D(
                c, -s, 0.0, 0.0,
                s, c, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// X축 이동 행렬을 생성합니다.
        /// </summary>
        public static Mat4D TranslateX(double distance)
        {
            GuardFinite(distance, nameof(distance));
            return new Mat4D(
                1.0, 0.0, 0.0, distance,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// Z축 이동 행렬을 생성합니다.
        /// </summary>
        public static Mat4D TranslateZ(double distance)
        {
            GuardFinite(distance, nameof(distance));
            return new Mat4D(
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, distance,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Mat4D other)
        {
            return Equals(other, DefaultTolerance);
        }

        /// <summary>
        /// 지정 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Mat4D other, double tolerance)
        {
            if (tolerance < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance));
            }

            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
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
            return obj is Mat4D other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    hash.Add(System.Math.Round(this[row, col], 8));
                }
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(Mat4D left, Mat4D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 기본 허용 오차로 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(Mat4D left, Mat4D right)
        {
            return !left.Equals(right);
        }

        private static double DotRowCol(Mat4D a, int row, Mat4D b, int col)
        {
            return (a[row, 0] * b[0, col])
                + (a[row, 1] * b[1, col])
                + (a[row, 2] * b[2, col])
                + (a[row, 3] * b[3, col]);
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
