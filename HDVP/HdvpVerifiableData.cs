using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;

using JetBrains.Annotations;

namespace HDVP
{
    /// <summary>
    /// Represents the binary data that's to be verified with HDVP.
    /// </summary>
    public sealed class HdvpVerifiableData
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

        private HdvpVerifiableData(byte[] hash)
        {
            this.Hash = hash.ToImmutableArray();
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpVerifiableData ReadFromMemory(byte[] data)
        {
            using var hashAlgorithm = CreateHashAlgorithm();

            var hash = hashAlgorithm.ComputeHash(data);

            return new HdvpVerifiableData(hash);
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpVerifiableData ReadFromStream(Stream stream)
        {
            using var hashAlgorithm = CreateHashAlgorithm();

            var hash = hashAlgorithm.ComputeHash(stream);

            return new HdvpVerifiableData(hash);
        }

        [MustUseReturnValue]
        private static HashAlgorithm CreateHashAlgorithm()
        {
            return SHA512.Create();
        }
    }
}
