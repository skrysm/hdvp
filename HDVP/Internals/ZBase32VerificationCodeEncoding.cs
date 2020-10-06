using System.Collections.Generic;

using AppMotor.Core.Utils;

namespace HDVP.Internals
{
    internal sealed class ZBase32VerificationCodeEncoding : IVerificationCodeEncoding
    {
        private const string ZBASE32_SYMBOLS = "ybndrfg8ejkmcpqxot1uwisza345h769";

        private static readonly Base32Encoding ZBASE32_ENCODING = new Base32Encoding(ZBASE32_SYMBOLS, paddingChar: null);

        private static readonly Dictionary<char, byte> ZBASE32_INVERSE_SYMBOLS = Rfc4648Encoding.CreateInverseSymbolsDictionary(ZBASE32_SYMBOLS);

        /// <inheritdoc />
        public int AvailableSymbolCount => ZBASE32_SYMBOLS.Length;

        /// <inheritdoc />
        public bool IsValidSymbol(char symbol)
        {
            return ZBASE32_INVERSE_SYMBOLS.ContainsKey(symbol);
        }

        /// <inheritdoc />
        public string EncodeBytes(byte[] bytes)
        {
            return ZBASE32_ENCODING.Encode(bytes);
        }

        /// <inheritdoc />
        public char EncodeSingleValue(int value)
        {
            return ZBASE32_SYMBOLS[value];
        }

        /// <inheritdoc />
        public int DecodeSingleSymbol(char symbol)
        {
            return ZBASE32_INVERSE_SYMBOLS[symbol];
        }
    }
}
