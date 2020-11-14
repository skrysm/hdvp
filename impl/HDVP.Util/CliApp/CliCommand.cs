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
using System.CommandLine;
using System.Reflection;
using System.Threading.Tasks;

using AppMotor.Core.Extensions;

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

        private readonly Lazy<Command> m_underlyingImplementation;

        internal Command UnderlyingImplementation => this.m_underlyingImplementation.Value;

        protected CliCommand()
        {
            this.m_underlyingImplementation = new Lazy<Command>(ToUnderlyingImplementation);
        }

        public abstract int Execute(CliValues args);

        private Command ToUnderlyingImplementation()
        {
            var command = new Command(this.Name, this.HelpText);

            var thisType = GetType();

            foreach (var propertyInfo in thisType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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

                command.AddOption(cliParam.UnderlyingImplementation);
            }

            return command;
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
