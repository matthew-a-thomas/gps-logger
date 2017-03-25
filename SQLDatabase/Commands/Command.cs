using System.Collections.Generic;
using System.Linq;

namespace SQLDatabase.Commands
{
    public class Command
    {
        public string CommandText { get; set; }
        public IDictionary<string, object> Parameters { get; set; }

        public static Command Create(string commandText, params KeyValuePair<string, object>[] parameters) =>
            new Command
            {
                CommandText = commandText,
                Parameters = parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
    }
}
