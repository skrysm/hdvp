using System.Collections.Generic;

using AppMotor.CliApp.CommandLine;

using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal sealed class Program : CliApplicationWithCommands
    {
        /// <inheritdoc />
        protected override string AppDescription => LocalizableResources.AppDescription;

        /// <inheritdoc />
        protected override IEnumerable<CliVerb> GetVerbs()
        {
            yield return new BenchmarkCommand();
        }

        private static int Main(string[] args)
        {
            return Run<Program>(args);
        }
    }
}
