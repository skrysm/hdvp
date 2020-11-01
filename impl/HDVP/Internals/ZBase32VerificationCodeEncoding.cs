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

using AppMotor.Core.Utils;

namespace HDVP.Internals
{
    internal sealed class ZBase32VerificationCodeEncoding : IVerificationCodeEncoding
    {
        private const string ZBASE32_SYMBOLS = "ybndrfg8ejkmcpqxot1uwisza345h769";

        private static readonly Base32Encoding ZBASE32_ENCODING = new Base32Encoding(ZBASE32_SYMBOLS, paddingChar: null);

        private static readonly Dictionary<char, byte> ZBASE32_INVERSE_SYMBOLS = Rfc4648Encoding.CreateInverseSymbolsDictionary(ZBASE32_SYMBOLS);

        private const int BITS_PER_BYTE = 8;

        /// <summary>
        /// 2^5 = 32 -> Base32
        /// </summary>
        private const int BITS_PER_SYMBOL = 5;

        /// <summary>
        /// The least common multiple between <see cref="BITS_PER_SYMBOL"/> and <see cref="BITS_PER_BYTE"/>.
        /// </summary>
        private const int BITS_PER_GROUP = 40;

        /// <summary>
        /// 8
        /// </summary>
        private const int SYMBOLS_PER_GROUP = BITS_PER_GROUP / BITS_PER_SYMBOL;

        /// <summary>
        /// 5
        /// </summary>
        private const int BYTES_PER_GROUP = BITS_PER_GROUP / BITS_PER_BYTE;

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

        /// <inheritdoc />
        public int GetRequiredByteCount(int expectedStringLength)
        {
            int groupCount;

            if (expectedStringLength % SYMBOLS_PER_GROUP == 0)
            {
                groupCount = expectedStringLength / SYMBOLS_PER_GROUP;
            }
            else
            {
                groupCount = (int)Math.Ceiling((double)expectedStringLength / SYMBOLS_PER_GROUP);
            }

            return groupCount * BYTES_PER_GROUP;
        }
    }
}
