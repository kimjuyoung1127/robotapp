// Folder: Types - Domain value types; no UnityEngine references.
using System;

namespace KineTutor3D.Types
{
    /// <summary>
    /// 표준 DH 파라미터 한 링크를 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct DHLink : IEquatable<DHLink>
    {
        /// <summary>
        /// 관절 각도(theta) 기본값입니다.
        /// </summary>
        public double Theta { get; }

        /// <summary>
        /// 링크 오프셋(d)입니다.
        /// </summary>
        public double D { get; }

        /// <summary>
        /// 링크 길이(a)입니다.
        /// </summary>
        public double A { get; }

        /// <summary>
        /// 링크 비틀림(alpha)입니다.
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// 관절 타입입니다.
        /// </summary>
        public JointType JointType { get; }

        /// <summary>
        /// DH 링크를 생성합니다.
        /// </summary>
        public DHLink(double theta, double d, double a, double alpha, JointType jointType)
        {
            GuardFinite(theta, nameof(theta));
            GuardFinite(d, nameof(d));
            GuardFinite(a, nameof(a));
            GuardFinite(alpha, nameof(alpha));

            Theta = theta;
            D = d;
            A = a;
            Alpha = alpha;
            JointType = jointType;
        }

        /// <summary>
        /// DH 링크를 생성합니다. 기본 관절 타입은 Revolute입니다.
        /// </summary>
        public DHLink(double theta, double d, double a, double alpha)
            : this(theta, d, a, alpha, JointType.Revolute)
        {
        }

        /// <summary>
        /// 동일한 값을 가지는지 비교합니다.
        /// </summary>
        public bool Equals(DHLink other)
        {
            return Theta.Equals(other.Theta)
                && D.Equals(other.D)
                && A.Equals(other.A)
                && Alpha.Equals(other.Alpha)
                && JointType == other.JointType;
        }

        /// <summary>
        /// 동일한 값을 가지는지 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DHLink other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Theta, D, A, Alpha, JointType);
        }

        /// <summary>
        /// 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(DHLink left, DHLink right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(DHLink left, DHLink right)
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
