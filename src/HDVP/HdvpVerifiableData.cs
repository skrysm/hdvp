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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using JetBrains.Annotations;

namespace HDVP
{
    /// <summary>
    /// Represents the binary data that's to be verified with HDVP. Instances of this class
    /// can be created either via <see cref="CalculateHashFromMemory"/> or <see cref="ReadFromStream"/>.
    /// </summary>
    /// <remarks>
    /// For the reason why this class exists, see <see cref="Hash"/>.
    /// </remarks>
    public sealed class HdvpVerifiableData : IEquatable<HdvpVerifiableData>
    {
        /// <summary>
        /// The verifiable data - "reduced" to a hash. Hashing the data has two main benefits:
        ///
        /// <para>1. The input data can be kept in memory because it's rather small (compared to the input data). This way,
        /// recalculated a verification code doesn't require that much memory.</para>
        ///
        /// <para>2. The hash was created with a hash algorithm that is considered "not broken" (i.e. a pre-image attack
        /// is not feasible). This way, the slow hash used to create the verification code doesn't need to be that resilient
        /// against pre-image attacks.</para>
        /// </summary>
        public ImmutableArray<byte> Hash { get; }

        /// <summary>
        /// Creates an instance of this class from data that's already available in memory
        /// (in form of a byte array or something similar).
        /// </summary>
        /// <seealso cref="ReadFromStream"/>
        [PublicAPI]
        public HdvpVerifiableData(IEnumerable<byte> bytes)
            : this(CalculateHashFromMemory(bytes))
        {
        }

        private HdvpVerifiableData(ImmutableArray<byte> hash)
        {
            this.Hash = hash;
        }

        private static ImmutableArray<byte> CalculateHashFromMemory(IEnumerable<byte> data)
        {
            using var hashAlgorithm = CreateHashAlgorithm();

            var hash = hashAlgorithm.ComputeHash(data as byte[] ?? data.ToArray());

            return hash.ToImmutableArray();
        }

        /// <summary>
        /// Implicit conversion from byte array.
        /// </summary>
        public static implicit operator HdvpVerifiableData(byte[] bytes)
        {
            return new(bytes);
        }

        /// <summary>
        /// Creates an instance of this class from data available in a stream.
        ///
        /// <para>Note: This method reads the whole stream but does not(!) store
        /// its contents in memory.</para>
        /// </summary>
        [PublicAPI, MustUseReturnValue]
        public static HdvpVerifiableData ReadFromStream(Stream stream)
        {
            using var hashAlgorithm = CreateHashAlgorithm();

            var hash = hashAlgorithm.ComputeHash(stream);

            return new HdvpVerifiableData(hash.ToImmutableArray());
        }

        [MustUseReturnValue]
        private static HashAlgorithm CreateHashAlgorithm()
        {
            return SHA512.Create();
        }

        /// <inheritdoc />
        public bool Equals(HdvpVerifiableData? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Hash.SequenceEqual(other.Hash);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is HdvpVerifiableData other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return BitConverter.ToInt32(this.Hash.AsSpan());
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(HdvpVerifiableData? left, HdvpVerifiableData? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Not-equals operator
        /// </summary>
        public static bool operator !=(HdvpVerifiableData? left, HdvpVerifiableData? right)
        {
            return !Equals(left, right);
        }
    }
}
