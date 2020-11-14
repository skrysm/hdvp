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

using System.Collections.Generic;
using System.CommandLine;

using AppMotor.Core.System;

namespace HDVP.Util.CliApp
{
    public abstract class MultiCommandApplication : ConsoleApplication
    {
        protected abstract string AppDescription { get; }

        /// <inheritdoc />
        protected override int Run(string[] args)
        {
            /*var rootCommand = new RootCommand(this.AppDescription);

            foreach (var cliCommand in GetCommands())
            {
                var underylingCommand = new
            }

            foreach (var option in CreateOptions())
            {
                rootCommand.AddOption(option);
            }

            rootCommand.Handler = CreateCommandHandler();

            return rootCommand.Invoke(args);*/
            return 0;
        }

        protected abstract IEnumerable<CliCommand> GetCommands();
    }
}
