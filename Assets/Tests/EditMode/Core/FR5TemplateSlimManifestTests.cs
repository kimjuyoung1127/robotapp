// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// FR5TemplateSlimManifest 동작을 검증하는 EditMode 테스트입니다.
using System.Linq;
using NUnit.Framework;
using KineTutor3D.App;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// FR5 슬림 템플릿 추출 manifest가 필요한 자산만 포함하는지 검증합니다.
    /// </summary>
    public class FR5TemplateSlimManifestTests
    {
        [Test]
        public void PackageRoots_IncludeRequiredCoreAssets()
        {
            var roots = FR5TemplateSlimManifest.GetPackageRoots();

            Assert.That(roots, Does.Contain(FR5TemplateSlimManifest.DemoScenePath));
            Assert.That(roots, Does.Contain(FR5TemplateSlimManifest.ControlPrefabAssetPath));
            Assert.That(roots, Does.Contain(FR5TemplateSlimManifest.PreviewPrefabAssetPath));
            Assert.That(roots, Does.Contain(FR5TemplateSlimManifest.PreviewMaterialAssetPath));
            Assert.That(roots, Does.Contain(FR5TemplateSlimManifest.RuntimeRobotFolder));
        }

        [Test]
        public void PackageRoots_DoNotPullUiFolder()
        {
            var roots = FR5TemplateSlimManifest.GetPackageRoots();

            Assert.That(roots.Any(path => path.StartsWith("Assets/Scripts/UI")), Is.False, "slim package should not include UI folder");
        }
    }
}
