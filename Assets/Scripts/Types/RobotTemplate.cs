// Folder: Types - Domain value types; no UnityEngine references.
using System;

namespace KineTutor3D.Types
{
    /// <summary>
    /// 관절 제한 범위를 표현하는 불변 구조체입니다.
    /// </summary>
    public readonly struct JointLimit : IEquatable<JointLimit>
    {
        /// <summary>
        /// 최소값입니다.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// 최대값입니다.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// 관절 제한을 생성합니다.
        /// </summary>
        public JointLimit(double min, double max)
        {
            GuardFinite(min, nameof(min));
            GuardFinite(max, nameof(max));

            if (max < min)
            {
                throw new ArgumentException("최대값은 최소값보다 크거나 같아야 합니다.", nameof(max));
            }

            Min = min;
            Max = max;
        }

        /// <summary>
        /// 값 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(JointLimit other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        /// <summary>
        /// 값 동일 여부를 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is JointLimit other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        /// <summary>
        /// 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(JointLimit left, JointLimit right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(JointLimit left, JointLimit right)
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

    /// <summary>
    /// 로봇 링크/관절 제한을 묶은 불변 템플릿입니다.
    /// </summary>
    public sealed class RobotTemplate
    {
        private readonly DHLink[] links;
        private readonly JointLimit[] jointLimits;

        /// <summary>
        /// 템플릿 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 자유도(DOF)입니다.
        /// </summary>
        public int Dof => links.Length;

        /// <summary>
        /// 템플릿을 생성합니다.
        /// </summary>
        public RobotTemplate(string name, DHLink[] links, JointLimit[] jointLimits)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("템플릿 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            if (jointLimits == null)
            {
                throw new ArgumentNullException(nameof(jointLimits));
            }

            if (links.Length == 0)
            {
                throw new ArgumentException("링크는 최소 1개 이상이어야 합니다.", nameof(links));
            }

            if (links.Length != jointLimits.Length)
            {
                throw new ArgumentException("링크와 관절 제한 개수는 같아야 합니다.", nameof(jointLimits));
            }

            Name = name;
            this.links = (DHLink[])links.Clone();
            this.jointLimits = (JointLimit[])jointLimits.Clone();
        }

        /// <summary>
        /// 인덱스의 링크를 반환합니다.
        /// </summary>
        public DHLink GetLink(int index)
        {
            return links[index];
        }

        /// <summary>
        /// 인덱스의 관절 제한을 반환합니다.
        /// </summary>
        public JointLimit GetJointLimit(int index)
        {
            return jointLimits[index];
        }

        /// <summary>
        /// 링크 배열 복사본을 반환합니다.
        /// </summary>
        public DHLink[] GetLinks()
        {
            return (DHLink[])links.Clone();
        }

        /// <summary>
        /// 관절 제한 배열 복사본을 반환합니다.
        /// </summary>
        public JointLimit[] GetJointLimits()
        {
            return (JointLimit[])jointLimits.Clone();
        }
    }
}
