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
