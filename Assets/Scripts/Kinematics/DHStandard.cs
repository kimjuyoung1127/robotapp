// Folder: Kinematics - DH parameter and FK algorithms; no UnityEngine references.
using System;
using KineTutor3D.Math;
using KineTutor3D.Types;

namespace KineTutor3D.Kinematics
{
    /// <summary>
    /// 표준 DH 공식을 사용해 단일 링크 변환 행렬 A_i를 계산합니다.
    /// A_i = Rz(theta) * Tz(d) * Tx(a) * Rx(alpha)
    /// </summary>
    public static class DHStandard
    {
        /// <summary>
        /// DH 링크와 관절값으로 변환 행렬을 계산합니다.
        /// Revolute는 theta, Prismatic은 d에 관절값을 반영합니다.
        /// </summary>
        public static Mat4D ComputeA(DHLink link, double jointValue)
        {
            GuardFinite(jointValue, nameof(jointValue));

            var theta = link.JointType == JointType.Revolute
                ? link.Theta + jointValue
                : link.Theta;

            var d = link.JointType == JointType.Prismatic
                ? link.D + jointValue
                : link.D;

            var a = link.A;
            var alpha = link.Alpha;

            var ct = System.Math.Cos(theta);
            var st = System.Math.Sin(theta);
            var ca = System.Math.Cos(alpha);
            var sa = System.Math.Sin(alpha);

            return new Mat4D(
                ct, -st * ca, st * sa, a * ct,
                st, ct * ca, -ct * sa, a * st,
                0.0, sa, ca, d,
                0.0, 0.0, 0.0, 1.0);
        }

        /// <summary>
        /// 관절값 0으로 링크 변환 행렬을 계산합니다.
        /// </summary>
        public static Mat4D ComputeA(DHLink link)
        {
            return ComputeA(link, 0.0);
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
