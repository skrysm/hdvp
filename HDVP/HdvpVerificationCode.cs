using System;
using System.Collections.Immutable;
using System.Linq;

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
        public static int SaltLength => 32;

        [PublicAPI]
        public static int MinCodeLength => 9;

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
        public ImmutableArray<byte> Salt { get; }

        private HdvpVerificationCode(string code, ImmutableArray<byte> salt)
        {
            this.Code = code;
            this.Salt = salt;
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpVerificationCode Create(string verificationCode, ImmutableArray<byte> salt)
        {
            var validationResult = CheckFormat(verificationCode);
            if (validationResult != HdvpFormatValidationResults.Valid)
            {
                throw new ArgumentException($"The verification code has a bad format. ({validationResult})", nameof(verificationCode));
            }

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [MustUseReturnValue]
        internal static HdvpVerificationCode Create(HdvpVerifiableData data, ImmutableArray<byte> salt, int codeLength)
        {
            Validate.Argument.IsNotNull(data, nameof(data));

            if (salt.Length != SaltLength)
            {
                throw new ArgumentException($"The provided salt is not {SaltLength} bytes long.", nameof(salt));
            }

            if (codeLength < MinCodeLength || codeLength > MaxCodeLength)
            {
                throw new ArgumentException($"The code length must be at least {MinCodeLength} and at most {MaxCodeLength}.");
            }

            var slowHash = GetSlowHash(data, salt, codeLength);

            var zBase32String = CodeEncoding.EncodeBytes(slowHash);

            var verificationCode = EncodeLength(codeLength) + zBase32String.Substring(0, codeLength - 1);

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [MustUseReturnValue]
        private static byte[] GetSlowHash(HdvpVerifiableData data, ImmutableArray<byte> salt, int codeLength)
        {
            int byteCount = CodeEncoding.GetRequiredByteCount(codeLength);
            return HdvpSlowHashAlgorithm.CreateHash(data.Hash.ToArray(), salt.ToArray(), byteCount: byteCount);
        }

        [MustUseReturnValue]
        private static char EncodeLength(int codeLength)
        {
            return CodeEncoding.EncodeSingleValue(codeLength - MinCodeLength);
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
