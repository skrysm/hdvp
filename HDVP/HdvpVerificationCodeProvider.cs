using System;
using System.Collections.Immutable;
using System.Security.Cryptography;

using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace HDVP
{
    public class HdvpVerificationCodeProvider
    {
        [PublicAPI]
        public static readonly TimeSpan DEFAULT_TIME_TO_LIVE = TimeSpan.FromMinutes(1);

        [PublicAPI]
        public DateTime VerificationCodeValidUntil { get; private set; }

        private HdvpVerificationCode? m_currentVerificationCode;

        private readonly HdvpVerifiableData m_data;

        private readonly int m_verificationCodeLength;

        private readonly TimeSpan m_verificationCodeTimeToLive;

        public HdvpVerificationCodeProvider(HdvpVerifiableData data, int verificationCodeLength)
            : this(data, verificationCodeLength, DEFAULT_TIME_TO_LIVE)
        {
        }

        public HdvpVerificationCodeProvider(HdvpVerifiableData data, int verificationCodeLength, TimeSpan verificationCodeTimeToLive)
        {
            Validate.Argument.IsNotNull(data, nameof(data));

            this.m_data = data;
            this.m_verificationCodeLength = verificationCodeLength;
            this.m_verificationCodeTimeToLive = verificationCodeTimeToLive;

            this.VerificationCodeValidUntil = DateTime.UtcNow + verificationCodeTimeToLive;
        }

        [PublicAPI]
        public HdvpVerificationCode GetVerificationCode()
        {
            if (this.m_currentVerificationCode != null && DateTime.UtcNow < this.VerificationCodeValidUntil)
            {
                return this.m_currentVerificationCode;
            }

            var salt = CreateSalt();
            this.m_currentVerificationCode = HdvpVerificationCode.Create(this.m_data, salt, this.m_verificationCodeLength);
            this.VerificationCodeValidUntil = DateTime.UtcNow + this.m_verificationCodeTimeToLive;

            return this.m_currentVerificationCode;
        }

        [MustUseReturnValue]
        private static ImmutableArray<byte> CreateSalt()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();

            var salt = new byte[HdvpVerificationCode.SALT_LENGTH];
            rngCryptoServiceProvider.GetBytes(salt);

            return salt.ToImmutableArray();
        }
    }
}
