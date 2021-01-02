using AppMotor.CliApp.CommandLine;

using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CliApplicationWithVerbs()
            {
                AppDescription = LocalizableResources.AppDescription,

                Verbs = new[]
                {
                    new CliVerb("benchmark", new BenchmarkCommand()),
                },
            };

            return app.Run(args);
        }
    }
}
