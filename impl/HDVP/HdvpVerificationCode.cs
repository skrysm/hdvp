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

using AppMotor.Core.Utils;

using HDVP.Internals;

using JetBrains.Annotations;

namespace HDVP
{
    public sealed class HdvpVerificationCode
    {
        /// <summary>
        /// The text encoding used for verification codes.
        /// </summary>
        private static IVerificationCodeEncoding CodeEncoding { get; } = new ZBase32VerificationCodeEncoding();

        [PublicAPI]
        public static int MinCodeLength => 9;

        /// <summary>
        /// The maximum length (i.e. <c>Code.Length</c>) of a verification code (including length character).
        /// </summary>
        /// <remarks>
        /// This length uses "- 1" because we need to encode the values "0..31" - not "1..32".
        /// </remarks>
        [PublicAPI]
        public static int MaxCodeLength { get; } = MinCodeLength + CodeEncoding.AvailableSymbolCount - 1;

        /// <summary>
        /// The verification code itself.
        /// </summary>
        [PublicAPI]
        public string Code { get; }

        /// <summary>
        /// The salt being used to create this verification code.
        /// </summary>
        [PublicAPI]
        public HdvpSalt Salt { get; }

        private HdvpVerificationCode(string code, HdvpSalt salt)
        {
            this.Code = code;
            this.Salt = salt;
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpVerificationCode Create(string verificationCode, HdvpSalt salt)
        {
            var validationResult = CheckFormat(verificationCode);
            if (validationResult != HdvpFormatValidationResults.Valid)
            {
                throw new ArgumentException($"The verification code has a bad format. ({validationResult})", nameof(verificationCode));
            }

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [MustUseReturnValue]
        internal static HdvpVerificationCode Create(HdvpVerifiableData data, HdvpSalt salt, int codeLength)
        {
            Validate.Argument.IsNotNull(data, nameof(data));

            if (codeLength < MinCodeLength || codeLength > MaxCodeLength)
            {
                throw new ArgumentException($"The code length must be at least {MinCodeLength} and at most {MaxCodeLength}.");
            }

            // Calculate the number of required slow hash bytes
            int hashByteCount = CodeEncoding.GetRequiredByteCount(codeLength - 1);

            // Calculate the slow hash
            var slowHash = HdvpSlowHashAlgorithm.CreateHash(data.Hash, salt, byteCount: hashByteCount);

            // Convert the has to z-base-32
            var slowHashAsZBase32 = CodeEncoding.EncodeBytes(slowHash);

            // Encode code length
            char lengthChar = CodeEncoding.EncodeSingleValue(codeLength - MinCodeLength);

            // Assemble verification code
            var verificationCode = lengthChar + slowHashAsZBase32.Substring(0, codeLength - 1);

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [PublicAPI, Pure]
        public bool IsMatch(HdvpVerifiableData data)
        {
            var dataVerificationCode = Create(data, this.Salt, this.Code.Length);

            return dataVerificationCode.Code == this.Code;
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpFormatValidationResults CheckFormat(string verificationCode)
        {
            Validate.Argument.IsNotNull(verificationCode, nameof(verificationCode));

            if (verificationCode.Length < MinCodeLength || verificationCode.Length > MaxCodeLength)
            {
                return HdvpFormatValidationResults.InvalidLength;
            }

            foreach (var ch in verificationCode)
            {
                if (!CodeEncoding.IsValidSymbol(ch))
                {
                    return HdvpFormatValidationResults.InvalidSymbols;
                }
            }

            var lengthChar = verificationCode[0];
            var expectedLength = CodeEncoding.DecodeSingleSymbol(lengthChar) + MinCodeLength;

            if (verificationCode.Length != expectedLength)
            {
                return HdvpFormatValidationResults.InvalidLength;
            }

            return HdvpFormatValidationResults.Valid;
        }
    }
}
