namespace HDVP
{
    public enum HdvpFormatValidationResults
    {
        /// <summary>
        /// The format of the verification code is valid.
        /// </summary>
        Valid,

        /// <summary>
        /// The verification code contains invalid symbols (characters).
        /// </summary>
        InvalidSymbols,

        /// <summary>
        /// The length of the verification code is invalid.
        /// </summary>
        InvalidLength,
    }
}
