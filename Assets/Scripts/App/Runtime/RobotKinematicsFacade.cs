// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;

namespace KineTutor3D.App
{
    /// <summary>
    /// 임의의 RobotTemplate을 받아 DH 파라미터 기반 FK 계산을 수행하는 일반 facade입니다.
    /// DH 편집과 관절값 변경에 따른 FK 결과를 이벤트로 전파합니다.
    /// </summary>
    public sealed class RobotKinematicsFacade
    {
        private const double Deg2Rad = System.Math.PI / 180.0;

        private readonly RobotTemplate baseTemplate;
        private DHLink[] links;
        private double[] jointValuesRad;
        private Mat4D[] cumulativeTransforms;
        private Mat4D endEffectorTransform;

        /// <summary>
        /// FK가 갱신될 때 발생하는 이벤트입니다.
        /// (누적 변환 배열, EE 변환)을 전달합니다.
        /// </summary>
        public event Action<Mat4D[], Mat4D> OnKinematicsUpdated;

        /// <summary>
        /// 현재 DH 링크 배열 복사본을 반환합니다.
        /// </summary>
        public DHLink[] Links => (DHLink[])links.Clone();

        /// <summary>
        /// 현재 관절값(라디안) 복사본을 반환합니다.
        /// </summary>
        public double[] JointValuesRad => (double[])jointValuesRad.Clone();

        /// <summary>
        /// 현재 EE 변환을 반환합니다.
        /// </summary>
        public Mat4D EndEffectorTransform => endEffectorTransform;

        /// <summary>
        /// 누적 변환 배열을 반환합니다.
        /// </summary>
        public Mat4D[] CumulativeTransforms => cumulativeTransforms != null
            ? (Mat4D[])cumulativeTransforms.Clone()
            : Array.Empty<Mat4D>();

        /// <summary>
        /// 지정된 RobotTemplate으로 facade를 생성합니다.
        /// </summary>
        public RobotKinematicsFacade(RobotTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            baseTemplate = template;
            links = baseTemplate.GetLinks();
            jointValuesRad = new double[baseTemplate.Dof];
            Recompute();
        }

        /// <summary>
        /// 관절 각도를 도 단위로 설정합니다.
        /// </summary>
        public void SetJointAnglesDegrees(double[] anglesDeg)
        {
            if (anglesDeg == null || anglesDeg.Length != links.Length)
            {
                return;
            }

            for (var i = 0; i < links.Length; i++)
            {
                var rad = anglesDeg[i] * Deg2Rad;
                if (double.IsNaN(rad) || double.IsInfinity(rad))
                {
                    continue;
                }

                jointValuesRad[i] = rad;
            }

            Recompute();
        }

        /// <summary>
        /// 단일 관절 각도를 도 단위로 설정합니다.
        /// </summary>
        public void SetJointAngleDegrees(int index, double angleDeg)
        {
            if (index < 0 || index >= links.Length)
            {
                return;
            }

            var rad = angleDeg * Deg2Rad;
            if (double.IsNaN(rad) || double.IsInfinity(rad))
            {
                return;
            }

            jointValuesRad[index] = rad;
            Recompute();
        }

        /// <summary>
        /// DH 파라미터(d, a, alpha)를 변경합니다. theta는 관절값이므로 읽기 전용입니다.
        /// </summary>
        public bool SetDhParameter(int linkIndex, string paramName, double value)
        {
            if (linkIndex < 0 || linkIndex >= links.Length)
            {
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return false;
            }

            var link = links[linkIndex];
            switch (paramName)
            {
                case "d":
                    links[linkIndex] = new DHLink(link.Theta, value, link.A, link.Alpha, link.JointType);
                    break;
                case "a":
                    links[linkIndex] = new DHLink(link.Theta, link.D, value, link.Alpha, link.JointType);
                    break;
                case "alpha":
                    links[linkIndex] = new DHLink(link.Theta, link.D, link.A, value, link.JointType);
                    break;
                default:
                    return false;
            }

            Recompute();
            return true;
        }

        /// <summary>
        /// DH 파라미터를 템플릿 기본값으로 복원합니다.
        /// </summary>
        public void ResetToDefault()
        {
            links = baseTemplate.GetLinks();
            jointValuesRad = new double[baseTemplate.Dof];
            Recompute();
        }

        private void Recompute()
        {
            cumulativeTransforms = ForwardKinematics.ComputeAll(links, jointValuesRad);
            endEffectorTransform = cumulativeTransforms[cumulativeTransforms.Length - 1];
            OnKinematicsUpdated?.Invoke(cumulativeTransforms, endEffectorTransform);
        }
    }
}
