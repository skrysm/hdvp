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

namespace HDVP.Internals
{
    /// <summary>
    /// Interface for the text encoding used for verification codes.
    /// </summary>
    /// <seealso cref="HdvpVerificationCode"/>
    internal interface IVerificationCodeEncoding
    {
        /// <summary>
        /// The number of symbols available in this encoding.
        /// </summary>
        int AvailableSymbolCount { get; }

        /// <summary>
        /// Checks whether the specified character is a valid symbol in this encoding.
        /// </summary>
        [Pure]
        bool IsValidSymbol(char symbol);

        /// <summary>
        /// Encodes the specified byte array into a string.
        /// </summary>
        [Pure]
        string EncodeBytes(byte[] bytes);

        /// <summary>
        /// Encodes the specified value into a single symbol. Throws an exception
        /// if the value doesn't fix into one symbol.
        /// </summary>
        [Pure]
        char EncodeSingleValue(int value);

        /// <summary>
        /// Decodes a single symbol into its numeric value. Throws an exception if
        /// the symbol is not part of this encoding.
        /// </summary>
        [Pure]
        int DecodeSingleSymbol(char symbol);

        /// <summary>
        /// Returns an number of bytes required to produce an encoded string with
        /// the length of <paramref name="expectedStringLength"/>. Note that the
        /// number of bytes can be an approximation - i.e. it may be greater than
        /// the smallest possible number.
        /// </summary>
        [Pure]
        int GetRequiredByteCount(int expectedStringLength);
    }
}
