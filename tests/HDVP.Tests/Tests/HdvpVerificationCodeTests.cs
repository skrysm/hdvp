using System;

using HDVP.TestUtils;

using Shouldly;

using Xunit;

namespace HDVP.Tests
{
    public sealed class HdvpVerificationCodeTests
    {
        [Fact]
        public void TestCreate()
        {
            // Setup
            var data = TestDataProvider.CreateVerifiableData();

            var salt = TestDataProvider.CreateNonRandomSalt();

            // Test 1
            var verificationCode1 = HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCodeProvider.DEFAULT_CODE_LENGTH);

            // Verification
            verificationCode1.Code.Length.ShouldBe(HdvpVerificationCodeProvider.DEFAULT_CODE_LENGTH);
            verificationCode1.Salt.ShouldBe(salt);

            // Test 2
            var verificationCode2 = HdvpVerificationCode.Create(verificationCode1.Code, verificationCode1.Salt);
            verificationCode2.Code.ShouldBe(verificationCode1.Code);
            verificationCode2.Salt.ShouldBe(salt);

            verificationCode2.IsMatch(data).ShouldBe(true);
        }

        /// <summary>
        /// Verifies that having two different salts for the same binary data gives two different verification codes.
        /// </summary>
        [Fact]
        public void TestDifferentSaltsGiveDifferentCodes()
        {
            // Setup
            var data = TestDataProvider.CreateVerifiableData();

            var salt1 = HdvpSalt.CreateNewSalt();
            var verificationCode1 = HdvpVerificationCode.Create(data, salt1, HdvpVerificationCodeProvider.DEFAULT_CODE_LENGTH);

            var salt2 = HdvpSalt.CreateNewSalt();
            var verificationCode2 = HdvpVerificationCode.Create(data, salt2, HdvpVerificationCodeProvider.DEFAULT_CODE_LENGTH);

            // Verification
            verificationCode2.Code.ShouldNotBe(verificationCode1.Code);
        }

        [Fact]
        public void TestCodeLength()
        {
            var data = TestDataProvider.CreateVerifiableData();
            var salt = TestDataProvider.CreateNonRandomSalt();

            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MinCodeLength - 1));
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MaxCodeLength + 1));

            var verificationCode1 = HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MinCodeLength);
            verificationCode1.Code.Length.ShouldBe(HdvpVerificationCode.MinCodeLength);

            var verificationCode2 = HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MaxCodeLength);
            verificationCode2.Code.Length.ShouldBe(HdvpVerificationCode.MaxCodeLength);
        }

        [Fact]
        public void TestWrongCodeLengthDetection()
        {
            // Setup
            var data = TestDataProvider.CreateVerifiableData();
            var salt = TestDataProvider.CreateNonRandomSalt();

            // Setup 1
            var verificationCode1 = HdvpVerificationCode.Create(data, salt, codeLength: 12);

            // Test 1
            var shortenedCode1 = verificationCode1.Code.Substring(0, verificationCode1.Code.Length - 1);
            HdvpVerificationCode.CheckFormat(shortenedCode1).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(shortenedCode1, salt));

            var longerCode1 = verificationCode1.Code + "a";
            HdvpVerificationCode.CheckFormat(longerCode1).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(longerCode1, salt));

            var codeWithInvalidSymbol = verificationCode1.Code.Remove(2, 1).Insert(2, "@");
            HdvpVerificationCode.CheckFormat(codeWithInvalidSymbol).ShouldBe(HdvpFormatValidationResults.InvalidSymbols);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(codeWithInvalidSymbol, salt));

            // Setup 2
            var verificationCode2 = HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MinCodeLength);

            // Test 2
            var shortenedCode2 = verificationCode2.Code.Substring(0, verificationCode2.Code.Length - 1);
            HdvpVerificationCode.CheckFormat(shortenedCode2).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(shortenedCode2, salt));

            // Setup 3
            var verificationCode3 = HdvpVerificationCode.Create(data, salt, codeLength: HdvpVerificationCode.MaxCodeLength);

            // Test 3
            var longerCode2 = verificationCode3.Code + "a";
            HdvpVerificationCode.CheckFormat(longerCode2).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(longerCode2, salt));
        }
    }
}
