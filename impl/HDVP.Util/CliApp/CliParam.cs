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
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace HDVP.Util.CliApp
{
    public abstract class CliParam
    {
        internal abstract Option UnderlyingImplementation { get; }

        internal abstract void SetValueFromParseResult(ParseResult parseResult);
    }

    public class CliParam<T> : CliParam where T : notnull
    {
        /// <summary>
        /// The names (or aliases) of this parameter.
        /// </summary>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// The help text for this parameter.
        /// </summary>
        public string? HelpText { get; init; }

        /// <summary>
        /// The default value of this parameter. If set, the parameter is considered "optional"; if
        /// not set, the parameter is considered "required".
        /// </summary>
        public T? DefaultValue { get; init; }

        /// <summary>
        /// The function used to convert the string representation of the parameter into <typeparamref name="T"/>. If
        /// <c>null</c>, the default parser function will be used.
        /// </summary>
        public Func<string, T?>? ParserFunc { get; init; }

        private readonly Lazy<Option> m_underlyingImplementation;

        /// <inheritdoc />
        internal override Option UnderlyingImplementation => this.m_underlyingImplementation.Value;

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

        public CliParam(string primaryName, params string[] aliases)
        {
            this.Names = ParamsUtils.Combine(primaryName, aliases).ToImmutableList();

            this.m_underlyingImplementation = new Lazy<Option>(ToUnderlyingImplementation);
        }

        internal override void SetValueFromParseResult(ParseResult parseResult)
        {
            OptionResult? result = parseResult.FindResultFor(this.UnderlyingImplementation);

            if (result is null)
            {
                this.m_value = this.DefaultValue;
            }
            else
            {
                this.m_value = result.GetValueOrDefault<T>()!;
            }

            this.m_hasValueBeenSet = true;
        }

        private Option ToUnderlyingImplementation()
        {
            var option = new Option<T>(this.Names.ToArray(), this.HelpText);

            if (this.DefaultValue != null)
            {
                option.Argument.SetDefaultValue(this.DefaultValue);
            }

            // TODO: Set parse func

            return option;
        }

    }
}
