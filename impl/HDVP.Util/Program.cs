using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Text;

using AppMotor.Core.System;

using HDVP.Internals;
using HDVP.Util.CliApp;
using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal static class Program
    {
        private sealed class BenchmarkCommand : CliCommand
        {
            /// <inheritdoc />
            public override string Name => "benchmark";

            private CliParam<int> Seconds { get; } =
                new CliParam<int>("--seconds", "-s")
                {
                    HelpText = LocalizableResources.HelpText_Benchmark_Seconds,
                    DefaultValue = 10,
                };

            private CliParam<int> HashLength { get; } =
                new CliParam<int>("--hash-length", "-l")
                {
                    HelpText = LocalizableResources.HelpText_Benchmark_HashLength,
                    DefaultValue = 8,
                };

            /// <inheritdoc />
            public override int Execute(CliValues args)
            {
                if (args.GetValue(this.Seconds) < 1)
                {
                    throw new ArgumentException("Seconds must be greater than 1");
                }

                Terminal.WriteLine(LocalizableResources.Benchmark_CalculateFirstHash);
                Terminal.WriteLine();

                var salt = HdvpSalt.CreateNewSalt();
                var verifiableData = HdvpVerifiableData.ReadFromMemory(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consetetur sadipscing elitr"));

                var firstSlowHash = HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: args.GetValue(this.HashLength));

                Terminal.WriteLine(LocalizableResources.Benchmark_FirstHashResult + " " + BitConverter.ToString(firstSlowHash));
                Terminal.WriteLine();
                Terminal.WriteLine();

                Terminal.WriteLine(LocalizableResources.Benchmark_RunIntro, args.GetValue(this.Seconds));

                var testTime = TimeSpan.FromSeconds(args.GetValue(this.Seconds));
                var startTime = DateTime.UtcNow;

                int hashCount = 0;
                while (DateTime.UtcNow - startTime < testTime)
                {
                    // ReSharper disable once MustUseReturnValue
                    HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: args.GetValue(this.HashLength));
                    hashCount++;
                }

                var timeSpent = DateTime.UtcNow - startTime;
                Terminal.WriteLine(LocalizableResources.Benchmark_HashesPerSecondResult, hashCount / timeSpent.TotalSeconds);

                return 0;
            }
        }

        private static int Main(string[] args)
        {
            /*var benchmarkCommand = new Command("benchmark", LocalizableResources.HelpText_Benchmark)
            {
                new Option<int>(
                    new[] { "--seconds", "-s" },
                    getDefaultValue: () => 10,
                    description: LocalizableResources.HelpText_Benchmark_Seconds
                ),
                new Option<int>(
                    new[] { "--hash-length", "-l" },
                    getDefaultValue: () => 8,
                    description: LocalizableResources.HelpText_Benchmark_HashLength
                ),
            };
            //benchmarkCommand.Handler = CommandHandler.Create<int, int>(RunBenchmark);

            var rootCommand = new RootCommand(LocalizableResources.AppDescription)
            {
                benchmarkCommand,
            };*/

            var rootCommand = new RootCommand(LocalizableResources.AppDescription);

            var benchmarkCommand = new BenchmarkCommand();

            rootCommand.AddCommand(benchmarkCommand.UnderlyingImplementation);

            var parser = new CommandLineBuilder(rootCommand)
                          .UseDefaults()
                          .Build();

            var parseResult = parser.Parse(args);

            benchmarkCommand.UnderlyingImplementation.Handler = CommandHandler.Create(() => benchmarkCommand.Execute(new CliValues(parseResult)));

            return rootCommand.Invoke(args);
        }
    }
}
