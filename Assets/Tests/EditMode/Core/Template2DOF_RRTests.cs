// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// Template2DOF_RR 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Templates;
using KineTutor3D.Types;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 2DOF RR 템플릿의 기본 정의를 검증합니다.
    /// </summary>
    public class Template2DOF_RRTests
    {
        [Test]
        public void Create_ReturnsExpectedDofAndName()
        {
            var template = Template2DOF_RR.Create();

            Assert.AreEqual("2DOF_RR", template.Name);
            Assert.AreEqual(2, template.Dof);
        }

        [Test]
        public void Create_ReturnsExpectedLinkDefinitions()
        {
            var template = Template2DOF_RR.Create();
            var links = template.GetLinks();

            Assert.AreEqual(2, links.Length);

            Assert.AreEqual(JointType.Revolute, links[0].JointType);
            Assert.AreEqual(0.0, links[0].Theta, TestTolerances.Math);
            Assert.AreEqual(0.0, links[0].D, TestTolerances.Math);
            Assert.AreEqual(1.0, links[0].A, TestTolerances.Math);
            Assert.AreEqual(0.0, links[0].Alpha, TestTolerances.Math);

            Assert.AreEqual(JointType.Revolute, links[1].JointType);
            Assert.AreEqual(0.0, links[1].Theta, TestTolerances.Math);
            Assert.AreEqual(0.0, links[1].D, TestTolerances.Math);
            Assert.AreEqual(1.0, links[1].A, TestTolerances.Math);
            Assert.AreEqual(0.0, links[1].Alpha, TestTolerances.Math);
        }

        [Test]
        public void Create_ReturnsExpectedJointLimits()
        {
            var template = Template2DOF_RR.Create();
            var limits = template.GetJointLimits();

            Assert.AreEqual(2, limits.Length);

            Assert.AreEqual(-System.Math.PI, limits[0].Min, TestTolerances.Math);
            Assert.AreEqual(System.Math.PI, limits[0].Max, TestTolerances.Math);
            Assert.AreEqual(-System.Math.PI, limits[1].Min, TestTolerances.Math);
            Assert.AreEqual(System.Math.PI, limits[1].Max, TestTolerances.Math);
        }
    }
}
