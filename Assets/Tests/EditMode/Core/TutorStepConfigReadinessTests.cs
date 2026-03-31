// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// TutorStepConfigReadiness 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI.Data;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class TutorStepConfigReadinessTests
    {
        [Test]
        public void Defaults_ExposeMathReadinessFields()
        {
            var config = ScriptableObject.CreateInstance<TutorStepConfig>();

            Assert.That(config.mathReadinessMode, Is.False);
            Assert.That(config.showMathReadinessPanel, Is.False);
            Assert.That(config.mathReadinessContent, Is.EqualTo(MathReadinessContent.None));
            Assert.That(config.interactiveJointCount, Is.EqualTo(0));
            Assert.That(config.warmupChoicesKo, Is.Not.Null);
            Assert.That(config.correctionMessagesKo, Is.Not.Null);
            Assert.That(config.readinessQuestions, Is.Not.Null);
        }
    }
}
