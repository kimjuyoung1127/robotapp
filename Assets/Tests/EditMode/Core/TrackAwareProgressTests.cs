// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// TrackAwareProgress 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Validates track-aware progress persistence for Phase 5.
    /// </summary>
    public class TrackAwareProgressTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        [Test]
        public void TrackAwareKeys_StoreProgressSeparately()
        {
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.PreKinematicsTrack, 2);
            StepProgressSaver.SaveLastCompletedStep(StepProgressSaver.CoreKinematicsTrack, 5);

            Assert.That(StepProgressSaver.GetLastCompletedStep(StepProgressSaver.PreKinematicsTrack), Is.EqualTo(2));
            Assert.That(StepProgressSaver.GetLastCompletedStep(StepProgressSaver.CoreKinematicsTrack), Is.EqualTo(5));
            Assert.That(StepProgressSaver.GetResumeStep(StepProgressSaver.PreKinematicsTrack, 1), Is.EqualTo(3));
            Assert.That(StepProgressSaver.GetResumeStep(StepProgressSaver.CoreKinematicsTrack, 1), Is.EqualTo(6));
        }

        [Test]
        public void LegacyWrappers_RemainMappedToCoreTrack()
        {
            StepProgressSaver.SaveLastCompletedStep(4);

            Assert.That(StepProgressSaver.GetLastCompletedStep(), Is.EqualTo(4));
            Assert.That(StepProgressSaver.GetLastCompletedStep(StepProgressSaver.CoreKinematicsTrack), Is.EqualTo(4));
            Assert.That(StepProgressSaver.GetResumeStep(1), Is.EqualTo(5));
        }

        [Test]
        public void CurrentTrack_NormalizesToKnownTracks()
        {
            StepProgressSaver.SetCurrentTrack("unexpected_track");
            Assert.That(StepProgressSaver.GetCurrentTrack(), Is.EqualTo(StepProgressSaver.CoreKinematicsTrack));

            StepProgressSaver.SetCurrentTrack(StepProgressSaver.PreKinematicsTrack);
            Assert.That(StepProgressSaver.GetCurrentTrack(), Is.EqualTo(StepProgressSaver.PreKinematicsTrack));
        }
    }
}
