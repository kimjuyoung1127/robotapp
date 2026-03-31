// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// BeginnerLessonFactory 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// BeginnerLessonFactory의 L0~L3 설정 생성을 검증합니다.
    /// </summary>
    public class BeginnerLessonFactoryTests
    {
        [Test]
        public void CreateLessons_Returns4Configs()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            Assert.That(lessons, Is.Not.Null);
            Assert.That(lessons.Length, Is.EqualTo(BeginnerLessonFactory.LessonCount));
        }

        [Test]
        public void CreateLessons_AllConfigsAreBeginnerMode()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            for (int i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].beginnerMode, Is.True, $"Lesson {i} should be beginner mode");
            }
        }

        [Test]
        public void CreateLessons_AllConfigsHaveGateConditions()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            for (int i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].conditions, Is.Not.Null, $"Lesson {i} should have conditions");
                Assert.That(lessons[i].conditions.Length, Is.GreaterThan(0), $"Lesson {i} should have at least 1 gate condition");
            }
        }

        [Test]
        public void CreateLessons_L0_HasObserveGuideContent()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            Assert.That(lessons[0].beginnerLeftContent, Is.EqualTo(BeginnerLeftContent.ObserveGuide));
            Assert.That(lessons[0].showWhyItMoved, Is.True);
            Assert.That(lessons[0].showJointHighlight, Is.True);
            Assert.That(lessons[0].showEndEffectorTrail, Is.True);
        }

        [Test]
        public void CreateLessons_L2_HasCombinationGuide()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            Assert.That(lessons[2].beginnerLeftContent, Is.EqualTo(BeginnerLeftContent.CombinationGuide));
        }

        [Test]
        public void CreateLessons_L3_HasTargetMarkers()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            Assert.That(lessons[3].showTargetMarkers, Is.True);
            Assert.That(lessons[3].beginnerLeftContent, Is.EqualTo(BeginnerLeftContent.TargetHintGuide));
        }

        [Test]
        public void CreateLessons_NamesAreL00ToL03()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            Assert.That(lessons[0].name, Is.EqualTo("L00"));
            Assert.That(lessons[1].name, Is.EqualTo("L01"));
            Assert.That(lessons[2].name, Is.EqualTo("L02"));
            Assert.That(lessons[3].name, Is.EqualTo("L03"));
        }

        [Test]
        public void CreateLessons_AllHideDHTableAndMatrices()
        {
            var lessons = BeginnerLessonFactory.CreateLessons();

            for (int i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].showDHTable, Is.False, $"Lesson {i} should hide DH table");
                Assert.That(lessons[i].showMatrices, Is.False, $"Lesson {i} should hide matrices");
                Assert.That(lessons[i].showFormula, Is.False, $"Lesson {i} should hide formula");
                Assert.That(lessons[i].showPlainLanguage, Is.True, $"Lesson {i} should show plain language");
            }
        }
    }
}
