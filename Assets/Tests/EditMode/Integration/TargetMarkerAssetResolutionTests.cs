// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// TargetMarkerAssetResolution 동작을 검증하는 EditMode 테스트입니다.
using System.IO;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class TargetMarkerAssetResolutionTests
    {
        [Test]
        public void MarkerAssets_ExistInCuratedOrVendorPaths()
        {
            Assert.That(
            File.Exists("Assets/Runtime/Prefabs/Teaching/Markers/ShootingTarget.prefab") ||
            File.Exists("Assets/Vendors/Archive/GlowingRifts/Shooting Target/ShootingTarget.prefab"),
                Is.True,
                "ShootingTarget prefab is required.");

            Assert.That(
            File.Exists("Assets/Runtime/Prefabs/Teaching/Markers/Checkmark_3D_Icon.prefab") ||
            File.Exists("Assets/Vendors/Archive/HQPStudios/Low Poly 3D Icons - Pack Lite/Prefabs/Checkmark_3D_Icon.prefab"),
                Is.True,
                "Checkmark prefab is required.");

            Assert.That(
            File.Exists("Assets/Runtime/Prefabs/Teaching/Markers/Warning_3D_Icon.prefab") ||
            File.Exists("Assets/Vendors/Archive/HQPStudios/Low Poly 3D Icons - Pack Lite/Prefabs/Warning_3D_Icon.prefab"),
                Is.True,
                "Warning prefab is required.");
        }
    }
}
