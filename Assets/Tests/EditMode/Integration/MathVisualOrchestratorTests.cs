// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// MathVisualOrchestrator 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using KineTutor3D.Visualization;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class MathVisualOrchestratorTests
    {
        [Test]
        public void CreateLessons_AllHaveMathVisualHintsEnabled()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            for (var i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].showMathVisualHints, Is.True,
                    $"Lesson M{i} should have showMathVisualHints=true");
            }
        }

        [Test]
        public void TutorStepConfig_ShowMathVisualHints_DefaultsFalse()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<TutorStepConfig>();

            Assert.That(config.showMathVisualHints, Is.False);

            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void MathReadinessContentTheme_ReturnsDistinctColors()
        {
            var m0 = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.AngleDirection);
            var m1 = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.LengthAngleToPoint);
            var m2 = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.DiagonalIntuition);
            var m3 = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.TwoLinkComposition);

            Assert.That(m0, Is.Not.EqualTo(m1));
            Assert.That(m1, Is.Not.EqualTo(m2));
            Assert.That(m2, Is.Not.EqualTo(m3));
            Assert.That(m0, Is.Not.EqualTo(m3));
        }

        [Test]
        public void MathReadinessLessons_ContentMapping_IsCorrect()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            Assert.That(lessons[0].mathReadinessContent, Is.EqualTo(MathReadinessContent.AngleDirection));
            Assert.That(lessons[1].mathReadinessContent, Is.EqualTo(MathReadinessContent.LengthAngleToPoint));
            Assert.That(lessons[2].mathReadinessContent, Is.EqualTo(MathReadinessContent.DiagonalIntuition));
            Assert.That(lessons[3].mathReadinessContent, Is.EqualTo(MathReadinessContent.TwoLinkComposition));
        }

        [Test]
        public void AngleReferenceMarker_CanCreateVisibleReferences()
        {
            var host = new GameObject("AngleReferenceHost");
            var pivot = new GameObject("Pivot").transform;
            var marker = host.AddComponent<AngleReferenceMarker>();

            marker.SetVisible(true);
            marker.ShowMarkers(pivot, Color.cyan);

            Assert.That(host.transform.Find("RefLine_0"), Is.Not.Null);
            Assert.That(host.transform.Find("RefLine_1"), Is.Not.Null);
            Assert.That(host.transform.Find("RefLine_2"), Is.Not.Null);
            Assert.That(host.transform.Find("RefLabel_0"), Is.Not.Null);
            Assert.That(host.transform.Find("RefLabel_1"), Is.Not.Null);
            Assert.That(host.transform.Find("RefLabel_2"), Is.Not.Null);

            Object.DestroyImmediate(pivot.gameObject);
            Object.DestroyImmediate(host);
        }
    }
}
