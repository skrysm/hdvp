#region License
// Copyright 2021 HDVP (https://github.com/skrysmanski/hdvp)
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

namespace HDVP.Tests;

public sealed class HdvpVerifiableDataTests
{
    private const int EXPECTED_DATA_HASH_LENGTH = 512 / 8;

    [Fact]
    public void Test_FromBytes()
    {
        byte[] baseData = TestDataProvider.CreateVerifiableDataRaw();
        HdvpVerifiableData data1 = new(baseData);
        HdvpVerifiableData data2 = baseData;

        data1.Hash.ShouldBe(data2.Hash);
        data1.Hash.Length.ShouldBe(EXPECTED_DATA_HASH_LENGTH);
    }

    [Fact]
    public void Test_FromStream()
    {
        byte[] baseData = TestDataProvider.CreateVerifiableDataRaw();
        HdvpVerifiableData data1 = HdvpVerifiableData.ReadFromStream(new MemoryStream(baseData, writable: false));
        HdvpVerifiableData data2 = new(baseData);

        data1.Hash.ShouldBe(data2.Hash);
        data1.Hash.Length.ShouldBe(EXPECTED_DATA_HASH_LENGTH);
    }

    [Fact]
    public void Test_Equals()
    {
        byte[] baseData = TestDataProvider.CreateVerifiableDataRaw();
        byte[] randomTestData = new byte[128];

        new Random(1234).NextBytes(randomTestData);

        HdvpVerifiableData data1 = new(baseData);
        HdvpVerifiableData data2 = new(baseData);
        HdvpVerifiableData data3 = new(randomTestData);

        data1.Equals(data1).ShouldBe(true);
        data1.Equals(data2).ShouldBe(true);
        data1.Equals(data3).ShouldBe(false);
        data1.Equals(null).ShouldBe(false);

        data1!.Equals((object)data1).ShouldBe(true);
        data1.Equals((object)data2).ShouldBe(true);
        data1.Equals((object)data3).ShouldBe(false);
        data1.Equals((object?)null).ShouldBe(false);

        (data1 == data2).ShouldBe(true);
        (data1 == data3).ShouldBe(false);
        (data1 != data2).ShouldBe(false);
        (data1 != data3).ShouldBe(true);
    }

    [Fact]
    public void Test_GetHashCode()
    {
        var bytes = TestDataProvider.CreateNonRandomSaltRaw();

        HdvpVerifiableData data1 = new(bytes);
        HdvpVerifiableData data2 = new(bytes);

        data1.GetHashCode().ShouldBe(data2.GetHashCode());
    }
}
