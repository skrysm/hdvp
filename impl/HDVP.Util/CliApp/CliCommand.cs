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
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using AppMotor.Core.Exceptions;
using AppMotor.Core.Extensions;

using JetBrains.Annotations;

namespace HDVP.Util.CliApp
{
    /// <summary>
    /// Represents a command/verb in a command line call; e.g. in "git add ." the word "add" is the command.
    ///
    /// <para>Commands can be nested like "myapp command1 subcommmand --some-option".</para>
    /// </summary>
    public abstract class CliCommand
    {
        public abstract string Name { get; }

        public virtual IReadOnlyList<string>? Aliases => null;

        public virtual string? HelpText => null;

        private ImmutableList<CliParam>? m_allParams;

        private readonly Lazy<Command> m_underlyingImplementation;

        internal Command UnderlyingImplementation => this.m_underlyingImplementation.Value;

        protected CliCommand()
        {
            this.m_underlyingImplementation = new Lazy<Command>(ToUnderlyingImplementation);
        }

        /// <summary>
        /// Runs this command.
        /// </summary>
        /// <returns>The exit code for the running program.</returns>
        [PublicAPI]
        public abstract int Execute();

        /// <summary>
        /// Returns all parameters defined for this command. The default implementation uses reflection to find all properties
        /// of type <see cref="CliParam"/>. Inheritors may override this method either to filter its result or completely change
        /// the way the parameters are found.
        /// </summary>
        [PublicAPI]
        protected virtual IEnumerable<CliParam> GetAllParams()
        {
            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!propertyInfo.PropertyType.Is<CliParam>())
                {
                    continue;
                }

                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                var cliParam = (CliParam?)propertyInfo.GetValue(this);
                if (cliParam == null)
                {
                    continue;
                }

                yield return cliParam;
            }
        }

        private Command ToUnderlyingImplementation()
        {
            this.m_allParams = GetAllParams().ToImmutableList();

            var command = new Command(this.Name, this.HelpText);

            foreach (var cliParam in this.m_allParams)
            {
                command.Add(cliParam.UnderlyingImplementation);
            }

            command.Handler = new CliCommandHandler(this);

            return command;
        }

        private void SetAllParamValues(ParseResult parseResult)
        {
            if (this.m_allParams == null)
            {
                throw new UnexpectedBehaviorException("Can't set param values.");
            }

            foreach (var cliParam in this.m_allParams)
            {
                cliParam.SetValueFromParseResult(parseResult);
            }
        }

        private sealed class CliCommandHandler : ICommandHandler
        {
            private readonly CliCommand m_command;

            public CliCommandHandler(CliCommand command)
            {
                this.m_command = command;
            }

            /// <inheritdoc />
            public Task<int> InvokeAsync(InvocationContext context)
            {
                this.m_command.SetAllParamValues(context.ParseResult);

                int retVal = this.m_command.Execute();

                return Task.FromResult(retVal);
            }
        }
    }

    /*
    public abstract class AsyncCliCommand
    {
        public abstract string Name { get; }

        public virtual IReadOnlyList<string>? Aliases => null;

        public abstract Task<int> Execute();
    }
*/
}
