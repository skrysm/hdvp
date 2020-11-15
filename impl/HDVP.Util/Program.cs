using System.Collections.Generic;
using System.Threading.Tasks;

using AppMotor.CliApp;

using HDVP.Util.Properties;

namespace HDVP.Util
{
    internal sealed class Program : CliApplicationWithCommands
    {
        /// <inheritdoc />
        protected override string AppDescription => LocalizableResources.AppDescription;

        /// <inheritdoc />
        protected override IEnumerable<CliCommand> GetCommands()
        {
            yield return new BenchmarkCommand();
        }

        private static Task<int> Main(string[] args)
        {
            return Execute<Program>(args);
        }
    }
}
