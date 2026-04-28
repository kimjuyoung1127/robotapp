using System.Collections.Generic;
using KineTutor3D.App.Fairino;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode.Core
{
    [TestFixture]
    public sealed class BuchimgaeCookingSequencePresetTests
    {
        [Test]
        public void BuildManifest_CreatesGroupedCookingSequence()
        {
            var manifest = BuchimgaeCookingSequencePreset.BuildManifest();

            Assert.That(manifest.Points, Has.Length.EqualTo(28));
            Assert.That(manifest.Bundles, Has.Length.EqualTo(5));
            Assert.That(manifest.Blocks, Has.Length.EqualTo(5));
            Assert.That(manifest.Bundles[0].name, Is.EqualTo(BuchimgaeCookingSequencePreset.SetupBundleName));
            Assert.That(manifest.Bundles[4].name, Is.EqualTo(BuchimgaeCookingSequencePreset.FinishBundleName));
        }

        [Test]
        public void BuildManifest_AllBundleRefsResolveToPoints()
        {
            var manifest = BuchimgaeCookingSequencePreset.BuildManifest();
            var pointNames = new HashSet<string>();
            for (var index = 0; index < manifest.Points.Length; index++)
            {
                pointNames.Add(manifest.Points[index].name);
            }

            for (var bundleIndex = 0; bundleIndex < manifest.Bundles.Length; bundleIndex++)
            {
                var steps = manifest.Bundles[bundleIndex].steps;
                Assert.That(steps, Is.Not.Empty);
                for (var stepIndex = 0; stepIndex < steps.Length; stepIndex++)
                {
                    Assert.That(steps[stepIndex].kind, Is.EqualTo("PointRef"));
                    Assert.That(pointNames, Does.Contain(steps[stepIndex].refName));
                }
            }

            Assert.That(pointNames, Does.Not.Contain("BCH_COOK_EDGE_CHECK"));
        }

        [Test]
        public void BuildManifest_BlockSequenceUsesBundleRefs()
        {
            var manifest = BuchimgaeCookingSequencePreset.BuildManifest();
            var bundleNames = new HashSet<string>();
            for (var index = 0; index < manifest.Bundles.Length; index++)
            {
                bundleNames.Add(manifest.Bundles[index].name);
            }

            for (var index = 0; index < manifest.Blocks.Length; index++)
            {
                Assert.That(manifest.Blocks[index].kind, Is.EqualTo(TeachingSequenceBlock.BundleRefKind));
                Assert.That(bundleNames, Does.Contain(manifest.Blocks[index].refName));
            }
        }
    }
}
