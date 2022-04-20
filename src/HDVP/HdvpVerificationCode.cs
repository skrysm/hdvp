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

using AppMotor.Core.Utils;

using HDVP.Internals;

using JetBrains.Annotations;

namespace HDVP;

/// <summary>
/// Represents a HDVP verification code - consisting of the <see cref="Code"/> itself and the
/// <see cref="Salt"/> used to create it. Binary data can be verified via <see cref="IsMatch"/>.
///
/// <para>Instances of this class always represent "valid" verification code - i.e. codes that
/// have the correct format. The format of a string can be checked with <see cref="CheckFormat"/>.
/// </para>
///
/// <para>Instance of this class can either be created for an existing code via <see cref="Create(string,HdvpSalt)"/>
/// or obtained via <see cref="HdvpVerificationCodeProvider.GetVerificationCode"/>.</para>
/// </summary>
public sealed class HdvpVerificationCode
{
    /// <summary>
    /// The text encoding used for verification codes.
    /// </summary>
    private static IVerificationCodeEncoding CodeEncoding { get; } = new ZBase32VerificationCodeEncoding();

    /// <summary>
    /// The minimum length (i.e. <c>Code.Length</c>) of a verification code (including length character).
    /// </summary>
    /// <seealso cref="MaxCodeLength"/>
    [PublicAPI]
    public static int MinCodeLength => 9;

    /// <summary>
    /// The maximum length (i.e. <c>Code.Length</c>) of a verification code (including length character).
    /// </summary>
    /// <seealso cref="MinCodeLength"/>
    [PublicAPI]
    // NOTE: We need to use "- 1" because for the length character we need to encode the values "0..31" - not "1..32".
    public static int MaxCodeLength { get; } = MinCodeLength + CodeEncoding.AvailableSymbolCount - 1;

    /// <summary>
    /// The verification code itself.
    /// </summary>
    [PublicAPI]
    public string Code { get; }

    /// <summary>
    /// The salt that was used to create this verification code.
    /// </summary>
    [PublicAPI]
    public HdvpSalt Salt { get; }

    private HdvpVerificationCode(string code, HdvpSalt salt)
    {
        this.Code = code;
        this.Salt = salt;
    }

    /// <summary>
    /// Creates an instance of this class from an existing verification code and its salt.
    ///
    /// <para>This main purpose of this method is to be used on the "client" side - i.e.
    /// it takes the received salt and the verification code the user entered. For the
    /// server side, see <see cref="HdvpVerificationCodeProvider.GetVerificationCode"/>.
    /// </para>
    /// </summary>
    /// <param name="verificationCode">The verification code (the user entered)</param>
    /// <param name="salt">The salt received from the server</param>
    /// <exception cref="ArgumentException">Thrown when the format of the <paramref name="verificationCode"/>
    /// is invalid - i.e. if <see cref="CheckFormat"/> returns anything but
    /// <see cref="HdvpFormatValidationResults.Valid"/>.</exception>
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

    /// <summary>
    /// Creates an instance of this class from its input data (on the server side).
    /// </summary>
    /// <param name="data">The binary data to verify</param>
    /// <param name="salt">The salt to be used</param>
    /// <param name="codeLength">The (expected) length of the code in characters.
    /// This length includes the length character itself. Must be greater than or
    /// equal to <see cref="MinCodeLength"/> and smaller than or equal to
    /// <see cref="MaxCodeLength"/>.</param>
    [MustUseReturnValue]
    internal static HdvpVerificationCode Create(HdvpVerifiableData data, HdvpSalt salt, int codeLength)
    {
        Validate.ArgumentWithName(nameof(data)).IsNotNull(data);

        if (codeLength < MinCodeLength || codeLength > MaxCodeLength)
        {
            throw new ArgumentException($"The code length must be at least {MinCodeLength} and at most {MaxCodeLength}.");
        }

        // Calculate the number of required slow hash bytes
        int hashByteCount = CodeEncoding.GetRequiredByteCount(codeLength - 1);

        // Calculate the slow hash
        var slowHash = HdvpSlowHashAlgorithm.CreateHash(data, salt, byteCount: hashByteCount);

        // Convert the has to z-base-32
        var slowHashAsZBase32 = CodeEncoding.EncodeBytes(slowHash);

        // Encode code length
        char lengthChar = CodeEncoding.EncodeSingleValue(codeLength - MinCodeLength);

        // Assemble verification code
        var verificationCode = lengthChar + slowHashAsZBase32.Substring(0, codeLength - 1);

        return new HdvpVerificationCode(verificationCode, salt);
    }

    /// <summary>
    /// Checks whether the specified data matches this verification code.
    /// </summary>
    [PublicAPI, Pure]
    public bool IsMatch(HdvpVerifiableData data)
    {
        var dataVerificationCode = Create(data, this.Salt, this.Code.Length);

        return dataVerificationCode.Code == this.Code;
    }

    /// <summary>
    /// Checks the format of the specified verification code and returns what's wrong
    /// with it - or <see cref="HdvpFormatValidationResults.Valid"/> if it's format
    /// is valid.
    ///
    /// <para>Note: This method just checks the format (see remarks for details). It
    /// basically says whether this code could(!) be valid when checked against some
    /// binary data via <see cref="IsMatch"/>.</para>
    /// </summary>
    /// <remarks>
    /// This method does a couple of things:
    ///
    /// <para>1. It checks that the length of the code falls in the range of
    /// <see cref="MinCodeLength"/> and <see cref="MaxCodeLength"/>. If not,
    /// <see cref="HdvpFormatValidationResults.InvalidLength"/> is returned.</para>
    ///
    /// <para>2. It checks that all characters of this code are valid z-base-32 characters.
    /// If not, <see cref="HdvpFormatValidationResults.InvalidSymbols"/> is returned.</para>
    ///
    /// <para>3. It checks whether the code's expected length - encoded in the first character -
    /// matches the actual code length. If not, <see cref="HdvpFormatValidationResults.InvalidLength"/>
    /// is returned.</para>
    /// </remarks>
    [PublicAPI, MustUseReturnValue]
    public static HdvpFormatValidationResults CheckFormat(string verificationCode)
    {
        Validate.ArgumentWithName(nameof(verificationCode)).IsNotNull(verificationCode);

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
