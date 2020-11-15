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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

using AppMotor.Core.Exceptions;
using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace HDVP.Util.CliApp
{
    public abstract class CliParam
    {
        internal abstract Symbol UnderlyingImplementation { get; }

        internal abstract void SetValueFromParseResult(ParseResult parseResult);
    }

    public class CliParam<T> : CliParam where T : notnull
    {
        /// <summary>
        /// The primary name for this parameter.
        /// </summary>
        [PublicAPI]
        public string PrimaryName => this.Names[0];

        /// <summary>
        /// Whether this parameter is a positional parameter (created via <see cref="CliParam{T}(string,int)"/>).
        /// Positional parameters are identified only by their position on the command line (and not by their name).
        /// For example, a copy command would be <c>copy "c:\test.txt" "d:\"</c> with both parameters being
        /// positional ones (first: source, second: destination).
        ///
        /// <para>Opposite of a named parameter (see <see cref="IsNamedParameter"/>).</para>
        /// </summary>
        [PublicAPI]
        public bool IsPositionalParameter => this.PositionIndex != null;

        /// <summary>
        /// Whether this parameter is a named parameter (created via <see cref="CliParam{T}(string,string[])"/>).
        /// Named parameters can either be flags/standalone (only for <c>bool</c> parameters; like <c>--verbose</c>) or
        /// key value pairs (like <c>--message "my text"</c>).
        ///
        /// <para>Opposite of a positional parameter (see <see cref="IsPositionalParameter"/>).</para>
        /// </summary>
        [PublicAPI]
        public bool IsNamedParameter => !this.IsPositionalParameter;

        /// <summary>
        /// The names (or aliases) of this parameter.
        ///
        /// <para>Note: Positional parameters (<see cref="IsPositionalParameter"/>) always only have one name.</para>
        /// </summary>
        [PublicAPI]
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// The position of this positional parameter among all other positional parameters; positional parameters
        /// are ordered by this value. No two positional parameter can have the same position index.
        ///
        /// <para>Is <c>null</c> for named parameters (<see cref="IsNamedParameter"/>).</para>
        /// </summary>
        /// <remarks>
        /// This property is a safety feature. It primarily exists because positional parameters must be ordered, and because,
        /// by default, we use reflection to find all parameters of a command. If this property didn't exist, we would rely
        /// on the order of <see cref="Type.GetProperties()"/> (which may be arbitrary) - or, if the order is always the same
        /// in which the properties are declared - we would require developers to order their properties in a certain way, which
        /// is problematic because the order of members in a type normally doesn't matter (and through refactoring the
        /// order of the properties could change and thus change the order of the positional parameters would change).
        /// </remarks>
        [PublicAPI]
        public int? PositionIndex { get; }

        /// <summary>
        /// The help text for this parameter.
        /// </summary>
        public string? HelpText { get; init; }

        /// <summary>
        /// The default value of this parameter. If set, the parameter is considered "optional"; if
        /// not set, the parameter is considered "required".
        /// </summary>
        public T? DefaultValue { get; init; }

        private readonly Lazy<Symbol> m_underlyingImplementation;

        /// <inheritdoc />
        internal override Symbol UnderlyingImplementation => this.m_underlyingImplementation.Value;

        /// <summary>
        /// The value of this parameter. Only set if <see cref="CliCommand.Execute"/> of the containing class is executed.
        /// </summary>
        public T Value
        {
            get
            {
                if (!this.m_hasValueBeenSet)
                {
                    if (this.DefaultValue != null)
                    {
                        return this.DefaultValue;
                    }
                    else
                    {
                        throw new InvalidOperationException("This value can't be accessed at this state.");
                    }
                }

                return this.m_value!;
            }
        }

        private T? m_value;

        private bool m_hasValueBeenSet;

        /// <summary>
        /// Creates a named parameter (in contrast to a positional one). See <see cref="IsNamedParameter"/> for more details.
        /// </summary>
        /// <param name="primaryName">The primary name for this parameter; for better documentation purposes this
        /// should start with either "--", "-" or "/".</param>
        /// <param name="aliases">Other names that represent the same parameter. Usually the <paramref name="primaryName"/>
        /// would be the long form (like <c>--length</c>) and the alias(es) would be the short form (like <c>-l</c>).</param>
        public CliParam(string primaryName, params string[] aliases)
        {
            this.Names = ParamsUtils.Combine(primaryName, aliases).ToImmutableList();

            this.m_underlyingImplementation = new Lazy<Symbol>(CreateUnderlyingNamedParameter);
        }

        /// <summary>
        /// Creates a positional parameter (in contrast to a named parameter). See <see cref="IsPositionalParameter"/> for more details.
        /// </summary>
        /// <param name="name">The name of this parameter; only used for generating the help text.</param>
        /// <param name="positionIndex">The position of this parameter among all other positional parameters; positional parameters
        /// are ordered by this value</param>
        public CliParam(string name, int positionIndex)
        {
            this.Names = new[] { name }.ToImmutableList();
            this.PositionIndex = positionIndex;

            this.m_underlyingImplementation = new Lazy<Symbol>(CreateUnderlyingPositionalParameter);
        }

        private Symbol CreateUnderlyingNamedParameter()
        {
            var option = new Option<T>(this.Names.ToArray(), this.HelpText);

            if (this.DefaultValue != null)
            {
                option.Argument.SetDefaultValue(this.DefaultValue);
            }

            return option;
        }

        private Symbol CreateUnderlyingPositionalParameter()
        {
            var argument = new Argument<T>(this.PrimaryName, this.HelpText);

            if (this.DefaultValue != null)
            {
                argument.SetDefaultValue(this.DefaultValue);
            }

            return argument;
        }

        internal override void SetValueFromParseResult(ParseResult parseResult)
        {
            switch (this.UnderlyingImplementation)
            {
                case Option<T> option:
                {
                    OptionResult? result = parseResult.FindResultFor(option);

                    if (result is null)
                    {
                        this.m_value = this.DefaultValue;
                    }
                    else
                    {
                        this.m_value = result.GetValueOrDefault<T>()!;
                    }

                    break;
                }

                case Argument<T> argument:
                {
                    ArgumentResult? result = parseResult.FindResultFor(argument);

                    if (result is null)
                    {
                        this.m_value = this.DefaultValue;
                    }
                    else
                    {
                        this.m_value = result.GetValueOrDefault<T>()!;
                    }

                    break;
                }

                default:
                    throw new UnexpectedSwitchValueException(nameof(this.UnderlyingImplementation), this.UnderlyingImplementation.GetType());
            }

            this.m_hasValueBeenSet = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.PrimaryName;
        }
    }
}
