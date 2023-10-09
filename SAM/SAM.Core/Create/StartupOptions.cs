using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static StartupOptions StartupOptions(IEnumerable<string> arguments)
        {
            StartupOptions result = new StartupOptions();

            if (arguments == null || arguments.Count() == 0)
            {
                return result;
            }

            string text = string.Join(" ", arguments);
            arguments = text.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, StartupArgument> dictionary = new Dictionary<string, StartupArgument>();
            foreach (StartupArgument startupArgument in System.Enum.GetValues(typeof(StartupArgument)))
            {
                dictionary[startupArgument.ToString().ToLower()] = startupArgument;
            }

            foreach (string argument in arguments)
            {
                string type = null;
                string value = null;

                string[] values = argument.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (values == null || values.Length == 0)
                {
                    continue;
                }

                type = values[0]?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(type))
                {
                    continue;
                }

                if (!dictionary.TryGetValue(type, out StartupArgument startupArgument))
                {
                    continue;
                }

                if (values.Length > 1)
                {
                    value = values[1].Trim();
                }

                switch (startupArgument)
                {
                    case StartupArgument.TemporaryFile:
                        result.TemporaryFile = true;
                        break;

                    case StartupArgument.Path:
                        result.Path = value;
                        break;
                }
            }

            return result;
        }
    }
}
