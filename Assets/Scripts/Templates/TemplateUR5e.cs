// Folder: Templates - Robot configuration templates; no UnityEngine references.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// Universal Robots UR5e 6축 협동로봇 템플릿을 제공합니다.
    /// DH 파라미터는 Universal Robots 공식 사양(Standard DH)에 기반합니다.
    /// </summary>
    public static class TemplateUR5e
    {
        public const string Name = "UR5e";
        private const double Deg2Rad = System.Math.PI / 180.0;

        /// <summary>
        /// UR5e 기본 DH 파라미터로 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0,  0.1625,  0.0,      System.Math.PI / 2,  JointType.Revolute),
                new DHLink(0.0,  0.0,    -0.425,    0.0,                  JointType.Revolute),
                new DHLink(0.0,  0.0,    -0.3922,   0.0,                  JointType.Revolute),
                new DHLink(0.0,  0.1333,  0.0,      System.Math.PI / 2,  JointType.Revolute),
                new DHLink(0.0,  0.0997,  0.0,     -System.Math.PI / 2,  JointType.Revolute),
                new DHLink(0.0,  0.0996,  0.0,      0.0,                  JointType.Revolute),
            };

            var limits = new[]
            {
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
                new JointLimit(-180.0 * Deg2Rad, 180.0 * Deg2Rad),
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
                new JointLimit(-360.0 * Deg2Rad, 360.0 * Deg2Rad),
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
