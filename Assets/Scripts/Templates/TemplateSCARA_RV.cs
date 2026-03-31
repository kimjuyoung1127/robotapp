// Folder: Templates - Robot configuration templates; no UnityEngine references.
// SCARA RV 로봇 템플릿을 정의합니다.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// realvirtual donor와 맞물리는 4DOF SCARA 템플릿을 제공합니다.
    /// </summary>
    public static class TemplateSCARA_RV
    {
        public const string Name = "SCARA_RV";

        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.40, 0.90, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.00, 0.75, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.00, 0.25, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.10, 0.10, 0.0, JointType.Revolute)
            };

            var limits = new[]
            {
                new JointLimit(-System.Math.PI, System.Math.PI),
                new JointLimit(-160.0 * System.Math.PI / 180.0, 160.0 * System.Math.PI / 180.0),
                new JointLimit(-150.0 * System.Math.PI / 180.0, 150.0 * System.Math.PI / 180.0),
                new JointLimit(-180.0 * System.Math.PI / 180.0, 180.0 * System.Math.PI / 180.0)
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
