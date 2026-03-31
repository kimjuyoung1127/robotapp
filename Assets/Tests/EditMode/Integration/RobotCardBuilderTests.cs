// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// RobotCardBuilder 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.Templates;
using KineTutor3D.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class RobotCardBuilderTests
    {
        private GameObject rootObject;
        private RectTransform parent;
        private Font font;

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject("CardParent", typeof(RectTransform));
            parent = rootObject.GetComponent<RectTransform>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [TearDown]
        public void TearDown()
        {
            if (rootObject != null)
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void BuildCard_WiresSelectAndDetailButtons_WithoutRootButton()
        {
            Assert.That(RobotCatalog.TryGet("FAIRINO_FR5", out var entry), Is.True);

            var selectCount = 0;
            var detailCount = 0;
            var card = RobotCardBuilder.BuildCard(
                parent,
                entry,
                font,
                false,
                () => selectCount++,
                () => detailCount++,
                280f,
                220f);

            Assert.That(card.GetComponent<Button>(), Is.Null, "Card root should not own a Button.");

            var bodyButton = card.Find("BtnCardBody")?.GetComponent<Button>();
            var selectButton = card.Find("BtnSelect")?.GetComponent<Button>();
            var detailButton = card.Find("BtnDetail")?.GetComponent<Button>();

            Assert.That(bodyButton, Is.Not.Null);
            Assert.That(selectButton, Is.Not.Null);
            Assert.That(detailButton, Is.Not.Null);

            bodyButton.onClick.Invoke();
            Assert.That(selectCount, Is.EqualTo(1));
            Assert.That(detailCount, Is.EqualTo(0));

            selectButton.onClick.Invoke();
            Assert.That(selectCount, Is.EqualTo(2));
            Assert.That(detailCount, Is.EqualTo(0));

            detailButton.onClick.Invoke();
            Assert.That(selectCount, Is.EqualTo(2));
            Assert.That(detailCount, Is.EqualTo(1));
        }

        [Test]
        public void BuildCard_RebuildsWithoutDuplicatingListeners()
        {
            Assert.That(RobotCatalog.TryGet("FAIRINO_FR5", out var entry), Is.True);

            var selectCount = 0;
            var detailCount = 0;
            RobotCardBuilder.BuildCard(parent, entry, font, false, () => selectCount++, () => detailCount++, 280f, 220f);
            var card = RobotCardBuilder.BuildCard(parent, entry, font, false, () => selectCount++, () => detailCount++, 280f, 220f);

            var bodyButton = card.Find("BtnCardBody")?.GetComponent<Button>();
            var selectButton = card.Find("BtnSelect")?.GetComponent<Button>();
            var detailButton = card.Find("BtnDetail")?.GetComponent<Button>();

            bodyButton.onClick.Invoke();
            selectButton.onClick.Invoke();
            detailButton.onClick.Invoke();

            Assert.That(selectCount, Is.EqualTo(2), "Select listeners should be rebound exactly once.");
            Assert.That(detailCount, Is.EqualTo(1), "Detail listeners should be rebound exactly once.");
        }

        [Test]
        public void BuildCard_SelectOnlyEntry_HidesDetailButton()
        {
            Assert.That(RobotCatalog.TryGet("FAIRINO_FR5_TEMPLATE", out var entry), Is.True);

            var card = RobotCardBuilder.BuildCard(
                parent,
                entry,
                font,
                false,
                () => { },
                () => { },
                280f,
                220f);

            var detailButton = card.Find("BtnDetail");
            Assert.That(detailButton, Is.Not.Null);
            Assert.That(detailButton.gameObject.activeSelf, Is.False);
        }
    }
}
