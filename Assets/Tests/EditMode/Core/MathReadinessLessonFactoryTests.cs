// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// MathReadinessLessonFactory 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class MathReadinessLessonFactoryTests
    {
        [Test]
        public void CreateLessons_ReturnsFourConfigs()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            Assert.That(lessons, Is.Not.Null);
            Assert.That(lessons.Length, Is.EqualTo(MathReadinessLessonFactory.LessonCount));
        }

        [Test]
        public void CreateLessons_AllConfigsAreMathReadinessMode()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            for (var i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].mathReadinessMode, Is.True);
                Assert.That(lessons[i].showMathReadinessPanel, Is.True);
                Assert.That(lessons[i].showFormula, Is.False);
                Assert.That(lessons[i].showMatrices, Is.False);
                Assert.That(lessons[i].showDHTable, Is.False);
                Assert.That(lessons[i].showPlainLanguage, Is.True);
                Assert.That(lessons[i].showRightPanel, Is.False);
            }
        }

        [Test]
        public void CreateLessons_InteractiveJointCounts_AreExpected()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            Assert.That(lessons[0].interactiveJointCount, Is.EqualTo(1));
            Assert.That(lessons[1].interactiveJointCount, Is.EqualTo(1));
            Assert.That(lessons[2].interactiveJointCount, Is.EqualTo(1));
            Assert.That(lessons[3].interactiveJointCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateLessons_KeyLessonTexts_AreFilled()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            for (var i = 0; i < lessons.Length; i++)
            {
                Assert.That(lessons[i].objectiveKo, Is.Not.Empty);
                Assert.That(lessons[i].hintKo, Is.Not.Empty);
                Assert.That(lessons[i].successToastKo, Is.Not.Empty);
                Assert.That(lessons[i].warmupPromptKo, Is.Empty);
                Assert.That(lessons[i].warmupChoicesKo.Length, Is.EqualTo(0));
                Assert.That(lessons[i].readinessQuestions.Length, Is.GreaterThan(0));
                for (var j = 0; j < lessons[i].readinessQuestions.Length; j++)
                {
                    Assert.That(lessons[i].readinessQuestions[j].promptKo, Is.Not.Empty);
                    Assert.That(lessons[i].readinessQuestions[j].manipulationInstructionKo, Is.Not.Empty);
                }
            }
        }

        [Test]
        public void CreateLessons_GateIds_MatchExpectedTargets()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            CollectionAssert.Contains(GetTargets(lessons[0]), "math_m0_correct");
            CollectionAssert.Contains(GetTargets(lessons[1]), "math_m1_zero_correct");
            CollectionAssert.Contains(GetTargets(lessons[1]), "math_m1_ninety_correct");
            CollectionAssert.Contains(GetTargets(lessons[2]), "math_m2_correct");
            CollectionAssert.Contains(GetTargets(lessons[3]), "math_m3_correct");
        }

        [Test]
        public void CreateLessons_AllQuestionsRequireManipulationFirst()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            for (var i = 0; i < lessons.Length; i++)
            {
                for (var j = 0; j < lessons[i].readinessQuestions.Length; j++)
                {
                    var question = lessons[i].readinessQuestions[j];
                    Assert.That(question.requiresManipulationFirst, Is.True, $"Lesson M{i} question {j} should require manipulation first");
                    Assert.That(float.IsNaN(question.targetAngleDeg), Is.False, $"Lesson M{i} question {j} should define targetAngleDeg");
                    Assert.That(question.targetReachGateId, Is.Not.Empty, $"Lesson M{i} question {j} should define targetReachGateId");
                    Assert.That(question.manipulationInstructionKo, Is.Not.Empty, $"Lesson M{i} question {j} should define manipulationInstructionKo");
                }
            }
        }

        [Test]
        public void CreateLessons_UseSliderReachTargetGate()
        {
            var lessons = MathReadinessLessonFactory.CreateLessons();

            Assert.That(HasInteractionTarget(lessons[0], InteractionType.SliderReachTarget, "math_m0_pose_ready"), Is.True);
            Assert.That(HasInteractionTarget(lessons[1], InteractionType.SliderReachTarget, "math_m1_zero_pose_ready"), Is.True);
            Assert.That(HasInteractionTarget(lessons[1], InteractionType.SliderReachTarget, "math_m1_ninety_pose_ready"), Is.True);
            Assert.That(HasInteractionTarget(lessons[2], InteractionType.SliderReachTarget, "math_m2_pose_ready"), Is.True);
            Assert.That(HasInteractionTarget(lessons[3], InteractionType.SliderReachTarget, "math_m3_pose_ready"), Is.True);
        }

        private static string[] GetTargets(TutorStepConfig config)
        {
            var result = new string[config.conditions.Length];
            for (var i = 0; i < config.conditions.Length; i++)
            {
                result[i] = config.conditions[i].targetId;
            }

            return result;
        }

        private static bool HasInteractionTarget(TutorStepConfig config, InteractionType type, string targetId)
        {
            for (var i = 0; i < config.conditions.Length; i++)
            {
                if (config.conditions[i].interactionType == type && config.conditions[i].targetId == targetId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
