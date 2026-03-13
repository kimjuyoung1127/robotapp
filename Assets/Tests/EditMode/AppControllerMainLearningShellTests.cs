using System.Reflection;
using KineTutor3D.App;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// MainLearningShell 활성 조건이 기존 학습 트랙을 침범하지 않는지 검증합니다.
    /// </summary>
    public class AppControllerMainLearningShellTests
    {
        [Test]
        public void ShouldActivateMainLearningShell_MainSceneAndCoreTrack_ReturnsTrue()
        {
            var result = InvokeShouldActivateMainLearningShell(SceneId.Main, StepProgressSaver.CoreKinematicsTrack);
            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldActivateMainLearningShell_MainSceneAndMathReadinessTrack_ReturnsFalse()
        {
            var result = InvokeShouldActivateMainLearningShell(SceneId.Main, StepProgressSaver.MathReadinessTrack);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldActivateMainLearningShell_MainSceneAndPreKinematicsTrack_ReturnsFalse()
        {
            var result = InvokeShouldActivateMainLearningShell(SceneId.Main, StepProgressSaver.PreKinematicsTrack);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldActivateMainLearningShell_NonMainScene_ReturnsFalse()
        {
            var result = InvokeShouldActivateMainLearningShell(SceneId.Home, StepProgressSaver.CoreKinematicsTrack);
            Assert.That(result, Is.False);
        }

        private static bool InvokeShouldActivateMainLearningShell(SceneId sceneId, string track)
        {
            var method = typeof(AppController).GetMethod("ShouldActivateMainLearningShell", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, new object[] { sceneId, track });
        }
    }
}
