/// <ai_context>
/// Component: Types
/// Responsibility: 엔드이펙터의 위치와 회전을 포함하는 불변 포즈(Pose) 타입 정의.
/// Dependencies: Math (Pure C#)
/// Quality Gate: EditMode Tests (PoseTests.cs)
/// Navigation: Assets/Scripts/Types/AGENTS.md
/// </ai_context>
using System;
using KineTutor3D.Math;

namespace KineTutor3D.Types
{
    /// <summary>
    /// 엔드이펙터 위치와 회전을 묶은 불변 포즈입니다.
    /// </summary>
    public readonly struct Pose : IEquatable<Pose>
    {
        /// <summary>
        /// 위치 벡터입니다.
        /// </summary>
        public Vec3D Position { get; }

        /// <summary>
        /// 회전 행렬입니다.
        /// </summary>
        public Mat3D Rotation { get; }

        /// <summary>
        /// 포즈를 생성합니다.
        /// </summary>
        public Pose(Vec3D position, Mat3D rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// 값 동일 여부를 비교합니다.
        /// </summary>
        public bool Equals(Pose other)
        {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation);
        }

        /// <summary>
        /// 값 동일 여부를 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Pose other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation);
        }

        /// <summary>
        /// 동일 여부를 비교합니다.
        /// </summary>
        public static bool operator ==(Pose left, Pose right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 비동일 여부를 비교합니다.
        /// </summary>
        public static bool operator !=(Pose left, Pose right)
        {
            return !left.Equals(right);
        }
    }
}
