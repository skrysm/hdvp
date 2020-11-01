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

using JetBrains.Annotations;

namespace HDVP
{
    public class HdvpVerificationCodeProvider
    {
        [PublicAPI]
        public static readonly TimeSpan DEFAULT_TIME_TO_LIVE = TimeSpan.FromMinutes(1);

        /// <summary>
        /// The default verification code length.
        /// </summary>
        [PublicAPI]
        public static readonly int DEFAULT_CODE_LENGTH = 12;

        [PublicAPI]
        public DateTime VerificationCodeValidUntil { get; private set; }

        private HdvpVerificationCode? m_currentVerificationCode;

        private readonly HdvpVerifiableData m_data;

        private readonly int m_verificationCodeLength;

        private readonly TimeSpan m_verificationCodeTimeToLive;

        private readonly IDateTimeProvider m_dateTimeProvider;

        public HdvpVerificationCodeProvider(
                HdvpVerifiableData data,
                IDateTimeProvider? dateTimeProvider = null
            )
                : this(data, verificationCodeLength: DEFAULT_CODE_LENGTH, DEFAULT_TIME_TO_LIVE, dateTimeProvider)
        {
        }

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

            var salt = HdvpSalt.CreateNewSalt();
            this.m_currentVerificationCode = HdvpVerificationCode.Create(this.m_data, salt, this.m_verificationCodeLength);
            this.VerificationCodeValidUntil = this.m_dateTimeProvider.UtcNow + this.m_verificationCodeTimeToLive;

            return this.m_currentVerificationCode;
        }
    }
}
