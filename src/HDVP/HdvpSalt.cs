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
using System.Security.Cryptography;

using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace HDVP
{
    /// <summary>
    /// Represents the salt using in a <see cref="HdvpVerificationCode"/>
    /// </summary>
    public sealed class HdvpSalt
    {
        /// <summary>
        /// The length an HDVP salt must have.
        /// </summary>
        [PublicAPI]
        public static int SaltLength => 32;

        /// <summary>
        /// The bytes this salt represents.
        /// </summary>
        [PublicAPI]
        public ImmutableArray<byte> Value { get; }

        /// <summary>
        /// Creates an instance of this class for a pre-existing salt.
        /// </summary>
        /// <param name="value">The salt; must be exactly 32 bytes long (see <see cref="SaltLength"/>).</param>
        public HdvpSalt([NotNull] IReadOnlyCollection<byte> value)
        {
            Validate.ArgumentWithName(nameof(value)).IsNotNull(value);

            if (value.Count != SaltLength)
            {
                throw new ArgumentException($"The salt must be {SaltLength} bytes long but was {value.Count} bytes long.");
            }

            this.Value = value.ToImmutableArray();
        }

        /// <summary>
        /// Creates a new, cryptographically random HDVP salt.
        /// </summary>
        [MustUseReturnValue]
        public static HdvpSalt CreateNewSalt()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();

            var salt = new byte[SaltLength];
            rngCryptoServiceProvider.GetBytes(salt);

            return new HdvpSalt(salt);
        }
    }
}
