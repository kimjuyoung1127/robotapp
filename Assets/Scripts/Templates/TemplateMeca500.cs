// Folder: Templates - Robot configuration templates; no UnityEngine references.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// Mecademic Meca500 6축 협동로봇 템플릿을 제공합니다.
    /// DH 파라미터는 Meca500 URDF joint origin 값 기반 표준 DH 근사치입니다.
    /// </summary>
    public static class TemplateMeca500
    {
        public const string Name = "MECA500";

        /// <summary>
        /// Meca500 기본 DH 파라미터로 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.091,  0.0,    -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.044,  0.135,   0.0,                JointType.Revolute),
                new DHLink(0.0, 0.0,    0.0,    -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.038,  0.0,     System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.0,    0.120,  -System.Math.PI / 2, JointType.Revolute),
                new DHLink(0.0, 0.070,  0.0,     0.0,                JointType.Revolute),
            };

            var deg2Rad = System.Math.PI / 180.0;
            var limits = new[]
            {
                new JointLimit(-175.0 * deg2Rad, 175.0 * deg2Rad),
                new JointLimit(-70.0  * deg2Rad, 90.0  * deg2Rad),
                new JointLimit(-135.0 * deg2Rad, 70.0  * deg2Rad),
                new JointLimit(-170.0 * deg2Rad, 170.0 * deg2Rad),
                new JointLimit(-115.0 * deg2Rad, 115.0 * deg2Rad),
                new JointLimit(-180.0 * deg2Rad, 180.0 * deg2Rad),
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
