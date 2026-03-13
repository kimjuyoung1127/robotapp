// Folder: Kinematics - DH parameter and FK algorithms; no UnityEngine references.
using System;
using KineTutor3D.Math;
using KineTutor3D.Types;

namespace KineTutor3D.Kinematics
{
    /// <summary>
    /// DH 링크 체인의 누적 변환과 엔드이펙터 포즈를 계산합니다.
    /// </summary>
    public static class ForwardKinematics
    {
        /// <summary>
        /// 각 링크까지의 누적 변환 T0i 배열을 반환합니다.
        /// </summary>
        public static Mat4D[] ComputeAll(DHLink[] links, double[] jointValues)
        {
            ValidateInputs(links, jointValues);

            var cumulative = Mat4D.Identity;
            var transforms = new Mat4D[links.Length];

            for (var i = 0; i < links.Length; i++)
            {
                var ai = DHStandard.ComputeA(links[i], jointValues[i]);
                cumulative = cumulative * ai;
                transforms[i] = cumulative;
            }

            return transforms;
        }

        /// <summary>
        /// 최종 누적 변환 T0n을 반환합니다.
        /// </summary>
        public static Mat4D ComputeEndEffectorTransform(DHLink[] links, double[] jointValues)
        {
            var all = ComputeAll(links, jointValues);
            return all[all.Length - 1];
        }

        /// <summary>
        /// 엔드이펙터 포즈(위치/회전)를 반환합니다.
        /// </summary>
        public static Pose ComputePose(DHLink[] links, double[] jointValues)
        {
            var t0n = ComputeEndEffectorTransform(links, jointValues);
            return new Pose(t0n.ExtractPosition(), t0n.ExtractRotation());
        }

        private static void ValidateInputs(DHLink[] links, double[] jointValues)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            if (jointValues == null)
            {
                throw new ArgumentNullException(nameof(jointValues));
            }

            if (links.Length == 0)
            {
                throw new ArgumentException("링크는 최소 1개 이상이어야 합니다.", nameof(links));
            }

            if (links.Length != jointValues.Length)
            {
                throw new ArgumentException("링크 개수와 관절값 개수는 같아야 합니다.", nameof(jointValues));
            }

            for (var i = 0; i < jointValues.Length; i++)
            {
                if (double.IsNaN(jointValues[i]) || double.IsInfinity(jointValues[i]))
                {
                    throw new ArgumentException($"유효하지 않은 관절값입니다: jointValues[{i}]={jointValues[i]}", nameof(jointValues));
                }
            }
        }
    }
}
