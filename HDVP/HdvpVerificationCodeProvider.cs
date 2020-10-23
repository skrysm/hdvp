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

        private readonly IDateTimeProvider m_dateTimeProvider;

        public HdvpVerificationCodeProvider(
                HdvpVerifiableData data,
                int verificationCodeLength,
                IDateTimeProvider? dateTimeProvider = null
            )
                : this(data, verificationCodeLength, DEFAULT_TIME_TO_LIVE, dateTimeProvider)
        {
        }

        public HdvpVerificationCodeProvider(
                HdvpVerifiableData data,
                int verificationCodeLength,
                TimeSpan verificationCodeTimeToLive,
                IDateTimeProvider? dateTimeProvider = null
            )
        {
            Validate.Argument.IsNotNull(data, nameof(data));

            this.m_data = data;
            this.m_verificationCodeLength = verificationCodeLength;
            this.m_verificationCodeTimeToLive = verificationCodeTimeToLive;
            this.m_dateTimeProvider = dateTimeProvider ?? DefaultDateTimeProvider.Instance;

            this.VerificationCodeValidUntil = this.m_dateTimeProvider.UtcNow + verificationCodeTimeToLive;
        }

        [PublicAPI]
        public HdvpVerificationCode GetVerificationCode()
        {
            if (this.m_currentVerificationCode != null && this.m_dateTimeProvider.UtcNow < this.VerificationCodeValidUntil)
            {
                return this.m_currentVerificationCode;
            }

            var salt = CreateSalt();
            this.m_currentVerificationCode = HdvpVerificationCode.Create(this.m_data, salt, this.m_verificationCodeLength);
            this.VerificationCodeValidUntil = this.m_dateTimeProvider.UtcNow + this.m_verificationCodeTimeToLive;

            return this.m_currentVerificationCode;
        }

        [MustUseReturnValue]
        private static ImmutableArray<byte> CreateSalt()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();

            var salt = new byte[HdvpVerificationCode.SaltLength];
            rngCryptoServiceProvider.GetBytes(salt);

            return salt.ToImmutableArray();
        }
    }
}
