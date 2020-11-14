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

using System.CommandLine;

using HDVP.Util.Utils;

namespace HDVP.Util.CliApp
{
    /// <summary>
    /// Represents an option (like "--verbose" or "--force") in a command line call. Options are
    /// either attached to a <see cref="CliCommand"/> (or to the "root command" for applications
    /// that don't need commands) or a globally available to all commands.
    /// </summary>
    /// <typeparam name="T">the type this option converts into</typeparam>
    public class CliOption<T> where T : notnull
    {
        private readonly Option<T> m_underlyingImplementation;

        /// <summary>
        /// The description/help text of this option.
        /// </summary>
        public string? Description
        {
            get => this.m_underlyingImplementation.Description;
            set => this.m_underlyingImplementation.Description = value;
        }

        /*
        private T? m_defaultValue;

        /// <summary>
        /// The default value of this option.
        /// </summary>
        /*public T? DefaultValue
        {
            get => this.m_defaultValue;
            set
            {
                this.m_defaultValue = value;

                if (value.IsSet)
                {
                    this.m_underlyingImplementation.Argument = new Argument<T>(getDefaultValue: () => this.m_defaultValue.Value);
                    this.m_underlyingImplementation.IsRequired = false;
                }
                else
                {
                    this.m_underlyingImplementation.Argument = new Argument<T>();
                    this.m_underlyingImplementation.IsRequired = true;
                }
            }
        }*/

        public CliOption(string primaryName, params string[] aliases)
        {
            this.m_underlyingImplementation = new Option<T>(primaryName)
            {
                IsRequired = true,
            };

            foreach (var alias in aliases)
            {
                this.m_underlyingImplementation.AddAlias(alias);
            }
        }
    }
}
