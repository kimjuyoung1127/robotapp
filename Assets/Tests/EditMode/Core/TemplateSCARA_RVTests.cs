// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// TemplateSCARA_RV 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Templates;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class TemplateSCARA_RVTests
    {
        [Test]
        public void Create_Returns4DofTemplate()
        {
            var template = TemplateSCARA_RV.Create();

            Assert.That(template, Is.Not.Null);
            Assert.That(template.Name, Is.EqualTo("SCARA_RV"));
            Assert.That(template.Dof, Is.EqualTo(4));
            Assert.That(template.GetLinks().Length, Is.EqualTo(4));
            Assert.That(template.GetJointLimits().Length, Is.EqualTo(4));
        }

        [Test]
        public void Create_AllJointLimitsAreFinite()
        {
            var template = TemplateSCARA_RV.Create();
            var limits = template.GetJointLimits();

            for (var i = 0; i < limits.Length; i++)
            {
                Assert.That(double.IsNaN(limits[i].Min), Is.False);
                Assert.That(double.IsNaN(limits[i].Max), Is.False);
                Assert.That(limits[i].Max, Is.GreaterThanOrEqualTo(limits[i].Min));
            }
        }
    }
}
