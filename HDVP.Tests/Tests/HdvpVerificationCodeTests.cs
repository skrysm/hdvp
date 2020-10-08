using System.Collections.Immutable;
using System.Text;

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

            var dataBytes = Encoding.UTF8.GetBytes(
                "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. " +
                "At vero eos et accusam et justo duo dolores et ea rebum."
            );
            var data = HdvpVerifiableData.ReadFromMemory(dataBytes);

            var salt = ImmutableArray.Create(Encoding.ASCII.GetBytes("Stet clita kasd gubergren, no se"));

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
    }
}
