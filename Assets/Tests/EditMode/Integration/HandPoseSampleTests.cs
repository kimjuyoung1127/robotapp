// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// HandPoseSample 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App.HandTracking;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Hand teaching input payload의 최소 파싱/정규화 규칙을 검증합니다.
    /// </summary>
    public class HandPoseSampleTests
    {
        [Test]
        public void TryParse_ClampsExpectedRanges()
        {
            const string json = "{\"seq\":3,\"tracked\":true,\"handX\":9.5,\"handY\":-3.2,\"pinch\":4.1,\"palmYaw\":270.0,\"palmPitch\":-220.0,\"confidence\":2.0}";

            var ok = HandPoseSample.TryParse(json, out var sample);

            Assert.That(ok, Is.True);
            Assert.That(sample, Is.Not.Null);
            Assert.That(sample.handX, Is.EqualTo(1f));
            Assert.That(sample.handY, Is.EqualTo(-1f));
            Assert.That(sample.pinch, Is.EqualTo(1f));
            Assert.That(sample.palmYaw, Is.EqualTo(180f));
            Assert.That(sample.palmPitch, Is.EqualTo(-180f));
            Assert.That(sample.confidence, Is.EqualTo(1f));
        }

        [Test]
        public void CreateDebugPayload_ProducesTrackedSample()
        {
            var sample = HandPoseSample.CreateDebugPayload();

            Assert.That(sample, Is.Not.Null);
            Assert.That(sample.tracked, Is.True);
            Assert.That(sample.sourceId, Is.EqualTo("debug-phone"));
            Assert.That(sample.pinch, Is.InRange(0f, 1f));
        }
    }
}
