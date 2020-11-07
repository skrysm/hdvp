using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;

using AppMotor.Core.System;

using HDVP.Internals;
using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var benchmarkCommand = new Command("benchmark", LocalizableResources.HelpText_Benchmark)
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
            benchmarkCommand.Handler = CommandHandler.Create<int, int>(RunBenchmark);

            var rootCommand = new RootCommand(LocalizableResources.AppDescription)
            {
                benchmarkCommand,
            };

            // Parse the incoming args and invoke the handler
            return rootCommand.Invoke(args);
        }

        private static int RunBenchmark(int seconds, int hashLength)
        {
            if (seconds < 1)
            {
                throw new ArgumentException("Seconds must be greater than 1");
            }

            Terminal.WriteLine(LocalizableResources.Benchmark_CalculateFirstHash);
            Terminal.WriteLine();

            var salt = HdvpSalt.CreateNewSalt();
            var verifiableData = HdvpVerifiableData.ReadFromMemory(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consetetur sadipscing elitr"));

            var firstSlowHash = HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: hashLength);

            Terminal.WriteLine(LocalizableResources.Benchmark_FirstHashResult + " " + BitConverter.ToString(firstSlowHash));
            Terminal.WriteLine();
            Terminal.WriteLine();

            Terminal.WriteLine($"Calculating hashes per second for the next {seconds} seconds...");

            var testTime = TimeSpan.FromSeconds(seconds);
            var startTime = DateTime.UtcNow;

            int hashCount = 0;
            while (DateTime.UtcNow - startTime < testTime)
            {
                // ReSharper disable once MustUseReturnValue
                HdvpSlowHashAlgorithm.CreateHash(verifiableData, salt, byteCount: 8);
                hashCount++;
            }

            var timeSpent = DateTime.UtcNow - startTime;
            Console.WriteLine($"Hashes per second: {hashCount / timeSpent.TotalSeconds:0.0}");

            return 0;
        }
    }
}
