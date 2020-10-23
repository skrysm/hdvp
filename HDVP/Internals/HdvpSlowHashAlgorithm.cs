using JetBrains.Annotations;

using Konscious.Security.Cryptography;

namespace HDVP.Internals
{
    internal static class HdvpSlowHashAlgorithm
    {
        [MustUseReturnValue]
        public static byte[] CreateHash(byte[] data, byte[] salt, int byteCount)
        {
            using var argon2 = new Argon2id(data);

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8; // 8 = max CPU usage on CPU with 4 cores and hyper threading
            argon2.MemorySize = 130; // MB

            // This gives about 0.6 hashes per second on a Raspberry Pi 4 and about
            // 9 hashes per second on a medium desktop CPU.
            argon2.Iterations = 1000;

            return argon2.GetBytes(byteCount);
        }
    }
}
