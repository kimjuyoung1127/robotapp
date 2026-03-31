// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class FairinoErrorTranslatorTests
    {
        private FairinoErrorTranslator translator;

        [SetUp]
        public void SetUp()
        {
            translator = new FairinoErrorTranslator();
        }

        [Test]
        public void Translate_ZeroCode_ReturnsOK()
        {
            Assert.AreEqual("OK", translator.Translate(0));
        }

        [Test]
        public void Translate_MinusOne_ReturnsConnectionMessage()
        {
            var msg = translator.Translate(-1);
            Assert.That(msg, Does.Contain("연결"));
        }

        [Test]
        public void Translate_MinusTwo_ReturnsEnableMessage()
        {
            var msg = translator.Translate(-2);
            Assert.That(msg, Does.Contain("비활성"));
        }

        [Test]
        public void Translate_MinusThree_ReturnsLimitMessage()
        {
            var msg = translator.Translate(-3);
            Assert.That(msg, Does.Contain("관절 제한"));
        }

        [Test]
        public void Translate_MinusFour_ReturnsEStopMessage()
        {
            var msg = translator.Translate(-4);
            Assert.That(msg, Does.Contain("비상 정지"));
        }

        [Test]
        public void Translate_UnknownCode_ReturnsUnknownMessage()
        {
            var msg = translator.Translate(-999);
            Assert.That(msg, Does.Contain("알 수 없는"));
            Assert.That(msg, Does.Contain("-999"));
        }

        [Test]
        public void AddMapping_OverridesDefault()
        {
            translator.AddMapping(0, "Custom OK");
            Assert.AreEqual("Custom OK", translator.Translate(0));
        }

        [Test]
        public void AddMapping_NewCode_ReturnsMapped()
        {
            translator.AddMapping(-100, "커스텀 에러");
            Assert.AreEqual("커스텀 에러", translator.Translate(-100));
        }

        [Test]
        public void ToResult_ZeroCode_ReturnsSuccess()
        {
            var result = translator.ToResult(0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.ErrorCode);
        }

        [Test]
        public void ToResult_NonZeroCode_ReturnsFail()
        {
            var result = translator.ToResult(-1);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(-1, result.ErrorCode);
        }
    }
}
