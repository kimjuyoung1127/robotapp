// Folder: Templates - Robot configuration templates; no UnityEngine references.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// 2자유도 RR(회전-회전) 로봇 템플릿을 제공합니다.
    /// </summary>
    public static class Template2DOF_RR
    {
        /// <summary>
        /// 템플릿 이름입니다.
        /// </summary>
        public const string Name = "2DOF_RR";

        /// <summary>
        /// 기본 2DOF RR 로봇 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute)
            };

            var limits = new[]
            {
                new JointLimit(-System.Math.PI, System.Math.PI),
                new JointLimit(-System.Math.PI, System.Math.PI)
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
