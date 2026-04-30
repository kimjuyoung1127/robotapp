/// <ai_context>
/// Component: Math
/// Responsibility: double 기반 3차원 벡터(Vec3D) 불변 구조체 및 산술 연산 정의.
/// Dependencies: None (Pure C#)
/// Quality Gate: EditMode Tests (Vec3DTests.cs)
/// Navigation: Assets/Scripts/Math/AGENTS.md
/// </ai_context>
using System;
using System.Globalization;

namespace KineTutor3D.Math
{
    /// <summary>
    /// double 기반 3차원 벡터를 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct Vec3D : IEquatable<Vec3D>
    {
        /// <summary>
        /// Equals 비교 기본 허용 오차입니다.
        /// </summary>
        public const double DefaultTolerance = 1e-10;

        /// <summary>
        /// X 성분입니다.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y 성분입니다.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Z 성분입니다.
        /// </summary>
        public double Z { get; }

        /// <summary>
        /// 영벡터입니다.
        /// </summary>
        public static Vec3D Zero => new(0.0, 0.0, 0.0);

        /// <summary>
        /// X축 단위 벡터입니다.
        /// </summary>
        public static Vec3D UnitX => new(1.0, 0.0, 0.0);

        /// <summary>
        /// Y축 단위 벡터입니다.
        /// </summary>
        public static Vec3D UnitY => new(0.0, 1.0, 0.0);

        /// <summary>
        /// Z축 단위 벡터입니다.
        /// </summary>
        public static Vec3D UnitZ => new(0.0, 0.0, 1.0);

        /// <summary>
        /// 벡터를 생성합니다.
        /// </summary>
        public Vec3D(double x, double y, double z)
        {
            GuardFinite(x, nameof(x));
            GuardFinite(y, nameof(y));
            GuardFinite(z, nameof(z));

            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// 벡터 크기를 반환합니다.
        /// </summary>
        public double Magnitude()
        {
            return System.Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// 정규화된 벡터를 반환합니다.
        /// </summary>
        public Vec3D Normalized()
        {
            var mag = Magnitude();
            if (mag < DefaultTolerance)
            {
                throw new InvalidOperationException("영벡터는 정규화할 수 없습니다.");
            }

            return this * (1.0 / mag);
        }

        /// <summary>
        /// 내적을 계산합니다.
        /// </summary>
        public double Dot(Vec3D other)
        {
            return (X * other.X) + (Y * other.Y) + (Z * other.Z);
        }

        /// <summary>
        /// 외적을 계산합니다.
        /// </summary>
        public Vec3D Cross(Vec3D other)
        {
            return new Vec3D(
                (Y * other.Z) - (Z * other.Y),
                (Z * other.X) - (X * other.Z),
                (X * other.Y) - (Y * other.X));
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Vec3D other)
        {
            return Equals(other, DefaultTolerance);
        }

        /// <summary>
        /// 지정된 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Vec3D other, double tolerance)
        {
            if (tolerance < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(tolerance));
            }

            return System.Math.Abs(X - other.X) < tolerance
                && System.Math.Abs(Y - other.Y) < tolerance
                && System.Math.Abs(Z - other.Z) < tolerance;
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Vec3D other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                System.Math.Round(X, 8),
                System.Math.Round(Y, 8),
                System.Math.Round(Z, 8));
        }

        /// <summary>
        /// 벡터 덧셈입니다.
        /// </summary>
        public static Vec3D operator +(Vec3D a, Vec3D b)
        {
            return new Vec3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// 벡터 뺄셈입니다.
        /// </summary>
        public static Vec3D operator -(Vec3D a, Vec3D b)
        {
            return new Vec3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// 단항 음수 연산입니다.
        /// </summary>
        public static Vec3D operator -(Vec3D v)
        {
            return new Vec3D(-v.X, -v.Y, -v.Z);
        }

        /// <summary>
        /// 벡터-스칼라 곱입니다.
        /// </summary>
        public static Vec3D operator *(Vec3D v, double scalar)
        {
            return new Vec3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        /// <summary>
        /// 스칼라-벡터 곱입니다.
        /// </summary>
        public static Vec3D operator *(double scalar, Vec3D v)
        {
            return v * scalar;
        }

        /// <summary>
        /// 기본 허용 오차로 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(Vec3D a, Vec3D b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// 기본 허용 오차로 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(Vec3D a, Vec3D b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"({X.ToString("F4", CultureInfo.InvariantCulture)}, {Y.ToString("F4", CultureInfo.InvariantCulture)}, {Z.ToString("F4", CultureInfo.InvariantCulture)})";
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
