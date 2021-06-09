using System;

using AppMotor.Core.Utils;

using HDVP.TestUtils;

using Shouldly;

using Xunit;

namespace HDVP.Tests
{
    public sealed class HdvpVerificationCodeProviderTests
    {
        [Fact]
        public void TestBasicBehavior()
        {
            var data = TestDataProvider.CreateVerifiableData();

            var codeProviderCreationTime = DateTime.UtcNow;
            var codeProvider = new HdvpVerificationCodeProvider(data, verificationCodeLength: 12);

            var verificationCode1 = codeProvider.GetVerificationCode();
            var verificationCode2 = codeProvider.GetVerificationCode();

            verificationCode1.ShouldBeSameAs(verificationCode2);

            codeProvider.VerificationCodeValidUntilUtc.ShouldBe(
                codeProviderCreationTime + HdvpVerificationCodeProvider.DEFAULT_TIME_TO_LIVE,
                tolerance: TimeSpan.FromSeconds(2)
            );
            codeProvider.VerificationCodeValidUntilUtc.Kind.ShouldBe(DateTimeKind.Utc);
        }

        [Fact]
        public void TestCodeRecreation()
        {
            var data = TestDataProvider.CreateVerifiableData();
            var dateTimeProvider = new TestDateTimeProvider();

            var codeProvider = new HdvpVerificationCodeProvider(data, verificationCodeLength: 12, dateTimeProvider);

            var verificationCode1 = codeProvider.GetVerificationCode();

            dateTimeProvider.UtcNow = codeProvider.VerificationCodeValidUntilUtc + TimeSpan.FromSeconds(1);

            var verificationCode2 = codeProvider.GetVerificationCode();

            verificationCode2.Salt.ShouldNotBe(verificationCode1.Salt);
            verificationCode2.Code.ShouldNotBe(verificationCode1.Code);

            codeProvider.VerificationCodeValidUntilUtc.ShouldBe(dateTimeProvider.UtcNow + HdvpVerificationCodeProvider.DEFAULT_TIME_TO_LIVE);
        }

        private sealed class TestDateTimeProvider : IDateTimeProvider
        {
            /// <inheritdoc />
            public DateTime LocalNow => throw new NotSupportedException();

            /// <inheritdoc />
            public DateTime UtcNow { get; set; } = DateTime.UtcNow;
        }
    }
}
