using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

using AppMotor.Core.Utils;

using CommandLine;

using HDVP.Util.Properties;

using JetBrains.Annotations;

namespace HDVP.Util
{
    internal static class Program
    {
        [Verb("benchmark", HelpText = "blaa")]
        private sealed class BenchmarkOptions
        {

        }

        private static int Main(string[] args)
        {
            // using var parser = new Parser(config =>
            //     {
            //         config.HelpWriter = Console.Out;
            //         config.AutoHelp = true;
            //     }
            // );
            // parser
            //        .ParseArguments<BenchmarkOptions>(args)
            //        .WithParsed(opts => RunBenchmark(opts))        ;
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<int>(
                    "--int-option",
                    getDefaultValue: () => 42,
                    description: "An option whose argument is parsed as an int"
                ),
                new Option<bool>(
                    "--bool-option",
                    "An option whose argument is parsed as a bool"),
                new Option<FileInfo>(
                    "--file-option",
                    "An option whose argument is parsed as a FileInfo"
                )
                {
                    IsRequired = true,
                },
            };

            rootCommand.Description = "My sample app";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<int, bool, FileInfo>((intOption, boolOption, fileOption) =>
            {
                Console.WriteLine($"The value for --int-option is: {intOption}");
                Console.WriteLine($"The value for --bool-option is: {boolOption}");
                Console.WriteLine($"The value for --file-option is: {fileOption?.FullName ?? "null"}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.Invoke(args);
        }

        /*
        private static int RunBenchmark([NotNull] BenchmarkOptions options)
        {
            Validate.Argument.IsNotNull(options, nameof(options));

            return 0;
        }
    */
    }
}
