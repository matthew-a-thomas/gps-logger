using System;
using System.Linq;

namespace Common.Utilities
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CommandLineArgumentsProvider : IArgumentsProvider
    {
        public string[] GetArguments() => Environment.GetCommandLineArgs().Skip(1).ToArray();
    }
}
