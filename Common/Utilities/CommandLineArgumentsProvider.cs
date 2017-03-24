using System;

namespace Common.Utilities
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CommandLineArgumentsProvider : IArgumentsProvider
    {
        public string[] GetArguments() => Environment.GetCommandLineArgs();
    }
}
