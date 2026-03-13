// Folder: Templates - Robot configuration templates; no UnityEngine references.
using KineTutor3D.Types;

namespace KineTutor3D.Templates
{
    /// <summary>
    /// 지정된 DOF로 기본 DH 파라미터 동적 템플릿을 생성합니다.
    /// </summary>
    public static class CustomTemplateBuilder
    {
        /// <summary>
        /// 기본값(a=1.0, d=0, alpha=0, theta=0, Revolute, limit=±π)으로 N-DOF 템플릿을 생성합니다.
        /// </summary>
        public static RobotTemplate Create(int dof)
        {
            if (dof < 1)
            {
                throw new System.ArgumentException("DOF는 1 이상이어야 합니다.", nameof(dof));
            }

            if (dof > 12)
            {
                throw new System.ArgumentException("DOF는 12 이하여야 합니다.", nameof(dof));
            }

            var links = new DHLink[dof];
            var limits = new JointLimit[dof];
            var piLimit = new JointLimit(-System.Math.PI, System.Math.PI);

            for (var i = 0; i < dof; i++)
            {
                links[i] = new DHLink(0.0, 0.0, 1.0, 0.0, JointType.Revolute);
                limits[i] = piLimit;
            }

            return new RobotTemplate($"Custom_{dof}DOF", links, limits);
        }
    }
}
