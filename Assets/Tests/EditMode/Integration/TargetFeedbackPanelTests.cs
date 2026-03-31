// Folder: Tests/EditMode - TargetFeedbackPanel 가시성 및 기본 동작 검증
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// TargetFeedbackPanel의 가시성 제어와 기본 레이아웃을 검증합니다.
    /// </summary>
    public class TargetFeedbackPanelTests
    {
        private GameObject root;
        private TargetFeedbackPanel panel;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            panel = root.AddComponent<TargetFeedbackPanel>();
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
        }

        [Test]
        public void SetVisible_False_HidesPanelRoot()
        {
            panel.SetVisible(false);

            var panelRoot = root.transform.Find("TargetFeedbackRect");
            if (panelRoot != null)
            {
                Assert.That(panelRoot.gameObject.activeSelf, Is.False);
            }
        }

        [Test]
        public void SetVisible_True_ShowsPanelRoot()
        {
            panel.SetVisible(false);
            panel.SetVisible(true);

            var panelRoot = root.transform.Find("TargetFeedbackRect");
            if (panelRoot != null)
            {
                Assert.That(panelRoot.gameObject.activeSelf, Is.True);
            }
        }

        [Test]
        public void Implements_IVisibilityControllable()
        {
            Assert.That(panel, Is.InstanceOf<IVisibilityControllable>());
        }

        [Test]
        public void SetVisible_DoesNotThrow_BeforeBind()
        {
            Assert.DoesNotThrow(() => panel.SetVisible(false));
            Assert.DoesNotThrow(() => panel.SetVisible(true));
        }
    }
}
