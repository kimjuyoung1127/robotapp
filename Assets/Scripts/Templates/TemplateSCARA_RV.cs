using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// 3자유도 SCARA(수평 다관절) 로봇 템플릿을 제공합니다.
    /// 2개 Revolute + 1개 Prismatic 관절 구성.
    /// DH 파라미터 출처: docs/ref/test-reference-values.md Section 7.
    /// </summary>
    public static class TemplateSCARA_RV
    {
        /// <summary>
        /// 템플릿 이름입니다.
        /// </summary>
        public const string Name = "SCARA_RV";

        /// <summary>
        /// 기본 3DOF SCARA 로봇 템플릿을 생성합니다.
        /// a₁ = a₂ = 0.5 m, α₂ = π (Z축 반전).
        /// </summary>
        public static RobotTemplate Create()
        {
            var links = new[]
            {
                new DHLink(0.0, 0.0, 0.5, 0.0, JointType.Revolute),
                new DHLink(0.0, 0.0, 0.5, System.Math.PI, JointType.Revolute),
                new DHLink(0.0, 0.0, 0.0, 0.0, JointType.Prismatic)
            };

            var limits = new[]
            {
                new JointLimit(-System.Math.PI, System.Math.PI),
                new JointLimit(-System.Math.PI, System.Math.PI),
                new JointLimit(0.0, 1.0)
            };

            return new RobotTemplate(Name, links, limits);
        }
    }
}
