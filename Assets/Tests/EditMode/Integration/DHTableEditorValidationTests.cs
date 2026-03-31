// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// DHTableEditorValidation 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.UI;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// DH 테이블 입력 파싱/포맷 유틸을 검증합니다.
    /// </summary>
    public class DHTableEditorValidationTests
    {
        [Test]
        public void TryParseFinite_ValidNumericString_ReturnsTrue()
        {
            var ok = DHTableEditor.TryParseFinite("1.25", out var value);
            Assert.IsTrue(ok);
            Assert.AreEqual(1.25, value, TestTolerances.Math);
        }

        [Test]
        public void TryParseFinite_NaNOrInfinity_ReturnsFalse()
        {
            Assert.IsFalse(DHTableEditor.TryParseFinite("NaN", out _));
            Assert.IsFalse(DHTableEditor.TryParseFinite("Infinity", out _));
            Assert.IsFalse(DHTableEditor.TryParseFinite("-Infinity", out _));
        }

        [Test]
        public void TryParseFinite_InvalidString_ReturnsFalse()
        {
            Assert.IsFalse(DHTableEditor.TryParseFinite("abc", out _));
            Assert.IsFalse(DHTableEditor.TryParseFinite(string.Empty, out _));
        }

        [Test]
        public void FormatDouble_UsesInvariantFixedDigits()
        {
            var formatted = DHTableEditor.FormatDouble(1.23456, 4);
            Assert.AreEqual("1.2346", formatted);
        }
    }
}
