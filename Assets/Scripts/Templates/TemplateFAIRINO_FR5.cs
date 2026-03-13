// Folder: Templates - Robot configuration templates; no UnityEngine references.
using System;
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// FAIRINO FR5 6축 산업로봇 템플릿을 제공합니다.
    /// DH 파라미터는 FAIRINO 공식 사양에 기반합니다.
    /// </summary>
    public static class TemplateFAIRINO_FR5
    {
        public const string Name = "FAIRINO_FR5";
        private const double Deg2Rad = System.Math.PI / 180.0;

        /// <summary>
        /// FAIRINO FR5 기본 DH 파라미터로 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.4045, 0.0,    -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.0,    0.3225,  0.0,                 JointType.Revolute),
                new DHLink(0.0, 0.0,    0.0,    -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.3685, 0.0,     System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.0,    0.0,    -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.1578, 0.0,     0.0,                 JointType.Revolute),
            };

            var limits = new[]
            {
                new JointLimit(-175.0 * Deg2Rad, 175.0 * Deg2Rad),
                new JointLimit(-265.0 * Deg2Rad,  85.0 * Deg2Rad),
                new JointLimit(-162.0 * Deg2Rad, 162.0 * Deg2Rad),
                new JointLimit(-265.0 * Deg2Rad,  85.0 * Deg2Rad),
                new JointLimit(-175.0 * Deg2Rad, 175.0 * Deg2Rad),
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
            };

            return new RobotTemplate(Name, links, limits);
        }

        /// <summary>
        /// JSON 설정에서 DH 파라미터를 읽어 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate CreateFromConfig(
            double[][] dhParams, double[][] jointLimitsMinMaxDeg)
        {
            if (dhParams == null) throw new ArgumentNullException(nameof(dhParams));
            if (jointLimitsMinMaxDeg == null) throw new ArgumentNullException(nameof(jointLimitsMinMaxDeg));
            if (dhParams.Length != jointLimitsMinMaxDeg.Length)
                throw new ArgumentException("DH 파라미터와 관절 제한 수가 일치하지 않습니다.");

            int dof = dhParams.Length;
            var links = new DHLink[dof];
            var limits = new JointLimit[dof];

            for (int i = 0; i < dof; i++)
            {
                var p = dhParams[i];
                if (p == null || p.Length < 4)
                    throw new ArgumentException($"DH 파라미터 [{i}]에 4개 값이 필요합니다.");

                links[i] = new DHLink(p[0], p[1], p[2], p[3], JointType.Revolute);

                var lim = jointLimitsMinMaxDeg[i];
                if (lim == null || lim.Length < 2)
                    throw new ArgumentException($"관절 제한 [{i}]에 min/max 2개 값이 필요합니다.");

                limits[i] = new JointLimit(lim[0] * Deg2Rad, lim[1] * Deg2Rad);
            }

            return new RobotTemplate(Name, links, limits);
        }
    }
}
