// Folder: Templates - Robot configuration templates; no UnityEngine references.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// Doosan Robotics M1013 6축 협동로봇 템플릿을 제공합니다.
    /// DH 파라미터는 Doosan M1013 URDF 사양(Standard DH)에 기반합니다.
    /// </summary>
    public static class TemplateDoosanM1013
    {
        public const string Name = "DOOSAN_M1013";
        private const double Deg2Rad = System.Math.PI / 180.0;

        /// <summary>
        /// Doosan M1013 기본 DH 파라미터로 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0,  0.1525,  0.0,   -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0,  0.0345,  0.62,   0.0,                 JointType.Revolute),
                new DHLink(0.0,  0.0,     0.0,   -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0,  0.559,   0.0,    System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0,  0.0,     0.0,   -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0,  0.121,   0.0,    0.0,                 JointType.Revolute),
            };

            var limits = new[]
            {
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
                new JointLimit(-150.0 * Deg2Rad, 150.0 * Deg2Rad),
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
