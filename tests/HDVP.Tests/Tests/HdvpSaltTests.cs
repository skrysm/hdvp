#region License
// Copyright 2020 HDVP (https://github.com/skrysmanski/hdvp)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using HDVP.TestUtils;

using Shouldly;

using Xunit;

namespace HDVP.Tests
{
    public class HdvpSaltTests
    {
        [Fact]
        public void TestSaltLength()
        {
            var data = TestDataProvider.CreateVerifiableData();

            HdvpSalt.SaltLength.ShouldBe(32);

            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, new HdvpSalt(new byte[HdvpSalt.SaltLength - 1]), codeLength: HdvpVerificationCode.MinCodeLength));
            Should.Throw<ArgumentException>(() => HdvpVerificationCode.Create(data, new HdvpSalt(new byte[HdvpSalt.SaltLength + 1]), codeLength: HdvpVerificationCode.MinCodeLength));
        }

        [Fact]
        public void Test_ConversionFromByteArray()
        {
            var bytes = TestDataProvider.CreateNonRandomSaltRaw();

            HdvpSalt salt1 = new(bytes);
            salt1.Value.ShouldBe(bytes);

            HdvpSalt salt2 = bytes;
            salt2.Value.ShouldBe(bytes);
        }

        [Fact]
        public void Test_Equals()
        {
            var bytes = TestDataProvider.CreateNonRandomSaltRaw();

            HdvpSalt salt1 = new(bytes);
            HdvpSalt salt2 = new(bytes);
            HdvpSalt salt3 = HdvpSalt.CreateNewSalt();

            salt1.Equals(salt1).ShouldBe(true);
            salt1.Equals(salt2).ShouldBe(true);
            salt1.Equals(salt3).ShouldBe(false);
            salt1.Equals(null).ShouldBe(false);

            salt1!.Equals((object)salt1).ShouldBe(true);
            salt1.Equals((object)salt2).ShouldBe(true);
            salt1.Equals((object)salt3).ShouldBe(false);
            salt1.Equals((object?)null).ShouldBe(false);

            (salt1 == salt2).ShouldBe(true);
            (salt1 == salt3).ShouldBe(false);
            (salt1 != salt2).ShouldBe(false);
            (salt1 != salt3).ShouldBe(true);
        }

        [Fact]
        public void Test_GetHashCode()
        {
            var bytes = TestDataProvider.CreateNonRandomSaltRaw();

            HdvpSalt salt1 = new(bytes);
            HdvpSalt salt2 = new(bytes);

            salt1.GetHashCode().ShouldBe(salt2.GetHashCode());
        }
    }
}
