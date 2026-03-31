// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// FR5TemplatePoseCatalog 동작을 검증하는 EditMode 테스트입니다.
using NUnit.Framework;
using KineTutor3D.App;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// FR5 슬림 템플릿용 대표 포즈 카탈로그를 검증합니다.
    /// </summary>
    public class FR5TemplatePoseCatalogTests
    {
        [Test]
        public void TryGetPose_AllKnownPoses_ReturnSixJointAngles()
        {
            foreach (var poseName in FR5TemplatePoseCatalog.GetNames())
            {
                var found = FR5TemplatePoseCatalog.TryGetPose(poseName, out var jointAnglesDeg);

                Assert.That(found, Is.True, $"pose '{poseName}' should exist");
                Assert.That(jointAnglesDeg, Has.Length.EqualTo(6), $"pose '{poseName}' should have 6 joints");
            }
        }

        [Test]
        public void TryGetPose_ReturnsClone_NotSharedArray()
        {
            Assert.That(FR5TemplatePoseCatalog.TryGetPose(FR5TemplatePoseCatalog.ReadyName, out var first), Is.True);
            Assert.That(FR5TemplatePoseCatalog.TryGetPose(FR5TemplatePoseCatalog.ReadyName, out var second), Is.True);

            first[0] = 999d;

            Assert.AreNotEqual(first[0], second[0], "pose arrays should be cloned");
        }
    }
}
