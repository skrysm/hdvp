using System;
using System.Collections.Immutable;
using System.Text;

using JetBrains.Annotations;

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
            const int CODE_LENGTH = 12;

            var data = CreateVerifiableData();

            var salt = CreateNonRandomSalt();

            // Test 1
            var verificationCode1 = HdvpVerificationCode.Create(data, salt, codeLength: CODE_LENGTH);

            // Verification
            verificationCode1.Code.Length.ShouldBe(CODE_LENGTH);
            verificationCode1.Salt.ShouldBe(salt);

            // Test 2
            var verificationCode2 = HdvpVerificationCode.Create(verificationCode1.Code, verificationCode1.Salt);
            verificationCode2.Code.ShouldBe(verificationCode1.Code);
            verificationCode2.Salt.ShouldBe(salt);

            verificationCode2.IsMatch(data).ShouldBe(true);
        }

        [Fact]
        public void TestSaltLength()
        {
            var data = CreateVerifiableData();

            HdvpVerificationCode.SaltLength.ShouldBe(32);

            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, ImmutableArray.Create(new byte[HdvpVerificationCode.SaltLength - 1]), codeLength: HdvpVerificationCode.MinCodeLength));
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, ImmutableArray.Create(new byte[HdvpVerificationCode.SaltLength + 1]), codeLength: HdvpVerificationCode.MinCodeLength));
        }

        [Fact]
        public void TestCodeLength()
        {
            var data = CreateVerifiableData();
            var salt = CreateNonRandomSalt();

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
            var data = CreateVerifiableData();
            var salt = CreateNonRandomSalt();

            // Setup 1
            var verificationCode1 = HdvpVerificationCode.Create(data, salt, codeLength: 12);

            // Test 1
            var shortenedCode1 = verificationCode1.Code.Substring(0, verificationCode1.Code.Length - 1);
            HdvpVerificationCode.CheckFormat(shortenedCode1).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(shortenedCode1, salt));

            var longerCode1 = verificationCode1.Code + "a";
            HdvpVerificationCode.CheckFormat(longerCode1).ShouldBe(HdvpFormatValidationResults.InvalidLength);
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(longerCode1, salt));

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

        [MustUseReturnValue]
        private static HdvpVerifiableData CreateVerifiableData()
        {
            var dataBytes = Encoding.UTF8.GetBytes(
                "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. " +
                "At vero eos et accusam et justo duo dolores et ea rebum."
            );

            return HdvpVerifiableData.ReadFromMemory(dataBytes);
        }

        [MustUseReturnValue]
        private static ImmutableArray<byte> CreateNonRandomSalt()
        {
            return ImmutableArray.Create(Encoding.ASCII.GetBytes("Stet clita kasd gubergren, no se"));
        }
    }
}
