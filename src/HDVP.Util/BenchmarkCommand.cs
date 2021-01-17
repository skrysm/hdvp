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
using System.Text;

using AppMotor.CliApp.CommandLine;
using AppMotor.CliApp.Terminals;
using AppMotor.Core.Exceptions;

using HDVP.Internals;
using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal sealed class BenchmarkCommand : CliCommand
    {
        /// <inheritdoc />
        public override string? HelpText => LocalizableResources.HelpText_Benchmark;

        private CliParam<int> Seconds { get; } = new("--seconds", "-s")
        {
            HelpText = LocalizableResources.HelpText_Benchmark_Seconds,
            DefaultValue = 10,
        };

        private CliParam<int> HashLength { get; } = new("--hash-length", "-l")
        {
            HelpText = LocalizableResources.HelpText_Benchmark_HashLength,
            DefaultValue = 8,
        };

        /// <inheritdoc />
        protected override CliCommandExecutor Executor => new(Execute);

        private void Execute()
        {
            if (this.Seconds.Value < 1)
            {
                throw new ErrorMessageException(LocalizableResources.Benchmark_Error_TooFewSeconds);
            }

            Terminal.WriteLine(LocalizableResources.Benchmark_CalculateFirstHash);
            Terminal.WriteLine();

            var salt = HdvpSalt.CreateNewSalt();
            var verifiableData = HdvpVerifiableData.ReadFromMemory(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consetetur sadipscing elitr"));

            var firstSlowHash = HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: this.HashLength.Value);

            Terminal.WriteLine(LocalizableResources.Benchmark_FirstHashResult + " " + BitConverter.ToString(firstSlowHash));
            Terminal.WriteLine();
            Terminal.WriteLine();

            Terminal.WriteLine(LocalizableResources.Benchmark_RunIntro, this.Seconds.Value);

            var testTime = TimeSpan.FromSeconds(this.Seconds.Value);
            var startTime = DateTime.UtcNow;

            int hashCount = 0;
            while (DateTime.UtcNow - startTime < testTime)
            {
                // ReSharper disable once MustUseReturnValue
                HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: this.HashLength.Value);
                hashCount++;
            }

            var timeSpent = DateTime.UtcNow - startTime;
            Terminal.WriteLine(LocalizableResources.Benchmark_HashesPerSecondResult, hashCount / timeSpent.TotalSeconds);
        }
    }
}
