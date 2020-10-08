using System.Security.Cryptography;

using JetBrains.Annotations;

namespace HDVP.Internals
{
    internal static class HdvpSlowHashAlgorithm
    {
        [MustUseReturnValue]
        public static byte[] CreateHash(byte[] data, byte[] salt, int byteCount)
        {
            const int ITERATIONS = 1000;

            // TODO: Replace with something more secure (i.e. more slow)
            using DeriveBytes slowHash = new Rfc2898DeriveBytes(data, salt, ITERATIONS, HashAlgorithmName.SHA512);

            return slowHash.GetBytes(byteCount);
        }
    }
}
