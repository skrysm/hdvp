﻿#region License
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

using JetBrains.Annotations;

namespace HDVP.Util.Utils
{
    /// <summary>
    /// Represents a value that may or may not be set.
    ///
    /// <para>The primary use case for this type is to be with a nullable type where
    /// <c>null</c> is a valid value and thus can't be used to signal whether the value
    /// is set or not.
    /// </para>
    /// </summary>
    public readonly struct Optional<T>
    {
        /// <summary>
        /// You may use this to unset an optional value.
        /// </summary>
        [PublicAPI]
        public static readonly Optional<T> UNSET = new Optional<T>();

        private readonly T m_value;

        /// <summary>
        /// The value. Can only be obtained if <see cref="IsSet"/> is <c>true</c>;
        /// otherwise an exception will be thrown.
        /// </summary>
        public T Value
        {
            get
            {
                if (!this.IsSet)
                {
                    throw new InvalidOperationException("This value is not set.");
                }

                return this.m_value;
            }
        }

        /// <summary>
        /// Whether this value is set.
        /// </summary>
        public bool IsSet { get; }

        /// <inheritdoc />
        public Optional(T value) : this()
        {
            this.m_value = value;
            this.IsSet = true;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }
    }
}
