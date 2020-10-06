using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace HDVP
{
    public sealed class HdvpVerificationCode
    {
        private const string ZBASE32_SYMBOLS = "ybndrfg8ejkmcpqxot1uwisza345h769";

        private static readonly Base32Encoding ZBASE32_ENCODING = new Base32Encoding(ZBASE32_SYMBOLS, paddingChar: null);

        private static readonly Dictionary<char, byte> ZBASE32_INVERSE_SYMBOLS = Rfc4648Encoding.CreateInverseSymbolsDictionary(ZBASE32_SYMBOLS);

        [PublicAPI]
        public static readonly int SALT_LENGTH = 32;

        [PublicAPI]
        public static readonly int MIN_CODE_LENGTH = 9;

        [PublicAPI]
        public static readonly int MAX_CODE_LENGTH = 9 + ZBASE32_SYMBOLS.Length;

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
            if (CheckFormat(verificationCode) != HdvpFormatValidationResults.Valid)
            {
                throw new ArgumentException("The verification code has a bad format.", nameof(verificationCode));
            }

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [PublicAPI, MustUseReturnValue]
        public static HdvpVerificationCode Create(HdvpVerifiableData data, ImmutableArray<byte> salt, int codeLength)
        {
            Validate.Argument.IsNotNull(data, nameof(data));

            if (salt.Length != SALT_LENGTH)
            {
                throw new ArgumentException($"The provided salt is not {SALT_LENGTH} bytes long.", nameof(salt));
            }

            if (codeLength < MIN_CODE_LENGTH || codeLength > MAX_CODE_LENGTH)
            {
                throw new ArgumentException($"The code length must be at least {MIN_CODE_LENGTH} and at most {MAX_CODE_LENGTH}.");
            }

            var slowHash = GetSlowHash(data, salt, codeLength);

            var zBase32String = ConvertToZBase32(slowHash);

            var verificationCode = EncodeLength(codeLength) + zBase32String.Substring(0, codeLength - 1);

            return new HdvpVerificationCode(verificationCode, salt);
        }

        [MustUseReturnValue]
        private static byte[] GetSlowHash(HdvpVerifiableData data, ImmutableArray<byte> salt, int codeLength)
        {
            // TODO: Calculate byte count from code length
            return HdvpSlowHashAlgorithm.CreateHash(data.Hash.ToArray(), salt.ToArray(), byteCount: 16);
        }

        [MustUseReturnValue]
        private static string ConvertToZBase32(byte[] hash)
        {
            return ZBASE32_ENCODING.Encode(hash);
        }

        [MustUseReturnValue]
        private static char EncodeLength(int codeLength)
        {
            return ZBASE32_SYMBOLS[codeLength - MIN_CODE_LENGTH];
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

            if (verificationCode.Length < MIN_CODE_LENGTH || verificationCode.Length > MAX_CODE_LENGTH)
            {
                return HdvpFormatValidationResults.InvalidLength;
            }

            foreach (var ch in verificationCode)
            {
                if (!ZBASE32_INVERSE_SYMBOLS.ContainsKey(ch))
                {
                    return HdvpFormatValidationResults.InvalidSymbols;
                }
            }

            var lengthChar = verificationCode[0];
            var expectedLength = ZBASE32_INVERSE_SYMBOLS[lengthChar] + MIN_CODE_LENGTH;

            if (verificationCode.Length != expectedLength)
            {
                return HdvpFormatValidationResults.InvalidLength;
            }

            return HdvpFormatValidationResults.Valid;
        }
    }
}
