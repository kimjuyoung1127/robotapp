// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotControlSceneCoordinator 동작을 검증하는 EditMode 테스트입니다.
using System.Reflection;
using KineTutor3D.App.Fairino;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControl 씬의 FR5 control prefab 로드 경로와 runtime root 해석을 검증합니다.
    /// </summary>
    public class RobotControlSceneCoordinatorTests
    {
        [Test]
        public void ControlPrefab_LoadsFromResources_AndHasVisibleMeshes()
        {
            var prefab = Resources.Load<GameObject>("Robots/FAIRINO_FR5_Control");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<MeshFilter>(true).Length, Is.GreaterThanOrEqualTo(7));
            Assert.That(prefab.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThanOrEqualTo(7));
        }

        [Test]
        public void FindSceneRuntimeRoot_UsesExistingSceneRoot()
        {
            var runtimeRoot = new GameObject("FR5_RuntimeRoot");

            try
            {
                var method = typeof(RobotControlSceneCoordinator).GetMethod(
                    "FindSceneRuntimeRoot",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    System.Type.EmptyTypes,
                    null);

                Assert.That(method, Is.Not.Null);

                var resolved = method.Invoke(null, null) as Transform;

                Assert.That(resolved, Is.EqualTo(runtimeRoot.transform));
            }
            finally
            {
                Object.DestroyImmediate(runtimeRoot);
            }
        }

        [Test]
        public void TryLoadControlPrefab_ReturnsSuccessAndMeshCounts()
        {
            var host = new GameObject("RobotControlCoordinatorHost");
            var method = typeof(RobotControlSceneCoordinator).GetMethod(
                "TryLoadControlPrefab",
                BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                var coordinator = host.AddComponent<RobotControlSceneCoordinator>();
                Assert.That(method, Is.Not.Null);

                var args = new object[] { null, null, 0, 0 };
                var success = (bool)method.Invoke(coordinator, args);
                var prefab = args[0] as GameObject;
                var diagnostic = args[1] as string;
                var meshFilterCount = (int)args[2];
                var meshRendererCount = (int)args[3];

                Assert.That(success, Is.True, diagnostic);
                Assert.That(prefab, Is.Not.Null);
                Assert.That(meshFilterCount, Is.GreaterThanOrEqualTo(7));
                Assert.That(meshRendererCount, Is.GreaterThanOrEqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
