// Folder: Tests/EditMode - IVisibilityControllable 인터페이스 계약 검증
using KineTutor3D.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// IVisibilityControllable 인터페이스 계약을 구현체 전체에서 검증합니다.
    /// </summary>
    public class VisibilityControllableContractTests
    {
        private static readonly Type[] ImplementingTypes = typeof(IVisibilityControllable).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface
                        && typeof(IVisibilityControllable).IsAssignableFrom(t)
                        && typeof(MonoBehaviour).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToArray();

        private readonly List<GameObject> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in created)
            {
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
            created.Clear();
        }

        [Test]
        public void AllImplementations_AreMonoBehaviour()
        {
            Assert.That(ImplementingTypes.Length, Is.GreaterThanOrEqualTo(18),
                "At least 18 panels should implement IVisibilityControllable");

            foreach (var type in ImplementingTypes)
            {
                Assert.That(typeof(MonoBehaviour).IsAssignableFrom(type), Is.True,
                    $"{type.Name} should be a MonoBehaviour");
            }
        }

        [Test]
        public void AllImplementations_HaveSetVisibleMethod()
        {
            foreach (var type in ImplementingTypes)
            {
                var method = type.GetMethod("SetVisible", new[] { typeof(bool) });
                Assert.That(method, Is.Not.Null, $"{type.Name} should have SetVisible(bool)");
                Assert.That(method.ReturnType, Is.EqualTo(typeof(void)),
                    $"{type.Name}.SetVisible should return void");
            }
        }

        [Test]
        public void SetVisible_False_DoesNotThrow_OnFreshComponent()
        {
            foreach (var type in ImplementingTypes)
            {
                var go = new GameObject($"Test_{type.Name}", typeof(RectTransform));
                created.Add(go);

                var component = (IVisibilityControllable)go.AddComponent(type);
                Assert.DoesNotThrow(() => component.SetVisible(false),
                    $"{type.Name}.SetVisible(false) should not throw on fresh component");
            }
        }

        [Test]
        public void SetVisible_True_DoesNotThrow_OnFreshComponent()
        {
            foreach (var type in ImplementingTypes)
            {
                var go = new GameObject($"Test_{type.Name}", typeof(RectTransform));
                created.Add(go);

                var component = (IVisibilityControllable)go.AddComponent(type);
                Assert.DoesNotThrow(() => component.SetVisible(true),
                    $"{type.Name}.SetVisible(true) should not throw on fresh component");
            }
        }

        [Test]
        public void ImplementingTypes_ListIsNotEmpty()
        {
            Assert.That(ImplementingTypes, Is.Not.Empty);
            foreach (var type in ImplementingTypes)
            {
                Assert.That(type.GetInterfaces(), Does.Contain(typeof(IVisibilityControllable)),
                    $"{type.Name} should implement IVisibilityControllable");
            }
        }
    }
}
